using System;
using System.Collections.Generic;
using System.Reflection;

namespace ECM7.Migrator
{
	using ECM7.Migrator.Framework;

	public class ProviderFactory
	{
		#region Shortcuts

		// todo: �������� ��������
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
			Require.IsNotNull(providerType, "�� ������� ��������� �������: {0}", providerName.Nvl("null"));
			return Create(providerType, connectionString);
		}

		public static ITransformationProvider Create(Type providerType, string connectionString)
		{
			ValidateProviderType(providerType);
			return Activator.CreateInstance(providerType, connectionString) as ITransformationProvider;
		}

		// todo: �������� ��������� ����
		/// <summary>
		/// ��������, ���:
		/// <para>- �������� providerType �� ����� null;</para>
		/// <para>- ����� ����������� �� Dialect;</para>
		/// <para>- ����� ����� �������� ����������� ��� ����������;</para>
		/// </summary>
		/// <param name="providerType">����� ��������</param>
		internal static void ValidateProviderType(Type providerType)
		{
			Require.IsNotNull(providerType, "�� ����� �������");
			Require.That(providerType .IsSubclassOf(typeof(ITransformationProvider)), "����� �������� ������ ���� ����������� �� Dialect");

			ConstructorInfo constructor = providerType.GetConstructor(Type.EmptyTypes);
			Require.IsNotNull(constructor, "����� �������� ������ ����� �������� ����������� ��� ����������");
		}

		#endregion
	}
}
