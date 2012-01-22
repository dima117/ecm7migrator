using System;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.Validation
{
	public class ProviderValidationProxy : RealProxy
	{
		private readonly Context context;

		// параметры валидации
		private readonly Type connectionType;
		private readonly bool schemaNameSupported;

		public ProviderValidationProxy(Type type, Type connectionType, bool schemaNameSupported)
			: base(type)
		{
			context = new Context();
			context.Freeze();

			this.connectionType = connectionType;
			this.schemaNameSupported = schemaNameSupported;
		}

		private void ConnectionTypeValidation(IConstructionCallMessage msg)
		{
			// todo: РЕАЛИЗОВАТЬ ПРОВЕРКУ ПРОВАЙДЕРА!!!!!!!!!!!!!!!!!!
		}

		private void SchemaNameValidation(IMethodCallMessage msg)
		{
			if (!schemaNameSupported)
			{
				foreach (var arg in msg.Args)
				{
					var name = arg as SchemaQualifiedObjectName;

					if (name != null)
					{
						Require.That(name.SchemaIsEmpty, "Схемы не поддерживаются");
					}
				}
			}
		}
		
		public override IMessage Invoke(IMessage msg)
		{
			var call = new ObjectCall(this, msg, ConnectionTypeValidation, SchemaNameValidation);

			if (msg is IConstructionCallMessage)
			{
				context.DoCallBack(call.Initialize);
			}
			else if (msg is IMethodCallMessage)
			{
				context.DoCallBack(call.Execute);
			}
			else
			{
				throw new Exception();
			}

			return call.ReturnMessage;
		}
	}
}
