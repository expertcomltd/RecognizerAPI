using System.ServiceProcess;
using EmotionApi_PyProc;
using ServiceStack;

namespace ER_Recogniser
{
    public partial class WinService : ServiceBase
    {
        private readonly AppHostHttpListenerBase appHost;
        private readonly string listeningOn;

        public WinService(AppHostHttpListenerBase appHost, string listeningOn)
        {
            this.appHost = appHost;
            this.listeningOn = listeningOn;

            this.appHost.Init();

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.appHost.Start(listeningOn);
        }

        protected override void OnStop()
        {
            this.appHost.Stop();
            EmotionDetector_PyProc.Instance.Deinit();
        }
    }
}
