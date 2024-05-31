using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SegmentHelper
{

    public static Tuple<Segment, bool> FindNearestSegment(IList<Segment> segments, Vector2 worldPos)
    {
        Segment segment = null;
        float minDist = float.MaxValue;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 v1 = segments[i].Vertex1.WorldPosition;
            Vector2 v2 = segments[i].Vertex2.WorldPosition;

            float sqrDist = Geom2D.PointToLineSqrDist(worldPos, v1, v2);
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                segment = segments[i];
            }
            else if (sqrDist == minDist) //find best candidate of multiple segments with same distance
            {
                //the steeper the angle (--> 0) the less reliable the result is - always take bigger angle (smaller dot product)
                Vector2 connection;
                Vector2 current;
                Vector2 next;
                if (segment.Vertex2 == segments[i].Vertex1)
                {
                    connection = segment.Vertex2.WorldPosition;
                    current = segment.Vertex1.WorldPosition;
                    next = segments[i].Vertex2.WorldPosition;
                }
                else if (segment.Vertex1 == segments[i].Vertex1)
                {
                    connection = segment.Vertex1.WorldPosition;
                    current = segment.Vertex2.WorldPosition;
                    next = segments[i].Vertex2.WorldPosition;
                }
                else if (segment.Vertex2 == segments[i].Vertex2)
                {
                    connection = segment.Vertex2.WorldPosition;
                    current = segment.Vertex1.WorldPosition;
                    next = segments[i].Vertex1.WorldPosition;
                }
                else
                {
                    connection = segment.Vertex1.WorldPosition;
                    current = segment.Vertex2.WorldPosition;
                    next = segments[i].Vertex1.WorldPosition;
                }

                Vector2 lhs = (connection - worldPos).normalized;
                Vector2 rhscurrent = (current - connection).normalized;
                Vector2 rhsnext = (next - connection).normalized;

                float dotcurrent = Vector2.Dot(lhs, rhscurrent);
                float dotnext = Vector2.Dot(lhs, rhsnext);

                if (dotnext < dotcurrent)
                    segment = segments[i];
            }
            else
            {
                //nop
            }
        }
        bool left = Geom2D.IsCcw(segment.Vertex1.WorldPosition, segment.Vertex2.WorldPosition, worldPos);
        Tuple<Segment, bool> nearest = new Tuple<Segment, bool>(segment, left);

        return nearest;
    }

    public static List<Segment> FindInnerSegments(IList<Segment> segments, List<Vector2> contour, List<Tuple<Segment, bool>> ignore)
    {
        List<Segment> innerSegments = new List<Segment>();
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 point = segments[i].Vertex1.WorldPosition + (segments[i].Vertex2.WorldPosition - segments[i].Vertex1.WorldPosition) * 0.5f;
            if (
                Geom2D.IsInside(contour, point) &&
                ignore.Where(x => x.Item1 == segments[i]).FirstOrDefault() == null
                //Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) &&
                //Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) &&
                //!ignore.Contains(segments[i])
                )
                innerSegments.Add(segments[i]); //TODO is this sufficient?
        }
        return innerSegments;
    }

    public static List<Segment> FindOuterSegments(IList<Segment> segments, List<Vector2> contour, List<Tuple<Segment, bool>> ignore)
    {
        List<Segment> outerSegments = new List<Segment>();
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 point = segments[i].Vertex1.WorldPosition + (segments[i].Vertex2.WorldPosition - segments[i].Vertex1.WorldPosition) * 0.5f;
            if (
                !Geom2D.IsInside(contour, point) &&
                ignore.Where(x => x.Item1 == segments[i]).FirstOrDefault() == null
                //!Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) &&
                //!Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) &&
                //!ignore.Contains(segments[i])
                )
                outerSegments.Add(segments[i]); //TODO is this sufficient?
        }
        return outerSegments;
    }

    public static void RemoveInnerSegments(List<Segment> segments, List<Vector2> contour, List<Tuple<Segment, bool>> ignore)
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (
                (Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) ||
                Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) ||
                (contour.Contains(segments[i].Vertex1.WorldPosition) && contour.Contains(segments[i].Vertex2.WorldPosition)) //edge case
                ) &&
                ignore.Where(x => x.Item1 == segments[i]).FirstOrDefault() == null
                //!ignore.Contains(segments[i])
                )
                segments.RemoveAt(i); //TODO is this sufficient?
        }
    }

}
