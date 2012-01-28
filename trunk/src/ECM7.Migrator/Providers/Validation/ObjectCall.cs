using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ECM7.Migrator.Providers.Validation
{
	/// <summary>
	/// ��������������� ������ ��� �������� ���������� ������.
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
		/// ����� ������������ � �������������
		/// </summary>
		public void Initialize()
		{
			// ���������
			if (validationOnInit != null)
			{
				validationOnInit(callMessage as IConstructionCallMessage);
			}

			//  ����� InitializeServerObject ������� 
			//  ��������� "����������" �������
			//  � ������ TransparentProxy � ������� ����������
			ReturnMessage = proxy.InitializeServerObject(
				(IConstructionCallMessage)callMessage);
		}

		/// <summary>
		/// ����� �������. 
		/// ����� �������� ��������, ��� � ������ ������ ����� ������
		/// �������� �������� Context.InternalContextID == __TP.stubData
		///	� ��������� ������ ����� RemotingServices.ExecuteMessage �������� � ������������.
		/// </summary>
		public void Execute()
		{
			// ���������
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