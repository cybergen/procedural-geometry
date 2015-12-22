using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FractalGenerator : MonoBehaviour
{
    public int FractalIterations;
    public float BaseWidth;

    public void CreateMesh()
    {
        Debug.Log("Generating mesh");

        var mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.Clear();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        AddTriangle(Vector3.zero, Vector3.up, vertices, triangles, BaseWidth);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray(); 
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void AddTriangle(Vector3 center, Vector3 up, List<Vector3> currentVectors, List<int> triangles, float width)
    {
        var triangles = new int[12];

        var halfWidth = width / 2;
        var vertLeg = Mathf.Sqrt(Sq(width) - Sq(halfWidth));

        var s = currentVectors.Count;
        currentVectors.Add(new Vector3(0, 0, 0));
        currentVectors.Add(new Vector3(width, 0, 0));
        currentVectors.Add(new Vector3(width * 0.5f, 0, vertLeg));
        currentVectors.Add(new Vector3(0.5f * width, vertLeg, vertLeg * 0.5f));

        triangles = new int[12] {
            s + 1, s + 2, s + 0,
            s + 0, s + 2, s + 3,
            s + 2, s + 1, s + 3,
            s + 0, s + 3, s + 1 };
        return triangles;
    }

    private static float Sq(float value)
    {
        return value * value;
    }
}
