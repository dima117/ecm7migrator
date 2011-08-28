namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using ECM7.Migrator.Providers.PostgreSQL;

	using NUnit.Framework;

	[TestFixture]
	public class PostgreSQLTransformationProviderTest
		: TransformationProviderTestBase<PostgreSQLTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<PostgreSQLTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "NpgsqlConnectionString"; }
		}

		#endregion
	}
}
