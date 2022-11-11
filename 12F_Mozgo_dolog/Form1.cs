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
		CelestialBody mozgo = new CelestialBody(new Vector(90, 90), new Vector(7, 4), 30, 2, Color.Orange);
		CelestialBody mozgo2 = new CelestialBody(new Vector(30, 60), new Vector(-3, 2.5), 20, 1, Color.Blue);

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


		private void button1_Click(object sender, EventArgs e)
		{
			/*for (int i = 0; i < 100; i++)
			{
				Thread.Sleep(20);
				mozgo.Move();
				mozgo2.Move();
				Mozgo.DrawAll(pictureBox1);
			}*/

			CelestialBody.running = true;
			CelestialBody.StartSimulation(pictureBox1);
		}

        private void button2_Click(object sender, EventArgs e)
        {
			CelestialBody.running = false;
		}
    }
}
