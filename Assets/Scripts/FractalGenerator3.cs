using System;
using System.Collections.Generic;
using UnityEngine;

public class FractalGenerator3 : Generatable
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
    public bool DrawPoints;
    public bool DrawVoronoi;

    public override void Generate()
    {
        //var kernel = TopologyCompute.FindKernel("CSMain");        

        //var displace = new RenderTexture(TextureSize, TextureSize, 24);
        //displace.enableRandomWrite = true;
        //displace.Create();

        //var normal = new RenderTexture(TextureSize, TextureSize, 24);
        //normal.enableRandomWrite = true;
        //normal.Create();

        //TopologyCompute.SetTexture(kernel, "Displace", displace);
        //TopologyCompute.SetTexture(kernel, "Normal", normal);
        //TopologyCompute.SetInt("Iterations", FractalIterations);
        //TopologyCompute.SetInt("SpiralCount", SpiralCount);
        //TopologyCompute.SetInt("HalfTextureSize", TextureSize / 2);
        //TopologyCompute.Dispatch(kernel, TextureSize / 8, TextureSize / 8, 1);

        ////AttachedMaterial.SetTexture("_MainTex", normal);
        ////AttachedMaterial.SetTexture("_Displacement", displace);
        //AttachedMaterial.SetTexture("_Normal", normal);

        Texture2D tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++) tex.SetPixel(x, y, Color.white);
        }

        float e = 2.71828f;

        //for (int theta = 0; theta < 420; theta += SpiralStep)
        //{
        //    var power = Mathf.Pow(e, b * theta);

        //    for (int i = 0; i < SpiralCount; i++)
        //    {
        //        var angle = theta + (360f / SpiralCount * i) * Mathf.Deg2Rad;
        //        var x = a * Mathf.Cos(angle) * power;
        //        var y = a * Mathf.Sin(angle) * power;
        //        var pixX = (int)x + TextureSize / 2;
        //        var pixY = (int)y + TextureSize / 2;
        //        DrawPoint(tex, pixX, pixY, Color.black);
        //    }
        //}

        var buffer = new SpiralBuffer();

        for (int i = 0; i < PhyllotaxisCount; i++)
        {
            var angleDeg = i * PhyllotaxisAngle;
            var power = Mathf.Pow(e, B * angleDeg);
            float angle = angleDeg * Mathf.Deg2Rad;
            var normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            normal.Normalize();
            float distance = C * Mathf.Sqrt(i) * power;
            normal *= distance;
            var x = (int)normal.x + TextureSize / 2;
            var y = (int)normal.y + TextureSize / 2;
            buffer.AddEntry(x, y);
            //DrawPoint(tex, x, y, Color.black);
        }
        if (DrawVoronoi) buffer.DrawVoronoi(tex);
        if (DrawPoints) buffer.DrawPoints(tex, Color.black);

        tex.Apply();
        AttachedMaterial.SetTexture("_MainTex", tex);
    }    

    private class SpiralBuffer
    {
        private List<Tuple<int, int>> _list = new List<Tuple<int, int>>();
        private List<Color> _colors = new List<Color>();
        private int _lowX = int.MaxValue;
        private int _highX = int.MinValue;
        private int _lowY = int.MaxValue;
        private int _highY = int.MinValue;
        private System.Random _rand = new System.Random();

        public void AddEntry(int x, int y)
        {
            _lowX = Mathf.Min(x, _lowX);
            _highX = Mathf.Max(x, _highX);
            _lowY = Mathf.Min(y, _lowY);
            _highY = Mathf.Max(y, _highY);
            _list.Add(new Tuple<int, int>(x, y));
            _colors.Add(GetRandomColor());
        }

        public void DrawOriented(Texture2D tex, int minX, int maxX, int minY, int maxY, Color color)
        {

        }

        public void DrawPoints(Texture2D tex, Color color)
        {
            foreach (var entry in _list)
            {
                DrawPoint(tex, entry.ItemOne, entry.ItemTwo, color);
            }
        }

        public void DrawVoronoi(Texture2D tex)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, _colors[GetNearestNeighbor(x, y)]);
                }
            }
        }

        private int GetNearestNeighbor(int x, int y)
        {
            int previousClosest = 0;
            float previousDistance = float.MaxValue;
            var point = new Vector2(x, y);
            for (int i = 0; i < _list.Count; i++)
            {
                var entry = _list[i];
                var neighbor = new Vector2(entry.ItemOne, entry.ItemTwo);
                var dist = Vector2.Distance(neighbor, point);
                if (dist < previousDistance)
                {
                    previousClosest = i;
                    previousDistance = dist;
                }
            }
            return previousClosest;
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
            var r = _rand.Next(256) / 256f;
            var g = _rand.Next(256) / 256f;
            var b = _rand.Next(256) / 256f;
            return new Color(r, g, b);
        }
    }

    private struct Tuple<T, K>
    {
        public T ItemOne;
        public K ItemTwo;

        public Tuple(T itemOne, K itemTwo)
        {
            ItemOne = itemOne;
            ItemTwo = itemTwo;
        }
    }

    private struct Tuple<T, K, L>
    {
        public T ItemOne;
        public K ItemTwo;
        public L ItemThree;

        public Tuple(T itemOne, K itemTwo, L itemThree)
        {
            ItemOne = itemOne;
            ItemTwo = itemTwo;
            ItemThree = itemThree;
        }
    }
}
