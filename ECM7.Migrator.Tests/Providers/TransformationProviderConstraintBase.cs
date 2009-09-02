using System.Data;
using ECM7.Migrator.Framework;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers
{
    /// <summary>
    /// Base class for Provider tests for all tests including constraint oriented tests.
    /// </summary>
    public class TransformationProviderConstraintBase : TransformationProviderBase
    {

        public void AddForeignKey()
        {
            AddTableWithPrimaryKey();
            provider.AddForeignKey("FK_Test_TestTwo", "TestTwo", "TestId", "Test", "Id");
        }

        public void AddPrimaryKey()
        {
            AddTable();
            provider.AddPrimaryKey("PK_Test", "Test", "Id");
        }

        public void AddUniqueConstraint()
        {
            provider.AddUniqueConstraint("UN_Test_TestTwo", "TestTwo", "TestId");
        }

        public void AddMultipleUniqueConstraint()
        {
            provider.AddUniqueConstraint("UN_Test_TestTwo", "TestTwo", "Id", "TestId");
        }

        public void AddCheckConstraint()
        {
            provider.AddCheckConstraint("CK_TestTwo_TestId", "TestTwo", "TestId>5");
        }

        [Test]
        public void CanAddPrimaryKey()
        {
            AddPrimaryKey();
            Assert.IsTrue(provider.PrimaryKeyExists("Test", "PK_Test"));
        }

        [Test]
        public void AddIndexedColumn()
        {
            provider.AddColumn("TestTwo", "Test", DbType.String, 50, ColumnProperty.Indexed);
        }

        [Test]
        public void AddUniqueColumn()
        {
            provider.AddColumn("TestTwo", "Test", DbType.String, 50, ColumnProperty.Unique);
        }

        [Test]
        public void CanAddForeignKey()
        {
            AddForeignKey();
            Assert.IsTrue(provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddUniqueConstraint()
        {
            AddUniqueConstraint();
            Assert.IsTrue(provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddMultipleUniqueConstraint()
        {
            AddMultipleUniqueConstraint();
            Assert.IsTrue(provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddCheckConstraint()
        {
            AddCheckConstraint();
            Assert.IsTrue(provider.ConstraintExists("TestTwo", "CK_TestTwo_TestId"));
        }

        [Test]
        public void RemoveForeignKey()
        {
            AddForeignKey();
            provider.RemoveForeignKey("TestTwo", "FK_Test_TestTwo");
            Assert.IsFalse(provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
        }

        [Test]
        public void RemoveUniqueConstraint()
        {
            AddUniqueConstraint();
            provider.RemoveConstraint("TestTwo", "UN_Test_TestTwo");
            Assert.IsFalse(provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void RemoveCheckConstraint()
        {
            AddCheckConstraint();
            provider.RemoveConstraint("TestTwo", "CK_TestTwo_TestId");
            Assert.IsFalse(provider.ConstraintExists("TestTwo", "CK_TestTwo_TestId"));
        }

        [Test]
        public void RemoveUnexistingForeignKey()
        {
            AddForeignKey();
            provider.RemoveForeignKey("abc", "FK_Test_TestTwo");
            provider.RemoveForeignKey("abc", "abc");
            provider.RemoveForeignKey("Test", "abc");
        }

        [Test]
        public void ConstraintExist()
        {
            AddForeignKey();
            Assert.IsTrue(provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
            Assert.IsFalse(provider.ConstraintExists("abc", "abc"));
        }


        [Test]
        public void AddTableWithCompoundPrimaryKey()
        {
            provider.AddTable("Test",
                               new Column("PersonId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("AddressId", DbType.Int32, ColumnProperty.PrimaryKey)
                );
            Assert.IsTrue(provider.TableExists("Test"), "Table doesn't exist");
            Assert.IsTrue(provider.PrimaryKeyExists("Test", "PK_Test"), "Constraint doesn't exist");
        }

        [Test]
        public void AddTableWithCompoundPrimaryKeyShouldKeepNullForOtherProperties()
        {
            provider.AddTable("Test",
                               new Column("PersonId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("AddressId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("Name", DbType.String, 30, ColumnProperty.Null)
                );
            Assert.IsTrue(provider.TableExists("Test"), "Table doesn't exist");
            Assert.IsTrue(provider.PrimaryKeyExists("Test", "PK_Test"), "Constraint doesn't exist");

        	Column column = provider.GetColumnByName("Test", "Name");
            Assert.IsNotNull(column);
            Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
        }
    }
}
