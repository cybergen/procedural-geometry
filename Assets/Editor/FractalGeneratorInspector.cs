using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FractalGenerator))]
public class FractalGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            (target as Generatable).Generate();
        }
    }
}
