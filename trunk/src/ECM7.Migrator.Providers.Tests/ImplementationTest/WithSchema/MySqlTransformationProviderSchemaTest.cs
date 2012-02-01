﻿using ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	[TestFixture, Category("MySql")]
	public class MySqlTransformationProviderSchemaTest : MySqlTransformationProviderTest
	{
		protected override string DefaultSchema
		{
			get { return "Moo"; }
		}
	}
}
