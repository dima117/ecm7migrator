using System;
using NLog;

namespace ECM7.Migrator.Framework.Logging
{
	/// <summary>
	/// Методы расширения для объектов, реализующих интерфейс ILogger
	/// </summary>
	public static class LogExtensions
	{
		/// <summary>
		/// Запись в лог о начале выполнения серии миграций
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="currentVersion">Текущая версия БД</param>
		/// <param name="finalVersion">Новая версия БД</param>
		public static void Started(this Logger log, long currentVersion, long finalVersion)
		{
			log.Info("Latest version applied : {0}.  Target version : {1}", currentVersion, finalVersion);
		}

		/// <summary>
		/// Запись о выполнении миграции
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="version">Версия миграции</param>
		/// <param name="migrationName">Название миграции</param>
		public static void MigrateUp(this Logger log, long version, string migrationName)
		{
			log.Info("Applying {0}: {1}", version, migrationName);
		}

		/// <summary>
		/// Запись об откате миграции
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="version">Версия миграции</param>
		/// <param name="migrationName">Название миграции</param>
		public static void MigrateDown(this Logger log, long version, string migrationName)
		{
			log.Info("Removing {0}: {1}", version, migrationName);
		}

		/// <summary>
		/// Запись о пропущенной миграции
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="version">Версия миграции</param>
		public static void Skipping(this Logger log, long version)
		{
			log.Info("{0} {1}", version, "<Migration not found>");
		}

		/// <summary>
		/// Запись об откате изменений миграции во время выполнения
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="originalVersion">Версия БД, к которой производится откат</param>
		public static void RollingBack(this Logger log, long originalVersion)
		{
			log.Info("Rolling back to migration {0}", originalVersion);
		}

		/// <summary>
		/// Запись о выполнении SQL-запроса
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="sql">Текст SQL запроса</param>
		public static void ExecuteSql(this Logger log, string sql)
		{
			log.Info(sql);
		}

		/// <summary>
		/// Запись об ошибке
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="version">Версия миграции, в которой произощла ошибка</param>
		/// <param name="migrationName">Название миграции</param>
		/// <param name="ex">Исключение</param>
		public static void Exception(this Logger log, long version, string migrationName, Exception ex)
		{
			Exception(log, "Error in migration: " + version, ex);
		}

		/// <summary>
		/// Запись об ошибке 
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="message">Сообщение об ошибке</param>
		/// <param name="ex">Исключение</param>
		public static void Exception(this Logger log, string message, Exception ex)
		{
			string msg = message;
			for (Exception current = ex; current != null; current = current.InnerException)
			{
				log.Error(msg, current);
				msg = "Inner exception:";
			}
		}

		/// <summary>
		/// Запись об окончании выполнения серии миграций
		/// </summary>
		/// <param name="log">Лог</param>
		/// <param name="originalVersion">Начальная версия БД</param>
		/// <param name="currentVersion">Конечная версия БД</param>
		public static void Finished(this Logger log, long originalVersion, long currentVersion)
		{
			log.Info("Migrated to version {0}", currentVersion);
		}
	}
}
