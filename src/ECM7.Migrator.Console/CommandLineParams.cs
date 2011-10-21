using System.Configuration;
using System.Linq;
using System.IO;
using ECM7.Migrator.Configuration;
using NDesk.Options;

namespace ECM7.Migrator.Console
{
	public sealed class CommandLineParams
	{
		private CommandLineParams(string[] args)
		{
			if (args.Length < 3)
			{
				mode = MigratorConsoleMode.Help;
			}
			else
			{
				#region обязательные параметры

				// provider
				config.Provider = args[0];

				// assembly
				string migrationsAssembly = args[2];

				// сборка с миграциями
				if (File.Exists(migrationsAssembly))
				{
					config.AssemblyFile = migrationsAssembly;
				}
				else
				{
					config.Assembly = migrationsAssembly;
				}

				string connectionString = args[1];

				// строка подключения
				if (ConfigurationManager.ConnectionStrings[connectionString] != null)
				{
					config.ConnectionStringName = connectionString;
				}
				else
				{
					config.ConnectionString = connectionString;
				}

				#endregion

				options = new OptionSet()
					.Add("v|ver|version=", "To specific version to migrate the database to (for migrae to latest version use -1)", (long v) => version = v)
					.Add("list", "Show list of available migrations", v => { if (v != null) { mode = MigratorConsoleMode.List; } })
					.Add("h|help|?", "Show help", v => { if (v != null) { mode = MigratorConsoleMode.Help; } });

				options.Parse(args.Skip(3));
			}
		}

		public static CommandLineParams Parse(string[] args)
		{
			return new CommandLineParams(args);
		}
		
		public readonly OptionSet options;

		public readonly MigratorConfiguration config = new MigratorConfiguration();

		public MigratorConsoleMode mode = MigratorConsoleMode.Migrate;

		public long version = -1;
 
	}
}
