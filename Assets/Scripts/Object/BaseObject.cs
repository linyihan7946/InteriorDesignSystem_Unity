using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExtensionEntry
{
    public string key;
    public string jsonValue;
}

[DisallowMultipleComponent]
public class BaseObject : MonoBehaviour
{
    [SerializeField]
    private string _className;

    [SerializeField]
    private string _id;

    [SerializeField]
    private List<ExtensionEntry> _extensionData = new List<ExtensionEntry>();

    [SerializeField]
    private Matrix4x4 _matrix2D = Matrix4x4.identity;

    [SerializeField]
    private Matrix4x4 _matrix3D = Matrix4x4.identity;

    public string ClassName
    {
        get => string.IsNullOrEmpty(_className) ? GetType().Name : _className;
        set => _className = value;
    }

    public string ObjectName
    {
        get => gameObject != null ? gameObject.name : name;
        set { if (gameObject != null) gameObject.name = value; }
    }

    public string Id
    {
        get
        {
            if (string.IsNullOrEmpty(_id)) _id = Guid.NewGuid().ToString();
            return _id;
        }
        set => _id = value;
    }

    public Matrix4x4 Matrix2D { get => _matrix2D; set => _matrix2D = value; }

    // 参数化构造（注意：Unity 在通过 AddComponent 创建组件时不会调用带参构造函数，
    // 但这个构造函数可用于手工 new 时的初始化，建议优先使用静态工厂 CreateFromJson）
    public BaseObject(string json = null)
    {
        if (!string.IsNullOrEmpty(json))
        {
            try { Deserialize(json); } catch { }
        }
        if (string.IsNullOrEmpty(_id)) _id = Guid.NewGuid().ToString();
    }

    // 通用静态工厂：创建 GameObject、添加组件并使用 json 初始化
    public static T CreateFromJson<T>(string json = null, string name = null) where T : BaseObject
    {
        var go = new GameObject(string.IsNullOrEmpty(name) ? typeof(T).Name : name);
        var comp = go.AddComponent<T>();
        if (!string.IsNullOrEmpty(json))
        {
            comp.Deserialize(json);
        }
        // ensure id
        if (string.IsNullOrEmpty(comp._id)) comp._id = Guid.NewGuid().ToString();
        return comp;
    }
    public Matrix4x4 Matrix3D { get => _matrix3D; set => _matrix3D = value; }

    // 将给定 4x4 矩阵应用到本对象的 Transform（可选择作为局部矩阵或世界矩阵）
    public void ApplyMatrix(Matrix4x4 matrix, bool asLocal = false)
    {
        Matrix4x4 localMatrix = matrix;
        if (!asLocal)
        {
            if (transform.parent != null) localMatrix = transform.parent.worldToLocalMatrix * matrix;
        }

        Vector3 pos = new Vector3(localMatrix.m03, localMatrix.m13, localMatrix.m23);
        Vector3 col0 = new Vector3(localMatrix.m00, localMatrix.m10, localMatrix.m20);
        Vector3 col1 = new Vector3(localMatrix.m01, localMatrix.m11, localMatrix.m21);
        Vector3 col2 = new Vector3(localMatrix.m02, localMatrix.m12, localMatrix.m22);

        Vector3 scale = new Vector3(col0.magnitude, col1.magnitude, col2.magnitude);
        Quaternion rot = Quaternion.identity;
        if (scale.x > Mathf.Epsilon && scale.y > Mathf.Epsilon && scale.z > Mathf.Epsilon)
        {
            Vector3 forward = col2 / scale.z;
            Vector3 up = col1 / scale.y;
            rot = Quaternion.LookRotation(forward, up);
        }

        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = scale;
    }

    // 获取本对象的局部 TRS 矩阵
    public Matrix4x4 GetLocalMatrix()
    {
        return Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
    }

    // 获取本对象的世界矩阵
    public Matrix4x4 GetWorldMatrix()
    {
        return transform.localToWorldMatrix;
    }

    // 将内部存储的 Matrix2D/Matrix3D 应用到 Transform
    public void ApplyMatrix2DToTransform(bool asLocal = true)
    {
        ApplyMatrix(_matrix2D, asLocal);
    }

    public void ApplyMatrix3DToTransform(bool asLocal = true)
    {
        ApplyMatrix(_matrix3D, asLocal);
    }

    // 从当前 Transform 更新内部矩阵（把局部 TRS 复制到 Matrix3D，同时把同样的矩阵复制到 Matrix2D）
    public void UpdateMatricesFromTransform()
    {
        _matrix3D = GetLocalMatrix();
        _matrix2D = _matrix3D;
    }
    // 为建模/更新提供的入口，派生类应覆写以创建或重建其可视化结构
    public virtual void Rebuild()
    {
        // 默认：不做任何操作
    }

    public virtual string Serialize()
    {
        var wrapper = new SerializationWrapper
        {
            type = GetType().FullName,
            data = JsonUtility.ToJson(this)
        };
        return JsonUtility.ToJson(wrapper);
    }

    public virtual void Deserialize(string json)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
            if (wrapper != null && !string.IsNullOrEmpty(wrapper.data))
            {
                JsonUtility.FromJsonOverwrite(wrapper.data, this);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Deserialize error: " + e.Message);
        }
    }

    public virtual void AddChild(BaseObject child)
    {
        if (child == null) return;
        child.transform.SetParent(this.transform, worldPositionStays: true);
    }

    public virtual void SetParent(BaseObject parent)
    {
        if (parent == null) transform.SetParent(null);
        else transform.SetParent(parent.transform, worldPositionStays: true);
    }

    public bool IsTypeOf<T>() where T : BaseObject
    {
        return this is T;
    }

    public bool IsTypeOf(string typeName)
    {
        return GetType().Name == typeName || GetType().FullName == typeName;
    }

    public List<BaseObject> FindAllChildren(bool recursive = true)
    {
        var result = new List<BaseObject>();
        if (recursive)
        {
            var comps = GetComponentsInChildren<BaseObject>(includeInactive: true);
            foreach (var c in comps)
            {
                if (c != this) result.Add(c);
            }
        }
        else
        {
            foreach (Transform t in transform)
            {
                var bo = t.GetComponent<BaseObject>();
                if (bo != null) result.Add(bo);
            }
        }
        return result;
    }

    public BaseObject FindChildById(string id, bool recursive = true)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (recursive)
        {
            var comps = GetComponentsInChildren<BaseObject>(includeInactive: true);
            foreach (var c in comps)
            {
                if (c.Id == id) return c;
            }
            return null;
        }
        else
        {
            foreach (Transform t in transform)
            {
                var bo = t.GetComponent<BaseObject>();
                if (bo != null && bo.Id == id) return bo;
            }
            return null;
        }
    }

    public void SetExtensionData<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key)) return;
        var json = JsonUtility.ToJson(new Wrapper<T> { value = value });
        var entry = _extensionData.Find(e => e.key == key);
        if (entry != null) entry.jsonValue = json;
        else _extensionData.Add(new ExtensionEntry { key = key, jsonValue = json });
    }

    public T GetExtensionData<T>(string key, T defaultValue = default)
    {
        var entry = _extensionData.Find(e => e.key == key);
        if (entry == null || string.IsNullOrEmpty(entry.jsonValue)) return defaultValue;
        try
        {
            var w = JsonUtility.FromJson<Wrapper<T>>(entry.jsonValue);
            return w != null ? w.value : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    [Serializable]
    private class SerializationWrapper
    {
        public string type;
        public string data;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T value;
    }
}
