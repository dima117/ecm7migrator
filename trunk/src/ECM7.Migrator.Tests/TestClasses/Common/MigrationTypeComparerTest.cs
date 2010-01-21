using System;
using System.Collections.Generic;
using ECM7.Migrator.Framework;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class MigrationTypeComparerTest
	{
		private readonly Type[] types = {
		                                	typeof(Migration1),
		                                	typeof(Migration2),
		                                	typeof(Migration3)
		                                };
		
		[Test]
		public void SortAscending()
		{
			List<Type> list = new List<Type>();
			
			list.Add(types[1]);
			list.Add(types[0]);
			list.Add(types[2]);
			
			list.Sort(new MigrationTypeComparer(true));
			
			for (int i = 0; i < 3; i++) {
				Assert.AreSame(types[i], list[i]);
			}			
		}
		
		[Test]
		public void SortDescending()
		{
			List<Type> list = new List<Type>();
			
			list.Add(types[1]);
			list.Add(types[0]);
			list.Add(types[2]);
			
			list.Sort(new MigrationTypeComparer(false));
			
			for (int i = 0; i < 3; i++) {
				Assert.AreSame(types[2-i], list[i]);
			}			
		}
				
		[Migration(1, Ignore=true)]
		internal class Migration1 : Migration {
			override public void Up() {}
			override public void Down() {}
		}
		
		[Migration(2, Ignore=true)]
		internal class Migration2 : Migration {
			override public void Up() {}
			override public void Down() {}
		}

		[Migration(3, Ignore=true)]
		internal class Migration3 : Migration {
			override public void Up() {}
			override public void Down() {}
		}
	}
}