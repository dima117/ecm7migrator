namespace ECM7.Migrator.Tests2
{
	using System;

	using ECM7.Migrator.Providers;

	using NUnit.Framework;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	[TestFixture]
	public class SqlFormatterTest
	{
		private static string Convert(string arg)
		{
			return "<{0}>".FormatWith(arg);
		}

		private static SqlFormatter formatter = new SqlFormatter(Convert);


		[Test]
		public void FormatterTest()
		{
			string sql = "insert into {0:TAB} set {1:COL} = '{2}'".FormatWith(formatter, "test1", "column1", "value1");
			Assert.AreEqual(sql, "insert into <test1> set <column1> = 'value1'");
		}
	}
}
