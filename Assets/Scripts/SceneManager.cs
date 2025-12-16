using System.Collections.Generic;
using UnityEngine;

public static class SceneManager
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (!Application.isPlaying)
            return;

        // 生成墙体
        WallObject leftWall = new WallObject();
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(0f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            leftWall.height = height;
            leftWall.thickness = thickness;
            leftWall.centerLine = new SegmentObject();
            leftWall.centerLine.startPoint = start;
            leftWall.centerLine.endPoint = end;
            leftWall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start + new Vector3(thickness / 2f, 0f, 0f)); 
                pts.Add(end + new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(end - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                leftWall.contour.SetContourPoints(pts);
            }
        }
        WallObject rightWall = new WallObject();
        {
            Vector3 start = new Vector3(6000f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            rightWall.height = height;
            rightWall.thickness = thickness;
            rightWall.centerLine = new SegmentObject();
            rightWall.centerLine.startPoint = start;
            rightWall.centerLine.endPoint = end;
            rightWall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start + new Vector3(thickness / 2f, 0f, 0f)); 
                pts.Add(end + new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(end - new Vector3(thickness / 2f, 0f, 0f));
                pts.Add(start - new Vector3(thickness / 2f, 0f, 0f));
                rightWall.contour.SetContourPoints(pts);
            }
        }
        WallObject topWall = new WallObject();// 上
        {
            Vector3 start = new Vector3(0f, 4000f, 0f);
            Vector3 end = new Vector3(6000f, 4000f, 0f);
            float thickness = 120f;
            float height = 3000f;
            topWall.height = height;
            topWall.thickness = thickness;
            topWall.centerLine = new SegmentObject();
            topWall.centerLine.startPoint = start;
            topWall.centerLine.endPoint = end;
            topWall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start + new Vector3(0f, thickness / 2f, 0f)); 
                pts.Add(end + new Vector3(0f, thickness / 2f, 0f));
                pts.Add(end - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                topWall.contour.SetContourPoints(pts);
            }
        }
        WallObject bottomWall = new WallObject();// 下
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(6000f, 0f, 0f);
            float thickness = 120f;
            float height = 3000f;
            bottomWall.height = height;
            bottomWall.thickness = thickness;
            bottomWall.centerLine = new SegmentObject();
            bottomWall.centerLine.startPoint = start;
            bottomWall.centerLine.endPoint = end;
            bottomWall.contour = new CompositeLine();
            {
                List<Vector3> pts = new List<Vector3>();
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start + new Vector3(0f, thickness / 2f, 0f)); 
                pts.Add(end + new Vector3(0f, thickness / 2f, 0f));
                pts.Add(end - new Vector3(0f, thickness / 2f, 0f));
                pts.Add(start - new Vector3(0f, thickness / 2f, 0f));
                bottomWall.contour.SetContourPoints(pts);
            }
        }
    }
}
