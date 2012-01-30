using System;
using System.Text;
using ECM7.Migrator.Providers.Firebird;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema
{
	[TestFixture]
	public class FirebirdTransformationProviderTest
		: TransformationProviderTestBase<FirebirdTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<FirebirdTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "FirebirdConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (11, 111);");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (22, 222);");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (33, 333);");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (44, 444);");
				sb.AppendLine("/");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (55, 555);");

				return sb.ToString();
			}
		}

		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.fb.test.res.migration.sql"; }
		}

		#endregion

		#region tests

		[Test]
		public override void CanRenameTable()
		{
			Assert.Throws<NotSupportedException>(() => base.CanRenameTable());
		}

		#endregion

		protected override string GetRandomName(string baseName = "")
		{
			return base.GetRandomName(baseName).Substring(0, 27);
		}
	}
}
