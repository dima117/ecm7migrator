using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests2.Providers
{
	[TestFixture]
	public class TypeNamesTest
	{
		[Test]
		public void PutAndGetTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.AnsiString, "TEXT");
			map.Put(DbType.AnsiString, 255, "VARCHAR($l)");
			map.Put(DbType.AnsiString, 65534, "LONGVARCHAR($l)");

			Assert.AreEqual("TEXT", map.Get(DbType.AnsiString));						// default
			Assert.AreEqual("VARCHAR(100)", map.Get(DbType.AnsiString, 100));		// 100 is in [0:255]
			Assert.AreEqual("LONGVARCHAR(1000)", map.Get(DbType.AnsiString, 1000));	// 100 is in [256:65534])
			Assert.AreEqual("TEXT", map.Get(DbType.AnsiString, 100000));				// default

		}

		[Test]
		public void PutAndGetByColumnTypeTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.AnsiString, "TEXT");
			map.Put(DbType.AnsiString, 255, "VARCHAR($l)");
			map.Put(DbType.AnsiString, 65534, "LONGVARCHAR($l)");

			Assert.AreEqual("TEXT", map.Get(new ColumnType(DbType.AnsiString)));						// default
			Assert.AreEqual("VARCHAR(100)", map.Get(DbType.AnsiString.WithSize(100)));		// 100 is in [0:255]
			Assert.AreEqual("LONGVARCHAR(1000)", map.Get(DbType.AnsiString.WithSize(1000)));	// 100 is in [256:65534])
			Assert.AreEqual("TEXT", map.Get(DbType.AnsiString.WithSize(100000)));				// default

		}

		[Test]
		public void PutAndGetTest2()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.AnsiString, "VARCHAR($l)");

			Assert.AreEqual("VARCHAR($l)", map.Get(DbType.AnsiString));			// will cause trouble
			Assert.AreEqual("VARCHAR(100)", map.Get(DbType.AnsiString, 100));
			Assert.AreEqual("VARCHAR(1000)", map.Get(DbType.AnsiString, 1000));
			Assert.AreEqual("VARCHAR(10000)", map.Get(DbType.AnsiString, 10000));

		}

		[Test]
		public void PutAndGetByColumnTypeTest2()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.AnsiString, "VARCHAR($l)");

			Assert.AreEqual("VARCHAR($l)", map.Get(new ColumnType(DbType.AnsiString)));			// will cause trouble
			Assert.AreEqual("VARCHAR(100)", map.Get(DbType.AnsiString.WithSize(100)));
			Assert.AreEqual("VARCHAR(1000)", map.Get(DbType.AnsiString.WithSize(1000)));
			Assert.AreEqual("VARCHAR(10000)", map.Get(DbType.AnsiString.WithSize(10000)));

		}

		[Test]
		public void PutAndGetDecimalTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.Decimal, "NUMBER");
			map.Put(DbType.Decimal, 18, "NUMBER($l, $s)");

			Assert.AreEqual("NUMBER", map.Get(DbType.Decimal));
			Assert.AreEqual("NUMBER(10, 5)", map.Get(DbType.Decimal, 10, 5));
			Assert.AreEqual("NUMBER(7, $s)", map.Get(DbType.Decimal, 7)); // will cause trouble
		}

		[Test]
		public void PutAndGetDecimalByColumnTypeTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.Decimal, "NUMBER");
			map.Put(DbType.Decimal, 18, "NUMBER($l, $s)");

			Assert.AreEqual("NUMBER(11, 2)", map.Get(DbType.Decimal.WithSize(11, 2)));
			Assert.AreEqual("NUMBER(8, $s)", map.Get(DbType.Decimal.WithSize(8))); // will cause trouble
		}

		[Test]
		public void ReplacingWithDefaultScaleTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.Xml, 20, "foo($l, $s)", 5);
			Assert.AreEqual("foo(12, 7)", map.Get(DbType.Xml, 12, 7));
			Assert.AreEqual("foo(12, 5)", map.Get(DbType.Xml, 12));
		}

		[Test]
		public void HasTypeTest()
		{
			TypeMap map = new TypeMap();
			Assert.IsFalse(map.HasType(DbType.Int32));
			map.Put(DbType.Int32, string.Empty);
			Assert.IsTrue(map.HasType(DbType.Int32));
		}

		[Test]
		public void HasTypeWithLengthTest()
		{
			TypeMap map = new TypeMap();
			map.Put(DbType.Int32, 4, string.Empty);
			Assert.IsTrue(map.HasType(DbType.Int32));
		}
	}
}