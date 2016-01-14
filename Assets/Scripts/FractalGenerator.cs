using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FractalGenerator : MonoBehaviour
{
    public enum GeneratorType
    {
        Triforce = 0,
        Tetrahedron = 1,
        Cube = 3
    }

    public int FractalIterations;
    public float BaseWidth;
    public bool SharpEdges;
    public bool TriforceMode;
    public bool GrowByOne;
    public bool ShrinkByOne;
    public float AnimationTime;
    public bool GrowTillEnd;
    public bool ShrinkTillEnd;
    public bool CycleForever;
    public GeneratorType[] GeneratorList;

    private Action onAnimationComplete;
    private MeshData currentData;
    private float animationTimeElapsed = float.MaxValue;
    private Mesh mesh;
    private bool growingTillEnd;
    private bool shrinkingTillEnd;
    private List<IMeshDataGenerator> generators;
    private int generatorIndex = 0;
    private int maxDepth
    {
        get
        {
            return GeneratorList[generatorIndex] == GeneratorType.Cube ? 5 : 6;
        }
    }

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        SetGenerators();
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        SetGenerators();
    }

    private void SetGenerators()
    {
        generatorIndex = 0;
        generators = new List<IMeshDataGenerator>();
        foreach (var generator in GeneratorList)
        {
            switch (generator)
            {
                case GeneratorType.Triforce:
                    generators.Add(new TriforceGenerator());
                    break;
                case GeneratorType.Tetrahedron:
                    generators.Add(new TetrahedronGenerator());
                    break;
                case GeneratorType.Cube:
                    generators.Add(new CubeGenerator());
                    break;
            }
        }
    }

    public void Initialize()
    {        
        SetGenerators();
        CreateMesh();   
    }   

    public void CreateMesh()
    {
        if (generators.Count < 1) return;
        if (mesh == null) mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.Clear();

        var md = new MeshData();
        generators[0].Draw(Vector3.zero, Vector3.up, Vector3.forward, md, BaseWidth, FractalIterations);

        mesh.vertices = md.verts.ToArray();
        mesh.triangles = md.tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void Update()
    {
        var delta = Time.deltaTime;
        if (GrowByOne)
        {
            GrowByOne = false;
            OnGrowOne();
        }
        else if (ShrinkByOne)
        {
            ShrinkByOne = false;
            OnShrinkOne();
        }
        else if (GrowTillEnd)
        {
            GrowTillEnd = false;
            growingTillEnd = true;
            OnGrowOne();
        }
        else if (ShrinkTillEnd)
        {
            ShrinkTillEnd = false;
            shrinkingTillEnd = true;
            OnShrinkOne();
        }

        if (animationTimeElapsed < AnimationTime)
        {
            animationTimeElapsed += delta;

            if (animationTimeElapsed >= AnimationTime)
            {
                foreach (var key in currentData.AnimationTargets.Keys)
                {
                    currentData.verts[key] = currentData.AnimationTargets[key];
                }

                mesh.vertices = currentData.verts.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                if (onAnimationComplete != null)
                {
                    onAnimationComplete();
                }
            }
            else
            {
                foreach (var key in currentData.AnimationTargets.Keys)
                {
                    var start = currentData.AnimationOrigins[key];
                    var end = currentData.AnimationTargets[key];

                    var x = Circ(animationTimeElapsed, start.x, end.x - start.x, AnimationTime);
                    var y = Circ(animationTimeElapsed, start.y, end.y - start.y, AnimationTime);
                    var z = Circ(animationTimeElapsed, start.z, end.z - start.z, AnimationTime);

                    currentData.verts[key] = new Vector3(x, y, z);
                }

                mesh.vertices = currentData.verts.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }
    }

    private void OnGrowOne()
    {
        var md = new MeshData();
        if (FractalIterations > maxDepth)
        {
            growingTillEnd = false;
            OnGrowthFinished();
            return;
        }

        FractalIterations++;
        animationTimeElapsed = 0f;
        currentData = md;

        onAnimationComplete += OnOneGrowthComplete;

        generators[generatorIndex].Draw(Vector3.zero, Vector3.up, Vector3.forward, md, BaseWidth, FractalIterations);

        foreach (var key in md.AnimationOrigins.Keys)
        {
            md.verts[key] = md.AnimationOrigins[key];
        }

        mesh.vertices = md.verts.ToArray();
        mesh.triangles = md.tris.ToArray();
        mesh.RecalculateNormals();
    }

    private void OnOneGrowthComplete()
    {
        onAnimationComplete -= OnOneGrowthComplete;
        if (growingTillEnd) OnGrowOne();
    }

    private void OnShrinkOne()
    {
        var md = new MeshData();
        if (FractalIterations == 1)
        {
            shrinkingTillEnd = false;
            OnShrinkFinished();
            return;
        }

        onAnimationComplete += OnOneShrinkComplete;

        animationTimeElapsed = 0f;
        currentData = md;

        generators[generatorIndex].Draw(Vector3.zero, Vector3.up, Vector3.forward, md, BaseWidth, FractalIterations);

        var tempTarget = md.AnimationTargets;
        md.AnimationTargets = md.AnimationOrigins;
        md.AnimationOrigins = tempTarget;

        foreach (var key in md.AnimationOrigins.Keys)
        {
            md.verts[key] = md.AnimationOrigins[key];
        }

        mesh.vertices = md.verts.ToArray();
        mesh.triangles = md.tris.ToArray();
        mesh.RecalculateNormals();
    }

    private void OnOneShrinkComplete()
    {
        onAnimationComplete -= OnOneShrinkComplete;
        FractalIterations--;
        CreateMesh();

        if (shrinkingTillEnd) OnShrinkOne();
    }

    private void OnGrowthFinished()
    {
        if (CycleForever)
        {
            ShrinkTillEnd = true;
        }
    }

    private void OnShrinkFinished()
    {
        if (CycleForever)
        {
            generatorIndex++;
            generatorIndex = generatorIndex % generators.Count;
            GrowTillEnd = true;
        }
    }

    private static float Sq(float value)
    {
        return value * value;
    }

    private static float Quartic(float t, float b, float c, float d)
    {
        t /= d;
        return c * t * t * t * t + b;
    }

    private static float Circ(float t, float b, float c, float d)
    {
        t /= d;
        return -c * (Mathf.Sqrt(1 - t * t) - 1) + b;
    }
}
