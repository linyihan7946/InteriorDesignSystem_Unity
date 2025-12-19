using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public static class SceneManager
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (!Application.isPlaying)
            return;

        SceneManager.CreateRectRoom();
    }

    // 创建一个矩形房间场景
    private static void CreateRectRoom()
    {
        // Unity MonoBehaviours must be created via GameObject.AddComponent — use Create helper
        SceneObject scene = SceneObject.Create(null, "AutoCreatedScene");
        FloorObject floor = FloorObject.Create(null, "AutoCreatedFloor");
        floor.SetParent(scene);
        // 生成墙体
        float thickness = ProjectConfig.Instance.wallDefaultThickness;
        float height = ProjectConfig.Instance.wallDefaultHeight;
        List<CompositeLine> compositeLines = new List<CompositeLine>();
        {// 左
            WallObject wall = SceneManager.CreateWall("Wall_Left",
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 4000f),
                height,
                thickness
            );
            wall.SetParent(floor);
            wall.Rebuild();
            compositeLines.Add(wall.contour);
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
            compositeLines.Add(wall.contour);
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
            compositeLines.Add(wall.contour);
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
            compositeLines.Add(wall.contour);
        }


        var result = RegionSearcher_Clipper.SearchRoomsByPolygons(compositeLines);
        
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
        
        // 天花
        CeilingObject ceiling = CeilingObject.Create(null, "AutoCreatedCeiling");
        ceiling.contours = new CompositeLine[] { line };
        ceiling.Elevation = height;
        ceiling.SetParent(floor);
        ceiling.Rebuild();
        
        // 模型
        ModelObject model = ModelObject.Create(null, "AutoCreatedModel");
        model.SetParent(floor);
        model.fbxPath = "E:\\GitHubWorkSpace\\Tuanjie_Projects\\InteriorDesignSystem_Unity\\Assets\\Models\\test\\t1.fbx";
        model.Rebuild();
        // 将模型平移到(2000, 0, 2000)
        model.ApplyMatrix(Matrix4x4.TRS(new Vector3(2000f, 0f, 2000f), Quaternion.identity, Vector3.one));

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