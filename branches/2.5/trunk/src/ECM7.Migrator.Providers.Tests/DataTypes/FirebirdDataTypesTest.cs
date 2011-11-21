using System;
using System.Configuration;
using ECM7.Migrator.Providers.Firebird;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.DataTypes
{
	[TestFixture]
	public class FirebirdDataTypesTest :DataTypesTestBase<FirebirdTransformationProvider>
	{
		public override string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["FirebirdConnectionString"]; }
		}

		public override string ParameterName
		{
			get { return "?"; }
		}
	}
}
