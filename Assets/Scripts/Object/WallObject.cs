using System.Collections.Generic;
using Unity.VisualScripting;
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
        WallObject wall = CreateFromJson<WallObject>(json, name);
        if (wall.contour == null)
        {
            wall.contour = new CompositeLine();
        }
        return wall;
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
        Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z);
        if (dirXZ.sqrMagnitude < 1e-6f) return;
        dirXZ.Normalize();

        Matrix4x4 rotMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
        Vector3 normal = rotMatrix.MultiplyVector(dirXZ) * (thickness * 0.5f);

        
        List<Vector3> poly = new List<Vector3>
        {
            new Vector3(p0.x, 0f, p0.z) - normal,
            new Vector3(p0.x, 0f, p0.z) + normal,
            new Vector3(p1.x, 0f, p1.z) + normal,
            new Vector3(p1.x, 0f, p1.z) - normal
        };
        
        var wallGo = ModelingUtility.CreateExtrudedPolygon(this.ObjectName, poly, height, this.transform);
        if (wallGo != null)
        {
            // 将墙体底部对齐到 centerLine 的 y 位置 (centerLine.startPoint.y)
            wallGo.transform.localPosition = new Vector3(0f, 0f, 0f);

            // 设置墙体材质颜色或替换材质为 ProjectConfig 中的默认材质
            var cfg = ProjectConfig.Instance;
            var desiredColor = (cfg != null) ? cfg.wallDefaultColor : new Color(0.8f, 0.8f, 0.8f, 1f);
            var desiredMat = cfg != null ? cfg.wallMaterial : null;
            var renderers = wallGo.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (desiredMat != null)
                {
                    // 使用配置中的材质（赋值会实例化材质以免修改共享材质）
                    r.material = desiredMat;
                }
                else
                {
                    // 确保有实例化材质然后设置颜色（兼容 Standard / URP / HDRP 属性名）
                    var mat = r.material;
                    if (mat != null)
                    {
                        mat.color = desiredColor;
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", desiredColor);
                        if (mat.HasProperty("_Color")) mat.SetColor("_Color", desiredColor);
                    }
                }
            }
        }

    }

}
