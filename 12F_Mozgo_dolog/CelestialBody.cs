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
		Bitmap planetTexture;
		static List<CelestialBody> list = new List<CelestialBody>(); // ezt muszáj itt inicializálni most.
		int countOfRFrames = 0;
		int frameIndex = 0;
		List<Bitmap> rotationFrames = new List<Bitmap>();

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

		public CelestialBody(Vector location, Vector velocity, int size, int mass, Bitmap palentTexture)
		{
			this.location = location;
			this.velocity = velocity;
			this.size = size;
			this.mass = mass;
			//this.color = color;
			this.countOfRFrames = 20;
			Bitmap frame = new Bitmap(size, size);
			this.planetTexture = palentTexture;

			Bitmap shadow;

			using (Graphics g2 = Graphics.FromImage(frame))
			{
				for (int i = 0; i < countOfRFrames; i++)
				{
					

					g2.Clear(Color.Transparent);
					g2.DrawImage(planetTexture, planetTexture.Width / countOfRFrames * i, 0, planetTexture.Width, size);
					g2.DrawImage(planetTexture, (planetTexture.Width / countOfRFrames * i) - planetTexture.Width, 0, planetTexture.Width, size);

					Color color;
					Brush brush;
					Bitmap mask = new Bitmap(Properties.Resources.mask, size, size);
					for (int y = 0; y < size; y++)
					{
						for (int x = 0; x < size; x++)
						{
							color = mask.GetPixel(x, y);
							brush = new SolidBrush(Color.FromArgb(255-color.A, 0, 0, 0));
							g2.FillRectangle(brush, x, y, 1, 1);
						}
					}

					shadow = Properties.Resources.shadow;
					shadow.RotateFlip(RotateFlipType.Rotate90FlipNone);
					g2.DrawImage(shadow, 0, 0, size, size);

					this.rotationFrames.Add(new Bitmap(frame));					
				}
			}
			frame.Dispose();

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
			if (countOfRFrames == 0)
				g.FillEllipse(brush, h.X - size / 2, h.Y - size / 2, size, size);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], h.X - size / 2, h.Y - size / 2);
				frameIndex++;
				if(frameIndex == countOfRFrames)
					frameIndex = 0;
			}
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
