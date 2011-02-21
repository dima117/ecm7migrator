#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion

using System;
using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Tools;
using System.Collections.Generic;

namespace ECM7.Migrator.Console
{
	/// <summary>
	/// Commande line utility to run the migrations
	/// </summary>
	/// </remarks>
	public class MigratorConsole
	{
		private string dialect;
		private string connectionString;
		private string migrationsAssembly;
		private bool list;
		private bool trace;
		private bool dryrun;
		private string dumpTo;
		private long migrateTo = -1;
		private readonly string[] args;

		/// <summary>
		/// Builds a new console
		/// </summary>
		/// <param name="argv">Command line arguments</param>
		public MigratorConsole(string[] argv)
		{
			args = argv;
		}

		/// <summary>
		/// Run the migrator's console
		/// </summary>
		/// <returns>-1 if error, else 0</returns>
		public int Run()
		{
			if (!ParseArguments(args))
				return -1;

			try
			{
				ParseArguments(args);

				if (list)
					List();
				else if (dumpTo != null)
					Dump();
				else
					Migrate();
			}
			catch (ArgumentException aex)
			{
				System.Console.WriteLine("Invalid argument '{0}' : {1}", aex.ParamName, aex.Message);
				System.Console.WriteLine();
				return -1;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex);
				return -1;
			}
			return 0;
		}

		/// <summary>
		/// Runs the migrations.
		/// </summary>
		public void Migrate()
		{
			CheckArguments();

			Migrator mig = GetMigrator();
			if (mig.DryRun)
				mig.Logger.Log("********** Dry run! Not actually applying changes. **********");

			if (migrateTo == -1)
				mig.MigrateToLastVersion();
			else
				mig.MigrateTo(migrateTo);
		}

		/// <summary>
		/// List migrations.
		/// </summary>
		public void List()
		{
			CheckArguments();

			Migrator mig = GetMigrator();
			List<long> appliedMigrations = mig.AppliedMigrations;

			System.Console.WriteLine("Available migrations:");
			foreach (var info in mig.MigrationsTypes)
			{
				long v = info.Version;
				System.Console.WriteLine("{0} {1} {2}",
				                  appliedMigrations.Contains(v) ? "=>" : "  ",
				                  v.ToString().PadLeft(3),
				                  StringUtils.ToHumanName(info.Type.Name)
					);
			}
		}

		public void Dump()
		{
			CheckArguments();

			SchemaDumper dumper = new SchemaDumper(dialect, connectionString);

			dumper.DumpTo(dumpTo);
		}

		/// <summary>
		/// Show usage information and help.
		/// </summary>
		public void PrintUsage()
		{
			const int TAB = 17;
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;

			System.Console.WriteLine("Database migrator - v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			System.Console.WriteLine();
			System.Console.WriteLine("usage:\nECM7.Migrator.Console.exe dialect connectionString migrationsAssembly [options]");
			System.Console.WriteLine();
			System.Console.WriteLine("\t{0} {1}", "dialect".PadRight(TAB), "Full name of dialect type (include assembly name)");
			System.Console.WriteLine("\t{0} {1}", "connectionString".PadRight(TAB), "Connection string to the database");
			System.Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(TAB), "Path to the assembly containing the migrations");
			System.Console.WriteLine("Options:");
			System.Console.WriteLine("\t-{0}{1}", "version NO".PadRight(TAB), "To specific version to migrate the database to (for migrae to latest version use -1)");
			System.Console.WriteLine("\t-{0}{1}", "list".PadRight(TAB), "List migrations");
			System.Console.WriteLine("\t-{0}{1}", "trace".PadRight(TAB), "Show debug informations");
			System.Console.WriteLine("\t-{0}{1}", "dump FILE".PadRight(TAB), "Dump the database schema as migration code");
			System.Console.WriteLine("\t-{0}{1}", "dryrun".PadRight(TAB), "Simulation mode (don't actually apply/remove any migrations)");
			System.Console.WriteLine();
		}

		#region Private helper methods

		private void CheckArguments()
		{
			if (connectionString == null)
				throw new ArgumentException("Connection string missing", "connectionString");
			if (migrationsAssembly == null)
				throw new ArgumentException("Migrations assembly missing", "migrationsAssembly");
		}

		private Migrator GetMigrator()
		{
			Assembly asm = Assembly.Load(migrationsAssembly);

			return new Migrator(dialect, connectionString, trace, asm) { Args = args, DryRun = dryrun };
		}

		private bool ParseArguments(string[] argv)
		{
			try
			{
				dialect = argv[0];
				connectionString = argv[1];
				migrationsAssembly = argv[2];

				for (int i = 0; i < argv.Length; i++)
				{
					if (argv[i].Equals("-list"))
					{
						list = true;
					}
					else if (argv[i].Equals("-trace"))
					{
						trace = true;
					}
					else if (argv[i].Equals("-dryrun"))
					{
						dryrun = true;
					}
					else if (argv[i].Equals("-version"))
					{
						migrateTo = long.Parse(argv[i + 1]);
						i++;
					}
					else if (argv[i].Equals("-dump"))
					{
						dumpTo = argv[i + 1];
						i++;
					}
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Parse arguments error:");
				System.Console.WriteLine(ex.Message);
				PrintUsage();
				return false;
			}

			return true;
		}
		#endregion
	}
}