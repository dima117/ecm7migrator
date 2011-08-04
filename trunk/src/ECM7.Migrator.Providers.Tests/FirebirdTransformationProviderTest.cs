namespace ECM7.Migrator.Providers.Tests
{
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
			Require.IsNotNullOrEmpty(constr, "Connection string \"FirebirdConnectionString\" is not exist");

			provider = ProviderFactoryBuilder
				.CreateProviderFactory<FirebirdTransformationProviderFactory>()
				.CreateProvider(constr);
			
            AddDefaultTable();
        }

    }
}