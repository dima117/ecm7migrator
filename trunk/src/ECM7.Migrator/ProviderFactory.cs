#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Providers;

namespace ECM7.Migrator
{
	/// <summary>
	/// Handles loading Provider implementations
	/// </summary>
	public class ProviderFactory
	{
		#region Shortcuts

		private static readonly Dictionary<string, string> shortcuts = new Dictionary<string, string>
		{
			{"SqlServer",		"ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"},
			{"SqlServer2005",	"ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect, ECM7.Migrator.Providers.SqlServer"},
			{"SqlServerCe",		"ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer"},
			{"Oracle",			"ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle"},
			{"MySql",			"ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql"},
			{"SQLite",			"ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite"},
			{"PostgreSQL",		"ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL"}
		};

		#endregion

		#region GetDialect

		private static readonly Dictionary<Type, Dialect> dialects = new Dictionary<Type, Dialect>();

		public static Dialect GetDialect<TDialect>()
			where TDialect : Dialect, new()
		{
			Type type = typeof(TDialect);
			if (!dialects.ContainsKey(type) || dialects[type] == null)
				dialects[type] = new TDialect();

			Require.IsNotNull(dialects[type], "Ќе удалось инициализировать диалект");
			return dialects[type];
		}

		public static Dialect GetDialect(Type dialectType)
		{
			ValidateDialectType(dialectType);

			if (!dialects.ContainsKey(dialectType) || dialects[dialectType] == null)
				dialects[dialectType] = Activator.CreateInstance(dialectType, null) as Dialect;

			Require.IsNotNull(dialects[dialectType], "Ќе удалось инициализировать диалект");
			return dialects[dialectType];
		}

		/// <summary>
		/// ѕроверка, что:
		/// <para>- параметр dialectType не равен null;</para>
		/// <para>- класс унаследован от Dialect;</para>
		/// <para>- класс имеет открытый конструктор без параметров;</para>
		/// </summary>
		/// <param name="dialectType"> ласс диалекта</param>
		internal static void ValidateDialectType(Type dialectType)
		{
			Require.IsNotNull(dialectType, "Ќе задан диалект");
			Require.That(dialectType.IsSubclassOf(typeof(Dialect)), " ласс диалекта должен быть унаследован от Dialect");

			ConstructorInfo constructor = dialectType.GetConstructor(Type.EmptyTypes);
			Require.IsNotNull(constructor, " ласс диалекта должен иметь открытый конструктор без параметров");
		}

		#endregion

		#region Create

		public static TransformationProvider Create<TDialect>(
			Type dialectType, string connectionString, string key)
			where TDialect : Dialect, new()
		{
			return GetDialect<TDialect>().NewProviderForDialect(connectionString, key);
		}

		public static TransformationProvider Create(Type dialectType, string connectionString, string key)
		{
			return GetDialect(dialectType).NewProviderForDialect(connectionString, key);
		}

		public static TransformationProvider Create(string dialectName, string connectionString, string key)
		{
			string dialectTypeName = shortcuts.ContainsKey(dialectName)
			                         	? shortcuts[dialectName]
			                         	: dialectName;
			Type dialectType = Type.GetType(dialectTypeName);
			Require.IsNotNull(dialectType, "Ќе удалось загрузить диалект: {0}".FormatWith(dialectName.Nvl("null")));
			return Create(dialectType, connectionString, key);
		}

		#endregion

	}
}
