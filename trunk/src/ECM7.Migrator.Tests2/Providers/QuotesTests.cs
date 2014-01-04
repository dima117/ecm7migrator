using System.Configuration;
using System.Data;
using System.Linq;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Logging;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.SqlServer;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace ECM7.Migrator.Tests2.Providers
{
	[TestFixture]
	public class QuotesTests
	{
		private ITransformationProvider CreateProvider(bool quotesNeeded)
		{
			string constr = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			var provider = ProviderFactory.Create<SqlServerTransformationProvider>(constr);
			provider.NeedQuotesForNames = quotesNeeded;

			return provider;
		}

		[Test]
		public void CanCreateTableWithQuotes()
		{
			var target = new MemoryTarget { Name = MigratorLogManager.LOGGER_NAME, Layout = new SimpleLayout("${message}") };
			MigratorLogManager.SetNLogTarget(target);

			var provider = CreateProvider(true);
			provider.AddTable("quoted", new Column("id", DbType.Int32));

			var sql = target.Logs.First();
			provider.RemoveTable("quoted");

			Assert.AreEqual("CREATE TABLE [quoted] ([id] INT)", sql);
		}

		[Test]
		public void CanCreateTableWithoutQuotes()
		{
			var target = new MemoryTarget { Name = MigratorLogManager.LOGGER_NAME, Layout = new SimpleLayout("${message}") };
			MigratorLogManager.SetNLogTarget(target);

			var provider = CreateProvider(false);
			provider.AddTable("unquoted", new Column("id", DbType.Int32));

			var sql = target.Logs.First();
			
			provider.RemoveTable("unquoted");

			Assert.AreEqual("CREATE TABLE unquoted (id INT)", sql);
		}
	}
}
