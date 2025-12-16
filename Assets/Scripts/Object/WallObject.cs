using System.Collections.Generic;
using UnityEngine;

public class WallObject : BaseObject
{
    public SegmentObject centerLine;
    public float thickness = 120f;
    public float height = 2800f;
    public CompositeLine contour;

    public WallObject(string json = null) : base(json) { }

    public static WallObject Create(string json = null, string name = null)
    {
        return CreateFromJson<WallObject>(json, name);
    }
    
    public override void Rebuild()
    {
        // 删除已有墙体网格
        var existing = transform.Find(this.ObjectName);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        
        if (centerLine == null) return;
        var p0 = centerLine.startPoint;
        var p1 = centerLine.endPoint;
        var dir = p1 - p0;
        var dirXZ = new Vector3(dir.x, 0f, dir.z);
        if (dirXZ.sqrMagnitude < 1e-6f) return;
        dirXZ.Normalize();
        var perp = new Vector3(-dirXZ.z, 0f, dirXZ.x) * (thickness * 0.5f);
        
        List<Vector3> poly = new List<Vector3>
        {
            new Vector3(p0.x + perp.x, p0.y, p0.z + perp.z),
            new Vector3(p0.x - perp.x, p0.y, p0.z - perp.z),
            new Vector3(p1.x - perp.x, p1.y, p1.z - perp.z),
            new Vector3(p1.x + perp.x, p1.y, p1.z + perp.z)
        };
        
        var wallGo = ModelingUtility.CreateExtrudedPolygon(this.ObjectName, poly, height, this.transform);
        if (wallGo != null)
        {
            // 将墙体底部对齐到 centerLine 的 y 位置 (centerLine.startPoint.y)
            wallGo.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

    }

}
