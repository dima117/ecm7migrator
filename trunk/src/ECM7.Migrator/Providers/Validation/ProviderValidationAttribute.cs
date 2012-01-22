using System;
using System.Runtime.Remoting.Proxies;

namespace ECM7.Migrator.Providers.Validation
{
	public class ProviderValidationAttribute : ProxyAttribute
	{
		// параметры валидации
		public readonly Type connectionType;
		public readonly bool schemaNameSupported;

		public ProviderValidationAttribute(Type connectionType, bool schemaNameSupported)
		{
			this.connectionType = connectionType;
			this.schemaNameSupported = schemaNameSupported;
		}

		public override MarshalByRefObject CreateInstance(Type serverType)
		{
			// создаем собственный proxy
			var proxy = new ProviderValidationProxy(serverType, connectionType, schemaNameSupported);
			return (MarshalByRefObject)proxy.GetTransparentProxy();
		}
	}
}
