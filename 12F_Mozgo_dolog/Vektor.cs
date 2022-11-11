using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _12F_Mozgo_dolog
{
	struct Vektor
	{
		public double X, Y;

		public Vektor(double X, double Y)
		{
			this.X = X;
			this.Y = Y;
		}

		public static Vektor operator +(Vektor a, Vektor b) => new Vektor(a.X + b.X, a.Y + b.Y);

		public Point ToPoint() => new Point((int)Math.Round(X), (int)Math.Round(Y));

	}
}
