using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FractalGenerator : MonoBehaviour
{
    public int FractalIterations;
    public float BaseWidth;
    public bool SharpEdges;
    public bool TriforceMode;

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

        var back = -forward;
        var left = -Vector3.Cross(up, forward);
        var right = -left;

        up.Normalize();
        back.Normalize();
        forward.Normalize();
        left.Normalize();
        right.Normalize();

        var s = vectors.Count;
        var bottomFrontMidPoint = center + halfWidth * back;
        var frontLeftPoint = bottomFrontMidPoint + halfWidth * left;
        vectors.Add(frontLeftPoint);

        var frontRightPoint = frontLeftPoint + halfWidth * 2 * right;
        vectors.Add(frontRightPoint);

        var bottomTip = center + halfVert * forward;
        var bottomLeftMidPoint = frontLeftPoint + (bottomTip - frontLeftPoint) / 2;
        var bottomRightMidPoint = bottomTip + (frontRightPoint - bottomTip) / 2;
        vectors.Add(bottomTip);

        var dir = frontRightPoint - bottomLeftMidPoint;
        dir.Normalize();
        var top = bottomLeftMidPoint + dir * vertLeg / 3 + up * vertLeg;
        vectors.Add(top);

        if (SharpEdges)
        {
            vectors.Add(frontLeftPoint);
            vectors.Add(frontRightPoint);
            vectors.Add(bottomTip);
            vectors.Add(top);

            vectors.Add(frontLeftPoint);
            vectors.Add(frontRightPoint);
            vectors.Add(bottomTip);
            vectors.Add(top);

            vectors.Add(frontLeftPoint);
            vectors.Add(frontRightPoint);
            vectors.Add(bottomTip);
            vectors.Add(top);

            triangles.AddRange(new List<int> {
                s + 1, s + 2, s + 0,
                s + 4, s + 6, s + 7,
                s + 10, s + 9, s + 11,
                s + 12, s + 15, s + 13 });
        }
        else
        {
            triangles.AddRange(new List<int> {
                s + 1, s + 2, s + 0,
                s + 0, s + 2, s + 3,
                s + 2, s + 1, s + 3,
                s + 0, s + 3, s + 1 });
        }
        
        if (recursionDepth > 1)
        {
            var newDepth = recursionDepth - 1;
            if (recurseBottom) Draw(center, -up, forward, vectors, triangles, width / 2, newDepth, false);

            //Front tri
            var frontForward = top - bottomFrontMidPoint;
            var frontCent = bottomFrontMidPoint + frontForward / 2;
            var frontUp = Vector3.Cross(frontRightPoint - frontLeftPoint, frontForward);

            if (!TriforceMode)
            {
                Draw(frontCent, -frontUp, frontForward, vectors, triangles, width / 2, newDepth, false);
            }
            else
            {
                frontCent = bottomFrontMidPoint + frontForward / 4.3f;
                Draw(frontCent, -frontUp, -frontForward, vectors, triangles, width / 2, newDepth, false);
            }

            //Left side tri
            var leftForward = top - bottomLeftMidPoint;
            var leftCent = bottomLeftMidPoint + leftForward / 2;
            var leftUp = Vector3.Cross(bottomTip - frontLeftPoint, leftForward);
            if (!TriforceMode)
            {
                Draw(leftCent, leftUp, leftForward, vectors, triangles, width / 2, newDepth, false);
            }
            else
            {
                leftCent = bottomLeftMidPoint + leftForward / 4.3f;
                Draw(leftCent, leftUp, -leftForward, vectors, triangles, width / 2, newDepth, false);
            }

            //Right side tri
            var rightForward = top - bottomRightMidPoint;
            var rightCent = bottomRightMidPoint + rightForward / 2;
            var rightUp = Vector3.Cross(frontRightPoint - bottomTip, rightForward);
            if (!TriforceMode)
            {
                Draw(rightCent, rightUp, rightForward, vectors, triangles, width / 2, newDepth, false);
            }
            else
            {
                rightCent = bottomRightMidPoint + rightForward / 4.3f;
                Draw(rightCent, rightUp, -rightForward, vectors, triangles, width / 2, newDepth, false);
            }
        }
    }

    private static float Sq(float value)
    {
        return value * value;
    }
}
