using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // вспомагательные данные
        const int MAX = 9999999;
        const int R = 28; // размер вершины
        int curTopFourLR = 0; // это для маркера 4 лр
        int LoadingProsent = 0;
        int IndexSelectRibs;
        int IndexSelectTops;
        int sqrtCount;
        bool IsAded;
        bool Replace = false;
        int Upped = 1;
        short CountClick = 0;
        List<int[]> RezFiveLR = new List<int[]>();
        List<rib> RezultOfFourLR = new List<rib>();
        Graph graph = new Graph();
        Point firstp = new Point();
        Point Mouse = new Point();
        String NameButtonMouse;
        StringFormat Sf = new StringFormat();
        Stack<int> KeeperWays = new Stack<int>(); // хранитель пути
        List<int[]> Ways = new List<int[]>(); // место для хранения путей
        Pen PenForSelectedTop = new Pen(Brushes.Green, 3); // для окраски 
        Pen PenForSimpleTops = new Pen(Brushes.Red, 3);
        //

        public Form1()
        {
            DoubleBuffered = true;
            InitializeComponent();
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (graph.Tops.Count == 0)
                IndexSelectTops = 0;
            for (int i = 0; i < graph.Tops.Count; i++)
                if (Math.Abs(e.X - graph.Tops[i].position.X) < R && Math.Abs(e.Y - graph.Tops[i].position.Y) < R)
                {
                    Replace = true;
                    IsAded = false;
                    IndexSelectTops = i;
                    break;
                }
            NameButtonMouse = Control.MouseButtons.ToString();
            if (NameButtonMouse != "Left")
                Replace = false;
            if (Replace == false && NameButtonMouse == "Left")
            {
                firstp = e.Location;
                graph.InsertTop(firstp);
                graph.matrix.Add(new List<int>());
                int i = 0;
                while (i < graph.Tops.Count) // приводим матрицу к квадратному виду
                {
                    while (graph.matrix[i].Count < graph.Tops.Count)
                        graph.matrix[i].Add(MAX);
                    i++;
                }
                IsAded = true;
                ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            }
            pictureBox1.Refresh();
            Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            NameButtonMouse = Control.MouseButtons.ToString();
            Graphics gr = pictureBox1.CreateGraphics();
            if (NameButtonMouse == "Left")
            {
                if (Replace == false)
                {
                    if (graph.Tops[graph.Tops.Count - 1].position.X + R / 2 + PenForSimpleTops.Width > pictureBox1.Width)
                        graph.Tops[graph.Tops.Count - 1].position.X = pictureBox1.Width - R / 2 - (int)PenForSimpleTops.Width;
                    if (e.Y > pictureBox1.Height)
                        graph.Tops[graph.Tops.Count - 1].position.Y = pictureBox1.Height - R / 2 - (int)PenForSimpleTops.Width;
                    if (e.X < R / 2)
                        graph.Tops[graph.Tops.Count - 1].position.X = R / 2;
                    if (e.Y < R / 2)
                        graph.Tops[graph.Tops.Count - 1].position.Y = R / 2;
                    if (e.Y + R / 2 < pictureBox1.Height && e.Y > R / 2)
                        graph.Tops[graph.Tops.Count - 1].position.Y = e.Y;
                    if (e.X + R / 2 < pictureBox1.Width && e.X > R / 2)
                        graph.Tops[graph.Tops.Count - 1].position.X = e.X;
                }
                else
                {
                    if (e.X + R / 2 + PenForSelectedTop.Width > pictureBox1.Width)
                        graph.Tops[IndexSelectTops].position.X = pictureBox1.Width - R / 2 - (int)PenForSelectedTop.Width;
                    if (e.Y > pictureBox1.Height)
                        graph.Tops[IndexSelectTops].position.Y = pictureBox1.Height - R / 2 - (int)PenForSelectedTop.Width;
                    if (e.X < R / 2)
                        graph.Tops[IndexSelectTops].position.X = R / 2;
                    if (e.Y < R / 2)
                        graph.Tops[IndexSelectTops].position.Y = R / 2;
                    if (e.X + R / 2 < pictureBox1.Width && e.X > R / 2)
                        graph.Tops[IndexSelectTops].position.X = e.X;
                    if (e.Y + R / 2 < pictureBox1.Height && e.Y > R / 2)
                        graph.Tops[IndexSelectTops].position.Y = e.Y;
                }
                pictureBox1.Refresh();
                Invalidate();

            }
            if (NameButtonMouse == "Right" && graph.Tops.Count > 0)
            {
                pictureBox1.Refresh();
                Point Start = new Point();
                Start.X = graph.Tops[IndexSelectTops].position.X;
                Start.Y = graph.Tops[IndexSelectTops].position.Y;
                Mouse.X = e.X;
                Mouse.Y = e.Y;
                for (int i = 0; i < graph.Tops.Count; i++)
                    if (Math.Abs(e.X - graph.Tops[i].position.X) < R && Math.Abs(e.Y - graph.Tops[i].position.Y) < R)
                    {
                        Mouse.X = graph.Tops[i].position.X;
                        Mouse.Y = graph.Tops[i].position.Y;
                        break;
                    }
                Invalidate();
                gr.DrawLine(new Pen(Brushes.Black), Start, Mouse);
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Replace = false;
            bool incert = false;
            int newCost = 0;
            if (NameButtonMouse == "Right" && graph.Tops.Count > 0)
            {
                String Cost = textBox1.Text;
                bool check = true;
                for (int i = 0; i < Cost.Length; i++)
                    if (Cost[i] < '0' || Cost[i] > '9')
                    {
                        check = false;
                        textBox1.Text = "1";
                        break;
                    }
                if (check == false)
                    newCost = 1;
                else
                    newCost = Convert.ToInt32(Cost);
                for (int i = 0; i < graph.Tops.Count; i++)
                    if (Math.Abs(e.X - graph.Tops[i].position.X) < R && Math.Abs(e.Y - graph.Tops[i].position.Y) < R)
                    {
                        if (tabControl1.SelectedIndex == 2) // ЛР3
                        {
                            newCost = (int)Math.Sqrt(Math.Pow(graph.Tops[IndexSelectTops].position.X - graph.Tops[i].position.X, 2)
                                        + Math.Pow(graph.Tops[IndexSelectTops].position.Y - graph.Tops[i].position.Y, 2));
                        }
                        graph.InsertRib(graph.Tops[IndexSelectTops], graph.Tops[i], newCost);
                        graph.matrix[IndexSelectTops][i] = graph.Ribs.Last().cost;
                        if (checkBox1.Checked)
                        {
                            graph.InsertRib(graph.Tops[i], graph.Tops[IndexSelectTops], newCost);
                            graph.matrix[i][IndexSelectTops] = graph.Ribs.Last().cost;
                        }
                        IndexSelectTops = i;
                        graph.SelectRib(graph.Ribs.Count - 1);
                        IndexSelectRibs = graph.Ribs.Count - 1;
                        incert = true;
                        ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
                        break;
                    }
            }
            ResetListBox();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            if (incert)
                listBox2.SelectedIndex = graph.Ribs.Count - 1;
            graph.SelectTop(IndexSelectTops);
            if (graph.Tops.Count > 0)
                listBox1.SelectedIndex = IndexSelectTops;
            if (IsAded)
            {
                listBox1.SelectedIndex = IndexSelectTops = listBox1.Items.Count - 1;
                graph.SelectTop(IndexSelectTops);
            }
            if (graph.Tops.Count > 0)
                graph.SelectTop(IndexSelectTops);
            pictureBox1.Refresh();
            NameButtonMouse = "";
            if (graph.Ribs.Count == 0)
                checkBox1.Enabled = true;
            else
                checkBox1.Enabled = false;
            if (listBox1.Items.Count == 0)
                button7.Enabled = false;
            else
                button7.Enabled = true;
            Invalidate();
        }
        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (graph.Tops.Count == 1)
                IndexSelectTops = 0;
            IndexSelectTops = listBox1.SelectedIndex;
            graph.SelectTop(IndexSelectTops);
            pictureBox1.Refresh();
            Invalidate();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int Num = listBox1.SelectedIndex;
            if (listBox1.SelectedIndex >= 0)
            {
                graph.RemoveTop(graph.Tops[listBox1.SelectedIndex]);
                if (IndexSelectTops != 0)
                    IndexSelectTops--;
                else
                    IndexSelectTops++;
                if (listBox2.Items.Count > 0)
                    listBox2.SelectedIndex = 0;
                if (IndexSelectRibs > 0)
                    IndexSelectRibs--;
                else
                    IndexSelectRibs = 0;
                ResetListBox();
                if (listBox1.Items.Count > 0)
                    listBox1.SelectedIndex = 0;
                IndexSelectTops = listBox1.SelectedIndex;
                graph.SelectTop(IndexSelectTops);
                for (int i = 0; i < graph.Ribs.Count; i++)
                    if (graph.Ribs[i].selected)
                    {
                        listBox2.SelectedIndex = IndexSelectRibs = i;
                        break;
                    }
                if (graph.Ribs.Count == 1)
                {
                    listBox2.SelectedIndex = 0;
                    graph.SelectRib(0);
                }
                if (Num == graph.Tops.Count && graph.Tops.Count > 0)
                    Num--;
                if (Num < graph.Tops.Count)
                {
                    graph.SelectTop(Num);
                    listBox1.SelectedIndex = Num;
                    IndexSelectRibs = Num;
                }
                if (listBox2.SelectedIndex == -1)
                {
                    listBox2.SelectedIndex = IndexSelectRibs = graph.Ribs.Count - 1;
                    graph.SelectRib(graph.Ribs.Count - 1);
                }
                ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
                pictureBox1.Refresh();
                Invalidate();
                if (numericUpDown1.Value > 0)
                    numericUpDown1.Value--;
            }
            if (graph.Ribs.Count == 0)
                checkBox1.Enabled = true;
            else
                checkBox1.Enabled = false;
            // переопределение степеней вершины
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                graph.Tops[i].Power = 0;
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    rib temp = graph.ReturnRibs(i, j);
                    if (temp != null)
                        graph.Tops[i].Power++;
                }
            }
        }

        private void listBox2_MouseClick(object sender, MouseEventArgs e)
        {
            graph.SelectRib(listBox2.SelectedIndex);
            pictureBox1.Refresh();
            Invalidate();
            IndexSelectRibs = listBox2.SelectedIndex;
            textBox3.Text = graph.Ribs[listBox2.SelectedIndex].cost.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
                return;
            graph.matrix[graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].first)][graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].second)] = MAX;
            graph.RemoveRibs(listBox2.SelectedIndex);
            IndexSelectRibs = 0;
            ResetListBox();
            for (int i = 0; i < graph.Ribs.Count; i++)
                if (graph.Ribs[i].selected)
                {
                    listBox2.SelectedIndex = IndexSelectRibs = i;
                    break;
                }
            if (graph.Ribs.Count == 1)
            {
                listBox2.SelectedIndex = 0;
                graph.SelectRib(0);
            }
            pictureBox1.Refresh();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            Invalidate();
            if (graph.Ribs.Count == 0)
                checkBox1.Enabled = true;
            else
                checkBox1.Enabled = false;
            // переопределение степеней вершины
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                graph.Tops[i].Power = 0;
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    rib temp = graph.ReturnRibs(i, j);
                    if (temp != null)
                        graph.Tops[i].Power++;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) ////////////////////////////////////////////////
        {
            Draw(e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IndexSelectRibs = IndexSelectTops = 0;
            graph.Ribs.Clear();
            graph.Tops.Clear();
            ResetListBox();
            listBox3.Items.Clear();
            pictureBox1.Refresh();
            graph.matrix.Clear();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            checkBox1.Enabled = true;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            int Cycle = (min(pictureBox1.Height, pictureBox1.Width) / 2) - R;
            // чтение из файла
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            String filename = openFileDialog1.FileName;
            FileStream file = new FileStream(filename, FileMode.Open); //создаем файловый поток
            StreamReader reader = new StreamReader(file); // создаем «потоковый читатель» и связываем его с файловым потоком 
            String StrInfile = reader.ReadToEnd();
            reader.Close(); //закрываем поток
            String[] words = StrInfile.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int countNum = words.Length;
            sqrtCount = (int)(Math.Sqrt(countNum));
            if (sqrtCount * sqrtCount != countNum)
            {
                MessageBox.Show("Ошибка с количеством чисел в матрице");
                return;
            }
            if (countNum == 0)
            {
                MessageBox.Show("Матрицы в файле не обнаружено");
                return;
            }
            graph.matrix.Clear();
            // Добавить  в элеметны в матрицу
            for (int i = 0; i < sqrtCount; i++)
            {
                graph.matrix.Add(new List<int>());
                for (int j = 0; j < sqrtCount; j++)
                    graph.matrix[i].Add(0);
            }
            //////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                for (int i = 0; i < sqrtCount; i++)
                    for (int j = 0; j < sqrtCount; j++)
                    {
                        if (words[i * sqrtCount + j] != "-")
                            graph.matrix[i][j] = Convert.ToInt32(words[i * sqrtCount + j]);
                        else
                            graph.matrix[i][j] = MAX;
                    }
            }
            catch
            {
                MessageBox.Show("Что-то не так с файлом. Возможно, там есть недопустимые символы");
                return;
            }
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            for (int i = 0; i < sqrtCount; i++)
            {
                dataGridView1.Columns.Add("", (i + 1).ToString());
                dataGridView1.Columns[i].Width = 40;
            }
            // заполнение таблицы 
            dataGridView1.Rows.Add(sqrtCount);
            for (int i = 0; i < sqrtCount; i++)
                for (int j = 0; j < sqrtCount; j++)
                {
                    if (graph.matrix[i][j] != MAX)
                        dataGridView1.Rows[i].Cells[j].Value = 1;
                    else
                        dataGridView1.Rows[i].Cells[j].Value = "-";
                    if (tabControl1.SelectedIndex == 1)
                        dataGridView1.Rows[i].Cells[j].Value = graph.matrix[i][j];
                    if (tabControl1.SelectedIndex == 1 && graph.matrix[i][j] == MAX)
                        dataGridView1.Rows[i].Cells[j].Value = "-";
                }
            // очистим граф
            IndexSelectRibs = IndexSelectTops = 0;
            graph.Ribs.Clear();
            graph.Tops.Clear();
            pictureBox1.Refresh();
            //  
            //Random rnd = new Random();
            for (int i = 0; i < sqrtCount + 1; i++)
            {
                Point newTop = new Point();

                double x = Cycle * Math.Cos((Math.PI / 180) * ((360 / sqrtCount) * i));
                double y = Cycle * Math.Sin((Math.PI / 180) * ((360 / sqrtCount) * i));
                newTop.X = (int)x + (pictureBox1.Width / 2);
                newTop.Y = (int)(y * (-1)) + (pictureBox1.Height / 2);
                graph.InsertTop(newTop);
            }
            graph.Tops.Remove(graph.Tops[sqrtCount]);
            for (int i = 0; i < sqrtCount; i++)
                for (int j = 0; j < sqrtCount; j++)
                {
                    if (graph.matrix[i][j] != MAX)
                    {
                        graph.InsertRib(graph.Tops[i], graph.Tops[j], graph.matrix[i][j]);
                    }
                }
            ResetListBox();
            //Invalidate();
            Draw();
            button5.Enabled = true;
            bool check = true;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].HeaderText = (i + 1).ToString();
                dataGridView1.Columns[i].HeaderCell.Style.BackColor = Color.LightGreen;
                dataGridView1.Rows[i].HeaderCell.Style.BackColor = Color.LightGreen;
            }
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    if (dataGridView1.Rows[i].Cells[j].Value.ToString() != dataGridView1.Rows[j].Cells[i].Value.ToString())
                    {
                        check = false;
                        break;
                    }
            checkBox1.Checked = check;
            if (graph.Ribs.Count == 0)
                checkBox1.Enabled = true;
            else
                checkBox1.Enabled = false;
            // переопределение степеней вершины
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                graph.Tops[i].Power = 0;
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    rib temp = graph.ReturnRibs(i, j);
                    if (temp != null)
                        graph.Tops[i].Power++;
                }
            }
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = (e.Row.Index + 1).ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int end = dataGridView1.CurrentCell.ColumnIndex + 1;
            int start = dataGridView1.CurrentCell.RowIndex + 1;
            listBox3.Items.Clear();
            int counter = CountingWays(start, end);// запускаем код
            label5.Text = "Результат: " + counter.ToString();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetListBox();
            pictureBox1.Refresh();
            Draw();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            if (graph.Tops.Count > 0)
                listBox1.SelectedIndex = IndexSelectTops;
            if (tabControl1.SelectedIndex == 1)
                textBox1.Enabled = true;
            if (tabControl1.SelectedIndex == 0)
                textBox1.Enabled = false;
            if (listBox1.Items.Count == 0)
                button7.Enabled = false;
            else
                button7.Enabled = true;
            if (tabControl1.SelectedIndex != 0)
            {
                button16.Enabled = true;
                textBox3.Enabled = true;
            }
            else
            {
                button16.Enabled = false;
                textBox3.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            String filename = saveFileDialog1.FileName;
            StreamWriter writer = new StreamWriter(filename, false);
            for (int i = 0; i < graph.matrix.Count; i++)
            {
                for (int j = 0; j < graph.matrix.Count; j++)
                    if (graph.matrix[i][j] != MAX)
                        writer.Write(graph.matrix[i][j].ToString() + " ");
                    else
                        writer.Write("- ");
                writer.Write("\n");
            }
            writer.Close();
        }

        /////////////////////////////////////////////// Вспомогательные методы

        public void Draw(PaintEventArgs e)
        {
            Pen PenForSelectedRibs = new Pen(Color.DarkOrange, 8);
            Pen PenForSimpleRibs = new Pen(Brushes.DarkGray, 6);
            List<Point> CorrectPoint;
            int[] indexS = new int[graph.Ribs.Count];
            int count = 0;
            if (tabControl1.SelectedIndex == 4)
            {
                e.Graphics.DrawLine(new Pen(Color.Black, 1), pictureBox1.Width / 2, 0, pictureBox1.Width / 2, pictureBox1.Height);
            }
            if (tabControl1.SelectedIndex == 2)
                for (int i = 0; i < graph.Tops.Count; i++)
                    for (int j = 0; j < graph.Tops.Count; j++)
                        if (i != j)
                        {
                            bool check = false;
                            for (int h = 0; h < graph.Ribs.Count; h++)
                            {
                                if (graph.Ribs[h].first == graph.Tops[i] && graph.Ribs[h].second == graph.Tops[j])
                                    check = true;
                            }
                            if (!check)
                                graph.matrix[i][j] = MAX;
                            else

                                graph.matrix[i][j] = (int)Math.Sqrt(Math.Pow(graph.Tops[i].position.X - graph.Tops[j].position.X, 2)
                                    + Math.Pow(graph.Tops[i].position.Y - graph.Tops[j].position.Y, 2));
                            for (int k = 0; k < graph.Ribs.Count; k++)
                            {
                                if ((graph.Ribs[k].first == graph.Tops[i] && graph.Ribs[k].second == graph.Tops[j]) ||
                                    (graph.Ribs[k].second == graph.Tops[i] && graph.Ribs[k].first == graph.Tops[j]))
                                    graph.Ribs[k].cost = graph.matrix[i][j];
                            }
                        }
            if (!checkBox1.Checked)
            {
                PenForSelectedRibs.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                PenForSimpleRibs.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            }
            for (int i = 0; i < graph.Ribs.Count; i++)
            {
                CorrectPoint = new List<Point>();
                CorrectPoint = CorrectingLine(graph.Ribs[i].first.position, graph.Ribs[i].second.position);
                if (!graph.Ribs[i].selected)
                {
                    if (graph.Ribs[i].first.position != graph.Ribs[i].second.position)
                        e.Graphics.DrawLine(PenForSimpleRibs, CorrectPoint[0], CorrectPoint[1]);
                    else
                        e.Graphics.DrawEllipse(PenForSimpleRibs, graph.Ribs[i].first.position.X - (R - 4), graph.Ribs[i].first.position.Y - (R - 4), R + 4, R + 4);
                }
                else
                {
                    if (graph.Ribs[i].first.position != graph.Ribs[i].second.position)
                    {
                        indexS[count] = i;
                        count++;
                        e.Graphics.DrawLine(PenForSelectedRibs, CorrectPoint[0], CorrectPoint[1]);
                    }
                    else
                        e.Graphics.DrawEllipse(PenForSelectedRibs, graph.Ribs[i].first.position.X - (R - 4), graph.Ribs[i].first.position.Y - (R - 4), R + 4, R + 4);
                }
                if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 6)
                {
                    Point textPos = new Point
                        (Math.Abs(graph.Ribs[i].first.position.X + graph.Ribs[i].second.position.X) / 2 - 3,
                        Math.Abs(graph.Ribs[i].first.position.Y + graph.Ribs[i].second.position.Y) / 2 - 5);
                    e.Graphics.DrawString(
                        graph.matrix[graph.Tops.IndexOf(graph.Ribs[i].first)][graph.Tops.IndexOf(graph.Ribs[i].second)].ToString()
                        , new Font("Arial", 11), Brushes.Black, textPos, Sf);

                }
            }
            if (indexS.Length > 0)
                for (int i = 0; i < count; i++)
                {
                    CorrectPoint = new List<Point>();
                    CorrectPoint = CorrectingLine(graph.Ribs[indexS[i]].first.position, graph.Ribs[indexS[i]].second.position);
                    e.Graphics.DrawLine(PenForSelectedRibs, CorrectPoint[0], CorrectPoint[1]);
                }
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                if (!graph.Tops[i].selected)
                    e.Graphics.DrawEllipse(PenForSimpleTops, graph.Tops[i].position.X - R / 2, graph.Tops[i].position.Y - R / 2, R, R);
                if (graph.Tops[i].selected)
                    e.Graphics.DrawEllipse(PenForSelectedTop, graph.Tops[i].position.X - R / 2, graph.Tops[i].position.Y - R / 2, R, R);
                Point TextPosition = new Point();
                if (i + 1 < 10)
                {
                    TextPosition.X = graph.Tops[i].position.X - R / 4 - 1;
                    TextPosition.Y = graph.Tops[i].position.Y - R / 4 - 3;
                }
                else
                {
                    TextPosition.X = graph.Tops[i].position.X - R / 4 - 5;
                    TextPosition.Y = graph.Tops[i].position.Y - R / 4 - 3;
                }
                e.Graphics.DrawString((i + 1).ToString(), new Font("Arial", 11), Brushes.Black, TextPosition, Sf);
            }
            for (int i = 0; i < graph.Ribs.Count; i++)
            {
                if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 6)
                {
                    Point textPos = new Point
                    (Math.Abs(graph.Ribs[i].first.position.X + graph.Ribs[i].second.position.X) / 2 - 3,
                    Math.Abs(graph.Ribs[i].first.position.Y + graph.Ribs[i].second.position.Y) / 2 - 5);
                    e.Graphics.DrawString(
                        graph.matrix[graph.Tops.IndexOf(graph.Ribs[i].first)][graph.Tops.IndexOf(graph.Ribs[i].second)].ToString()
                        , new Font("Arial", 11), Brushes.Black, textPos, Sf);
                }
            }
        }

        public void Draw()
        {
            Graphics g = pictureBox1.CreateGraphics();
            Pen PenForSelectedRibs = new Pen(Color.DarkOrange, 8);
            Pen PenForSimpleRibs = new Pen(Brushes.DarkGray, 6);
            List<Point> CorrectPoint;
            int[] indexS = new int[graph.Ribs.Count];
            int count = 0;
            if (tabControl1.SelectedIndex == 2)
                for (int i = 0; i < graph.Tops.Count; i++)
                    for (int j = 0; j < graph.Tops.Count; j++)
                        if (i != j)
                        {
                            graph.matrix[i][j] = (int)Math.Sqrt(Math.Pow(graph.Tops[i].position.X - graph.Tops[j].position.X, 2)
                                + Math.Pow(graph.Tops[i].position.Y - graph.Tops[j].position.Y, 2));
                        }
            if (!checkBox1.Checked)
            {
                PenForSelectedRibs.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                PenForSimpleRibs.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            }
            for (int i = 0; i < graph.Ribs.Count; i++)
            {
                CorrectPoint = new List<Point>();
                CorrectPoint = CorrectingLine(graph.Ribs[i].first.position, graph.Ribs[i].second.position);
                if (!graph.Ribs[i].selected)
                {
                    if (graph.Ribs[i].first.position != graph.Ribs[i].second.position)
                        g.DrawLine(PenForSimpleRibs, CorrectPoint[0], CorrectPoint[1]);
                    else
                        g.DrawEllipse(PenForSimpleRibs, graph.Ribs[i].first.position.X - (R - 4), graph.Ribs[i].first.position.Y - (R - 4), R + 4, R + 4);
                }
                else
                {
                    if (graph.Ribs[i].first.position != graph.Ribs[i].second.position)
                    {
                        indexS[count] = i;
                        count++;
                        g.DrawLine(PenForSelectedRibs, CorrectPoint[0], CorrectPoint[1]);
                    }
                    else
                        g.DrawEllipse(PenForSelectedRibs, graph.Ribs[i].first.position.X - (R - 4), graph.Ribs[i].first.position.Y - (R - 4), R + 4, R + 4);
                }

            }
            if (indexS.Length > 0)
                for (int i = 0; i < count; i++)
                {
                    CorrectPoint = new List<Point>();
                    CorrectPoint = CorrectingLine(graph.Ribs[indexS[i]].first.position, graph.Ribs[indexS[i]].second.position);
                    g.DrawLine(PenForSelectedRibs, CorrectPoint[0], CorrectPoint[1]);
                }

            for (int i = 0; i < graph.Tops.Count; i++)
            {
                if (!graph.Tops[i].selected)
                    g.DrawEllipse(PenForSimpleTops, graph.Tops[i].position.X - R / 2, graph.Tops[i].position.Y - R / 2, R, R);
                if (graph.Tops[i].selected)
                    g.DrawEllipse(PenForSelectedTop, graph.Tops[i].position.X - R / 2, graph.Tops[i].position.Y - R / 2, R, R);
                Point TextPosition = new Point();
                if (i + 1 < 10)
                {
                    TextPosition.X = graph.Tops[i].position.X - R / 4 - 1;
                    TextPosition.Y = graph.Tops[i].position.Y - R / 4 - 3;
                }
                else
                {
                    TextPosition.X = graph.Tops[i].position.X - R / 4 - 5;
                    TextPosition.Y = graph.Tops[i].position.Y - R / 4 - 3;
                }
                g.DrawString((i + 1).ToString(), new Font("Arial", 11), Brushes.Black, TextPosition, Sf);
            }
            for (int i = 0; i < graph.Ribs.Count; i++)
            {
                if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 6)
                {
                    Point textPos = new Point
                    (Math.Abs(graph.Ribs[i].first.position.X + graph.Ribs[i].second.position.X) / 2 - 3,
                    Math.Abs(graph.Ribs[i].first.position.Y + graph.Ribs[i].second.position.Y) / 2 - 5);
                    g.DrawString(
                        graph.matrix[graph.Tops.IndexOf(graph.Ribs[i].first)][graph.Tops.IndexOf(graph.Ribs[i].second)].ToString()
                        , new Font("Arial", 11), Brushes.Black, textPos, Sf);
                }
            }
        }
        public void ResizeTableAndFilling(DataGridView table, List<top> Tops, List<List<int>> matrix)
        {
            if (table.RowCount < Tops.Count)
            {
                if (table.RowCount > 0)
                    for (int i = table.RowCount - 1; i < Tops.Count; i++)
                    {
                        table.Columns.Add("", (i + 1).ToString());
                        table.Rows.Add();
                        table.Columns[i].Width = 40;
                    }
                else
                {
                    for (int i = 0; i < Tops.Count; i++)
                    {
                        table.Columns.Add("", (i + 1).ToString());
                        table.Rows.Add();
                        table.Columns[i].Width = 40;
                    }
                }
            }
            else
            {
                table.Columns.Add("", (1).ToString());
                table.Rows.Add();
                table.Columns[0].Width = 40;
            }
            while (table.RowCount != graph.Tops.Count && table.RowCount > 0)
            {
                table.Rows.RemoveAt(table.RowCount - 1);
                table.Columns.RemoveAt(table.ColumnCount - 1);
            }
            for (int i = 0; i < Tops.Count; i++)
                for (int j = 0; j < Tops.Count; j++)
                {
                    if (tabControl1.SelectedIndex != 0)
                    {
                        if (matrix[i][j] != MAX)
                            table.Rows[i].Cells[j].Value = matrix[i][j];
                        else
                            table.Rows[i].Cells[j].Value = "-";
                    }
                    else
                    {
                        if (matrix[i][j] != MAX)
                            table.Rows[i].Cells[j].Value = 1;
                        else
                            table.Rows[i].Cells[j].Value = "-";
                    }
                }
            for (int i = 0; i < table.ColumnCount; i++)
            {
                table.Columns[i].HeaderText = (i + 1).ToString();
                dataGridView1.Columns[i].HeaderCell.Style.BackColor = Color.LightGreen;
                dataGridView1.Rows[i].HeaderCell.Style.BackColor = Color.LightGreen;
            }
        }

        public void ResetListBox()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                comboBox1.Items.Add(i + 1);
                comboBox2.Items.Add(i + 1);
            }
            for (int i = 0; i < graph.Tops.Count; i++)
                listBox1.Items.Add((i + 1).ToString() + ")   " + graph.Tops[i].ToString());
            for (int i = 0; i < graph.Ribs.Count; i++)
            {
                int f = graph.Tops.IndexOf(graph.Ribs[i].first) + 1;
                int s = graph.Tops.IndexOf(graph.Ribs[i].second) + 1;
                if (tabControl1.SelectedIndex == 0)
                    listBox2.Items.Add((i + 1).ToString() + ")   (" + f.ToString() + ") -> (" + s.ToString() + ")");
                else
                    listBox2.Items.Add((i + 1).ToString() + ")   (" + f.ToString() + ") -> (" + s.ToString() + "); Вес: " + graph.Ribs[i].cost.ToString());
            }
            if (listBox2.Items.Count > 0 && IndexSelectRibs < listBox2.Items.Count)
                listBox2.SelectedIndex = IndexSelectRibs;
            if (listBox1.Items.Count == 0)
                button5.Enabled = false;
            else
                button5.Enabled = true;
        }
        ///////////////////////////////////////////////
        public List<Point> CorrectingLine(Point First, Point Second)
        {
            int x1, y1, x2, y2;
            double deltaY, deltaX, lenght;
            x1 = First.X;
            y1 = First.Y;
            x2 = Second.X;
            y2 = Second.Y;
            lenght = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            deltaX = R / 2 * (Math.Abs(x2 - x1) / lenght);
            deltaY = R / 2 * Math.Sin(Math.Acos(Math.Abs(x2 - x1) / lenght));
            if (x1 < x2)
            {
                x1 += (int)deltaX;
                x2 -= (int)deltaX;
            }
            else
            {
                x1 -= (int)deltaX;
                x2 += (int)deltaX;
            }

            if (y1 < y2)
            {
                y1 += (int)deltaY;
                y2 -= (int)deltaY;
            }
            else
            {
                y1 -= (int)deltaY;
                y2 += (int)deltaY;
            }
            List<Point> a = new List<Point>();
            a.Add(new Point(x1, y1));
            a.Add(new Point(x2, y2));
            return a;
        }

        public int min(int one, int two)
        {
            if (one < two)
                return one;
            else
                return two;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            /////////////////////////////
            /*int end = dataGridView1.CurrentCell.ColumnIndex;  // конечная вершина
            int start = dataGridView1.CurrentCell.RowIndex;  // стартовая вершина
            top[] mass = new top[graph.Tops.Count]; // массив вершин
            for (int i = 0; i < mass.Length; i++) // цикл по всем вершинам в массиве
            {
                if (i != start) // если веришна не стартовая, то 
                    mass[i] = null; // null
                else // иначе  
                    mass[i] = graph.Tops[i]; // присвоить веришне вершину из листа вершин
            }
            int[] dist = new int[graph.Tops.Count]; // масссив расстояний
            for (int i = 0; i < graph.Tops.Count; i++) // по умолчанию расстоянии соответсвуют тому числу
                dist[i] = MAX; // который обозначает, что пути нет
            dist[start] = 0; // расстояние от стартовой вершины до нее же делаем равным 0
            bool[] IsVisited = new bool[graph.Tops.Count]; // массив посещенных вершин
            int min_dist = 0; // число, хранящее минимальное расстояние
            int min_vertex = start; // индексом минимальной вершины делаем иддекс начальной
            while (min_dist < MAX) // цикл, пока минимальное расстояние - максимально
            {
                int i = min_vertex; // i равно индексу минимальной вершине
                IsVisited[i] = true; // текущая вершина становится посещенной
                for (int j = 0; j < graph.Tops.Count; ++j) // цикл по всем вершинам
                    if (dist[i] + graph.matrix[i][j] < dist[j]) // если дистанция до текущей вершины + длина ребра меньше до j-й вершины
                    {
                        dist[j] = dist[i] + graph.matrix[i][j]; // исправить расстояние.
                        mass[j] = graph.Tops[i];
                    }
                min_dist = MAX; // теперь минимальное расстояние равно Max
                for (int j = 0; j < graph.Tops.Count; ++j) // цикл по всем вершинам
                    if (!IsVisited[j] && dist[j] < min_dist) // если вершина j непосещенная и расстояние от начальной до j-й меньше минимального
                    {
                        min_dist = dist[j]; // минимальное растояние теперь равно расстоянию от стартовой вершины до j-й
                        min_vertex = j; // и индекс минимальной вершины равен j
                    }
            }
            /////////////////////////////
            String str = String.Empty;
            if (dist[end] != MAX)
            {
                str = "Результат:" + dist[end] + " - это длина \n";
                str += (start + 1) + "->";

                /* if (start >  end)
                     for (int i = end; mass[i] != graph.Tops[start]; i++)
                         str += (graph.Tops.IndexOf(mass[i]) + 1) + " -> ";
                 else
                     for (int i = end; mass[i] != graph.Tops[start]; i--)
                         str += (graph.Tops.IndexOf(mass[i]) + 1) + " -> ";
                List<int> mylist = new List<int>();
                int i = end;         
                while(i != start)
                {
                    mylist.Add(i + 1);
                    for (int j = 0; j < graph.Tops.Count; j++)
                        if (graph.Tops[j] == mass[i])
                        {
                            i = j;
                            break;
                        }
                }
                for (int j = 0; j < mylist.Count; j++)
                    if (j + 1 != mylist.Count)
                        str += mylist[mylist.Count - j -1] + "->";
                else
                        str += mylist[mylist.Count - j - 1];
                str += " - это путь ";
                label7.Text = str;
            }
            else
                label7.Text = "Результат: путь не найден";
            */
            // найти кратчайший путь
            int end = dataGridView1.CurrentCell.ColumnIndex + 1;
            int start = dataGridView1.CurrentCell.RowIndex + 1;
            CountingWays(start, end);// запускаем код, он найдет все пути 
            int[] lenWays = new int[Ways.Count]; // создадим массив для хранения длин путей
            for (int i = 0; i < Ways.Count; i++) // проходимся по всем путям
            {
                int sum = 0; // переменная для суммирования
                for (int j = 0; j < Ways[i].Length - 1; j++) // проходимся по всем вершинам, входящие в путь
                    sum += graph.matrix[Ways[i][j] - 1][Ways[i][j + 1] - 1]; // подсчет длины
                lenWays[i] = sum; // длина пути найдена
            }
            if (Ways.Count > 0) // если был найден хотя бы один путь
            {
                int minlen = lenWays[0]; // храним минимальный
                for (int i = 1; i < Ways.Count; i++) // выбираем из всех путей путь минимальной длины
                    if (minlen > lenWays[i])
                        minlen = lenWays[i];
                label7.Text = "Результат: " + minlen + " - это длина\n"; // минимальная длина найдена
                // далее выведем путь на экран
                String str = String.Empty;
                int indexShort = 0;
                for (int i = 0; i < Ways.Count; i++)
                    if (lenWays[i] == minlen)
                    {
                        indexShort = i;
                        for (int j = 0; j < Ways[i].Length; j++)
                        {
                            str += Ways[i][j].ToString();
                            if (j != Ways[i].Length - 1)
                                str += " -> ";
                        }
                        break;
                    }
                label7.Text += str + " - это путь"; // найден минимальный путь и его длина. Задача решена.
                for (int i = 0; i < graph.Ribs.Count; i++)
                    graph.Ribs[i].selected = false;

                for (int i = 0; i < Ways[indexShort].Length - 1; i++)
                    for (int j = 0; j < graph.Ribs.Count; j++)
                        if (graph.Tops.IndexOf(graph.Ribs[j].first) == Ways[indexShort][i] - 1 && graph.Tops.IndexOf(graph.Ribs[j].second) == Ways[indexShort][i + 1] - 1)
                            graph.Ribs[j].selected = true;
            }
            else
                label7.Text = "Результат: путей не найдено";
            pictureBox1.Refresh();
            Ways.Clear();
            Draw();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            ///////////////////////////////////Найти остов ЛР3
            if (graph.Ribs.Count == 0) // если нет ребер
                return; //выход           
            int summ = 0; // для нахождения суммарной длинны
            label9.Text = "Список ребер: ";
            listBox4.Items.Clear(); // очистить список ребер перед очередным выполнением.
            if (graph.Ribs.Count > 0)
            {
                graph.SelectRib(graph.Ribs.Count - 1);
                graph.Ribs[graph.Ribs.Count - 1].selected = false;
            }
            if (graph.Tops.Count == 0) // если нет вершин
                return;
            /////////////////////////////// 
            List<bool> IsIncluded = new List<bool>(graph.Tops.Count); // для определения, входит ли вершина в остов
            List<int> min_e = new List<int>(); // min_e[i] хранит вес наименьшего допустимого ребра из вершины i;
            List<int> sel_e = new List<int>(); // sel_e[i] содержит конец этого наименьшего ребра (это нужно для вывода рёбер).
            for (int i = 0; i < graph.Tops.Count; i++) // цикл по всем вершинам
            {
                min_e.Add(MAX); // заполнить массив min_e числом MAX, означающим, что ребра нет
                sel_e.Add(-1); // заполнить массив sel_e числом -1
                IsIncluded.Add(false); // заполнимть массив IsIncluded значениями false
            }
            min_e[0] = 0; // для вершины с номером 1 min_e = 0
            for (int i = 0; i < graph.Tops.Count; ++i) // цикл по всем вершинам
            {
                int v = -1; // переменная для обработки вершины
                for (int j = 0; j < graph.Tops.Count; ++j) // цикл по всем вершинам
                    if (!IsIncluded[j] && (v == -1 || min_e[j] < min_e[v])) // если вершина j не входит в остов и 
                        // (вес наименьшего допустимого ребра из вершины j меньше веса наименьшего допустимого ребра из вершины v )
                        v = j; // тогда вершина v = j
                if (min_e[v] == MAX) // если вес наименьшего допустимого ребра из вершины v равно числу, означающему бесконечность
                {
                    MessageBox.Show("Ошибка. Есть изолированная вершина"); // это говорит о том, что вершина изолирована, и остов составить невозможно
                    if (graph.Ribs.Count > 0) // это для отмены выделения
                    {
                        graph.SelectRib(graph.Ribs.Count - 1);
                        graph.Ribs[graph.Ribs.Count - 1].selected = false;
                    }
                    return; // выход, ведь остов составить невозможно
                }
                IsIncluded[v] = true; // вершина v становится входящей в остов
                if (sel_e[v] != -1) // если конец наименьшего допустимого ребра из вершины v найден;
                {
                    listBox4.Items.Add((v + 1) + " <-> " + (sel_e[v] + 1)); // добавить вершину в листбокс
                    // организуем выделение ребер
                    for (int k = 0; k < graph.Ribs.Count; k++)
                        if (graph.Ribs[k].first == graph.Tops[v] && graph.Ribs[k].second == graph.Tops[sel_e[v]])
                        {
                            graph.Ribs[k].selected = true;
                            summ += graph.Ribs[k].cost;
                            break;
                        }
                }
                for (int j = 0; j < graph.Tops.Count; ++j) // цикл по всем вершинам
                    if (graph.matrix[v][j] < min_e[j]) // Если вес ребра между вершинами v и j меньше веса наименьшего ребра из вершины j;
                    {
                        min_e[j] = graph.matrix[v][j]; // вес наименьшего  ребра из вершины j = вес ребра между вершинами v и j 
                        sel_e[j] = v; // конец  наименьшего ребра из вершины j = v
                    }
            }
            // для вывода результата и прорисовки остова
            label9.Text += summ + " - это суммарная длина входящих ребер";
            pictureBox1.Refresh();
            Draw();
        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = true;
            graph.Tops.Clear();
            graph.Ribs.Clear();
            graph.matrix.Clear();
            ResetListBox();
            pictureBox1.Refresh();
            // граф очищен
            sqrtCount = (int)numericUpDown1.Value;
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                int Cycle = (min(pictureBox1.Height, pictureBox1.Width) / 2) - R;
                Point newTop = new Point();
                double x = Cycle * Math.Cos((Math.PI / 180) * ((360 / sqrtCount) * i));
                double y = Cycle * Math.Sin((Math.PI / 180) * ((360 / sqrtCount) * i));
                newTop.X = (int)x + (pictureBox1.Width / 2);
                newTop.Y = (int)(y * (-1)) + (pictureBox1.Height / 2);
                graph.InsertTop(newTop);
            }
            for (int i = 0; i < sqrtCount; i++)
            {
                graph.matrix.Add(new List<int>());
                for (int j = 0; j < sqrtCount; j++)
                    graph.matrix[i].Add(MAX);
            }
            for (int i = 0; i < graph.Tops.Count; i++)
                for (int j = 0; j < graph.Tops.Count; j++)
                    if (i != j)
                    {
                        graph.matrix[i][j] = (int)Math.Sqrt(Math.Pow(graph.Tops[i].position.X - graph.Tops[j].position.X, 2)
                            + Math.Pow(graph.Tops[i].position.Y - graph.Tops[j].position.Y, 2));
                        graph.InsertRib(graph.Tops[i], graph.Tops[j],
                            (int)Math.Sqrt(Math.Pow(graph.Tops[i].position.X - graph.Tops[j].position.X, 2)
                            + Math.Pow(graph.Tops[i].position.Y - graph.Tops[j].position.Y, 2)));
                    }

            ResetListBox();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            Draw();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            graph.Ribs.Clear();
            for (int i = 0; i < graph.Tops.Count; i++)
                for (int j = 0; j < graph.Tops.Count; j++)
                    graph.matrix[i][j] = MAX;
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            ResetListBox();
            checkBox1.Enabled = true;
            pictureBox1.Refresh();
            Draw();
            // переопределение степеней вершины
            for (int i = 0; i < graph.Tops.Count; i++)
            {
                graph.Tops[i].Power = 0;
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    rib temp = graph.ReturnRibs(i, j);
                    if (temp != null)
                        graph.Tops[i].Power++;
                }
            }
        }


        private void button10_Click(object sender, EventArgs e)
        {
            // Домашняя работа №1. Поиск шарниров     

        }

        bool AdmissionToAlgorithm()
        {
            bool[] usd = new bool[graph.Tops.Count];
            int cnt = 0;
            for (int i = 0; i < graph.Tops.Count; ++i)
                if (!usd[i])
                {
                    ++cnt;
                    usd[i] = true;
                    for (int j = 0; j < graph.Tops.Count; ++j)
                    {
                        if (graph.matrix[i][j] != MAX && !usd[j])
                            usd[j] = true;
                    }
                }
            if (cnt > 1)
                return false;
            return true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // построить маршрут   
            if (checkBox1.Checked == false)
                return;
            Way[,] matrixWays = new Way[graph.Tops.Count, graph.Tops.Count]; // матрица для хранения результата алгоритма Флойда
            int[,] path = new int[graph.Tops.Count, graph.Tops.Count]; // Матрица для восстановления пути
            for (int i = 0; i < graph.Tops.Count; i++) // инициализация массивов
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    matrixWays[i, j] = new Way();
                    matrixWays[i, j].cost = graph.matrix[i][j];
                    if (i == j)
                        matrixWays[i, j].cost = 0;
                }
            for (int i = 0; i < graph.Tops.Count; ++i) // инициализация массива path
                for (int j = 0; j < graph.Tops.Count; ++j)
                    if (graph.matrix[i][j] != MAX || i == j)
                        path[i, j] = j;
                    else
                        path[i, j] = graph.Tops.Count;
            // изначально path состоит из чисел 0, 1, 2, 3, ..., N-1, 0 на каждой строке.
            // Алгоритм Флойда
            for (int k = 0; k < graph.Tops.Count; ++k)
                for (int u = 0; u < graph.Tops.Count; ++u)
                    if (matrixWays[u, k].cost != MAX)
                        for (int v = 0; v < graph.Tops.Count; ++v)
                            if (matrixWays[u, v].cost > matrixWays[u, k].cost + matrixWays[k, v].cost)
                            {
                                matrixWays[u, v].cost = matrixWays[u, k].cost + matrixWays[k, v].cost;
                                path[u, v] = path[u, k];
                            }
            for (int i = 0; i < graph.Tops.Count; i++) // теперь надо вставить пути в каждую ячейку матрицы
                for (int j = 0; j < graph.Tops.Count; j++)
                    IncertWay(i, j, matrixWays[i, j].KeeperWay, path); // вставка пути   
            // Матрица создана, подгоровительная работа закончена.     
            List<int> PotentialCurTop = new List<int>(); // список  вершин, с которых надо начинать.
            for (int counter = 0; counter < graph.Tops.Count; counter++) // цикл по всем вершинам
                PotentialCurTop.Add(counter); // добавить  в список вершину
            Way[] MainWay = new Way[PotentialCurTop.Count]; // массив для хранения полученных путей 
            for (int i = 0; i < PotentialCurTop.Count; i++) // инициализируем массив
                MainWay[i] = new Way();
            for (int Counter = 0; Counter < graph.Tops.Count; Counter++) //  цикл по всем потенциальным начальным вершинам
            {
                if (graph.Ribs.Count > 0) // если есть ребра
                {
                    graph.SelectRib(0); // делаем их непосещенными
                    graph.Ribs[0].selected = false;
                }
                int currentTop = Counter;
                while (!IsSelectedAllRibs()) // цикл пока все ребра непосещены
                {
                    int MinTotalCostWayInCurrentTop = MAX; // минимальное расстояние от текущей вершины до ближайшей, которая инцедентна непройденному рербу
                    int PotentialNextCurrentTop = -1; // возможная следующая вершина 
                    for (int i = 0; i < graph.Ribs.Count; i++) // цикл по всем ребрам 
                        if (graph.Ribs[i].selected == false) // если ребро не посещено
                        {
                            int IndexFirst = graph.Tops.IndexOf(graph.Ribs[i].first); // Индекс первой вершины - это начало ребра 
                            int IndexSecond = graph.Tops.IndexOf(graph.Ribs[i].second); // индекс второй вершины  - это конец ребра
                            if (MinTotalCostWayInCurrentTop > matrixWays[currentTop, IndexFirst].cost) // если минимальное расстояние больше расстоляния до первой вершины
                            {
                                PotentialNextCurrentTop = IndexFirst; // первая вершина становится возможной следующей
                                MinTotalCostWayInCurrentTop = matrixWays[currentTop, IndexFirst].cost; // минимальное расстояние становится равным расстоянию до первой вершины ребра
                            }
                            if (MinTotalCostWayInCurrentTop > matrixWays[currentTop, IndexSecond].cost) // аналогичнные действия, если минимальное расстояние больше рассбояния до второй вершины
                            {
                                PotentialNextCurrentTop = IndexSecond;
                                MinTotalCostWayInCurrentTop = matrixWays[currentTop, IndexSecond].cost;
                            }
                        }
                    if (MinTotalCostWayInCurrentTop == MAX) // если минимальное расстояние до ближайшей вершины, через которую проходит непосещенное ребро равно бесконечности 
                    {
                        MessageBox.Show("Есть недостижимая вершина"); // вывести ошибку
                        listBox5.Items.Clear();
                        label10.Text = "Суммарная длина: -";
                        return; // и закончинть работу.            
                    }
                    MainWay[Counter].cost += MinTotalCostWayInCurrentTop; // к суммарной длине пути добавляется кратчайшее растояние от текущей до следующей текущей
                    for (int i = 0; i < matrixWays[currentTop, PotentialNextCurrentTop].KeeperWay.Count; i++) // цикл по всем ребрам, что содерхатся в пути от текущей до следующей текущей
                    {
                        matrixWays[currentTop, PotentialNextCurrentTop].KeeperWay[i].selected = true; // ребро становится почещеннфм
                        MainWay[Counter].KeeperWay.Add(matrixWays[currentTop, PotentialNextCurrentTop].KeeperWay[i]); // оно добавляется в главный маршрут
                        rib temp = matrixWays[currentTop, PotentialNextCurrentTop].KeeperWay[i]; // и обратное ребро тоже делаем  
                        graph.ReturnRibs(graph.Tops.IndexOf(temp.second), graph.Tops.IndexOf(temp.first)).selected = true; // посещенным
                    }
                    currentTop = PotentialNextCurrentTop; // текущая вершина равна следующей текущей
                    for (int i = 0; i < graph.Ribs.Count; i++) // цикл по всем ребрам
                        if (graph.ReturnRibs(currentTop, i) != null) // если есть ребро между текущей вершиной и вершиной i
                            if (graph.ReturnRibs(currentTop, i).selected == false) // и если оно непосещенное
                            {
                                graph.ReturnRibs(currentTop, i).selected = true; // посетить его
                                MainWay[Counter].KeeperWay.Add(graph.ReturnRibs(currentTop, i)); // добавить его в главный маршрут
                                MainWay[Counter].cost += graph.ReturnRibs(currentTop, i).cost; // и в общей длине его учесть
                                graph.ReturnRibs(i, currentTop).selected = true; // и обратное ребро тоже сделать посещенным
                                currentTop = i; // текущая вершина становится концом посещетого только что ребра
                                break; // и выход из цикла
                            }
                }
            }
            // Все, осталось вывести путь
            // В MainWay есть все пути от потенциальных вершин. Среди них надо просто найти самый короткий и вывести его.
            listBox5.Items.Clear();
            int minlen = MAX;
            for (int i = 0; i < MainWay.Length; i++)
                if (minlen > MainWay[i].cost)
                    minlen = MainWay[i].cost;
            RezultOfFourLR.Clear();
            for (int i = 0; i < MainWay.Length; i++)
                if (minlen == MainWay[i].cost)
                {
                    for (int j = 0; j < MainWay[i].KeeperWay.Count; j++)
                    {
                        listBox5.Items.Add((graph.Tops.IndexOf(MainWay[i].KeeperWay[j].first) + 1)
                                   + " -> " + (graph.Tops.IndexOf(MainWay[i].KeeperWay[j].second) + 1));
                        RezultOfFourLR.Add(MainWay[i].KeeperWay[j]);
                    }
                    break;
                }
            label10.Text = "Суммарная длина:" + minlen;
            graph.SelectRib(0);
            graph.Ribs[0].selected = false;
            pictureBox1.Refresh();
            Draw();
            timer1.Enabled = true;
        }

        public bool IsSelectedAllRibs()
        {
            for (int i = 0; i < graph.Ribs.Count; i++)
                if (graph.Ribs[i].selected == false)
                    return false;
            return true;
        }

        void IncertWay(int start, int finish, List<rib> Way, int[,] path)
        {
            // вставка пути из вершины start в finish
            if (path[start, finish] == graph.Tops.Count)
                return;
            int currentTop = start;
            while (currentTop != finish)
            {
                int temp = currentTop;
                currentTop = path[currentTop, finish];
                Way.Add(graph.ReturnRibs(temp, currentTop));
            }
        }
        int CountingWays(int IndexStart, int IndexFinish) // подсчет путей. Индексы отчитываются с 1.
        {
            int count = 0; // в эту переменную запишется количество путей
            KeeperWays.Push(IndexStart); // ложим стартовую вершину в стек
            for (int i = 0; i < graph.Tops.Count; i++) // прохожим по всем вершинам
            {
                if (graph.matrix[IndexStart - 1][i] != MAX) // если в матрице найдена связь от стартовой вершине к i-й и i не равно стартовой вершине
                {
                    KeeperWays.Push(i + 1); // ложим i-ю вершину в стек
                    recursive_Obhod(i + 1, IndexFinish, ref count); // запускаем рекурсивный обход 
                }
            }
            KeeperWays.Clear(); // очистить стек
            return count; // вернуть результат
        }
        int CountingWays(int IndexStart, int IndexFinish, Graph UseGraph) // подсчет путей. Индексы отчитываются с 1.
        {
            int count = 0; // в эту переменную запишется количество путей
            KeeperWays.Push(IndexStart); // ложим стартовую вершину в стек
            for (int i = 0; i < UseGraph.Tops.Count; i++) // прохожим по всем вершинам
            {
                if (UseGraph.matrix[IndexStart - 1][i] != MAX) // если в матрице найдена связь от стартовой вершине к i-й и i не равно стартовой вершине
                {
                    KeeperWays.Push(i + 1); // ложим i-ю вершину в стек
                    recursive_Obhod(i + 1, IndexFinish, ref count, UseGraph); // запускаем рекурсивный обход 
                }
            }
            KeeperWays.Clear(); // очистить стек
            return count; // вернуть результат
        }

        void recursive_Obhod(int CurrentTop, int IndexFinish, ref int count)
        {
            if (CurrentTop == IndexFinish)
            {   // путь найден, он находится в стеке KeeperWays
                // требуется развернуть содержимое стека
                String str = String.Empty;
                int[] mas = new int[KeeperWays.Count];  // массив для записи последовательности вершин
                int k = 0; // вспомогательная переменная
                foreach (int j in KeeperWays) // циклом записываем числа из стека в массив
                {
                    mas[KeeperWays.Count - 1 - k] = j;
                    k++;
                }
                // в mas теперь хранится путь в правильном порядке
                for (int i = 0; i < KeeperWays.Count; i++) // составим строку с путем для вывода в listbox
                {
                    str += mas[i].ToString();
                    if (i + 1 != KeeperWays.Count)
                        str += " -> ";
                }
                if (tabControl1.SelectedIndex == 0)
                    listBox3.Items.Add(str); // добавим путь в листбокс
                if (tabControl1.SelectedIndex == 1)
                    Ways.Add(mas); // добавить путь в двусвязный список
                KeeperWays.Pop(); // удалить элемент из стека
                count++; // увеличить счетчик на 1;
                return;
            } // если текущий узел не совпадает с конечным
            bool check = false; // переменная для контроля вершин
            for (int i = 0; i < graph.Tops.Count; i++) // циклом проходимся по всем вершинам
            {
                foreach (int j in KeeperWays) // проходимся по всем вершинам из стека
                    if (j == i + 1) // если нашли вершину которая уже есть в стеке
                    {
                        check = true;
                        break;
                    }
                if (check == true) // если нашли повторяющуюся вершину
                    check = false; // для следующей итерации
                else // если нет повторений
                if (graph.matrix[CurrentTop - 1][i] != MAX) // если найдено ребро от CurrentTop  к i - й вершине
                {
                    KeeperWays.Push(i + 1); // занести ребро в стек
                    recursive_Obhod(i + 1, IndexFinish, ref count); // запустить рекурсивный обход
                }
            }
            KeeperWays.Pop();
            return;
        }

        void recursive_Obhod(int CurrentTop, int IndexFinish, ref int count, Graph UseGraph)
        {
            if (CurrentTop == IndexFinish)
            {   // путь найден, он находится в стеке KeeperWays
                // требуется развернуть содержимое стека
                String str = String.Empty;
                int[] mas = new int[KeeperWays.Count];  // массив для записи последовательности вершин
                int k = 0; // вспомогательная переменная
                foreach (int j in KeeperWays) // циклом записываем числа из стека в массив
                {
                    mas[KeeperWays.Count - 1 - k] = j;
                    k++;
                }
                // в mas теперь хранится путь в правильном порядке
                for (int i = 0; i < KeeperWays.Count; i++) // составим строку с путем для вывода в listbox
                {
                    str += mas[i].ToString();
                    if (i + 1 != KeeperWays.Count)
                        str += " -> ";
                }
                if (tabControl1.SelectedIndex == 0)
                    listBox3.Items.Add(str); // добавим путь в листбокс
                Ways.Add(mas); // добавить путь в двусвязный список
                KeeperWays.Pop(); // удалить элемент из стека
                count++; // увеличить счетчик на 1;
                return;
            } // если текущий узел не совпадает с конечным
            bool check = false; // переменная для контроля вершин
            for (int i = 0; i < UseGraph.Tops.Count; i++) // циклом проходимся по всем вершинам
            {
                foreach (int j in KeeperWays) // проходимся по всем вершинам из стека
                    if (j == i + 1) // если нашли вершину которая уже есть в стеке
                    {
                        check = true;
                        break;
                    }
                if (check == true) // если нашли повторяющуюся вершину
                    check = false; // для следующей итерации
                else // если нет повторений
                if (UseGraph.matrix[CurrentTop - 1][i] != MAX) // если найдено ребро от CurrentTop  к i - й вершине
                {
                    KeeperWays.Push(i + 1); // занести ребро в стек
                    recursive_Obhod(i + 1, IndexFinish, ref count, UseGraph); // запустить рекурсивный обход
                }
            }
            KeeperWays.Pop();
            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            tabControl1.SelectedIndex = 6;
            textBox1.Enabled = true;

        }
        private void DrawWay(object sender, EventArgs e) // дичайшее рисование пути. Для 4 лр
        {
            Pen pen = new Pen(Color.Black, 7);
            Pen pen1 = new Pen(Color.Red, 10);
            Graphics g = pictureBox1.CreateGraphics();
            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            List<Point> CorrectPoint = new List<Point>();
            Point delta = new Point(RezultOfFourLR[curTopFourLR].second.position.X - RezultOfFourLR[curTopFourLR].first.position.X,
                RezultOfFourLR[curTopFourLR].second.position.Y - RezultOfFourLR[curTopFourLR].first.position.Y);
            Point markerPosition = new Point(RezultOfFourLR[curTopFourLR].first.position.X + (int)(delta.X * LoadingProsent * 0.01),
                RezultOfFourLR[curTopFourLR].first.position.Y + (int)(delta.Y * LoadingProsent * 0.01));
            CorrectPoint = CorrectingLine(RezultOfFourLR[curTopFourLR].first.position, markerPosition);
            LoadingProsent += 5;
            pictureBox1.Refresh();
            //Invalidate();
            g.DrawEllipse(pen1, markerPosition.X - R / 6, markerPosition.Y - R / 6, R / 3, R / 3);
            g.DrawLine(pen, CorrectPoint[0], CorrectPoint[1]);
            if (curTopFourLR == RezultOfFourLR.Count - 1 && LoadingProsent > 100)
            {
                timer1.Enabled = false;
                RezultOfFourLR[curTopFourLR].selected = true;
                pictureBox1.Refresh();
                Invalidate();
                curTopFourLR = 0;
            }
            if (LoadingProsent > 100)
            {
                LoadingProsent = 0;
                RezultOfFourLR[curTopFourLR].selected = true;
                curTopFourLR++;
            }
            if (timer1.Enabled == false)
            {
                curTopFourLR = 0;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (listBox6.SelectedIndex >= 0)
            {
                try
                {
                    int num = Convert.ToInt32(textBox2.Text);
                    listBox6.Items[listBox6.SelectedIndex] = "Переводчик №" + (listBox6.SelectedIndex + 1) + " З/П - " + textBox2.Text;
                    textBox2.Text = "";
                }
                catch
                {
                    textBox2.Text = "";
                    MessageBox.Show("Недопустимое значение");
                    return;
                }
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            graph.Tops.Clear();
            graph.Ribs.Clear();
            graph.matrix.Clear();
            // граф очищен
            sqrtCount = (int)(numericUpDown2.Value + numericUpDown3.Value);
            int delta = pictureBox1.Height - R;
            for (int i = 0; i < numericUpDown2.Value; i++)
            {
                Point newTop = new Point();
                newTop.Y = R + (int)(delta / numericUpDown2.Value) * i;
                newTop.X = pictureBox1.Width / 4;
                graph.InsertTop(newTop);
            }
            for (int i = 0; i < sqrtCount; i++)
            {
                graph.matrix.Add(new List<int>());
                for (int j = 0; j < sqrtCount; j++)
                    graph.matrix[i].Add(MAX);
            }
            ResetListBox();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            pictureBox1.Refresh();
            Draw();
            if (numericUpDown2.Value > listBox6.Items.Count)
                listBox6.Items.Add("Переводчик №" + numericUpDown2.Value + " З/П - ?");
            if (numericUpDown2.Value < listBox6.Items.Count)
                listBox6.Items.RemoveAt(listBox6.Items.Count - 1);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown2_ValueChanged(sender, e);
            int delta = pictureBox1.Height - R;
            for (int i = 0; i < numericUpDown3.Value; i++)
            {
                Point newTop = new Point();
                newTop.Y = R + (int)(delta / numericUpDown3.Value) * i;
                newTop.X = pictureBox1.Width / 4 * 3;
                graph.InsertTop(newTop);
            }
            for (int i = (int)numericUpDown2.Value; i < sqrtCount; i++)
            {
                graph.matrix.Add(new List<int>());
                for (int j = (int)numericUpDown2.Value; j < sqrtCount; j++)
                    graph.matrix[i].Add(MAX);
            }
            ResetListBox();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            pictureBox1.Refresh();
            Draw();
        }

        bool NextSet(ref int[] NumericArray, int NumofElements, int size) // дает комбинацию
        {
            for (int i = size - 1; i >= 0; --i)
                if (NumericArray[i] < NumofElements - size + i + 1)
                {
                    ++NumericArray[i];
                    for (int j = i + 1; j < size; ++j)
                        NumericArray[j] = NumericArray[j - 1] + 1;
                    return true;
                }
            return false;
        }

        void SaveCombination(ref int[] NumericArray, int size) // ПП для сохранения сочетания в список
        {
            int[] Mass = new int[size]; // объявляем массив, который занесем в список
            for (int i = 0; i < size; i++) // цикл, заполняющий массив
                Mass[i] = NumericArray[i];
            // В Mass комбинация
            RezFiveLR.Add(Mass); // заносим в список полученную комбинацию       
        }

        private void button13_Click(object sender, EventArgs e)
        {
            int[] PayingWork = new int[(int)numericUpDown2.Value]; // массив с зарплатами
            List<int> Rez = new List<int>(); //массив для хранения индексов сочетаний, прошедших условие
            for (int i = 0; i < numericUpDown2.Value; i++) // цикл для получения зарплат
            {
                String str = listBox6.Items[i].ToString();
                String[] words = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    PayingWork[i] = Convert.ToInt32(words[words.Length - 1]);
                }
                catch
                {
                    MessageBox.Show("Не все значения есть ");
                    return;
                }
            }
            for (int SizeСombination = 1; SizeСombination <= numericUpDown2.Value; SizeСombination++) // цикл по количествам элементов в сочетании
            {
                int[] NumericArray = new int[(int)numericUpDown2.Value]; // в этом массиве храним числа 
                for (int i = 0; i < (int)numericUpDown2.Value; i++) // заполняем массив
                    NumericArray[i] = i + 1;
                SaveCombination(ref NumericArray, SizeСombination); // сохраняем комбинацию
                if ((int)numericUpDown2.Value >= SizeСombination) // если количество переводчиков >= размер комбинации 
                {
                    while (NextSet(ref NumericArray, (int)numericUpDown2.Value, SizeСombination)) // пока NextSet возвращает истину
                        SaveCombination(ref NumericArray, SizeСombination); // сохранить комбинацию
                }
            }
            for (int i = 0; i < RezFiveLR.Count; i++) // проходимя по всем комбинациям
            {
                bool[] check = new bool[(int)numericUpDown3.Value]; // для проверки главного условия 
                for (int j = 0; j < RezFiveLR[i].Length; j++) // проходим по всем переводчикам
                {
                    for (int k = (int)numericUpDown2.Value; k < graph.Tops.Count; k++) // и отмечаем все владеемыми ими языками
                        if (graph.ReturnRibs(RezFiveLR[i][j] - 1, k) != null)
                            check[k - (int)numericUpDown2.Value] = true;
                }
                for (int j = 0; j < check.Length; j++) // проходимся по всем сочетаниям
                {
                    if (check[j] == false) // если нашелся "невладеемый" язык
                        break; // сочетание не подходит
                    if (j + 1 == check.Length) // если же переводчики владеют всеми языками
                    {
                        // Ура! Найдена комбинация
                        Rez.Add(i); // заносим в список
                    }
                }
            }
            // В Rez теперь есть все потенциальные "сочетания переводчиков"
            if (Rez.Count == 0) // если не было найдено ни одно подходящее сочетание
            {
                MessageBox.Show("Решение не найдено");
                listBox7.Items.Clear();
                label13.Text = "Суммарная З/П: -";
                return;
            }
            int IndexRez = 0; // индекс результата в массиве Rez
            int RezCost = MAX; // суммарная зраплата
            for (int i = 0; i < Rez.Count; i++) // цикл по всем переводчикам в сочетании
            {
                int cost = 0; // сумарная зп
                for (int j = 0; j < RezFiveLR[Rez[i]].Length; j++) // подсчитываем зп
                    cost += PayingWork[RezFiveLR[Rez[i]][j] - 1];
                if (cost < RezCost) // находим минимальную зп и индекс сочетания, чтобы его вывести
                {
                    IndexRez = i;
                    RezCost = cost;
                }
            }
            listBox7.Items.Clear(); // очищаем листбокс
            label13.Text = "Суммарная З/П: " + RezCost; // выводим сумарную зп
            for (int i = 0; i < RezFiveLR[Rez[IndexRez]].Length; i++) // выводим всех переводчиков, которые наймем на работу           
                listBox7.Items.Add("Переводчик №" + RezFiveLR[Rez[IndexRez]][i]);
        }

        private void button14_Click(object sender, EventArgs e)
        {

            List<List<int>> MainSep = new List<List<int>>(); // здесь храним группы вершин, которые будут окрашены в один цвет. 
            Graph WorkGraph = new Graph(graph); // рабочий граф, копия основного
            bool check = true; // переменная для окончания цикла нахождения групп
            while (check) // основной цикл
            {
                List<int> temp = new List<int>(); // здесь зраним максимальное независимое множество
                temp = MaxSeparate(WorkGraph); // получаем максиальное назевисимое множество
                // удалить все ребра, инцедентные вершинам из temp
                for (int i = 0; i < temp.Count; i++)
                    for (int j = 0; j < WorkGraph.Ribs.Count; j++)
                        if (WorkGraph.Ribs[j].first == WorkGraph.Tops[temp[i]] || WorkGraph.Ribs[j].second == WorkGraph.Tops[temp[i]])
                        {
                            WorkGraph.RemoveRibs(j);
                            j--;
                        }
                for (int i = 0; i < MainSep.Count; i++) // это вспомогательный цикл, устранение копий из temp, это вызвано спецификой определения номера вершины
                    for (int j = 0; j < MainSep[i].Count; j++)
                        for (int k = 0; k < temp.Count; k++)
                            if (MainSep[i][j] == temp[k])
                            {
                                temp.RemoveAt(k);
                                k--;
                            }
                if (temp.Count == 0) // если очередное независимое множество не было найдено 
                    check = false; // пора закончить цикл
                if (temp.Count != 0) // если найдено максимальное независимое множество
                    MainSep.Add(temp); // добавить его в группу для раскраски
            }
            // теперь надо каждую группу раскрасить в собственный цвет, а количество этих групп -  есть ответ на задачу.
            Random r = new Random();
            if (graph.Ribs.Count > 0)
            {
                graph.SelectRib(0);
                graph.Ribs[0].selected = false;
            }
            if (graph.Tops.Count > 0)
            {
                graph.SelectTop(0);
                graph.Tops[0].selected = false;
            }
            ///////////////////////////////////////////
            SolidBrush[] Colors = new SolidBrush[10];
            Colors[0] = new SolidBrush(Color.DarkOrange);
            Colors[1] = new SolidBrush(Color.Aqua);
            Colors[2] = new SolidBrush(Color.Red);
            Colors[3] = new SolidBrush(Color.DarkViolet);
            Colors[4] = new SolidBrush(Color.Yellow);
            Colors[5] = new SolidBrush(Color.Green);
            Colors[6] = new SolidBrush(Color.LightBlue);
            Colors[7] = new SolidBrush(Color.White);
            Colors[8] = new SolidBrush(Color.Brown);
            Colors[9] = new SolidBrush(Color.Firebrick);
            //////////////////////////////////////////
            pictureBox1.Refresh();
            Graphics g = pictureBox1.CreateGraphics();
            for (int j = 0; j < MainSep.Count; j++)
            {
                SolidBrush br = new SolidBrush(Color.FromArgb(r.Next(255), r.Next(255), r.Next(255), r.Next(255)));
                for (int i = 0; i < MainSep[j].Count; i++)
                {
                    if (j > 9)
                        g.FillEllipse(br, graph.Tops[MainSep[j][i]].position.X - R / 2, graph.Tops[MainSep[j][i]].position.Y - R / 2, R, R);
                    else
                        g.FillEllipse(Colors[j], graph.Tops[MainSep[j][i]].position.X - R / 2, graph.Tops[MainSep[j][i]].position.Y - R / 2, R, R);
                }
            }
            Draw();
            label14.Text = "Хроматическое число: " + MainSep.Count;
        }
        public List<int> MinCover(Graph graph1) // минимальное вершинное покрытие
        {
            List<int> rezult = new List<int>();
            int[] powers = new int[graph1.Tops.Count];
            for (int i = 0; i < graph1.Tops.Count; i++)
                powers[i] = graph1.Tops[i].Power;
            Graph WorkGraph = new Graph(graph1);
            while (WorkGraph.Ribs.Count != 0)
            {
                int CurrTop = WorkGraph.ReturnIndexMaxPowerTop();
                List<int> potentialTop = new List<int>();
                for (int i = 0; i < WorkGraph.Tops.Count; i++)
                    if (WorkGraph.Tops[i].Power == WorkGraph.ReturnMaxPower())
                        potentialTop.Add(i);
                for (int i = 0; i < potentialTop.Count; i++)
                    if (powers[CurrTop] > powers[potentialTop[i]])
                        CurrTop = potentialTop[i];
                rezult.Add(CurrTop);
                for (int i = 0; i < WorkGraph.Tops.Count; i++)
                    powers[i] = WorkGraph.Tops[i].Power;
                for (int i = 0; i < WorkGraph.Ribs.Count; i++)
                    if (WorkGraph.Ribs[i].first == WorkGraph.Tops[CurrTop] || WorkGraph.Ribs[i].second == WorkGraph.Tops[CurrTop])
                    {
                        WorkGraph.RemoveRibs(i);
                        i--;
                    }
            }
            rezult.Sort();
            return rezult;
        }
        public List<int> MaxSeparate(Graph graph1) // макс независ множество
        {
            List<int> temp = new List<int>();

            List<int> rez = new List<int>();
            temp = MinCover(graph1);
            for (int i = 0; i < graph1.Tops.Count; i++)
                if (temp.IndexOf(i) == -1)
                    rez.Add(i);
            return rez;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            // ЛР №7. Посчитать максимальный поток
            if (graph.Tops.Count > 0) // начинается все с защит от некорректного ввода
            {
                graph.SelectTop(0);
                graph.Tops[0].selected = false;
            }
            if (graph.Ribs.Count > 0)
            {
                graph.SelectRib(0);
                graph.Ribs[0].selected = false;
            }
            pictureBox1.Refresh();
            Invalidate();
            if (checkBox1.Checked == true)
                return;
            int s, t;
            try
            {
                s = Convert.ToInt32(comboBox1.Text);
                t = Convert.ToInt32(comboBox2.Text);
            }
            catch
            {
                MessageBox.Show("Ошибка при вводе истока и/или стока");
                return;
            }
            if (s > graph.Tops.Count || t > graph.Tops.Count || t == s)
            {
                MessageBox.Show("Ошибка при вводе истока и/или стока");
                return;
            }
            Graph WorkingGraph = new Graph(); // это остаточная сеть
            for (int i = 0; i < graph.Tops.Count; i++) // добавляем вершины
            {
                WorkingGraph.Tops.Add(graph.Tops[i]);
            }

            for (int i = 0; i < graph.Tops.Count; i++) // создаем матрицу весов
            {
                WorkingGraph.matrix.Add(new List<int>());
                for (int j = 0; j < graph.Tops.Count; j++)
                    WorkingGraph.matrix[i].Add(graph.matrix[i][j]);
            }

            for (int i = 0; i < graph.Ribs.Count; i++) // добавляем ребра и корректируем матрицу весов в остаточной сети
            {
                rib newR = new rib(graph.Ribs[i].second, graph.Ribs[i].first, 0);
                rib newRib = new rib(graph.Ribs[i].first, graph.Ribs[i].second, graph.Ribs[i].cost);
                WorkingGraph.Ribs.Add(newRib);
                WorkingGraph.Ribs.Add(newR);
                WorkingGraph.matrix
                    [WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[WorkingGraph.Ribs.Count - 1].first)]
                    [WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[WorkingGraph.Ribs.Count - 1].second)] = 0;
            }
            CountingWays(s, t, WorkingGraph); // ищем все пути от истока к стоку, они в Ways
            int WayIndex = 0; // индекс для навигации в путях
            int minCost; // переменная для хранения веса минимального ребра.
            int MaxFlow = 0; // предеменная для подсчета максимального потока
            while (WayIndex != Ways.Count) // цикл пока есть путь от истока к стоку через ненулевые ребра в остаточной сети
            {
                WayIndex = 0; // зануляем индекс
                List<rib> Way = new List<rib>(); // список для хранения ребер из пути.
                do
                {
                    Way.Clear(); // очистить список ребер в пути.
                    for (int i = 0; i < Ways[WayIndex].Count() - 1; i++) // из последовательности вершин делаем последовательность ребер.
                    {
                        Way.Add(WorkingGraph.ReturnRibs(Ways[WayIndex][i] - 1, Ways[WayIndex][i + 1] - 1));
                    }
                    // теперь ищем ребро минимального веса в полученной только что последовательности.
                    minCost = MAX;
                    for (int i = 0; i < Way.Count; i++)
                        if (minCost > Way[i].cost)
                            minCost = Way[i].cost;
                    if (minCost == 0) // если минимальный вес ребра оказывается равным 0, то
                    {
                        WayIndex++; // этот путь нам не подходит
                    }
                    if (WayIndex == Ways.Count)
                    {
                        break;
                    }
                } while (minCost == 0);

                if (WayIndex != Ways.Count) // если необходимый путь нашелся
                {
                    for (int i = 0; i < Way.Count; i++) // корректируем ребра: от прямых ребер вычитаем минимальный вес ребра, к обратным - добавляем
                    {
                        WorkingGraph.ReturnRibs(Way[i].first, Way[i].second).cost -= minCost;
                        WorkingGraph.ReturnRibs(Way[i].second, Way[i].first).cost += minCost;
                        WorkingGraph.matrix[WorkingGraph.Tops.IndexOf(Way[i].first)]
                            [WorkingGraph.Tops.IndexOf(Way[i].second)] -= minCost;
                        WorkingGraph.matrix[WorkingGraph.Tops.IndexOf(Way[i].second)]
                            [WorkingGraph.Tops.IndexOf(Way[i].first)] += minCost;
                    }
                    MaxFlow += minCost; // к максимальному потоку добавляем минимальный вес ребра.
                    KeeperWays.Clear();
                    Ways.Clear();
                    CountingWays(s, t, WorkingGraph);
                }
            }
            label17.Text = "Максимальный поток: " + MaxFlow.ToString();
            s--;
            t--;
            bool[] IsVisited = new bool[WorkingGraph.Tops.Count]; // массив для обхода
            DFS(s, ref WorkingGraph, ref IsVisited); // выполняем обход в глубину для нахождения разреза
            List<int> TopsAroundS = new List<int>(); // список вершин, в которые можно попасть из истока
            List<int> TopsAroundT = new List<int>(); // список  вершин, дополняющие предыдущие
            for (int i = 0; i < WorkingGraph.Tops.Count; i++) // заполняем списки
                if (IsVisited[i])
                    TopsAroundS.Add(i);
                else
                    TopsAroundT.Add(i);
            listBox8.Items.Clear();
            String str = String.Empty;
            for (int i = 0; i < TopsAroundS.Count; i++)
                str += (TopsAroundS[i] + 1).ToString() + ", ";
            listBox8.Items.Add(str);
            str = String.Empty;
            for (int i = 0; i < TopsAroundT.Count; i++)
                str += (TopsAroundT[i] + 1).ToString() + ", ";
            listBox8.Items.Add(str);


            List<rib> MinCut = new List<rib>(); // список ребер, входящих в минимальный разрез
            for (int i = 0; i < TopsAroundS.Count; i++)
            {
                for (int j = 0; j < TopsAroundT.Count; j++)
                {
                    if (graph.ReturnRibs(TopsAroundS[i], TopsAroundT[j]) != null)
                        if (graph.ReturnRibs(TopsAroundS[i], TopsAroundT[j]).cost != 0)
                        {
                            MinCut.Add(graph.ReturnRibs(TopsAroundS[i], TopsAroundT[j]));
                            graph.ReturnRibs(TopsAroundS[i], TopsAroundT[j]).selected = true;
                        }
                }
            }
            Ways.Clear();
            KeeperWays.Clear();
            // дальше ничего относящегося к алгоритму нет.

            Invalidate();
            Draw();
            Graphics g = pictureBox1.CreateGraphics();
            g.FillEllipse(new SolidBrush(Color.Aqua), graph.Tops[s].position.X - R / 2, graph.Tops[s].position.Y - R / 2, R, R);
            g.FillEllipse(new SolidBrush(Color.Aqua), graph.Tops[t].position.X - R / 2, graph.Tops[t].position.Y - R / 2, R, R);
            Point TextPosition = new Point();
            TextPosition.X = graph.Tops[s].position.X - R / 4 - 1;
            TextPosition.Y = graph.Tops[s].position.Y - R / 4 - 3;
            g.DrawString("S", new Font("Arial", 11), Brushes.Black, TextPosition, Sf);
            TextPosition.X = graph.Tops[t].position.X - R / 4 - 1;
            TextPosition.Y = graph.Tops[t].position.Y - R / 4 - 3;
            g.DrawString("T", new Font("Arial", 11), Brushes.Black, TextPosition, Sf);
        }

        void DFS(int st, ref Graph g, ref bool[] visited)
        {
            visited[st] = true;
            for (int i = 0; i < g.Tops.Count; i++)
                if ((g.matrix[st][i] != 0) && (g.matrix[st][i] != MAX) && (visited[i] == false))
                    DFS(i, ref g, ref visited);
        }


        int bfs(int start, int end, ref int[] color, ref Queue<int> queue, ref int[] pred, ref int[,] flow)
        {
            // 0 - белый 
            // 1 - серый
            // 2 - черный
            int u, v;
            for (u = 0; u < graph.Tops.Count; u++) // Сначала отмечаем все вершины не пройденными
                color[u] = 0;

            queue.Enqueue(start);      // Вступили на первую вершину
            color[start] = 1;
            pred[start] = -1;   // Специальная метка для начала пути
            while (queue.Count != 0)  // Пока хвост не совпадёт с головой
            {
                u = queue.Dequeue();       // вершина u пройдена
                color[u] = 2;
                for (v = 0; v < graph.Tops.Count; v++) // Смотрим смежные вершины
                {
                    // Если не пройдена и не заполнена
                    if (color[v] == 0 && (graph.matrix[u][v] - flow[u, v]) > 0 && graph.matrix[u][v] != MAX)
                    {
                        queue.Enqueue(v);  // Вступаем на вершину v
                        color[v] = 1;
                        pred[v] = u; // Путь обновляем
                    }
                }
            }
            if (color[end] == 2) // Если конечная вершина, дошли - возвращаем 0
                return 0;
            else return 1;
        }


        private void button16_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                try
                {
                    graph.Ribs[listBox2.SelectedIndex].cost = Convert.ToInt32(textBox3.Text);
                    graph.matrix[graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].first)]
                        [graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].second)] = Convert.ToInt32(textBox3.Text);
                    if (graph.ReturnRibs(graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].second),
                        graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].first)) != null)
                    {
                        graph.matrix[graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].second)]
                        [graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].first)] = Convert.ToInt32(textBox3.Text);
                        graph.ReturnRibs(graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].second),
                        graph.Tops.IndexOf(graph.Ribs[listBox2.SelectedIndex].first)).cost = Convert.ToInt32(textBox3.Text);
                    }
                    ResetListBox();
                    ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
                    graph.SelectRib(0);
                    graph.Ribs[0].selected = false;
                    pictureBox1.Refresh();
                    Invalidate();
                }
                catch
                {
                    MessageBox.Show("Неверная запись");
                    return;
                }
            }
        }
        public List<int> Hakimi(Way[,] d, Graph WorkingGraph)
        {
            int[] v = new int[WorkingGraph.Tops.Count];
            for (int i = 0; i < WorkingGraph.Tops.Count; i++)
                v[i] = 1;
            //1. для каждого ребра найти точку с наименьшим разделением (дискретные значения)
            int edgNumb = WorkingGraph.Ribs.Count;
            int vrtxNumb = WorkingGraph.Tops.Count;
            int[] s = new int[edgNumb]; // массив точек с наименьшим разделением (отсчет от первого индекса ребра)\
            int[] y = new int[edgNumb];
            for (int ij = 0; ij < edgNumb; ij++)
            { //цикл по всем ребрам
                s[ij] = -1;                       //изначальное значение по умолчанию
                for (int dy = 0; dy < WorkingGraph.Ribs[ij].cost; dy++)
                { //цикл смещения точки (дискретное)
                    for (int k = 0; k < vrtxNumb; k++)
                    { //цикл по вершинам для данной точки данного ребра
                        if (WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].first) != k
                            && WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].second) != k
                            && d[WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].first), k].cost != MAX
                            && d[WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].second), k].cost != MAX)
                        { //если вершина не инцидентна текущему ребру
                            int sk = v[k] * min(dy + d[WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].first), k].cost,
                                WorkingGraph.Ribs[ij].cost + d[WorkingGraph.Tops.IndexOf(WorkingGraph.Ribs[ij].second), k].cost - dy); //получить значение разделения
                            if (s[ij] == -1)
                            {                                                       //если это первая итерация для данного ребра
                                s[ij] = sk; //то инициализировать начальные значения как максимальные
                                y[ij] = dy;
                            }
                            else
                                if (sk > s[ij])
                            { //если найдено большее разделение
                                s[ij] = sk; //то сохранить информацию о нем
                                y[ij] = dy;
                            }
                        }
                    }
                }
            }
            List<int> rezult = new List<int>();
            int index = 0;
            for (int i = 1; i < edgNumb; i++)
                if (s[index] > s[i])
                    index = i;
            rezult.Add(index);
            rezult.Add(y[index]);
            return rezult;
        }
        private void button17_Click(object sender, EventArgs e)
        {
            listBox9.Items.Clear();
            if (checkBox1.Checked == false)
            {
                listBox9.Items.Add("Нужно использовать только неориентированный граф");
                return;
            }
            Way[,] matrixWays = new Way[graph.Tops.Count, graph.Tops.Count]; // матрица для хранения результата алгоритма Флойда
            int[,] path = new int[graph.Tops.Count, graph.Tops.Count]; // Матрица для восстановления пути
            for (int i = 0; i < graph.Tops.Count; i++) // инициализация массивов
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    matrixWays[i, j] = new Way();
                    matrixWays[i, j].cost = graph.matrix[i][j];
                    if (i == j)
                        matrixWays[i, j].cost = 0;
                }
            for (int i = 0; i < graph.Tops.Count; ++i) // инициализация массива path
                for (int j = 0; j < graph.Tops.Count; ++j)
                    if (graph.matrix[i][j] != MAX || i == j)
                        path[i, j] = j;
                    else
                        path[i, j] = graph.Tops.Count;
            // изначально path состоит из чисел 0, 1, 2, 3, ..., N-1, 0 на каждой строке.
            // Алгоритм Флойда
            for (int k = 0; k < graph.Tops.Count; ++k)
                for (int u = 0; u < graph.Tops.Count; ++u)
                    if (matrixWays[u, k].cost != MAX)
                        for (int v = 0; v < graph.Tops.Count; ++v)
                            if (matrixWays[u, v].cost > matrixWays[u, k].cost + matrixWays[k, v].cost)
                            {
                                matrixWays[u, v].cost = matrixWays[u, k].cost + matrixWays[k, v].cost;
                                path[u, v] = path[u, k];
                            }
            for (int i = 0; i < graph.Tops.Count; i++) // теперь надо вставить пути в каждую ячейку матрицы
                for (int j = 0; j < graph.Tops.Count; j++)
                    IncertWay(i, j, matrixWays[i, j].KeeperWay, path); // вставка пути   
            int MaxLenWay = 0;
            for (int i = 0; i < graph.Tops.Count; i++)
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    if (MaxLenWay < matrixWays[i, j].cost && matrixWays[i, j].cost == MAX)
                        MaxLenWay = matrixWays[i, j].cost;
                    if (i != j && matrixWays[i, j].cost == MAX)
                    {
                        listBox9.Items.Add("Абсолютного центра нет, граф не связный");
                        return;
                    }

                }
            double lambda = (double)(MaxLenWay) / 2; // изначальный максимальный вес пути
            List<PositionAbsCenter> centers = new List<PositionAbsCenter>();//содержит центры графа
            double delta = 0.001;//точность (для увеличения максимально допустимого расстояния)
            bool found = false;//сигнализатор того, был найден центр или нет
            while (!found)//пока не найдём центр
            {
                for (int i = 0; i < graph.Tops.Count; i++)//проходим по каждому элементу матрицы смежности
                    for (int j = 0; j < graph.Tops.Count && i > j; j++)
                        if (graph.matrix[i][j] != MAX)//если он не нулевой, то ребро существует
                        {
                            double distance = СheckWay(graph.matrix[i][j], matrixWays, graph.Tops.Count, i, j, lambda);//вычисляем расстояние нa котором котором на ребре расолагается центр
                            if (distance != -1)//центр есть
                            {
                                found = true;
                                PositionAbsCenter center = new PositionAbsCenter(i, j, distance);
                                centers.Add(center);  //заносим в список центров
                            }
                        }
                if (found)
                    break;
                lambda += delta;//увеличиваем максимально допустимую длину пути
            }
            for (int i = 0; i < centers.Count; i++)
            {
                listBox9.Items.Add("Абсолютный ценр находится на ребре " + (centers[i].first + 1).ToString()
                    + " <-> " + (centers[i].second + 1).ToString());
                listBox9.Items.Add("На расстоянии " + centers[i].offset + " от начала");
            }
            listBox9.Items.Add("Радиус равен " + lambda);



        }
        public double СheckWay(double weight, Way[,] ways, int amount, int u, int v, double lambda)
        {//проверить, есть ли абсолютный центр на ребре (u,v)
            double add = 0.01;//шаг передвижения по ребру
            double point = 0;//сдвиг по ребру (u,v)
            while (point <= weight)//Пока сдвиг не дойдёт до другого конца ребра
            {
                bool isCenter = true;//является ли точка на ребре центром
                for (int j = 0; j < amount; j++)//Просмотр путей до остальных вершин для u и v
                {
                    if (ways[u, j].cost + point <= lambda || weight - point + ways[v, j].cost <= lambda)
                        //если расстояние от точки point до j-ой вершины меньше lambda                 
                        isCenter = true;                  
                    else
                    {
                        isCenter = false;//иначе эта точка не является центром
                        break;
                    }
                }
                if (isCenter == true)//если центр найден
                    break;
                point += add;//изменяем положение точки
            }
            if (point <= weight)//если точка располагается на ребре
                return point;
            else
                return -1;
        }

        private void Button18_Click(object sender, EventArgs e)
        {
            Upped = 1;
            button19.Enabled = true;
            for (int i = 0; i < graph.Ribs.Count; i++)
                Upped *= graph.Ribs[i].cost;
            label18.Text = "Множитель: " + Upped;
        }

        private void Button19_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < graph.Ribs.Count; i++)
                graph.Ribs[i].cost = (int)(Math.Pow(graph.Ribs[i].cost, -1) * Upped);
            for (int i = 0; i < graph.Tops.Count; i++)
                for (int j = 0; j < graph.Tops.Count; j++)
                {
                    if (graph.matrix[i][j] != 0 && graph.matrix[i][j] != MAX)
                        graph.matrix[i][j] = (int)(Math.Pow(graph.matrix[i][j], -1) * Upped);

                }
            ResetListBox();
            pictureBox1.Refresh();
            Invalidate();
            Draw();
            ResizeTableAndFilling(dataGridView1, graph.Tops, graph.matrix);
            CountClick++;
            if (CountClick % 2 == 1)
            {
                button19.Text = "Восстановить граф";
                button18.Enabled = false;

                pictureBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button16.Enabled = false;
            }
            else
            {
                button19.Text = "Испортить граф";
                button18.Enabled = true;

                pictureBox1.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button16.Enabled = true;
            }
        }
    }
}

