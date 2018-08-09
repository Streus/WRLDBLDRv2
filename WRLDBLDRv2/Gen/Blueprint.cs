using UnityEngine;

namespace WrldBldr
{
	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class Blueprint
	{
		#region INSTANCE_VARS

		[SerializeField]
		private Region root;
		#endregion

		#region INSTANCE_METHODS

		public Blueprint()
		{
			root = new Region (30);
		}

		public Region GetRegionRoot()
		{
			return root;
		}
		#endregion
	}
}
