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

using System.Collections.Generic;

namespace ECM7.Migrator.Loader
{
	/// <summary>
	/// Comparer of Migration by their version attribute.
	/// </summary>
	public class MigrationInfoComparer : IComparer<MigrationInfo>
	{
		private readonly bool ascending = true;
		
		public MigrationInfoComparer(bool ascending)
		{
			this.ascending = ascending;
		}

		public int Compare(MigrationInfo x, MigrationInfo y)
		{
			return ascending 
				? x.Version.CompareTo(y.Version) 
				: y.Version.CompareTo(x.Version);
		}
	}
}