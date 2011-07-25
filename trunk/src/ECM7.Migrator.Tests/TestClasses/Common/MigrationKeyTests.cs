using System;
using System.Configuration;
using System.Data;

using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.SqlServer;

using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture, Category("SqlServer")]
	public class MigrationKeyTests
	{
		[Test]
		public void UpdateOldStyleSchemaInfo()
		{
			// TODO: проверить получение списка выполненных миграций по ключу
			var provider = this.CreateSqlServerProvider("some key");
			if (provider.TableExists("SchemaInfo"))
			{
				provider.RemoveTable("SchemaInfo");
			}

			provider.AddTable(
				"SchemaInfo",
				new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey));
			provider.Insert("SchemaInfo", new[] { "Version" }, new[] { "1" });

			Assert.AreEqual(0, provider.GetAppliedMigrations(string.Empty).Count);

			using (IDataReader reader = provider.ExecuteQuery("SELECT [Key], Version FROM SchemaInfo"))
			{
				reader.Read();
				Assert.AreEqual(string.Empty, reader[0]);
				Assert.AreEqual(1, ((Int64)reader[1]));
				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable("SchemaInfo");
		}

		#region Helpers

		private static string ConnectionString
		{
			get
			{
				string constr = ConfigurationManager.AppSettings["SqlServerConnectionString"];
				if (constr == null)
				{
					throw new ArgumentNullException("SqlServerConnectionString", "No config file");
				}

				return constr;
			}
		}

		public ITransformationProvider CreateSqlServerProvider(string key)
		{
			var provider = new SqlServerTransformationProvider(new SqlServerDialect(), ConnectionString);

			return provider;
		}

		#endregion
	}
}
