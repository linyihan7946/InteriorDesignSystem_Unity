using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

/// <summary>
/// Clipper-backed 精确区域搜索器。启用编译符号 `CLIPPER_EXISTS` 并引入 Clipper 库后使用此实现。
/// </summary>
public static class RegionSearcher_Clipper
{
    private const double CLIPPER_SCALE = 1000.0;

    public static List<CompositeLine> SearchRoomsByPolygons(List<CompositeLine> obstacles, float minRoomArea = 1.0E6f)
    {
        var result = new List<CompositeLine>();
        if (obstacles == null) return result;

        // compute bounds
        float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;
        bool any = false;
        foreach (var comp in obstacles)
        {
            if (comp == null) continue;
            var p = comp.GetContourPoints();
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
        foreach (var comp in obstacles)
        {
            if (comp == null) continue;
            var poly = comp.GetContourPoints();
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

        // 构造包络框
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
        // var sol = new List<List<IntPoint>>();
        // c.Execute(ClipType.ctDifference, sol, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
        // 1. 修改执行部分：使用 PolyTree 替代 List<List<IntPoint>>
        var polyTree = new PolyTree();
        c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

        // 2. 递归处理节点
        int roomIdx = 0;
        ProcessPolyNodes(polyTree.Childs, ref roomIdx, minRoomArea, result);

        return result;
    }

    // --- 辅助递归函数 ---
    private static void ProcessPolyNodes(List<PolyNode> nodes, ref int idx, float minArea, List<CompositeLine> resList, int layerIndex = 0)
    {
        foreach (var node in nodes)
        {
            // 只有非孔洞（IsHole == false）的节点才是我们要的“房间面积”
            if (!node.IsHole && layerIndex != 0)
            {
                var poly = IntPathToVector3(node.Contour);
                if (poly != null && poly.Count >= 3)
                {
                    float area = Mathf.Abs(PolygonAreaXZ(poly));
                    if (area >= minArea)
                    {
                        // 创建房间对象
                        var room = CompositeLine.Create(null, "Room_" + idx);
                        room.SetContourPoints(poly, true);
                        // ... (保持原有的父子层级设置代码)
                        
                        resList.Add(room);
                        idx++;
                    }
                }
            }

            // 关键：递归处理子节点
            // 如果当前节点是房间，它的子节点可能是“内墙”
            // 内墙的子节点则可能是“房中房”
            if (node.ChildCount > 0)
            {
                ProcessPolyNodes(node.Childs, ref idx, minArea, resList, layerIndex + 1);
            }
        }
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