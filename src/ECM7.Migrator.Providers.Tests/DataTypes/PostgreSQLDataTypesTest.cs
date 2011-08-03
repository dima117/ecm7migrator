using System.Configuration;
using ECM7.Migrator.Providers.PostgreSQL;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	[TestFixture]
	public class PostgreSQLDataTypesTest : DataTypesTestBase<PostgreSQLTransformationProviderFactory>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["NpgsqlConnectionString"]; }
		}

		public override string ParameterName
		{
			get { return "@value"; }
		}
	}
}