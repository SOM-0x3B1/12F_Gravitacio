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
		string id;
		SolidBrush brush;		
		Bitmap planetTexture;		
		int countOfRFrames = 0;
		int frameIndex = 0;
		bool hasShadow;
		Bitmap shadow;
		Bitmap glow;
		Bitmap mask;
		List<Bitmap> rotationFrames = new List<Bitmap>();
		SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush opaqueBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255));

        public bool placing;
		public bool vectoring;

		public static int wayPointLookAhead;
		Queue<Point> history = new Queue<Point>();

		public static BasicCB lightsrc;
		public static Bitmap space = Properties.Resources.space;
		public static Graphics g; // a Form1-ben, kívülről inicializálom, így nem kell using (Graphics g...)-t használni frame-enként


        public static List<CelestialBody> list = new List<CelestialBody>();


        public CelestialBody(string id, Vector location, Vector velocity, int height, double mass, Color color)
		{
			this.id = id;
			this.location = location;
			this.velocity = velocity;
			this.height = height;
			this.mass = mass;
            this.movements = new List<Vector>();

			/*for (int i = 0; i < wayPointLookAhead; i++)
				wayPoints.Add(new BasicCB(this));*/

            //this.color = color;
            this.brush = new SolidBrush(color);			
			CelestialBody.list.Add(this);
		}

		public CelestialBody(string id, Vector location, Vector velocity, int height, double mass, Bitmap planetTexture, bool hasShadow)
		{
			this.id = id;
			this.location = location;
			this.velocity = velocity;
			this.height = height;
			this.mass = mass;			
			this.movements = new List<Vector>();

			this.countOfRFrames = 100;
			Bitmap frame = new Bitmap(height, height);
			this.hasShadow = hasShadow;
			if (hasShadow)
				shadow = new Bitmap(Properties.Resources.shadow, height + 4, height + 4);
			else
				glow = new Bitmap(Properties.Resources.glow, (int)(height * 1.5), (int)(height * 1.5));

			mask = new Bitmap(Properties.Resources.mask, height, height);


			int width = (int)Math.Round((double)height / planetTexture.Height * planetTexture.Width);
			this.planetTexture = new Bitmap(planetTexture, width, height);

			for (int i = 0; i < countOfRFrames; i++)
			{
				using (Graphics g2 = Graphics.FromImage(frame))
				{
					g2.Clear(Color.Transparent);
					g2.DrawImage(this.planetTexture, (int)Math.Round(((double)width / countOfRFrames) * i), 0, this.planetTexture.Width, this.planetTexture.Height);
					g2.DrawImage(this.planetTexture, (int)Math.Round(((double)width / countOfRFrames) * i) - width, 0, this.planetTexture.Width, this.planetTexture.Height);

					Color color;
					Brush brush;
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < height; x++)
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

			mask.Dispose();
			frame.Dispose();
			planetTexture.Dispose();
			this.planetTexture.Dispose();

			CelestialBody.list.Add(this);
		}

		
		public void CalcGVectors()
        {
			for (int j = 0; j < list.Count; j++)
				if (this != list[j])
					this.movements.Add(this.Gravity(list[j]));
		}
		public void SetVelocity()
		{
			for (int j = 0; j < this.movements.Count; j++)
				this.velocity += this.movements[j];
			this.movements.Clear();
		}
		public void Move()
		{
			this.location += this.velocity;
			future.Enqueue(this.location.ToPoint());

			if(vectoring && future.Count > wayPointLookAhead)
				vectoring = false;
		}


		public static void CalcAllGVectors()
        {
			for (int i = 0; i < list.Count; i++)
				list[i].CalcGVectors();
        }
		public static void SetAllVelocity()
		{
			for (int i = 0; i < list.Count; i++)
				list[i].SetVelocity();
		}
		public static void MoveAll()
		{
			for (int i = 0; i < list.Count; i++)
				list[i].Move();
		}


        Vector Gravity(CelestialBody that)
        {
            double d = this.DirectionFrom(that);
            double dsqr = d * d;
            double vectord = that.mass / dsqr;

            Vector vector = that.location - this.location;
            Vector vectorUnit = vector / d; //John Carmack! 
            return vectorUnit * vectord;
        }

        private double DirectionFrom(CelestialBody that) => (this.location - that.location).Distance();

		private int AngleFromSun(Point p)
        {
			double result = (double)Math.Atan2(p.Y - lightsrc.future.Peek().Y, p.X - lightsrc.future.Peek().X) * (double)(180 / Math.PI)+90;
			return (int)result;
        }

		public static Bitmap RotateImage(Bitmap b, int angle)
		{
			Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
			using (Graphics g = Graphics.FromImage(returnBitmap))
			{
				g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
				g.RotateTransform(angle);
				g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
				g.DrawImage(b, 0, 0, b.Width, b.Height);
			}
			return returnBitmap;
		}

		public void Draw()
		{
            Queue<Point> tempFuture = new Queue<Point>(future);            

            Point CBLoc = vectoring ? future.Peek() : future.Dequeue();
			Point cPoint;

			int xOffset = (int)Form1.screenOffset.X;
			int yOffset = (int)Form1.screenOffset.Y;

			for (int i = 0; i < tempFuture.Count; i++)
			{
				cPoint = tempFuture.Dequeue();

				if (i % 10 == 0)
				{
					SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 - 255 * i / wayPointLookAhead, 255, 255, 255));
					g.FillEllipse(wayPointBrush, cPoint.X - 1 - xOffset, cPoint.Y - 1 - yOffset, 2, 2);
					wayPointBrush.Dispose();
				}	
			}
            tempFuture.Clear();


			if (history.Count > 0)
			{
				Queue<Point> tempHistroy = new Queue<Point>(history);
				for (int i = 0; i < history.Count; i++)
				{
					cPoint = tempHistroy.Dequeue();
					if (i % 10 == 0)
					{
						SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 * i / history.Count, 255, 255, 255));
						g.FillEllipse(wayPointBrush, cPoint.X - 1 -xOffset, cPoint.Y - 1 - yOffset, 2, 2);
						wayPointBrush.Dispose();
					}
				}
				tempHistroy.Clear();				
			}
			history.Enqueue(CBLoc);
			if (history.Count > wayPointLookAhead / 2)
				history.Dequeue();


			if (countOfRFrames == 0)
				g.FillEllipse(brush, CBLoc.X - height / 2 - xOffset, CBLoc.Y - height / 2 - yOffset, height, height);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], CBLoc.X - height / 2 - xOffset, CBLoc.Y - height / 2 - yOffset, rotationFrames[frameIndex].Width, rotationFrames[frameIndex].Height);
				frameIndex++;
				if (frameIndex == countOfRFrames)
					frameIndex = 0;
			}

			if (hasShadow)
			{
				Bitmap rotatedShadow = RotateImage(shadow, AngleFromSun(CBLoc));
				g.DrawImage(rotatedShadow, CBLoc.X - height / 2 - 2 - xOffset, CBLoc.Y - height / 2 - 2 - yOffset, shadow.Width, shadow.Height);
				rotatedShadow.Dispose();
			}
			else
				g.DrawImage(glow, CBLoc.X - (glow.Width / 2) - xOffset, CBLoc.Y - (glow.Height / 2) - yOffset, glow.Width, glow.Height);
		}

		public static void DrawAll(PictureBox pictureBox1)
		{
			g.DrawImage(space, 0, 0, space.Width, space.Height); //Frame törlése

			for (int i = 0; i < list.Count; i++)
				list[i].Draw();

			//pictureBox1.Refresh(); //nem működik, mert cross-threading lenne
			Refresh(pictureBox1);
		}


		public void DrawPlacement()
		{
			Point CBLoc;
			int xOffset = (int)Form1.screenOffset.X;
			int yOffset = (int)Form1.screenOffset.Y;

			if (!placing && !vectoring)
			{
				Queue<Point> tempFuture = new Queue<Point>(future);

				CBLoc = future.Peek();
				Point cPoint = tempFuture.Dequeue();

				for (int i = 0; i < future.Count - 1; i++)
				{
					cPoint = tempFuture.Dequeue();
					if (tempFuture.Count > 0)
					{
						if (i % 10 == 0)
						{
							SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 - 255 * i / wayPointLookAhead, 255, 255, 255));
							g.FillEllipse(wayPointBrush, cPoint.X - 1 - xOffset, cPoint.Y - 1 - yOffset, 2, 2);
							wayPointBrush.Dispose();
						}
					}
					else                 
                        g.FillEllipse(opaqueBrush, cPoint.X - height / 2 - xOffset, cPoint.Y - height / 2 - yOffset, height, height);
				}
				tempFuture.Clear();


				if (history.Count > 0)
				{
					Queue<Point> tempHistroy = new Queue<Point>(history);
					for (int i = 0; i < history.Count; i++)
					{
						cPoint = tempHistroy.Dequeue();

						if (i % 10 == 0)
						{
							SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 * i / history.Count, 255, 255, 255));
							g.FillEllipse(wayPointBrush, cPoint.X - 1 - xOffset, cPoint.Y - 1 - yOffset, 2, 2);
							wayPointBrush.Dispose();
						}
					}
					tempHistroy.Clear();
				}
			}
			else
				CBLoc = location.ToPoint();


			if (vectoring)
				for (int i = 0; i < wayPointLookAhead; i+=10)
					g.FillEllipse(whiteBrush, (int)(location.X + this.velocity.X * i - 1 - xOffset), (int)(location.Y + this.velocity.Y * i - 1 - yOffset), 2 , 2);
			

			if (countOfRFrames == 0)
				g.FillEllipse(brush, CBLoc.X - height / 2 - xOffset, CBLoc.Y - height / 2 - yOffset, height, height);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], CBLoc.X - height / 2 - xOffset, CBLoc.Y - height / 2 - yOffset, rotationFrames[frameIndex].Width, rotationFrames[frameIndex].Height);
				frameIndex++;
				if (frameIndex == countOfRFrames)
					frameIndex = 0;
			}

			if (hasShadow)
			{
				Bitmap rotatedShadow = RotateImage(shadow, AngleFromSun(CBLoc));
				g.DrawImage(rotatedShadow, CBLoc.X - height / 2 - 2 - xOffset, CBLoc.Y - height / 2 - 2 - yOffset, shadow.Width, shadow.Height);
				rotatedShadow.Dispose();
			}
			else
				g.DrawImage(glow, CBLoc.X - (glow.Width / 2) - xOffset, CBLoc.Y - (glow.Height / 2) - yOffset, glow.Width, glow.Height);
		}

		public static void DrawAllPlacement(PictureBox pictureBox1)
		{
			g.DrawImage(space, 0, 0, space.Width, space.Height); //Frame törlése

			for (int i = 0; i < list.Count; i++)
				list[i].DrawPlacement();

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
        private static void SetText(Label label, string text) //a fő szálon futó pictureBox1 frissítése
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


		public bool ContainsClick(int Xp, int Yp)
		{
			Vector offsettedLocation = new Vector(future.Peek().X, future.Peek().Y) - Form1.screenOffset;
			int Xc = (int)offsettedLocation.X;
			int Yc = (int)offsettedLocation.Y;

			int d = (int)Math.Sqrt(Math.Pow(Xp - Xc, 2) + Math.Pow(Yp - Yc, 2));

			return d <= height / 2 ? true : false;
		}


        public static bool running = false;
		internal static void StartSimulation(PictureBox pictureBox1, Label timeLabel, Label offsetLabel, CancellationTokenSource _canceller)
		{
			int time = 0;

			while (running)
			{
				CalcAllGVectors();
				SetAllVelocity();
				MoveAll();
				DrawAll(pictureBox1);

				SetText(timeLabel, time.ToString());
				time++;				


				if (Form1.following == null)
				{
					if (Form1.dragging)
						Form1.screenOffset = Form1.lastScreenOffset + (Form1.lastMousePos - new Vector(Form1.MousePosition));
				}
				else
				{
					if (Form1.following.future.Count == 0)
						Form1.screenOffset = Form1.following.location - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);
					else
						Form1.screenOffset = new Vector(Form1.following.future.Peek()) - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);
				}
				SetText(offsetLabel, Form1.screenOffset.ToString());


				if (_canceller.Token.IsCancellationRequested)
					break;
			}
		}
	}
}
