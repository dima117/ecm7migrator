using System;
using System.Configuration;
using ECM7.Migrator.Tools;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Tools
{
	[TestFixture, Category("MySql")]
	public class SchemaDumperTest
	{
		[Test]
		public void Dump()
		{

			string constr = ConfigurationManager.AppSettings["MySqlConnectionString"];

			if (constr == null)
				throw new ArgumentNullException("MySqlConnectionString", "No config file");

			SchemaDumper dumper = new SchemaDumper("ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql", constr);
			string output = dumper.Dump();
			
			Assert.IsNotNull(output);
		}
	}
}