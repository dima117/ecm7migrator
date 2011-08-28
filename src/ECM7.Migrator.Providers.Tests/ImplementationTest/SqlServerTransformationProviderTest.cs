namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture]
	public class SqlServerTransformationProviderTest 
		: TransformationProviderTestBase<SqlServerTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<SqlServerTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "SqlServerConnectionString"; }
		}

		#endregion
	}
}
