namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;

	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture, Category("SqlServer2005")]
	public class SqlServer2005TransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["SqlServer2005ConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("SqlServer2005ConnectionString", "No config file");

			provider = new SqlServerTransformationProvider(new SqlServer2005Dialect(), constr);
			provider.BeginTransaction();

			AddDefaultTable();
		}
	}
}