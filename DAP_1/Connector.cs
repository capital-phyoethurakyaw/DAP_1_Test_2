using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdObjects;

namespace DAP_1
{
    public class Connector : IComparable 
    {
     public   Node sNode;
        public Node eNode;

        public vdLine line;

       public List<Instrument> instruments= new List<Instrument>();    
        

        public Connector(Node sNode, Node eNode, vdDocument doc) { 
             this.sNode= sNode;
            this.eNode= eNode;
            line= new vdLine(doc, sNode.gp, eNode.gp);



        }

        public int CompareTo(object other)
        {
            return instruments.Count.CompareTo(other);
        }

        public double getLength() {



            return sNode.gp.Distance2D(eNode.gp); ;
        }

    }
}
