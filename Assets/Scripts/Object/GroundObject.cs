using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class GroundObject : BaseObject
{
    // 地面类：在 XZ 平面创建大平面并添加 MeshCollider
    // 轮廓：第一个为外圈，其余为内圈（孔）
    // 顺逆时针都可以
    public CompositeLine[] contours;
    public float Elevation = 0f; // Y
    public bool AddCollider = true;

    public GroundObject(string json = null) : base(json) { }

    public static GroundObject Create(string json = null, string name = null)
    {
        return CreateFromJson<GroundObject>(json, name);
    }

    public override void Rebuild()
    {
        var name = this.Id;
        var existing = transform.Find(name);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        // 构建多边形平面：外圈 + 内圈
        List<Vector3> outer = null;
        var holes = new List<List<Vector3>>();
        if (contours != null && contours.Length > 0)
        {
            if (contours[0] != null) outer = contours[0].GetContourPoints();
            for (int i = 1; i < contours.Length; i++)
            {
                if (contours[i] != null) holes.Add(contours[i].GetContourPoints());
            }
        }

        GameObject go = null;
        if (outer != null && outer.Count >= 3)
        {
            go = ModelingUtility.CreatePolygonPlaneWithHoles(name, outer, holes.Count>0?holes:null, this.transform);
        }
        if (go == null) return;

        go.transform.localPosition = new Vector3(0f, Elevation, 0f);

        // 材质：优先使用 ProjectConfig.floorMaterial，否则创建运行时材质并设置颜色
        var cfg = ProjectConfig.Instance;
        Material mat = (cfg != null) ? cfg.floorMaterial : null;
        if (mat == null)
        {
            var color = (cfg != null) ? cfg.floorDefaultColor : new Color(0.6f, 0.6f, 0.6f, 1f);
            mat = MaterialManager.CreateRuntimeMaterial("GroundMaterial", null, color);
        }

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;

        if (AddCollider)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                var col = go.GetComponent<MeshCollider>();
                if (col == null) col = go.AddComponent<MeshCollider>();
                col.sharedMesh = mf.sharedMesh;
            }
        }
    }

    // 设置轮廓（第一个为外圈，其余为内圈）
    public void SetContours(CompositeLine[] newContours)
    {
        contours = newContours;
    }

    // 获取当前轮廓数组（可能为 null）
    public CompositeLine[] GetContours()
    {
        return contours;
    }
}
