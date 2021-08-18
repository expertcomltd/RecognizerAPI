using Funq;
using ServiceStack;
using NUnit.Framework;
using ER_Recogniser.ServiceInterface;
using ER_Recogniser.ServiceModel;

namespace ER_Recogniser.Tests
{
    /// <summary>
    /// Integration test class
    /// </summary>
    public class IntegrationTest
    {
        /// <summary>
        /// The base URI
        /// </summary>
        const string BaseUri = "http://localhost:2000/";
        /// <summary>
        /// The application host
        /// </summary>
        private readonly ServiceStackHost appHost;

        /// <summary>
        /// Application host implementing service stack AppSelfHostBase
        /// </summary>
        /// <seealso cref="ServiceStack.AppSelfHostBase" />
        class AppHost : AppSelfHostBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AppHost"/> class.
            /// </summary>
            public AppHost() : base(nameof(IntegrationTest), typeof(RecogniserService).Assembly) { }

            /// <summary>
            /// Configures the specified container.
            /// </summary>
            /// <param name="container">The container.</param>
            public override void Configure(Container container)
            {
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
        /// </summary>
        public IntegrationTest()
        {
            appHost = new AppHost()
                .Init()
                .Start(BaseUri);
        }

        /// <summary>
        /// Called when [time tear down].
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <returns></returns>
        public IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

        /// <summary>
        /// Determines whether this instance [can call hello service].
        /// </summary>
        [Test]
        public void Can_call_Hello_Service()
        {
            //wtf
            //var client = CreateClient();

            //var response = client.Get(new Hello { Name = "World" });

            //Assert.That(response.Result, Is.EqualTo("Hello, World!"));
        }
    }
}