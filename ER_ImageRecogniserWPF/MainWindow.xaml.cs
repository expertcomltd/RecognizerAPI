using ER_Recogniser.ServiceModel;
using Microsoft.Win32;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using VideoApi;

namespace ER_ImageRecogniserWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The video capture
        /// </summary>
        VideoCapture VideoCapture;
        /// <summary>
        /// The need snapshot
        /// </summary>
        ManualResetEvent NeedSnapshot = new ManualResetEvent(false);

        /// <summary>
        /// The API key
        /// </summary>
        string ApiKey = "C68FCBEB554442229307452DC0126379";
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            VideoCapture = new VideoCapture();

            //FaceDetectionResponse response = serviceClient.Get(new FaceDetectionRequest { ApiKey = "123456", ImageUrl = "" });
            //var response = serviceClient.Get<FaceDetectionResponse>("/hello/World!");
            //response.Result.Print();
            //response.Result.Print();
            //FaceDetectionResponse response = await serviceClient.GetAsync(new FaceDetectionRequest { ApiKey = "123456",ImageUrl="" });

            this.Closed += MainWindow_Closed;

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

        }

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VideoCapture.OnFrameArrived += VideoCapture_OnFrameArrived;
            VideoCapture.StartCapture();
            GetTeams();
        }

        /// <summary>
        /// Handles the Closed event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (serviceClient != null)
                serviceClient.Dispose();
        }

        /// <summary>
        /// The faces
        /// </summary>
        List<FaceDetectionResponse.ImageRect> _faces = new List<FaceDetectionResponse.ImageRect>();
        /// <summary>
        /// The eyes
        /// </summary>
        List<System.Drawing.Rectangle> _eyes = new List<System.Drawing.Rectangle>();

        /// <summary>
        /// Handles video frame arrived event.
        /// </summary>
        /// <param name="frame">The frame.</param>
        private void VideoCapture_OnFrameArrived(VideoCaptureFrame frame)
        {
            if(DetectAvailable.WaitOne(0))
            {
                DetectAvailable.Reset();
                ThreadPool.QueueUserWorkItem(DetectFacesCallback, frame.Clone());
            }

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                try
                {

                    int x = -1;int y = -1;

                    lock (_faces)
                    {
                        if(_faces.Count>0)
                        {
                            x = _faces[0].X;
                            y = _faces[0].Y;
                        }
                        foreach (var face in _faces)
                        {
                            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(face.X,face.Y,face.W,face.H);
                            frame.DrawFrame(rect);
                            frame.DrawText(rect.X, rect.Y, ((EmotionIDs)face.E).ToString(), 10, 10, 80);
                        }
                    }

                    lock (_eyes)
                    {
                        foreach (var eye in _eyes)
                        {
                            frame.DrawFrame(eye,100,200,10);
                        }
                    }

                    if (x >= 0 && y >= 0)
                    {
                        lock (FaceInfoMutex)
                        {
                            if (!string.IsNullOrEmpty(FaceInfo))
                            {
                                frame.DrawText(x, y, FaceInfo, 10, 10, 80);
                            }
                        }
                    }

                    imgVideoCapture.Stretch = Stretch.Uniform;
                    imgVideoCapture.Source = frame.Bitmap;
                    imgBitmap.Source = frame.Bitmap;
                    frame.Dispose();
                }
                catch(Exception ex)
                {

                }
            }));
        }

        /// <summary>
        /// A thread callback for detecting faces.
        /// </summary>
        /// <param name="state">The state.</param>
        private void DetectFacesCallback(object state)
        {
            VideoCaptureFrame frame = state as VideoCaptureFrame;
            try
            {
                byte[] imagedata = null;
                using (var ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(frame.Bitmap));
                    encoder.Save(ms);
                    imagedata = ms.ToArray();
                }
                FaceDetectionResponse response = serviceClient.Send<FaceDetectionResponse>(new FaceDetection { ApiKey = ApiKey, MimeType = "Image/png", ImageData = imagedata, PredictEmotion = true });

                System.Drawing.Rectangle face0 = System.Drawing.Rectangle.Empty;

                lock (_faces)
                {
                    _faces.Clear();
                    foreach (var rect in response.Faces)
                    {
                        _faces.Add(rect);
                    }
                    if(_faces.Count>0)
                    {
                        face0 = new System.Drawing.Rectangle(_faces[0].X, _faces[0].Y, _faces[0].W, _faces[0].H);
                    }
                }

                lock (_eyes)
                {
                    _eyes.Clear();
                    foreach (var rect in response.Eyes)
                    {
                        _eyes.Add(new System.Drawing.Rectangle(rect.X, rect.Y, rect.W, rect.H));
                    }
                }

                if (face0 != System.Drawing.Rectangle.Empty)
                {
                    if (NeedFaceRecognition.WaitOne(0) && !FaceRecognitionInProgress.WaitOne(0))
                    {
                        FaceRecognitionInProgress.Set();
                        System.Drawing.Rectangle rect = face0;
                        if (rect.Width != rect.Height)
                        {
                            if (rect.Width > rect.Height)
                            {
                                int diff = rect.Width - rect.Height;
                                rect.Y -= diff / 2;
                                rect.Height = rect.Width;
                            }
                            else
                            {
                                int diff = rect.Height - rect.Width;
                                rect.X -= diff / 2;
                                rect.Width = rect.Height;
                            }
                        }

                        VideoCaptureFrame faceframe = frame.CopyRect(rect, true, 128, 128);
                        ThreadPool.QueueUserWorkItem(RecogFacesCallback, faceframe);
                    }

                    if (NeedSnapshot.WaitOne(0))
                    {
                        System.Drawing.Rectangle rect = face0;
                        if (rect.Width != rect.Height)
                        {
                            if (rect.Width > rect.Height)
                            {
                                int diff = rect.Width - rect.Height;
                                rect.Y -= diff / 2;
                                rect.Height = rect.Width;
                            }
                            else
                            {
                                int diff = rect.Height - rect.Width;
                                rect.X -= diff / 2;
                                rect.Width = rect.Height;
                            }
                        }

                        VideoCaptureFrame faceframe = frame.CopyRect(rect, true, 128, 128);
                        using (var ms = new MemoryStream())
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(faceframe.Bitmap));
                            encoder.Save(ms);
                            FaceImageData = ms.ToArray();
                        }
                        Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            imgFaceBitmap.Source = faceframe.Bitmap;
                            faceframe.Dispose();
                        }));
                        NeedSnapshot.Reset();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            frame.Dispose();
            DetectAvailable.Set();
        }

        /// <summary>
        /// The face information mutex
        /// </summary>
        object FaceInfoMutex = new object();
        /// <summary>
        /// The face information
        /// </summary>
        string FaceInfo = "";
        /// <summary>
        /// The need face recognition event
        /// </summary>
        ManualResetEvent NeedFaceRecognition = new ManualResetEvent(false);
        /// <summary>
        /// The face recognition in progress event
        /// </summary>
        ManualResetEvent FaceRecognitionInProgress = new ManualResetEvent(false);

        /// <summary>
        /// Thread callback for recognize a face.
        /// </summary>
        /// <param name="state">The state.</param>
        private void RecogFacesCallback(object state)
        {
            VideoCaptureFrame faceframe = state as VideoCaptureFrame;
            try
            {
                byte[] imgdata = null;

                using (var ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(faceframe.Bitmap));
                    encoder.Save(ms);
                    imgdata = ms.ToArray();
                }
                faceframe.Dispose();

                FaceRecognitionResponse response = serviceClient.Send<FaceRecognitionResponse>(new FaceRecognition
                {
                    ApiKey = ApiKey,
                    ImageData = imgdata,
                    TeamGroupID = SelectedMainTeamID.ToString("N"),
                });

                lock (FaceInfoMutex)
                {
                    if (response.Status == "OK")
                    {
                        FaceInfo = response.TeamMember != null ? response.TeamMember.Name + "("  + response.TeamMember.TeamMemberID + ")" : "?";
                    }
                    else
                    {
                        FaceInfo = response.Status;
                    }

                    if(!NeedFaceRecognition.WaitOne(0))
                    {
                        FaceInfo = "";
                    }

                }
            }
            catch(Exception ex)
            {

            }
            FaceRecognitionInProgress.Reset();
        }

        /// <summary>
        /// The detect available event
        /// </summary>
        ManualResetEvent DetectAvailable = new ManualResetEvent(true);

        /// <summary>
        /// Shows a face API message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void ShowFaceApiMessage(string msg)
        {

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                faceApiMessages.Text = msg;
            }));
        }

        /// <summary>
        /// The service client
        /// </summary>
        JsonServiceClient serviceClient = new JsonServiceClient("http://192.168.101.78:7777");

        #region FaceDetect
        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    OpenFileDialog openFileDialog1 = new OpenFileDialog()
            //    {
            //        InitialDirectory = @"d:\",
            //        Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|All files (*.*)|*.*",
            //        FilterIndex = 1,
            //        DefaultExt = "png",
            //        CheckFileExists = true,
            //        CheckPathExists = true,
            //    };
            //    if (openFileDialog1.ShowDialog() == true)
            //    {
            //        imgBitmap.Source = new BitmapImage(new Uri(openFileDialog1.FileName));
            //        FaceDetect(openFileDialog1.FileName);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Exception:" + ex);
            //}
        }

        /// <summary>
        /// Handles the Click event of the btnDownload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //using (WebClient webClient = new WebClient())
                //{

                    //var qs = System.Web.HttpUtility.ParseQueryString(txtImageUrl.Text);
                    //var str = qs.Get(key);

                    //var uri = new Uri(txtImageUrl.Text);
                    //imgBitmap.Source = new BitmapImage(uri);
                    //byte[] data = webClient.DownloadData(uri);
                    //FaceDetect(data);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image download error:" + ex);
            }

        }

        //void AddRect(Canvas cnv, Color color, ER_Recogniser.ServiceModel.FaceDetectionResponse.ImageRect face)
        //{
        //    Rectangle rect = new Rectangle();
        //    rect.SetValue(Canvas.LeftProperty, (double)face.X);
        //    rect.SetValue(Canvas.TopProperty, (double)face.Y);
        //    rect.Width = (double)face.W;
        //    rect.Height = (double)face.H;
        //    rect.Visibility = Visibility.Visible;
        //    rect.Stroke = new SolidColorBrush(color);
        //    rect.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        //    rect.StrokeThickness = 2.0f;
        //    cnv.Children.Add(rect);
        //}

        //void FaceDetect(string filename)
        //{
        //    byte[] array = System.IO.File.ReadAllBytes(filename);
        //    FaceDetect(array, System.IO.Path.GetExtension(filename));
        //}
        //void FaceDetect(byte[] array, string mimeType = "")
        //{
        //    try
        //    {

        //        //remove rectangles
        //        if (cnvMain.Children.Count > 1)
        //        {
        //            int max = cnvMain.Children.Count - 1;
        //            for (int i = max; i > 0; i--)
        //                cnvMain.Children.RemoveAt(i);
        //        }

        //        //start detection...
        //        FaceDetectionResponse response = serviceClient.Send<FaceDetectionResponse>(new FaceDetection { ApiKey = "123456", MimeType = mimeType, ImageData = array });
        //        if (response.Status == "OK")
        //        {
        //            if (response.Faces != null && response.Faces.Count > 0)
        //            {
        //                foreach (var rect in response.Faces)
        //                {
        //                    AddRect(cnvMain, Colors.Purple, rect);
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("No faces detected!");
        //            }
        //            if (response.Eyes != null && response.Eyes.Count > 0)
        //            {
        //                foreach (var rect in response.Eyes)
        //                {
        //                    AddRect(cnvMain, Colors.Yellow, rect);
        //                }
        //            }

        //        }
        //        else
        //        {
        //            MessageBox.Show("Detection error:" + response.Status);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("FaceDetect Exception:" + ex);

        //    }
        //}

        #endregion


        /// <summary>
        /// Handles the SelectionChanged event of the tabMain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                string tabHeader = ((TabItem)tabMain.SelectedItem).Header.ToString();
                switch (tabHeader)
                {
                    case "FaceRecognition":
                        {
                            GetTeams();
                        }
                        break;
                }
            }
        }


        #region Teams

        /// <summary>
        /// Gets the teams.
        /// </summary>
        void GetTeams() 
        {
            try
            {
                Team team = new Team() { TeamID = Guid.NewGuid() };
                GetTeamsResponse response = serviceClient.Send<GetTeamsResponse>(new GetTeams { ApiKey = ApiKey });
                dgTeams.ItemsSource = response.Teams;

                if(response.Teams.Count==0)
                {
                    dgTeamMembers.ItemsSource = null;
                }

                if (dgTeams.ItemsSource != null && dgTeams.SelectedIndex < 0)
                {
                    try { dgTeams.SelectedIndex = 0; }catch { }
                }

                cboTeam.ItemsSource = response.Teams;
                if (cboTeam.ItemsSource != null && cboTeam.SelectedIndex < 0)
                {
                    try { cboTeam.SelectedIndex = 0; }
                    catch { }
                }

                cboMainTeam.ItemsSource = response.Teams;
                if (cboMainTeam.ItemsSource != null && cboMainTeam.SelectedIndex < 0)
                {
                    try { cboMainTeam.SelectedIndex = 0; }
                    catch { }
                }
            }
            catch (Exception ex) 
            {
                ShowFaceApiMessage("GetTeamsResponse Exception:" + ex);
            }
        }
        /// <summary>
        /// Handles the Click event of the btnCreateTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateTeamResponse response = serviceClient.Send<CreateTeamResponse>(new CreateTeam { ApiKey = ApiKey, Name = txtTeamName.Text });
                ShowFaceApiMessage(response.Status);
                GetTeams();
            }
            catch (Exception ex)
            {
                ShowFaceApiMessage("CreateTeamResponse Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnUpdateTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateTeamResponse response = serviceClient.Send<UpdateTeamResponse>(new UpdateTeam { ApiKey = ApiKey, Team = (Team)dgTeams.SelectedItem });
                ShowFaceApiMessage(response.Status);
                GetTeams();
            }
            catch (Exception ex)
            {
                ShowFaceApiMessage("UpdateTeamResponse Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTeams.SelectedItem != null)
                {
                    DeleteTeamResponse response = serviceClient.Send<DeleteTeamResponse>(new DeleteTeam { ApiKey = ApiKey, Team = (Team)dgTeams.SelectedItem });
                    ShowFaceApiMessage(response.Status);
                    GetTeams();
                }
            }
            catch (Exception ex)
            {
                ShowFaceApiMessage("DeleteTeamResponse Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the dgTeams control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dgTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stpTeams.DataContext = dgTeams.SelectedItem;
        }

        #endregion


        #region TeamMembers
        /// <summary>
        /// Handles the SelectionChanged event of the cboTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTeam.SelectedValue != null)
            {
                SelectedTeamID = (Guid)cboTeam.SelectedValue;
                GetTeamMembers();
            }
        }

        /// <summary>
        /// The selected team identifier
        /// </summary>
        Guid SelectedTeamID = Guid.Empty;
        /// <summary>
        /// Gets the team members.
        /// </summary>
        void GetTeamMembers() 
        {
            try
            {
                GetTeamMembersResponse response = serviceClient.Send<GetTeamMembersResponse>(new GetTeamMembers
                {
                    ApiKey = ApiKey,
                    TeamGroupID = SelectedTeamID.ToString("N"),
                });
                dgTeamMembers.ItemsSource = response.TeamMembers;

                if (dgTeamMembers.ItemsSource != null && dgTeamMembers.SelectedIndex < 0)
                {
                    try { dgTeamMembers.SelectedIndex = 0; }catch { }
                }

                cboTeamMember.ItemsSource = response.TeamMembers;
                if (cboTeamMember.ItemsSource != null && cboTeamMember.SelectedIndex < 0)
                {
                    try { cboTeamMember.SelectedIndex = 0; }
                    catch { }
                }

            }
            catch (Exception ex) 
            {
                ShowFaceApiMessage("GetTeamMembersResponse Exception:" + ex);
                //todo: cont
            }
        }
        /// <summary>
        /// Handles the Click event of the btnCreateTeamMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateTeamMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateTeamMemberResponse response = serviceClient.Send<CreateTeamMemberResponse>(new CreateTeamMember
                {
                    ApiKey = ApiKey,
                    Name = txtTeamMemberName.Text,
                    TeamGroupID = SelectedTeamID.ToString("N")
                });
                ShowFaceApiMessage("CreateTeamMemberResponse:" + response.Status);
                GetTeamMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateTeamMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnUpdateTeamMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateTeamMemberResponse response = serviceClient.Send<UpdateTeamMemberResponse>(new UpdateTeamMember { ApiKey = ApiKey, TeamMember = (TeamMember)dgTeamMembers.SelectedItem });
                ShowFaceApiMessage("UpdateTeamMemberResponse:" + response.Status);
                GetTeamMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteTeamMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteTeamMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTeamMembers.SelectedItem != null)
                {
                    DeleteTeamMemberResponse response = serviceClient.Send<DeleteTeamMemberResponse>(new DeleteTeamMember { ApiKey = ApiKey, TeamMember = (TeamMember)dgTeamMembers.SelectedItem });
                    ShowFaceApiMessage("DeleteTeamMemberResponse:" + response.Status);
                    GetTeamMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }
        /// <summary>
        /// Handles the SelectionChanged event of the dgTeamMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dgTeamMembers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stpTeamMembers.DataContext = dgTeamMembers.SelectedItem;

        }

        #endregion


        #region FaceImages
        /// <summary>
        /// Handles the SelectionChanged event of the cboTeamMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboTeamMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTeamMember.SelectedValue != null)
            {
                SelectedTeamMemberID = (int)cboTeamMember.SelectedValue;
                GetFaceImages();
            }
        }

        /// <summary>
        /// The selected team member identifier
        /// </summary>
        int SelectedTeamMemberID = 0;
        /// <summary>
        /// Gets the face images.
        /// </summary>
        void GetFaceImages()
        {
            try
            {
                GetFaceImagesResponse response = serviceClient.Send<GetFaceImagesResponse>(new GetFaceImages { ApiKey = ApiKey, TeamMemberID = SelectedTeamMemberID });
                dgFaceImages.ItemsSource = response.FaceImages;

                if (dgFaceImages.ItemsSource != null && dgFaceImages.SelectedIndex < 0)
                {
                    try { dgFaceImages.SelectedIndex = 0; }
                    catch { }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }
        /// <summary>
        /// Handles the Click event of the btnCreateFaceImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateFaceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool autoteach = chkAutoTeach.IsChecked == true;

                CreateFaceImageResponse response = serviceClient.Send<CreateFaceImageResponse>(new CreateFaceImage
                {
                    ApiKey = ApiKey,
                    Comment = txtFaceImageName.Text,
                    TeamMemberID = SelectedTeamMemberID,
                    TeamGroupID = SelectedTeamID.ToString("N"),
                    ImageData = FaceImageData,
                    MimeType = FaceImageMimeType,
                    TrainGroup = autoteach,
                });
                ShowFaceApiMessage("CreateFaceImageResponse:" + response.Status);
                GetFaceImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateFaceImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnUpdateFaceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateFaceImageResponse response = serviceClient.Send<UpdateFaceImageResponse>(new UpdateFaceImage { ApiKey = ApiKey, FaceImage = (FaceImage)dgFaceImages.SelectedItem, ImageData = FaceImageData, MimeType=FaceImageMimeType });
                ShowFaceApiMessage("UpdateFaceImageResponse:" + response.Status);
                GetFaceImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteFaceImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteFaceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgFaceImages.SelectedItem != null)
                {
                    DeleteFaceImageResponse response = serviceClient.Send<DeleteFaceImageResponse>(new DeleteFaceImage { ApiKey = ApiKey, FaceImage = (FaceImage)dgFaceImages.SelectedItem });
                    ShowFaceApiMessage("DeleteFaceImageResponse:" + response.Status);
                    GetFaceImages();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex);
            }
        }
        /// <summary>
        /// Handles the SelectionChanged event of the dgFaceImages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dgFaceImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stpFaceImages.DataContext = dgFaceImages.SelectedItem;
            RetriveFaceImageContent();
        }

        /// <summary>
        /// Retrives the content of the face image.
        /// </summary>
        private void RetriveFaceImageContent()
        {
            if (dgFaceImages.SelectedItem != null)
            {
                try
                {
                    FaceImage image = (FaceImage)dgFaceImages.SelectedItem;
                    FaceImageContentResponse response = serviceClient.Send<FaceImageContentResponse>(new FaceImageContent
                    {
                        ApiKey = ApiKey,
                        TeamMemberID = SelectedTeamMemberID,
                        TeamGroupID = SelectedTeamID.ToString("N"),
                        FaceImageID = image.GetFaceImageIDString(),
                    });
                    ShowFaceApiMessage("FaceImageContentResponse:" + response.Status);
                    if (response.Status=="OK")
                    {
                        try
                        {
                            MemoryStream strmImg = new MemoryStream(response.ImageData);
                            BitmapImage myBitmapImage = new BitmapImage();
                            myBitmapImage.BeginInit();
                            myBitmapImage.StreamSource = strmImg;
                            myBitmapImage.DecodePixelWidth = 200;
                            myBitmapImage.EndInit();
                            imgFaceImageFace.Source = myBitmapImage;
                        }
                        catch
                        {

                        }
                       
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex);
                }
            }
        }

        /// <summary>
        /// The face image MIME type
        /// </summary>
        string FaceImageMimeType = "";
        /// <summary>
        /// The face image data
        /// </summary>
        byte[] FaceImageData = null;

        /// <summary>
        /// Handles the Click event of the btnSelectFaceImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectFaceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog()
                {
                    InitialDirectory = @"d:\",
                    Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|All files (*.*)|*.*",
                    FilterIndex = 1,
                    DefaultExt = "png",
                    CheckFileExists = true,
                    CheckPathExists = true,
                };
                if (openFileDialog1.ShowDialog() == true)
                {
                    string filename = openFileDialog1.FileName;

                    FaceImageData = System.IO.File.ReadAllBytes(filename);
                    FaceImageMimeType = System.IO.Path.GetExtension(filename);

                    imgFaceBitmap.Source = new BitmapImage(new Uri(filename));
                    txtFaceImagePath.Text = filename;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnSelectFaceImage_Click Exception:" + ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDownloadFaceImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDownloadFaceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri(txtFaceImageUrl.Text);
                imgFaceBitmap.Source = new BitmapImage(uri);
                txtFaceImagePath.Text = uri.ToString();
                using (WebClient webClient = new WebClient())
                {
                    FaceImageData = webClient.DownloadData(uri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnDownloadFaceImage_Click Exception:" + ex);
            }
        }
        #endregion


        #region FaceRecognition
        //string FaceRecognitionImageMimeType = "";
        //byte[] FaceRecognitionImageData = null;

        /// <summary>
        /// Handles the Click event of the btnDownloadFaceRecognitionImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDownloadFaceRecognitionImage_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var uri = new Uri(txtFaceRecognitionImageUrl.Text);
            //    imgFaceRecognitionBitmap.Source = new BitmapImage(uri);
            //    txtFaceRecognitionImagePath.Text = uri.ToString();
            //    using (WebClient webClient = new WebClient())
            //    {
            //        FaceRecognitionImageData = webClient.DownloadData(uri);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("btnDownloadFaceRecognitionImage_Click Exception:" + ex);
            //}
        }

        /// <summary>
        /// Handles the Click event of the btnSelectFaceRecognitionImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectFaceRecognitionImage_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    OpenFileDialog openFileDialog1 = new OpenFileDialog()
            //    {
            //        InitialDirectory = @"d:\",
            //        Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|All files (*.*)|*.*",
            //        FilterIndex = 1,
            //        DefaultExt = "png",
            //        CheckFileExists = true,
            //        CheckPathExists = true,
            //    };
            //    if (openFileDialog1.ShowDialog() == true)
            //    {
            //        string filename = openFileDialog1.FileName;

            //        FaceRecognitionImageData = System.IO.File.ReadAllBytes(filename);
            //        FaceRecognitionImageMimeType = System.IO.Path.GetExtension(filename);

            //        imgFaceRecognitionBitmap.Source = new BitmapImage(new Uri(filename));
            //        txtFaceRecognitionImagePath.Text = filename;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("btnSelectFaceRecognitionImage_Click Exception:" + ex);
            //}
        }

        /// <summary>
        /// Handles the Click event of the btnFaceRecognitionStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnFaceRecognitionStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //start detection...
                //FaceRecognitionResponse response = serviceClient.Send<FaceRecognitionResponse>(new FaceRecognition { ApiKey = "123456", MimeType = FaceRecognitionImageMimeType, ImageData = FaceRecognitionImageData });
                //if (response.Status == "OK")
                //{
                //    if (response.TeamMemberID == 0)
                //    {
                //        MessageBox.Show("No teammember identified!");
                //    }
                //    else 
                //    {
                //        MessageBox.Show("Teammember found, id:" + response.TeamMemberID + ", name:" + response.TeamMember.Name);
                //    }

                //}
                //else
                //{
                //    MessageBox.Show("Identification error:" + response.Status);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Identification Exception:" + ex);

            }

        }

        #endregion

        /// <summary>
        /// Handles the Click event of the Face_Snapshot_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Face_Snapshot_Button_Click(object sender, RoutedEventArgs e)
        {
            NeedSnapshot.Set();
        }

        #region main page, select team for test identification

        /// <summary>
        /// The selected main team identifier
        /// </summary>
        Guid SelectedMainTeamID = Guid.Empty;

        /// <summary>
        /// Handles the SelectionChanged event of the cboMainTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboMainTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTeam.SelectedValue != null)
            {
                SelectedMainTeamID = (Guid)cboTeam.SelectedValue;
            }
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the Start_Recognition_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Start_Recognition_Button_Click(object sender, RoutedEventArgs e)
        {
            NeedFaceRecognition.Set();
        }
        /// <summary>
        /// Handles the Click event of the Stop_Recognition_Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Stop_Recognition_Button_Click(object sender, RoutedEventArgs e)
        {
            NeedFaceRecognition.Reset();
            lock (FaceInfoMutex)
            {
                FaceInfo = "";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTrainTeam control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnTrainTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TrainTeamResponse response = serviceClient.Send<TrainTeamResponse>(new TrainTeam
                {
                    ApiKey = ApiKey,
                    TeamGroupID = SelectedTeamID.ToString("N"),
                });
                ShowFaceApiMessage("TrainTeamResponse:" + response.Status);

                if (response.Status=="OK")
                {

                }
            }
            catch
            {

            }
           
        }
    }
}
