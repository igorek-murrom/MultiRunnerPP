using System;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Management;

namespace AppRunner
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        List<Process> processList = new List<Process>();
        List<Path> paths = new List<Path>();

        public Form1()
        {
            InitializeComponent();
            //openFileDialog.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";

            readX();
        }

        private void readX()
        {
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "ArrayOfPath";
            xRoot.IsNullable = true;
            using (StreamReader reader = new StreamReader("db.xml"))
            {
                paths = (List<Path>)(new XmlSerializer(typeof(List<Path>), xRoot)).Deserialize(reader);
            }
            foreach(Path pa in paths)
            {
                addApp(pa.path);
            }
        }

        private void writeX()
        {
            XmlRootAttribute xRoot = new XmlRootAttribute();
            XmlSerializer p1 = new XmlSerializer(typeof(List<Path>), xRoot);
            xRoot.ElementName = "ArrayOfPath";
            xRoot.IsNullable = true;
            using (StreamWriter writer = new StreamWriter("db.xml"))
            {
                p1.Serialize(writer, paths);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            string filename = openFileDialog.FileName;
            Path pi = new Path();
            pi.path = filename;
            paths.Add(pi);
            writeX();
            addApp(filename);
        }

        private void addApp(string filename)
        {
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            Button buttonR = new Button();
            buttonR.Text = "R";
            buttonR.Name = filename;
            buttonR.Dock = DockStyle.Top;
            buttonR.Click += dynamicClickRemove;

            Button buttonT = new Button();
            buttonT.Text = "T";
            buttonT.Name = filename;
            buttonT.Dock = DockStyle.Top;
            buttonT.Click += dynamicClickTerminate;

            Button buttonS = new Button();
            buttonS.Text = "S";
            buttonS.Name = filename;
            buttonS.Dock = DockStyle.Top;
            buttonS.Click += dynamicClickStart;

            Label label = new Label();
            label.Text = filename;
            label.TextAlign = ContentAlignment.TopCenter;
            label.Dock = DockStyle.Top;

            int cnt = processList.Count + 1;
            tableLayoutPanel1.Controls.Add(buttonR, 0, cnt);
            tableLayoutPanel1.Controls.Add(label, 1, cnt);
            tableLayoutPanel1.Controls.Add(buttonT, 2, cnt);
            tableLayoutPanel1.Controls.Add(buttonS, 3, cnt);
            tableLayoutPanel1.RowCount += 1;

            Process proc = new Process();
            proc.StartInfo.FileName = filename;
            proc.EnableRaisingEvents = true;

            processList.Add(proc);
        }

        private void dynamicClickStart(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int index = getIndex(clickedButton, processList);
            processList[index].Refresh();
            if (IsRunning(processList[index]))
            {
                MessageBox.Show("Уже запущено", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return;
            }

            try
            {
                processList[index].Start();
            }
            catch (Exception ex)
            {
                return;
            }

        }

        private void dynamicClickTerminate(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int index = getIndex(clickedButton, processList);
            try
            {
                processList[index].Kill();
            }
            catch (Exception ex)
            {

            }
        }

        private void dynamicClickRemove(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int index = getIndex(clickedButton, processList);
            try
            {
                processList[index].Kill();
            }
            catch (Exception ex)
            {

            }
            processList.RemoveAt(index);
            paths.RemoveAt(index);
            writeX();
            RemoveArbitraryRow(tableLayoutPanel1, index + 1);
        }

        public void RemoveArbitraryRow(TableLayoutPanel panel, int rowIndex)
        {
            if (rowIndex >= panel.RowCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, rowIndex);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            var removeStyle = panel.RowCount - 1;

            if (panel.RowStyles.Count > removeStyle)
                panel.RowStyles.RemoveAt(removeStyle);

            panel.RowCount--;
        }

        private static bool IsRunning(Process process)
        {
            try
            {
                Process.GetProcessById(process.Id).Dispose();
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return false;
            }
            return true;
        }

        public int getIndex(Button clickedButton, List<Process> listp)
        {
            int index = 0;

            for (int i = 0; i < listp.Count; i++)
            {
                if (listp[i].StartInfo.FileName == clickedButton.Name)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
