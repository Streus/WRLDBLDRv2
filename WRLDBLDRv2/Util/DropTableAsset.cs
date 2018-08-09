using UnityEngine;

namespace WrldBldr.Util
{
	[CreateAssetMenu (menuName = "WrldBldr/Drop Table")]
	public class DropTableAsset : ScriptableObject
	{
		[SerializeField]
		private DropTable<GameObject> table = new DropTable<GameObject> ();

		public DropTable<GameObject> GetTable()
		{
			return table;
		}
	}
}