public class Way
{
    public int cost;
    public List<rib> KeeperWay = new List<rib>();
    public Way()
    {
        cost = 0;
    }
    public Way(int newCost)
    {
        cost = newCost;
    }
}
public class top
{
    public Point position;
    public int Power; // это типо степень вершины
    public bool selected;
    public top()
    {
        position = new Point();
        Power = 0;
    }

    public top(int x, int y)
    {
        position = new Point(x, y);
        Power = 0;
    }

    public top(top CopiedTop)
    {
        position.X = CopiedTop.position.X;
        position.Y = CopiedTop.position.Y;
        Power = CopiedTop.Power;
        selected = CopiedTop.selected;
    }

    public void Init(int x, int y)
    {
        position.X = x;
        position.Y = y;
        Power = 0;
    }
    public override String ToString()
    {
        String str = position.X + " ; " + position.Y;
        return str;
    }
}
public class rib
{
    public bool selected;
    public top first, second;
    public int cost;
    public rib(top Newfirst, top NewSecond, int NewCost)
    {
        first = Newfirst;
        second = NewSecond;
        cost = NewCost;
    }


}
public class Graph
{
    public List<top> Tops = new List<top>(); // это коллекция вершин
    public List<rib> Ribs = new List<rib>(); // это коллекция ребер
    public List<List<int>> matrix = new List<List<int>>(); // это матрица смежности
    private const int MAX = 9999999;
    public Graph(Graph CopiedGraph)
    {
        for (int i = 0; i < CopiedGraph.Tops.Count; i++)
        {
            InsertTop(CopiedGraph.Tops[i]);
        }
        for (int i = 0; i < CopiedGraph.Ribs.Count; i++)
            InsertRib(CopiedGraph.Ribs[i].first, CopiedGraph.Ribs[i].second, CopiedGraph.Ribs[i].cost);
        for (int i = 0; i < CopiedGraph.Tops.Count; i++)
        {
            this.matrix.Add(new List<int>());
            for (int j = 0; j < CopiedGraph.Tops.Count; j++)
            {
                int num = CopiedGraph.matrix[i][j];
                this.matrix[i].Add(num);
            }
        }
        for (int i = 0; i < this.Tops.Count; i++)
        {
            this.Tops[i].Power = 0;
            for (int j = 0; j < this.Tops.Count; j++)
            {
                rib temp = this.ReturnRibs(i, j);
                if (temp != null)
                    this.Tops[i].Power++;
            }
        }
    }
    public Graph()
    {

    }
    public void SelectRib(int index) // Выбирает ребро по индексу в Ribs.
    {
        for (int i = 0; i < Ribs.Count; i++)
            if (i != index)
                Ribs[i].selected = false;
            else
                Ribs[i].selected = true;
    }

