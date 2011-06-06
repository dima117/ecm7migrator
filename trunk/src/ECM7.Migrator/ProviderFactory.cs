using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Providers;

namespace ECM7.Migrator
{
	/// <summary>
	/// ������� - ������ ������������� ����������� ����
	/// </summary>
	public class ProviderFactory
	{
		#region Shortcuts

		private static readonly Dictionary<string, string> shortcuts = new Dictionary<string, string>
		{
			{ "SqlServer",		"ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer" },
			{ "SqlServer2005",	"ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect, ECM7.Migrator.Providers.SqlServer" },
			{ "SqlServerCe",	"ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer" },
			{ "Oracle",			"ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle" },
			{ "MySql",			"ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql" },
			{ "SQLite",			"ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite" },
			{ "PostgreSQL",		"ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL" }
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

			Require.IsNotNull(dialects[type], "�� ������� ���������������� �������");
			return dialects[type];
		}

		public static Dialect GetDialect(Type dialectType)
		{
			ValidateDialectType(dialectType);

			if (!dialects.ContainsKey(dialectType) || dialects[dialectType] == null)
				dialects[dialectType] = Activator.CreateInstance(dialectType, null) as Dialect;

			Require.IsNotNull(dialects[dialectType], "�� ������� ���������������� �������");
			return dialects[dialectType];
		}

		/// <summary>
		/// ��������, ���:
		/// <para>- �������� dialectType �� ����� null;</para>
		/// <para>- ����� ����������� �� Dialect;</para>
		/// <para>- ����� ����� �������� ����������� ��� ����������;</para>
		/// </summary>
		/// <param name="dialectType">����� ��������</param>
		internal static void ValidateDialectType(Type dialectType)
		{
			Require.IsNotNull(dialectType, "�� ����� �������");
			Require.That(dialectType.IsSubclassOf(typeof(Dialect)), "����� �������� ������ ���� ����������� �� Dialect");

			ConstructorInfo constructor = dialectType.GetConstructor(Type.EmptyTypes);
			Require.IsNotNull(constructor, "����� �������� ������ ����� �������� ����������� ��� ����������");
		}

		#endregion

		#region Create

		public static TransformationProvider Create<TDialect>(
			Type dialectType, string connectionString)
			where TDialect : Dialect, new()
		{
			return GetDialect<TDialect>().NewProviderForDialect(connectionString);
		}

		public static TransformationProvider Create(Type dialectType, string connectionString)
		{
			return GetDialect(dialectType).NewProviderForDialect(connectionString);
		}

		public static TransformationProvider Create(string dialectName, string connectionString)
		{
			string dialectTypeName = shortcuts.ContainsKey(dialectName)
			                         	? shortcuts[dialectName]
			                         	: dialectName;
			Type dialectType = Type.GetType(dialectTypeName);
			Require.IsNotNull(dialectType, "�� ������� ��������� �������: {0}", dialectName.Nvl("null"));
			return Create(dialectType, connectionString);
		}

		#endregion
	}
}
