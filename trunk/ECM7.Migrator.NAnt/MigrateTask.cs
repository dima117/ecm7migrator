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
using System.IO;
using System.Reflection;
using ECM7.Migrator.Compile;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.NAnt.Loggers;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace ECM7.Migrator.NAnt
{
	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <example>
	/// <loadtasks assembly="…/Migrator.NAnt.dll" />
    /// <target name="migrate" description="Migrate the database" depends="build">
    ///  <property name="version" value="-1" overwrite="false" />
    ///  <migrate
	///    dialect="ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"
    ///    connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
    ///    migrations="bin/MyProject.dll"
    ///    to="${version}" />
    /// </target>
	/// </example>
	[TaskName("migrate")]
	public class MigrateTask : Task
	{
		private long to = -1; // To last revision

		[TaskAttribute("dialect", Required = true)]
		public string Dialect { set; get; }

		[TaskAttribute("connectionstring", Required = true)]
		public string ConnectionString { set; get; }

		[TaskAttribute("migrations")]
		public FileInfo MigrationsAssembly { set; get; }

		/// <summary>
		/// The paths to the directory that contains your migrations. 
		/// This will generally just be a single item.
		/// </summary>
		[TaskAttribute("directory")]
		public string Directory { set; get; }

		[TaskAttribute("language")]
		public string Language { set; get; }


		[TaskAttribute("to")]
		public long To
		{
			set { to = value; }
			get { return to; }
		}

		[TaskAttribute("trace")]
		public bool Trace { set; get; }

		[TaskAttribute("dryrun")]
		public bool DryRun { set; get; }

		/// <summary>
        /// Gets value indicating whether to script the changes made to the database 
        /// to the file indicated by <see cref="ScriptFile"/>.
        /// </summary>
        /// <value><c>true</c> if the changes should be scripted to a file; otherwise, <c>false</c>.</value>
        public bool ScriptChanges
        {
            get { return !String.IsNullOrEmpty(ScriptFile); }
        }

		/// <summary>
		/// Gets or sets the script file that will contain the Sql statements 
		/// that are executed as part of the migrations.
		/// </summary>
		[TaskAttribute("scriptFile")]
		public string ScriptFile { get; set; }

		protected override void ExecuteTask()
		{
            if (! string.IsNullOrEmpty(Directory))
            {
                ScriptEngine engine = new ScriptEngine(Language, null);
                Execute(engine.Compile(Directory));
            }

            if (null != MigrationsAssembly)
            {
                Assembly asm = Assembly.LoadFrom(MigrationsAssembly.FullName);
                Execute(asm);
            }
		}

        private void Execute(Assembly asm)
        {
			Migrator mig = new Migrator(Dialect, ConnectionString, Trace, new TaskLogger(this), asm);
            mig.DryRun = DryRun;
            if (ScriptChanges)
            {
                using (StreamWriter writer = new StreamWriter(ScriptFile))
                {
                    mig.Logger = new SqlScriptFileLogger(mig.Logger, writer);
                    RunMigration(mig);
                }
            }
            else
            {
                RunMigration(mig);
            }
        }

        private void RunMigration(Migrator mig)
        {
            if (mig.DryRun)
                mig.Logger.Log("********** Dry run! Not actually applying changes. **********");

            if (to == -1)
                mig.MigrateToLastVersion();
            else
                mig.MigrateTo(to);
        }
	}
}
