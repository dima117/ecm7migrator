using System.Collections.Generic;
using System.Linq;

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
	}
}
