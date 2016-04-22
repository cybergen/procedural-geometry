
using System.Collections.Generic;
using UnityEngine;
using BriLib;

public class ScalablePointBuffer
{
    private List<Tuple<float, float, float>> _unscaledList = new List<Tuple<float, float, float>>();
    private int[,] _voronoiField;
    private float[,] _scaleField;
    private ScalablePointBuffer[,] _subBuffers;
    private List<Tuple<int, int>> _scaledList = new List<Tuple<int, int>>();
    private float _maxRange = float.MinValue;
    private int _width;
    private int _height;

    public IEnumerable<Tuple<int, int>> Points
    {
        get
        {
            foreach (var point in _scaledList)
            {
                yield return point;
                if (_subBuffers != null && _subBuffers[point.ItemOne, point.ItemTwo] != null)
                {
                    foreach (var child in _subBuffers[point.ItemOne, point.ItemTwo].Points)
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    public void AddEntry(float x, float y, float dist)
    {
        _unscaledList.Add(new Tuple<float, float, float>(x / 2f, y / 2f, dist));
        _maxRange = Mathf.Max(_maxRange, dist);
    }

    public void CalculateScaledPoints(float size, int centerX, int centerY)
    {
        int previousX = centerX;
        int previousY = centerY;
        float realMax = Mathf.Max(0.5f, _maxRange);
        _width = (int)(size * realMax + centerX);
        _height = (int)(size * realMax + centerY);
        _scaleField = new float[_width, _height];
        foreach (var entry in _unscaledList)
        {
            var sizeMult = size * entry.ItemThree / realMax;
            var x = (int)(entry.ItemOne * sizeMult + centerX);
            var y = (int)(entry.ItemTwo * sizeMult + centerY);

            _scaledList.Add(new Tuple<int, int>(x, y));            
            _scaleField[x, y] = Mathf.Sqrt(Mathf.Pow(x - previousX, 2) + Mathf.Pow(y - previousY, 2));

            previousX = x;
            previousY = y;
        }
    }

    public void CalculateVoronoi(float size, int centerX, int centerY)
    {
        _voronoiField = new int[_width, _height];
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var neighbor = GetNearestNeighbor(x, y, size, centerX, centerY);
                _voronoiField[x, y] = neighbor;
            }
        }
    }

    public void RestrainToVoronoi(int[,] voronoiField, int targetIndex)
    {
        var removeList = new List<Tuple<int, int>>();
        foreach (var entry in _scaledList)
        {
            if (voronoiField[entry.ItemOne, entry.ItemTwo] != targetIndex)
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

    public void CalculateChildren(int depth, float scaleFactor, List<Tuple<float, float, float>> pointList = null)
    {
        Debug.Log("Calculating children at depth: " + depth);

        var newDepth = depth - 1;
        if (newDepth <= 0) return;

        var realRange = Mathf.Max(0.5f, _maxRange);
        _subBuffers = new ScalablePointBuffer[_width, _height];

        for (int i = 0; i < _scaledList.Count; i++)
        {
            var point = _scaledList[i];
            var range = _scaleField[point.ItemOne, point.ItemTwo];            
            var newPoints = pointList != null ? pointList : _unscaledList;
            var newBuffer = new ScalablePointBuffer();
            _subBuffers[point.ItemOne, point.ItemTwo] = newBuffer;
            foreach (var newPoint in newPoints)
            {
                newBuffer.AddEntry(newPoint.ItemOne, newPoint.ItemTwo, newPoint.ItemThree);
            }

            var targetRange = range == 0 ? _width : range * scaleFactor;
            newBuffer.CalculateScaledPoints(targetRange, point.ItemOne, point.ItemTwo);
            //newBuffer.CalculateVoronoi(targetRange, point.ItemOne, point.ItemTwo);
            //newBuffer.RestrainToVoronoi(_voronoiField, i);
            newBuffer.CalculateChildren(newDepth, targetRange);
        }
    }

    private int GetNearestNeighbor(int x, int y, float size, int centerX, int centerY)
    {
        int previousClosest = 0;
        float previousDistance = float.MaxValue;
        var point = new Vector2(x, y);
        for (int i = 0; i < _unscaledList.Count; i++)
        {
            var entry = _unscaledList[i];

            var sizeMult = size * entry.ItemThree / _maxRange;
            var entryX = (int)(entry.ItemOne * sizeMult + centerX);
            var entryY = (int)(entry.ItemTwo * sizeMult + centerY);
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
