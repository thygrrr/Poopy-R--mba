using PicaVoxel;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatternGenerator))]
public class PatternFuzzer : Editor
{
	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();

		EditorGUILayout.Space();

		if (GUILayout.Button("Fuzz Pattern"))
		{
			
		}

	}
}
