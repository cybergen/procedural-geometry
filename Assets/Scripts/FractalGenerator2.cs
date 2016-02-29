using UnityEngine;

public class FractalGenerator2 : Generatable
{
    public ComputeShader TopologyCompute;
    public int FractalIterations;
    public Material AttachedMaterial;

    public override void Generate()
    {
        var kernel = TopologyCompute.FindKernel("CSMain");

        var displace = new RenderTexture(256, 256, 24);
        displace.enableRandomWrite = true;
        displace.Create();
        var normal = new RenderTexture(256, 256, 24);
        normal.enableRandomWrite = true;
        normal.Create();

        TopologyCompute.SetTexture(kernel, "Displace", displace);
        TopologyCompute.Dispatch(kernel, 256 / 8, 256 / 8, 1);

        AttachedMaterial.SetTexture("_Displacement", displace);
    }
}
