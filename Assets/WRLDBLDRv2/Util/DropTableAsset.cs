using System.Collections.Generic;
using UnityEngine;

namespace WrldBldr.Util
{
	[CreateAssetMenu (menuName = "WrldBldr/Drop Table")]
	public class DropTableAsset : ScriptableObject
	{
		private DropTable<GameObject> table;

		[SerializeField]
		private List<int> dropChances;
		[SerializeField]
		private List<GameObject> items;

		public DropTableAsset()
		{
			table = null;
			dropChances = new List<int> ();
			items = new List<GameObject> ();
		}

		public void Reset()
		{
			table = null;
			dropChances = new List<int> ();
			items = new List<GameObject> ();
		}

		public DropTable<GameObject> GetTable()
		{
			if (table == null)
			{
				table = new DropTable<GameObject> ();
				table.Add (dropChances, items);
			}
			return table;
		}

		public GameObject GetGameObject()
		{
			return GetTable().Get (Random.Range, Random.value);
		}
	}
}
