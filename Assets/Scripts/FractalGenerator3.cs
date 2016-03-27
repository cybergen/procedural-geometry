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

        for (int i = 0; i < PhyllotaxisCount; i++)
        {
            var angleDeg = i * PhyllotaxisAngle;
            var power = Mathf.Pow(e, B * angleDeg);
            float angle = angleDeg * Mathf.Deg2Rad;
            var normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            normal.Normalize();
            float distance = C * Mathf.Sqrt(i) * power;
            normal *= distance;
            DrawPoint(tex, (int)normal.x + TextureSize / 2, (int)normal.y + TextureSize / 2, Color.black);
        }

        tex.Apply();
        AttachedMaterial.SetTexture("_MainTex", tex);
    }

    private void DrawPoint(Texture2D tex, int pixX, int pixY, Color color)
    {
        tex.SetPixel(pixX, pixY, Color.black);
        tex.SetPixel(pixX - 1, pixY - 1, Color.black);
        tex.SetPixel(pixX - 1, pixY, Color.black);
        tex.SetPixel(pixX - 1, pixY + 1, Color.black);
        tex.SetPixel(pixX, pixY - 1, Color.black);
        tex.SetPixel(pixX, pixY + 1, Color.black);
    }

    private class SpiralBuffer
    {
        private List<Tuple<float, float>> _list = new List<Tuple<float, float>>();

        public void AddEntry(float angle, float distance)
        {
            _list.Add(new Tuple<float, float>(angle, distance));
        }

        public void DrawBuffer(Texture2D tex, int minX, int maxX, int minY, int maxY, Color color)
        {

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
