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
            buffer.AddEntry(normal.x, normal.y, distance);
        }
        if (DrawVoronoi) buffer.DrawVoronoi(tex);
        if (DrawPoints) buffer.DrawOriented(tex, (float)TextureSize, Color.black, TextureSize / 2, TextureSize / 2, FractalIterations);

        tex.Apply();
        AttachedMaterial.SetTexture("_MainTex", tex);
    }    

    private class SpiralBuffer
    {
        private List<Tuple<float, float, float>> _list = new List<Tuple<float, float, float>>();
        private int[,] _voronoiField;
        private float _maxRange = float.MinValue;
        private List<Color> _colors = new List<Color>();
        private System.Random _rand = new System.Random();

        public void AddEntry(float x, float y, float dist)
        {
            _list.Add(new Tuple<float, float, float>(x / 2f, y / 2f, dist));
            _maxRange = Mathf.Max(_maxRange, dist);
            _colors.Add(GetRandomColor());
        }

        public void DrawOriented(Texture2D tex, float size, Color color, int centerX, int centerY, int depth)
        {
            int previousX = centerX;
            int previousY = centerY;
            foreach (var entry in _list)
            {
                var sizeMult = size * entry.ItemThree / _maxRange;
                var x = (int)(entry.ItemOne * sizeMult + centerX);
                var y = (int)(entry.ItemTwo * sizeMult + centerY);
                DrawPoint(tex, x, y, color);
                if (depth > 1)
                {
                    var dist = Mathf.Sqrt(Mathf.Pow(x - previousX, 2) + Mathf.Pow(y - previousY, 2));
                    DrawOriented(tex, dist, color, x, y, depth - 1);
                }
                previousX = x;
                previousY = y;
            }
        }

        public void DrawPoints(Texture2D tex, Color color)
        {
            foreach (var entry in _list)
            {
                DrawPoint(tex, (int)entry.ItemOne, (int)entry.ItemTwo, color);
            }
        }

        public void DrawVoronoi(Texture2D tex)
        {
            _voronoiField = new int[tex.width, tex.height];
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    var neighbor = GetNearestNeighbor(x, y);
                    _voronoiField[x, y] = neighbor;
                    tex.SetPixel(x, y, _colors[neighbor]);
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
