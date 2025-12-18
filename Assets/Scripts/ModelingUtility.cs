using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用建模工具，提供生成常用几何体的静态接口。
/// 生成的物体会包含 MeshFilter 和 MeshRenderer，并使用内置材质。
/// </summary>
public static class ModelingUtility
{
    public static GameObject CreateCube(string name, Vector3 size, Vector3 center, Transform parent = null)
    {
        var go = new GameObject(string.IsNullOrEmpty(name) ? "Cube" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        go.transform.localPosition = center;

        var mesh = new Mesh();

        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;
        float hz = size.z * 0.5f;

        var verts = new Vector3[]
        {
            // front
            new Vector3(-hx,-hy, hz), new Vector3(hx,-hy, hz), new Vector3(hx,hy, hz), new Vector3(-hx,hy, hz),
            // back
            new Vector3(hx,-hy,-hz), new Vector3(-hx,-hy,-hz), new Vector3(-hx,hy,-hz), new Vector3(hx,hy,-hz),
            // left
            new Vector3(-hx,-hy,-hz), new Vector3(-hx,-hy, hz), new Vector3(-hx,hy, hz), new Vector3(-hx,hy,-hz),
            // right
            new Vector3(hx,-hy, hz), new Vector3(hx,-hy,-hz), new Vector3(hx,hy,-hz), new Vector3(hx,hy, hz),
            // top
            new Vector3(-hx,hy, hz), new Vector3(hx,hy, hz), new Vector3(hx,hy,-hz), new Vector3(-hx,hy,-hz),
            // bottom
            new Vector3(-hx,-hy,-hz), new Vector3(hx,-hy,-hz), new Vector3(hx,-hy, hz), new Vector3(-hx,-hy, hz)
        };

        var uvs = new Vector2[verts.Length];
        for (int i = 0; i < uvs.Length; i++) uvs[i] = new Vector2(0, 0);

        var tris = new int[]
        {
            0,2,1,0,3,2,        // front
            4,6,5,4,7,6,        // back
            8,10,9,8,11,10,     // left
            12,14,13,12,15,14,  // right
            16,18,17,16,19,18,  // top
            20,22,21,20,23,22   // bottom
        };

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    public static GameObject CreatePlane(string name, Vector2 size, int segX = 1, int segY = 1, Transform parent = null)
    {
        segX = Mathf.Max(1, segX);
        segY = Mathf.Max(1, segY);

        var go = new GameObject(string.IsNullOrEmpty(name) ? "Plane" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);

        var mesh = new Mesh();

        int vx = segX + 1;
        int vy = segY + 1;
        var verts = new Vector3[vx * vy];
        var uvs = new Vector2[verts.Length];

        float stepX = size.x / segX;
        float stepY = size.y / segY;
        float startX = -size.x * 0.5f;
        float startY = -size.y * 0.5f;

        for (int y = 0; y < vy; y++)
        {
            for (int x = 0; x < vx; x++)
            {
                int i = y * vx + x;
                verts[i] = new Vector3(startX + x * stepX, 0f, startY + y * stepY);
                uvs[i] = new Vector2((float)x / segX, (float)y / segY);
            }
        }

        var tris = new List<int>();
        for (int y = 0; y < segY; y++)
        {
            for (int x = 0; x < segX; x++)
            {
                int a = y * vx + x;
                int b = a + 1;
                int c = a + vx;
                int d = c + 1;
                tris.Add(a); tris.Add(d); tris.Add(b);
                tris.Add(a); tris.Add(c); tris.Add(d);
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    public static GameObject CreateCylinder(string name, float radius, float height, int radialSegments = 16, Transform parent = null)
    {
        radialSegments = Mathf.Max(3, radialSegments);
        var go = new GameObject(string.IsNullOrEmpty(name) ? "Cylinder" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);

        var mesh = new Mesh();

        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        // side verts
        for (int i = 0; i <= radialSegments; i++)
        {
            float t = (float)i / radialSegments;
            float ang = t * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * radius;
            float z = Mathf.Sin(ang) * radius;
            verts.Add(new Vector3(x, -height * 0.5f, z));
            verts.Add(new Vector3(x, height * 0.5f, z));
            uvs.Add(new Vector2(t, 0));
            uvs.Add(new Vector2(t, 1));
        }

        // side tris
        for (int i = 0; i < radialSegments; i++)
        {
            int idx = i * 2;
            tris.Add(idx);
            tris.Add(idx + 3);
            tris.Add(idx + 1);

            tris.Add(idx);
            tris.Add(idx + 2);
            tris.Add(idx + 3);
        }

        int baseIndex = verts.Count;
        // bottom center
        verts.Add(new Vector3(0, -height * 0.5f, 0));
        uvs.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i < radialSegments; i++)
        {
            float t = (float)i / radialSegments;
            float ang = t * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * radius;
            float z = Mathf.Sin(ang) * radius;
            verts.Add(new Vector3(x, -height * 0.5f, z));
            uvs.Add(new Vector2((x / radius + 1f) * 0.5f, (z / radius + 1f) * 0.5f));
        }
        for (int i = 0; i < radialSegments; i++)
        {
            int a = baseIndex;
            int b = baseIndex + 1 + i;
            int c = baseIndex + 1 + ((i + 1) % radialSegments);
            tris.Add(a); tris.Add(c); tris.Add(b);
        }

        // top
        baseIndex = verts.Count;
        verts.Add(new Vector3(0, height * 0.5f, 0));
        uvs.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i < radialSegments; i++)
        {
            float t = (float)i / radialSegments;
            float ang = t * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * radius;
            float z = Mathf.Sin(ang) * radius;
            verts.Add(new Vector3(x, height * 0.5f, z));
            uvs.Add(new Vector2((x / radius + 1f) * 0.5f, (z / radius + 1f) * 0.5f));
        }
        for (int i = 0; i < radialSegments; i++)
        {
            int a = baseIndex;
            int b = baseIndex + 1 + ((i + 1) % radialSegments);
            int c = baseIndex + 1 + i;
            tris.Add(a); tris.Add(b); tris.Add(c);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    // --- 新增方法：生成复合线、闭合多边形平面、以及多边形拉伸 ---
    public static CompositeLine CreateCompositeLine(string name, List<Vector3> points, bool closed = true, Transform parent = null)
    {
        if (points == null || points.Count < 2) return null;
        var go = new GameObject(string.IsNullOrEmpty(name) ? "CompositeLine" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        var comp = go.AddComponent<CompositeLine>();
        comp.ClassName = "CompositeLine";

        int count = points.Count;
        int segCount = closed ? count : count - 1;
        for (int i = 0; i < segCount; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % count];
            var segGo = new GameObject($"Segment_{i}");
            segGo.transform.SetParent(comp.transform, worldPositionStays: true);
            var seg = segGo.AddComponent<SegmentObject>();
            seg.startPoint = a;
            seg.endPoint = b;
            comp.segments.Add(seg);
        }

        return comp;
    }

    // 仅支持单圈（无孔）的简单多边形（XZ 平面）三角化
    public static GameObject CreatePolygonPlaneObject(string name, List<Vector3> polygon, Transform parent = null)
    {
        if (polygon == null || polygon.Count < 3) return null;
        var tris = TriangulatePolygon(polygon);
        if (tris == null || tris.Count == 0) return null;

        var go = new GameObject(string.IsNullOrEmpty(name) ? "PolygonPlane" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        var mesh = new Mesh();
        mesh.vertices = polygon.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    // 支持多个内孔的多边形平面（outer 为外圈，holes 为内圈列表，均为 XZ 平面坐标）
    public static GameObject CreatePolygonPlaneWithHoles(string name, List<Vector3> outer, List<List<Vector3>> holes, Transform parent = null)
    {
        if (outer == null || outer.Count < 3) return null;

        // copy outer
        var merged = new List<Vector3>(outer);

        if (holes != null)
        {
            foreach (var hole in holes)
            {
                if (hole == null || hole.Count < 3) continue;

                // find hole's rightmost point index
                int hr = 0;
                for (int i = 1; i < hole.Count; i++) if (hole[i].x > hole[hr].x) hr = i;
                var H = hole[hr];

                // find visible vertex on merged (outer + already-merged holes)
                int visibleIndex = -1;
                for (int vi = 0; vi < merged.Count; vi++)
                {
                    var V = merged[vi];
                    if (IsSegmentIntersectingPolygons(H, V, merged, hole)) continue;
                    // midpoint inside outer?
                    var mid = new Vector3((H.x + V.x) * 0.5f, 0f, (H.z + V.z) * 0.5f);
                    if (!PointInPolygonXZ(mid, outer)) continue;
                    visibleIndex = vi;
                    break;
                }

                if (visibleIndex == -1)
                {
                    // fallback: pick nearest outer vertex by distance
                    float bestDist = float.MaxValue;
                    for (int vi = 0; vi < merged.Count; vi++)
                    {
                        var d = Vector2.SqrMagnitude(new Vector2(merged[vi].x - H.x, merged[vi].z - H.z));
                        if (d < bestDist) { bestDist = d; visibleIndex = vi; }
                    }
                }

                // splice hole into merged at visibleIndex: V -> H -> hole vertices (starting from hr) -> H -> V
                var spliced = new List<Vector3>();
                for (int i = 0; i <= visibleIndex; i++) spliced.Add(merged[i]);
                // add bridge to hole
                spliced.Add(H);
                for (int k = 0; k < hole.Count; k++) spliced.Add(hole[(hr + 1 + k) % hole.Count]);
                // add bridge back
                spliced.Add(H);
                for (int i = visibleIndex + 1; i < merged.Count; i++) spliced.Add(merged[i]);

                merged = spliced;
            }
        }

        // triangulate merged polygon
        var tris = TriangulatePolygon(merged);
        if (tris == null || tris.Count == 0) return null;

        ReverseTriangulateOrder(tris);

        var go = new GameObject(string.IsNullOrEmpty(name) ? "PolygonPlaneWithHoles" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        var mesh = new Mesh();
        mesh.vertices = merged.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        // mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    // 判断线段 H-V 是否与 outerPolygon 或 holePolygon 的任何边相交（忽略端点相连情况）
    private static bool IsSegmentIntersectingPolygons(Vector3 a3, Vector3 b3, List<Vector3> outerPoly, List<Vector3> holePoly)
    {
        var a = new Vector2(a3.x, a3.z);
        var b = new Vector2(b3.x, b3.z);

        // check outer
        for (int i = 0; i < outerPoly.Count; i++)
        {
            var p0 = new Vector2(outerPoly[i].x, outerPoly[i].z);
            var p1 = new Vector2(outerPoly[(i + 1) % outerPoly.Count].x, outerPoly[(i + 1) % outerPoly.Count].z);
            if (SegmentsIntersectStrict(a, b, p0, p1)) return true;
        }
        // check hole
        for (int i = 0; i < holePoly.Count; i++)
        {
            var p0 = new Vector2(holePoly[i].x, holePoly[i].z);
            var p1 = new Vector2(holePoly[(i + 1) % holePoly.Count].x, holePoly[(i + 1) % holePoly.Count].z);
            if (SegmentsIntersectStrict(a, b, p0, p1)) return true;
        }
        return false;
    }

    private static bool SegmentsIntersectStrict(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        // exclude cases where segments share endpoints by returning false
        if ((a == c) || (a == d) || (b == c) || (b == d)) return false;
        return SegmentsIntersect(a, b, c, d);
    }

    private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float s1_x = b.x - a.x; float s1_y = b.y - a.y;
        float s2_x = d.x - c.x; float s2_y = d.y - c.y;

        float s = (-s1_y * (a.x - c.x) + s1_x * (a.y - c.y)) / (-s2_x * s1_y + s1_x * s2_y + 1e-9f);
        float t = ( s2_x * (a.y - c.y) - s2_y * (a.x - c.x)) / (-s2_x * s1_y + s1_x * s2_y + 1e-9f);

        return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
    }

    // 点是否在多边形内（XZ 平面）
    private static bool PointInPolygonXZ(Vector3 p3, List<Vector3> poly)
    {
        var p = new Vector2(p3.x, p3.z);
        var pts = new List<Vector2>(poly.Count);
        for (int i = 0; i < poly.Count; i++) pts.Add(new Vector2(poly[i].x, poly[i].z));

        bool inside = false;
        for (int i = 0, j = pts.Count - 1; i < pts.Count; j = i++)
        {
            if (((pts[i].y > p.y) != (pts[j].y > p.y)) &&
                (p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y + 1e-9f) + pts[i].x))
                inside = !inside;
        }
        return inside;
    }

    // 反转三角形顶点顺序（CW <-> CCW）
    public static void ReverseTriangulateOrder(List<int> tris)
    {
        for (int i = 0; i < tris.Count; i += 3)
        {
            int temp = tris[i + 1];
            tris[i + 1] = tris[i + 2];
            tris[i + 2] = temp;
        }
    }

    // 将多边形沿 Y 轴拉伸为实体（棱柱），返回包含网格的 GameObject
    public static GameObject CreateExtrudedPolygon(string name, List<Vector3> polygon, float height, Transform parent = null)
    {
        if (polygon == null || polygon.Count < 3) return null;
        var baseTris = TriangulatePolygon(polygon);
        if (baseTris == null || baseTris.Count == 0) return null;

        var top = new List<Vector3>(polygon.Count);
        for (int i = 0; i < polygon.Count; i++) 
            top.Add(new Vector3(polygon[i].x, height, polygon[i].z));

        var verts = new List<Vector3>();
        var tris = new List<int>();

        // bottom vertices
        int baseIndex = 0;
        verts.AddRange(polygon);
        // top vertices
        int topIndex = verts.Count;
        verts.AddRange(top);

        // bottom (use baseTris as-is)
        tris.AddRange(baseTris);

        // top (reverse order)
        for (int i = 0; i < baseTris.Count; i += 3)
        {
            tris.Add(topIndex + baseTris[i]);
            tris.Add(topIndex + baseTris[i + 2]);
            tris.Add(topIndex + baseTris[i + 1]);
        }

        // sides
        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            int ni = (i + 1) % n;
            int a = baseIndex + i;
            int b = baseIndex + ni;
            int c = topIndex + ni;
            int d = topIndex + i;

            tris.Add(a); tris.Add(c); tris.Add(b);
            tris.Add(a); tris.Add(d); tris.Add(c);
        }

        var mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        var go = new GameObject(string.IsNullOrEmpty(name) ? "ExtrudedPolygon" : name);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        // mr.sharedMaterial = GetDefaultMaterial();

        return go;
    }

    // 简单耳切三角化（在 XZ 平面上工作），返回三角索引
    private static List<int> TriangulatePolygon(List<Vector3> poly)
    {
        int n = poly.Count;
        if (n < 3) return null;
        // 处理闭合（首尾相同）情况，并建立从工作点到原始索引的映射
        var pts = new List<Vector2>();
        var indexMap = new List<int>();
        for (int i = 0; i < n; i++)
        {
            // 如果最后一个点等于第一个点（闭合环），则忽略最后一个重复点
            if (i == n - 1)
            {
                if (Mathf.Approximately(poly[i].x, poly[0].x) && Mathf.Approximately(poly[i].z, poly[0].z))
                    break;
            }
            pts.Add(new Vector2(poly[i].x, poly[i].z));
            indexMap.Add(i);
        }

        int m = pts.Count;
        if (m < 3) return null;

        var indices = new List<int>();
        var V = new List<int>();
        for (int i = 0; i < m; i++) V.Add(i);

        // 计算方向（signed area），确保为 CCW 顶点序列
        if (SignedArea(pts) < 0f) V.Reverse();

        int guard = 0;
        while (V.Count > 2 && guard < m * m)
        {
            bool earFound = false;
            for (int i = 0; i < V.Count; i++)
            {
                int prev = V[(i - 1 + V.Count) % V.Count];
                int curr = V[i];
                int next = V[(i + 1) % V.Count];

                var a = pts[prev];
                var b = pts[curr];
                var c = pts[next];

                if (!IsConvex(a, b, c)) continue;

                bool hasPointInside = false;
                for (int j = 0; j < V.Count; j++)
                {
                    int vi = V[j];
                    if (vi == prev || vi == curr || vi == next) continue;
                    if (PointInTriangle(pts[vi], a, b, c))
                    {
                        hasPointInside = true; break;
                    }
                }
                if (hasPointInside) continue;

                // ear -> 输出为原始多边形索引
                indices.Add(indexMap[prev]);
                indices.Add(indexMap[curr]);
                indices.Add(indexMap[next]);
                V.RemoveAt(i);
                earFound = true;
                break;
            }
            if (!earFound) break;
            guard++;
        }

        return indices;
    }

    private static float SignedArea(List<Vector2> pts)
    {
        float a = 0f;
        for (int i = 0; i < pts.Count; i++)
        {
            var p0 = pts[i];
            var p1 = pts[(i + 1) % pts.Count];
            a += (p0.x * p1.y - p1.x * p0.y);
        }
        return a * 0.5f;
    }

    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 1e-6f;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // barycentric
        var v0 = c - a;
        var v1 = b - a;
        var v2 = p - a;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float denom = dot00 * dot11 - dot01 * dot01;
        if (Mathf.Abs(denom) < Mathf.Epsilon) return false;
        float u = (dot11 * dot02 - dot01 * dot12) / denom;
        float v = (dot00 * dot12 - dot01 * dot02) / denom;
        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    private static Material _defaultMaterial;
    private static Material GetDefaultMaterial()
    {
        if (_defaultMaterial == null)
        {
            _defaultMaterial = new Material(Shader.Find("Standard"));
        }
        return _defaultMaterial;
    }
}
