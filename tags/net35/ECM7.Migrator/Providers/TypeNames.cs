using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using ECM7.Common.DataStructure;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers
{

	using TypesDictionary = Dictionary<DbType, SortedList<int, Pair<string, int?>>>;


	/// <summary>
	/// This class maps a DbType to names.
	/// </summary>
	/// <remarks>
	/// Associations may be marked with a capacity. Calling the <c>Get()</c>
	/// method with a type and actual size n will return the associated
	/// name with smallest capacity >= n, if available and an unmarked
	/// default type otherwise.
	/// Eg, setting
	/// <code>
	///		Names.Put(DbType,			"TEXT" );
	///		Names.Put(DbType,	255,	"VARCHAR($l)" );
	///		Names.Put(DbType,	65534,	"LONGVARCHAR($l)" );
	/// </code>
	/// will give you back the following:
	/// <code>
	///		Names.Get(DbType)			// --> "TEXT" (default)
	///		Names.Get(DbType,100)		// --> "VARCHAR(100)" (100 is in [0:255])
	///		Names.Get(DbType,1000)		// --> "LONGVARCHAR(1000)" (100 is in [256:65534])
	///		Names.Get(DbType,100000)	// --> "TEXT" (default)
	/// </code>
	/// On the other hand, simply putting
	/// <code>
	///		Names.Put(DbType, "VARCHAR($l)" );
	/// </code>
	/// would result in
	/// <code>
	///		Names.Get(DbType)		// --> "VARCHAR($l)" (will cause trouble)
	///		Names.Get(DbType,100)	// --> "VARCHAR(100)" 
	///		Names.Get(DbType,1000)	// --> "VARCHAR(1000)"
	///		Names.Get(DbType,10000)	// --> "VARCHAR(10000)"
	/// </code>
	/// </remarks>
	public class TypeNames
	{


		#region Private

		private readonly TypesDictionary typeMapping = new TypesDictionary();
		private readonly Dictionary<DbType, string> defaults = new Dictionary<DbType, string>();

		/// <summary>
		/// Добавить значение по-умолчанию для типа
		/// </summary>
		/// <param name="typecode">Тип</param>
		/// <param name="value">Значение</param>
		private void PutDefaultValue(DbType typecode, string value)
		{
			defaults[typecode] = value;
		}

		/// <summary>
		/// Получить значение по-умолчанию
		/// </summary>
		/// <param name="typecode">Тип</param>
		/// <returns>
		/// Значение по-умолчанию для данного типа.
		/// Если значение определить не удалось, генерируется исключение.
		/// </returns>
		private string GetDefaultValue(DbType typecode)
		{
			string result;
			if (!defaults.TryGetValue(typecode, out result))
				throw new ArgumentException("Dialect does not support DbType." + typecode, "typecode");

			return result;
		}

		private void PutValue(DbType typecode, int length, Pair<string, int?> value)
		{
			SortedList<int, Pair<string, int?>> map;
			if (!typeMapping.TryGetValue(typecode, out map))
				typeMapping[typecode] = map = new SortedList<int, Pair<string, int?>>();

			map[length] = value;
		}

		/// <summary>
		/// Получение строки SQL для типа с учетом размеров
		/// </summary>
		/// <param name="typecode">Тип</param>
		/// <param name="size">Размер</param>
		/// <returns>
		/// Возвращает строку SQL для типа, определенную с учетом его размеров.
		/// Если строку SQL определить не удалось, возвращается null.
		/// </returns>
		private Pair<string, int?> GetValue(DbType typecode, int size)
		{
			SortedList<int, Pair<string, int?>> map;
			typeMapping.TryGetValue(typecode, out map);

			if (map == null) return null;

			if (map.Count(pair => pair.Key >= size) == 0) return null;

			return map
				.OrderBy(pair => pair.Key)
				.First(pair => pair.Key >= size).Value;
		}

		#endregion

		#region Put

		public void Put(DbType typecode, int? length, string value)
		{
			Put(typecode, length, value, null);
		}

		public void Put(DbType typecode, int? length, string value, int? defaultScale)
		{
			if (length.HasValue)
				PutValue(typecode, length.Value, new Pair<string, int?>(value, defaultScale));
			else
				PutDefaultValue(typecode, value);
		}

		public void Put(DbType typecode, string value)
		{
			PutDefaultValue(typecode, value);
		}

		#endregion

		#region Get

		public string Get(ColumnType columnType)
		{
			return Get(columnType.DataType, columnType.Length, columnType.Scale);
		}

		public string Get(DbType typecode)
		{
			return Get(typecode, null);
		}

		public string Get(DbType typecode, int? length)
		{
			return Get(typecode, length, null);
		}

		public string Get(DbType typecode, int? length, int? scale)
		{
			Pair<string, int?> result = null;
			
			if (length.HasValue)
				result = GetValue(typecode, length.Value);
				
			if (result == null)
				result = new Pair<string, int?>(GetDefaultValue(typecode), null);

			return Replace(result.First, length, scale ?? result.Second);
		}

		#endregion

		/// <summary>
		/// Проверка, содержит ли провайдер мэппинг для заданного типа
		/// </summary>
		/// <param name="type">Проверяемый тип</param>
		/// <returns>Если мэппинг для проверяемого типа установлен, возвращается true, иначе возвращается false.</returns>
		public bool HasType(DbType type)
		{
			return typeMapping.ContainsKey(type) || defaults.ContainsKey(type);
		}

		#region Replacing

		public const string LENGTH_PLACE_HOLDER = "$l";
		public const string SCALE_PLACE_HOLDER = "$s";

		private static string Replace(string type, int? size, int? scale)
		{
			if (size.HasValue)
				type = StringUtils.ReplaceOnce(type, LENGTH_PLACE_HOLDER, size.ToString());

			if (scale.HasValue)
				type = StringUtils.ReplaceOnce(type, SCALE_PLACE_HOLDER, scale.ToString());

			return type;
		}

		#endregion

	}
}