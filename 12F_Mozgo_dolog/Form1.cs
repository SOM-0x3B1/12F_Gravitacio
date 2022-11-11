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
		Mozgo mozgo = new Mozgo(new Vektor(90, 90), new Vektor(7, 4), 30, Color.Navy);
		Mozgo mozgo2 = new Mozgo(new Vektor(30, 60), new Vektor(-3, 2.5), 20, Color.Black);

		public Form1()
		{
			InitializeComponent();

			//MessageBox.Show("szia!");

			Mozgo.Összes_lerajzolása(pictureBox1);



		}


		private void button1_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 10; i++)
			{
				Thread.Sleep(200);
				mozgo.Lépj();
				mozgo2.Lépj();
				Mozgo.Összes_lerajzolása(pictureBox1);
			}
		}
	}
}
