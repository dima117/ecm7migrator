namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture]
	public class SqlServerTransformationProviderTest 
		: TransfprmationProviderTestBase<SqlServerTransformationProvider>
	{
		#region Overrides of TransfprmationProviderTestBase<SqlServerTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "SqlServerConnectionString"; }
		}

		#endregion
	}
}
