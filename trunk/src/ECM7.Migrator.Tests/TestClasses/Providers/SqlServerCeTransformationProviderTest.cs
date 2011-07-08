using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Providers
{
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

			provider = new SqlServerCeTransformationProvider(new SqlServerCeDialect(), constr, null);
			provider.BeginTransaction();

			AddDefaultTable();
		}

		private void EnsureDatabase(string constr)
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
		// see: http://www.pocketpcdn.com/articles/articles.php?&atb.set(c_id)=74&atb.set(a_id)=8145&atb.perform(details)=&
		public override void RenameTableThatExists()
		{
			base.RenameTableThatExists();
		}
	}
}