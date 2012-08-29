namespace ECM7.Migrator.Tests2
{
	using System;
	using System.Configuration;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture, Category("SqlServer")]
	public class MigrationKeyTests
	{
		[Test]
		public void UpdateOldStyleSchemaInfo()
		{
			// строка подключения
			string constr = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			Require.IsNotNullOrEmpty(constr, "Connection string \"SqlServerConnectionString\" is not exist");


			// провайдер
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(constr, null))
			{
				if (provider.TableExists("SchemaInfo"))
				{
					provider.RemoveTable("SchemaInfo");
				}

				// добавляем таблицу для версий, имеющую старую структуру
				provider.AddTable("SchemaInfo", new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey));
				provider.Insert("SchemaInfo", new[] { "Version" }, new[] { "1" });

				Assert.AreEqual(1, provider.GetAppliedMigrations().Count);

				string sql = provider.FormatSql("SELECT {0:NAME}, {1:NAME} FROM {2:NAME}", "Key", "Version", "SchemaInfo");
				using (IDataReader reader = provider.ExecuteReader(sql))
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(string.Empty, reader[0]);
					Assert.AreEqual(1, Convert.ToInt32(reader[1]));
					Assert.IsFalse(reader.Read());
				}

				provider.RemoveTable("SchemaInfo");
			}
		}
	}
}
