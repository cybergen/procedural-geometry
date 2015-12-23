using System.Collections.Generic;
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
        Draw(Vector3.zero, Vector3.up, Vector3.forward, vertices, triangles, BaseWidth, FractalIterations);
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
        float width,
        float recursionDepth,
        bool recurseBottom = true)
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
        var bottomFrontMidPoint = center + halfWidth * back;
        var frontLeftPoint = center + halfWidth * left + halfVert * back;
        vectors.Add(frontLeftPoint);
        var frontRightPoint = center + halfWidth * right + halfVert * back;
        vectors.Add(frontRightPoint);
        var bottomTip = center + halfVert * forward;
        var bottomLeftMidPoint = frontLeftPoint + (bottomTip - frontLeftPoint) / 2;
        var bottomRightMidPoint = bottomTip + (frontRightPoint - bottomTip) / 2;
        vectors.Add(bottomTip);
        var top = center + up * vertLeg;
        vectors.Add(top);

        triangles.AddRange(new List<int> {
            s + 1, s + 2, s + 0,
            s + 0, s + 2, s + 3,
            s + 2, s + 1, s + 3,
            s + 0, s + 3, s + 1 });
        
        if (recursionDepth > 1)
        {
            var newDepth = recursionDepth - 1;
            if (recurseBottom) Draw(center, -up, forward, vectors, triangles, width / 2, newDepth, false);

            //Front tri
            var frontForward = top - bottomFrontMidPoint;
            var frontCent = bottomFrontMidPoint + frontForward / 2;
            var frontUp = Vector3.Cross(frontRightPoint - frontLeftPoint, frontForward);
            Draw(frontCent, -frontUp, frontForward, vectors, triangles, width / 2, newDepth, false);

            //Left side tri

            //Right side tri
        }
    }

    private static float Sq(float value)
    {
        return value * value;
    }
}
