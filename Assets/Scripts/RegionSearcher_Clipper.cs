#if CLIPPER_EXISTS
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

/// <summary>
/// Clipper-backed 精确区域搜索器。启用编译符号 `CLIPPER_EXISTS` 并引入 Clipper 库后使用此实现。
/// </summary>
public static class RegionSearcher_Clipper
{
    private const double CLIPPER_SCALE = 1000.0;

    public static List<List<Vector3>> SearchRoomsByPolygons(List<List<Vector3>> obstacles, float minRoomArea = 0.5f)
    {
        var result = new List<List<Vector3>>();
        if (obstacles == null) return result;

        // compute bounds
        float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;
        bool any = false;
        foreach (var p in obstacles)
        {
            if (p == null) continue;
            foreach (var v in p)
            {
                if (!any) { minX = maxX = v.x; minZ = maxZ = v.z; any = true; }
                else
                {
                    if (v.x < minX) minX = v.x;
                    if (v.x > maxX) maxX = v.x;
                    if (v.z < minZ) minZ = v.z;
                    if (v.z > maxZ) maxZ = v.z;
                }
            }
        }
        if (!any) return result;

        float pad = Mathf.Max(5f, Mathf.Max(maxX - minX, maxZ - minZ));
        minX -= pad; minZ -= pad; maxX += pad; maxZ += pad;

        var clip = new List<List<IntPoint>>();
        foreach (var poly in obstacles)
        {
            if (poly == null || poly.Count < 3) continue;
            var path = new List<IntPoint>();
            foreach (var v in poly)
            {
                long x = (long)System.Math.Round(v.x * CLIPPER_SCALE);
                long y = (long)System.Math.Round(v.z * CLIPPER_SCALE);
                path.Add(new IntPoint(x, y));
            }
            clip.Add(path);
        }

        var env = new List<IntPoint>
        {
            new IntPoint((long)System.Math.Round(minX * CLIPPER_SCALE), (long)System.Math.Round(minZ * CLIPPER_SCALE)),
            new IntPoint((long)System.Math.Round(maxX * CLIPPER_SCALE), (long)System.Math.Round(minZ * CLIPPER_SCALE)),
            new IntPoint((long)System.Math.Round(maxX * CLIPPER_SCALE), (long)System.Math.Round(maxZ * CLIPPER_SCALE)),
            new IntPoint((long)System.Math.Round(minX * CLIPPER_SCALE), (long)System.Math.Round(maxZ * CLIPPER_SCALE))
        };

        var c = new Clipper();
        c.AddPath(env, PolyType.ptSubject, true);
        foreach (var p in clip) c.AddPath(p, PolyType.ptClip, true);
        var sol = new List<List<IntPoint>>();
        c.Execute(ClipType.ctDifference, sol, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        foreach (var path in sol)
        {
            var poly = IntPathToVector3(path);
            if (poly == null || poly.Count < 3) continue;
            float area = Mathf.Abs(PolygonAreaXZ(poly));
            if (area < minRoomArea) continue;
            result.Add(poly);
        }

        return result;
    }

    private static List<Vector3> IntPathToVector3(List<IntPoint> path)
    {
        if (path == null || path.Count < 3) return null;
        var outp = new List<Vector3>(path.Count);
        foreach (var ip in path)
        {
            float x = (float)ip.X / (float)CLIPPER_SCALE;
            float z = (float)ip.Y / (float)CLIPPER_SCALE;
            outp.Add(new Vector3(x, 0f, z));
        }
        return outp;
    }

    private static float PolygonAreaXZ(List<Vector3> poly)
    {
        if (poly == null || poly.Count < 3) return 0f;
        float a = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            var p0 = poly[i];
            var p1 = poly[(i + 1) % poly.Count];
            a += (p0.x * p1.z - p1.x * p0.z);
        }
        return 0.5f * a;
    }
}
#endif
