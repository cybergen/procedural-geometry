using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhyllotaxisPatternGenerator))]
public class PhyllotaxisPatternGeneratorInspector : Editor
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
