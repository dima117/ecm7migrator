namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;

	using ECM7.Migrator.Providers.PostgreSQL;

	using NUnit.Framework;

	[TestFixture, Category("Postgre")]
	public class PostgreSQLTransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("ConnectionString", "No config file");

			provider = new PostgreSQLTransformationProvider(new PostgreSQLDialect(), constr);
			provider.BeginTransaction();
            
			AddDefaultTable();
		}
	}
}