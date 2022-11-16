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


		CelestialBody mozgo = new CelestialBody(new Vector(90, 90), new Vector(1, 1), 40, 40, Color.Orange);
		CelestialBody mozgo2 = new CelestialBody(new Vector(30, 60), new Vector(1, 2), 30, 10, Properties.Resources.earth);

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

			_canceller = new CancellationTokenSource();			

			await Task.Run(() =>
			{
				CelestialBody.running = true;
				CelestialBody.StartSimulation(pictureBox1, _canceller);
			});

			_canceller.Dispose();
		}

        private void button2_Click(object sender, EventArgs e)
        {
			button1.Enabled = true;

			_canceller.Cancel();
			CelestialBody.running = false;			
		}
    }
}
