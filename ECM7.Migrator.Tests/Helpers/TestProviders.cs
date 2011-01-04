using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Tests.Helpers
{
	public static class TestProviders
	{
		#region const

		private const string MYSQL_DIALECT = "ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql";
		private const string ORACLE_DIALECT = "ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle";
		private const string SQLITE_DIALECT = "ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite";
		private const string POSTGRE_SQL_DIALECT = "ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL";

		private const string SQL_SERVER_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_2005_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_CE_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer";

		#endregion

		public static ITransformationProvider SqlServer
		{
			get
			{
				return ProviderFactory.Create(
					SQL_SERVER_DIALECT, ConfigurationManager.AppSettings["SqlServerConnectionString"]);
			}
		}

		public static ITransformationProvider SqlServer2005
		{
			get
			{
				return ProviderFactory.Create(
					SQL_SERVER_2005_DIALECT, ConfigurationManager.AppSettings["SqlServerConnectionString"]);
			}
		}

		public static ITransformationProvider SqlServerCe
		{
			get
			{
				return ProviderFactory.Create(
					SQL_SERVER_CE_DIALECT, ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);
			}
		}

		public static ITransformationProvider MySql
		{
			get
			{
				return ProviderFactory.Create(
					MYSQL_DIALECT, ConfigurationManager.AppSettings["MySqlConnectionString"]);
			}
		}

		public static ITransformationProvider PostgreSQL
		{
			get
			{
				return ProviderFactory.Create(
					POSTGRE_SQL_DIALECT, ConfigurationManager.AppSettings["NpgsqlConnectionString"]);
			}
		}

		public static ITransformationProvider SQLite
		{
			get
			{
				return ProviderFactory.Create(
					SQLITE_DIALECT, ConfigurationManager.AppSettings["SQLiteConnectionString"]);
			}
		}

		public static ITransformationProvider Oracle
		{
			get
			{
				return ProviderFactory.Create(
					ORACLE_DIALECT, ConfigurationManager.AppSettings["OracleConnectionString"]);
			}
		}
	}
}
