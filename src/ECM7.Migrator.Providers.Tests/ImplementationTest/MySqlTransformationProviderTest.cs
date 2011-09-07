namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using System;

	using ECM7.Migrator.Providers.MySql;

	using NUnit.Framework;

	[TestFixture]
	public class MySqlTransformationProviderTest
		: TransformationProviderTestBase<MySqlTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<MySqlTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "MySqlConnectionString"; }
		}

		#endregion

		#region override tests

		[Test]
		public override void CanVerifyThatCheckConstraintIsExist()
		{
			// todo: пройтись по всем тестам с NotSupportedException и проверить необходимость выдачи исключения
			Assert.Throws<NotSupportedException>(() =>
				base.CanVerifyThatCheckConstraintIsExist());
		}

		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddCheckConstraint());
		}

		[Test]
		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanRenameColumn());
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddForeignKeyWithDeleteSetDefault());
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddForeignKeyWithUpdateSetDefault());
		}


		#endregion
	}
}
