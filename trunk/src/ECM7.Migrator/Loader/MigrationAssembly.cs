using ECM7.Migrator.Exceptions;

namespace ECM7.Migrator.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// ����� ��� ������ � ���������� � ������
	/// </summary>
	public class MigrationAssembly
	{
		/// <summary>
		/// ������ ����������� ����� ��������
		/// </summary>
		private readonly ReadOnlyCollection<MigrationInfo> migrationsTypes;

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public ReadOnlyCollection<MigrationInfo> MigrationsTypes
		{
			get { return migrationsTypes; }
		}

		/// <summary>
		/// ���� �������� ��� ������ ������
		/// </summary>
		private readonly string key;

		/// <summary>
		/// ���� �������� ��� ������ ������
		/// </summary>
		public string Key
		{
			get { return key; }
		}

		/// <summary>
		/// ������������ ��������� ������
		/// </summary>
		private readonly long lastVersion;

		/// <summary>
		/// ������������ ��������� ������
		/// </summary>
		public long LastVersion
		{
			get { return lastVersion; }
		}

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="asm">������ � ����������</param>
		public MigrationAssembly(Assembly asm)
		{
			Require.IsNotNull(asm, "�� ������ ������ � ����������");

			key = GetAssemblyKey(asm);

			var mt = GetMigrationInfoList(asm);
			var versions = mt.Select(info => info.Version).ToArray();

			CheckForDuplicatedVersion(versions);
			migrationsTypes = new ReadOnlyCollection<MigrationInfo>(mt);

			lastVersion = versions.IsEmpty() ? 0 : versions.Max();
		}

		public static MigrationAssembly Load(Assembly asm)
		{
			return new MigrationAssembly(asm);
		}

		/// <summary>
		/// ��������� ����� �������� ��� �������� ������
		/// </summary>
		private static string GetAssemblyKey(Assembly assembly)
		{
			MigrationAssemblyAttribute asmAttribute =
				assembly.GetCustomAttribute<MigrationAssemblyAttribute>();

			string assemblyKey = asmAttribute == null
				? string.Empty
				: asmAttribute.Key ?? string.Empty;

			MigratorLogManager.Log.DebugFormat("Migration key: {0}", assemblyKey);
			return assemblyKey;
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		private static List<MigrationInfo> GetMigrationInfoList(Assembly asm)
		{
			List<MigrationInfo> migrations = new List<MigrationInfo>();

			foreach (Type type in asm.GetExportedTypes())
			{
				MigrationAttribute attribute = type.GetCustomAttribute<MigrationAttribute>();

				if (attribute != null
					&& typeof(IMigration).IsAssignableFrom(type)
					&& !attribute.Ignore)
				{
					MigrationInfo mi = new MigrationInfo(type);
					migrations.Add(mi);
				}
			}

			migrations.Sort(new MigrationInfoComparer());

			// ����� � ��� ������ ����������� ��������
			StringBuilder logMessageBuilder = new StringBuilder("Loaded migrations:").AppendLine();

			foreach (MigrationInfo mi in migrations)
			{
				string msg = "{0} {1}".FormatWith(mi.Version.ToString().PadLeft(5), StringUtils.ToHumanName(mi.Type.Name));
				logMessageBuilder.AppendLine(msg);
			}

			MigratorLogManager.Log.DebugFormat(logMessageBuilder.ToString());


			return migrations;
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		public static void CheckForDuplicatedVersion(IEnumerable<long> migrationsTypes)
		{
			IEnumerable<long> list = migrationsTypes
				.GroupBy(v => v)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
				.ToList();

			if (list.Any())
			{
				throw new DuplicatedVersionException(list);
			}
		}

		public MigrationInfo GetMigrationInfo(long version)
		{
			var targetMigrationInfo = migrationsTypes
				.Where(info => info.Version == version)
				.ToList();

			Require.That(targetMigrationInfo.Any(), "�� ������� �������� ������ {0}", version);

			return targetMigrationInfo.First();
		}

		/// <summary>
		/// ������� �������� �� ������ ������
		/// </summary>
		/// <param name="migrationInfo">���������� � ��������</param>
		/// <param name="provider">��������� ���� ��� ��������� � �������� �������� ���������� ��������</param>
		public IMigration InstantiateMigration(MigrationInfo migrationInfo, ITransformationProvider provider)
		{
			Require.IsNotNull(provider, "�� ����� ��������� ����");
			Require.IsNotNull(migrationInfo.Type, "�� ����� ����� ��������");
			
			IMigration migration = (IMigration)Activator.CreateInstance(migrationInfo.Type);
			migration.Database = provider;
			return migration;
		}
	}
}