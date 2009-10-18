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
using ECM7.Migrator.MSBuild.Logger;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace ECM7.Migrator.MSBuild
{
	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <remarks>
	/// To script the changes applied to the database via the migrations into a file, set the <see cref="ScriptChanges"/> 
	/// flag and provide a file to write the changes to via the <see cref="ScriptFile"/> setting.
	/// </remarks>
	/// <example>
    /// <Target name="Migrate" DependsOnTargets="Build">
    ///     <Migrate Dialect="ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"
    ///         Connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
    ///         Migrations="bin/MyProject.dll"/>
    /// </Target>
	/// </example>
    /// <example>
    /// <Target name="Migrate" DependsOnTargets="Build">
    ///     <CreateProperty Value="-1"  Condition="'$(SchemaVersion)'==''">
    ///        <Output TaskParameter="Value" PropertyName="SchemaVersion"/>
    ///     </CreateProperty>
    ///     <Migrate
	///			Dialect="ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"
    ///         Connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
    ///         Migrations="bin/MyProject.dll"
    ///         To="$(SchemaVersion)"/>
    /// </Target>
    /// </example>
	public class Migrate : Task
	{
		private long to = -1; // To last revision
		private string scriptFile;

		[Required]
		public string Dialect { set; get; }

		[Required]
		public string ConnectionString { set; get; }

		/// <summary>
		/// The paths to the assemblies that contain your migrations. 
		/// This will generally just be a single item.
		/// </summary>
		public ITaskItem[] Migrations { set; get; }

		/// <summary>
		/// The paths to the directory that contains your migrations. 
		/// This will generally just be a single item.
		/// </summary>
		public string Directory { set; get; }

		public string Language { set; get; }

		public long To
		{
			set { to = value; }
			get { return to; }
		}

		public bool Trace { set; get; }

		public bool DryRun { set; get; }

		/// <summary>
        /// Gets value indicating whether to script the changes made to the database 
        /// to the file indicated by <see cref="ScriptFile"/>.
        /// </summary>
        /// <value><c>true</c> if the changes should be scripted to a file; otherwise, <c>false</c>.</value>
	    public bool ScriptChanges
	    {
            get { return !string.IsNullOrEmpty(scriptFile); }
	    }

	    /// <summary>
        /// Gets or sets the script file that will contain the Sql statements 
        /// that are executed as part of the migrations.
        /// </summary>
	    public string ScriptFile
	    {
	        get { return scriptFile; }
	        set { scriptFile = value; }
	    }

		public override bool Execute()
		{
            if (! String.IsNullOrEmpty(Directory))
            {
                ScriptEngine engine = new ScriptEngine(Language, null);
                Execute(engine.Compile(Directory));
            }

            if (null != Migrations)
            {
                foreach (ITaskItem assembly in Migrations)
                {
                    Assembly asm = Assembly.LoadFrom(assembly.GetMetadata("FullPath"));
                    Execute(asm);
                }
            }

		    return true;
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


