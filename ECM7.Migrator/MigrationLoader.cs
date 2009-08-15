using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using System.Linq;

namespace ECM7.Migrator
{
	/// <summary>
	/// Handles inspecting code to find all of the Migrations in assemblies and reading
	/// other metadata such as the last revision, etc.
	/// </summary>
	public class MigrationLoader
	{
		private readonly ITransformationProvider provider;
		private readonly List<Type> migrationsTypes = new List<Type>();

		public MigrationLoader(ITransformationProvider provider, bool trace, params Assembly[] migrationAssembly)
		{
			this.provider = provider;
			AddMigrations(migrationAssembly);

			if (trace)
			{
				provider.Logger.Trace("Loaded migrations:");
				foreach (Type t in migrationsTypes)
				{
					provider.Logger.Trace("{0} {1}", GetMigrationVersion(t).ToString().PadLeft(5), StringUtils.ToHumanName(t.Name));
				}
			}
		}

		public void AddMigrations(params Assembly[] assemblies)
		{
			foreach (Assembly assembly in assemblies)
				if (assembly != null)
				{
					List<Type> collection = GetMigrationTypes(assembly);
					migrationsTypes.AddRange(collection);
				}
		}

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<Type> MigrationsTypes
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
				if (migrationsTypes.Count == 0)
					return 0;
				return GetMigrationVersion(migrationsTypes[migrationsTypes.Count - 1]);
			}
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		public void CheckForDuplicatedVersion()
		{
			List<long> versions = new List<long>();
			foreach (Type t in migrationsTypes)
			{
				long version = GetMigrationVersion(t);

				if (versions.Contains(version))
					throw new DuplicatedVersionException(version);

				versions.Add(version);
			}
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <returns>The migrations collection</returns>
		public static List<Type> GetMigrationTypes(Assembly asm)
		{
			List<Type> migrations = new List<Type>();
			foreach (Type t in asm.GetExportedTypes())
			{
				MigrationAttribute attrib =
					(MigrationAttribute)Attribute.GetCustomAttribute(t, typeof(MigrationAttribute));

				if (attrib != null && typeof(IMigration).IsAssignableFrom(t) && !attrib.Ignore)
				{
					migrations.Add(t);
				}
			}

			migrations.Sort(new MigrationTypeComparer(true));
			return migrations;
		}

		/// <summary>
		/// Returns the version of the migration
		/// <see cref="MigrationAttribute">MigrationAttribute</see>.
		/// </summary>
		/// <param name="t">Migration type.</param>
		/// <returns>Version number sepcified in the attribute</returns>
		public static long GetMigrationVersion(Type t)
		{
			MigrationAttribute attrib = (MigrationAttribute)
										Attribute.GetCustomAttribute(t, typeof(MigrationAttribute));

			return attrib.Version;
		}

		public List<long> GetAvailableMigrations()
		{
			migrationsTypes.Sort(new MigrationTypeComparer(true));
			return migrationsTypes.Select(type => GetMigrationVersion(type)).ToList();
		}

		public IMigration GetMigration(long version)
		{
			foreach (Type t in migrationsTypes)
				if (GetMigrationVersion(t) == version)
				{
					IMigration migration = (IMigration)Activator.CreateInstance(t);
					migration.Database = provider;
					return migration;
				}

			return null;
		}
	}
}
