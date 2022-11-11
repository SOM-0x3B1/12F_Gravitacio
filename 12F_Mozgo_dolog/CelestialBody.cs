using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace _12F_Mozgo_dolog
{
	class CelestialBody
	{
		Vector location;
		Vector velocity;
		int size;
		int mass;
		//Color color;
		SolidBrush brush;
		static List<CelestialBody> list = new List<CelestialBody>(); // ezt muszáj itt inicializálni most.
		public static Graphics g; // a Form1-ben, kívülről inicializálom, így nem kell using (Graphics g...)-t használni frame-enként

		public CelestialBody(Vector location, Vector velocity, int size, int mass, Color color)
		{
			this.location = location;
			this.velocity = velocity;
			this.size = size;
			this.mass = mass;
			//this.color = color;
			this.brush = new SolidBrush(color);			
			CelestialBody.list.Add(this);
		}

		internal void Move()
		{
			this.location += this.velocity;
		}

		public static void MoveAll()
		{
			for (int i = 0; i < list.Count; i++)
				list[i].Move();
		}


		public void Draw(Graphics g)
		{
			Point h = location.ToPoint();
			g.FillEllipse(brush, h.X - size / 2, h.Y - size / 2, size, size);
		}

		public static void DrawAll(PictureBox pictureBox1)
		{
			g.Clear(Color.Black); // Frame törlése

			for (int i = 0; i < list.Count; i++)
				list[i].Draw(g);

			//pictureBox1.Refresh(); //nem működik, mert cross-threading lenne
			Refresh(pictureBox1);
		}

		private static void Refresh(PictureBox pictureBox1) //a fő szálon futó pictureBox1 frissítése
		{
			if (pictureBox1.InvokeRequired)
			{
				pictureBox1.Invoke(new MethodInvoker(
				delegate ()
				{
					pictureBox1.Refresh();
				}));
			}
			else
				pictureBox1.Refresh();
		}


		public static bool running = false;
		internal static void StartSimulation(PictureBox pictureBox1, CancellationTokenSource _canceller)
		{
			while (running)
			{
				Thread.Sleep(50);
				MoveAll();
				DrawAll(pictureBox1);

				if (_canceller.Token.IsCancellationRequested)
					break;
			}
		}
	}
}
