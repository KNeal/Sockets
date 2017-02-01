using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DemoApp.Simulation;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        //private ISimulation _simulation = new LocalSimulation();
        private ISimulation _simulation = new NetworkedSimulation();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        } 

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _simulation.Start(graphicsView);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            _simulation.Update();
            graphicsView.Refresh();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _simulation.Stop();
        }
    }
}
