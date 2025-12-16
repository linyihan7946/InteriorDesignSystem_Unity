using System.Collections.Generic;
using UnityEngine;

public static class SceneManager
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (!Application.isPlaying)
            return;

        SceneObject scene = new SceneObject();
        {
            // scene.name = "AutoCreatedScene";
            // scene.transform.position = Vector3.zero;
            // scene.transform.rotation = Quaternion.identity;
            // scene.transform.localScale = Vector3.one;
        }
        FloorObject floor = new FloorObject();
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
            WallObject wall = new WallObject();// 左
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(0f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = new SegmentObject();
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start + new Vector3(thickness / 2f, 0f, 0f)); 
                pts.Add(end + new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(end - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                wall.contour.SetContourPoints(pts);
            }
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 右
            WallObject wall = new WallObject();
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(6000f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = new SegmentObject();
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start + new Vector3(thickness / 2f, 0f, 0f)); 
                pts.Add(end + new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(end - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                wall.contour.SetContourPoints(pts);
            }
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 上
            WallObject wall = new WallObject();// 上
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 4000f, 0f);
            Vector3 end = new Vector3(6000f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = new SegmentObject();
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start + new Vector3(0f, thickness / 2f, 0f)); 
                pts.Add(end + new Vector3(0f, thickness / 2f, 0f));
                pts.Add(end - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                wall.contour.SetContourPoints(pts);
            }
            wall.SetParent(floor);
            wall.Rebuild();
        }
        {// 下
            WallObject wall = new WallObject();// 下
            wall.ObjectName = wall.Id;
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 0f, 0f);
            float thickness = 120f;
            float height = 3000f;
            wall.height = height;
            wall.thickness = thickness;
            wall.centerLine = new SegmentObject();
            wall.centerLine.startPoint = start;
            wall.centerLine.endPoint = end;
            wall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start + new Vector3(0f, thickness / 2f, 0f)); 
                pts.Add(end + new Vector3(0f, thickness / 2f, 0f));
                pts.Add(end - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                wall.contour.SetContourPoints(pts);
            }
            wall.SetParent(floor);
            wall.Rebuild();
        }
    }
}
