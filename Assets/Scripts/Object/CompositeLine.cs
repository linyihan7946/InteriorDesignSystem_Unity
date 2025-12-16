using System.Collections.Generic;
using UnityEngine;

public class CompositeLine : BaseObject
{
    [SerializeField]
    public List<SegmentObject> segments = new List<SegmentObject>();

    // 获取轮廓点集，按顺序返回顶点（首段起点 + 每段终点）
    public List<Vector3> GetContourPoints()
    {
        var pts = new List<Vector3>();
        if (segments == null || segments.Count == 0) return pts;
        pts.Add(segments[0].startPoint);
        foreach (var s in segments)
        {
            pts.Add(s.endPoint);
        }
        return pts;
    }
}
