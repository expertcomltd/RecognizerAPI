using Accord.Video.FFMPEG;
using ER_Recogniser.ServiceModel;
using Microsoft.Win32;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ER_VideoRecogniser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The API key
        /// </summary>
        string ApiKey = "c68fcbeb554442229307452dc0126379";

        /// <summary>
        /// The playing flag
        /// </summary>
        bool Playing = false;
        /// <summary>
        /// The video thread
        /// </summary>
        Thread VideoThread = null;
        /// <summary>
        /// The playing enabled flag
        /// </summary>
        ManualResetEvent PlayingEnabled = new ManualResetEvent(false);
        /// <summary>
        /// The service client
        /// </summary>
        JsonServiceClient serviceClient;
        /// <summary>
        /// The video analizer context
        /// </summary>
        VideoAnalizerContext videoAnalizerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            try
            {
                serviceClient = new JsonServiceClient("http://127.0.0.1:7777/");
            }
            catch(Exception ex)
            {

            }


        }

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> recognizers = new List<string>() { "EigenFace", "FisherFace", "LBPH" };

            cboRecognizer.ItemsSource = recognizers;
            try
            {
                cboRecognizer.SelectedIndex = 0;
            }
            catch
            {

            }

        }

        /// <summary>
        /// Handles the Closed event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            PlayingEnabled.Reset();

            if (videoAnalizerContext != null)
            {
                videoAnalizerContext.Dispose();
            }

            if (serviceClient!=null)
            {
                serviceClient.Dispose();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayingEnabled.Reset();
                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Videos");

                OpenFileDialog openFileDialog1 = new OpenFileDialog()
                {
                    InitialDirectory = path,// @"d:\dox\Magan\Fun\Movies",
                    Filter = "avi files (*.avi)|*.avi|mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*",
                    FilterIndex = 2,
                    DefaultExt = "mp4",
                    CheckFileExists = true,
                    CheckPathExists = true,
                };
                if (openFileDialog1.ShowDialog() == true)
                {
                    StartPlaying(openFileDialog1.FileName);
                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// The last ticks
        /// </summary>
        long LastTicks = 0;
        /// <summary>
        /// Starts the playing.
        /// </summary>
        /// <param name="filename">The filename.</param>
        void StartPlaying(string filename)
        {
            this.Title = filename;
            LastTicks = 0;
            Playing = true;
            detectedImages.Children.Clear();
            float threshold = -1f;
            try
            {
                threshold = float.Parse(txtThreshHold.Text);
            }
            catch
            {

            }

            int irecog = 0;

            try
            {
                irecog = cboRecognizer.SelectedIndex;
            }
            catch
            {

            }



            PlayingEnabled.Set();

            this.VideoThread = new System.Threading.Thread(() =>
            {
                videoAnalizerContext = new VideoAnalizerContext(ApiKey, serviceClient, irecog, threshold);
                System.Threading.Thread.CurrentThread.IsBackground = true;
                /* run your code here */
                // create instance of video reader
                VideoFileReader reader = new VideoFileReader();
                // open video file
                reader.Open(filename);
                // check some of its attributes
                Console.WriteLine("width:  " + reader.Width);
                Console.WriteLine("height: " + reader.Height);
                Console.WriteLine("fps:    " + reader.FrameRate);
                Console.WriteLine("codec:  " + reader.CodecName);
                Console.WriteLine("codec:  " + reader.FrameCount);
                // read 100 video frames out of it

                double fps = reader.FrameRate.Value;
                double frms = 1000f / fps;

                DateTime ts;
                DateTime ts2;

                DateTime starttime = DateTime.Now;

                for (int i = 0; i < reader.FrameCount; i++)
                {

                    if (PlayingEnabled.WaitOne(0))
                    {

                        Bitmap videoFrame = reader.ReadVideoFrame();
                        if (AnalizeAvailable.WaitOne(0))
                        {
                            AnalizeAvailable.Reset();
                            BitmapSource src = BitmapToBitmapSource(videoFrame);
                            src.Freeze();
                            ThreadPool.QueueUserWorkItem(AnalizeFrame, src);
                        }

                        ts = DateTime.Now;

                        //Convert and display the frame on UI
                        Dispatcher.Invoke(() =>
                        {
                            BitmapSource src = BitmapToBitmapSource(videoFrame);
                            imgVideo.Source = src;
                            tbFrame.Text = "Frame: " + i + " " + (starttime - DateTime.Now).ToString(@"mm\:ss\.ff");
                            videoFrame.Dispose();
                        });

                        ts2 = DateTime.Now;

                        double elapsed = (ts2 - ts).TotalMilliseconds;

                        if (elapsed < frms)
                        {
                            Thread.Sleep((int)(frms - elapsed));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                reader.Close();
                int wait = 100;
                while (wait>0)
                {
                    if (AnalizeAvailable.WaitOne(100))
                    {
                        videoAnalizerContext.Dispose();
                        break;
                    }
                    --wait;
                }
                PlayingEnabled.Reset();
            });
            VideoThread.Start();
        }

        /// <summary>
        /// The analize available
        /// </summary>
        ManualResetEvent AnalizeAvailable = new ManualResetEvent(true);


        /// <summary>
        /// Analizes the frame.
        /// </summary>
        /// <param name="state">The state.</param>
        private void AnalizeFrame(object state)
        {
            try
            {
                BitmapSource src = state as BitmapSource;
                byte[] imagedata = null;
                using (var ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(src));
                    encoder.Save(ms);
                    imagedata = ms.ToArray();
                }

                List<VideoAnalizerResult> result = new List<VideoAnalizerResult>();

                if(videoAnalizerContext.AnalizeFrame(imagedata, "image/png", result))
                {
                    foreach (var face in result)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Int32Rect rect = new Int32Rect(face.X, face.Y, face.W, face.H);
                            BitmapSource bpsface = new CroppedBitmap(src, rect);
                            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                            img.Tag = face.Label;
                            img.Width = 128;
                            img.Stretch = Stretch.Uniform;
                            img.Source = bpsface;
                            detectedImages.Children.Add(img);
                        });
                    }
                }
            }
            catch(Exception ex)
            {

            }
            AnalizeAvailable.Set();


        }

        /// <summary>
        /// Bitmaps to bitmap source.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        /// <summary>
        /// Bitmaps to bitmap image.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
