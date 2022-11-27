using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _12F_Mozgo_dolog
{
	public struct Vector
	{
		public double X, Y;

		public Vector(double X, double Y)
		{
			this.X = X;
			this.Y = Y;
		}
		public Vector(Point point)
		{
			this.X = point.X;
			this.Y = point.Y;
		}

		/*public static Vector ToVector(Point point)
        {
			return new Vector(point.X, point.Y);
		}*/		

		public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
		public static Vector operator -(Vector a) => new Vector(-a.X, -a.Y);
        public static Vector operator *(Vector a, double d) => new Vector(a.X * d, a.Y * d);
        public static Vector operator /(Vector a, double d) => new Vector(a.X / d, a.Y / d);
        
        public double Distance() => Math.Sqrt(X * X + Y * Y);

        public Point ToPoint() => new Point((int)Math.Round(X), (int)Math.Round(Y));


		public override string ToString()
		{
			return "{"+ (int)X + ", " + (int)Y + "}";
		}
	}
}
