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
            var wallMaterialName = "WallMaterial";
            var cfg = ProjectConfig.Instance;
            var desiredColor = (cfg != null) ? cfg.wallDefaultColor : new Color(0.8f, 0.8f, 0.8f, 1f);

            // prefer material from ProjectConfig if provided
            Material material = (cfg != null) ? cfg.wallMaterial : null;

            // try to find existing material asset in editor or scene
            if (material == null)
            {
                // try find in scene, otherwise create a runtime material
                material = MaterialManager.FindMaterialInScene(wallMaterialName) ?? MaterialManager.CreateRuntimeMaterial(wallMaterialName);
            }

            // apply color
            if (material != null) MaterialManager.SetMaterialColor(material, desiredColor);

            // assign to all renderers produced by ModelingUtility
            var renderers = wallGo.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (material != null)
                {
                    // prefer sharedMaterial so editor asset is used; at runtime this will use the material reference
                    r.sharedMaterial = material;
                }
            }
        }

    }

}
