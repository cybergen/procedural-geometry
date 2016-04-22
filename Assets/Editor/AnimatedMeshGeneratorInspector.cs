using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimatedMeshGenerator))]
public class AnimatedMeshGeneratorInspector : Editor
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
