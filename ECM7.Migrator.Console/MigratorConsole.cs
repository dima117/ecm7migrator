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

namespace ECM7.Migrator.MigratorConsole
{
	/// <summary>
	/// Commande line utility to run the migrations
	/// </summary>
	/// </remarks>
	public class MigratorConsole
	{
		private string provider;
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
			ParseArguments(argv);
		}
		
		/// <summary>
		/// Run the migrator's console
		/// </summary>
		/// <returns>-1 if error, else 0</returns>
		public int Run()
		{
			try
			{
				if (list)
					List();
				else if (dumpTo != null)
					Dump();
				else
					Migrate();
			}
			catch (ArgumentException aex)
			{
				Console.WriteLine("Invalid argument '{0}' : {1}", aex.ParamName, aex.Message);
				Console.WriteLine();
				PrintUsage();
				return -1;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
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
			
			Console.WriteLine("Available migrations:");
			foreach (Type t in mig.MigrationsTypes)
			{
                long v = MigrationLoader.GetMigrationVersion(t);
				Console.WriteLine("{0} {1} {2}",
                                  appliedMigrations.Contains(v) ? "=>" : "  ",
				                  v.ToString().PadLeft(3),
				                  StringUtils.ToHumanName(t.Name)
				                 );
			}
		}
		
		public void Dump()
		{
			CheckArguments();
			
			SchemaDumper dumper = new SchemaDumper(provider, connectionString);
			
			dumper.DumpTo(dumpTo);
		}
		
		/// <summary>
		/// Show usage information and help.
		/// </summary>
		public void PrintUsage()
		{
			const int TAB = 17;
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;
			
			Console.WriteLine("Database migrator - v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			Console.WriteLine();
			Console.WriteLine("usage:\nMigrator.Console.exe provider connectionString migrationsAssembly [options]");
			Console.WriteLine();
			Console.WriteLine("\t{0} {1}", "provider".PadRight(TAB), "The database provider (SqlServer, MySql, Postgre)");
			Console.WriteLine("\t{0} {1}", "connectionString".PadRight(TAB), "Connection string to the database");
			Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(TAB), "Path to the assembly containing the migrations");
			Console.WriteLine("Options:");
			Console.WriteLine("\t-{0}{1}", "version NO".PadRight(TAB), "To specific version to migrate the database to");
			Console.WriteLine("\t-{0}{1}", "list".PadRight(TAB), "List migrations");
			Console.WriteLine("\t-{0}{1}", "trace".PadRight(TAB), "Show debug informations");
			Console.WriteLine("\t-{0}{1}", "dump FILE".PadRight(TAB), "Dump the database schema as migration code");
			Console.WriteLine("\t-{0}{1}", "dryrun".PadRight(TAB), "Simulation mode (don't actually apply/remove any migrations)");
			Console.WriteLine();
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
			Assembly asm = Assembly.LoadFrom(migrationsAssembly);

			Migrator migrator = new Migrator(provider, connectionString, trace, asm);
			migrator.Args = args;
		    migrator.DryRun = dryrun;
			return migrator;
		}
				
		private void ParseArguments(string[] argv)
		{
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
					migrateTo = long.Parse(argv[i+1]);
					i++;
				}
				else if (argv[i].Equals("-dump"))
				{
					dumpTo = argv[i+1];
					i++;
				}
				else
				{
					if (i == 0) provider = argv[i];
					if (i == 1) connectionString = argv[i];
					if (i == 2) migrationsAssembly = argv[i];
				}
			}
		}
		#endregion
	}
}
