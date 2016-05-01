using System.Collections.Generic;
using UnityEngine;
using BriLib;

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

    private List<Color> _colors = new List<Color>();

    public override void Generate()
    {
        var ts = TextureSize;
        var debugPoints = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
        var displaceMap = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
        var normalMap = new Texture2D(ts, ts, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < ts; y++)
        {
            for (int x = 0; x < ts; x++)
            {
                debugPoints.SetPixel(x, y, Color.white);
                displaceMap.SetPixel(x, y, new Color(0.5f, 0.5f, 0.5f));
                normalMap.SetPixel(x, y, new Color(0.5f, 1f, 0.5f));
            }
        }

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

        var StartBuffer = new ScalablePointBuffer();
        StartBuffer.AddEntry(0, 0, 0);
        StartBuffer.CalculateScaledPoints(ts, ts / 2, ts / 2);        

        var depth = FractalIterations;
        foreach (var child in StartBuffer.Points)
        {
            depth--;
            if (DrawDebugPoints)
            {
                DrawPoint(debugPoints, child.ItemOne, child.ItemTwo, Color.black);
            }

            if (DrawCones)
            {
                var size = StartBuffer.Size / 2;

                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float maxZ = float.MinValue;
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float minZ = float.MaxValue;

                for (int y = 0; y < ts; y++)
                {
                    for (int x = 0; x < ts; x++)
                    {
                        var xNess = ((x - child.ItemOne) / size);
                        var yNess = GetHeight(x, y, child.ItemOne, child.ItemTwo, size);
                        var zNess = ((y - child.ItemTwo) / size);

                        maxX = Mathf.Max(xNess, maxX);
                        maxY = Mathf.Max(yNess, maxY);
                        maxZ = Mathf.Max(zNess, maxZ);

                        minX = Mathf.Min(xNess, minX);
                        minY = Mathf.Min(yNess, minY);
                        minZ = Mathf.Min(zNess, minZ);

                        var displace = new Vector3(xNess, yNess, zNess);
                        displace /= -2;
                        displace += new Vector3(0.5f, 0.5f, 0.5f);
                        var col = new Color(displace.x, displace.y, displace.z);
                        displace.Normalize();
                        var norm = new Color(displace.x, displace.y, displace.z);
                        displaceMap.SetPixel(x, y, col);
                        normalMap.SetPixel(x, y, norm);
                    }
                }

                Debug.Log("Max x: " + maxX + ", y: " + maxY + ", z: " + maxZ);
                Debug.Log("Min x: " + minX + ", y: " + minY + ", z: " + minZ);
            }

            if (depth >= 1)
            {
                var buffer = StartBuffer.GetChild(ChildSizeScale, child.ItemOne, child.ItemTwo, newPoints);
                if (DrawDebugPoints)
                {
                    DrawPoints(depth, debugPoints, buffer, ChildSizeScale);
                }
            }
        }

        if (DrawDebugPoints)
        {
            debugPoints.Apply();
            AttachedMaterial.SetTexture("_MainTex", debugPoints);
        }

        if (DrawCones)
        {
            displaceMap.Apply();
            normalMap.Apply();
            AttachedMaterial.SetTexture("_Displacement", displaceMap);
            AttachedMaterial.SetTexture("_Normal", normalMap);
        }
    }

    private float GetHeight(int x, int y, int centerX, int centerY, float cellSize)
    {
        float totalScale = cellSize * 2 / TextureSize;
        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
        var hypotenuse = Mathf.Sqrt(cellSize.Sq() + cellSize.Sq());
        float distScale = (hypotenuse - distance) / hypotenuse;
        var returnValue = distScale * totalScale;
        return returnValue;
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
