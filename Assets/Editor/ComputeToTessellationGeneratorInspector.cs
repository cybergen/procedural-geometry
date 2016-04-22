using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ComputeToTessellationGenerator))]
public class ComputeToTessellationGeneratorInspector : Editor
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
