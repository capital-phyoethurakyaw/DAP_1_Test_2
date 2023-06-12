using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vdControls;
using VectorDraw.Geometry;
using VectorDraw.Professional.Constants;
using VectorDraw.Professional.vdCollections;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdObjects;
using VectorDraw.Professional.vdPrimaries;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

namespace DAP_1
{
    public partial class Form1 : Form
    {
        List<Instrument> instruments = new List<Instrument>();
        List<vdPolyline>  obstacles = new List<vdPolyline>();
        List<vdCircle> instrument = new List<vdCircle>();
       
        gPoints gridPoints = new gPoints();
        vdPolyline  boundary ;
        vdPolyline offsetBoundary;
        Destination destination  ;
        double sx = 0;
        double sy = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string fname;
            string DocPath;
            
                object ret = vdsc.BaseControl.ActiveDocument.GetOpenFileNameDlg(0, "", 0);
                if (ret == null) return;

                DocPath = ret as string;
                fname = (string)ret;
            
            bool success = vdsc.BaseControl.ActiveDocument.Open(fname);
            if (!success) return;

            foreach (vdFigure f in vdsc.BaseControl.ActiveDocument.Model.Entities) {

                if (f.Layer.Name == "Instrument") {
                    vdCircle circle = (vdCircle)f;
                    this.instrument.Add(circle);
                  
                    Instrument instrument = new Instrument(circle);
                    this.instruments.Add(instrument);
                }
                if (f.Layer.Name == "Obstacle")
                {
                    vdPolyline poly = (vdPolyline)f;
                    this.obstacles.Add(poly);

                }
                if (f.Layer.Name == "Boundary")
                {
                    vdPolyline poly = (vdPolyline)f;
                    this.boundary = poly;
                    vdCurves c = poly.getOffsetCurve(1000); ;
                    this.offsetBoundary = new vdPolyline(c.Document, c[0].GetGripPoints());
                }
                if (f.Layer.Name == "Destination")
                {
                    
                    vdPolyline poly = (vdPolyline)f;
                    this.destination = new Destination(poly);
                }
            }
            
            vdsc.BaseControl.ActiveDocument.Redraw(true);

            makeGrid();
        }



