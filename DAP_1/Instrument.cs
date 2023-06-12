using DAP_1.Analysis;
using System;
using System.Collections.Generic;
using System.Drawing;
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

   

        public int index = 0;

        public int gridIndex = 0;
        public gPoint gridPoint;
        public gPoint centerPoint;
        public vdCircle circle;
        public double distance;
        public MatrixPoint mp;


        public List<MatrixPoint> mps = new List<MatrixPoint>();


        public double distanceFromDestination;

        public  List<SubRoute> routes = new List<SubRoute>();

        public List<Point> point = new List<Point>();

        public Instrument(vdCircle circle, int index)
        {
            this.circle = circle;
            this.centerPoint = circle.Center;
            this.index = index;
        }

        public int CompareTo(object obj)
        {
            return distanceFromDestination.CompareTo(obj);
        }
    }
}
