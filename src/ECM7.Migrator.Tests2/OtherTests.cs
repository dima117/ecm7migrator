using System;
using System.Collections.Generic;
using System.Configuration;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.SqlServer;
using NLog;
using NUnit.Framework;

namespace ECM7.Migrator.Tests2
{
	[TestFixture]
	public class OtherTests
	{
		[Test]
		public void CanGetMigrationHumanName()
		{
			Assert.AreEqual(
			"Migration0101 add new table with primary key",
			StringUtils.ToHumanName("Migration0101_Add_NewTable_with_primary___Key"));
		}

		[Test]
		public void ProviderFinalizeTest()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			var provider = ProviderFactory.Create(typeof(SqlServerTransformationProvider), cstring);
			provider.ExecuteScalar("select 1");

			provider = null;
			GC.Collect();
		}

		#region object converter

		public class Gwelkghlw
		{
			private readonly Dictionary<int, string> dic = new Dictionary<int, string>();

			public int Id { get; set; }

			public string this[int index]
			{
				get
				{
					return dic.ContainsKey(index) ? dic[index] : null;
				}
				set { dic[index] = value; }
			}
		}

		[Test]
		public void CanConvertNullObjectToArrays()
		{
			var arrays = TransformationProvider.ConvertObjectToArrays(null);
			Assert.IsNull(arrays);
		}

		[Test]
		public void CanConvertObjectWithIndexedFieldToArrays()
		{
			var obj = new Gwelkghlw { Id = 1254 };
			obj[12] = "qewgfwgewrgh";
			obj[13] = "eljpowwdoihgvwoihio";

			var arrays = TransformationProvider.ConvertObjectToArrays(obj);

			Assert.AreEqual(new[] { "Id" }, arrays.Item1);
			Assert.AreEqual(new[] { "1254" }, arrays.Item2);
		}

		[Test]
		public void CanConvertObjectWithNullFieldToArrays()
		{
			var obj = new { x = 1254, y = (object)null, z = (string)null };

			var arrays = TransformationProvider.ConvertObjectToArrays(obj);

			Assert.AreEqual(new[] { "x", "y", "z" }, arrays.Item1);
			Assert.AreEqual(new[] { "1254", null, null }, arrays.Item2);
		}

		#endregion
	}
}
