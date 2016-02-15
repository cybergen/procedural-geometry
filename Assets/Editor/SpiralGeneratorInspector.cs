using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiralGenerator))]
public class SpiralGeneratorInspector : Editor
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
