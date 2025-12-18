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

    // 设置轮廓点集（按点顺序生成首尾相连的段），在运行时使用 Destroy，编辑器使用 DestroyImmediate
    public void SetContourPoints(List<Vector3> points, bool closed = true)
    {
        if (segments == null) segments = new List<SegmentObject>();
        segments.Clear();

        if (points == null || points.Count < 2) return;
        int count = points.Count;
        int segCount = closed ? count : count - 1;
        for (int i = 0; i < segCount; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % count];
            // 计算a跟b的距离
            float distance = Vector3.Distance(a, b);
            if (distance < 0.001f) continue; // 跳过过近的点

            var seg = SegmentObject.Create();
            seg.startPoint = a;
            seg.endPoint = b;
            segments.Add(seg);
        }
    }

    public override void Rebuild()
    {
        // 清除旧的可视化段
        var toRemove = new List<Transform>();
        foreach (Transform t in transform)
        {
            if (t.name.StartsWith("SegmentVis_") || t.name == "SegmentVisual") toRemove.Add(t);
        }
        foreach (var t in toRemove) UnityEngine.Object.DestroyImmediate(t.gameObject);

        for (int i = 0; i < (segments != null ? segments.Count : 0); i++)
        {
            var s = segments[i];
            if (s == null) continue;
            var mid = (s.startPoint + s.endPoint) * 0.5f;
            var dir = s.endPoint - s.startPoint;
            float len = dir.magnitude;
            if (len <= 0.0001f) continue;
            var vis = ModelingUtility.CreateCylinder($"SegmentVis_{i}", 0.02f, len, 12, this.transform);
            if (vis != null)
            {
                vis.transform.localPosition = mid;
                vis.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
            }
        }
    }

    // 遗留兼容方法，转到统一实现
    public void SetContourPoints(List<Vector3> points)
    {
        SetContourPoints(points, true);
    }

    public CompositeLine(string json = null) : base(json) { }

    public static CompositeLine Create(string json = null, string name = null)
    {
        return CreateFromJson<CompositeLine>(json, name);
    }
}
