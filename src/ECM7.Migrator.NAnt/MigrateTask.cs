

using NAnt.Core;
using NAnt.Core.Attributes;

namespace ECM7.Migrator.NAnt
{
	using System.IO;
	using Configuration;
	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <example>
	/// <loadtasks assembly="…/Migrator.NAnt.dll" />
	/// <target name="migrate" description="Migrate the database" depends="build">
	///  <property name="version" value="-1" overwrite="false" />
	///  <migrate
	///    provider="ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider, ECM7.Migrator.Providers.SqlServer"
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
		[TaskAttribute("provider", Required = true)]
		public string Provider { get; set; }

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
		/// Путь к файлу сборки с миграциями
		/// </summary>
		public string AssemblyFile
		{
			get { return AssemblyFileInfo.FullName; }
		}

		/// <summary>
		/// Максимальное время выполнения команды
		/// </summary>
		[TaskAttribute("command-timeout")]
		public int? CommandTimeout { get; set; }

		/// <summary>
		/// Необходимо ли оборачивать имена в кавычки
		/// </summary>
		[TaskAttribute("quotes-needed")]
		public bool? NeedQuotesForNames { get; set; }

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
			ConfigureLogging();

			using (Migrator migrator = MigratorFactory.CreateMigrator(this))
			{
				migrator.Migrate(to);
			}
		}

		private void ConfigureLogging()
		{
			var simpleLayout = new NLog.Layouts.SimpleLayout("${longdate}:${message}");
			var nlogTarget = new NAntNLogTarget(this) { Layout = simpleLayout, Name = MigratorLogManager.LOGGER_NAME };
			MigratorLogManager.SetNLogTarget(nlogTarget);
		}
	}
}