    public void SelectTop(int index) // Выбирает вершину по индексу в Tops.
    {
        for (int i = 0; i < Tops.Count; i++)
            if (i != index)
                Tops[i].selected = false;
            else
                Tops[i].selected = true;
    }

    public void InsertTop(Point Position) // Добавляет вершину с заданной позицией.
    {
        top Newtop = new top(Position.X, Position.Y);
        Tops.Add(Newtop);

    }
    public void InsertTop(top NewTop) // Добавляет вершину с заданной позицией.
    {
        top Newtop = new top(NewTop);
        Tops.Add(Newtop);

    }

    public bool InsertRib(top first, top second, int cost) // Добавляет ребро с между вершинами first и second, весом cost c защитой от повторов
    {
        rib NewRib = new rib(first, second, cost);
        bool rez = true;
        for (int i = 0; i < Ribs.Count; i++)
            if (Ribs[i].first == NewRib.first && Ribs[i].second == NewRib.second)
            {
                rez = false;
                break;
            }
        if (rez == true)
            Ribs.Add(NewRib);
        // переопределение степеней вершины
        for (int i = 0; i < Tops.Count; i++)
        {
            Tops[i].Power = 0;
            for (int j = 0; j < Tops.Count; j++)
            {
                rib temp = ReturnRibs(i, j);
                if (temp != null)
                    Tops[i].Power++;
            }
        }

        return rez;
    }

