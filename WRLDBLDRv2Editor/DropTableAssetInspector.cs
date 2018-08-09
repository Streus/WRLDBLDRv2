using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace WrldBldr.Util
{
	[CustomEditor (typeof (DropTableAsset))]
	public class DropTableAssetInspector : Editor
	{
		private GameObject addObj;
		private int addDropChance;

		public void OnEnable()
		{
			addObj = null;
			addDropChance = DropTable<GameObject>.MIN_DROP_CHANCE;
		}

		public override void OnInspectorGUI()
		{
			DropTableAsset asset = (DropTableAsset)target;
			DropTable<GameObject> table = asset.GetTable ();
			SerializedObject obj = new SerializedObject(target);

			//table stats


			//add button
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Add Item", EditorStyles.boldLabel);
			addDropChance = EditorGUILayout.IntField ("Drop Chance", addDropChance);
			addObj = (GameObject)EditorGUILayout.ObjectField (new GUIContent("Item"), addObj, typeof (GameObject), false);
			if (GUILayout.Button ("Add"))
			{
				table.Add (addDropChance, addObj);
				OnEnable ();
			}

			//object list and remove buttons
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Items", EditorStyles.boldLabel);
			Queue<GameObject> removeQueue = new Queue<GameObject> ();
			foreach (DropTable<GameObject>.Drop go in table)
			{
				EditorGUILayout.BeginHorizontal ();

				GUILayout.Label (go.dropChance+"");
				GUILayout.Label (go.obj != null ? go.obj.name : "NULL");

				if (GUILayout.Button ("Find") && go.obj != null)
					EditorGUIUtility.PingObject (go.obj);

				if (GUILayout.Button ("Remove"))
					removeQueue.Enqueue (go.obj);

				EditorGUILayout.EndHorizontal ();
			}

			while (removeQueue.Count > 0)
				table.Remove (removeQueue.Dequeue ());
		}
	}
}