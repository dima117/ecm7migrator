using System.Configuration;
using ECM7.Migrator.Providers.MySql;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	[TestFixture]
	public class MySqlDataTypesTest : DataTypesTestBase<MySqlTransformationProviderFactory>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["MySqlConnectionString"]; }
		}

		public override int MaxStringFixedLength { get { return 2000; } }

		public override string ParameterName
		{
			get { return "?"; }
		}
	}
}