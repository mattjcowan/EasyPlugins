using System;
using EasyPlugins.Mock;
using NUnit.Framework;

namespace EasyPlugins.Tests
{
    [TestFixture]
    public class RunInSeparateAppDomainTests
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
            PluginManager.Initialize(new MockHostFactory());
            Assert.IsFalse(_initializationDuration.HasValue);
            _initializationDuration = PluginManager.Instance.InitializationDuration;
        }

        [Test, RunInApplicationDomain]
        public void RunInSeparateApplicationDomainTest2()
        {
            //RunInSeparateApplicationDomainTest1 and RunInSeparateApplicationDomainTest2 will fail 
            //if the RunInApplicationDomain isn't executing in separate app domains
            PluginManager.Initialize(new MockHostFactory());
            Assert.IsFalse(_initializationDuration.HasValue);
            _initializationDuration = PluginManager.Instance.InitializationDuration;
        }

        // UNCOMMENT THE FOLLOWING TO TEST JUST THIS FILE, BUT COMMENT OUT FOR THE WHOLE TEST SUITE

        //[Test, Order(1)]
        //public void RunInSameApplicationDomainTest1()
        //{
        //    PluginManager.Initialize(PluginManagerTests.new MockHostFactory());
        //    _initializationStartedAt = PluginManager.Instance.Config.InitializationStartedAt;
        //}

        //[Test, Order(2)]
        //public void RunInSameApplicationDomainTest2()
        //{
        //    try
        //    {
        //        PluginManager.Initialize(PluginManagerTests.new MockHostFactory());
        //    }
        //    catch (Exception ex)
        //    {
        //        var epx = ex as EasyPluginException;
        //        Assert.IsNotNull(epx, "Wrong exception type: " + ex.GetType());
        //        Assert.AreEqual(ErrorCode.AlreadyInitialized, epx.ErrorCode);
        //    }
        //    Assert.IsTrue(_initializationStartedAt.HasValue);
        //}
    }
}
