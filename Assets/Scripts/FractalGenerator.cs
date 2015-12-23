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
        Draw(Vector3.zero, Vector3.up, Vector3.forward, vertices, triangles, BaseWidth);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray(); 
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void Draw(
        Vector3 center, 
        Vector3 up, 
        Vector3 forward,
        List<Vector3> vectors, 
        List<int> triangles, 
        float width)
    {
        var halfWidth = width / 2;
        var vertLeg = Mathf.Sqrt(Sq(width) - Sq(halfWidth));
        var halfVert = vertLeg / 2;

        up.Normalize();
        forward.Normalize();
        var back = -forward;
        var left = -Vector3.Cross(up, forward);
        var right = Vector3.Cross(up, forward);

        var s = vectors.Count;
        vectors.Add(center + halfWidth * left + halfVert * back);
        vectors.Add(center + halfWidth * right + halfVert * back);
        vectors.Add(center + halfVert * forward);
        vectors.Add(center + up * vertLeg);

        triangles.AddRange(new List<int> {
            s + 1, s + 2, s + 0,
            s + 0, s + 2, s + 3,
            s + 2, s + 1, s + 3,
            s + 0, s + 3, s + 1 });
    }

    private static float Sq(float value)
    {
        return value * value;
    }
}
