using System;
using System.Collections.Generic;
using System.Reflection;

namespace ECM7.Migrator
{
	using ECM7.Migrator.Framework;

	public class ProviderFactory
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

		#region Create

		public static ITransformationProvider Create<TProvider>(string connectionString)
			where TProvider : ITransformationProvider
		{
			return Create(typeof(TProvider), connectionString);
		}

		public static ITransformationProvider Create(string providerName, string connectionString)
		{
			string providerTypeName = shortcuts.ContainsKey(providerName)
										? shortcuts[providerName]
										: providerName;
			Type providerType = Type.GetType(providerTypeName);
			Require.IsNotNull(providerType, "Не удалось загрузить диалект: {0}", providerName.Nvl("null"));
			return Create(providerType, connectionString);
		}

		public static ITransformationProvider Create(Type providerType, string connectionString)
		{
			ValidateProviderType(providerType);
			return Activator.CreateInstance(providerType, connectionString) as ITransformationProvider;
		}

		// todo: поменять валидацию типа
		/// <summary>
		/// Проверка, что:
		/// <para>- параметр providerType не равен null;</para>
		/// <para>- класс унаследован от Dialect;</para>
		/// <para>- класс имеет открытый конструктор без параметров;</para>
		/// </summary>
		/// <param name="providerType">Класс диалекта</param>
		internal static void ValidateProviderType(Type providerType)
		{
			Require.IsNotNull(providerType, "Не задан диалект");
			Require.That(providerType .IsSubclassOf(typeof(ITransformationProvider)), "Класс диалекта должен быть унаследован от Dialect");

			ConstructorInfo constructor = providerType.GetConstructor(Type.EmptyTypes);
			Require.IsNotNull(constructor, "Класс диалекта должен иметь открытый конструктор без параметров");
		}

		#endregion
	}
}
