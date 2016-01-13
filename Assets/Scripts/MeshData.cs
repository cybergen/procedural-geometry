using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> verts = new List<Vector3>();
    public List<int> tris = new List<int>();
    public Dictionary<int, Vector3> AnimationTargets = new Dictionary<int, Vector3>();
    public Dictionary<int, Vector3> AnimationOrigins = new Dictionary<int, Vector3>();
}
