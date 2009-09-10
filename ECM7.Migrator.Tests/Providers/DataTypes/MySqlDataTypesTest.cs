using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ECM7.Migrator.Providers.MySql;

namespace ECM7.Migrator.Tests.Providers.DataTypes
{
	[TestFixture]
	public class MySqlDataTypesTest : DataTypesTestBase<MySqlDialect>
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
