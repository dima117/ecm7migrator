using System.Configuration;
using ECM7.Migrator.Providers.SQLite;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Providers.DataTypes
{
	[TestFixture]
	public class SQLiteDataTypesTest : DataTypesTestBase<SQLiteDialect>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["SQLiteConnectionString"]; }
		}

		public override string ParameterName
		{
			get { return "@value"; }
		}
	}
}