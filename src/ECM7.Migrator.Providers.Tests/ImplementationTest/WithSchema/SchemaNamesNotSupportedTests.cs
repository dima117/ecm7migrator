using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using ECM7.Common.Utils.Exceptions;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Firebird;
using ECM7.Migrator.Providers.SQLite;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	/// <summary>
	/// Тестирование провайдеров, не поддерживающих схемы
	/// </summary>
	[TestFixture]
	public class SchemaNamesNotSupportedTests
	{
		#region helpers
		
		/// <summary>
		/// Значение по умолчанию для заданного типа
		/// </summary>
		private static object GetDefaultValue(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}

		/// <summary>
		/// Запись в лог
		/// </summary>
		private static void WriteLog(string format, MethodInfo method)
		{
			Debug.WriteLine(string.Format(format, method.Name));
		}

		/// <summary>
		/// Проходим по всем методам провайдера и проверяем, что методы, 
		/// принимающие в параметрах SchemaQualifiedObjectName, генерируют исключение
		/// </summary>
		private static void AllMethodTest(Type providerType, string cstringName)
		{
			string cstring = ConfigurationManager.AppSettings[cstringName];

			using (ITransformationProvider provider = ProviderFactory.Create(providerType, cstring))
			{
				MethodInfo[] methods = providerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

				foreach (var method in methods)
				{
					WriteLog("{0}: verify method", method);

					ParameterInfo[] parameters = method.GetParameters();
					var values = new object[parameters.Length];

					bool hasName = false;

					for (int j = 0; j < parameters.Length; j++)
					{
						var parameter = parameters[j];
						if (parameter.ParameterType == typeof(SchemaQualifiedObjectName))
						{
							values[j] = "TEST".WithSchema("MOO");
							hasName = true;
						}
						else
						{
							values[j] = GetDefaultValue(parameter.ParameterType);
						}
					}

					if (hasName)
					{
						// получаем TargetInvocationException, т.к Reflection оборачивает исключения
						MethodInfo method1 = method;
						var ex = Assert.Throws<TargetInvocationException>(() => method1.Invoke(provider, values));

						Assert.IsNotNull(ex.InnerException);
						Assert.IsInstanceOf<RequirementNotCompliedException>(ex.InnerException);
						Assert.AreEqual(ex.InnerException.Message, Messages.SchemaNamesIsNotSupported);

						WriteLog("{0}: correct exception was thrown", method);
					}
					else
					{
						WriteLog("{0}: name parameters not found", method);
					}
					Debug.WriteLine(string.Empty);
				}
			}
		}

		#endregion
		
		/// <summary>
		/// Проверяем методы провайдера MSSQL CE
		/// </summary>
		[Test, Category("SqlServerCe")]
		public void SqlServerCeTransformationProviderTest()
		{
			AllMethodTest(typeof(SqlServerCeTransformationProvider), "SqlServerCeConnectionString");
		}

		/// <summary>
		/// Проверяем методы провайдера SQLite
		/// </summary>
		[Test, Category("SQLite")]
		public void SQLiteTransformationProviderTest()
		{
			AllMethodTest(typeof(SQLiteTransformationProvider), "SQLiteConnectionString");
		}

		/// <summary>
		/// Проверяем методы провайдера Firebird
		/// </summary>
		[Test, Category("Firebird")]
		public void FirebirdTransformationProviderTest()
		{
			AllMethodTest(typeof(FirebirdTransformationProvider), "FirebirdConnectionString");
		}
	}
}
