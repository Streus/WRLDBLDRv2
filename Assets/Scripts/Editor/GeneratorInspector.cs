using UnityEngine;
using UnityEditor;

namespace WrldBldr
{
	[CustomEditor(typeof(Generator))]
	public class GeneratorInspector : Editor
	{
		private bool immediateGen = false;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			Generator g = (Generator)target;
			if (GUILayout.Button ("Generate"))
			{
				g.Generate ();
			}
		}
	}
}
