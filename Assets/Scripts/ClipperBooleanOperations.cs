using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

/// <summary>
/// 基于Clipper库的布尔运算工具类，用于处理CompositeLine对象的几何布尔运算操作
/// </summary>
public static class ClipperBooleanOperations
{
    private const double CLIPPER_SCALE = 1000.0;

    /// <summary>
    /// 对CompositeLine对象执行布尔运算操作
    /// </summary>
    /// <param name="pts1">第一个参数，List<CompositeLine>，第一圈是外圈，后面的是内圈</param>
    /// <param name="pts2">第二个参数，List<List<CompositeLine>>，多个带内圈的内孔</param>
    /// <param name="clipType">布尔运算方式（交、并、差）</param>
    /// <returns>运算结果，List<List<CompositeLine>></returns>
    public static List<List<CompositeLine>> BooleanOperation(
        List<CompositeLine> pts1,
        List<List<CompositeLine>> pts2,
        ClipType clipType)
    {
        var result = new List<List<CompositeLine>>();

        // 检查输入有效性
        if (pts1 == null)
            return result;

        // 将pts1转换为Clipper路径格式
        var subjectPaths = ConvertCompositeLinesToPaths(pts1);
        
        // 将pts2转换为Clipper路径格式
        var clipPaths = new List<List<List<ClipperLib.IntPoint>>>();
        if (pts2 != null)
        {
            foreach (var compositeLineList in pts2)
            {
                if (compositeLineList != null)
                {
                    clipPaths.Add(ConvertCompositeLinesToPaths(compositeLineList));
                }
            }
        }

        // 执行布尔运算
        var clipper = new Clipper();
        
        // 添加主体路径
        foreach (var path in subjectPaths)
        {
            clipper.AddPath(path, PolyType.ptSubject, true);
        }
        
        // 添加裁剪路径
        foreach (var pathGroup in clipPaths)
        {
            foreach (var path in pathGroup)
            {
                clipper.AddPath(path, PolyType.ptClip, true);
            }
        }

        // 执行运算
        var solution = new List<List<IntPoint>>();
        clipper.Execute(clipType, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        // 将结果转换回CompositeLine格式
        // 这里简化处理，实际项目中可能需要更复杂的多边形内外圈匹配算法
        var resultList = new List<CompositeLine>();
        foreach (var path in solution)
        {
            var points = ConvertPathToVector3(path);
            if (points != null && points.Count >= 3)
            {
                var compositeLine = CompositeLine.Create(null, "Result_" + resultList.Count);
                compositeLine.SetContourPoints(points, true);
                resultList.Add(compositeLine);
            }
        }

        if (resultList.Count > 0)
        {
            result.Add(resultList);
        }

        return result;
    }

    /// <summary>
    /// 将CompositeLine列表转换为Clipper路径
    /// </summary>
    /// <param name="compositeLines">CompositeLine对象列表</param>
    /// <returns>Clipper路径列表</returns>
    private static List<List<ClipperLib.IntPoint>> ConvertCompositeLinesToPaths(List<CompositeLine> compositeLines)
    {
        var paths = new List<List<ClipperLib.IntPoint>>();

        if (compositeLines == null)
            return paths;

        foreach (var compositeLine in compositeLines)
        {
            if (compositeLine == null)
                continue;

            var points = compositeLine.GetContourPoints();
            if (points == null || points.Count < 3)
                continue;

            var path = new List<ClipperLib.IntPoint>();
            foreach (var point in points)
            {
                long x = (long)System.Math.Round(point.x * CLIPPER_SCALE);
                long y = (long)System.Math.Round(point.z * CLIPPER_SCALE); // 使用Z作为Y坐标
                path.Add(new ClipperLib.IntPoint(x, y));
            }
            paths.Add(path);
        }

        return paths;
    }

    /// <summary>
    /// 将Clipper路径转换为Vector3点列表
    /// </summary>
    /// <param name="path">Clipper路径</param>
    /// <returns>Vector3点列表</returns>
    private static List<Vector3> ConvertPathToVector3(List<ClipperLib.IntPoint> path)
    {
        if (path == null || path.Count < 3)
            return null;

        var points = new List<Vector3>();
        foreach (var point in path)
        {
            float x = (float)point.X / (float)CLIPPER_SCALE;
            float z = (float)point.Y / (float)CLIPPER_SCALE;
            points.Add(new Vector3(x, 0f, z)); // Y坐标设为0
        }

        return points;
    }
}