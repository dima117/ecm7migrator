namespace ECM7.Migrator.Providers
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Поддержка форматов "TAB", "COL", "NAME" для экранирования идентификаторов
	/// </summary>
	public class SqlFormatter : IFormatProvider, ICustomFormatter
	{
		protected readonly Func<string, string> converter;

		public SqlFormatter(Func<string, string> converter)
		{
			Require.IsNotNull(converter, "Не задана функция экранирования идентификаторов");
			this.converter = converter;
		}

		public object GetFormat(Type formatType)
		{
			return formatType == typeof(ICustomFormatter) ? this : null;
		}

		public string Format(string fmt, object arg, IFormatProvider formatProvider)
		{
			// TODO: добавить обработку ienumerable

			string ufmt = (fmt ?? string.Empty).ToUpper(CultureInfo.InvariantCulture);

			if (!ufmt.In("TAB", "COL", "NAME"))
			{
				try
				{
					return HandleOtherFormats(fmt, arg);
				}
				catch (FormatException ex)
				{
					throw new FormatException(string.Format("The format of '{0}' is invalid.", fmt), ex);
				}
			}

			return converter(arg.ToString());
		}

		private static string HandleOtherFormats(string fmt, object arg)
		{
			if (arg is IFormattable)
			{
				return (arg as IFormattable).ToString(fmt, CultureInfo.CurrentCulture);
			}

			if (arg != null)
			{
				return arg.ToString();
			}
			return string.Empty;
		}
	}
}