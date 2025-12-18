using System.Collections.Generic;
using UnityEngine;

public class CeilingObject : BaseObject
{
    // 天花板类
    public float Elevation = 3.0f; // 世界 Y 高度
    // 轮廓：第一个为外圈，其余为内圈（孔）
    public CompositeLine[] contours;

    public CeilingObject(string json = null) : base(json) { }

    public static CeilingObject Create(string json = null, string name = null)
    {
        return CreateFromJson<CeilingObject>(json, name);
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
            go = ModelingUtility.CreatePolygonPlaneWithHoles(name, outer, holes.Count>0?holes:null, this.transform, true);
        }
        if (go == null) return;

        go.transform.localPosition = new Vector3(0f, Elevation, 0f);

        var cfg = ProjectConfig.Instance;
        Material mat = (cfg != null) ? cfg.defaultMaterial : null;
        if (mat == null)
        {
            mat = MaterialManager.CreateRuntimeMaterial("CeilingMaterial", null, Color.white);
        }
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;
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
