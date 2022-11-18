using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace _12F_Mozgo_dolog
{
	public class CelestialBody:BasicCB
	{
		/*Vector location;
		Vector velocity;
		int size;
		int mass;*/
		//Color color;
		SolidBrush brush;		
		Bitmap planetTexture;
		static List<CelestialBody> list = new List<CelestialBody>(); // ezt muszáj itt inicializálni most.
		int countOfRFrames = 0;
		int frameIndex = 0;
		bool hasShadow;
		Bitmap shadow;
		Bitmap mask;
		List<Bitmap> rotationFrames = new List<Bitmap>();

		public static int wayPointLookAhead;
		//List<BasicCB> wayPoints = new List<BasicCB>();		
		Queue<Point> history = new Queue<Point>();
		LinkedList<int> llist = new LinkedList<int>();

		public static BasicCB sun;


		public static Graphics g; // a Form1-ben, kívülről inicializálom, így nem kell using (Graphics g...)-t használni frame-enként
		public static Label label3;

		public CelestialBody(Vector location, Vector velocity, int size, double mass, Color color)
		{
			this.location = location;
			this.velocity = velocity;
			this.size = size;
			this.mass = mass;
            this.movements = new List<Vector>();

			/*for (int i = 0; i < wayPointLookAhead; i++)
				wayPoints.Add(new BasicCB(this));*/

            //this.color = color;
            this.brush = new SolidBrush(color);			
			CelestialBody.list.Add(this);
		}

		public CelestialBody(Vector location, Vector velocity, int size, double mass, Bitmap palentTexture, bool hasShadow)
		{
			this.location = location;
			this.velocity = velocity;
			this.size = size;
			this.mass = mass;
			this.movements = new List<Vector>();

			//this.color = color;
			this.countOfRFrames = 100;
			Bitmap frame = new Bitmap(size, size);
			this.planetTexture = palentTexture;
			this.hasShadow = hasShadow;
			if (hasShadow)
				shadow = new Bitmap(Properties.Resources.shadow);
			mask = new Bitmap(Properties.Resources.mask, size, size);


			for (int i = 0; i < countOfRFrames; i++)
			{
				using (Graphics g2 = Graphics.FromImage(frame))
				{
					g2.Clear(Color.Transparent);
					g2.DrawImage(planetTexture, (int)Math.Round(((double)planetTexture.Width / countOfRFrames) * i), 0, planetTexture.Width, size);
					g2.DrawImage(planetTexture, (int)Math.Round(((double)planetTexture.Width / countOfRFrames) * i) - planetTexture.Width, 0, planetTexture.Width, size);

					Color color;
					Brush brush;
					for (int y = 0; y < size; y++)
					{
						for (int x = 0; x < size; x++)
						{
							color = mask.GetPixel(x, y);
							brush = new SolidBrush(Color.FromArgb(255 - color.A, 0, 0, 0));
							g2.FillRectangle(brush, x, y, 1, 1);
							brush.Dispose();
						}
					}					
				}

				frame.MakeTransparent(Color.Black);
				this.rotationFrames.Add(new Bitmap(frame));
			}

			frame.Dispose();

			CelestialBody.list.Add(this);
		}

        public static void CalcAllGVectors()
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < list.Count; j++)
					if(list[i] != list[j])
						list[i].movements.Add(list[i].Gravity(list[j]));
        }
		public static void SetAllVelocity()
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list[i].movements.Count; j++)
					list[i].velocity += list[i].movements[j];
				list[i].movements.Clear();
			}
		}

        internal void Move()
		{
			this.location += this.velocity;
			future.Enqueue(this.location.ToPoint());
		}

		public static void MoveAll()
		{
			for (int i = 0; i < list.Count; i++)
				list[i].Move();
		}

        Vector Gravity(CelestialBody that)
        {
            double d = this.DirectionFrom(that); // ez is vektoraritmetika!
            double dsqr = d * d;
            double vectord = that.mass / dsqr;

            Vector vector = that.location - this.location; // egy komplett vektoraritmetikát ki kell majd dolgoznunk!
            Vector vectorUnit = vector / d;// John Carmack! 
            return vectorUnit * vectord;
        }

        private double DirectionFrom(CelestialBody that) => (this.location - that.location).Distance();

		private int AngleFromSun(Point p)
        {
			double result = (double)Math.Atan2(p.Y - sun.future.Peek().Y, p.X - sun.future.Peek().X) * (double)(180 / Math.PI)+90;
			Settext(label3, result.ToString());
			return (int)result;
        }

		/*public static void RecordHistroy()
        {
			for (int i = 0; i < list.Count; i++)
                list[i].history.Enqueue(list[i].location.ToPoint());
		}*/

		public static Bitmap RotateImage(Bitmap b, int angle)
		{
			Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
			using (Graphics g = Graphics.FromImage(returnBitmap))
			{
				g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
				g.RotateTransform(angle);
				g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
				g.DrawImage(b, new Point(0, 0));
			}
			return returnBitmap;
		}

		public void Draw(Graphics g)
		{
            Queue<Point> tempFuture = new Queue<Point>(future);            

            Point CBPoint = future.Dequeue();
			Point cpoint = tempFuture.Dequeue();

			for (int i = 0; i < wayPointLookAhead - 1; i++)
			{
				cpoint = tempFuture.Dequeue();
				if (i % 5 == 0)
				{
					SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 - 255 * i / wayPointLookAhead, 255, 255, 255));
					g.FillEllipse(wayPointBrush, cpoint.X - 1, cpoint.Y - 1, 2, 2);
					wayPointBrush.Dispose();
				}	
			}
            tempFuture.Clear();


			if (history.Count > 0)
			{
				Queue<Point> tempHistroy = new Queue<Point>(history);
				for (int i = 0; i < history.Count; i++)
				{
					cpoint = tempHistroy.Dequeue();
					if (i % 5 == 0)
					{
						SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 * i / history.Count, 255, 255, 255));
						g.FillEllipse(wayPointBrush, cpoint.X - 1, cpoint.Y - 1, 2, 2);
						wayPointBrush.Dispose();
					}
				}
				tempHistroy.Clear();
				
			}
			history.Enqueue(CBPoint);
			if (history.Count > wayPointLookAhead / 2)
				history.Dequeue();


			if (countOfRFrames == 0)
				g.FillEllipse(brush, CBPoint.X - size / 2, CBPoint.Y - size / 2, size, size);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], CBPoint.X - size / 2, CBPoint.Y - size / 2);
				frameIndex++;
				if (frameIndex == countOfRFrames)
					frameIndex = 0;
			}

			if (hasShadow)
			{
				/*using (Graphics g4 = Graphics.FromImage(shadow)) {
					g4.TranslateTransform((float)shadow.Width / 2, (float)shadow.Height / 2);
					g4.RotateTransform(AngleFromSun());
					g4.TranslateTransform(-(float)shadow.Width / 2, -(float)shadow.Height / 2);
				}*/
				g.DrawImage(RotateImage(shadow, AngleFromSun(CBPoint)), CBPoint.X - size / 2 - 2, CBPoint.Y - size / 2 - 2, size + 4, size + 4);
			}
		}

		public static void DrawAll(PictureBox pictureBox1)
		{
			g.Clear(Color.Black); //Frame törlése

			for (int i = 0; i < list.Count; i++)
				list[i].Draw(g);

			//pictureBox1.Refresh(); //nem működik, mert cross-threading lenne
			Refresh(pictureBox1);
		}

		private static void Refresh(Control pictureBox1) //a fő szálon futó pictureBox1 frissítése
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
        private static void Settext(Label label, string text) //a fő szálon futó pictureBox1 frissítése
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(
                delegate ()
                {
					label.Text = text;
                }));
            }
            else
                label.Text = text;
        }


        public static bool running = false;
		internal static void StartSimulation(PictureBox pictureBox1, Label label2, CancellationTokenSource _canceller)
		{
			int time = 0;

			while (running)
			{
				Thread.Sleep(20);

				CalcAllGVectors();
				SetAllVelocity();
				MoveAll();				
				DrawAll(pictureBox1);

				Settext(label2, time.ToString());
				time++;

                if (_canceller.Token.IsCancellationRequested)
                    break;
            }
		}
	}
}
