using System.IO;
using System.Reflection;
using ECM7.Migrator.Compile;
using NUnit.Framework;

namespace ECM7.Migrator.Tests
{
    [TestFixture]
    public class ScriptEngineTests
    {
        [Test]
        public void CanCompileAssemblies() 
        {
            ScriptEngine engine = new ScriptEngine();

            // This should let it work on windows or mono/unix I hope
			string dataPath = Path.Combine("..", Path.Combine("..", @"src\ECM7.Migrator.Tests\Data"));

            Assembly asm = engine.Compile(dataPath);
            Assert.IsNotNull(asm);

            MigrationLoader loader = new MigrationLoader(null, false, asm);
            Assert.AreEqual(2, loader.LastVersion);

            Assert.AreEqual(2, MigrationLoader.GetMigrationTypes(asm).Count);
        }
    }
}