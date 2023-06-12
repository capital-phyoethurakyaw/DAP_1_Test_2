using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VectorDraw.Geometry;
using VectorDraw.Professional.Constants;
using VectorDraw.Professional.vdCollections;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdObjects;

namespace DAP_1.Analysis
{
    public class AnaylisRoute
    {

        public static void removeSubRoute(vdDocument doc, Route route, List<Instrument> instruments)
        {

            List<Connector> endConnectors = new List<Connector>();
            List<Instrument> noneInstruments = new List<Instrument>();
            foreach (Instrument instrument in instruments)
            {
                Connector minConnector = null;
                double minDist = double.MaxValue;
                foreach (Connector connector in route.connectors)
                {
                    if ((connector.sNode.connectors.Count == 1 || connector.eNode.connectors.Count == 1)) continue;

                    gPoint gp = connector.line.getClosestPointTo(instrument.centerPoint);
                    double dist = gp.Distance2D(instrument.centerPoint);
                    if (dist > 3000) continue;
                    if (minDist > dist) {
                        minDist = dist;
                        minConnector = connector;
                    }
                }
                if (minConnector != null) minConnector.instruments.Add(instrument);
                 else noneInstruments.Add(instrument);
            }


            // 앞에 조건에 연결된 connetor가 없는 instrumnet를 대상으로 최단 거리의 connecot와 연결
            // connector가 많은 쪽으로 밀기 ************************

            route.connectors =    route.connectors.OrderBy(x => x.instruments.Count).ToList<Connector>();
            route.connectors.Reverse();
            foreach (Instrument instrument in noneInstruments)
            {
                Connector minConnector = null;
                foreach (Connector connector in route.connectors)
                {
                    gPoint gp = connector.line.getClosestPointTo(instrument.centerPoint);
                    double dist = gp.Distance2D(instrument.centerPoint);
                    if ( dist < 3000)
                    {
                        minConnector = connector;
                        break;
                    }
                }
                if (minConnector != null) minConnector.instruments.Add(instrument);
            }

            /*
                foreach (Instrument instrument in noneInstruments)
                {
                    Connector minConnector = null;
                    double minDist = double.MaxValue;
                    foreach (Connector connector in route.connectors)
                    {
                        gPoint gp = connector.line.getClosestPointTo(instrument.centerPoint);
                        double dist = gp.Distance2D(instrument.centerPoint);
                        if (minDist > dist)
                        {
                            minDist = dist;
                            minConnector = connector;
                        }
                    }
                    if (minConnector != null) minConnector.instruments.Add(instrument);

                }
            */

                //끝단이고 연결된 instrumnet가 없는 경우
                for (int i = 0; i < route.connectors.Count; i++)
            {
                Connector c = route.connectors[i];

                if (c.sNode.connectors.Count == 1 || c.eNode.connectors.Count == 1)
                {
                    if (c.instruments.Count == 0)
                    {
                        route.connectors.Remove(c);
                    
                        route.delectedConnector.Add(c);
                        i--;
                    }
                }
            }


            foreach (Connector connector1 in route.delectedConnector) {
                connector1.sNode.connectors.Remove(connector1);
                connector1.eNode.connectors.Remove(connector1);
            }
            for (int i = 0; i < route.nodes.Count; i++)
            {
                if (route.nodes[i].connectors.Count == 0) {
                    route.nodes.Remove(route.nodes[i]);
                    i--;
                }
            }

            foreach (Connector connector1 in route.connectors) connector1.instruments.Clear();

            //남아 있는 connector에서 최단 거리의 connector와 연결
            
            foreach (Instrument instrument in instruments)
            {
                Connector minConnector = null;
                double minDist = double.MaxValue;
                foreach (Connector connector in route.connectors)
                {
                    gPoint gp = connector.line.getClosestPointTo(instrument.centerPoint);
                    double dist = gp.Distance2D(instrument.centerPoint);
                    if (minDist > dist)
                    {
                        minDist = dist;
                        minConnector = connector;
                    }
                }
                if (minConnector != null) minConnector.instruments.Add(instrument);
            }
        }

