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

    // 设置轮廓点集，按顺序设置各段的起点和终点
    public void SetContourPoints(List<Vector3> points)
    {
        segments.Clear();
        if (points == null || points.Count < 2) return;

        for (int i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = (i == points.Count - 1) ? points[0] : points[i + 1];
            var segGo = new GameObject($"Segment_{i}");
            segGo.transform.SetParent(this.transform, worldPositionStays: true);
            var seg = segGo.AddComponent<SegmentObject>();
            seg.startPoint = a;
            seg.endPoint = b;
            segments.Add(seg);
        }
    }
}
