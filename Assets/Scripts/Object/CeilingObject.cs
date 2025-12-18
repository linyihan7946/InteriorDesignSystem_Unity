using UnityEngine;

public class CeilingObject : BaseObject
{
    // 天花板类
    public float Elevation = 3.0f; // 世界 Y 高度
    public Vector2 Size = new Vector2(10f, 10f);

    public CeilingObject(string json = null) : base(json) { }

    public static CeilingObject Create(string json = null, string name = null)
    {
        return CreateFromJson<CeilingObject>(json, name);
    }

    public override void Rebuild()
    {
        var existing = transform.Find(this.ObjectName);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        var plane = ModelingUtility.CreatePlane(this.ObjectName, Size, 1, 1, this.transform);
        if (plane != null)
        {
            plane.transform.localPosition = new Vector3(0f, Elevation, 0f);

            var cfg = ProjectConfig.Instance;
            Material mat = (cfg != null) ? cfg.defaultMaterial : null;
            if (mat == null)
            {
                // create runtime material with white color as default
                mat = MaterialManager.CreateRuntimeMaterial("CeilingMaterial", null, Color.white);
            }
            var mr = plane.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }
    }
}
