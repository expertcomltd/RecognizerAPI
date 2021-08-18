using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;
using ServiceStack.Logging;
using ServiceStack.Logging.NLogger;
using ServiceStack.OrmLite;
using FaceApi_OpenCV;
using System.IO;
using ER_Recogniser.ServiceModel;
using VideoApi;
using EmotionApi_PyProc;

namespace ER_Recogniser
{

    //https://www.codeproject.com/Articles/501608/Sending-Stream-to-ServiceStack
    //https://alistefano.wordpress.com/2014/10/10/servicestack-tutorial-how-to-upload-and-download-binary-files-from-azure/

    /// <summary>
    /// The main class of the application
    /// </summary>
    class Program
    {
        /// <summary>
        /// The listening on
        /// </summary>
        private static string ListeningOn = "http://127.0.0.1:7777/";
        /// <summary>
        /// The database connection
        /// </summary>
        private static string DBConnection = @"Server=DESKTOP-J0G1FG8\SQLEXPRESS;Database=recogniser;User ID=sa;Password=vr2018;Pooling=true;"; //@"Server=192.168.101.36\SQLEXPRESS;";
        /// <summary>
        /// The service mode
        /// </summary>
        private static int ServiceMode = 0;
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var appHost = new AppHost();
            try
            {
                try
                {
                    System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
                    string value = string.Empty;
                    int ival = 0;
                    ListeningOn = (string)(configurationAppSettings.GetValue("ListeningOn", value.GetType()));
                    DBConnection = (string)(configurationAppSettings.GetValue("DBConnection", value.GetType()));
                    ServiceMode = (int)(configurationAppSettings.GetValue("ServiceMode", ival.GetType()));
                }
                catch
                {

                }

                //create logger
                LogManager.LogFactory = new NLogFactory();
                ServiceInterface.Common.Log = LogManager.GetLogger("ER_Recogniser_Server");
                ServiceInterface.Common.Log.Debug("ER_Recogniser_Server starting...");

                //emotion detctor init/test
                try
                {
                    //EmotionDetector.OnDebugMessage += LogEmotionDetectorApi;
                    //EmotionDetector.Instance.Init(@"c:\Projects\Embeddingpython\Resource\fer2013_emotions.h5");

                    EmotionDetector_PyProc.OnDebugMessage += LogEmotionDetectorApi;
                    EmotionDetector_PyProc.Instance.Init("./Res");


                    //{'Angry': 0, 'Disgust': 1, 'Fear': 2, 'Happy': 3, 'Neutral': 4, 'Sad': 5, 'Surprise': 6}
                    /*
                    byte[] pic;
                    using (var fs = new FileStream(@"c:\Projects\Embeddingpython\Resource\how-to-be-happy.jpg", FileMode.Open,FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            pic = ms.ToArray();
                        }
                    }

                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 477, 499);
                    pic = CropImage.CopyRect(pic, rect, true, 48, 48);
                    string error;
                    int emot = detector.Predict(pic,out error);
                    Console.WriteLine("RESULT: " + emot);
                    */
                }
                catch (Exception ex)
                {

                }






                var ormLiteConnectionFactory = new OrmLiteConnectionFactory(DBConnection, ServiceStack.OrmLite.SqlServer.SqlServerOrmLiteDialectProvider.Instance);
                ServiceInterface.DbInitializer.InitializeDb(ormLiteConnectionFactory);


                ServiceInterface.Common.TeamDbRepository = new ServiceInterface.TeamDbRepository(ormLiteConnectionFactory);

                //create tmp image dir
                //ServiceInterface.Common.ImageDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "images");

                ServiceInterface.Common.ResourceFolder = "C:/Projects/ApiRepository";

                ServiceInterface.Common.ResourceFolder = Path.GetFullPath(ServiceInterface.Common.ResourceFolder);

                if (!Directory.Exists(ServiceInterface.Common.ResourceFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(ServiceInterface.Common.ResourceFolder);
                    }
                    catch
                    {
                        return;
                    }
                }

                FaceApi.OnDebugMessage += LogFaceApi;

                if (!ServiceInterface.Common.InitFaceAPI(ServiceInterface.Common.ResourceFolder))
                    return;

                string apikey = "c68fcbeb554442229307452dc0126379";
                Guid guidApiKey = Guid.Parse(apikey);

                Api api = ServiceInterface.Common.TeamDbRepository.GetApi(apikey);
                if(api==null)
                {
                    if (ServiceInterface.Common.TeamDbRepository.CreateApi("test api", apikey))
                    {
                        if(FaceApi.AddFaceApi(apikey, RecognizerType.FisherFaceRecognizer))
                        {
                            Team team = new Team() { ApiKey = guidApiKey, Name = "Test group", TeamID = Guid.NewGuid() };
                            if (ServiceInterface.Common.TeamDbRepository.CreateTeam(team))
                            {
                                FaceApi.CreateTeamGroup(apikey, team.TeamID.ToString("N"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceInterface.Common.Log.Error("ER_Recogniser_Server start exception:" + ex);
                return;
            }

            //Allow you to debug your Windows Service while you're developing it. 
            if (ServiceMode == 0)
            {
                ServiceInterface.Common.Log.Debug("Running WinServiceAppHost in Console mode");
                try
                {
                    appHost.Init();
                    appHost.Start(ListeningOn);
                    Process.Start(ListeningOn + "swagger-ui/");
                    Console.WriteLine("Press <CTRL>+C to stop.");
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    ServiceInterface.Common.Log.Error("ER_Recogniser_Server start exception:" + ex);
                    throw;
                }
                finally
                {
                    appHost.Stop();
                    EmotionDetector_PyProc.Instance.Deinit();
                }
                ServiceInterface.Common.Log.Error("WinServiceAppHost has finished...");
            }
            else
            {

                //When in RELEASE mode it will run as a Windows Service with the code below

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new WinService(appHost, ListeningOn)
                };
                ServiceBase.Run(ServicesToRun);
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Logs a face API debug event.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        static void LogFaceApi(string msg)
        {
            ServiceInterface.Common.Log.Debug("FaceApi - " + msg);

        }

        static void LogEmotionDetectorApi(string msg)
        {
            ServiceInterface.Common.Log.Debug("EmotionDetector - " + msg);

        }


    }
}
