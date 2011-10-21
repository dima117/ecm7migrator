using ECM7.Migrator.Console;
using NUnit.Framework;

namespace ECM7.Migrator.Tests2.Console
{
	[TestFixture]
	public class CommandLineParamsTest
	{
		[Test]
		public void NotEnoughtParametersMustSetHelpMode()
		{
			var p = CommandLineParams.Parse(new[] { "111" });
			Assert.AreEqual(MigratorConsoleMode.Help, p.mode);
		}

		[Test]
		public void CanSetMainParams1()
		{
			var p = CommandLineParams.Parse(new[] {"111", "222", "333"});
			Assert.AreEqual("111", p.config.Provider);
			Assert.AreEqual("222", p.config.ConnectionString);
			Assert.AreEqual("333", p.config.Assembly);

			Assert.IsNull(p.config.ConnectionStringName);
			Assert.IsNull(p.config.AssemblyFile);
		}

		[Test]
		public void CanSetMainParams2()
		{
			var p = CommandLineParams.Parse(new[] { "111", "moo-test", "ECM7.Migrator.TestAssembly.dll" });
			Assert.AreEqual("111", p.config.Provider);
			Assert.AreEqual("moo-test", p.config.ConnectionStringName);
			Assert.AreEqual("ECM7.Migrator.TestAssembly.dll", p.config.AssemblyFile);

			Assert.IsNull(p.config.ConnectionString);
			Assert.IsNull(p.config.Assembly);
		}

		[Test]
		public void CanSetAdditionalOptions()
		{
			var p = CommandLineParams.Parse(new[]
			        {
			            "111", 
						"moo-test", 
						"ECM7.Migrator.TestAssembly.dll",
						"-v:123",
						"-list"
			        });
			
			Assert.AreEqual(123, p.version);
			Assert.AreEqual(MigratorConsoleMode.List, p.mode);
		}

		[Test]
		public void CanSetAdditionalOptions2()
		{
			var p = CommandLineParams.Parse(new[]
			        {
			            "111", 
						"moo-test", 
						"ECM7.Migrator.TestAssembly.dll",
						@"/?"
			        });

			Assert.AreEqual(-1, p.version);
			Assert.AreEqual(MigratorConsoleMode.Help, p.mode);
		}
	}
}
