#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;

namespace ECM7.Migrator
{
	/// <summary>
	/// Handles loading Provider implementations
	/// </summary>
	public class ProviderFactory
	{
		#region GetDialect

		private static readonly Dictionary<Type, Dialect> dialects = new Dictionary<Type, Dialect>();

		public static Dialect GetDialect<TDialect>()
			where TDialect : Dialect, new()
		{
			Type type = typeof(TDialect);
			if (!dialects.ContainsKey(type) || dialects[type] == null)
				dialects[type] = new TDialect();

			Require.IsNotNull(dialects[type], "�� ������� ���������������� �������");
			return dialects[type];
		}

		public static Dialect GetDialect(Type dialectType)
		{
			ValidateDialectType(dialectType);

			if (!dialects.ContainsKey(dialectType) || dialects[dialectType] == null)
				dialects[dialectType] = Activator.CreateInstance(dialectType, null) as Dialect;

			Require.IsNotNull(dialects[dialectType], "�� ������� ���������������� �������");
			return dialects[dialectType];
		}

		/// <summary>
		/// ��������, ���:
		/// <para>- �������� dialectType �� ����� null;</para>
		/// <para>- ����� ����������� �� Dialect;</para>
		/// <para>- ����� ����� �������� ����������� ��� ����������;</para>
		/// </summary>
		/// <param name="dialectType">����� ��������</param>
		internal static void ValidateDialectType(Type dialectType)
		{
			Require.IsNotNull(dialectType, "�� ����� �������");
			Require.That(dialectType.IsSubclassOf(typeof(Dialect)), "����� �������� ������ ���� ����������� �� Dialect");

			ConstructorInfo constructor = dialectType.GetConstructor(Type.EmptyTypes);
			Require.IsNotNull(constructor, "����� �������� ������ ����� �������� ����������� ��� ����������");
		}

		#endregion

		#region Create

		public static ITransformationProvider Create<TDialect>(
			Type dialectType, string connectionString)
			where TDialect : Dialect, new()
		{
			return GetDialect<TDialect>().NewProviderForDialect(connectionString);
		}

		public static ITransformationProvider Create(Type dialectType, string connectionString)
		{
			return GetDialect(dialectType).NewProviderForDialect(connectionString);
		}		
		
		public static ITransformationProvider Create(string dialectTypeName, string connectionString)
		{
			Type dialectType = Type.GetType(dialectTypeName);
			Require.IsNotNull(dialectType, "�� ������� ��������� ��� ��������: {0}".FormatWith(dialectTypeName.Nvl("null")));
			return Create(dialectType, connectionString);
		}

		#endregion

	}
}
