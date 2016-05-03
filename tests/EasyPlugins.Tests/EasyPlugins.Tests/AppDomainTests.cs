using NUnit.Framework;

namespace EasyPlugins.Tests
{
    [TestFixture]
    public class AppDomainTests
    {
        //The following are tests to ensure the NUnit.ApplicationDomain package is functioning correctly,
        //as it is critical for our suite of tests, to be able to run certain tests in separate appdomains
        //See: https://bitbucket.org/zastrowm/nunit.applicationdomain/

        private static long? _initializationDuration;

        [Test, RunInApplicationDomain]
        public void RunInSeparateApplicationDomainTest1()
        {
            //RunInSeparateApplicationDomainTest1 and RunInSeparateApplicationDomainTest2 will fail
            //if the RunInApplicationDomain isn't executing in separate app domains
            PluginsManager.Initialize();
            Assert.IsFalse(_initializationDuration.HasValue);
            _initializationDuration = PluginsManager.Instance.InitializationDuration;
        }

        [Test, RunInApplicationDomain]
        public void RunInSeparateApplicationDomainTest2()
        {
            //RunInSeparateApplicationDomainTest1 and RunInSeparateApplicationDomainTest2 will fail
            //if the RunInApplicationDomain isn't executing in separate app domains
            PluginsManager.Initialize();
            Assert.IsFalse(_initializationDuration.HasValue);
            _initializationDuration = PluginsManager.Instance.InitializationDuration;
        }

        [Test, RunInApplicationDomain]
        public void RunInSameApplicationDomainTest3()
        {
            PluginsManager.Initialize();
            Assert.That(() => PluginsManager.Initialize(), Throws.TypeOf<PluginsException>(), "Correctly throws an initialization exception");
        }
    }
}