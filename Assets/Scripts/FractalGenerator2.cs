using UnityEngine;

public class FractalGenerator2 : MonoBehaviour
{
    public ComputeShader TopologyCompute;
    public ComputeShader NormalCompute;
    public int FractalIterations;
    public Material AttachedMaterial;

    public void Generate()
    {
        var kernel = TopologyCompute.FindKernel("CSMain");
        var normKernel = NormalCompute.FindKernel("CSMain");

        var displace = new RenderTexture(256, 256, 24);
        displace.enableRandomWrite = true;
        displace.Create();
        var normal = new RenderTexture(256, 256, 24);
        normal.enableRandomWrite = true;
        normal.Create();

        TopologyCompute.SetTexture(kernel, "Displace", displace);
        TopologyCompute.Dispatch(kernel, 256 / 8, 256 / 8, 1);

        NormalCompute.SetTexture(normKernel, "Normal", normal);
        NormalCompute.Dispatch(normKernel, 256 / 8, 256 / 8, 1);

        AttachedMaterial.SetTexture("_Displacement", displace);
        AttachedMaterial.SetTexture("_Normal", normal);
    }
}
