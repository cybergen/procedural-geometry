using System.Collections.Generic;
using UnityEngine;
using BriLib;
using Buffer = ScalablePointBuffer;

public class PhyllotaxisPatternGenerator : Generatable
{
    public ComputeShader TopologyCompute;
    public int FractalIterations;
    public int SpiralCount;
    public Material AttachedMaterial;
    public int TextureSize;
    public int SpiralStep;
    public float A;
    public float B;
    public float C;
    public int PhyllotaxisCount;
    public float PhyllotaxisAngle;
    public float ChildSizeScale;
    public float GeometryHeightScale = 1f;

    public bool DrawDebugPoints;
    public bool DrawCones;

    public override void Generate()
    {
        //Initialization of variables for storing point data
        var displaceArray = new Vector3[TextureSize, TextureSize];
        var normalArray = new Vector3[TextureSize, TextureSize];

        //Texture initializaition
        var ts = TextureSize;
        var debugPoints = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
        var displaceMap = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
        var normalMap = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };

        var plusVect = new Vector3(0.5f, 0.5f, 0.5f);
        var upVector = Vector3.up;
        upVector /= 2;
        upVector += plusVect;

        for (int y = 0; y < ts; y++)
        {
            for (int x = 0; x < ts; x++)
            {
                debugPoints.SetPixel(x, y, Color.white);
                displaceMap.SetPixel(x, y, new Color(0.5f, 0.5f, 0.5f));
                normalMap.SetPixel(x, y, new Color(0.5f, 1f, 0.5f));
                normalArray[x, y] = Vector3.up;
                displaceArray[x, y] = Vector3.zero;
            }
        }

        //Phyllotaxis pattern generation
        float e = 2.71828f;
        List<Tuple<float, float, float>> newPoints = new List<Tuple<float, float, float>>();

        for (int i = 0; i < PhyllotaxisCount; i++)
        {
            var angleDeg = i * PhyllotaxisAngle;
            var power = Mathf.Pow(e, B * angleDeg);
            float angle = angleDeg * Mathf.Deg2Rad;
            var normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            normal.Normalize();
            float distance = C * Mathf.Sqrt(i) * power;
            newPoints.Add(new Tuple<float, float, float>(normal.x, normal.y, distance));
        }

        //Creation of the first buffer
        var StartBuffer = new Buffer();
        StartBuffer.AddEntry(0, 0, 0);
        StartBuffer.CalculateScaledPoints(ts, ts / 2, ts / 2);

        ProcessBuffers(
            StartBuffer,
            StartBuffer.Size / 2,
            displaceArray,
            normalArray,
            FractalIterations,
            debugPoints,
            TextureSize / 2,
            TextureSize / 2,
            newPoints);

        //DrawCone(512, 512, 300, 0.5f, displaceArray, normalArray);
        //DrawCone(662, 512, 100, 0.2f, displaceArray, normalArray);

        if (DrawDebugPoints)
        {
            debugPoints.Apply();
            AttachedMaterial.SetTexture("_MainTex", debugPoints);
        }

