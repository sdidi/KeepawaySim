// -*-C#-*-

/***************************************************************************
                                   FieldVisualizer.cs
                   Visualizer for C# implemenation fo RoboCup simulator
                             -------------------
    begin                : JUL-2009
    credit               : Implementation done by Phillip Verbancsics of the
                            Evolutionary Complexity Research Group
    email                : verb@cs.ucf.edu
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RoboCup
{
    public partial class FieldVisualizer : Form
    {
        private bool visualize = true;
        public RoboCup.Stadium std;
        public FieldVisualizer()
        {
            
            InitializeComponent();
      
            
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }


        private void FieldVisualizer_Paint(object sender, PaintEventArgs e)
        {
            if (visualize)
            {
                if (std != null)
                {

                    std.Paint(sender, e);
                    button1.Enabled = begin;
                    label1.Text = "Time: " + std.Time;
                    label2.Text = "Speed: " + speedup.ToString("0.0");
            

                }
                foreach (var f in OwnedForms)
                {
                    f.Invalidate();
                    f.Validate();
                }


            }
        }

        private void FieldVisualizer_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            switch (e.KeyChar)
            {
                case 'v': visualize = !visualize;
                    break;
                case 's': if(std != null) std.Realtime = !std.Realtime;
                    break;
                case 'b': if (begin) { backgroundWorker1.RunWorkerAsync(); System.Threading.Thread.Sleep(10); begin = false;}
                    
                    break;
                case 'p': if(std != null) std.Pause = !std.Pause;
                    break;
                case 'f': if(std != null) std.Step = true;
                    break;
                default: break;
            }

        }

        private bool begin = true;
        
        DateTime prev = DateTime.Now;
        double speedup = 0.0;
        public void OnStadiumUpdate(RoboCup.Stadium std)
        {
            this.std = std;
            DateTime now = DateTime.Now;
            speedup = 100.0 / (now - prev).TotalMilliseconds;
            prev = DateTime.Now;
 
            this.Invalidate();
        }

        private void FieldVisualizer_Resize(object sender, EventArgs e)
        {

            this.Invalidate();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (begin) { backgroundWorker1.RunWorkerAsync(); System.Threading.Thread.Sleep(10); begin = false; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (std != null)
            {
                std.Pause = !std.Pause;
                if (std.Pause)
                {
                    button2.Text = "Resume";
                }
                else
                {
                    button2.Text = "Pause";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (std != null) std.Step = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Button but = sender as Button;
            
            if (std != null)
            {
                std.Realtime = !std.Realtime;
                if (std.Realtime)
                {
                    but.Text = "Fast";
                }
                else
                {
                    but.Text = "Slow";
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            visualize = !visualize;
            Button but = sender as Button;
            if (visualize)
            {
                but.Text = "NoVisual";
            }
            else
            {
                but.Text = "Visual";
            }

        }
        ObjectData fob;
        private void button6_Click(object sender, EventArgs e)
        {
            if (fob == null)
            {
                fob = new ObjectData();
                fob.setStadium(std);
                
                AddOwnedForm(fob);
                fob.Show();
               
            }
        }

    }
}
