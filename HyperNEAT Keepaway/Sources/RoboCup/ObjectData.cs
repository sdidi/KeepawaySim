// -*-C#-*-

/***************************************************************************
                                   ObjectData.cs
                   Object Information Viewer for C# implemenation fo RoboCup simulator
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
    public partial class ObjectData : Form
    {
        Stadium std;
        public ObjectData()
        {
            InitializeComponent();
        }
        string selected;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            selected = combo.SelectedItem as string;

            RoboCup.Objects.MovingObject obj = null;

            if (selected[0] == 'B')
            {
                int id = int.Parse(selected.Split(' ')[1]);
                obj = std.Balls.Find(b => b.Id == id);
            }
            else
            {
                string[] split = selected.Split(' ');
                string team = split[1];
                int unum = int.Parse(split[2]);

                obj = std.Players.Find(p => p.Unum == unum && p.Team == team);
            }

            label6.Text = obj.currentPosition.X.ToString("0.00");
            label7.Text = obj.currentPosition.Y.ToString("0.00");
            label8.Text = obj.objectVelocity.X.ToString("0.00");
            label9.Text = obj.objectVelocity.Y.ToString("0.00");
            
        }

        public void setStadium(Stadium st)
        {
            this.std = st;

            foreach (var m in std.Balls)
            {
                comboBox1.Items.Add("Ball " + m.Id);

            }

            foreach (var m in std.Players)
            {
                comboBox1.Items.Add("Player " + m.Team + " " + m.Unum);
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {

        }

        private void ObjectData_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                updateVals(sender as ObjectData);

                Graphics g = e.Graphics;

                g.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);

            }
            catch (Exception ex)
            {
                ex.ToString();
                int i = 0;
                i++;
            }
        }

        internal void updateVals(ObjectData fob)
        {
            if (fob == null || fob.selected == null || fob.std == null) return;
            RoboCup.Objects.MovingObject obj = null;

            if (fob.selected[0] == 'B')
            {
                int id = int.Parse(fob.selected.Split(' ')[1]);
                obj = fob.std.Balls.Find(b => b.Id == id);
            }
            else
            {
                string[] split = fob.selected.Split(' ');
                string team = split[1];
                int unum = int.Parse(split[2]);

                obj = fob.std.Players.Find(p => p.Unum == unum && p.Team == team);
            }

            if (obj == null) return;

            fob.label6.Text = obj.currentPosition.X.ToString("0.00");
            fob.label7.Text = obj.currentPosition.Y.ToString("0.00");
            fob.label8.Text = obj.objectVelocity.X.ToString("0.00");
            fob.label9.Text = obj.objectVelocity.Y.ToString("0.00");
        }

        private void ObjectData_Validating(object sender, CancelEventArgs e)
        {
        
        }

        private void ObjectData_Validated(object sender, EventArgs e)
        {
            updateVals(sender as ObjectData);
        }
    }
}
