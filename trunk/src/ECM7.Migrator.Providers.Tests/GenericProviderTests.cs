namespace ECM7.Migrator.Providers.Tests
{
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;

	using Moq;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProviderTests
	{
		[Test]
		public void ExecuteActionsForProvider()
		{
			int i = 0;

			Mock<IDbConnection> conn = new Mock<IDbConnection>();
			ITransformationProvider provider = new GenericTransformationProvider<IDbConnection>(conn.Object);

			// передаем реальный класс провайдера
			provider.For<GenericTransformationProvider<IDbConnection>>(database => i = 5);
			Assert.AreEqual(5, i);

			// передаем левый класс
			provider.For<GenericProviderTests>(database => i = 15);
			Assert.AreEqual(5, i);
		}

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
		public GenericTransformationProvider(TConnection conn) : base(conn)
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

		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}
	}
}