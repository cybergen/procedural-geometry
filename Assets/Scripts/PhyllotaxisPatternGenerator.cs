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
    public bool DrawPoints;
    public bool DrawVoronoi;

    private List<Color> _colors = new List<Color>();

    public override void Generate()
    {
        Texture2D tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++) tex.SetPixel(x, y, Color.white);
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
        StartBuffer.CalculateScaledPoints(TextureSize, TextureSize / 2, TextureSize / 2);
        StartBuffer.CalculateVoronoi(TextureSize, TextureSize / 2, TextureSize / 2);
        StartBuffer.CalculateChildren(FractalIterations, ChildSizeScale, newPoints);

        foreach (var child in StartBuffer.Points)
        {
            DrawPoint(tex, child.ItemOne, child.ItemTwo, Color.black);
        }

        tex.Apply();
        AttachedMaterial.SetTexture("_MainTex", tex);
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