        public static void joinLines(vdDocument doc, Route route, List<Instrument> instruments) {
            List<vdLine> lines = new List<vdLine>();
            for (int i = 0; i < route.connectors.Count; i++)
            {
                Connector connector = (Connector)route.connectors[i];
                 lines.Add(connector.line);
            }

            route.connectors.Clear();
            route.nodes.Clear();



            gPoints gsa11 = new gPoints();
            gPoints gsa22 = new gPoints();
            foreach (vdLine l in lines)
            {
                gsa11.Add(l.StartPoint);
                gsa11.Add(l.EndPoint);
                gsa22.Add(l.StartPoint);
                gsa22.Add(l.EndPoint);
            }
            gsa11.RemoveEqualPoints();

            foreach (gPoint p1 in gsa11)
            {
                int co = 0;
                foreach (gPoint p2 in gsa22)
                {
                    if (p1.x == p2.x && p1.y == p2.y) co++;

                }

                if (co == 2)
                {
                    vdLine sl = null;
                    vdLine el = null;
                    foreach (vdLine l in lines)
                    {
                        if (l.StartPoint.x == p1.x && l.StartPoint.y == p1.y) el = l;
                        if (l.EndPoint.x == p1.x && l.EndPoint.y == p1.y) sl = l;
                    }
                    if (sl != null && el != null)
                    {
                        if (sl.StartPoint.x - el.EndPoint.x == 0 || sl.StartPoint.y - el.EndPoint.y == 0)
                        {
                            vdLine newLine = new vdLine(doc, new gPoint(sl.StartPoint.x, sl.StartPoint.y), new gPoint(el.EndPoint.x, el.EndPoint.y));
                            int index1 = lines.FindIndex(0, l => l.Equals(sl));
                            int index2 = lines.FindIndex(0, l => l.Equals(el));
                            lines.Insert(index1, newLine);
                            lines.Remove(sl);
                            lines.Remove(el);
                        }
                    }
                }
            }

            gPoints gsa1 = new gPoints();
            foreach (vdLine l in lines)
            {
                gsa1.Add(l.StartPoint);
                gsa1.Add(l.EndPoint);
            }
            gsa1.RemoveEqualPoints();

            foreach (gPoint p1 in gsa1)
            {
                
                Node n = new Node(p1);
                route.nodes.Add(n);
            }

                //   MessageBox.Show(" ----     " + lines.Count);
                route.connectors.Clear();

                foreach (vdLine l in lines)
            {
                Node sNode = getNode(route.nodes, l.StartPoint);
                Node eNode = getNode(route.nodes, l.EndPoint);
                 sNode.nodes.Add(eNode);
                eNode.nodes.Add(sNode);
                  
                Connector connector = new Connector(sNode, eNode, doc);
                sNode.connectors.Add(connector);
                eNode.connectors.Add(connector);
                route.connectors.Add(connector);
            }

            foreach (Instrument instrument in instruments)
            {
                Connector minConnector = null;
                double minDist = double.MaxValue;
                foreach (Connector connector in route.connectors)
                {
                    gPoint gp = connector.line.getClosestPointTo(instrument.centerPoint);
                    double dist = gp.Distance2D(instrument.centerPoint);
                    if (minDist > dist)
                    {
                        minDist = dist;
                        minConnector = connector;
                    }
                }
                if (minConnector != null) minConnector.instruments.Add(instrument);
            }

        }


        public static void analysisRoute(vdDocument doc, Route route, List<Instrument> instruments)
        {
            List<vdLine> lines = route.lines;
            for (int i = 0; i < route.subRoutes.Count; i++)
            {
                SubRoute sr1 = (SubRoute)route.subRoutes[i];
                vdPolyline poly = new vdPolyline(doc);
                poly.VertexList.AddRange(sr1.polyGps);
                vdEntities entities = poly.Explode();
                foreach (vdLine en in entities)
                {
                    lines.Add(en);
                }
            }

            gPoints gsa1 =  analysisRoute(doc, lines, instruments);

            foreach (gPoint p1 in gsa1)
            {
                Node n = new Node(p1);
                route.nodes.Add(n);
            }
            route.connectors.Clear();
            foreach (vdLine l in lines)
            {
                Node sNode = getNode(route.nodes, l.StartPoint);
                Node eNode = getNode(route.nodes, l.EndPoint);
                sNode.nodes.Add(eNode);
                eNode.nodes.Add(sNode);
                Connector connector = new Connector(sNode, eNode, doc);
                sNode.connectors.Add(connector);
                eNode.connectors.Add(connector);
                route.connectors.Add(connector);
            }
        }

      


