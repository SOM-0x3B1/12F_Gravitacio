﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _12F_Mozgo_dolog
{
    public class BasicCB
    {
        public Vector location;
        public Vector velocity;
        public int size;
        public int mass;
        public List<Vector> movements;

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
