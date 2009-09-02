#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion

using System;
using System.Configuration;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Mysql;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers
{
    [TestFixture, Category("MySql")]
    public class MySqlTransformationProviderTest : TransformationProviderConstraintBase
    {
        [SetUp]
        public void SetUp()
        {
            string constr = ConfigurationManager.AppSettings["MySqlConnectionString"];
            if (constr == null)
                throw new ArgumentNullException("MySqlConnectionString", "No config file");
            provider = new MySqlTransformationProvider(new MysqlDialect(), constr);
            // provider.Logger = new Logger(true, new ConsoleWriter());

            AddDefaultTable();
        }

        [TearDown]
        public override void TearDown()
        {
            DropTestTables();
        }

        [Test]
        public void AddTableWithMyISAMEngine()
        {
            provider.AddTable("Test", "MyISAM",
                               new Column("Id", DbType.Int32, ColumnProperty.NotNull),
                               new Column("name", DbType.String, 50)
                );
        }

		// [Test,Ignore("MySql doesn't support check constraints")]
        public override void CanAddCheckConstraint() {}

    }
}