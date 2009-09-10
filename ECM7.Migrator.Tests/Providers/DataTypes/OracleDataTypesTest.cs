using System;
using System.Configuration;
using ECM7.Migrator.Providers.Oracle;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers.DataTypes
{
	[TestFixture]
	public class OracleDataTypesTest : DataTypesTestBase<OracleDialect>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["OracleConnectionString"]; }
		}

		public override int MaxStringFixedLength { get { return 2000; } }

		public override object BooleanTestValue { get { return 1; } }

		public override string ParameterName
		{
			get { return ":value"; }
		}
	}
}
