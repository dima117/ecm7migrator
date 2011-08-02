namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;

	using Moq;

	using Npgsql;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProviderTests
	{
		[Test]
		public void ExecuteActionsForProvider()
		{
			int i = 0;

			Mock<NpgsqlConnection> conn = new Mock<NpgsqlConnection>();
			ITransformationProvider provider = new GenericTransformationProvider<NpgsqlConnection>(conn.Object);

			provider.For<GenericTransformationProvider<NpgsqlConnection>>(database => i = 5);
			Assert.AreEqual(5, i);

			provider.For<PostgreSQLTransformationProviderTest>(database => i = 15);
			Assert.AreNotEqual(5, i);
		}

		[Test]
		public void CanJoinColumnsAndValues()
		{
			Mock<NpgsqlConnection> conn = new Mock<NpgsqlConnection>();

			var provider = new GenericTransformationProvider<NpgsqlConnection>(conn.Object);
			string result = provider.JoinColumnsAndValues(new[] { "foo", "bar" }, new[] { "123", "456" });

			string expected = provider.FormatSql("{0:NAME}='123' , {1:NAME}='456'", "foo", "bar");
			Assert.AreEqual(expected, result);
		}

	}

	public class GenericTransformationProvider<TConnection> : TransformationProvider<TConnection>
		where TConnection : IDbConnection, new()
	{
		public GenericTransformationProvider(TConnection conn) : base(conn)
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

		#region Overrides of SqlGenerator

		public override bool IdentityNeedsType
		{
			get { throw new NotImplementedException(); }
		}

		public override bool NeedsNotNullForIdentity
		{
			get { throw new NotImplementedException(); }
		}

		public override bool SupportsIndex
		{
			get { throw new NotImplementedException(); }
		}

		public override string NamesQuoteTemplate
		{
			get { throw new NotImplementedException(); }
		}

		public override string BatchSeparator
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}