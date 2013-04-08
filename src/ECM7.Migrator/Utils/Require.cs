using System;
using System.Text.RegularExpressions;

namespace ECM7.Migrator.Utils
{
	/// <summary>
	/// Класс для проверки условий во время выполнения программы
	/// <para>Каждый метод этого класса проверяет определенное условие.</para>
	/// <para>Если условие не выполняется, генерируется исключение</para>
	/// </summary>
	public static class Require
	{
		/// <summary>
		/// Генерирует исключение
		/// </summary>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void Throw(string errorMessageFormatString, params object[] errorMessageArgs)
		{
			string msg = string.Format(errorMessageFormatString, errorMessageArgs);
			throw new Exception(msg);
		}

		/// <summary>
		/// Проверка, что выполняется некоторое условие
		/// </summary>
		/// <param name="condition">Проверяемое условие</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void That(bool condition, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (!condition)
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что два значения равны
		/// </summary>
		/// <param name="first">Первое значение</param>
		/// <param name="second">Второе значение</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void AreEqual(object first, object second, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (!Equals(first, second))
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что значение переменной - не пустое
		/// </summary>
		/// <param name="value">Проверяемое значение</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsNotNull(object value, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (ReferenceEquals(value, null))
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что значение переменной - пустое
		/// </summary>
		/// <param name="value">Проверяемое значение</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsNull(object value, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (!ReferenceEquals(value, null))
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что строка не является пустой
		/// </summary>
		/// <param name="value">Проверяемая строка</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsNotNullOrEmpty(string value, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			IsNotNullOrEmpty(value, false, errorMessageFormatString, errorMessageArgs);
		}

		/// <summary>
		/// Проверка, что строка не является пустой
		/// </summary>
		/// <param name="value">Проверяемая строка</param>
		/// <param name="ignoreWhiteSpaces">Признак "игнорировать при сравнении пустые символы"</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsNotNullOrEmpty(string value, bool ignoreWhiteSpaces, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			bool hasError = ignoreWhiteSpaces ? string.IsNullOrWhiteSpace(value) : string.IsNullOrEmpty(value);

			if (hasError)
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что логическая переменная или выражение принимает значение "ложь"
		/// </summary>
		/// <param name="condition">Проверяемое условие</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsFalse(bool condition, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (condition)
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что логическая переменная или выражение принимает значение "истина"
		/// </summary>
		/// <param name="condition">Проверяемое условие</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsTrue(bool condition, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (!condition)
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}

		/// <summary>
		/// Проверка, что строка соответствует заданному шаблону
		/// </summary>
		/// <param name="testString">Проверяемая строка</param>
		/// <param name="template">Шаблон</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsMatch(string testString, string template, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			IsMatch(testString, template, RegexOptions.None, errorMessageFormatString, errorMessageArgs);
		}

		/// <summary>
		/// Проверка, что строка соответствует заданному шаблону
		/// </summary>
		/// <param name="testString">Проверяемая строка</param>
		/// <param name="template">Шаблон</param>
		/// <param name="options">Параметры сравнения</param>
		/// <param name="errorMessageFormatString">Текст ошибки</param>
		/// <param name="errorMessageArgs">Параметры для сообщения об ошибке</param>
		public static void IsMatch(string testString, string template, RegexOptions options, string errorMessageFormatString, params object[] errorMessageArgs)
		{
			if (testString == null || !Regex.IsMatch(testString, template, options))
			{
				Throw(errorMessageFormatString, errorMessageArgs);
			}
		}
	}
}
