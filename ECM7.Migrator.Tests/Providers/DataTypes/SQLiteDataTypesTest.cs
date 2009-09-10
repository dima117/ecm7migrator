using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ECM7.Migrator.Providers.SQLite;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers.DataTypes
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
