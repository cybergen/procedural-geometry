using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator : IMeshDataGenerator
{
    public void Draw(Vector3 center, Vector3 up, Vector3 forward, MeshData md, float width, float depth, bool bottom = true)
    {
        var s = md.verts.Count;

        var halfWidth = width / 2;
        var right = Vector3.Cross(forward, up);
        var left = -right;
        right.Normalize();
        left.Normalize();

        var bottomBackLeft = center + forward * halfWidth + left * halfWidth;
        var bottomBackRight = bottomBackLeft + right * width;
        var bottomFrontRight = bottomBackRight - width * forward;
        var bottomFrontLeft = bottomFrontRight + width * left;
        var topBackLeft = bottomBackLeft + up * width;
        var topBackRight = bottomBackRight + up * width;
        var topFrontRight = bottomFrontRight + up * width;
        var topFrontLeft = bottomFrontLeft + up * width;

        //bottom face 1 = s
        md.verts.Add(bottomBackLeft);
        md.verts.Add(bottomBackRight);
        md.verts.Add(bottomFrontLeft);

        //bottom face 2 = s + 3
        md.verts.Add(bottomBackRight);
        md.verts.Add(bottomFrontRight);
        md.verts.Add(bottomFrontLeft);

        //left face 1 = s + 6
        md.verts.Add(bottomBackLeft);
        md.verts.Add(topBackLeft);
        md.verts.Add(bottomFrontLeft);

        //left face 2 = s + 9
        md.verts.Add(topBackLeft);
        md.verts.Add(topFrontLeft);
        md.verts.Add(bottomFrontLeft);

        //back face 1 = s + 12
        md.verts.Add(bottomBackRight);
        md.verts.Add(topBackRight);
        md.verts.Add(bottomBackLeft);

        //back face 2 = s + 15
        md.verts.Add(topBackRight);
        md.verts.Add(topBackLeft);
        md.verts.Add(bottomBackLeft);

        //right face 1 = s + 18
        md.verts.Add(bottomBackRight);
        md.verts.Add(bottomFrontRight);
        md.verts.Add(topBackRight);

        //right face 2 = s + 21
        md.verts.Add(bottomFrontRight);
        md.verts.Add(topFrontRight);
        md.verts.Add(topBackRight);

        //front face 1 = s + 24
        md.verts.Add(bottomFrontRight);
        md.verts.Add(bottomFrontLeft);
        md.verts.Add(topFrontRight);

        //front face 2 = s + 27
        md.verts.Add(bottomFrontLeft);
        md.verts.Add(topFrontLeft);
        md.verts.Add(topFrontRight);

        //top face 1 = s + 30
        md.verts.Add(topBackLeft);
        md.verts.Add(topBackRight);
        md.verts.Add(topFrontLeft);

        //top face 2 = s + 33
        md.verts.Add(topBackRight);
        md.verts.Add(topFrontRight);
        md.verts.Add(topFrontLeft);

        md.tris.AddRange(new List<int>
        {
            //bottom face 1
            s, s + 1, s + 2,
            //bottom face 2
            s + 5, s + 3, s + 4,
            //left face 1
            s + 8, s + 7, s + 6,
            //left face 2
            s + 11, s + 10, s + 9,
            //back face 1
            s + 14, s + 13, s + 12,
            //back face 2
            s + 17, s + 16, s + 15,
            //right face 1
            s + 20, s + 19, s + 18,
            //right face 2
            s + 23, s + 22, s + 21,
            //front face 1
            s + 26, s + 25, s + 24,
            //front face 2
            s + 29, s + 28, s + 27,
            //top face 1
            s + 32, s + 31, s + 30,
            //top face 2
            s + 35, s + 34, s + 33
        });
    }
}
