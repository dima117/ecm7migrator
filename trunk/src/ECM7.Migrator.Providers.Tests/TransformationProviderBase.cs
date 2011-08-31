namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data;
	using System.Reflection;

	using ECM7.Migrator.Framework;

	using log4net.Config;

	using Npgsql;

	using NUnit.Framework;

	/// <summary>
	/// Base class for Provider tests for all non-constraint oriented tests.
	/// </summary>
	//[TestFixture(Ignore = true, IgnoreReason = "")]
	public abstract class TransformationProviderBase<TProvider> where TProvider : ITransformationProvider
	{
		protected ITransformationProvider provider;

		protected static bool isInitialized;

		protected virtual string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.test.res.migration.sql"; }
		}

		protected virtual string BatchSql
		{
			get
			{
				return "переопределите SQL для проверки пакетов запросов";
			}
		}

		public abstract string ConnectionStrinSettingsName { get; }
		public abstract bool UseTransaction { get; }

		[SetUp]
		public virtual void SetUp()
		{
			if (!isInitialized)
			{
				BasicConfigurator.Configure();
				isInitialized = true;
			}

			string constr = ConfigurationManager.AppSettings[ConnectionStrinSettingsName];
			Require.IsNotNullOrEmpty(constr, "Connection string \"{0}\" is not exist", ConnectionStrinSettingsName);

			provider = ProviderFactory.Create<TProvider>(constr);

			if (UseTransaction)
			{
				provider.BeginTransaction();
			}

			AddDefaultTable();
		}

		[TearDown]
		public virtual void TearDown()
		{
			DropTestTables();

			if (UseTransaction)
			{
				provider.Rollback();
			}

			provider.Dispose();
		}

		protected void DropTestTables()
		{
			// Because MySql doesn't support schema transaction
			// we got to remove the tables manually... sad...
			provider.RemoveTable("TestTwo");
			provider.RemoveTable("Test");
			provider.RemoveTable("SchemaInfo");
		}

		public void AddDefaultTable()
		{
			provider.AddTable("TestTwo",
				new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("TestId", DbType.Int32));
		}

		[Test]
		public void CanExecuteBatches()
		{
			provider.ExecuteNonQuery(BatchSql);

			string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} ORDER BY {2:NAME}", "TestId", "TestTwo", "Id");

			using (var reader = provider.ExecuteReader(sql))
			{
				for (int i = 1; i <= 5; i++)
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(111 * i, Convert.ToInt32(reader[0]));
				}
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void CanExecuteScriptFromResources()
		{
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");
			provider.ExecuteFromResource(asm, ResourceSql);

			string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} WHERE {2:NAME} = {3}",
				"TestId", "TestTwo", "Id", 5555);

			object res = provider.ExecuteScalar(sql);

			Assert.AreEqual(9999, Convert.ToInt32(res));
		}

		[Test]
		public void CanExecuteBadSqlForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>(
				database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		[Test]
		public void CanExecuteBadSqlInDelegateForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>(
				database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		/// <summary>
		/// Reproduce bug reported by Luke Melia & Daniel Berlinger :
		/// http://macournoyer.wordpress.com/2006/10/15/migrate-nant-task/#comment-113
		/// </summary>
		[Test]
		public void CommitTwice()
		{
			provider.Commit();
			Assert.AreEqual(0, provider.GetAppliedMigrations(string.Empty).Count);
			provider.Commit();
		}
	}
}
