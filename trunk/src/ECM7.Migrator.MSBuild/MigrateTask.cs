namespace ECM7.Migrator.MSBuild
{
	using Configuration;
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
		/// Диалект
		/// </summary>
		[Required]
		public string Dialect { get; set; }

		/// <summary>
		/// Строка подключения
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// Название строки подключения
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// Сборка с миграциями
		/// </summary>
		public string Assembly { get; set; }

		/// <summary>
		/// Путь к файлу с миграциями
		/// </summary>
		public string AssemblyFile { get; set; }

		/// <summary>
		/// Ключ миграций
		/// </summary>
		public string Key { get; set; }

		#endregion

		/// <summary>
		/// Версия, до которой нужно обновить БД
		/// <para>По умолчанию = -1 (обновить до последней версии)</para>
		/// </summary>
		private long to = -1;

		/// <summary>
		/// Версия, до которой нужно обновить БД
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
			Migrator migrator = MigratorFactory.CreateMigrator(this);

			migrator.Migrate(to);

			return true;
		}
	}
}
