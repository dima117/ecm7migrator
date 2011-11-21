namespace ECM7.Migrator.Providers
{
	using System;

	using ECM7.Migrator.Framework;

	public class ConditionByProvider : IConditionByProvider
	{
		private readonly ITransformationProvider currentProvider;

		public ITransformationProvider CurrentProvider
		{
			get { return currentProvider; }
		}

		public bool isExecuted;

		public ConditionByProvider(ITransformationProvider current)
		{
			Require.IsNotNull(current, "Не задан текущий провайдер");
			currentProvider = current;
			isExecuted = false;
		}

		private static void ValidateProviderType(Type providerType)
		{
			Require.IsNotNull(providerType, "Не задан тип провайдера");

			bool isValid = typeof(ITransformationProvider).IsAssignableFrom(providerType);
			Require.IsTrue(isValid, "Тип провайдера должен реализозвывать интерфейс ITransformationProvider");
		}

		public IConditionByProvider For<TProvider>(Action<ITransformationProvider> action)
		{
			return For(typeof(TProvider), action);
		}

		public IConditionByProvider For(string providerTypeName, Action<ITransformationProvider> action)
		{
			Type providerType = ProviderFactory.GetProviderType(providerTypeName);
			return For(providerType, action);
		}

		public IConditionByProvider For(Type providerType, Action<ITransformationProvider> action)
		{
			ValidateProviderType(providerType);

			bool needExecute = providerType.IsAssignableFrom(currentProvider.GetType());

			if (needExecute && action != null)
			{
				action(currentProvider);
			}

			isExecuted |= needExecute;

			return this;
		}

		public void Else(Action<ITransformationProvider> action)
		{
			if (!isExecuted && action != null)
			{
				action(currentProvider);
				isExecuted = true;
			}
		}
	}
}
