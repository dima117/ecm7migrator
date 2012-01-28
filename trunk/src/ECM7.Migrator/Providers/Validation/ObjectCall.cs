using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ECM7.Migrator.Providers.Validation
{
	/// <summary>
	/// Вспомогательный объект для хранения параметров вызова.
	/// </summary>
	public class ObjectCall
	{
		private readonly RealProxy proxy;
		private readonly IMessage callMessage;
		private readonly Action<IConstructionCallMessage> validationOnInit;
		private readonly Action<IMethodCallMessage> validationOnAction;
		public IMessage ReturnMessage { get; private set; }

		public ObjectCall(
			RealProxy proxy,
			IMessage callMessage,
			Action<IConstructionCallMessage> validationOnInit = null,
			Action<IMethodCallMessage> validationOnAction = null)
		{
			this.proxy = proxy;
			this.callMessage = callMessage;
			this.validationOnInit = validationOnInit;
			this.validationOnAction = validationOnAction;
		}

		/// <summary>
		/// Вызов конструктора и инициализация
		/// </summary>
		public void Initialize()
		{
			// валидация
			if (validationOnInit != null)
			{
				validationOnInit(callMessage as IConstructionCallMessage);
			}

			//  Вызов InitializeServerObject создаст 
			//  экземпляр "серверного" объекта
			//  и свяжет TransparentProxy с текущим контекстом
			ReturnMessage = proxy.InitializeServerObject(
				(IConstructionCallMessage)callMessage);
		}

		/// <summary>
		/// Вызов методов. 
		/// Стоит обратить внимание, что в момент вызова этого метода
		/// значение свойства Context.InternalContextID == __TP.stubData
		///	В противном случае вызов RemotingServices.ExecuteMessage приведет к зацикливанию.
		/// </summary>
		public void Execute()
		{
			// валидация
			if (validationOnAction != null)
			{
				validationOnAction(callMessage as IMethodCallMessage);
			}

			ReturnMessage = RemotingServices.ExecuteMessage(
				(MarshalByRefObject)proxy.GetTransparentProxy(),
				(IMethodCallMessage)callMessage);
		}
	}
}