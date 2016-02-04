using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FractalGenerator2))]
public class FractalGeneratorInspector2 : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Mesh"))
        {
            (target as FractalGenerator2).Generate();
        }
    }
}
