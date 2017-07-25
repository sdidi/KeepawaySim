using System;
using Microsoft.Glee.Drawing;
using Microsoft.Glee.GraphViewerGdi;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace Keepaway
{
    public partial class Visualizers : Form
    {
        private System.Windows.Forms.Button button1;
        private GViewer gViewer;
        private System.Windows.Forms.Panel panel1;
        private Splitter splitter1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


       

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gViewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(86, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.gViewer);
            this.panel1.Controls.Add(this.splitter1);
            this.panel1.Location = new System.Drawing.Point(1, 50);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 688);
            this.panel1.TabIndex = 4;
            // 
            // gViewer
            // 
            this.gViewer.AsyncLayout = false;
            this.gViewer.AutoScroll = true;
            this.gViewer.BackwardEnabled = false;
            this.gViewer.ForwardEnabled = false;
            this.gViewer.Graph = null;
            this.gViewer.Location = new System.Drawing.Point(0, 0);
            this.gViewer.MouseHitDistance = 0.05D;
            this.gViewer.Name = "gViewer";
            this.gViewer.NavigationVisible = true;
            this.gViewer.PanButtonPressed = false;
            this.gViewer.SaveButtonVisible = true;
            this.gViewer.Size = new System.Drawing.Size(624, 578);
            this.gViewer.TabIndex = 0;
            this.gViewer.ZoomF = 1D;
            this.gViewer.ZoomFraction = 0.5D;
            this.gViewer.ZoomWindowThreshold = 0.05D;
            this.gViewer.Load += new System.EventHandler(this.gViewer_Load);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 688);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // Visualizers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Name = "Visualizers";
            this.Text = "Visualizers";
            this.Load += new System.EventHandler(this.Visualizers_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

       
        public Visualizers()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            NetworkGenome networkGenome = NetworkGenome.LoadFromFile(openFileDialog.FileName);
            Graph graph = new Graph("Genome: " + (object)networkGenome.Id);
            graph.GraphAttr.LayerDirection = LayerDirection.BT;
            graph.Directed = true;
            graph.GraphAttr.LayerSep = 10.0;
            graph.BuildNodeHierarchy = true;
            graph.GraphAttr.OptimizeLabelPositions = true;
            graph.GraphAttr.NodeAttr.Padding = 1.0;
            int num = networkGenome.Nodes.Find((Predicate<NodeGene>)(n => n.Type == NodeType.Output)).Layer;
            ArrayList arrayList1 = new ArrayList();
            for (int i = 0; i < num + 1; ++i)
            {
                ArrayList arrayList2 = new ArrayList();
                foreach (NodeGene nodeGene in Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)networkGenome.Nodes, (Func<NodeGene, bool>)(node => node.Layer == i)))
                    arrayList2.Add((object)nodeGene.Id.ToString());
                graph.AddSameLayer((IEnumerable)arrayList2);
            }
            graph.GraphAttr.Orientation = Microsoft.Glee.Drawing.Orientation.Landscape;
            graph.NeedCalculateLayout = true;
            graph.GraphAttr.Fontsize = 8;
            graph.GraphAttr.EdgeAttr.Fontsize = 7;
            graph.Cluster = true;
            graph.GraphAttr.LabelFloat = LabelFloat.Float;
            using (List<LinkGene>.Enumerator enumerator = networkGenome.Links.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    LinkGene link = enumerator.Current;
                    if (link.Weight != 0.0 && link.Source != -1)
                    {
                        //Edge edge = graph.AddEdge(link.Source.ToString(), link.Id.ToString(), link.Target.ToString());
                        Edge edge = graph.AddEdge(link.Source.ToString(), link.Weight.ToString("0.##"), link.Target.ToString());
                        edge.Attr.Fontsize = 7;
                        edge.EdgeAttr.Fontsize = 7;
                        if (link.Weight <= 0.0)
                            edge.Attr.Color = Microsoft.Glee.Drawing.Color.Red;
                        NodeGene nodeGene1 = networkGenome.Nodes.Find((Predicate<NodeGene>)(n => n.Id == link.Source));
                        NodeGene nodeGene2 = networkGenome.Nodes.Find((Predicate<NodeGene>)(n => n.Id == link.Target));
                        edge.Attr.Weight = Math.Max(nodeGene1.Layer - nodeGene2.Layer, 0);
                        edge.UserData = (object)link;
                    }
                }
            }
            for (int index = 0; index < networkGenome.Nodes.Count; ++index)
                (graph.FindNode(networkGenome.Nodes[index].Id.ToString()) ?? graph.AddNode(networkGenome.Nodes[index].Id.ToString())).UserData = (object)networkGenome.Nodes[index];
            foreach (NodeGene nodeGene in networkGenome.Nodes)
            {
                if (nodeGene.Layer == 0)
                    Visualizers.CreateSourceNode(graph.FindNode(nodeGene.Id.ToString()));
                else if (nodeGene.Layer == num)
                    this.CreateTargetNode(graph.FindNode(nodeGene.Id.ToString()));
                else
                    this.CreateHiddenNode(graph.FindNode(nodeGene.Id.ToString()));
            }
            this.gViewer.Graph = graph;
        }

        private void CreateHiddenNode(Microsoft.Glee.Drawing.Node a)
        {
            a.Attr.Fontsize = 8;
            a.Attr.Shape = Shape.Circle;
            a.Attr.XRad = 2f;
            a.Attr.YRad = 2f;
            a.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.White;
            a.Attr.LineWidth = 1;
        }

        private static void CreateSourceNode(Microsoft.Glee.Drawing.Node a)
        {
            a.Attr.Fontsize = 8;
            a.Attr.Shape = Shape.Box;
            a.Attr.XRad = 2f;
            a.Attr.YRad = 2f;
            a.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Green;
            a.Attr.LineWidth = 1;
        }

        private void CreateTargetNode(Microsoft.Glee.Drawing.Node a)
        {
            a.Attr.Fontsize = 8;
            a.Attr.Shape = Shape.Box;
            a.Attr.XRad = 2f;
            a.Attr.YRad = 2f;
            a.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Red;
            a.Attr.LineWidth = 1;
        }

        private void Visualizers_Load(object sender, EventArgs e)
        {

        }

        private void gViewer_Load(object sender, EventArgs e)
        {

        }
    }
}
