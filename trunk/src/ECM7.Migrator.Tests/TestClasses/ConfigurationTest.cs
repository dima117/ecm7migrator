namespace ECM7.Migrator.Tests.TestClasses
{
	using System.Configuration;
	using Configuration;
	using NUnit.Framework;

	/// <summary>
	/// Тесты конфигурационной секции
	/// </summary>
	[TestFixture]
	public class ConfigurationTes
	{
		/// <summary>
		/// Проверка получения параметров ин конфигурационной секции
		/// </summary>
		[Test]
		public void CanReadValues()
		{
			var config = ConfigurationManager.GetSection("migrator-test") as MigratorConfigurationSection;

			Assert.IsNotNull(config);
			Assert.AreEqual("111", config.Assembly);
			Assert.AreEqual("222", config.Dialect);
			Assert.AreEqual("333", config.ConnectionString);
			Assert.AreEqual("444", config.ConnectionStringName);
		}

		/// <summary>
		/// Тест инициализации из конфига
		/// </summary>
		[Test]
		public void CanInitByConfig()
		{
			Migrator migrator = MigratorFactory.InitByConfigFile();

			Assert.IsNotNull(migrator);
		}
	}
}
