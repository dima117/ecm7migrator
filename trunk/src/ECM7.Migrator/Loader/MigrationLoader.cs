namespace ECM7.Migrator.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ECM7.Migrator.Framework;

	/// <summary>
	/// ����� ��� ������ � ���������� � ������
	/// </summary>
	public class MigrationLoader
	{
		/// <summary>
		/// ������ ����������� ����� ��������
		/// </summary>
		private readonly List<MigrationInfo> migrationsTypes;

		/// <summary>
		/// ���� ��� ���������� ��������
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="key">���� ��� ���������� ��������</param>
		/// <param name="logger">������ ��� ������ ��������� �����������</param>
		/// <param name="migrationAssemblies">������ ������ � ����������</param>
		public MigrationLoader(string key, ILogger logger, params Assembly[] migrationAssemblies)
		{
			this.Key = key;

			this.migrationsTypes = LoadMigrations(key, migrationAssemblies);

			if (logger != null)
			{
				logger.Trace("Loaded migrations:");
				foreach (var m in migrationsTypes)
				{
					logger.Trace("{0} {1}", m.Version.ToString().PadLeft(5), StringUtils.ToHumanName(m.Type.Name));
				}
			}

			CheckForDuplicatedVersion(this.migrationsTypes);
		}

		/// <summary>
		/// ��������� ��������� ��������
		/// </summary>
		/// <param name="key">���� ��� ���������� ����������� ��������</param>
		/// <param name="assemblies">������ ������ � ����������</param>
		private static List<MigrationInfo> LoadMigrations(string key, params Assembly[] assemblies)
		{
			List<MigrationInfo> migrationsTypes = new List<MigrationInfo>();

			foreach (Assembly assembly in assemblies)
			{
				if (assembly != null && AssemblyHasTargetKey(assembly, key))
				{
					List<MigrationInfo> collection = GetMigrationInfoList(assembly);
					migrationsTypes.AddRange(collection);
				}
			}

			migrationsTypes.Sort(new MigrationInfoComparer(true));

			return migrationsTypes;
		}

		/// <summary>
		/// ��������, �������� �� ������ � ���������� �������� ������
		/// </summary>
		/// <param name="assembly">����������� ������</param>
		/// <param name="key">����</param>
		private static bool AssemblyHasTargetKey(Assembly assembly, string key)
		{
			MigrationAssemblyAttribute asmAttribute =
				assembly.GetCustomAttribute<MigrationAssemblyAttribute>();

			string targetKey = key ?? string.Empty;

			string assemblyKey = asmAttribute == null
				? string.Empty
				: asmAttribute.Key ?? string.Empty;

			return targetKey == assemblyKey;
		}

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<MigrationInfo> MigrationsTypes
		{
			get { return migrationsTypes; }
		}

		/// <summary>
		/// Returns the last version of the migrations.
		/// </summary>
		public long LastVersion
		{
			get
			{
				return migrationsTypes.IsEmpty() ? 0
					: migrationsTypes.Select(info => info.Version).Max();
			}
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		public static void CheckForDuplicatedVersion(List<MigrationInfo> migrationsTypes)
		{
			IEnumerable<long> list = migrationsTypes
				.GroupBy(v => v.Version)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key);
			
			if (!list.IsEmpty())
			{
				throw new DuplicatedVersionException(list);
			}
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <returns>The migrations collection</returns>
		public static List<MigrationInfo> GetMigrationInfoList(Assembly asm)
		{
			List<MigrationInfo> migrations = new List<MigrationInfo>();

			foreach (Type type in asm.GetExportedTypes())
			{
				MigrationAttribute attribute = type.GetCustomAttribute<MigrationAttribute>();

				if (attribute != null
					&& typeof(IMigration).IsAssignableFrom(type)
					&& !attribute.Ignore)
				{
					migrations.Add(new MigrationInfo(type));
				}
			}

			migrations.Sort(new MigrationInfoComparer(true));
			return migrations;
		}

		/// <summary>
		/// �������� �������� �� ������ ������
		/// </summary>
		/// <param name="version">������ ��������</param>
		/// <param name="provider">��������� ���� ��� ��������� � �������� �������� ���������� ��������</param>
		public IMigration GetMigration(long version, ITransformationProvider provider)
		{
			Require.IsNotNull(provider, "�� ����� ��������� ����");

			var list = migrationsTypes.Where(info => info.Version == version).ToList();

			if (list.Count == 0)
			{
				return null;
			}

			IMigration migration = (IMigration)Activator.CreateInstance(list[0].Type);
			migration.Database = provider;
			return migration;
		}
	}
}