using UnityEngine;

public class FractalGenerator2 : Generatable
{
    public ComputeShader TopologyCompute;
    public int FractalIterations;
    public int SpiralCount;
    public Material AttachedMaterial;
    public int TextureSize;

    public struct PointData
    {
        float x;
        float y;
        float iteration;
        float radius;
        float logRadius;
        float theta;
        Vector2 spiral;
        Vector2 uv;
        Vector3 stepDisplacement;
        Vector3 totalDirection;

        public override string ToString()
        {
            return string.Format("[PointData x={0}, y={1}, iteration={8},radius={3}, logRadius={4}, theta={5}, spiral={6}, uv={9}, stepDisplacement={2},  totalDir={7}]",
            x, y, stepDisplacement, radius, logRadius, theta, spiral, totalDirection, iteration, uv);
        }
    }

    public override void Generate()
    {
        var kernel = TopologyCompute.FindKernel("CSMain");

        var structBuffer = new PointData[3];
        var size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointData));
        var cb = new ComputeBuffer(structBuffer.Length, size);

        var displace = new RenderTexture(TextureSize, TextureSize, 24);
        displace.enableRandomWrite = true;
        displace.Create();

        var normal = new RenderTexture(TextureSize, TextureSize, 24);
        normal.enableRandomWrite = true;
        normal.Create();

        TopologyCompute.SetBuffer(kernel, "Data", cb);
        TopologyCompute.SetTexture(kernel, "Displace", displace);
        TopologyCompute.SetTexture(kernel, "Normal", displace);
        TopologyCompute.SetInt("Iterations", FractalIterations);
        TopologyCompute.SetInt("SpiralCount", SpiralCount);
        TopologyCompute.SetInt("HalfTextureSize", TextureSize / 2);
        TopologyCompute.Dispatch(kernel, TextureSize / 8, TextureSize / 8, 1);

        AttachedMaterial.SetTexture("_MainTex", displace);
        AttachedMaterial.SetTexture("_Displacement", displace);
        AttachedMaterial.SetTexture("_Normal", normal);

        cb.GetData(structBuffer);

        foreach (var data in structBuffer)
        {
            Debug.Log(data);
        }
    }
}
