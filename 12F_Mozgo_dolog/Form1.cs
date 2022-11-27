using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace _12F_Mozgo_dolog
{
	public partial class Form1 : Form
	{
		public static Vector screenOffset = new Vector(0, 0);
		public static Vector lastScreenOffset = new Vector(0, 0);
		public static Vector lastMousePos = new Vector(0, 0);
		public static bool dragging;
		public static CelestialBody placing;
		public static CelestialBody vectoring;
		public static CelestialBody following;

		private CancellationTokenSource _canceller = new CancellationTokenSource();
		

		CelestialBody sun = new CelestialBody("sun", new Vector(760, 300), new Vector(-0.5, 0), 100, 500, Properties.Resources.sun, false);
		CelestialBody earth = new CelestialBody("earth", new Vector(700, 60), new Vector(0.8, 0), 50, 40, Properties.Resources.earth, true);
		CelestialBody moon = new CelestialBody("moon", new Vector(600, -100), new Vector(0.6, -0.1), 20, 10, Properties.Resources.moon, true);
		//CelestialBody mars = new CelestialBody(new Vector(740, 540), new Vector(-1.5, 0), 40, 30, Properties.Resources.mars, true);

		public Form1()
		{
			InitializeComponent();

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
			pictureBox1.Image = bmp;
			CelestialBody.g = Graphics.FromImage(pictureBox1.Image);
			CelestialBody.g.Clear(Color.Black);
			pictureBox1.Refresh();

			following = earth;
			screenOffset = following.location - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);

			CelestialBody.wayPointLookAhead = 200;
			CelestialBody.sun = sun;
			for (int i = 0; i < CelestialBody.wayPointLookAhead; i++)
            {
                CelestialBody.CalcAllGVectors();
                CelestialBody.SetAllVelocity();
                CelestialBody.MoveAll();
            }
            CelestialBody.DrawAll(pictureBox1);

			label3.Text = screenOffset.ToString();
		}


		private async void button1_Click(object sender, EventArgs e)
		{
			button1.Enabled = false;
			button2.Enabled = true;

			_canceller = new CancellationTokenSource();

			await Task.Run(() =>
			{
				CelestialBody.running = true;
				CelestialBody.StartSimulation(pictureBox1, label2, label3, _canceller);
			});
		}

		private void button2_Click(object sender, EventArgs e)
		{
			button1.Enabled = true;
			button2.Enabled = false;

			_canceller.Cancel();
			CelestialBody.running = false;
		}


		//TODO

		private void button1_EnabledChanged(object sender, EventArgs e){/*button1.ForeColor = button1.Enabled == false ? Color.DimGray : Color.White;*/}

		private void button1_Paint(object sender, PaintEventArgs e){/*changePaint(button1, sender, e);*/}

		private void button2_EnabledChanged(object sender, EventArgs e) { /*button2.ForeColor = button1.Enabled == false ? Color.DimGray : Color.White;*/ }

		private void button2_Paint(object sender, PaintEventArgs e) { /*changePaint(button2, sender, e);*/ }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {			
			lastScreenOffset = screenOffset;

			if (vectoring == null)
			{
				lastMousePos = new Vector(MousePosition.X, MousePosition.Y);
				following = null;
				dragging = true;
			}
		}

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
			dragging = false;
		    placing = null;

			if (vectoring != null)
			{
				vectoring.velocity = (lastMousePos - new Vector(MousePosition))/100;
				vectoring = null;				
			}
		}


		private async void button3_Click(object sender, EventArgs e)
		{
			button2_Click(sender, e);

			lastMousePos = new Vector(MousePosition.X, MousePosition.Y);
			CelestialBody mars = new CelestialBody("mars", new Vector(MousePosition), new Vector(0, 0), 40, 30, Properties.Resources.mars, true);

			await Task.Run(() =>
			{
				placing = mars;
				mars.placing = true;
				while (placing != null)
				{
					if (pictureBox1.InvokeRequired)
					{
						pictureBox1.Invoke(new MethodInvoker(
						delegate ()
						{
							mars.location = new Vector(pictureBox1.PointToClient(Cursor.Position)) + screenOffset;
							CelestialBody.DrawAllPlacement(pictureBox1);
						}));
					}
					else
					{
						mars.location = new Vector(pictureBox1.PointToClient(Cursor.Position)) + screenOffset;
						CelestialBody.DrawAllPlacement(pictureBox1);
					}
				}
				mars.placing = false;

				following = mars;
				screenOffset = following.location - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);

				vectoring = mars;
				mars.vectoring = true;
				lastMousePos = new Vector(MousePosition) + (lastScreenOffset - screenOffset);
				while (vectoring != null)
				{
					mars.velocity = (lastMousePos - new Vector(MousePosition)) / 100;
					CelestialBody.DrawAllPlacement(pictureBox1);
				}
				mars.vectoring = false;


				for (int i = 0; i < CelestialBody.wayPointLookAhead; i++)
				{
					mars.CalcGVectors();
					mars.SetVelocity();
					mars.Move();
				}
				CelestialBody.CalcAllGVectors();
				CelestialBody.SetAllVelocity();
				CelestialBody.MoveAll();

				CelestialBody.DrawAll(pictureBox1);
			});

			button1_Click(sender, e);			
		}

        /*private void changePaint(Button button, object sender, PaintEventArgs e)
		{
            dynamic btn = (Button)sender;
            dynamic drawBrush = new SolidBrush(btn.ForeColor);
            dynamic sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };             
            e.Graphics.DrawString(button.Text, btn.Font, drawBrush, e.ClipRectangle, sf);
            drawBrush.Dispose();
            sf.Dispose();
        }*/
    }
}
