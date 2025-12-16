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
}
