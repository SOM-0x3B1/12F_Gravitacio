using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
		public static CelestialBody following;

		private CancellationTokenSource _canceller;
		

		CelestialBody sun = new CelestialBody(new Vector(760, 300), new Vector(-0.5, 0), 100, 350, Properties.Resources.sun, false);
		CelestialBody earth = new CelestialBody(new Vector(700, 60), new Vector(0.5, 0), 50, 40, Properties.Resources.earth, true);
		CelestialBody moon = new CelestialBody(new Vector(650, 0), new Vector(-1.5, 0.1), 20, 10, Properties.Resources.moon, true);
		CelestialBody mars = new CelestialBody(new Vector(740, 540), new Vector(-1.5, 0), 40, 30, Properties.Resources.mars, true);

		public Form1()
		{
			InitializeComponent();

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
			pictureBox1.Image = bmp;
			CelestialBody.g = Graphics.FromImage(pictureBox1.Image);
			CelestialBody.g.Clear(Color.Black);
			pictureBox1.Refresh();

			following = sun;
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

			_canceller.Dispose();
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
			following = null;
			dragging = true;
			lastMousePos = new Vector(MousePosition.X, MousePosition.Y);
		}

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
			dragging = false;
			lastScreenOffset = screenOffset;
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