    public void RemoveTop(top Deltop) // Удаляет вершину Deltop и все ребра с с ней связанные.
    {
        for (int i = 0; i < Ribs.Count; i++)
            if (Ribs[i].first.position == Tops[Tops.IndexOf(Deltop)].position || Ribs[i].second.position == Tops[Tops.IndexOf(Deltop)].position)
            {
                matrix[Tops.IndexOf(Ribs[i].first)][Tops.IndexOf(Ribs[i].second)] = MAX;
                matrix[Tops.IndexOf(Ribs[i].second)][Tops.IndexOf(Ribs[i].first)] = MAX;
                Ribs.RemoveAt(i);
                i--;
            }
        matrix.RemoveAt(Tops.IndexOf(Deltop));
        for (int i = 0; i < matrix.Count; i++)
            matrix[i].RemoveAt(Tops.IndexOf(Deltop));
        Tops.Remove(Deltop);
        // переопределение степеней вершины
        for (int i = 0; i < Tops.Count; i++)
        {
            Tops[i].Power = 0;
            for (int j = 0; j < Tops.Count; j++)
            {
                rib temp = ReturnRibs(i, j);
                if (temp != null)
                    Tops[i].Power++;
            }
        }
    }

    public void RemoveRibs(int index) // удаляет ребро
    {
        Ribs.RemoveAt(index);
        if (index < Ribs.Count)
            Ribs[index].selected = true;
        if (index - 1 > 0 && index == Ribs.Count)
            Ribs[index - 1].selected = true;

        // переопределение степеней вершины
        for (int i = 0; i < Tops.Count; i++)
        {
            Tops[i].Power = 0;
            for (int j = 0; j < Tops.Count; j++)
            {
                rib temp = ReturnRibs(i, j);
                if (temp != null)
                    Tops[i].Power++;
            }
        }
    }
    public rib ReturnRibs(int first, int second) // возвращает ребро (если оно есть) между вершинами first second
    {
        if (first >= Tops.Count || second >= Tops.Count)
            return null;
        for (int i = 0; i < Ribs.Count; i++)
            if (Ribs[i].first == Tops[first] && Ribs[i].second == Tops[second])
                return Ribs[i];
        return null;
    }
    public rib ReturnRibs(top first, top second) // возвращает ребро (если оно есть) между вершинами first second
    {
        for (int i = 0; i < Ribs.Count; i++)
            if (Ribs[i].first == first && Ribs[i].second == second)
                return Ribs[i];
        return null;
    }
    public rib ReturnRibs(top first, top second, int cost) // возвращает ребро (если оно есть) между вершинами first second
    {
        for (int i = 0; i < Ribs.Count; i++)
            if (Ribs[i].first == first && Ribs[i].second == second && Ribs[i].cost == cost)
                return Ribs[i];
        return null;
    }

    public int ReturnIndexMaxPowerTop()
    {
        int power = 0;
        int index = 0;
        for (int i = 0; i < Tops.Count; i++)
        {
            if (power < Tops[i].Power)
            {
                power = Tops[i].Power;
                index = i;
            }
        }
        return index;
    }
    public int ReturnIndexMinPowerTop()
    {
        int power = 0;
        int index = 0;
        for (int i = 0; i < Tops.Count; i++)
        {
            if (power > Tops[i].Power)
            {
                power = Tops[i].Power;
                index = i;
            }
        }
        return index;
    }
    public int ReturnMaxPower()
    {
        int power = 0;
        for (int i = 0; i < Tops.Count; i++)
        {
            if (power < Tops[i].Power)
            {
                power = Tops[i].Power;
            }
        }
        return power;
    }
}
public class PositionAbsCenter
{
    public int first; // это начало ребра
    public int second; // это конец ребра
    public double offset; // это смещение
    public PositionAbsCenter(int NewFirst, int NewSecond, double NewOffset)
    {
        first = NewFirst;
        second = NewSecond;
        offset = NewOffset;
    }
}