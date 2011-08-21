using System;
using System.Data;
using ECM7.Migrator.Framework;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	public abstract class DataTypesTestBase<TProvider>
		where TProvider : ITransformationProvider
	{
		#region Base

		public abstract string ConnectionString { get; }
		public abstract string ParameterName { get; }
		public virtual int MaxStringFixedLength { get { return 8000; } }
		public virtual int MaxStringVariableLength { get { return 50000; } }
		public virtual long MaxInt64Value { get { return long.MaxValue; } }
		public virtual long MinInt64Value { get { return long.MinValue; } }

		public virtual object BooleanTestValue { get { return true; } }

		public ITransformationProvider Provider { get; set; }

		[SetUp]
		public void Setup()
		{
			Provider = ProviderFactory.Create<TProvider>(ConnectionString);
		}

		[TearDown]
		public void TearDown()
		{
			Provider.Dispose();
		}

		#endregion

		#region Utils

		public void TestColumnType(ColumnType type, object testValue)
		{
			Require.IsNotNull(type, "Не задан тип колнки");

			if (!Provider.TypeIsSupported(type.DataType))
			{
				Console.WriteLine(
					"Тип {0} не поддерживаетя провайдером {1}"
						.FormatWith(type, Provider.GetType().Name));
				return;
			}

			string tableName = "test{0}{1}".FormatWith(type.DataType, type.Length);
			Provider.AddTable(tableName, new Column("testcolumn", type));
			try
			{
				InsertTest(tableName, testValue);
				SelectTest(tableName);
			}
			finally
			{
				Provider.RemoveTable(tableName);
			}
		}

		private void SelectTest(string tableName)
		{
			using (IDbCommand command = Provider.GetCommand())
			{
				command.CommandText = Provider.FormatSql("select {0:NAME} from {1:NAME}", "testcolumn", tableName);

				command.ExecuteScalar();
			}
			//Assert.AreEqual(value, loadedValue);
		}

		private void InsertTest(string tableName, object testValue)
		{
			using (IDbCommand command = Provider.GetCommand())
			{
				command.CommandText = Provider.FormatSql(
					"insert into {0:NAME} ({1:NAME}) values ({2})", tableName, "testcolumn", ParameterName);

				var parameter = command.CreateParameter();
				parameter.ParameterName = ParameterName;
				parameter.Value = testValue;

				command.Parameters.Add(parameter);
				Assert.AreEqual(1, command.ExecuteNonQuery());
			}
		}

		#endregion

		#region Tests

		#region Int16

		[Test]
		public virtual void Int16Test()
		{
			TestColumnType(DbType.Int16, 2);
		}

		[Test]
		public virtual void Int16MaxTest()
		{
			TestColumnType(DbType.Int16, Int16.MaxValue);
		}

		[Test]
		public virtual void Int16MinTest()
		{
			TestColumnType(DbType.Int16, Int16.MinValue);
		}

		#endregion

		#region Int32

		[Test]
		public virtual void Int32Test()
		{
			TestColumnType(DbType.Int32, 2);
		}

		[Test]
		public virtual void Int32MaxTest()
		{
			TestColumnType(DbType.Int32, Int32.MaxValue);
		}

		[Test]
		public virtual void Int32MinTest()
		{
			TestColumnType(DbType.Int32, Int32.MinValue);
		}

		#endregion

		#region Int64

		[Test]
		public virtual void Int64Test()
		{
			TestColumnType(DbType.Int64, 2);
		}

		[Test]
		public virtual void Int64MaxTest()
		{
			TestColumnType(DbType.Int64, MaxInt64Value);
		}

		[Test]
		public virtual void Int64MinTest()
		{
			TestColumnType(DbType.Int64, MinInt64Value);
		}

		#endregion

		#region Decimal

		[Test]
		public virtual void DecimalTest()
		{
			TestColumnType(DbType.Decimal.WithSize(10, 3), 2123423.423m);
		}

		#endregion

		#region Double

		[Test]
		public virtual void DoubleTest()
		{
			TestColumnType(DbType.Double.WithSize(10, 4), 2123423.1423m);
		}

		#endregion

		#region Boolean

		[Test]
		public virtual void BooleanTest()
		{
			TestColumnType(DbType.Boolean, BooleanTestValue);
		}

		#endregion

		#region AnsiStringFixedLength

		[Test]
		public virtual void AnsiStringFixedLengthTest()
		{
			TestColumnType(DbType.AnsiStringFixedLength, "test");
		}

		[Test]
		public virtual void AnsiStringFixedLength100Test()
		{
			TestColumnType(DbType.AnsiStringFixedLength.WithSize(100), "a".Repeat(100));
		}

		[Test]
		public virtual void AnsiStringFixedLength1000Test()
		{
			TestColumnType(DbType.AnsiStringFixedLength.WithSize(1000), "b".Repeat(1000));
		}

		[Test]
		public virtual void AnsiStringFixedLengthMaxTest()
		{
			TestColumnType(DbType.AnsiStringFixedLength.WithSize(MaxStringFixedLength), "c".Repeat(MaxStringFixedLength));
		}

		#endregion

		#region AnsiString

		[Test]
		public virtual void AnsiStringTest()
		{
			TestColumnType(DbType.AnsiString, "test");
		}

		[Test]
		public virtual void AnsiString100Test()
		{
			TestColumnType(DbType.AnsiString.WithSize(100), "a".Repeat(100));
		}

		[Test]
		public virtual void AnsiString1000Test()
		{
			TestColumnType(DbType.AnsiString.WithSize(1000), "b".Repeat(1000));
		}

		[Test]
		public virtual void AnsiStringMaxTest()
		{
			TestColumnType(DbType.AnsiString.WithSize(MaxStringVariableLength), "c".Repeat(MaxStringVariableLength));
		}

		#endregion

		//RegisterColumnType(DbType.Binary, "VARBINARY(8000)");
		//RegisterColumnType(DbType.Binary, 8000, "VARBINARY($l)");
		//RegisterColumnType(DbType.Binary, 2147483647, "IMAGE");
		//RegisterColumnType(DbType.Boolean, "BIT");

		//RegisterColumnType(DbType.Currency, "MONEY");
		//RegisterColumnType(DbType.Date, "DATETIME");
		//RegisterColumnType(DbType.DateTime, "DATETIME");

		//RegisterColumnType(DbType.Guid, "UNIQUEIDENTIFIER");

		//RegisterColumnType(DbType.Single, "REAL"); //synonym for FLOAT(24) 
		//RegisterColumnType(DbType.StringFixedLength, "NCHAR(255)");
		//RegisterColumnType(DbType.StringFixedLength, 4000, "NCHAR($l)");
		//RegisterColumnType(DbType.String, "NVARCHAR(255)");
		//RegisterColumnType(DbType.String, 4000, "NVARCHAR($l)");
		//RegisterColumnType(DbType.String, 1073741823, "NTEXT");
		//RegisterColumnType(DbType.Time, "DATETIME");


		#endregion
	}
}