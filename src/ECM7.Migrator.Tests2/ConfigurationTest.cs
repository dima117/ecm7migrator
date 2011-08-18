namespace ECM7.Migrator.Tests2
{
	using System.Configuration;

	using ECM7.Migrator.Configuration;

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
			Assert.AreEqual("xxx", config.AssemblyFile);
			Assert.AreEqual("222", config.Provider);
			Assert.AreEqual("333", config.ConnectionString);
			Assert.AreEqual("444", config.ConnectionStringName);
		}

		/// <summary>
		/// Тест инициализации из конфига
		/// </summary>
		[Test]
		public void CanInitByConfig()
		{
			using(Migrator migrator = MigratorFactory.InitByConfigFile())
			{
				Assert.IsNotNull(migrator);
			}
		}
	}
}
