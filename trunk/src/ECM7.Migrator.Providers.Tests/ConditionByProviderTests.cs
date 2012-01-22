using System;
using System.Data.SqlClient;

namespace ECM7.Migrator.Providers.Tests
{
	using System.Configuration;
	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.PostgreSQL;
	using ECM7.Migrator.Providers.SqlServer;
	using ECM7.Migrator.Providers.SqlServer.Base;

	using NUnit.Framework;

	[TestFixture]
	public class ConditionByProviderTests
	{
		[Test]
		public void CanExecuteActionForProvider()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = 5;
				provider.ConditionalExecuteAction()
					.For<SqlServerTransformationProvider>(db => i = 8)
					.For<PostgreSQLTransformationProvider>(db => i = 3)
					.Else(db => i = 18);
				Assert.AreEqual(i, 8);
			}
		}

		[Test]
		public void CanExecuteDifferentActionForDifferentProviders()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			using (var provider = ProviderFactory.Create<PostgreSQLTransformationProvider>(cstring))
			{
				int i = 0;
				provider.ConditionalExecuteAction()
					.For<SqlServerTransformationProvider>(db => i = 22)
					.For<PostgreSQLTransformationProvider>(db => i = 11)
					.Else(db => i = 33);
				Assert.AreEqual(i, 11);
			}
		}

		[Test]
		public void CanExecuteActionForExcludedProviders()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = -1;

				provider.ConditionalExecuteAction()
					.For<SqlServerCeTransformationProvider>(db => i = 44)
					.For<PostgreSQLTransformationProvider>(db => i = 55)
					.Else(db => i = 66);

				Assert.AreEqual(i, 66);
			}
		}

		[Test]
		public void CanExecuteActionForProvidersWithBaseClass()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			using (var provider = ProviderFactory.Create<SqlServerTransformationProvider>(cstring))
			{
				int i = -1;

				throw new NotImplementedException("Испраить тест");
				//provider.ConditionalExecuteAction()
				//    .For <BaseSqlServerTransformationProvider<SqlConnection>>(db => i = 77);
				Assert.AreEqual(77, i);

			}
		}

		[Test]
		public void CanExecuteActionForProviderByAlias()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			using (var provider = ProviderFactory.Create<PostgreSQLTransformationProvider>(cstring))
			{
				int i = 5;
				provider.ConditionalExecuteAction()
					.For("PostgreSQL", db => i = 21)
					.For("SqlServer", db => i = 23)
					.Else(db => i = 18);

				Assert.AreEqual(i, 21);
			}
		}

		[Test]
		public void ProviderTypeShouldBeValidated()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			using (var provider = ProviderFactory.Create<PostgreSQLTransformationProvider>(cstring))
			{
				Assert.Throws<RequirementNotCompliedException>(() =>
					new ConditionByProvider(provider).For<int>(null));

				Assert.Throws<RequirementNotCompliedException>(() =>
					new ConditionByProvider(provider).For("System.DateTime", null));
			}
		}
	}
}