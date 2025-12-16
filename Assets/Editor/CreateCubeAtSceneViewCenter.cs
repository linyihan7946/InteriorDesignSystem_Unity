using UnityEditor;
using UnityEngine;

public static class CreateCubeAtSceneViewCenter
{
    [MenuItem("Tools/Create Cube at Scene View Center")]
    public static void CreateAtSceneViewCenter()
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null)
        {
            Debug.LogWarning("没有可用的 SceneView。请打开 Scene 视图后重试。");
            return;
        }

        Vector3 position = sv.pivot;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one;
        cube.name = "Cube_SceneViewCenter";

        Undo.RegisterCreatedObjectUndo(cube, "Create Cube at Scene View Center");
        Selection.activeGameObject = cube;
    }
}
