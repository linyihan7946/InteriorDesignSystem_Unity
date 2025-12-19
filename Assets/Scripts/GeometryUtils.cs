using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    // 将 Vector3 投影到 XZ 平面（2D 空间为 x,z）
    public static Vector2 To2D(Vector3 v) => new Vector2(v.x, v.z);
    // 从 2D(x,z) 恢复 Vector3，第二个参数为 Y（高度）
    public static Vector3 From2D(Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);

    // 简单距离
    public static float Distance2D(Vector3 a, Vector3 b) => Vector2.Distance(To2D(a), To2D(b));

    // 计算闭合多边形 2D 面积（signed）
    public static float SignedArea2D(IList<Vector3> poly)
    {
        int n = poly.Count;
        if (n < 3) return 0f;
        float area = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = To2D(poly[i]);
            Vector2 b = To2D(poly[(i + 1) % n]);
            area += a.x * b.y - b.x * a.y;
        }
        return area * 0.5f;
    }

    public static Vector3 Centroid2D(IList<Vector3> poly)
    {
        int n = poly.Count;
        Vector2 c = Vector2.zero;
        float A = SignedArea2D(poly);
        if (Mathf.Approximately(A, 0f))
        {
            // fallback: average
            Vector2 s = Vector2.zero;
            for (int i = 0; i < n; i++) s += To2D(poly[i]);
            s /= Mathf.Max(1, n);
            return From2D(s, poly.Count>0?poly[0].y:0f);
        }
        for (int i = 0; i < n; i++)
        {
            Vector2 p0 = To2D(poly[i]);
            Vector2 p1 = To2D(poly[(i + 1) % n]);
            float cross = p0.x * p1.y - p1.x * p0.y;
            c += (p0 + p1) * cross;
        }
        c /= (6f * A);
        return From2D(c, poly.Count>0?poly[0].y:0f);
    }

    // 由两点和厚度生成闭合的矩形点集（XZ 平面），返回按顺时针或逆时针的四个顶点（不重复首点）
    public static List<Vector3> CreateThickSegmentPolygon(Vector3 p1, Vector3 p2, float thickness)
    {
        var a = To2D(p1);
        var b = To2D(p2);
        Vector2 dir = (b - a).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x); // right-hand perp
        float half = thickness * 0.5f;
        Vector2 a1 = a + normal * half;
        Vector2 b1 = b + normal * half;
        Vector2 b2 = b - normal * half;
        Vector2 a2 = a - normal * half;
        // 使用 Y 作为高度
        float y = (p1.y + p2.y) * 0.5f;
        var result = new List<Vector3>
        {
            From2D(a1, y),
            From2D(b1, y),
            From2D(b2, y),
            From2D(a2, y)
        };
        // 对result逆时针排列
        if (IsClockwise(result))
        {
            result.Reverse();
        }
        return result;
    }

    // 对闭合多边形进行偏移（外缩/扩展）。正值朝外，负值朝内。
    // 使用边法线偏移并求相邻偏移边的交点（miter）。对于平行边退回到简单偏移点。
    public static List<Vector3> CreateOffsetPolygon(IList<Vector3> poly, float offset)
    {
        int n = poly.Count;
        var result = new List<Vector3>(n);
        if (n < 3) return result;
        // 预计算边的方向和法线（单位）
        Vector2[] dirs = new Vector2[n];
        Vector2[] norms = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            Vector2 p0 = To2D(poly[i]);
            Vector2 p1 = To2D(poly[(i + 1) % n]);
            Vector2 d = (p1 - p0).normalized;
            dirs[i] = d;
            norms[i] = new Vector2(-d.y, d.x); // outward normal for edge i
        }
        for (int i = 0; i < n; i++)
        {
            int iPrev = (i - 1 + n) % n;
            Vector2 p = To2D(poly[i]);
            // line A: edge iPrev offset
            Vector2 a0 = To2D(poly[iPrev]) + norms[iPrev] * offset;
            Vector2 a1 = To2D(poly[(iPrev + 1) % n]) + norms[iPrev] * offset;
            // line B: edge i offset
            Vector2 b0 = To2D(poly[i]) + norms[i] * offset;
            Vector2 b1 = To2D(poly[(i + 1) % n]) + norms[i] * offset;
            bool ok = LineLineIntersection(a0, a1, b0, b1, out Vector2 inter);
            float y = poly[i].y;
            if (ok)
            {
                result.Add(From2D(inter, y));
            }
            else
            {
                // 平行或数值不稳定，退回到点沿平均法线偏移
                Vector2 avg = (norms[iPrev] + norms[i]).normalized;
                if (avg == Vector2.zero) avg = norms[i];
                result.Add(From2D(p + avg * offset, y));
            }
        }
        return result;
    }

    // 线段（p1->p2）与（p3->p4）求交（无限延长直线），返回是否有唯一交点
    public static bool LineLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        Vector2 r = p2 - p1;
        Vector2 s = p4 - p3;
        float rxs = Cross(r, s);
        float qpxr = Cross((p3 - p1), r);
        if (Mathf.Approximately(rxs, 0f))
        {
            return false; // 平行或共线
        }
        float t = Cross((p3 - p1), s) / rxs;
        intersection = p1 + t * r;
        return true;
    }

    static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    // 判断多边形是否顺时针（true = 顺时针）
    public static bool IsClockwise(IList<Vector3> poly) => SignedArea2D(poly) < 0f;
}
