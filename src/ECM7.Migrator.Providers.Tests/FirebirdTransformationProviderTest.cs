namespace ECM7.Migrator.Providers.Tests
{
    using System;
    using System.Configuration;

    using ECM7.Migrator.Providers.Firebird;

    using NUnit.Framework;

    [TestFixture, Category("Firebird")]
    public class FirebirdTransformationProviderTest : TransformationProviderConstraintBase
    {
        [SetUp]
        public void SetUp()
        {
            string constr = ConfigurationManager.AppSettings["FirebirdConnectionString"];
            if (constr == null)
            {
            	throw new ArgumentNullException("FirebirdConnectionString", "No config file");
            }

            provider = new FirebirdTransformationProvider(new FirebirdDialect(), constr);
            provider.BeginTransaction();

            AddDefaultTable();
        }

    }
}