using System;
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
    public bool GrowByOne;
    public float AnimationTime;
    public bool GrowTillEnd;

    private Action onAnimationComplete;
    private MeshData currentData;
    private float animationTimeElapsed = float.MaxValue;
    private Mesh mesh;
    private bool growingTillEnd;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    public void CreateMesh()
    {
        if (mesh == null) mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.Clear();

        var md = new MeshData();
        Draw(Vector3.zero, Vector3.up, Vector3.forward, md, BaseWidth, FractalIterations);

        mesh.vertices = md.verts.ToArray();
        mesh.triangles = md.tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void Draw(
        Vector3 center, 
        Vector3 up, 
        Vector3 forward,
        MeshData md, 
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

        var s = md.verts.Count;
        var bottomFrontMidPoint = center + halfWidth * back;
        var frontLeftPoint = bottomFrontMidPoint + halfWidth * left;
        md.verts.Add(frontLeftPoint);

        var frontRightPoint = frontLeftPoint + halfWidth * 2 * right;
        md.verts.Add(frontRightPoint);

        var bottomTip = center + halfVert * forward;
        var bottomLeftMidPoint = frontLeftPoint + (bottomTip - frontLeftPoint) / 2;
        var bottomRightMidPoint = bottomTip + (frontRightPoint - bottomTip) / 2;
        md.verts.Add(bottomTip);

        var dir = frontRightPoint - bottomLeftMidPoint;
        dir.Normalize();
        var topNoY = bottomLeftMidPoint + dir * vertLeg / 3;
        var top = topNoY + up * vertLeg;
        md.verts.Add(top);

        if (recursionDepth == 1)
        {
            md.AnimationOrigins.Add(s + 3, topNoY);
            md.AnimationTargets.Add(s + 3, top);
        }

        if (SharpEdges)
        {
            md.verts.Add(frontLeftPoint);
            md.verts.Add(frontRightPoint);
            md.verts.Add(bottomTip);

            md.verts.Add(top);
            if (recursionDepth == 1)
            {
                md.AnimationOrigins.Add(s + 7, topNoY);
                md.AnimationTargets.Add(s + 7, top);
            }
            
            md.verts.Add(frontLeftPoint);
            md.verts.Add(frontRightPoint);
            md.verts.Add(bottomTip);
            md.verts.Add(top);
            if (recursionDepth == 1)
            {
                md.AnimationOrigins.Add(s + 11, topNoY);
                md.AnimationTargets.Add(s + 11, top);
            }

            md.verts.Add(frontLeftPoint);
            md.verts.Add(frontRightPoint);
            md.verts.Add(bottomTip);
            md.verts.Add(top);
            if (recursionDepth == 1)
            {
                md.AnimationOrigins.Add(s + 15, topNoY);
                md.AnimationTargets.Add(s + 15, top);
            }

            md.tris.AddRange(new List<int> {
                s + 1, s + 2, s + 0,
                s + 4, s + 6, s + 7,
                s + 10, s + 9, s + 11,
                s + 12, s + 15, s + 13 });
        }
        else
        {
            md.tris.AddRange(new List<int> {
                s + 1, s + 2, s + 0,
                s + 0, s + 2, s + 3,
                s + 2, s + 1, s + 3,
                s + 0, s + 3, s + 1 });
        }
        
        if (recursionDepth > 1)
        {
            var newDepth = recursionDepth - 1;
            if (recurseBottom) Draw(center, -up, forward, md, width / 2, newDepth, false);

            //Front tri
            var frontForward = top - bottomFrontMidPoint;
            var frontCent = bottomFrontMidPoint + frontForward / 2;
            var frontUp = Vector3.Cross(frontRightPoint - frontLeftPoint, frontForward);

            if (!TriforceMode)
            {
                Draw(frontCent, -frontUp, frontForward, md, width / 2, newDepth, false);
            }
            else
            {
                frontCent = bottomFrontMidPoint + frontForward / 4.3f;
                Draw(frontCent, -frontUp, -frontForward, md, width / 2, newDepth, false);
            }

            //Left side tri
            var leftForward = top - bottomLeftMidPoint;
            var leftCent = bottomLeftMidPoint + leftForward / 2;
            var leftUp = Vector3.Cross(bottomTip - frontLeftPoint, leftForward);
            if (!TriforceMode)
            {
                Draw(leftCent, leftUp, leftForward, md, width / 2, newDepth, false);
            }
            else
            {
                leftCent = bottomLeftMidPoint + leftForward / 4.3f;
                Draw(leftCent, leftUp, -leftForward, md, width / 2, newDepth, false);
            }

            //Right side tri
            var rightForward = top - bottomRightMidPoint;
            var rightCent = bottomRightMidPoint + rightForward / 2;
            var rightUp = Vector3.Cross(frontRightPoint - bottomTip, rightForward);
            if (!TriforceMode)
            {
                Draw(rightCent, rightUp, rightForward, md, width / 2, newDepth, false);
            }
            else
            {
                rightCent = bottomRightMidPoint + rightForward / 4.3f;
                Draw(rightCent, rightUp, -rightForward, md, width / 2, newDepth, false);
            }
        }
    }

    private void Update()
    {
        var delta = Time.deltaTime;
        if (GrowByOne)
        {
            GrowByOne = false;
            OnGrowOne();
        }
        else if (GrowTillEnd)
        {
            GrowTillEnd = false;
            growingTillEnd = true;
            OnGrowOne();
        }

        if (animationTimeElapsed < AnimationTime)
        {
            animationTimeElapsed += delta;

            if (animationTimeElapsed >= AnimationTime)
            {
                foreach (var key in currentData.AnimationTargets.Keys)
                {
                    currentData.verts[key] = currentData.AnimationTargets[key];
                }

                mesh.vertices = currentData.verts.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                if (growingTillEnd)
                {
                    OnGrowOne();
                }
            }
            else
            {
                foreach (var key in currentData.AnimationTargets.Keys)
                {
                    var start = currentData.AnimationOrigins[key];
                    var end = currentData.AnimationTargets[key];
                    currentData.verts[key] = Vector3.Lerp(start, end, animationTimeElapsed / AnimationTime);
                }

                mesh.vertices = currentData.verts.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }
    }

    private void OnGrowOne()
    {
        var md = new MeshData();
        if (FractalIterations > 6)
        {
            growingTillEnd = false;
            return;
        }

        FractalIterations++;
        animationTimeElapsed = 0f;
        currentData = md;

        Draw(Vector3.zero, Vector3.up, Vector3.forward, md, BaseWidth, FractalIterations);

        foreach (var key in md.AnimationOrigins.Keys)
        {
            md.verts[key] = md.AnimationOrigins[key];
        }

        mesh.vertices = md.verts.ToArray();
        mesh.triangles = md.tris.ToArray();
        mesh.RecalculateNormals();
    }

    private static float Sq(float value)
    {
        return value * value;
    }

    private class MeshData
    {
        public List<Vector3> verts = new List<Vector3>();
        public List<int> tris = new List<int>();
        public Dictionary<int, Vector3> AnimationTargets = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3> AnimationOrigins = new Dictionary<int, Vector3>();
    }
}
