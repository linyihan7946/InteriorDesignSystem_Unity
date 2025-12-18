using System;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ModelObject : BaseObject
{
    // 本地 FBX 资源路径或 Resources 路径（例如 "Assets/Models/my.fbx" 或 "Models/my" 用于 Resources.Load）
    public string fbxPath;
    public float scale = 1f;

    public ModelObject(string json = null) : base(json) { }

    public static ModelObject Create(string json = null, string name = null)
    {
        return CreateFromJson<ModelObject>(json, name);
    }

    public override void Rebuild()
    {
        // 清除已有同名子对象
        var existing = transform.Find(this.ObjectName);
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        if (string.IsNullOrEmpty(fbxPath)) return;

        // 编辑器优先使用 AssetDatabase 加载项目内模型（支持直接使用 Assets/... 路径或文件系统绝对路径）
#if UNITY_EDITOR
        try
        {
            GameObject asset = null;
            if (fbxPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            }
            else
            {
                // 如果是文件系统绝对路径或其他位置，尝试按文件名在 AssetDatabase 中搜索
                if (File.Exists(fbxPath))
                {
                    var fileName = Path.GetFileNameWithoutExtension(fbxPath);
                    var guids = AssetDatabase.FindAssets(fileName + " t:Model");
                    if (guids != null && guids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                }
            }

            if (asset != null)
            {
                // 在编辑器中实例化为 prefab 实例（保留 prefab 关联）
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, this.transform);
                if (inst == null)
                {
                    inst = GameObject.Instantiate(asset, this.transform);
                }
                inst.name = this.ObjectName;
                inst.transform.localPosition = Vector3.zero;
                // 沿着x轴旋转90度
                inst.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                inst.transform.localScale = Vector3.one * 100000f;
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("ModelObject editor load failed: " + ex.Message);
        }
#endif

        // 运行时：尝试通过 Resources.Load 加载（资源必须放在 Assets/Resources 下，传入路径应为 Resources 下的相对路径且不含扩展名）
        try
        {
            string resPath = fbxPath;
            // 如果包含 "Resources/"，截取其后的相对路径
            var idx = resPath.IndexOf("Resources", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                // 路径示例： Assets/Resources/Models/t1.fbx -> Models/t1
                resPath = resPath.Substring(idx + "Resources".Length);
                resPath = resPath.TrimStart('/', '\\');
            }
            // remove extension if present
            resPath = Path.ChangeExtension(resPath, null);
            // ensure forward slashes for Resources.Load
            resPath = resPath.Replace('\\', '/');
            var prefab = Resources.Load<GameObject>(resPath);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab, this.transform);
                go.name = this.ObjectName;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * scale;
                return;
            }
        }
        catch { }

        // 无法加载，创建占位物体
        CreatePlaceholder(fbxPath);
    }

    private void CreatePlaceholder(string path)
    {
        var go = new GameObject(this.ObjectName);
        go.transform.SetParent(this.transform, worldPositionStays: true);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * scale;
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = ModelingUtility.CreateCube("_placeholder", new Vector3(1,1,1), Vector3.zero).GetComponent<MeshFilter>().sharedMesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = MaterialManager.CreateRuntimeMaterial("ModelPlaceholder", null, Color.magenta);
        var txt = new GameObject("info");
        txt.transform.SetParent(go.transform, false);
        var t = txt.AddComponent<TextMesh>();
        t.text = "Model placeholder\n" + Path.GetFileName(path);
        t.characterSize = 0.1f;
        t.anchor = TextAnchor.UpperCenter;
    }
}
