using UnityEngine;

public class SceneObject : BaseObject
{
    // 继承自 BaseObject，场景可扩展特殊属性或方法
    public override void Rebuild()
    {
        // 对直接子物体触发 Rebuild，避免重复遍历整个树
        foreach (Transform t in transform)
        {
            var bo = t.GetComponent<BaseObject>();
            if (bo != null && bo != this) bo.Rebuild();
        }
    }
}
