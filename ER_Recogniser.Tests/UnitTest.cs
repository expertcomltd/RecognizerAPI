using NUnit.Framework;
using ServiceStack;
using ServiceStack.Testing;
using ER_Recogniser.ServiceInterface;
using ER_Recogniser.ServiceModel;

namespace ER_Recogniser.Tests
{
    /// <summary>
    /// Unit test class
    /// </summary>
    public class UnitTest
    {
        /// <summary>
        /// The application host
        /// </summary>
        private readonly ServiceStackHost appHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTest"/> class.
        /// </summary>
        public UnitTest()
        {
            appHost = new BasicAppHost().Init();
            appHost.Container.AddTransient<RecogniserService>();
        }

        /// <summary>
        /// Called when [time tear down].
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        /// <summary>
        /// Determines whether this instance [can call my services].
        /// </summary>
        [Test]
        public void Can_call_MyServices()
        {
            //wtf
            //var service = appHost.Container.Resolve<RecogniserService>();

            //var response = (HelloResponse)service.Any(new Hello { Name = "World" });

            //Assert.That(response.Result, Is.EqualTo("Hello, World!"));
        }
    }
}
