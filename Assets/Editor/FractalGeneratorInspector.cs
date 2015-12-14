using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FractalGenerator))]
public class FractalGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Mesh"))
        {
            (target as FractalGenerator).CreateMesh();
        }
    }
}
