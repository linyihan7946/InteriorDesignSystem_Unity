using UnityEngine;

public static class CreateCubeOnPlay
{
    private const string CubeName = "AutoCreatedCube";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnRuntimeLoaded()
    {
        if (!Application.isPlaying)
            return;

        if (GameObject.Find(CubeName) != null)
            return;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = CubeName;
        cube.transform.position = Vector3.zero;
    }
}
