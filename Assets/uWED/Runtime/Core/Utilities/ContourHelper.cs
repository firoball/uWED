using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ContourHelper
{

    public static List<Vertex> FindContour(Contour contour, Tuple<Segment, bool> nearest, int limit)
    {
        List<Vertex> vertices = new List<Vertex>();

        //iterate through connected segments
        Tuple<Segment, bool> first = nearest;
        Tuple<Segment, bool> current = first;
        do
        {
            current = FindNextSegment(current);
            Vertex newVertex;
            if (current.Item2) //left sided segment
            {
                newVertex = current.Item1.Vertex1;
                current.Item1.CLeft = contour;
            }
            else //right sided segment
            {
                newVertex = current.Item1.Vertex2;
                current.Item1.CRight = contour;
            }

            if (vertices.Count > 0 && newVertex.WorldPosition == vertices[^1].WorldPosition)
            {
                // layered neighbor vertex  found - just ignore it
                Debug.LogWarning($"Removed duplicate Vertex at {newVertex.WorldPosition}");
            }
            else
            {
                // add the new vertex
                vertices.Add(newVertex);
            }
        } while (
            (current.Item1 != first.Item1 || current.Item2 != first.Item2) &&
            vertices.Count <= limit //avoid endless loop in case of some weird error
            );

        return vertices;
    }

    public static void FindInnerContour(Contour outer, List<Contour> inners)
    {
        /* this is a naive implementation which checks for all known inner contours whether
         * they are contained by the outer contour.
         * If this is the case, contour is set as parent of the inner contour.
         * In case the inner contour already has a parent, it has to be tested whether the parent
         * assignment needs to be updated.
         * The parent to choose is the contour which is closest to the inner contour under investigation.
         * This is done by checking whether the given contour is inside the parent contour of the inner contour.
         * 
         * Since the contains-check allows for touching segments, isolated segments which have 
         * identical inner and outer contours (expect for direction) can lead to errors.
         * Therefore, these cases have to be excluded. 
         * This is done by naively matching each single Vertex of both contours.
         * 
         * Running this function on a high count of contours is rather SLOW and 
         * should NOT be done at every render/repaint event.
         */

        //check all "inner" contours
        for (int i = 0; i < inners.Count; i++)
        {
            //"inner" contour is inside current contour?
            if (outer.Contains(inners[i]))
            {
                //"inner" contour already has some parent?
                if (inners[i].Outer != null)
                {
                    //find correct parent - innermost of outer contours is the correct parent
                    if (inners[i].Outer.Contains(outer)) //current contour is inside current parent
                    {
                        //isolated segment loops have identical inner/outer contours - Containment check not reliable in this case
                        //these cases must explicitly be excluded from re-parenting
                        if (!IsFlippedContour(inners[i], outer)) //make sure contour is not same as other (except direction)
                        {
                            inners[i].Outer.Unlink(inners[i]); //remove child from old parent
                            inners[i].Outer = outer; //switch parent
                            outer.Link(inners[i]); //add child to new parent
                        }
                    }
                }
                else
                {
                    inners[i].Outer = outer; //set new parent
                    outer.Link(inners[i]); //add child to parent
                }
            }
        }
    }

    public static bool IsInside(List<Vertex> polygon, Vertex point)
    {
        bool isInside = false;
        for (int i = 0; i < polygon.Count; i++)
        {
            int j = (i + 1) % polygon.Count;
            if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y || 
                polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
            {
                if (polygon[i].X + (point.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) *
                    (polygon[j].X - polygon[i].X) < point.X)
                {
                    isInside = !isInside;
                }
            }
        }
        return isInside;
    }

    private static bool IsFlippedContour(Contour c1, Contour c2)
    {
        //different vertex count - can't be a match
        if (c1.Vertices.Count != c2.Vertices.Count)
            return false;

        //naively match all vertices
        for (int i = 0; i < c1.Vertices.Count; i++)
        {
            //Vertex not found in other contour - can't be a match
            if (!c2.Vertices.Contains(c1.Vertices[i]))
                return false;
        }
        //contours are flipped identical
        return true;
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
        //clockwise (in front)
        float cw = float.MaxValue;
        Segment scw = null;
        bool scwLeft = false;

        //counter clock wise (behind)
        float ccw = float.MinValue;
        Segment sccw = null;
        bool sccwLeft = false;

        Vector2 lhs = ((Vector2)v2 - v1).normalized;
        for (int i = 0; i < v2.Connections.Count; i++)
        {
            if (v2.Connections[i] != segment) //skip current segment
            {
                Vector2 rhs;
                bool isLeftOfSegment;
                bool nextLeft;
                if (v2.Connections[i].Vertex1 == v2) //use right side of connected segment 
                {
                    rhs = ((Vector2)v2.Connections[i].Vertex2- v2).normalized;
                    isLeftOfSegment = Geom2D.IsCcw(v1, v2, v2.Connections[i].Vertex2);
                    nextLeft = false;
                }
                else //use left side of connected segment 
                {
                    rhs = ((Vector2)v2.Connections[i].Vertex1 - v2).normalized;
                    isLeftOfSegment = Geom2D.IsCcw(v1, v2, v2.Connections[i].Vertex1);
                    nextLeft = true;
                }

                float newdot = Vector2.Dot(lhs, rhs);
                if (!isLeftOfSegment && newdot < cw) //angle is smallest and < 180� (in front of current segment)
                {
                    cw = newdot;
                    scw = v2.Connections[i];
                    scwLeft = nextLeft;
                }
                if (isLeftOfSegment && newdot > ccw) //angle is greatest and > 180� (behind current segment)
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