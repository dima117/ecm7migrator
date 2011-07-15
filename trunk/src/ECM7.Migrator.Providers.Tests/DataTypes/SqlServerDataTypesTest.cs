using System.Configuration;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	[TestFixture]
	public class SqlServerDataTypesTest : DataTypesTestBase<SqlServerDialect>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["SqlServerConnectionString"]; }
		}

		public override string ParameterName
		{
			get { return "@value"; }
		}
	}
}