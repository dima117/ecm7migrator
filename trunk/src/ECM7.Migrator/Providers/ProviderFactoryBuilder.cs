namespace ECM7.Migrator.Providers
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using ECM7.Migrator.Framework;

	public class ProviderFactoryBuilder
	{
		#region Shortcuts

		// todo: поменять шорткаты
		private static readonly Dictionary<string, string> shortcuts = new Dictionary<string, string>
		{
			{ "SqlServer",		"ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer" },
			{ "SqlServerCe",	"ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer" },
			{ "Oracle",			"ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle" },
			{ "MySql",			"ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql" },
			{ "SQLite",			"ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite" },
			{ "PostgreSQL",		"ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL" }
		};

		#endregion

		#region CreateProviderFactory

		public static TProviderFactory CreateProviderFactory<TProviderFactory>()
			where TProviderFactory : ITransformationProviderFactory<ITransformationProvider>, new()
		{
			return new TProviderFactory();
		}

		public static ITransformationProviderFactory<ITransformationProvider> CreateProviderFactory(string providerFactoryName)
		{
			// todo: добавить обработку шорткатов
			//string providerTypeName = shortcuts.ContainsKey(providerName)
			//                            ? shortcuts[providerName]
			//                            : providerName;

			Type providerFactoryType = Type.GetType(providerFactoryName);
			Require.IsNotNull(providerFactoryType, "Не удалось загрузить класс фабрики провайдеров: {0}", providerFactoryName.Nvl("null"));
			return CreateProviderFactory(providerFactoryType);
		}

		public static ITransformationProviderFactory<ITransformationProvider> CreateProviderFactory(Type providerFactoryType)
		{
			// todo: добавить валидацию типа фабрики провайдеров
			// ValidateProviderType(providerType);
			return Activator.CreateInstance(providerFactoryType) as ITransformationProviderFactory<ITransformationProvider>;
		}

		#endregion
	}
}
