using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace _12F_Mozgo_dolog
{
	class Mozgo
	{
		Vektor hely;
		Vektor sebesseg;
		int meret;
		Color szin;
		SolidBrush kemenyecset;
		static List<Mozgo> lista = new List<Mozgo>(); // ezt muszáj itt inicializálni most.

		public Mozgo(Vektor hely, Vektor sebesseg, int meret, Color szin)
		{
			this.hely = hely;
			this.sebesseg = sebesseg;
			this.meret = meret;
			this.szin = szin;
			this.kemenyecset = new SolidBrush(szin);
			Mozgo.lista.Add(this);
		}

		internal void Lépj()
		{
			this.hely += this.sebesseg;
		}

		public void Lerajzol(Graphics g)
		{
			Point h = hely.ToPoint();
			g.FillEllipse(kemenyecset, h.X - meret / 2, h.Y - meret / 2, meret, meret);
		}

		public static void Összes_lerajzolása(PictureBox pictureBox1)
		{
			Size vaszonmeret = pictureBox1.Size;
			Bitmap bmp = new Bitmap(vaszonmeret.Width, vaszonmeret.Height);
			pictureBox1.Image = bmp;
			using (Graphics g = Graphics.FromImage(bmp))
			{
				foreach (Mozgo mozgo in Mozgo.lista)
				{
					mozgo.Lerajzol(g);
				}
			}
			pictureBox1.Refresh();
		}
	}
}
