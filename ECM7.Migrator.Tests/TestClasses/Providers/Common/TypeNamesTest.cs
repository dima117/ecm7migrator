using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Providers.Common
{
	[TestFixture]
	public class TypeNamesTest
	{
		[Test]
		public void PutAndGetTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.AnsiString, "TEXT");
			names.Put(DbType.AnsiString, 255, "VARCHAR($l)");
			names.Put(DbType.AnsiString, 65534, "LONGVARCHAR($l)");

			Assert.AreEqual("TEXT", names.Get(DbType.AnsiString));						// default
			Assert.AreEqual("VARCHAR(100)", names.Get(DbType.AnsiString, 100));		// 100 is in [0:255]
			Assert.AreEqual("LONGVARCHAR(1000)", names.Get(DbType.AnsiString, 1000));	// 100 is in [256:65534])
			Assert.AreEqual("TEXT", names.Get(DbType.AnsiString, 100000));				// default

		}

		[Test]
		public void PutAndGetByColumnTypeTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.AnsiString, "TEXT");
			names.Put(DbType.AnsiString, 255, "VARCHAR($l)");
			names.Put(DbType.AnsiString, 65534, "LONGVARCHAR($l)");

			Assert.AreEqual("TEXT", names.Get(new ColumnType(DbType.AnsiString)));						// default
			Assert.AreEqual("VARCHAR(100)", names.Get(DbType.AnsiString.WithSize(100)));		// 100 is in [0:255]
			Assert.AreEqual("LONGVARCHAR(1000)", names.Get(DbType.AnsiString.WithSize(1000)));	// 100 is in [256:65534])
			Assert.AreEqual("TEXT", names.Get(DbType.AnsiString.WithSize(100000)));				// default

		}

		[Test]
		public void PutAndGetTest2()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.AnsiString, "VARCHAR($l)");

			Assert.AreEqual("VARCHAR($l)", names.Get(DbType.AnsiString));			// will cause trouble
			Assert.AreEqual("VARCHAR(100)", names.Get(DbType.AnsiString, 100));
			Assert.AreEqual("VARCHAR(1000)", names.Get(DbType.AnsiString, 1000));
			Assert.AreEqual("VARCHAR(10000)", names.Get(DbType.AnsiString, 10000));

		}

		[Test]
		public void PutAndGetByColumnTypeTest2()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.AnsiString, "VARCHAR($l)");

			Assert.AreEqual("VARCHAR($l)", names.Get(new ColumnType(DbType.AnsiString)));			// will cause trouble
			Assert.AreEqual("VARCHAR(100)", names.Get(DbType.AnsiString.WithSize(100)));
			Assert.AreEqual("VARCHAR(1000)", names.Get(DbType.AnsiString.WithSize(1000)));
			Assert.AreEqual("VARCHAR(10000)", names.Get(DbType.AnsiString.WithSize(10000)));

		}

		[Test]
		public void PutAndGetDecimalTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.Decimal, "NUMBER");
			names.Put(DbType.Decimal, 18, "NUMBER($l, $s)");

			Assert.AreEqual("NUMBER", names.Get(DbType.Decimal));
			Assert.AreEqual("NUMBER(10, 5)", names.Get(DbType.Decimal, 10, 5));
			Assert.AreEqual("NUMBER(7, $s)", names.Get(DbType.Decimal, 7)); // will cause trouble
		}

		[Test]
		public void PutAndGetDecimalByColumnTypeTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.Decimal, "NUMBER");
			names.Put(DbType.Decimal, 18, "NUMBER($l, $s)");

			Assert.AreEqual("NUMBER(11, 2)", names.Get(DbType.Decimal.WithSize(11, 2)));
			Assert.AreEqual("NUMBER(8, $s)", names.Get(DbType.Decimal.WithSize(8))); // will cause trouble
		}

		[Test]
		public void ReplacingWithDefaultScaleTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.Xml, 20, "foo($l, $s)", 5);
			Assert.AreEqual("foo(12, 7)", names.Get(DbType.Xml, 12, 7));
			Assert.AreEqual("foo(12, 5)", names.Get(DbType.Xml, 12));
		}

		[Test]
		public void HasTypeTest()
		{
			TypeNames names = new TypeNames();
			Assert.IsFalse(names.HasType(DbType.Int32));
			names.Put(DbType.Int32, string.Empty);
			Assert.IsTrue(names.HasType(DbType.Int32));
		}

		[Test]
		public void HasTypeWithLengthTest()
		{
			TypeNames names = new TypeNames();
			names.Put(DbType.Int32, 4, string.Empty);
			Assert.IsTrue(names.HasType(DbType.Int32));
		}
	}
}