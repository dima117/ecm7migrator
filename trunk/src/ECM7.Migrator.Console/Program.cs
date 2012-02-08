namespace ECM7.Migrator.Console
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
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
		/// Выполнение программы
		/// </summary>
		/// <param name="args">Аргументы командной строки</param>
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				WriteHeader();

				var parameters = CommandLineParams.Parse(args);

				ConfigureLogging();

				switch (parameters.mode)
				{
					case MigratorConsoleMode.Migrate:
						Migrate(parameters);
						break;
					case MigratorConsoleMode.List:
						List(parameters);
						break;
					case MigratorConsoleMode.Help:
						PrintUsage(parameters);
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

				Console.WriteLine("Try `ECM7.Migrator.Console --help' for more information");

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

		/// <summary>
		/// Runs the migrations.
		/// </summary>
		public static void Migrate(CommandLineParams parameters)
		{
			using (Migrator migrator = MigratorFactory.CreateMigrator(parameters.config))
			{
				migrator.Migrate(parameters.version);
			}
		}

		/// <summary>
		/// Выводит текущий список миграций
		/// </summary>
		public static void List(CommandLineParams parameters)
		{
			using (Migrator mig = MigratorFactory.CreateMigrator(parameters.config))
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
		public static void PrintUsage(CommandLineParams parameters)
		{
			const int TAB = 17;

			Console.WriteLine("usage:\nECM7.Migrator.Console.exe provider connectionString migrationsAssembly [options]");
			Console.WriteLine();
			Console.WriteLine("\t{0} {1}", "provider".PadRight(TAB), "Full name of provider type (include assembly name)");
			Console.WriteLine("\t{0} {1}", "connectionString".PadRight(TAB), "Connection string to the database");
			Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(TAB), "Path to the assembly containing the migrations");
			Console.WriteLine("Options:");
			parameters.options.WriteOptionDescriptions(Console.Out);
			Console.WriteLine();
		}
	}
}
