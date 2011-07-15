namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.SqlServer;

	using Moq;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProviderTests
	{

		[Test]
		public void InstanceForProvider()
		{
			Mock<IDbConnection> conn = new Mock<IDbConnection>();
			ITransformationProvider provider = new GenericTransformationProvider(conn.Object);

			ITransformationProvider localProv = provider.For<GenericDialect>();
			Assert.That(localProv is GenericTransformationProvider);

			ITransformationProvider localProv2 = provider.For<SqlServerDialect>();
			Assert.That(localProv2 is NoOpTransformationProvider);
		}

		[Test]
		public void ExecuteActionsForProvider()
		{
			// TODO:!!!!! проверить везде, что имена таблиц и колонок оборачиваются кавычками;
			int i = 0;

			Mock<IDbConnection> conn = new Mock<IDbConnection>();
			ITransformationProvider provider = new GenericTransformationProvider(conn.Object);

			provider.For<GenericDialect>(database => i = 5);
			Assert.AreEqual(5, i);

			provider.For<SqlServerDialect>(database => i = 15);
			Assert.AreNotEqual(15, i);
		}

		[Test]
		public void CanJoinColumnsAndValues()
		{
			Mock<IDbConnection> conn = new Mock<IDbConnection>();

			GenericTransformationProvider provider = new GenericTransformationProvider(conn.Object);
			string result = provider.JoinColumnsAndValues(new[] { "foo", "bar" }, new[] { "123", "456" });

			string expected = "{0}='123' , {1}='456'".FormatWith(provider.QuoteName("foo"), provider.QuoteName("bar"));
			Assert.AreEqual(expected, result);
		}

	}

	public class GenericDialect : Dialect
	{
		public override Type TransformationProviderType
		{
			get { return typeof(GenericTransformationProvider); }
		}
	}

	public class GenericTransformationProvider : TransformationProvider
	{
		public GenericTransformationProvider(IDbConnection conn)
			: base(new GenericDialect(), conn)
		{
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			return false;
		}

		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}
	}
}