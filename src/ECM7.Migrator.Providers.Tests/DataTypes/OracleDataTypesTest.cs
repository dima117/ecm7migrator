using System.Configuration;
using ECM7.Migrator.Providers.Oracle;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	[TestFixture]
	public class OracleDataTypesTest : DataTypesTestBase<OracleTransformationProvider>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["OracleConnectionString"]; }
		}

		public override int MaxStringFixedLength { get { return 2000; } }

		public override object BooleanTestValue { get { return 1; } }

		public override long MaxInt64Value
		{
			get { return 999999999999999999; }
		}

		public override long MinInt64Value
		{
			get { return -999999999999999999; }
		}

		public override string ParameterName
		{
			get { return ":value"; }
		}
	}
}