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

	/// <summary>
	/// Console application
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Выполнение программы
		/// </summary>
		/// <param name="args">Аргументы командной строки</param>
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				MigratorConsoleMode mode = GetConsoleMode(args);
				MigratorConfiguration config = GetConfig(args);

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
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
				PrintUsage();
				return -1;
			}
		}

		#region parse arguments

		/// <summary>
		/// Определение нужной версии БД
		/// </summary>
		/// <param name="args">АРгументы командной строки</param>
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
		/// Получение конфигурации мигратора из набора аргументов консоли
		/// </summary>
		/// <param name="args">Список аргументов</param>
		private static MigratorConfiguration GetConfig(string[] args)
		{
			Require.That(args.Length >= 3, "Required parameters: dialect, connection string, migration assembly");

			// получаем значения первых трех аргументов
			string dialect = args[0];
			string connectionString = args[1];
			string migrationsAssembly = args[2];

			MigratorConfiguration config = new MigratorConfiguration { Dialect = dialect };

			// сборка с миграциями
			if (File.Exists(migrationsAssembly))
			{
				config.AssemblyFile = migrationsAssembly;
			}
			else
			{
				config.Assembly = migrationsAssembly;
			}

			// строка подключения
			if (ConfigurationManager.ConnectionStrings[connectionString] != null)
			{
				config.ConnectionStringName = connectionString;
			}
			else
			{
				config.ConnectionString = connectionString;
			}

			return config;
		}

		/// <summary>
		/// Определить режим работы программы по параметрам командной строки
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
			Migrator migrator = MigratorFactory.CreateMigrator(config);

			migrator.Migrate(migrateTo);
		}

		/// <summary>
		/// Выводит текущий список миграций
		/// </summary>
		public static void List(IMigratorConfiguration config)
		{
			Migrator mig = MigratorFactory.CreateMigrator(config);
			List<long> appliedMigrations = mig.AppliedMigrations;

			Console.WriteLine("Available migrations:");
			foreach (var info in mig.MigrationsTypes)
			{
				long v = info.Version;
				Console.WriteLine(
					"{0} {1} {2}",
					appliedMigrations.Contains(v) ? "=>" : "  ",
					v.ToString().PadLeft(3),
					StringUtils.ToHumanName(info.Type.Name));
			}
		}

		/// <summary>
		/// Show usage information and help.
		/// </summary>
		public static void PrintUsage()
		{
			const int tab = 17;

			Version ver = Assembly.GetExecutingAssembly().GetName().Version;

			Console.WriteLine("Database migrator - v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			Console.WriteLine();
			Console.WriteLine("usage:\nECM7.Migrator.Console.exe dialect connectionString migrationsAssembly [options]");
			Console.WriteLine();
			Console.WriteLine("\t{0} {1}", "dialect".PadRight(tab), "Full name of dialect type (include assembly name)");
			Console.WriteLine("\t{0} {1}", "connectionString".PadRight(tab), "Connection string to the database");
			Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(tab), "Path to the assembly containing the migrations");
			Console.WriteLine("Options:");
			Console.WriteLine(
				"\t-{0}{1}",
				"version:NUM".PadRight(tab),
				"To specific version to migrate the database to (for migrae to latest version use -1)");
			Console.WriteLine("\t-{0}{1}", "list".PadRight(tab), "List migrations");
			Console.WriteLine("\t-{0}{1}", "help".PadRight(tab), "Show help");
			Console.WriteLine();
		}
	}
}
