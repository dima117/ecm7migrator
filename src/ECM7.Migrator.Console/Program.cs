namespace ECM7.Migrator.Console
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using ECM7.Migrator.Configuration;
	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Logging;

	using log4net.Appender;
	using log4net.Layout;

	/// <summary>
	/// Console application
	/// </summary>
	public class Program
	{
		/// <summary>
		/// ¬ыполнение программы
		/// </summary>
		/// <param name="args">јргументы командной строки</param>
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				WriteHeader();

				MigratorConsoleMode mode = GetConsoleMode(args);
				MigratorConfiguration config = GetConfig(args);

				ConfigureLogging();

				switch (mode)
				{
					case MigratorConsoleMode.Migrate:
						long migrateTo = GetDestinationVersion(args);
						Migrate(config, migrateTo);
						break;
					case MigratorConsoleMode.List:
						List(config);
						break;
					case MigratorConsoleMode.Help:
						PrintUsage();
						break;
				}

				return 0;
			}
			catch (Exception exception)
			{
				for (Exception ex = exception; ex != null; ex = ex.InnerException)
				{
					Console.WriteLine(ex.Message);
				}

				Console.WriteLine(exception.StackTrace);

				PrintUsage();
				return -1;
			}
		}

		private static void WriteHeader()
		{
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;
			Console.WriteLine("Database migrator - v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			Console.WriteLine();
		}

		private static void ConfigureLogging()
		{
			PatternLayout layout = new PatternLayout
				{
					ConversionPattern = "%message%newline"
				};
			layout.ActivateOptions();

			ConsoleAppender appender = new ConsoleAppender
				{
					Name = "ecm7migrator-console-appender",
					Layout = layout
				};

			appender.ActivateOptions();

			MigratorLogManager.SetLevel("ALL");
			MigratorLogManager.AddAppender(appender);
		}

		#region parse arguments

		/// <summary>
		/// ќпределение нужной версии базы данных
		/// </summary>
		/// <param name="args">јргументы командной строки</param>
		private static long GetDestinationVersion(IEnumerable<string> args)
		{
			long version = -1;

			Regex regex = new Regex(@"^\s*-ver(sion)?:(?'ver'-1|\d+)\s*$", RegexOptions.IgnoreCase);

			foreach (string param in args)
			{
				if (regex.IsMatch(param))
				{
					Match match = regex.Match(param);
					string strVersion = match.Groups["ver"].Value;
					version = long.Parse(strVersion);
				}
			}

			return version;
		}

		/// <summary>
		/// ѕолучить ключ из параметров командной строки
		/// </summary>
		/// <param name="args">ѕараметры командной строки</param>
		private static string GetKey(IEnumerable<string> args)
		{
			// TODO: прикрутить какую-нибудь библиотеку дл€ разбора параметров командной строки
			string key = string.Empty;

			// TODO: учесть в ключах строки с пробелами, если есть кавычки
			Regex regex = new Regex(@"^\s*-key:(?'key'\.+)\s*$", RegexOptions.IgnoreCase);

			foreach (string param in args)
			{
				if (regex.IsMatch(param))
				{
					Match match = regex.Match(param);
					key = match.Groups["key"].Value;
				}
			}

			return key;
		}

		/// <summary>
		/// ѕолучение конфигурации мигратора из набора аргументов консоли
		/// </summary>
		/// <param name="args">—писок аргументов</param>
		private static MigratorConfiguration GetConfig(string[] args)
		{
			Require.That(args.Length >= 3, "Parameters: provider, connection string, migration assembly");

			// получаем значени€ первых трех аргументов
			string provider = args[0];
			string connectionString = args[1];
			string migrationsAssembly = args[2];
			string key = GetKey(args);

			MigratorConfiguration config = new MigratorConfiguration { Provider = provider };

			// сборка с миграци€ми
			if (File.Exists(migrationsAssembly))
			{
				config.AssemblyFile = migrationsAssembly;
			}
			else
			{
				config.Assembly = migrationsAssembly;
			}

			// строка подключени€
			if (ConfigurationManager.ConnectionStrings[connectionString] != null)
			{
				config.ConnectionStringName = connectionString;
			}
			else
			{
				config.ConnectionString = connectionString;
			}

			config.Key = key;

			return config;
		}

		/// <summary>
		/// ќпределить режим работы программы по параметрам командной строки
		/// </summary>
		private static MigratorConsoleMode GetConsoleMode(IEnumerable<string> args)
		{
			MigratorConsoleMode mode = MigratorConsoleMode.Migrate;

			foreach (string param in args)
			{
				switch (param.Trim().ToLower())
				{
					case "-list":
					case "/list":
						mode = MigratorConsoleMode.List;
						break;
					case "-help":
					case "/help":
					case "-?":
					case "/?":
						mode = MigratorConsoleMode.Help;
						break;
				}
			}

			return mode;
		}

		#endregion

		/// <summary>
		/// Runs the migrations.
		/// </summary>
		public static void Migrate(IMigratorConfiguration config, long migrateTo)
		{
			using (Migrator migrator = MigratorFactory.CreateMigrator(config))
			{
				migrator.Migrate(migrateTo);
			}
		}

		/// <summary>
		/// ¬ыводит текущий список миграций
		/// </summary>
		public static void List(IMigratorConfiguration config)
		{
			using (Migrator mig = MigratorFactory.CreateMigrator(config))
			{
				IList<long> appliedMigrations = mig.GetAppliedMigrations();

				Console.WriteLine("Available migrations:");
				foreach (var info in mig.AvailableMigrations)
				{
					long v = info.Version;
					Console.WriteLine(
						"{0} {1} {2}",
						appliedMigrations.Contains(v) ? "=>" : "  ",
						v.ToString().PadLeft(3),
						StringUtils.ToHumanName(info.Type.Name));
				}
			}
		}

		/// <summary>
		/// Show usage information and help.
		/// </summary>
		public static void PrintUsage()
		{
			const int TAB = 17;

			Console.WriteLine("usage:\nECM7.Migrator.Console.exe provider connectionString migrationsAssembly [options]");
			Console.WriteLine();
			Console.WriteLine("\t{0} {1}", "provider".PadRight(TAB), "Full name of provider type (include assembly name)");
			Console.WriteLine("\t{0} {1}", "connectionString".PadRight(TAB), "Connection string to the database");
			Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(TAB), "Path to the assembly containing the migrations");
			Console.WriteLine("Options:");
			Console.WriteLine(
				"\t-{0}{1}",
				"version:NUM".PadRight(TAB),
				"To specific version to migrate the database to (for migrae to latest version use -1)");
			Console.WriteLine("\t-{0}{1}", "list".PadRight(TAB), "List migrations");
			Console.WriteLine("\t-{0}{1}", "help".PadRight(TAB), "Show help");
			Console.WriteLine();
		}
	}
}
