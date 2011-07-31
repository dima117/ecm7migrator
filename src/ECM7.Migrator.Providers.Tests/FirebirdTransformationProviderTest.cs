namespace ECM7.Migrator.Providers.Tests
{
    using System;
    using System.Configuration;
    using System.Data;
    using ECM7.Migrator.Providers.Firebird;
    using Framework;
    using Oracle;

    using NUnit.Framework;

    [TestFixture, Category("Firebird")]
    public class FirebirdTransformationProviderTest : TransformationProviderConstraintBase
    {
        [SetUp]
        public void SetUp()
        {
            string constr = ConfigurationManager.AppSettings["FirebirdConnectionString"];
            if (constr == null)
                throw new ArgumentNullException("FirebirdConnectionString", "No config file");
            provider = new FirebirdTransformationProvider(new FirebirdDialect(), constr);
            provider.BeginTransaction();

            AddDefaultTable();
        }

    }
}