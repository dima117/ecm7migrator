namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;

	using PostgreSQL;

	using NUnit.Framework;

	[TestFixture, Category("Postgre")]
	public class PostgreSQLTransformationProviderTest : TransformationProviderConstraintBase
	{
		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.test.res.migration.sql"; }
		}

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