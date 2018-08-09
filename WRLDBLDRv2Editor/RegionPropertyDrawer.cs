using UnityEngine;
using UnityEditor;

namespace WrldBldr.Gen
{
	[CustomPropertyDrawer (typeof (Region))]
	public class RegionPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty (position, label, property);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

			EditorGUI.SelectableLabel (
				new Rect (position.x, position.y, position.width, position.height),
				"Sections: " + property.FindPropertyRelative ("size").intValue + "/" + property.FindPropertyRelative ("sections").arraySize);

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty ();
		}
	}
}
