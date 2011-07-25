namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data.SqlServerCe;
	using System.IO;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture, Category("SqlServerCe")]
	public class SqlServerCeTransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{

			string constr = ConfigurationManager.AppSettings["SqlServerCeConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("SqlServerCeConnectionString", "No config file");

			EnsureDatabase(constr);

			provider = new SqlServerCeTransformationProvider(new SqlServerCeDialect(), constr);
			provider.BeginTransaction();

			AddDefaultTable();
		}

		private static void EnsureDatabase(string constr)
		{
			SqlCeConnection connection = new SqlCeConnection(constr);
			if (!File.Exists(connection.Database))
			{
				SqlCeEngine engine = new SqlCeEngine(constr);
				engine.CreateDatabase();
			}
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public override void CanAddCheckConstraint()
		{
			base.CanAddCheckConstraint();
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public override void RemoveCheckConstraint()
		{
			base.RemoveCheckConstraint();
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public override void RenameColumnThatExists()
		{
			base.RenameColumnThatExists();
		}
	}
}