        public static gPoints analysisRoute(vdDocument doc, List<vdLine> lines, List<Instrument> instruments ) {

            List<vdLine> addedLines = new List<vdLine>();
            List<vdLine> deletedLines = new List<vdLine>();

            for (int i = 0; i < lines.Count - 1; i++)
            {

                vdLine l1 = (vdLine)lines[i];
                int t1 = -1;
                if (l1.StartPoint.x - l1.EndPoint.x == 0) t1 = 0;
                if (l1.StartPoint.y - l1.EndPoint.y == 0) t1 = 1;

                for (int j = i + 1; j < lines.Count; j++)
                {
                    vdLine l2 = (vdLine)lines[j];
                    gPoints gPoints = new gPoints();
                    int t2 = -1;
                    if (l2.StartPoint.x - l2.EndPoint.x == 0) t2 = 0;
                    if (l2.StartPoint.y - l2.EndPoint.y == 0) t2 = 1;
             
                    if (t1 == t2)
                    {

                        continue;
                    }
                    bool isIntersected = l1.IntersectWith(l2, VdConstInters.VdIntOnBothOperands, gPoints);
                    if (isIntersected)
                    {
                        gPoint p = gPoints[0];
                        if (!(p.x == l1.StartPoint.x && p.y == l1.StartPoint.y) && !(p.x == l1.EndPoint.x && p.y == l1.EndPoint.y))
                        {
                            vdLine sl = new vdLine(doc, l1.StartPoint, p);
                            vdLine el = new vdLine(doc, p, l1.EndPoint);

                            lines.Add(sl);
                            lines.Add(el);
                            lines.Remove(l1);
                            i--;
                            break;
                        }
                        else if (!(p.x == l2.StartPoint.x && p.y == l2.StartPoint.y) && !(p.x == l2.EndPoint.x && p.y == l2.EndPoint.y))
                        {
                            vdLine sl = new vdLine(doc, l2.StartPoint, p);
                            vdLine el = new vdLine(doc, p, l2.EndPoint);
                            lines.Add(sl);
                            lines.Add(el);
                            lines.Remove(l2);
                            j--;
                        }
                    }
                }
            }

            gPoints gsa11 = new gPoints();
            gPoints gsa22 = new gPoints();
            foreach (vdLine l in lines)
            {
                gsa11.Add(l.StartPoint);
                gsa11.Add(l.EndPoint);
                gsa22.Add(l.StartPoint);
                gsa22.Add(l.EndPoint);
            }
            gsa11.RemoveEqualPoints();
       
            foreach (gPoint p1 in gsa11)
            {
                int co = 0;
                foreach (gPoint p2 in gsa22)
                {
                    if (p1.x == p2.x && p1.y == p2.y) co++;

                }
          //      Console.WriteLine(p1.x + "  " + p1.y + "  " + co);

                if (co == 2)
                {
                    vdLine sl = null;
                    vdLine el = null;
                    foreach (vdLine l in lines)
                    {

                        if (l.StartPoint.x == p1.x && l.StartPoint.y == p1.y)
                        {
                            el = l;
                        }
                        if (l.EndPoint.x == p1.x && l.EndPoint.y == p1.y)
                        {
                            sl = l;
                        }
                    }
                    if (sl != null && el != null)
                    {
                        if (sl.StartPoint.x - el.EndPoint.x == 0 || sl.StartPoint.y - el.EndPoint.y == 0)
                        {


                            vdLine newLine = new vdLine(doc, new gPoint(sl.StartPoint.x, sl.StartPoint.y), new gPoint(el.EndPoint.x, el.EndPoint.y));

                            int index1 = lines.FindIndex(0, l => l.Equals(sl));
                            int index2 = lines.FindIndex(0, l => l.Equals(el));
                                                 lines.Insert(index1, newLine);
                            lines.Remove(sl);
                            lines.Remove(el);
 

                        }
                    }
                }
            }

            ///////////////////////
                gPoints gsa1 = new gPoints();
                foreach (vdLine l in lines)
                {
                    gsa1.Add(l.StartPoint);
                    gsa1.Add(l.EndPoint);
                }
                gsa1.RemoveEqualPoints();
               
        
           
            return gsa1;
          
        }


        public static Node getNode(List<Node> nodes, gPoint gp)
        {
            foreach (Node n in nodes)
            {
                if (n.gp.x == gp.x && n.gp.y == gp.y) return n;
            }
            return null;
        }


        public static int[,]  getNewMatrix(int[,] newMatrix, Route selectedRoute, MatrixPoint[,] mps) {
            foreach (Connector connector in selectedRoute.connectors)
            {
                if (selectedRoute.disconnectedConnectors.Contains(connector)) continue;

                Console.WriteLine(connector.sNode.gp.x + "  11  " + connector.sNode.gp.y);
                Console.WriteLine(connector.eNode.gp.x + "  11  " + connector.eNode.gp.y);
                MatrixPoint smp = getMatrixPoint(connector.sNode.gp.x, connector.sNode.gp.y, mps);
                MatrixPoint emp = getMatrixPoint(connector.eNode.gp.x, connector.eNode.gp.y, mps);
                Console.WriteLine(smp.x + "  22  " + smp.y);
                Console.WriteLine(emp.x + "  22  " + emp.y);
  
                int minX = 0; ;
                int minY = 0;
                int maxX = 0;
                int maxY = 0;

                if (smp.y == emp.y)
                {
                    if (smp.x > emp.x)
                    {
                        minX = emp.x;
                        maxX = smp.x;
                    }
                    else
                    {
                        minX = smp.x;
                        maxX = emp.x;
                    }
                    for (int i = minX; i <= maxX; i++) newMatrix[i, smp.y] = 2;
                }
                if (smp.x == emp.x)
                {
                    if (smp.y > emp.y)
                    {
                        minY = emp.y;
                        maxY = smp.y;
                    }
                    else
                    {
                        minY = smp.y;
                        maxY = emp.y;
                    }
                    for (int i = minY; i <= maxY; i++) newMatrix[smp.x, i] = 2;
                }
            }
            return newMatrix;
        }


        public static MatrixPoint getMatrixPoint(double x, double y, MatrixPoint[,] mps)
        {
            for (int i = 0; i < mps.GetLength(0); i++)
            {
                for (int j = 0; j < mps.GetLength(1); j++)
                {
                    if (mps[i, j].gp.x == x && mps[i, j].gp.y == y)
                    {

                        return mps[i, j];
                    }

                }
            }
            return null;
        }
    }
}
