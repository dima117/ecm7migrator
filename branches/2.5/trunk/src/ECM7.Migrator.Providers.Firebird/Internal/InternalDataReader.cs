using System;

namespace ECM7.Migrator.Providers.Firebird.Internal
{
	using System.Data;

	internal class InternalDataReader : IDataReader
	{
		public InternalDataReader(IDbCommand command, CommandBehavior behavior = CommandBehavior.Default)
		{
			internalReader = command.ExecuteReader(behavior);
			internalCommand = command;
		}

		private readonly IDbCommand internalCommand;
		private readonly IDataReader internalReader;

		#region Implementation of IDisposable

		public void Dispose()
		{
			internalReader.Dispose();
			this.Close();
		}

		#endregion

		#region Implementation of IDataRecord

		/// <summary>
		/// Gets the name for the field to find.
		/// </summary>
		/// <returns>
		/// The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		/// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
		public string GetName(int i)
		{
			return internalReader.GetName(i);
		}

		/// <summary>
		/// Gets the data type information for the specified field.
		/// </summary>
		/// <returns>
		/// The data type information for the specified field.
		/// </returns>
		/// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
		public string GetDataTypeName(int i)
		{
			return internalReader.GetDataTypeName(i);
		}

		/// <summary>
		/// Gets the <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
		/// </returns>
		/// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
		public Type GetFieldType(int i)
		{
			return internalReader.GetFieldType(i);
		}

		/// <summary>
		/// Return the value of the specified field.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Object"/> which will contain the field value upon return.
		/// </returns>
		/// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
		public object GetValue(int i)
		{
			return internalReader.GetValue(i);
		}

		/// <summary>
		/// Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <returns>
		/// The number of instances of <see cref="T:System.Object"/> in the array.
		/// </returns>
		/// <param name="values">An array of <see cref="T:System.Object"/> to copy the attribute fields into. </param><filterpriority>2</filterpriority>
		public int GetValues(object[] values)
		{
			return internalReader.GetValues(values);
		}

		/// <summary>
		/// Return the index of the named field.
		/// </summary>
		/// <returns>
		/// The index of the named field.
		/// </returns>
		/// <param name="name">The name of the field to find. </param><filterpriority>2</filterpriority>
		public int GetOrdinal(string name)
		{
			return internalReader.GetOrdinal(name);
		}

		/// <summary>
		/// Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <returns>
		/// The value of the column.
		/// </returns>
		/// <param name="i">The zero-based column ordinal. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
		public bool GetBoolean(int i)
		{
			return internalReader.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return internalReader.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return internalReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return internalReader.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return internalReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public Guid GetGuid(int i)
		{
			return internalReader.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return internalReader.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return internalReader.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return internalReader.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			return internalReader.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			return internalReader.GetDouble(i);
		}

		public string GetString(int i)
		{
			return internalReader.GetString(i);
		}

		public decimal GetDecimal(int i)
		{
			return internalReader.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			return internalReader.GetDateTime(i);
		}

		public IDataReader GetData(int i)
		{
			return internalReader.GetData(i);
		}

		public bool IsDBNull(int i)
		{
			return internalReader.IsDBNull(i);
		}

		public int FieldCount
		{
			get
			{
				return internalReader.FieldCount;
			}
		}

		object IDataRecord.this[int i]
		{
			get
			{
				return internalReader[i];
			}
		}

		object IDataRecord.this[string name]
		{
			get
			{
				return internalReader[name];
			}
		}

		#endregion

		#region Implementation of IDataReader

		public void Close()
		{
			internalReader.Close();
			internalCommand.Dispose();
		}

		public DataTable GetSchemaTable()
		{
			return internalReader.GetSchemaTable();
		}

		public bool NextResult()
		{
			return internalReader.NextResult();
		}

		public bool Read()
		{
			return internalReader.Read();
		}

		public int Depth
		{
			get
			{
				return internalReader.Depth;
			}
		}

		public bool IsClosed
		{
			get
			{
				return internalReader.IsClosed;
			}
		}

		public int RecordsAffected
		{
			get
			{
				return internalReader.RecordsAffected;
			}
		}

		#endregion
	}
}
