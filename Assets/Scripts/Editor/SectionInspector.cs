using UnityEngine;
using UnityEditor;

namespace WrldBldr
{
	[CustomEditor(typeof(Section))]
	public class SectionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			Section room = (Section)target;

			EditorGUILayout.LabelField (room.GetArchetype().ToString());
			EditorGUILayout.LabelField (System.Convert.ToString (room.GetAdjMask (), 2).PadLeft (16, '0'));
		}
	}
}