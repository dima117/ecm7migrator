using System;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	[TestFixture]
	public class SqlServerCeTransformationProviderTest
		: TransformationProviderTestBase<SqlServerCeTransformationProvider>
	{
		//protected override string DefaultSchema
		//{
		//    get { return null; }
		//}

		#region Overrides of TransformationProviderTestBase<SqlServerCeTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "SqlServerCeConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
					insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (44, 444)
					GO
					go
					insert into [BatchSqlTest] ([Id], [TestId]) values (55, 555)";
			}
		}

		#endregion

		#region override tests

		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(base.CanAddCheckConstraint);
		}

		[Test]
		public override void CanVerifyThatCheckConstraintIsExist()
		{
			Assert.Throws<NotSupportedException>(base.CanVerifyThatCheckConstraintIsExist);
		}

		[Test]
		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>(base.CanRenameColumn);
		}

		#endregion
	}
}
