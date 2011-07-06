namespace ECM7.Migrator.NAnt
{
	using System.IO;
	using Configuration;

	using ECM7.Migrator.NAnt.Loggers;

	using global::NAnt.Core;
	using global::NAnt.Core.Attributes;

	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <example>
	/// <loadtasks assembly="…/Migrator.NAnt.dll" />
	/// <target name="migrate" description="Migrate the database" depends="build">
	///  <property name="version" value="-1" overwrite="false" />
	///  <migrate
	///    dialect="ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer"
	///    connection-string="Database=MyDB;Data Source=localhost;User Id=;Password=;"
	///    assembly-file="bin/MyProject.dll"
	///    to="${version}" />
	/// </target>
	/// </example>
	[TaskName("migrate")]
	public class MigrateTask : Task, IMigratorConfiguration
	{
		/// <summary>
		/// Версия, до которой нужно обновить БД
		/// <para>По умолчанию = -1 (обновить до последней версии)</para>
		/// </summary>
		private long to = -1;

		/// <summary>
		/// Диалект
		/// </summary>
		[TaskAttribute("dialect", Required = true)]
		public string Dialect { get; set; }

		/// <summary>
		/// Строка подключения
		/// </summary>
		[TaskAttribute("connection-string")]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Название строки подключения
		/// </summary>
		[TaskAttribute("connection-string-name")]
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// Сборка с миграциями
		/// </summary>
		[TaskAttribute("assembly")]
		public string Assembly { get; set; }

		/// <summary>
		/// Путь к файлу с миграциями
		/// </summary>
		[TaskAttribute("assembly-file")]
		public FileInfo AssemblyFileInfo { get; set; }

		/// <summary>
		/// Ключ миграций
		/// </summary>
		[TaskAttribute("key")]
		public string Key { get; set; }

		/// <summary>
		/// Путь к файлу сборки с миграциями
		/// </summary>
		public string AssemblyFile
		{
			get { return AssemblyFileInfo.FullName; }
		}

		/// <summary>
		/// Версия, до которой нужно обновить БД
		/// </summary>
		[TaskAttribute("to")]
		public long To
		{
			get { return to; }
			set { to = value; }
		}

		/// <summary>
		/// Выполнить заданные действия над БД
		/// </summary>
		protected override void ExecuteTask()
		{
			Migrator migrator = MigratorFactory.CreateMigrator(this, new TaskLogger(this));

			migrator.Migrate(to);
		}
	}
}
