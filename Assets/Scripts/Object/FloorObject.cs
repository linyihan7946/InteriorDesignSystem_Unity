using UnityEngine;

public class FloorObject : BaseObject
{
    // 楼层类，可扩展楼层特有属性
    public float Elevation = 0f;

    public override void Rebuild()
    {

    }

    public FloorObject(string json = null) : base(json) { }

    public static FloorObject Create(string json = null, string name = null)
    {
        return CreateFromJson<FloorObject>(json, name);
    }
}
