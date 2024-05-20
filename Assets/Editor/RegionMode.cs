using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RegionMode : BaseEditorMode
{

    public RegionMode(MapData mapData) : base(mapData, new RegionDrawer(mapData))
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        //BuildRegions();
    }

    /*private void BuildRegions()
    {
        m_mapData.Regions.Clear(); //TODO: temp

        for (int s = 0; s < m_mapData.Segments.Count; s++)
        {
            if (m_mapData.Segments[s].Right == null)
            {
                BuildRightRegion(m_mapData.Segments[s]);
            }
            if (m_mapData.Segments[s].Left == null)
            {
                //BuildLeftContour(s);
            }
        }
    }*/

    /*private void BuildRightRegion(Segment segment) //TODO: redo
    {
        List<List<Segment>> contours = new List<List<Segment>>();
        List<List<Segment>> stitches = new List<List<Segment>>();
        List<Segment> contour = new List<Segment>();

        Region region = new Region();
        segment.Right = region;

        Vertex startVertex = segment.Vertex1;
        Vertex nextVertex = segment.Vertex2;

        do
        {
            if (nextVertex.Connections.Count > 2)
            {
                //Step1: multiple segments meet - start new contour
                contours.Add(contour);
                contour = new List<Segment>();

                //Step2: identify next Segment
                Segment nextSegment;
                //nextSegment = FindNextSegment(segment);

                //Step3: test which vertex of next segment is connected and determine on which side region is
                if (nextVertex == nextSegment.Vertex1) //right side
                {
                    nextVertex = nextSegment.Vertex2;
                    nextSegment.Right = region;
                }
                else //left side
                {
                    nextVertex = nextSegment.Vertex1;
                    nextSegment.Left = region;
                }
                contour.Add(nextSegment);
            }
            else if (nextVertex.Connections.Count == 2)
            {
                //Step1: pick "other" connection
                Segment nextSegment;
                if (nextVertex.Connections[0] == segment)
                    nextSegment = nextVertex.Connections[1];
                else
                    nextSegment = nextVertex.Connections[0];

                //Step2: test which vertex of next segment is connected and determine on which side region is
                if (nextVertex == nextSegment.Vertex1) //right side
                {
                    nextVertex = nextSegment.Vertex2;
                    nextSegment.Right = region;
                }
                else //left side
                {
                    nextVertex = nextSegment.Vertex1;
                    nextSegment.Left = region;
                }
                contour.Add(nextSegment);
            }
            else if (nextVertex == startVertex)
            {
                //contour closed
            }
            else
            {
                nextVertex = null; //TODO: this should be set to vertex of last segment of previously stored contour
                //dead end
                stitches.Add(contour);
                contour = contours[^1];
            }

        } while (nextVertex != startVertex || nextVertex != null);
    }*/

}
