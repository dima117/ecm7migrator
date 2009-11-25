using System;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers
{
	[TestFixture]
	public class GenericProviderTests
	{

		[Test]
		public void InstanceForProvider()
		{
			ITransformationProvider provider = new GenericTransformationProvider();
			ITransformationProvider localProv = provider.For<GenericDialect>();
			Assert.That(localProv is GenericTransformationProvider);

			ITransformationProvider localProv2 = provider.For<SqlServerDialect>();
			Assert.That(localProv2 is NoOpTransformationProvider);
		}

		[Test]
		public void ExecuteActionsForProvider()
		{
			int i = 0;
			ITransformationProvider provider = new GenericTransformationProvider();

			provider.For<GenericDialect>(database => i = 5);
			Assert.AreEqual(5, i);

			provider.For<SqlServerDialect>(database => i = 15);
			Assert.AreNotEqual(15, i);
		}

		[Test]
		public void CanJoinColumnsAndValues()
		{
			GenericTransformationProvider provider = new GenericTransformationProvider();
			string result = provider.JoinColumnsAndValues(new[] { "foo", "bar" }, new[] { "123", "456" });

			Assert.AreEqual("foo='123' , bar='456'", result);
		}

	}

	public class GenericDialect : Dialect
	{
		public override Type TransformationProviderType
		{
			get { return typeof(GenericTransformationProvider); }
		}
	}

	public class GenericTransformationProvider : TransformationProvider
	{
		public GenericTransformationProvider()
			: base(new GenericDialect(), null)
		{
		}

		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}
	}
}