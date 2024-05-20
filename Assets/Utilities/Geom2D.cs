using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geom2D
{
    public static bool DoIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        //identical segments are not allowed
        if ((p1 == q1 && p2 == q2) || (p1 == q2 && p2 == q1))
            return true;

        //touching segments can't intersect
        if (p1 == q1 || p1 == q2 || p2 == q1 || p2 == q2)
            return false;

        return (IsCcw(p1, q1, q2) != IsCcw(p2, q1, q2)) && (IsCcw(p1, p2, q1) != IsCcw(p1, p2, q2));
    }

    public static bool IsCcw(Vector2 a, Vector2 b, Vector2 c)
    {
        //2d cross product: Nz = Ax*By - Ay*Bx
        return ((b.x - a.x) * (c.y - a.y)) > ((b.y - a.y) * (c.x - a.x));
    }

    public static Vector2 CalculateRightNormal(Vector2 p1, Vector2 p2, float length)
    {
        Vector2 normal = new Vector2(-(p2.y - p1.y), p2.x - p1.x);
        normal.Normalize();
        normal *= length;
        return normal;
    }
    public static Vector2 SplitSegment(Vector2 p1, Vector2 p2)
    {
        return SplitSegment(p1, p2, 0.5f);
    }
    public static Vector2 SplitSegment(Vector2 p1, Vector2 p2, float factor)
    {
        return new Vector2(p1.x + factor * (p2.x - p1.x), p1.y + factor * (p2.y - p1.y));
    }

    public static float PointToLineDist(Vector2 point, Vector2 p1, Vector2 p2)
    {
        float dist = PointToLineSqrDist(point, p1, p2);
        return Mathf.Sqrt(dist);
    }

    public static Vector2 ProjectPointToLine(Vector2 point, Vector2 p1, Vector2 p2)
    {
        Vector2 linep1p2 = p2 - p1;
        //        float lengthp1p2 = linep1p2.magnitude;
        float sqrLengthp1p2 = linep1p2.sqrMagnitude;
        linep1p2.Normalize();

        Vector2 linep1pt = point - p1;
        float dot = Vector2.Dot(linep1pt, linep1p2);

        Vector2 pointOnLine;
        //        if (dot > lengthp1p2)
        if (dot * dot > sqrLengthp1p2)
            pointOnLine = p2;
        else if (dot < 0)
            pointOnLine = p1;
        else
        {
            Vector2 projected = linep1p2 * dot;
            pointOnLine = p1 + projected;
        }
        return pointOnLine;
    }

    public static float PointToLineSqrDist(Vector2 point, Vector2 p1, Vector2 p2)
    {
        Vector2 pointOnLine = ProjectPointToLine(point, p1, p2);
        Vector2 dist = point - pointOnLine;
        return dist.sqrMagnitude;
    }

    public static bool IsInside(List<Vector2> polygon, Vector2 point)
    {
        bool isInside = false;
        for (int i = 0; i < polygon.Count; i++)
        {
            int j = (i + 1) % polygon.Count;
            if (polygon[i].y < point.y && polygon[j].y >= point.y || polygon[j].y < point.y && polygon[i].y >= point.y)
            {
                if (polygon[i].x + (point.y - polygon[i].y) /
                                   (polygon[j].y - polygon[i].y) *
                                   (polygon[j].x - polygon[i].x) < point.x)
                {
                    isInside = !isInside;
                }
            }
        }
        return isInside;
    }

}
