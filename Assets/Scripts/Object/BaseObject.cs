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
    public Matrix4x4 Matrix3D { get => _matrix3D; set => _matrix3D = value; }

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
