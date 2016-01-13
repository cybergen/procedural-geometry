using System.Collections.Generic;
using UnityEngine;

public class TetrahedronGenerator : IMeshDataGenerator
{
    public void Draw(
        Vector3 center,
        Vector3 up,
        Vector3 forward,
        MeshData md,
        float width,
        float recursionDepth,
        bool recurseBottom = true)
    {
        var halfWidth = width / 2;
        var vertLeg = Mathf.Sqrt((width * width) - (halfWidth * halfWidth));
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
        var topNoY = bottomLeftMidPoint + dir * vertLeg / 3 - up * 0.1f;
        var top = bottomLeftMidPoint + dir * vertLeg / 3 + up * vertLeg;
        md.verts.Add(top);

        if (recursionDepth == 1)
        {
            md.AnimationOrigins.Add(s + 3, topNoY);
            md.AnimationTargets.Add(s + 3, top);
        }
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

        if (recursionDepth > 1)
        {
            var newDepth = recursionDepth - 1;
            if (recurseBottom) Draw(center, -up, forward, md, width / 2, newDepth, false);

            //Front tri
            var frontForward = top - bottomFrontMidPoint;
            var frontCent = bottomFrontMidPoint + frontForward / 2;
            var frontUp = Vector3.Cross(frontRightPoint - frontLeftPoint, frontForward);

            Draw(frontCent, -frontUp, frontForward, md, width / 2, newDepth, false);

            //Left side tri
            var leftForward = top - bottomLeftMidPoint;
            var leftCent = bottomLeftMidPoint + leftForward / 2;
            var leftUp = Vector3.Cross(bottomTip - frontLeftPoint, leftForward);

            Draw(leftCent, leftUp, leftForward, md, width / 2, newDepth, false);

            //Right side tri
            var rightForward = top - bottomRightMidPoint;
            var rightCent = bottomRightMidPoint + rightForward / 2;
            var rightUp = Vector3.Cross(frontRightPoint - bottomTip, rightForward);
            Draw(rightCent, rightUp, rightForward, md, width / 2, newDepth, false);
        }
    }
}
