using System.Collections.Generic;
using UnityEngine;

public class PolygonPlane : BaseObject
{
    [SerializeField]
    public List<CompositeLine> contours = new List<CompositeLine>(); // 第一圈为外圈，其余为内孔

    public CompositeLine AddContourPoints(List<Vector3> points)
    {
        if (points == null || points.Count < 2) return null;
        var go = new GameObject("CompositeLine");
        go.transform.SetParent(this.transform, worldPositionStays: true);
        var comp = go.AddComponent<CompositeLine>();
        comp.ClassName = "CompositeLine";

        for (int i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = (i == points.Count - 1) ? points[0] : points[i + 1];
            var segGo = new GameObject($"Segment_{i}");
            segGo.transform.SetParent(comp.transform, worldPositionStays: true);
            var seg = segGo.AddComponent<SegmentObject>();
            seg.startPoint = a;
            seg.endPoint = b;
            comp.segments.Add(seg);
        }

        contours.Add(comp);
        return comp;
    }

    public void ClearAllContours()
    {
        foreach (var c in contours)
        {
            if (c != null)
            {
                DestroyImmediate(c.gameObject);
            }
        }
        contours.Clear();
    }

    public void SetAllContours(List<List<Vector3>> allContours)
    {
        ClearAllContours();
        if (allContours == null) return;
        foreach (var ring in allContours)
        {
            AddContourPoints(ring);
        }
    }

    public List<List<Vector3>> GetAllContoursPoints()
    {
        var res = new List<List<Vector3>>();
        foreach (var c in contours)
        {
            if (c == null) continue;
            res.Add(c.GetContourPoints());
        }
        return res;
    }

    public List<Vector3> GetContourPointsByIndex(int index)
    {
        if (index < 0 || index >= contours.Count) return null;
        return contours[index].GetContourPoints();
    }

    public override void Rebuild()
    {
        var existing = transform.Find("PolygonPlaneMesh");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        if (contours == null || contours.Count == 0) return;
        var outer = contours[0].GetContourPoints();
        var holes = new List<List<Vector3>>();
        for (int i = 1; i < contours.Count; i++) holes.Add(contours[i].GetContourPoints());

        var go = ModelingUtility.CreatePolygonPlaneWithHoles("PolygonPlaneMesh", outer, holes, this.transform);
        if (go != null)
        {
            // keep as child
        }
    }

    public PolygonPlane(string json = null) : base(json) { }

    public static PolygonPlane Create(string json = null, string name = null)
    {
        return CreateFromJson<PolygonPlane>(json, name);
    }
}
