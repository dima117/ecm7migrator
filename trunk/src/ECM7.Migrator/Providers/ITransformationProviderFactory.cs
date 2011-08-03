namespace ECM7.Migrator.Providers
{
	using System.Data;

	using ECM7.Migrator.Framework;

	/// <summary>
	/// Интерфейс фабрики провайдеров
	/// </summary>
	public interface ITransformationProviderFactory<out TProvider>
		where TProvider: ITransformationProvider
	{
		TProvider CreateProvider(IDbConnection connection);

		TProvider CreateProvider(string connectionString);
	}
}