        if (DrawCones)
        {
            //Write array values into textures
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    var displace = displaceArray[x, y];
                    displace /= 2;
                    displace += plusVect;
                    var col = new Color(displace.x, displace.y, displace.z);
                    displaceMap.SetPixel(x, y, col);

                    var normVector = normalArray[x, y];
                    normVector /= 2;
                    normVector += plusVect;
                    var normCol = new Color(normVector.x, normVector.y, normVector.z);
                    normalMap.SetPixel(x, y, normCol);
                }
            }

            displaceMap.Apply();
            normalMap.Apply();
            AttachedMaterial.SetTexture("_Displacement", displaceMap);
            AttachedMaterial.SetTexture("_Normal", normalMap);
        }
    }

    private void DrawCone(int centerX, int centerY, int radius, float scale, Vector3[,] displace, Vector3[,] normalMap)
    {
        var startX = Mathf.Max(0, centerX - radius);
        var startY = Mathf.Max(0, centerY - radius);
        var endX = Mathf.Min(TextureSize, centerX + radius);
        var endY = Mathf.Min(TextureSize, centerY + radius);
        var center = new Vector2(centerX, centerY);

        var oldCenterVector = new Vector3(centerX / TextureSize, 0, centerY / TextureSize) + displace[centerX, centerY];
        var localUp = normalMap[centerX, centerY];

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                var dist = Vector2.Distance(center, new Vector2(x, y));
                if (dist < radius)
                {
                    var distScale = (radius - dist) / radius * scale;
                    var displaceVect = new Vector3(0, distScale, 0);
                    displace[x, y] += distScale * localUp;

                    var xNess = (float)(x - centerX) / TextureSize;
                    var zNess = (float)(y - centerY) / TextureSize;
                    var normal = new Vector3(xNess, distScale, zNess) - oldCenterVector;
                    normal.Normalize();
                    normalMap[x, y] = normal;
                }
            }
        }
    }

    private Quaternion GetDeviationFromUp(Vector3 localUp)
    {
        return Quaternion.LookRotation(new Vector3(0, 0, 1), localUp);
    }

    private void ProcessBuffers(
        Buffer buff, 
        float size, 
        Vector3[,] displaceArray, 
        Vector3[,] normArray, 
        int depth, 
        Texture2D debugPoints, 
        int centerX, 
        int centerY, 
        List<Tuple<float, float, float>> points = null)
    {
        depth--;
        var ts = TextureSize;
        var up = Vector3.up;
        Debug.Log("Launching process buffers with center x: " + centerX + ", center Y: " + centerY + ", size: " + size);

        for (int y = 0; y < 1024; y++)
        {
            for (int x = 0; x < 1024; x++)
            {
                displaceArray[x, y] = Vector3.zero;
            }
        }


        foreach (var child in buff.Points)
        {
            Debug.Log("Examining point with x: " + child.ItemOne + ", y: " + child.ItemTwo + ", and range: " + child.ItemThree);

            if (DrawDebugPoints)
            {
                DrawPoint(debugPoints, child.ItemOne, child.ItemTwo, Color.black);
            }

            if (DrawCones)
            {
                var childSize = child.ItemThree == 0 ? size : child.ItemThree;

                var startY = System.Math.Max(child.ItemTwo - childSize, 0);
                var startX = System.Math.Max(child.ItemOne - childSize, 0);
                var endY = System.Math.Min(child.ItemTwo + childSize, TextureSize);
                var endX = System.Math.Min(child.ItemOne + childSize, TextureSize);

                for (int y = (int)startY; y < endY; y++)
                {
                    for (int x = (int)startX; x < endX; x++)
                    {
                        var xNess = -1; //((x - child.ItemOne) / -size);
                        var yNess = GetHeight(x, y, child.ItemOne, child.ItemTwo, child.ItemThree == 0 ? size : child.ItemThree);
                        var zNess = -1; //((y - child.ItemTwo) / -size);

                        var displacePoint = new Vector3(xNess, yNess, zNess);

                        //Re-oriented normal mapping - rotate the new point about prior normal for this position
                        //var baseNorm = normArray[x, y];
                        //Vector3 newDisp = Vector3.down;

                        //if (baseNorm == up)
                        //{
                        //    newDisp = displacePoint;
                        //}
                        //else
                        //{
                        //    Quaternion q;
                        //    var cross = Vector3.Cross(up, baseNorm);
                        //    var w = Mathf.Sqrt(up.sqrMagnitude) * (baseNorm.sqrMagnitude) + Vector3.Dot(up, baseNorm);
                        //    q = new Quaternion(cross.x, cross.y, cross.z, w);

                        //    newDisp = q * displacePoint;
                        //}

                        //displaceArray[x, y] = newDisp;
                        displaceArray[x, y] += displacePoint;

                        //newDisp.Normalize();
                        //normArray[x, y] = newDisp;
                        displacePoint.Normalize();
                        normArray[x, y] = displacePoint;
                    }
                }
            }

            if (depth >= 1)
            {
                var buffer = buff.GetChild(ChildSizeScale, child.ItemOne, child.ItemTwo, points);
                if (DrawDebugPoints)
                {
                    ProcessBuffers(
                        buffer, 
                        buffer.Size / 2, 
                        displaceArray, 
                        normArray, 
                        depth, 
                        debugPoints, 
                        buffer.CenterX, 
                        buffer.CenterY, 
                        points);
                }
            }
        }
    }

    private float GetHeight(int x, int y, int centerX, int centerY, float cellSize)
    {
        float totalScale = cellSize / (TextureSize / 2);

        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
        var hypotenuse = Mathf.Sqrt(cellSize.Sq() + cellSize.Sq());
        float distScale = (hypotenuse - distance) / hypotenuse;

        return distScale * totalScale;
    }

    private void DrawPoints(int depth, Texture2D tex, ScalablePointBuffer startBuffer, float scale)
    {
        foreach (var point in startBuffer.Points)
        {
            DrawPoint(tex, point.ItemOne, point.ItemTwo, Color.black);
            if (depth > 1)
            {
                DrawPoints(depth - 1, tex, startBuffer.GetChild(scale, point.ItemOne, point.ItemTwo), scale);
            }
        }
    }

    private void DrawPoint(Texture2D tex, int pixX, int pixY, Color color)
    {
        tex.SetPixel(pixX, pixY, color);
        tex.SetPixel(pixX - 1, pixY - 1, color);
        tex.SetPixel(pixX - 1, pixY, color);
        tex.SetPixel(pixX - 1, pixY + 1, color);
        tex.SetPixel(pixX, pixY - 1, color);
        tex.SetPixel(pixX, pixY + 1, color);
    }

    private Color GetRandomColor()
    {
        var r = PerlinNoise.ScaledRandom(256) / 256f;
        var g = PerlinNoise.ScaledRandom(256) / 256f;
        var b = PerlinNoise.ScaledRandom(256) / 256f;
        return new Color(r, g, b);
    }
}
