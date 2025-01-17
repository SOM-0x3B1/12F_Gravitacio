﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;

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

        public static DateTime _lastTime; // marks the beginning the measurement began
        public static int _framesRendered; // an increasing count
        public static int _fps; // the FPS calculated from the last measurement


        private CelestialBody sun = new CelestialBody("sun", new Vector(760, 300), new Vector(0.1, 0), 100, 500, Properties.Resources.sun, false);
		private CelestialBody earth = new CelestialBody("earth", new Vector(700, 60), new Vector(1.5, 0), 50, 40, Properties.Resources.earth, true);
		private CelestialBody moon = new CelestialBody("moon", new Vector(600, -100), new Vector(1.3, -0.1), 20, 10, Properties.Resources.moon, true);
		//CelestialBody mars = new CelestialBody(new Vector(740, 540), new Vector(-1.5, 0), 40, 30, Properties.Resources.mars, true);

		public Form1()
		{
			InitializeComponent();

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
			pictureBox1.Image = bmp;
			CelestialBody.g = Graphics.FromImage(pictureBox1.Image);
			CelestialBody.g.Clear(Color.Black);
			pictureBox1.Refresh();

			following = sun;
			if(following != null)
				screenOffset = following.location - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);


			CelestialBody.fpsLabel = fpsLabel;
			CelestialBody.wayPointLookAhead = 200;
			CelestialBody.lightsrc = sun;
			for (int i = 0; i < CelestialBody.wayPointLookAhead; i++)
            {
                CelestialBody.CalcAllGVectors();
                CelestialBody.SetAllVelocity();
                CelestialBody.MoveAll();
            }

            CelestialBody.DrawAll(pictureBox1);
			offsetLabel.Text = screenOffset.ToString();
		}


		private async void start_Click(object sender, EventArgs e)
		{
			startButton.Enabled = false;
			stopButton.Enabled = true;

			_canceller = new CancellationTokenSource();

			await Task.Run(() =>
			{
				CelestialBody.running = true;
				CelestialBody.StartSimulation(pictureBox1, timeLabel, offsetLabel, fpsLabel, _canceller);
			});
		}

		private void stop_Click(object sender, EventArgs e)
		{
			startButton.Enabled = true;
			stopButton.Enabled = false;

			_canceller.Cancel();
			CelestialBody.running = false;
		}


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {			
			lastScreenOffset = screenOffset;

			if (vectoring == null && placing == null)
			{
                lastMousePos = new Vector(MousePosition.X, MousePosition.Y);
                Point relClickLoc = pictureBox1.PointToClient(Cursor.Position);
				bool inCB = false;

                for (int i = 0; i < CelestialBody.list.Count && !inCB; i++)
				{					
					inCB = CelestialBody.list[i].ContainsClick(relClickLoc.X, relClickLoc.Y);
					if (inCB)
						following = CelestialBody.list[i];
                }

				if (!inCB)
				{					
					following = null;
					dragging = true;
				}
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



        /// <summary>
        /// Add a planet after clicking the addPlanet1 button
        /// </summary>
        private void addPlanet1_Click(object sender, EventArgs e)
		{

			AddPlanet("mars", 40, 30, Properties.Resources.mars); 

		}


        /// <summary>
        /// The procedure of adding a new planet
		/// It will create a new planet object, and will let the user place it and set its vectors 
        /// </summary>
        /// <param name="name">The name of the planet</param>
        /// <param name="size">The size of the planet in pixels</param>
        /// <param name="mass">The mass of the planet</param>
        /// <param name="image">The Bitmap image of the planet</param>
        private async void AddPlanet(string name, int size, int mass, Bitmap image)
		{
            stop_Click(null, null); // pauses the simulation

            lastMousePos = new Vector(MousePosition.X, MousePosition.Y);
            CelestialBody newPlanet = new CelestialBody(name, new Vector(MousePosition), new Vector(0, 0), size, mass, image, true);

            await Task.Run(() =>
            {
                newPlanet.placing = true;

                placing = newPlanet;
                while (placing != null)
                {
                    if (pictureBox1.InvokeRequired)
                    {
                        pictureBox1.Invoke(new MethodInvoker(
                        delegate ()
                        {
                            newPlanet.location = new Vector(pictureBox1.PointToClient(Cursor.Position)) + screenOffset;
                            CelestialBody.DrawAllPlacement(pictureBox1);
                        }));
                    }
                    else
                    {
                        newPlanet.location = new Vector(pictureBox1.PointToClient(Cursor.Position)) + screenOffset;
                        CelestialBody.DrawAllPlacement(pictureBox1);
                    }

                    CelestialBody.UpdateFPS();
                }
                newPlanet.placing = false;

                following = newPlanet;
                screenOffset = following.location - new Vector(pictureBox1.Width / 2, pictureBox1.Height / 2);


                vectoring = newPlanet;
                newPlanet.vectoring = true;
                lastMousePos = new Vector(MousePosition) + (lastScreenOffset - screenOffset);
                while (vectoring != null)
                {
                    newPlanet.velocity = (lastMousePos - new Vector(MousePosition)) / 100;
                    CelestialBody.DrawAllPlacement(pictureBox1);
                }


                CelestialBody.CalcAllGVectors();
                CelestialBody.SetAllVelocity();
                CelestialBody.MoveAll();


                CelestialBody.DrawAll(pictureBox1);
            });

            start_Click(null, null); // resumes the game
        }
    }
}
