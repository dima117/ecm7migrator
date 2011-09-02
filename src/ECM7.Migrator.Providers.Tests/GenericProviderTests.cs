namespace ECM7.Migrator.Providers.Tests
{
	using System.Configuration;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.PostgreSQL;
	using ECM7.Migrator.Providers.SqlServer;
	using ECM7.Migrator.Providers.SqlServer.Base;

	using Moq;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProviderTests
	{
		#region for

		[Test]
		public void CanExecuteActionForProvider()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = 5;
				provider.ConditionalExecuteAction()
					.For<SqlServerTransformationProvider>(db => i = 8)
					.For<PostgreSQLTransformationProvider>(db => i = 3)
					.Else(db => i = 18);
				Assert.AreEqual(i, 8);
			}
		}

		[Test]
		public void CanExecuteDifferentActionForDifferentProviders()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			using (var provider = ProviderFactory.Create<PostgreSQLTransformationProvider>(cstring))
			{
				int i = 0;
				provider.ConditionalExecuteAction()
					.For<SqlServerTransformationProvider>(db => i = 22)
					.For<PostgreSQLTransformationProvider>(db => i = 11)
					.Else(db => i = 33);
				Assert.AreEqual(i, 11);
			}
		}

		[Test]
		public void CanExecuteActionForExcludedProviders()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = -1;

				provider.ConditionalExecuteAction()
					.For<SqlServerCeTransformationProvider>(db => i = 44)
					.For<PostgreSQLTransformationProvider>(db => i = 55)
					.Else(db => i = 66);

				Assert.AreEqual(i, 66);
			}
		}

		[Test]
		public void CanExecuteActionForProvidersWithBaseClass()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = -1;

				provider.ConditionalExecuteAction()
						.For<BaseSqlServerTransformationProvider<IDbConnection>>(db => i = 77);
				Assert.AreEqual(77, i);

			}
		}

		#endregion

		[Test]
		public void CanJoinColumnsAndValues()
		{
			Mock<IDbConnection> conn = new Mock<IDbConnection>();

			var provider = new GenericTransformationProvider<IDbConnection>(conn.Object);
			string result = provider.JoinColumnsAndValues(new[] { "foo", "bar" }, new[] { "123", "456" });

			string expected = provider.FormatSql("{0:NAME}='123' , {1:NAME}='456'", "foo", "bar");
			Assert.AreEqual(expected, result);
		}

	}

	public class GenericTransformationProvider<TConnection> : TransformationProvider<TConnection>
		where TConnection : IDbConnection
	{
		public GenericTransformationProvider(TConnection conn)
			: base(conn)
		{
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			return false;
		}

		public override bool TableExists(string table)
		{
			return false;
		}

		public override bool ColumnExists(string table, string column)
		{
			return false;
		}

		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}
	}
}