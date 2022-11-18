using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace _12F_Mozgo_dolog
{
    public class BasicCB
    {
        public Vector location;
        public Vector velocity;
        public int size;
        public double mass;
        public List<Vector> movements;

        public Queue<Point> future = new Queue<Point>();

        public BasicCB() { }

        public BasicCB(CelestialBody cBody)
        {
            this.location = cBody.location;
            this.velocity = cBody.velocity;
            this.size = cBody.size;
            this.mass = cBody.mass;
            this.movements = cBody.movements;
        }
    }
}
