using UnityEngine;

public class DoorWindowObject : BaseObject
{
    public float length = 0.9f; // 门/窗水平尺寸
    public float width = 0.2f;  // 厚度或轴向尺寸
    public float height = 2.1f; // 高度
    public float bottomHeight = 0f; // 离地高

    public override void Rebuild()
    {
        var existing = transform.Find("DoorWindowMesh");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        var cube = ModelingUtility.CreateCube("DoorWindowMesh", new UnityEngine.Vector3(length, height, width), new UnityEngine.Vector3(0f, bottomHeight + height * 0.5f, 0f), this.transform);
        if (cube != null)
        {
            // nothing else for now
        }
    }

    public DoorWindowObject(string json = null) : base(json) { }

    public static DoorWindowObject Create(string json = null, string name = null)
    {
        return CreateFromJson<DoorWindowObject>(json, name);
    }
}
