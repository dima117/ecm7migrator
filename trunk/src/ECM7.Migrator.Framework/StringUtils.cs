using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ECM7.Migrator.Framework
{
	public static class StringUtils
	{
		/// <summary>
		/// Convert a classname to something more readable.
		/// ex.: CreateATable => Create a table
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		public static string ToHumanName(string className)
		{
			if (string.IsNullOrWhiteSpace(className))
			{
				return string.Empty;
			}

			string name = Regex.Replace(className, "([a-z0-9])([A-Z])", "$1 $2").Replace('_', ' ');
			name = Regex.Replace(name, "\\s+", " ").ToLower();

			return char.ToUpper(name[0]) + name.Substring(1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		/// <param name="placeholder"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceOnce(string template, string placeholder, string replacement)
		{
			int loc = template.IndexOf(placeholder, StringComparison.Ordinal);
			if (loc < 0)
			{
				return template;
			}

			return new StringBuilder(template.Substring(0, loc))
				.Append(replacement)
				.Append(template.Substring(loc + placeholder.Length))
				.ToString();
		}

		public static SchemaQualifiedObjectName WithSchema(this string name, string schema)
		{
			return new SchemaQualifiedObjectName { Name = name, Schema = schema };
		}
	}
}
