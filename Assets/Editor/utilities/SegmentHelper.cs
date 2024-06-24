using System;
using System.Collections.Generic;
using UnityEngine;

public static class SegmentHelper
{

    public static Tuple<Segment, bool> FindNearestSegment(IList<Segment> segments, Vector2 worldPos)
    {
        if (segments == null) return null;

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
        Tuple<Segment, bool> nearest = null;
        if (segment != null)
        {
            bool left = Geom2D.IsCcw(segment.Vertex1.WorldPosition, segment.Vertex2.WorldPosition, worldPos);
            nearest = new Tuple<Segment, bool>(segment, left);
        }

        return nearest;
    }
 
}
