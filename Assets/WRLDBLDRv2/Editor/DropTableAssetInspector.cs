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
			SerializedObject obj = new SerializedObject (asset);
			DropTable<GameObject> table = asset.GetTable ();
			

			//table stats
			EditorGUILayout.LabelField ("Stats", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField ("Size", table.Size.ToString ());
			EditorGUILayout.LabelField ("MaxRoll", table.MaxRoll.ToString ());
			EditorGUILayout.LabelField ("Bias", table.Bias.ToString ());
			EditorGUI.indentLevel--;

			//add button
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Add Item", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			addDropChance = EditorGUILayout.IntField ("Drop Chance", addDropChance);
			addObj = (GameObject)EditorGUILayout.ObjectField (new GUIContent("Item"), addObj, typeof (GameObject), false);
			if (GUILayout.Button ("Add"))
			{
				int insertIndex = table.Size;
				try
				{
					if (!table.Contains(addObj) && table.Add (addDropChance, addObj))
					{
						SerializedProperty prop = obj.FindProperty ("dropChances");
						prop.InsertArrayElementAtIndex (insertIndex);
						prop.GetArrayElementAtIndex (insertIndex).intValue = addDropChance;
						obj.FindProperty ("items");
						prop.InsertArrayElementAtIndex (insertIndex);
						prop.GetArrayElementAtIndex (insertIndex).objectReferenceValue = addObj;
					}
					OnEnable ();
				}
				catch (System.ArgumentException ae)
				{
					Debug.LogException (ae);
				}
			}
			EditorGUI.indentLevel--;

			//object list and remove buttons
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Items", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
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
			EditorGUI.indentLevel--;

			while (removeQueue.Count > 0)
			{
				GameObject go = removeQueue.Dequeue ();
				int dc = table.GetDropChance (go);
				if (table.Remove (go))
				{
					RemoveFromList (obj.FindProperty ("dropChances"), dc);
					RemoveFromList (obj.FindProperty ("items"), go);
				}
			}

			obj.ApplyModifiedProperties ();
			if (GUI.changed)
				EditorUtility.SetDirty (asset);
		}

		private void RemoveFromList(SerializedProperty listProp, int item)
		{
			for (int i = 0; i < listProp.arraySize; i++)
			{
				if (listProp.GetArrayElementAtIndex (i).intValue == item)
				{
					listProp.DeleteArrayElementAtIndex (i);
					break;
				}
			}
		}
		private void RemoveFromList(SerializedProperty listProp, GameObject item)
		{
			for (int i = 0; i < listProp.arraySize; i++)
			{
				if (listProp.GetArrayElementAtIndex (i).objectReferenceValue == item)
				{
					listProp.DeleteArrayElementAtIndex (i);
					break;
				}
			}
		}
	}
}