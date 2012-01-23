using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.Validation
{
	/// <summary>
	/// Валидация параметров провайдера
	/// </summary>
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

		/// <summary>
		/// Проверка типа подключения при создании объекта
		/// </summary>
		/// <param name="msg">Дескриптор конструктора объекта</param>
		private void ConnectionTypeValidation(IConstructionCallMessage msg)
		{
			if (msg.ArgCount > 0)
			{
				foreach (var arg in msg.Args)
				{
					if (arg is IDbConnection)
					{
						Type type = arg.GetType();
						Require.That(connectionType.IsAssignableFrom(type),
							"Данный провайдер использует подключения типа [{0}]. При инициализации провайдера было указано подключение типа [{1}]", connectionType.FullName, type.FullName);
					}
				}
			}
		}

		/// <summary>
		/// Проверка имени схемы при вызове методов
		/// </summary>
		/// <param name="msg">Дескриптор вызываемого метода</param>
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
