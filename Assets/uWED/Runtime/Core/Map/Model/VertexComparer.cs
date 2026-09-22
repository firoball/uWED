using System;
using System.Collections.Generic;

public class VertexComparer : Comparer<Vertex>
{
    public override int Compare(Vertex v1, Vertex v2)
    {
        int retVal;
        
        if (v1 != null && v2 == null)
            retVal = 1;
        else if (v1 == null)
            retVal = (v2 == null) ? 0 : -1;
        else
        {
            if (v1.Y > v2.Y)
                retVal = 1;
            else if (v1.Y < v2.Y)
                retVal = -1;
            else
            {
                if (v1.X > v2.X) //TODO: may need to be smaller x wins
                    retVal = 1;
                else if (v1.X < v2.X)
                    retVal = -1;
                else
                    retVal = 0;
            }
            
        }

        return retVal;
    }
}

