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

        TopologyCompute.SetTexture(kernel, "Displace", displace);
        TopologyCompute.SetInt("Iterations", FractalIterations);
        TopologyCompute.SetInt("SpiralCount", SpiralCount);
        TopologyCompute.Dispatch(kernel, TextureSize / 8, TextureSize / 8, 1);

        AttachedMaterial.SetTexture("_MainTex", displace);
        AttachedMaterial.SetTexture("_Displacement", displace);
    }
}
