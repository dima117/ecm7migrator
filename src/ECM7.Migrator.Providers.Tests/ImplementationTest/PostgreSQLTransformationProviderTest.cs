namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using ECM7.Migrator.Providers.PostgreSQL;

	using NUnit.Framework;

	[TestFixture]
	public class PostgreSQLTransformationProviderTest
		: TransfprmationProviderTestBase<PostgreSQLTransformationProvider>
	{
		#region Overrides of TransfprmationProviderTestBase<PostgreSQLTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "NpgsqlConnectionString"; }
		}

		#endregion
	}
}
