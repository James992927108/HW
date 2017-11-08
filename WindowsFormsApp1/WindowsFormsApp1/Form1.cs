﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
//1.匯入命名空間System.Drawing
using System.Drawing;
using System.IO;
using System.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        List<int> NodeNumList = new List<int>();
        List<NodeStruct> NodeList = new List<NodeStruct>(); //紀錄所有的Node
        ArrayList ReadFileArrayList = new ArrayList();//讀檔用
        private int RemainDateCount = 0;//用於紀錄測資剩餘次數
        //新增字型用
        Font myFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        //新增畫筆用於畫線
        Pen myPen = new Pen(Color.Red, 1);
        public Form1()
        {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e)
        {
            //2.建立控制項的Graphic物件，將這動作想像是開啟一個空白畫布
            Graphics gra = this.panel1.CreateGraphics();
            //3.新增Pen物件，想像他是一隻筆
            Pen myPen = new Pen(Color.Red, 1);
            //4.在控制項上繪製，想像你在空白畫布上畫東西
            //4.1繪製直線
            gra.DrawLine(myPen, 1, 1, 100, 100);
            //4.2繪製正方形
            gra.DrawRectangle(myPen, 10, 20, 80, 80);
            //4.3繪製一拋物線
            gra.DrawArc(myPen, 10, 20, 70, 80, 123, 233);
            //4.4繪製一矩形
            gra.DrawRectangle(myPen, 50, 60, 110, 120);
        }

        private void Clean_Click(object sender, EventArgs e)
        {
            clean_function();
        }

        public void clean_function()
        {
            //            NodeList.Clear();//清空有問題
            //            NodeNumList.Clear();
            //            this.button_Next.Hide();
            this.panel1.Refresh();
        }

        //--------------------------------------------------------------------------------------------------
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
        private void OpenFile_Click(object sender, EventArgs e)
        {
            clean_function();
            string line;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog1.FileName);
                while ((line = file.ReadLine()) != null)
                {
                    string[] lineArray = line.Select(o => o.ToString()).ToArray();
                    if (lineArray.Length != 0 && lineArray[0] != "#")
                    {
                        ReadFileArrayList.Add(line);
                    }
                }
                file.Close();
            }
            for (int i = 0; i < ReadFileArrayList.Count; i++)
            {
                Char delimiter = ' ';
                String[] substrings = ReadFileArrayList[i].ToString().Split(delimiter);
                if (substrings.Length == 1)
                {
                    //NodeNumList的值代表有幾個點
                    NodeNumList.Add(Convert.ToInt32(ReadFileArrayList[i]));
                }
                else//為座標放入NodeList裡面
                {
                    NodeStruct node = new NodeStruct();//新增點結構
                    node.X = Convert.ToInt32(substrings[0]);
                    node.Y = Convert.ToInt32(substrings[1]);
                    NodeList.Add(node);//塞到list裡面                   
                }
            }
            this.button_Next.Show();//show按鈕，顯示測資個數
            this.button_Next.Text = $"{NodeNumList.Count}";
            RemainDateCount = NodeNumList.Count;
        }

        private void Next_Click(object sender, EventArgs e)
        {
            clean_function();
            int CurrentDataIndex = (NodeNumList.Count) - RemainDateCount;
            if (RemainDateCount == 0)
            {
                MessageBox.Show("已無資料");
                this.button_Next.Hide();
            }
            else
            {
                this.button_Next.Text = $"{RemainDateCount}";
                DrawVerticalLine(NodeNumList[CurrentDataIndex]);
                //NodeList[0].x, NodeList[0].y => node放到nodelist後面，並移除nodelist的第一個，用於下一次計算
                for (int i = 0; i < NodeNumList[CurrentDataIndex]; i++)
                {
                    NodeStruct node = new NodeStruct();//新增點結構
                    node.X = NodeList[0].X;
                    node.Y = NodeList[0].Y;
                    NodeList.Add(node);//塞到list裡面
                    NodeList.RemoveAt(0);
                }
                RemainDateCount--;
            }
        }

        private void Output_txt_Click(object sender, EventArgs e)
        {
            FileStream fileStream = new FileStream(@"..\..\..\..\bbb.txt", FileMode.Create);
            fileStream.Close();
            StreamWriter sw = new StreamWriter(@"..\..\..\..\bbb.txt");
            //將NodeList中的元素跟具x值排序
            var NodeList_Sort = from a in NodeList
                                orderby a.X
                                select a;

            foreach (var node in NodeList_Sort)
            {
                sw.Write("P " + node.X + " " + node.Y);
                sw.Write(System.Environment.NewLine);
            }

            int count = 0;
            int temp_node_x = 0;
            int temp_node_y = 0;
            foreach (var node in NodeList_Sort)
            {
                if (count % 2 == 0)
                {
                    if (count == 0)
                        sw.Write("E " + node.X + " " + node.Y);
                    else
                    {
                        sw.Write("E " + temp_node_x + " " + temp_node_y);
                        temp_node_x = node.X;
                        temp_node_y = node.Y;
                    }
                }
                else
                {
                    sw.Write(" " + node.X + " " + node.Y);
                    temp_node_x = node.X;
                    temp_node_y = node.Y;

                    sw.Write(System.Environment.NewLine);
                }
                count++;
            }

            sw.Close();
            MessageBox.Show("完成輸出txt");
        }
        //--------------------------------------------------------------------------------------------------
        //private void OnPanelMouseMove(object sender, MouseEventArgs e) => Text = $"Coordinate [{e.X},{e.Y}]";
        //private void OnPanelMouseLeave(object sender, EventArgs e) => Text = "Voronoi Homework";

        private void OnPanelMouseDown(object sender, MouseEventArgs e)
        {
            NodeStruct node = new NodeStruct();//新增點結構
            node.X = e.X;
            node.Y = e.Y;
            NodeList.Add(node);//將點塞到list裡面

            clean_function();//每次都要先清空畫布
            DrawVerticalLine(NodeList.Count);
        }

        private void DrawVerticalLine(int NodeCount)
        {
            Graphics g = Graphics.FromHwnd(this.panel1.Handle);
            DrawNode(NodeCount);//根據點數量，先畫出點
            if (NodeCount == 2)
            {
                NodeStruct A = NodeList[0];
                NodeStruct B = NodeList[1];
                NodeStruct Mid = new NodeStruct();
                if (A.X == B.X && A.Y != B.Y)//垂直
                {
                    Mid.Y = (A.Y + B.Y) / 2;
                    g.DrawLine(myPen, 0, Mid.Y, 600, Mid.Y);
                }
                if (A.Y == B.Y && A.X != B.X)//水平
                {
                    Mid.X = (A.X + B.X) / 2;
                    g.DrawLine(myPen, Mid.X, 0, Mid.X, 600);
                }
                if (A.X != B.X && A.Y != B.Y)//不同點
                {
                    NodeStruct normal_vector = new NodeStruct();//得ab法向量(y,-x)
                    normal_vector.X = A.Y - B.Y;
                    normal_vector.Y = -(A.X - B.X);
                    Mid.X = (A.X + B.X) / 2;
                    Mid.Y = (A.Y + B.Y) / 2;
                    int K = 10000;
                    NodeStruct topNode = new NodeStruct();
                    topNode.X = Mid.X + K * normal_vector.X;
                    topNode.Y = Mid.Y + K * normal_vector.Y;
                    NodeStruct downNode = new NodeStruct();
                    downNode.X = Mid.X - K * normal_vector.X;
                    downNode.Y = Mid.Y - K * normal_vector.Y;
                    g.DrawLine(myPen, topNode.X, topNode.Y, downNode.X, downNode.Y);
                }
            }
            if (NodeCount == 3) //三點時要找外心
            {
                ClockwiseSortPoints();

                NodeStruct Excenter = new NodeStruct();//三點時要找外心
                Excenter = GetTriangleExcenter(NodeList[0], NodeList[1], NodeList[2]);

                if (Excenter.X != 0 && Excenter.Y != 0)//有找到外心時
                {
                    g.DrawImageUnscaled(get_NodeBitmap(), Excenter.X, Excenter.Y);

                    //做逆時針排序，用於找每一條邊的法向量
                    //以逆時針順序記錄三角形的三個頂點（三角形的三條邊變成有向邊）。這麼做的好處是，三角形依序取兩條邊計算叉積，就得到朝外的法向量

                    List<NodeStruct> normal_vector_List = new List<NodeStruct>();

                    //法向量為，若ab向量為(x,y)->法向量為(y,-x)

                    for (int i = 0; i < 3; i++)
                    {
                        NodeStruct normal_vector = new NodeStruct();
                        normal_vector.X = NodeList[(i + 1) % 3].Y - NodeList[i].Y;
                        normal_vector.Y = -(NodeList[(i + 1) % 3].X - NodeList[i].X);
                        normal_vector_List.Add(normal_vector);
                    }

                    //取的法向量後，找出每邊依法向量方向過每邊中點到邊界的點，預設找一點*k倍法向量
                    List<NodeStruct> MidNode_List = new List<NodeStruct>();
                    for (int i = 0; i < 3; i++)
                    {
                        NodeStruct MidNode = new NodeStruct();
                        MidNode.X = (NodeList[(i + 1) % 3].X + NodeList[i].X) / 2;
                        MidNode.Y = (NodeList[(i + 1) % 3].Y + NodeList[i].Y) / 2;
                        MidNode_List.Add(MidNode);
                    }

                    List<NodeStruct> Vertical_line_List = new List<NodeStruct>();
                    int K = 10000;
                    for (int i = 0; i < 3; i++)
                    {
                        NodeStruct Vertical_line = new NodeStruct();
                        Vertical_line.X = MidNode_List[i].X + K * normal_vector_List[i].X;
                        Vertical_line.Y = MidNode_List[i].Y + K * normal_vector_List[i].Y;
                        Vertical_line_List.Add(Vertical_line);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        g.DrawLine(myPen, Vertical_line_List[i].X, Vertical_line_List[i].Y, Excenter.X, Excenter.Y);
                    }
                }
                else//沒有外心時
                {
                    NodeStruct Mid1 = new NodeStruct();
                    NodeStruct Mid2 = new NodeStruct();
                    if (NodeList[0].X == NodeList[1].X && NodeList[0].X == NodeList[2].X)//垂直
                    {
                        List<int> tempList = new List<int>();
                        tempList.Add(NodeList[0].Y);
                        tempList.Add(NodeList[1].Y);
                        tempList.Add(NodeList[2].Y);
                        tempList.Sort();
                        //找2中點做2條水平線，對y軸要排序
                        Mid1.Y = (tempList[0] + tempList[1]) / 2;
                        Mid2.Y = (tempList[1] + tempList[2]) / 2;

                        g.DrawLine(myPen, 0, Mid1.Y, 600, Mid1.Y);
                        g.DrawLine(myPen, 0, Mid2.Y, 600, Mid2.Y);
                    }
                    if (NodeList[0].Y == NodeList[1].Y && NodeList[0].Y == NodeList[2].Y)//水平
                    {
                        //找2中點做2條水平線，對x軸要排序
                        List<int> tempList = new List<int>();
                        tempList.Add(NodeList[0].X);
                        tempList.Add(NodeList[1].X);
                        tempList.Add(NodeList[2].X);
                        tempList.Sort();
                        Mid1.X = (tempList[0] + tempList[1]) / 2;
                        Mid2.X = (tempList[1] + tempList[2]) / 2;

                        g.DrawLine(myPen, Mid1.X, 0, Mid1.X, 600);
                        g.DrawLine(myPen, Mid2.X, 0, Mid2.X, 600);
                    }
                    if ((NodeList[0].Y / NodeList[0].X) == (NodeList[1].Y / NodeList[1].X) && (NodeList[0].Y / NodeList[0].X) == (NodeList[2].Y / NodeList[2].X))//為一直線
                    {
                        NodeStruct normal_vector = new NodeStruct();//得ab法向量(y,-x)
                        normal_vector.X = NodeList[0].Y - NodeList[1].Y;
                        normal_vector.Y = -(NodeList[0].X - NodeList[1].X);
                        int K = 10000;

                        Mid1.X = (NodeList[0].X + NodeList[1].X) / 2;
                        Mid1.Y = (NodeList[0].Y + NodeList[1].Y) / 2;
                        NodeStruct topNode1 = new NodeStruct();
                        topNode1.X = Mid1.X + K * normal_vector.X;
                        topNode1.Y = Mid1.Y + K * normal_vector.Y;
                        NodeStruct downNode1 = new NodeStruct();
                        downNode1.X = Mid1.X - K * normal_vector.X;
                        downNode1.Y = Mid1.Y - K * normal_vector.Y;

                        g.DrawLine(myPen, topNode1.X, topNode1.Y, downNode1.X, downNode1.Y);

                        Mid2.X = (NodeList[2].X + NodeList[1].X) / 2;
                        Mid2.Y = (NodeList[2].Y + NodeList[1].Y) / 2;
                        NodeStruct topNode2 = new NodeStruct();
                        topNode2.X = Mid2.X + K * normal_vector.X;
                        topNode2.Y = Mid2.Y + K * normal_vector.Y;
                        NodeStruct downNode2 = new NodeStruct();
                        downNode2.X = Mid2.X - K * normal_vector.X;
                        downNode2.Y = Mid2.Y - K * normal_vector.Y;

                        g.DrawLine(myPen, topNode2.X, topNode2.Y, downNode2.X, downNode2.Y);
                    }
                }
            }
        }
        //--------------------------------------------------------------------------------------------------
        private void ClockwiseSortPoints()
        {
            NodeStruct center;
            double x = 0, y = 0;
            for (int i = 0; i < 3; i++)
            {
                x += NodeList[i].X;
                y += NodeList[i].Y;
            }
            center.X = (int)x / 3;
            center.Y = (int)y / 3;
            //冒泡排序
            for (int i = 0; i < 3 - 1; i++)
            {
                for (int j = 0; j < 3 - i - 1; j++)
                {
                    if (PointCmp(NodeList[j], NodeList[j + 1], center))
                    {
                        NodeStruct tmp = NodeList[j];
                        NodeList[j] = NodeList[j + 1];
                        NodeList[j + 1] = tmp;
                    }
                }
            }
        }
        bool PointCmp(NodeStruct a, NodeStruct b, NodeStruct center)
        {
            if (a.X >= 0 && b.X < 0)
                return true;
            if (a.X == 0 && b.X == 0)
                return a.Y > b.Y;
            //向量OA和向量OB的叉积
            int det = (a.X - center.X) * (b.Y - center.Y) - (b.X - center.X) * (a.Y - center.Y);
            if (det < 0)
                return true;
            if (det > 0)
                return false;
            //向量OA和向量OB共线，以距离判断大小
            int d1 = (a.X - center.X) * (a.X - center.X) + (a.Y - center.Y) * (a.Y - center.Y);
            int d2 = (b.X - center.X) * (b.X - center.Y) + (b.Y - center.Y) * (b.Y - center.Y);
            return d1 > d2;
        }
        //--------------------------------------------------------------------------------------------------

        private NodeStruct GetTriangleExcenter(NodeStruct A, NodeStruct B, NodeStruct C)
        {
            NodeStruct Excenter = new NodeStruct();//新增點結構
            NodeStruct noExcenter = new NodeStruct(0,0);//用於不存在外心時回傳
            //same point
            if (A.X == B.X && A.Y == B.Y && A.X == C.X && A.Y == C.Y)
            {
                Excenter = A;
                return Excenter;
            }
            //三點共線，利用面積，判断 (ax-cx)*(by-cy) == (bx-cx)*(ay-cy)
            if ((A.X - C.X) * (B.Y - C.Y) == (B.X - C.X) * (A.Y - C.Y))
            {
                return noExcenter;
            }
            double x1 = A.X, x2 = B.X, x3 = C.X, y1 = A.Y, y2 = B.Y, y3 = C.Y;
            double C1 = Math.Pow(x1, 2) + Math.Pow(y1, 2) - Math.Pow(x2, 2) - Math.Pow(y2, 2);
            double C2 = Math.Pow(x2, 2) + Math.Pow(y2, 2) - Math.Pow(x3, 2) - Math.Pow(y3, 2);
            double centery = (C1 * (x2 - x3) - C2 * (x1 - x2)) / (2 * (y1 - y2) * (x2 - x3) - 2 * (y2 - y3) * (x1 - x2));
            double centerx = (C1 - 2 * centery * (y1 - y2)) / (2 * (x1 - x2));
            Excenter = new NodeStruct(Convert.ToInt32(centerx), Convert.ToInt32(centery));
            return Excenter;
        }
        //--------------------------------------------------------------------------------------------------
        private static Bitmap get_NodeBitmap()
        {
            int NodeSize = 5;
            Bitmap nodeBitmap = new Bitmap(NodeSize, NodeSize); //这里调整点的大小   
            for (int i = 0; i < NodeSize; i++)
            {
                for (int j = 0; j < NodeSize; j++)
                {
                    nodeBitmap.SetPixel(i, j, Color.Black); //设置点的颜色   
                }
            }
            return nodeBitmap;
        }
        //--------------------------------------------------------------------------------------------------
        private void DrawNode(int NodeCount)
        {
            Graphics g = Graphics.FromHwnd(this.panel1.Handle);
            for (int i = 0; i < NodeCount; i++)
            {
                g.DrawImageUnscaled(get_NodeBitmap(), NodeList[i].X, NodeList[i].Y);
                g.DrawString($"{NodeList[i].X},{NodeList[i].Y}", myFont, Brushes.Firebrick, NodeList[i].X, NodeList[i].Y);
            }
        }
        //--------------------------------------------------------------------------------------------------

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        //--------------------------------------------------------------------------------------------------
        private void TEST_Click(object sender, EventArgs e)
        {
            int size = 5;
            Bitmap bm = new Bitmap(size, size);     //这里调整点的大小   
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    bm.SetPixel(i, j, Color.Black);       //设置点的颜色   
                }
            }
            Graphics g = Graphics.FromHwnd(this.panel1.Handle);     //画在哪里     
            g.DrawImageUnscaled(bm, 100, 100);       //具体坐标
        }
    }

    public struct NodeStruct
    {
        public int X, Y;
        public NodeStruct(int p1, int p2)
        {
            X = p1;
            Y = p2;
        }
    }
}
