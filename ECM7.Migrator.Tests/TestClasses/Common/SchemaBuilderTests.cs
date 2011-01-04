using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.SchemaBuilder;
using NUnit.Framework;
using ForeignKeyConstraint=ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class SchemaBuilderTests
	{
		private SchemaBuilder _schemaBuilder;

		[SetUp]
		public void Setup()
		{
			_schemaBuilder = new SchemaBuilder();
			_schemaBuilder.AddTable("SomeTable");
		}

		[Test]
		public void CanAddTable()
		{
			_schemaBuilder.AddTable("MyUserTable");
			//Assert.AreEqual("MyUserTable", _schemaBuilder.Expressions.ElementAt(0));
		}

		[Test]
		public void CanAddColumn()
		{
			const string COLUMN_NAME = "MyUserId";

			_schemaBuilder
				.AddColumn(COLUMN_NAME);

			//Assert.IsTrue(_schemaBuilder.Columns.Count == 1);
			//Assert.AreEqual(columnName, _schemaBuilder.Columns[0].Name);
		}

		[Test]
		public void CanChainAddColumnOfType()
		{
			_schemaBuilder
				.AddColumn("SomeColumn")
				.OfType(DbType.Int32);

			//Assert.AreEqual(DbType.Int32, _schemaBuilder.Columns[0].Type, "Column.Type was not as expected");
		}

		[Test]
		public void CanChainAddColumnWithProperty()
		{
			_schemaBuilder
				.AddColumn("MyColumn")
				.OfType(DbType.Int32)
				.WithProperty(ColumnProperty.PrimaryKey);

			//Assert.IsTrue(_schemaBuilder.Columns[0].IsPrimaryKey);
		}

		[Test]
		public void CanChainAddColumnWithSize()
		{
			_schemaBuilder
				.AddColumn("column")
				.WithSize(100);

			//Assert.AreEqual(100, _schemaBuilder.Columns[0].Size);
		}

		[Test]
		public void CanChainAddColumnWithDefaultValue()
		{
			_schemaBuilder
				.AddColumn("something")
				.OfType(DbType.Int32)
				.WithDefaultValue("default value");

			//Assert.AreEqual("default value", _schemaBuilder.Columns[0].DefaultValue);
		}

		[Test]
		public void CanChainAddTableWithForeignKey()
		{
			_schemaBuilder
				.AddColumn("MyColumnThatIsForeignKey")
				.AsForeignKey().ReferencedTo("PrimaryKeyTable", "PrimaryKeyColumn").WithConstraint(ForeignKeyConstraint.NoAction);

			//Assert.IsTrue(_schemaBuilder.Columns[0].ColumnProperty == ColumnProperty.ForeignKey);
		}
	}
}