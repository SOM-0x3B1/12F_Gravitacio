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
		private CancellationTokenSource _canceller;

		CelestialBody mozgo = new CelestialBody(new Vector(300, 90), new Vector(0, 0), 100, 10, Properties.Resources.sun, false);
		CelestialBody mozgo2 = new CelestialBody(new Vector(200, 60), new Vector(2, -1), 50, 1, Properties.Resources.earth, true);

		public Form1()
		{
			InitializeComponent();

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
			pictureBox1.Image = bmp;
			CelestialBody.g = Graphics.FromImage(pictureBox1.Image);
			CelestialBody.g.Clear(Color.Black);
			pictureBox1.Refresh();

			CelestialBody.DrawAll(pictureBox1);
		}


		private async void button1_Click(object sender, EventArgs e)
		{
			button1.Enabled = false;
			button2.Enabled = true;

			_canceller = new CancellationTokenSource();

			await Task.Run(() =>
			{
				CelestialBody.running = true;
				CelestialBody.StartSimulation(pictureBox1, label2, _canceller);
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
