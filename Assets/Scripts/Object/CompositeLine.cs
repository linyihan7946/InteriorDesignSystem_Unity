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
        // 清空已有段（删除子 GameObject）
        var toRemove = new List<GameObject>();
        foreach (Transform t in transform)
        {
            toRemove.Add(t.gameObject);
        }
        if (segments == null) segments = new List<SegmentObject>();
        segments.Clear();
        foreach (var g in toRemove)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(g);
            else UnityEngine.Object.DestroyImmediate(g);
        }

        if (points == null || points.Count < 2) return;
        int count = points.Count;
        int segCount = closed ? count : count - 1;
        for (int i = 0; i < segCount; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % count];
            var segGo = new GameObject($"Segment_{i}");
            segGo.transform.SetParent(this.transform, worldPositionStays: true);
            var seg = segGo.AddComponent<SegmentObject>();
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
