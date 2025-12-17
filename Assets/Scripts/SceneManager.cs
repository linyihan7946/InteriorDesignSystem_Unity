using System.Collections.Generic;
using UnityEngine;

public static class SceneManager
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (!Application.isPlaying)
            return;

        // Unity MonoBehaviours must be created via GameObject.AddComponent — use Create helper
        SceneObject scene = SceneObject.Create(null, "AutoCreatedScene");
        {
            // scene.name = "AutoCreatedScene";
            // scene.transform.position = Vector3.zero;
            // scene.transform.rotation = Quaternion.identity;
            // scene.transform.localScale = Vector3.one;
        }
        FloorObject floor = FloorObject.Create(null, "AutoCreatedFloor");
        {
            // floor.name = "AutoCreatedFloor";
            // floor.transform.parent = scene.transform;
            // floor.transform.localPosition = Vector3.zero;
            // floor.transform.localRotation = Quaternion.identity;
            // floor.transform.localScale = Vector3.one;
            floor.SetParent(scene);
        }
        // 生成墙体
        {// 左
            WallObject wall = WallObject.Create(null, "Wall_Left"); // 左
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(0f, 0f, 4000f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = SegmentObject.Create(null, "CenterLine_Left");
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = CompositeLine.Create(null, "Contour_Left");
            List<Vector3> pts = GeometryUtils.CreateThickSegmentPolygon(start, end, thickness);
            wall.contour.SetContourPoints(pts);
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 右
            WallObject wall = WallObject.Create(null, "Wall_Right");
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(6000f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 0f, 4000f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = SegmentObject.Create(null, "CenterLine_Right");
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = CompositeLine.Create(null, "Contour_Right");
            List<Vector3> pts = GeometryUtils.CreateThickSegmentPolygon(start, end, thickness);
            wall.contour.SetContourPoints(pts);
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 上
            WallObject wall = WallObject.Create(null, "Wall_Top");// 上
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 0f, 4000f);
            Vector3 end = new Vector3(6000f, 0f, 4000f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = SegmentObject.Create(null, "CenterLine_Top");
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = CompositeLine.Create(null, "Contour_Top");
            List<Vector3> pts = GeometryUtils.CreateThickSegmentPolygon(start, end, thickness);
            wall.contour.SetContourPoints(pts);
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 下
            WallObject wall = WallObject.Create(null, "Wall_Bottom");// 下
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 0f, 0f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = SegmentObject.Create(null, "CenterLine_Bottom");
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = CompositeLine.Create(null, "Contour_Bottom");
            List<Vector3> pts = GeometryUtils.CreateThickSegmentPolygon(start, end, thickness);
            wall.contour.SetContourPoints(pts);
            wall.SetParent(floor);
            wall.Rebuild();
        }
    }
}
