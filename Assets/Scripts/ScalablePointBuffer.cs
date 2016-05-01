
using System.Collections.Generic;
using UnityEngine;
using BriLib;

public class ScalablePointBuffer : IVoronoiProvider
{
    public float Size { get; private set; }
    public int CenterX { get; private set; }
    public int CenterY { get; private set; }

    private List<Tuple<float, float, float>> _unscaledList = new List<Tuple<float, float, float>>();
    private List<Tuple<int, int, float>> _scaledList = new List<Tuple<int, int, float>>();
    private float _maxRange = float.MinValue;
    private int _width;
    private int _height;

    public void AddEntry(float x, float y, float dist)
    {
        _unscaledList.Add(new Tuple<float, float, float>(x / 2f, y / 2f, dist));
        _maxRange = Mathf.Max(_maxRange, dist);
    }

    public void CalculateScaledPoints(float size, int centerX, int centerY)
    {
        int previousX = CenterX = centerX;
        int previousY = CenterY = centerY;
        float realMax = Mathf.Max(0.5f, _maxRange);
        _width = (int)(size * realMax + centerX);
        _height = (int)(size * realMax + centerY);
        Size = size;
        foreach (var entry in _unscaledList)
        {
            var sizeMult = size * entry.ItemThree / realMax;
            var x = (int)(entry.ItemOne * sizeMult + centerX);
            var y = (int)(entry.ItemTwo * sizeMult + centerY);

            var scale = Mathf.Sqrt(Mathf.Pow(x - previousX, 2) + Mathf.Pow(y - previousY, 2));
            _scaledList.Add(new Tuple<int, int, float>(x, y, scale));

            previousX = x;
            previousY = y;
        }
    }

    public void RestrainToVoronoi(int targetIndex, IVoronoiProvider voronoiSource)
    {
        var removeList = new List<Tuple<int, int, float>>();
        foreach (var entry in _scaledList)
        {
            if (voronoiSource.GetNearestNeighbor(entry.ItemOne, entry.ItemTwo) != targetIndex)
            {
                removeList.Add(entry);
            }
        }

        foreach (var entry in removeList)
        {
            _scaledList.Remove(entry);
        }

        removeList.Clear();
    }

    public IEnumerable<Tuple<int, int, float>> Points
    {
        get { foreach (var point in _scaledList) yield return point; }
    }

    public ScalablePointBuffer GetChild(float scale, int x, int y, List<Tuple<float, float, float>> pointList = null)
    {
        var realRange = Mathf.Max(0.5f, _maxRange);

        for (int i = 0; i < _scaledList.Count; i++)
        {
            var point = _scaledList[i];
            if (point.ItemOne != x || point.ItemTwo != y) continue;
            
            var newPoints = pointList != null ? pointList : _unscaledList;
            var newBuffer = new ScalablePointBuffer();
            var range = point.ItemThree;

            foreach (var newPoint in newPoints)
            {
                newBuffer.AddEntry(newPoint.ItemOne, newPoint.ItemTwo, newPoint.ItemThree);
            }

            var targetRange = range == 0 ? _width : range * scale;
            newBuffer.CalculateScaledPoints(targetRange, point.ItemOne, point.ItemTwo);
            newBuffer.RestrainToVoronoi(i, this);

            return newBuffer;
        }
        return null;
    }

    public int GetNearestNeighbor(int x, int y)
    {
        int previousClosest = 0;
        float previousDistance = float.MaxValue;
        var point = new Vector2(x, y);
        for (int i = 0; i < _unscaledList.Count; i++)
        {
            var entry = _unscaledList[i];

            var sizeMult = Size * entry.ItemThree / _maxRange;
            var entryX = (int)(entry.ItemOne * sizeMult + CenterX);
            var entryY = (int)(entry.ItemTwo * sizeMult + CenterY);
            var neighbor = new Vector2(entryX, entryY);

            var dist = Vector2.Distance(neighbor, point);
            if (dist < previousDistance)
            {
                previousClosest = i;
                previousDistance = dist;
            }
        }
        return previousClosest;
    }
}
