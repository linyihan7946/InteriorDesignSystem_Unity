using System.Collections.Generic;
using Unity.VisualScripting;
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
        FloorObject floor = FloorObject.Create(null, "AutoCreatedFloor");
        floor.SetParent(scene);
        // 生成墙体
        float thickness = ProjectConfig.Instance.wallDefaultThickness;
        float height = ProjectConfig.Instance.wallDefaultHeight;
        {// 左
            WallObject wall = SceneManager.CreateWall("Wall_Left",
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 4000f),
                height,
                thickness
            );
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 右
            WallObject wall = SceneManager.CreateWall("Wall_Right",
                new Vector3(6000f, 0f, 0f),
                new Vector3(6000f, 0f, 4000f),
                height,
                thickness
            );
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 上
            WallObject wall = SceneManager.CreateWall("Wall_Top",
                new Vector3(0f, 0f, 4000f),
                new Vector3(6000f, 0f, 4000f),
                height,
                thickness
            );
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 下
            WallObject wall = SceneManager.CreateWall("Wall_Bottom",
                new Vector3(0f, 0f, 0f),
                new Vector3(6000f, 0f, 0f),
                height,
                thickness
            );
            wall.SetParent(floor);
            wall.Rebuild();
        }
        // 地板
        CompositeLine line = CompositeLine.Create(null, "GroundContour");
        List<Vector3> pts = new List<Vector3>
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(6000f, 0f, 0f),
            new Vector3(6000f, 0f, 4000f),
            new Vector3(0f, 0f, 4000f),
            new Vector3(0f, 0f, 0f),
        };
        line.SetContourPoints(pts);
        GroundObject ground = GroundObject.Create(null, "AutoCreatedGround");
        ground.contours = new CompositeLine[] { line };
        ground.SetParent(floor);
        ground.Rebuild();
    }

    private static WallObject CreateWall(string name, Vector3 start, Vector3 end, float height, float thickness)
    {
        WallObject wall = WallObject.Create(null, name);
        wall.ObjectName = wall.Id;
        wall.height = height;
        wall.thickness = thickness;
        wall.centerLine = SegmentObject.Create(null, "CenterLine_" + name);
        wall.centerLine.startPoint = start;
        wall.centerLine.endPoint = end;
        wall.contour = CompositeLine.Create(null, "Contour_" + name);
        List<Vector3> pts = GeometryUtils.CreateThickSegmentPolygon(start, end, thickness);
        wall.contour.SetContourPoints(pts);
        return wall;
    }
}