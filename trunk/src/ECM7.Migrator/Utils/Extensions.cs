using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECM7.Migrator.Utils
{
	public static class Extensions
	{
		/// <summary>
		/// Получает строку, состоящую из строковых представлений
		/// элементов коллекции, разделенных заданной строкой-разделителем
		/// </summary>
		/// <typeparam name="T">Тип элементов коллекции</typeparam>
		/// <param name="collection">Коллекция</param>
		/// <param name="separator">Разделитель</param>
		/// <returns>Строковое представление коллекции с использованием заданного разделителя</returns>
		public static string ToSeparatedString<T>(this IEnumerable<T> collection, string separator)
		{
			return string.Join(separator, collection.Select(el => el.ToString()).ToArray());
		}

		/// <summary>
		/// Получает строку, состоящую из элементов коллекции, разделенных запятыми
		/// </summary>
		/// <typeparam name="T">Тип элементов коллекции</typeparam>
		/// <param name="collection">Коллекция</param>
		/// <returns>Строковое представление коллекции с использованием запятой в качестве разделителя</returns>
		public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection)
		{
			return collection.ToSeparatedString(",");
		}

		/// <summary>
		/// Проверка, что коллекция == null или пуста
		/// </summary>
		public static bool IsEmpty<T>(this IEnumerable<T> collection)
		{
			return !HasElements(collection);
		}

		/// <summary>
		/// Проверка, что число элементов коллекции > 0
		/// </summary>
		public static bool HasElements<T>(this IEnumerable<T> collection)
		{
			return collection != null && collection.Any();
		}

		/// <summary>
		/// Типизированный метод поиска атрибутов
		/// </summary>
		/// <typeparam name="T">Тип атрибута</typeparam>
		/// <param name="memberInfo">Тип объекта, в котором производится поиск</param>
		/// <param name="inherit">Признак: искать в базовых классах</param>
		public static T GetCustomAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
		{
			return Attribute.GetCustomAttribute(memberInfo, typeof(T), inherit) as T;
		}

		/// <summary>
		/// Типизированный метод поиска атрибутов
		/// </summary>
		/// <typeparam name="T">Тип атрибута</typeparam>
		/// <param name="assembly">Сборка, в которой производится поиск</param>
		/// <param name="inherit">Признак: искать в базовых классах</param>
		public static T GetCustomAttribute<T>(this Assembly assembly, bool inherit = false) where T : Attribute
		{
			return Attribute.GetCustomAttribute(assembly, typeof(T), inherit) as T;
		}
	}
}
