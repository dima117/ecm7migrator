using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using System.Linq;

namespace ECM7.Migrator.Loader
{
	/// <summary>
	/// Handles inspecting code to find all of the Migrations in assemblies and reading
	/// other metadata such as the last revision, etc.
	/// </summary>
	public class MigrationLoader
	{
		private readonly ITransformationProvider provider;
		private readonly List<MigrationInfo> migrationsTypes = new List<MigrationInfo>();

		public MigrationLoader(ITransformationProvider provider, bool trace, params Assembly[] migrationAssemblies)
		{
			this.provider = provider;
			AddMigrations(migrationAssemblies);

			if (trace)
			{
				provider.Logger.Trace("Loaded migrations:");
				foreach (var m in migrationsTypes)
					provider.Logger.Trace("{0} {1}", m.Version.ToString().PadLeft(5), StringUtils.ToHumanName(m.Type.Name));
			}
		}

		public void AddMigrations(params Assembly[] assemblies)
		{
			foreach (Assembly assembly in assemblies)
				if (assembly != null)
				{
					List<MigrationInfo> collection = GetMigrationInfoList(assembly);
					migrationsTypes.AddRange(collection);
				}
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
				return migrationsTypes.Count == 0 ? 0 
					: migrationsTypes.Select(info => info.Version).Max();
			}
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		public void CheckForDuplicatedVersion()
		{
			HashSet<long> versions = new HashSet<long>();
			
			foreach (var info in migrationsTypes)
			{
				if (versions.Contains(info.Version))
					throw new DuplicatedVersionException(info.Version);

				versions.Add(info.Version);
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
				MigrationAttribute attribute = Attribute.GetCustomAttribute(
					type, typeof(MigrationAttribute)) as MigrationAttribute;

				if (attribute != null && typeof(IMigration).IsAssignableFrom(type) && !attribute.Ignore)
				{
					migrations.Add(new MigrationInfo(type));
				}
			}

			migrations.Sort(new MigrationInfoComparer(true));
			return migrations;
		}

		public List<long> GetAvailableMigrations()
		{
			migrationsTypes.Sort(new MigrationInfoComparer(true));
			return migrationsTypes.Select(mInfo => mInfo.Version).ToList();
		}

		public IMigration GetMigration(long version)
		{
			var list = migrationsTypes.Where(info => info.Version == version).ToList();

			if (list.Count == 0) return null;

			IMigration migration = (IMigration)Activator.CreateInstance(list[0].Type);
			migration.Database = provider;
			return migration;
		}
	}
}