        public void makeGrid()
        {

            Box box = this.offsetBoundary.BoundingBox;

            sx = box.Left;
            sy = box.Bottom;

            int wGap = 2000;
            int hGap = 2000;
            int wc = (int)box.Width / wGap + 1;
            int hc = (int)box.Height / hGap + 1;
            int pCount = wc * hc;



            double[,] adjMatrix = new double[pCount, pCount];
            for (int i = 0; i < pCount; i++)
            {
                for (int j = 0; j < pCount; j++)
                {
                    adjMatrix[i, j] = double.MaxValue;
                }
            }


            for (int i = 0; i < pCount - 1; i++)
            {
                if ((i / wc) == ((i + 1) / wc))
                {
                    adjMatrix[i, i + 1] = wGap;
                    adjMatrix[i + 1, i] = wGap;
                }
                if (i + wc < pCount)
                {
                    adjMatrix[i, i + wc] = hGap;
                    adjMatrix[i + wc, i] = hGap;
                }
            }

            for (int i = 0; i < hc; i++)
            {
                for (int j = 0; j < wc; j++)
                {
                    int index = i * wc + j;
                    gPoint p = new gPoint(sx + j * wGap, sy + i * hGap);
                    gridPoints.Add(p);
                    bool isIn = contains(this.boundary.VertexList, p);
                    bool isIn2 = false;
                    foreach (vdPolyline obstacle in this.obstacles)
                    {
                        isIn2 = contains(obstacle.VertexList, p);
                        if (isIn2) break;
                    }

                    if (!isIn || isIn2)
                    {
                        if ((index - wc) > 0)
                        {
                            adjMatrix[index - wc, index] = double.MaxValue;
                            adjMatrix[index, index - wc] = double.MaxValue;
                        }
                        if ((index - 1) > 0 && ((index - 1) / wc == index / wc))
                        {
                            adjMatrix[index - 1, index] = double.MaxValue;
                            adjMatrix[index, index - 1] = double.MaxValue;
                        }
                        if ((index + wc) < pCount)
                        {
                            adjMatrix[index + wc, index] = double.MaxValue;
                            adjMatrix[index, index + wc] = double.MaxValue;
                        }
                        if ((index + 1) < pCount && ((index + 1) / wc == index / wc))
                        {
                            adjMatrix[index + 1, index] = double.MaxValue;
                            adjMatrix[index, index + 1] = double.MaxValue;
                        }
                    }


                }
            }



            /*
            for (int i = 0; i < pCount; i++)
            {
                gPoint gp1 = gridPoints[i];
                vdCircle c = new vdCircle(this.vdsc.BaseControl.ActiveDocument, gp1, 100);
                vdsc.BaseControl.ActiveDocument.ActiveLayOut.Entities.AddItem(c);
                c.SetUnRegisterDocument(vdsc.BaseControl.ActiveDocument);
                c.setDocumentDefaults();
            }
            */

            for (int i = 0; i < pCount - 1; i++)
            {
                gPoint gp1 = gridPoints[i];
                for (int j = i + 1; j < pCount; j++)
                {
                    gPoint gp2 = gridPoints[j];
                    /*
                    if (adjMatrix[i, j] < 100000000)
                    {
                        vdLine line = new vdLine(this.vdsc.BaseControl.ActiveDocument, gp1, gp2);
                        vdsc.BaseControl.ActiveDocument.ActiveLayOut.Entities.AddItem(line);
                        line.SetUnRegisterDocument(vdsc.BaseControl.ActiveDocument);
                        line.setDocumentDefaults();
                    }
                    */
                }
            }


            double minDistance = double.MaxValue;
            int count = 0;
            foreach (gPoint gridPoint in this.gridPoints)
            {
                double dis = this.destination.center.Distance2D(gridPoint);
                if (minDistance > dis)
                {
                    minDistance = dis;
                    destination.gridPoint = gridPoint;
                    destination.gridIndex = count;
                }
                count++;
            }

            foreach (Instrument ins in this.instruments)
            {
                minDistance = double.MaxValue;
                count = 0;
                foreach (gPoint gridPoint in this.gridPoints)
                {
                    double dis = ins.centerPoint.Distance2D(gridPoint);
                    if (minDistance > dis)
                    {
                        minDistance = dis;
                        ins.distance = dis;
                        ins.gridPoint = gridPoint;
                        ins.gridIndex = count;
                    }
                    count++;
                }
            }


            foreach (Instrument ins in this.instruments)
            {
                //   ins.distanceFromDestination = this.destination.center.Distance2D(ins.centerPoint);
                //   Console.WriteLine("gridIndex   "+ins.gridIndex);
                double[] result = Dijkstra.analysis(ins.gridIndex, destination.gridIndex, adjMatrix);
                ins.distanceFromDestination = result[result.Length - 1];

            }

            this.instruments = this.instruments.OrderBy(x => x.distanceFromDestination).ToList();
           // this.instruments.Reverse();

            foreach (Instrument ins in this.instruments)
            {
                double[] result = Dijkstra.analysis(ins.gridIndex, destination.gridIndex, adjMatrix);
                gPoints ps = new gPoints();
                for (int i = 0; i < result.Length - 1; i++)
                {
                    gPoint gp = gridPoints[(int)result[i]];
                    ps.Add(gp);
                }

                for (int i = 0; i < result.Length - 2; i++)
                {
                    adjMatrix[(int)result[i], (int)result[i+1]] = adjMatrix[(int)result[i], (int)result[i + 1]]*0.1;
                    adjMatrix[(int)result[i+1], (int)result[i]] = adjMatrix[(int)result[i + 1], (int)result[i]]*0.1;
                 }
              
               vdPolyline line = new vdPolyline(this.vdsc.BaseControl.ActiveDocument, ps);
                line.PenColor = new vdColor(Color.Yellow);
                vdsc.BaseControl.ActiveDocument.ActiveLayOut.Entities.AddItem(line);
                line.SetUnRegisterDocument(vdsc.BaseControl.ActiveDocument);
                line.setDocumentDefaults();

            }
            this.vdsc.BaseControl.Redraw();
        }
 
        
        


        public static bool contains(Vertexes _pts, gPoint pt)
        {
            bool isIn = false;

            int NumberOfPoints = _pts.Count;
            if (true)
            {
                int i, j = 0;
                for (i = 0, j = NumberOfPoints - 1; i < NumberOfPoints; j = i++)
                {
                    if (
                    (
                    ((_pts[i].y <= pt.y) && (pt.y <= _pts[j].y)) || ((_pts[j].y <= pt.y) && (pt.y <= _pts[i].y))
                    ) &&
                    (pt.x <= (_pts[j].x - _pts[i].x) * (pt.y - _pts[i].y) / (_pts[j].y - _pts[i].y) + _pts[i].x)
                    )
                    {
                        isIn = !isIn;
                    }
                }
            }
            return isIn;
        }
        public static bool contains2(Vertexes _pts, gPoint pt)
        {
            bool isIn = false;

            int NumberOfPoints = _pts.Count;
            if (true)
            {
                int i, j = 0;
                for (i = 0, j = NumberOfPoints - 1; i < NumberOfPoints; j = i++)
                {
                    if (
                    (
                    ((_pts[i].y < pt.y) && (pt.y < _pts[j].y)) || ((_pts[j].y < pt.y) && (pt.y < _pts[i].y))
                    ) &&
                    (pt.x < (_pts[j].x - _pts[i].x) * (pt.y - _pts[i].y) / (_pts[j].y - _pts[i].y) + _pts[i].x)
                    )
                    {
                        isIn = !isIn;
                    }
                }
            }
            return isIn;
        }

        public static void setColorPath(vdPolyline polyline, Color color, VdConstLineWeight vdConstLineWeight)
        {
            polyline.PenColor = new VectorDraw.Professional.vdObjects.vdColor(color);
            polyline.LineWeight = vdConstLineWeight;
        }

    }
}
