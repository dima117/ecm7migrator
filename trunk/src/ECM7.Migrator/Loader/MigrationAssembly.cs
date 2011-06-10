namespace ECM7.Migrator.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;
	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Loggers;

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
		/// <param name="logger">������ ��� ������ ��������� �����������</param>
		public MigrationAssembly(Assembly asm, ILogger logger)
		{
			Require.IsNotNull(asm, "�� ������ ������ � ����������");
			Require.IsNotNull(logger, "�� ��������������� ������");

			this.key = GetAssemblyKey(asm, logger);

			var mt = GetMigrationInfoList(asm, logger);
			var versions = mt.Select(info => info.Version);

			CheckForDuplicatedVersion(versions);
			this.migrationsTypes = new ReadOnlyCollection<MigrationInfo>(mt);

			this.lastVersion = versions.IsEmpty() ? 0 : versions.Max();
		}

		public static MigrationAssembly Load(Assembly asm, ILogger logger)
		{
			return new MigrationAssembly(asm, logger);
		}

		/// <summary>
		/// ��������� ����� �������� ��� �������� ������
		/// </summary>
		private static string GetAssemblyKey(Assembly assembly, ILogger logger)
		{
			MigrationAssemblyAttribute asmAttribute =
				assembly.GetCustomAttribute<MigrationAssemblyAttribute>();

			string assemblyKey = asmAttribute == null
				? string.Empty
				: asmAttribute.Key ?? string.Empty;

			logger.Trace("Migration key: {0}", assemblyKey);
			return assemblyKey;
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <param name="logger">������</param>
		private static List<MigrationInfo> GetMigrationInfoList(Assembly asm, ILogger logger)
		{
			List<MigrationInfo> migrations = new List<MigrationInfo>();
			logger.Trace("Loaded migrations:");

			foreach (Type type in asm.GetExportedTypes())
			{
				MigrationAttribute attribute = type.GetCustomAttribute<MigrationAttribute>();

				if (attribute != null
					&& typeof(IMigration).IsAssignableFrom(type)
					&& !attribute.Ignore)
				{
					MigrationInfo mi = new MigrationInfo(type);
					migrations.Add(mi);
					logger.Trace("{0} {1}", mi.Version.ToString().PadLeft(5), StringUtils.ToHumanName(mi.Type.Name));
				}
			}

			migrations.Sort(new MigrationInfoComparer(true));
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
				.Select(x => x.Key);

			if (!list.IsEmpty())
			{
				throw new DuplicatedVersionException(list);
			}
		}

		/// <summary>
		/// ������� �������� �� ������ ������
		/// </summary>
		/// <param name="version">������ ��������</param>
		/// <param name="provider">��������� ���� ��� ��������� � �������� �������� ���������� ��������</param>
		public IMigration InstantiateMigration(long version, ITransformationProvider provider)
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