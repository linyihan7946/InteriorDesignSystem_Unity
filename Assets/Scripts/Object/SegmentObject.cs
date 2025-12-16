using System;
using UnityEngine;

[Serializable]
public class SegmentObject : BaseObject
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public float curvature = 0f; // 0 表示直线，大于0 表示曲率（简单表示）

    public float Length => Vector3.Distance(startPoint, endPoint);

    public Vector3 GetPointAt(float t)
    {
        // t: 0..1，线性插值；如果需要曲线，可扩展
        return Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(t));
    }

    public override void Rebuild()
    {
        // 移除已有可视化
        var exist = transform.Find("SegmentVisual");
        if (exist != null) UnityEngine.Object.DestroyImmediate(exist.gameObject);

        var mid = (startPoint + endPoint) * 0.5f;
        var dir = endPoint - startPoint;
        float len = dir.magnitude;
        if (len <= 0.0001f) return;

        var cyl = ModelingUtility.CreateCylinder("SegmentVisual", 0.02f, len, 12, this.transform);
        if (cyl != null)
        {
            cyl.transform.localPosition = mid;
            cyl.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        }
    }

    public SegmentObject(string json = null) : base(json) { }

    public static SegmentObject Create(string json = null, string name = null)
    {
        return CreateFromJson<SegmentObject>(json, name);
    }
}
