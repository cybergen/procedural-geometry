using UnityEngine;

public class FractalGenerator2 : Generatable
{
    public ComputeShader TopologyCompute;
    public int FractalIterations;
    public int SpiralCount;
    public Material AttachedMaterial;
    public int TextureSize;

    public override void Generate()
    {
        var kernel = TopologyCompute.FindKernel("CSMain");

        var displace = new RenderTexture(TextureSize, TextureSize, 24);
        displace.enableRandomWrite = true;
        displace.Create();

        var normal = new RenderTexture(TextureSize, TextureSize, 24);
        normal.enableRandomWrite = true;
        normal.Create();

        TopologyCompute.SetTexture(kernel, "Displace", displace);
        TopologyCompute.SetTexture(kernel, "Normal", displace);
        TopologyCompute.SetInt("Iterations", FractalIterations);
        TopologyCompute.SetInt("SpiralCount", SpiralCount);
        TopologyCompute.SetInt("HalfTextureSize", TextureSize / 2);
        TopologyCompute.Dispatch(kernel, TextureSize / 8, TextureSize / 8, 1);

        AttachedMaterial.SetTexture("_MainTex", displace);
        AttachedMaterial.SetTexture("_Displacement", displace);
        AttachedMaterial.SetTexture("_Normal", normal);
    }
}
