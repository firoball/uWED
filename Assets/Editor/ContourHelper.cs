using System;
using System.Collections.Generic;
using UnityEngine;

public static class ContourHelper
{
    public static List<Vector2> FindContour(Tuple<Segment, bool> nearest, out List<Tuple<Segment, bool>> hoveredSegments, int limit)
    {
        List<Vector2> hoveredContour = new List<Vector2>();
        hoveredSegments = new List<Tuple<Segment, bool>>();

        //iterate through connected segments
        Tuple<Segment, bool> first = nearest;
        Tuple<Segment, bool> current = first;
        do
        {
            current = FindNextSegment(current);
            hoveredSegments.Add(current);
            Vector2 nextPos;
            if (current.Item2) //left sided segment
                nextPos = current.Item1.Vertex1.WorldPosition;
            else
                nextPos = current.Item1.Vertex2.WorldPosition;
            hoveredContour.Add(nextPos);
        } while (
            (current.Item1 != first.Item1 || current.Item2 != first.Item2) &&
            hoveredSegments.Count <= limit //avoid endless loop in case of some weird error
            );

        return hoveredContour;
    }

    private static Tuple<Segment, bool> FindNextSegment(Tuple<Segment, bool> t)
    {
        if (t != null && t.Item1 != null)
        {
            Segment segment = t.Item1;
            bool isLeftSide = t.Item2;

            //calculation is done for right side
            //if current segment is left sided, swap it
            Vertex v1 = segment.Vertex1;
            Vertex v2 = segment.Vertex2;
            if (isLeftSide)
            {
                v1 = segment.Vertex2;
                v2 = segment.Vertex1;
            }

            if (v2.Connections.Count > 2) //multiple segment, pick the one with closest angle
            {
                return PickClosestSegment(segment, v1, v2);
            }
            else if (v2.Connections.Count == 2) //single segment
            {
                return PickOtherSegment(segment, v2);
            }
            else //no further segments, continue on other side of current segment
            {
                return new Tuple<Segment, bool>(segment, !isLeftSide);
            }
        }
        return null;
    }

    private static Tuple<Segment, bool> PickClosestSegment(Segment segment, Vertex v1, Vertex v2)
    {
        //clowise (in front)
        float cw = float.MaxValue;
        Segment scw = null;
        bool scwLeft = false;

        //counter clock wise (behind)
        float ccw = float.MinValue;
        Segment sccw = null;
        bool sccwLeft = false;

        Vector2 lhs = (v2.WorldPosition - v1.WorldPosition).normalized;
        for (int i = 0; i < v2.Connections.Count; i++)
        {
            if (v2.Connections[i] != segment) //skip current segment
            {
                Vector2 rhs;
                bool side;
                bool nextLeft;
                if (v2.Connections[i].Vertex1 == v2) //use right side of connected segment 
                {
                    rhs = (v2.Connections[i].Vertex2.WorldPosition - v2.WorldPosition).normalized;
                    side = Geom2D.IsCcw(v1.WorldPosition, v2.WorldPosition, v2.Connections[i].Vertex2.WorldPosition);
                    nextLeft = false;
                }
                else //use left side of connected segment 
                {
                    rhs = (v2.Connections[i].Vertex1.WorldPosition - v2.WorldPosition).normalized;
                    side = Geom2D.IsCcw(v1.WorldPosition, v2.WorldPosition, v2.Connections[i].Vertex1.WorldPosition);
                    nextLeft = true;
                }

                float newdot = Vector2.Dot(lhs, rhs);
                if (!side && newdot < cw) //angle is smallest and < 180° (in front of current segment)
                {
                    cw = newdot;
                    scw = v2.Connections[i];
                    scwLeft = nextLeft;
                }
                if (side && newdot > ccw) //angle is greatest and > 180° (behind current segment)
                {
                    ccw = newdot;
                    sccw = v2.Connections[i];
                    sccwLeft = nextLeft;
                }
            }
        }

        if (scw != null) //prefer segments "in front"
            return new Tuple<Segment, bool>(scw, scwLeft);
        else
            return new Tuple<Segment, bool>(sccw, sccwLeft);
    }

    private static Tuple<Segment, bool> PickOtherSegment(Segment segment, Vertex v2)
    {
        //Step1: pick "other" connection
        Segment nextSegment;
        if (v2.Connections[0] == segment)
            nextSegment = v2.Connections[1];
        else
            nextSegment = v2.Connections[0];

        //Step2: test which vertex of next segment is connected and determine which side is being looked at
        if (v2 == nextSegment.Vertex1) //right side
            return new Tuple<Segment, bool>(nextSegment, false);
        else //left side
            return new Tuple<Segment, bool>(nextSegment, true);
    }

}