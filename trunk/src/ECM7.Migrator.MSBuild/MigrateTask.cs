namespace ECM7.Migrator.MSBuild
{
	using Configuration;

	using ECM7.Migrator.Framework.Logging;

	using log4net.Config;
	using log4net.Layout;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <example>
	/// <Target name="Migrate" DependsOnTargets="Build">
	///     <Migrate Dialect="ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"
	///         Connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
	///         AssemblyFile="bin/MyProject.dll"/>
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
	public class Migrate : Task, IMigratorConfiguration
	{
		#region IMigratorConfiguration members

		/// <summary>
		/// �������
		/// </summary>
		[Required]
		public string Dialect { get; set; }

		/// <summary>
		/// ������ �����������
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// �������� ������ �����������
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// ������ � ����������
		/// </summary>
		public string Assembly { get; set; }

		/// <summary>
		/// ���� � ����� � ����������
		/// </summary>
		public string AssemblyFile { get; set; }

		/// <summary>
		/// ���� ��������
		/// </summary>
		public string Key { get; set; }

		#endregion

		/// <summary>
		/// ������, �� ������� ����� �������� ��
		/// <para>�� ��������� = -1 (�������� �� ��������� ������)</para>
		/// </summary>
		private long to = -1;

		/// <summary>
		/// ������, �� ������� ����� �������� ��
		/// </summary>
		public long To
		{
			get { return to; }
			set { to = value; }
		}

		/// <summary>
		/// Executes a task.
		/// </summary>
		/// <returns>
		/// true if the task executed successfully; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			ConfigureLogging();

			Migrator migrator = MigratorFactory.CreateMigrator(this);

			migrator.Migrate(to);

			return true;
		}

		private void ConfigureLogging()
		{
			MSBuildLogAppender appender = new MSBuildLogAppender(this.Log)
			{
				Name = "ecm7migrator-msbuild-appender",
				Layout = new SimpleLayout()
			};

			appender.ActivateOptions();

			MigratorLogManager.SetLevel("ALL");
			MigratorLogManager.AddAppender(appender);
			XmlConfigurator.Configure();
		}
	}
}
