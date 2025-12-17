using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// MaterialManager: 运行时与编辑器下的材质管理工具
/// - 运行时：创建运行时材质、分配、修改、查询场景中使用的材质
/// - 编辑器：创建/查找/删除材质资产，列出工程内材质
/// </summary>
public static class MaterialManager
{
    // 运行时：创建一个临时材质（不写入磁盘）
    public static Material CreateRuntimeMaterial(string name, Shader shader = null, Color? color = null)
    {
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader)
        {
            name = string.IsNullOrEmpty(name) ? "RuntimeMaterial" : name
        };
        if (color.HasValue) SetMaterialColor(mat, color.Value);
        return mat;
    }

    // 运行时：设置材质主颜色（同时尝试常见属性名）
    public static void SetMaterialColor(Material mat, Color color)
    {
        if (mat == null) return;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
    }

    // 运行时：为 GameObject 分配材质（可选择是否替换所有子 MeshRenderer）
    public static void AssignMaterial(GameObject go, Material mat, bool applyToAllRenderers = true)
    {
        if (go == null || mat == null) return;
        if (applyToAllRenderers)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                // 为避免修改共享材质，实例化每个 renderer 的材质数组
                var mats = r.materials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.materials = mats;
            }
        }
        else
        {
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                var mats = r.materials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.materials = mats;
            }
        }
    }

    // 运行时：列出场景中所有正在使用的材质（来自 Renderer）
    public static List<Material> ListMaterialsInScene()
    {
        var list = new List<Material>();
        var rends = Object.FindObjectsOfType<Renderer>();
        foreach (var r in rends)
        {
            var mats = r.sharedMaterials;
            if (mats == null) continue;
            foreach (var m in mats)
            {
                if (m != null && !list.Contains(m)) list.Add(m);
            }
        }
        return list;
    }

    // 运行时：查找指定名称的材质（优先场景 shared 材质）
    public static Material FindMaterialInScene(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var all = ListMaterialsInScene();
        foreach (var m in all) if (m.name == name) return m;
        return null;
    }

#if UNITY_EDITOR
    // 编辑器：创建材质资产（path: "Assets/.../name.mat"）
    public static Material CreateMaterialAsset(string assetPath, Shader shader = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(assetPath) };
        if (color.HasValue) SetMaterialColor(mat, color.Value);
        // Ensure directory exists
        var dir = System.IO.Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
        {
            // create folders recursively
            var parts = dir.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
        AssetDatabase.CreateAsset(mat, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
    }

    // 编辑器：根据名称查找工程内材质资产（返回路径列表或 Material 列表）
    public static List<string> FindMaterialAssetPathsByName(string name)
    {
        var paths = new List<string>();
        if (string.IsNullOrEmpty(name)) return paths;
        var guids = AssetDatabase.FindAssets("t:Material " + name);
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null && m.name == name) paths.Add(p);
        }
        return paths;
    }

    public static List<Material> FindMaterialAssetsByName(string name)
    {
        var list = new List<Material>();
        var paths = FindMaterialAssetPathsByName(name);
        foreach (var p in paths) list.Add(AssetDatabase.LoadAssetAtPath<Material>(p));
        return list;
    }

    // 编辑器：删除材质资产（返回是否成功）
    public static bool DeleteMaterialAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return false;
        return AssetDatabase.DeleteAsset(assetPath);
    }

    // 编辑器：列出工程内所有材质资产路径
    public static List<string> ListAllMaterialAssetPaths()
    {
        var list = new List<string>();
        var guids = AssetDatabase.FindAssets("t:Material");
        foreach (var g in guids) list.Add(AssetDatabase.GUIDToAssetPath(g));
        return list;
    }

    // 编辑器：将运行时材质保存为资产
    public static Material SaveMaterialToAsset(Material mat, string assetPath)
    {
        if (mat == null || string.IsNullOrEmpty(assetPath)) return null;
        var copy = new Material(mat) { name = System.IO.Path.GetFileNameWithoutExtension(assetPath) };
        AssetDatabase.CreateAsset(copy, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
    }
#endif
}
