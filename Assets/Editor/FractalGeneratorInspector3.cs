using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FractalGenerator3))]
public class FractalGeneratorInspector3 : Editor
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
