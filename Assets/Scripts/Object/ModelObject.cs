using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ModelObject : BaseObject
{
    // glTF 模型地址（支持 http(s) 与 file:// 以及相对/绝对路径）
    public string gltfUrl;
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

        if (string.IsNullOrEmpty(gltfUrl)) return;

        // 启动协程下载并尝试导入
        StartCoroutine(LoadGltfCoroutine(gltfUrl));
    }

    private IEnumerator LoadGltfCoroutine(string url)
    {
        // 支持 file:// 与 http(s)
        Uri uri;
        try { uri = new Uri(url, UriKind.RelativeOrAbsolute); }
        catch { uri = null; }

        byte[] data = null;
        if (uri != null && uri.IsFile)
        {
            var path = uri.LocalPath;
            if (File.Exists(path)) data = File.ReadAllBytes(path);
        }
        else if (uri != null && (uri.Scheme == "http" || uri.Scheme == "https" || uri.IsAbsoluteUri))
        {
            using (var uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    Debug.LogWarning($"ModelObject: failed to download {url}: {uwr.error}");
                }
                else
                {
                    data = uwr.downloadHandler.data;
                }
            }
        }
        else
        {
            // try relative path in project
            try
            {
                if (File.Exists(url)) data = File.ReadAllBytes(url);
            }
            catch { }
        }

        if (data == null || data.Length == 0)
        {
            Debug.LogWarning($"ModelObject: no data loaded from {url}");
            CreatePlaceholder(url);
            yield break;
        }

        // 尝试使用已存在的 glTF 导入器（如果项目中包含） via reflection
        bool imported = false;

        // Try GLTFast: look for type GLTFast.GltfImport
        imported = TryImportWithGLTFast(url);
        if (imported) yield break;

        // Try Siccity GLTFUtility: type "GLTFUtility.Importer" or static method Importer.LoadFromFile / LoadFromBytes
        imported = TryImportWithGLTFUtility(data, url);
        if (imported) yield break;

        // 未找到导入器，保存临时文件并创建占位物体
        var tmpPath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(uri != null ? uri.AbsolutePath : url));
        try { File.WriteAllBytes(tmpPath, data); } catch { tmpPath = null; }

        Debug.LogWarning($"ModelObject: downloaded glTF but no importer found. Saved to: {tmpPath ?? "(save failed)"}; please import with a glTF package.");
        CreatePlaceholder(url);
    }

    // 尝试使用 GLTFast（若存在），通过反射查找并调用 Load/LoadGltf 方法（有限支持）
    private bool TryImportWithGLTFast(string url)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies();
            var t = asm.SelectMany(a =>
            {
                try { return a.GetTypes(); } catch { return Type.EmptyTypes; }
            }).FirstOrDefault(tt => tt.FullName == "GLTFast.GltfImport");
            if (t == null) return false;

            var inst = Activator.CreateInstance(t);
            // look for asynchronous Load methods
            var loadMethod = t.GetMethod("Load", new Type[] { typeof(string) }) ?? t.GetMethod("LoadGltf", new Type[] { typeof(string) });
            if (loadMethod != null)
            {
                var ret = loadMethod.Invoke(inst, new object[] { url });
                // if returns a System.Threading.Tasks.Task, we cannot await here reliably; log and return
                Debug.Log("ModelObject: invoked GLTFast.Load (reflection). Check importer result in scene.");
                return true;
            }
        }
        catch (Exception ex) { Debug.Log("GLTFast import attempt failed: " + ex.Message); }
        return false;
    }

    // 尝试使用 Siccity GLTFUtility（有限支持）
    private bool TryImportWithGLTFUtility(byte[] data, string url)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies();
            var importerType = asm.SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(tt => tt.FullName != null && tt.FullName.Contains("GLTFUtility"));
            if (importerType == null) return false;

            // Try to find a static Importer method that accepts a path or byte[]
            var methods = importerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
            var m = methods.FirstOrDefault(mi => mi.Name.ToLower().Contains("load") && mi.GetParameters().Any(p => p.ParameterType == typeof(string)));
            if (m != null)
            {
                // if path required, save to temp file and call
                var tmp = Path.Combine(Application.temporaryCachePath, Path.GetFileName(url));
                try { File.WriteAllBytes(tmp, data); } catch { tmp = null; }
                if (tmp != null)
                {
                    var isStatic = m.IsStatic;
                    object instance = null;
                    if (!isStatic)
                    {
                        instance = Activator.CreateInstance(importerType);
                    }
                    m.Invoke(instance, new object[] { tmp });
                    Debug.Log("ModelObject: invoked GLTFUtility import (reflection).");
                    return true;
                }
            }
        }
        catch (Exception ex) { Debug.Log("GLTFUtility import attempt failed: " + ex.Message); }
        return false;
    }

    private void CreatePlaceholder(string url)
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
        t.text = "Model placeholder\n" + Path.GetFileName(url);
        t.characterSize = 0.1f;
        t.anchor = TextAnchor.UpperCenter;
    }
}
