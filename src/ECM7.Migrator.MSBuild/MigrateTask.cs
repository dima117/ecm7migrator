namespace ECM7.Migrator.MSBuild
{
	using System;
	using System.IO;
	using System.Reflection;
	using Framework.Loggers;
	using Logger;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

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
		public string Dialect { get; set; }

		[Required]
		public string ConnectionString { get; set; }

		/// <summary>
		/// The paths to the assemblies that contain your migrations. 
		/// This will generally just be a single item.
		/// </summary>
		public ITaskItem[] Migrations { get; set; }

		/// <summary>
		/// The paths to the directory that contains your migrations. 
		/// This will generally just be a single item.
		/// </summary>
		public string Directory { get; set; }

		public string Language { get; set; }

		public long To
		{
			get { return to; }
			set { to = value; }
		}

		public bool Trace { get; set; }

		public bool DryRun { get; set; }

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
			if (null != Migrations)
			{
				foreach (ITaskItem assembly in Migrations)
				{
					Assembly asm = Assembly.Load(assembly.GetMetadata("FullPath"));
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
				mig.Migrate(to);
		}
	}
}


