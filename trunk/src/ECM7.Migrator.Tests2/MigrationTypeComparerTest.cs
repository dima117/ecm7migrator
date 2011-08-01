namespace ECM7.Migrator.Tests2
{
	using System;
	using System.Collections.Generic;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Loader;

	using NUnit.Framework;

	[TestFixture]
	public class MigrationTypeComparerTest
	{
		private readonly Type[] types =
			{
				typeof(Migration1),
				typeof(Migration2),
				typeof(Migration3)
			};

		[Test]
		public void SortAscending()
		{
			List<MigrationInfo> list = new List<MigrationInfo>
			{
				new MigrationInfo(types[1]),
				new MigrationInfo(types[0]),
				new MigrationInfo(types[2])
			};

			list.Sort(new MigrationInfoComparer(true));

			for (int i = 0; i < 3; i++)
			{
				Assert.AreSame(types[i], list[i].Type);
			}
		}

		[Test]
		public void SortDescending()
		{
			List<MigrationInfo> list = new List<MigrationInfo>
			{
				new MigrationInfo(types[1]),
				new MigrationInfo(types[0]),
				new MigrationInfo(types[2])
			};

			list.Sort(new MigrationInfoComparer(false));

			for (int i = 0; i < 3; i++)
			{
				Assert.AreSame(types[2 - i], list[i].Type);
			}
		}

		[Migration(1, Ignore = true)]
		internal class Migration1 : Migration
		{
			override public void Up() { }
			override public void Down() { }
		}

		[Migration(2, Ignore = true)]
		internal class Migration2 : Migration
		{
			override public void Up() { }
			override public void Down() { }
		}

		[Migration(3, Ignore = true)]
		internal class Migration3 : Migration
		{
			override public void Up() { }
			override public void Down() { }
		}
	}
}
