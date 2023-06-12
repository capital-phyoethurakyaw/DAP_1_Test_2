using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorDraw.Geometry;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdPrimaries;

namespace DAP_1
{
    public class Instrument :   IComparable
    {
        public int gridIndex = 0;
        public gPoint gridPoint;
        public gPoint centerPoint;
        public vdCircle circle;
        public double distance;

        public double distanceFromDestination;

        public Instrument(vdCircle circle)
        {
            this.circle = circle;
            this.centerPoint = circle.Center;

        }

        public int CompareTo(object obj)
        {
            return distanceFromDestination.CompareTo(obj);
        }
    }
}
