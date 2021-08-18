using System.IO;
using Funq;
using ServiceStack;
using ServiceStack.Razor;
using ER_Recogniser.ServiceInterface;
using ServiceStack.Logging;
using ServiceStack.Text;
using ServiceStack.Api.Swagger;

namespace ER_Recogniser
{
    /// <summary>
    /// Applicatin host class implementing the ServiceStack.AppSelfHostBase functionality
    /// </summary>
    /// <seealso cref="ServiceStack.AppSelfHostBase" />
    public class AppHost : AppSelfHostBase
    {
        /// <summary>
        /// Base constructor requires a Name and Assembly where web service implementation is located
        /// </summary>
        public AppHost()
            : base("ER_Recogniser", typeof(RecogniserService).Assembly)
        {
            ILog log = LogManager.GetLogger(GetType());
            log.Info("AppHost created.");

        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container">The container.</param>
        public override void Configure(Container container)
        {
            container.Register<ILog>(ctx => LogManager.LogFactory.GetLogger(typeof(IService)));
            //container.Register(c => new TeamDbRepository()).ReusedWithin(Funq.ReuseScope.Request);
            //ILog logger = AppHostBase.Instance.Container.Resolve<ILog>();


            JsConfig.EmitLowercaseUnderscoreNames = false;
            //JsConfig.ExcludeDefaultValues = true;

            //JsConfig<Guid>.SerializeFn = guid => guid.ToString("N");
            //JsConfig<TimeSpan>.SerializeFn = time => 

            //Config examples
            this.Plugins.Add(new PostmanFeature());
            this.Plugins.Add(new CorsFeature());

            this.Plugins.Add(new RazorFormat());
            this.Plugins.Add(new SwaggerFeature());

            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = Path.GetFullPath(Path.Combine("~".MapServerPath(), "..", "..")),
#endif
            });
        }
    }
}
