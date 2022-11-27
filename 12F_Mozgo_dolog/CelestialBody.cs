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
		string id;
		SolidBrush brush;		
		Bitmap planetTexture;
		static List<CelestialBody> list = new List<CelestialBody>(); // ezt muszáj itt inicializálni most.
		int countOfRFrames = 0;
		int frameIndex = 0;
		bool hasShadow;
		Bitmap shadow;
		Bitmap glow;
		Bitmap mask;
		List<Bitmap> rotationFrames = new List<Bitmap>();
		SolidBrush whiteBrush = new SolidBrush(Color.White);

		public bool placing;
		public bool vectoring;

		public static int wayPointLookAhead;
		//List<BasicCB> wayPoints = new List<BasicCB>();		
		Queue<Point> history = new Queue<Point>();
		//LinkedList<int> llist = new LinkedList<int>();

		public static BasicCB sun;
		public static Bitmap space = Properties.Resources.space;
		public static Graphics g; // a Form1-ben, kívülről inicializálom, így nem kell using (Graphics g...)-t használni frame-enként
		

		public CelestialBody(string id, Vector location, Vector velocity, int height, double mass, Color color)
		{
			this.id = id;
			this.location = location;
			this.velocity = velocity;
			this.size = height;
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
			this.size = height;
			this.mass = mass;			
			this.movements = new List<Vector>();

			//this.color = color;
			this.countOfRFrames = 100;
			Bitmap frame = new Bitmap(height, height);
			//this.planetTexture = palentTexture;
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
			//Settext(label3, result.ToString());
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
				g.DrawImage(b, 0, 0, b.Width, b.Height);
			}
			return returnBitmap;
		}

		public void Draw()
		{
            Queue<Point> tempFuture = new Queue<Point>(future);            

            Point CBPoint = vectoring ? future.Peek() : future.Dequeue();
			Point cpoint;

			int xOffset = (int)Form1.screenOffset.X;
			int yOffset = (int)Form1.screenOffset.Y;

			for (int i = 0; i < tempFuture.Count; i++)
			{
				cpoint = tempFuture.Dequeue();
				if (i % 10 == 0)
				{
					SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 - 255 * i / wayPointLookAhead, 255, 255, 255));
					g.FillEllipse(wayPointBrush, cpoint.X - 1 - xOffset, cpoint.Y - 1 - yOffset, 2, 2);
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
					if (i % 10 == 0)
					{
						SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 * i / history.Count, 255, 255, 255));
						g.FillEllipse(wayPointBrush, cpoint.X - 1 -xOffset, cpoint.Y - 1 - yOffset, 2, 2);
						wayPointBrush.Dispose();
					}
				}
				tempHistroy.Clear();				
			}
			history.Enqueue(CBPoint);
			if (history.Count > wayPointLookAhead / 2)
				history.Dequeue();


			if (countOfRFrames == 0)
				g.FillEllipse(brush, CBPoint.X - size / 2 - xOffset, CBPoint.Y - size / 2 - yOffset, size, size);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], CBPoint.X - size / 2 - xOffset, CBPoint.Y - size / 2 - yOffset, rotationFrames[frameIndex].Width, rotationFrames[frameIndex].Height);
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
				Bitmap rotatedShadow = RotateImage(shadow, AngleFromSun(CBPoint));
				g.DrawImage(rotatedShadow, CBPoint.X - size / 2 - 2 - xOffset, CBPoint.Y - size / 2 - 2 - yOffset, shadow.Width, shadow.Height);
				rotatedShadow.Dispose();
			}
			else
				g.DrawImage(glow, CBPoint.X - (glow.Width / 2) - xOffset, CBPoint.Y - (glow.Height / 2) - yOffset, glow.Width, glow.Height);
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
			Point CBPoint;
			int xOffset = (int)Form1.screenOffset.X;
			int yOffset = (int)Form1.screenOffset.Y;

			if (!placing && !vectoring)
			{
				Queue<Point> tempFuture = new Queue<Point>(future);

				CBPoint = future.Peek();
				Point cpoint = tempFuture.Dequeue();

				for (int i = 0; i < future.Count - 1; i++)
				{
					cpoint = tempFuture.Dequeue();
					if (i % 10 == 0)
					{
						SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 - 255 * i / wayPointLookAhead, 255, 255, 255));
						g.FillEllipse(wayPointBrush, cpoint.X - 1 - xOffset, cpoint.Y - 1 - yOffset, 2, 2);
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
						if (i % 10 == 0)
						{
							SolidBrush wayPointBrush = new SolidBrush(Color.FromArgb(255 * i / history.Count, 255, 255, 255));
							g.FillEllipse(wayPointBrush, cpoint.X - 1 - xOffset, cpoint.Y - 1 - yOffset, 2, 2);
							wayPointBrush.Dispose();
						}
					}
					tempHistroy.Clear();
				}
			}
			else
				CBPoint = location.ToPoint();


			if (vectoring)
				for (int i = 0; i < wayPointLookAhead; i+=10)
					g.FillEllipse(whiteBrush, (int)(location.X + this.velocity.X * i - 1 - xOffset), (int)(location.Y + this.velocity.Y * i - 1 - yOffset), 2 , 2);
			//g.DrawLine(whitePen, (int)(location.X - 1 - xOffset), (int)(location.Y - 1 - yOffset), (int)(location.X + this.velocity.X - 1 - xOffset), (int)(location.Y + this.velocity.Y - 1 - yOffset));


			if (countOfRFrames == 0)
				g.FillEllipse(brush, CBPoint.X - size / 2 - xOffset, CBPoint.Y - size / 2 - yOffset, size, size);
			else
			{
				g.DrawImage(rotationFrames[frameIndex], CBPoint.X - size / 2 - xOffset, CBPoint.Y - size / 2 - yOffset, rotationFrames[frameIndex].Width, rotationFrames[frameIndex].Height);
				frameIndex++;
				if (frameIndex == countOfRFrames)
					frameIndex = 0;
			}

			if (hasShadow)
			{
				Bitmap rotatedShadow = RotateImage(shadow, AngleFromSun(CBPoint));
				g.DrawImage(rotatedShadow, CBPoint.X - size / 2 - 2 - xOffset, CBPoint.Y - size / 2 - 2 - yOffset, shadow.Width, shadow.Height);
				rotatedShadow.Dispose();
			}
			else
				g.DrawImage(glow, CBPoint.X - (glow.Width / 2) - xOffset, CBPoint.Y - (glow.Height / 2) - yOffset, glow.Width, glow.Height);
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
		internal static void StartSimulation(PictureBox pictureBox1, Label label2, Label label3, CancellationTokenSource _canceller)
		{
			int time = 0;

			while (running)
			{
				CalcAllGVectors();
				SetAllVelocity();
				MoveAll();
				DrawAll(pictureBox1);

				Settext(label2, time.ToString());
				time++;

				Settext(label3, Form1.screenOffset.ToString());

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


				if (_canceller.Token.IsCancellationRequested)
					break;
			}
		}
	}
}
