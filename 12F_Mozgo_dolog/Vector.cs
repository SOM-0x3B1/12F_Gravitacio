using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _12F_Mozgo_dolog
{
	struct Vector
	{
		public double X, Y;

		public Vector(double X, double Y)
		{
			this.X = X;
			this.Y = Y;
		}

		public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);

		public Point ToPoint() => new Point((int)Math.Round(X), (int)Math.Round(Y));

	}
}
