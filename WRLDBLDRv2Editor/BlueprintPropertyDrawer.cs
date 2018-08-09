using UnityEngine;
using UnityEditor;

namespace WrldBldr.Gen
{
	[CustomPropertyDrawer(typeof(Blueprint))]
	public class BlueprintPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty (position, label, property);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

			EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height), property.FindPropertyRelative ("root"), GUIContent.none);

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty ();
		}
	}
}
