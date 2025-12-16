using UnityEngine;

public class WallObject : BaseObject
{
    public SegmentObject centerLine;
    public float thickness = 0.2f;
    public float height = 2.8f;
    public CompositeLine contour;

    // 可扩展：通过 centerLine 和 thickness 生成轮廓
}
