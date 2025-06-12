using Personal_Organizer_Last.NoteBook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Personal_Organizer_Last.SalaryCalculator
{
    public partial class SalaryCalculator : Form
    {
        string filePath;
        SalaryCalculatorControl salaryControl;
        List<string[]> salaryValues;
        string user_type;

        double minWage = 20002.5;
        double baseSalary = 40005;
        double expFactor = 0;
        double resFactor = 0;
        double academicFactor = 0;
        double engFactor = 0;
        double familyFactor = 0;
        double posFactor = 0;
        double salary;

        public SalaryCalculator(string user_type, string filePath)
        {
            InitializeComponent();
            this.filePath = filePath;
            salaryControl = new SalaryCalculatorControl(filePath);
            salaryValues = new List<string[]>();
            this.user_type = user_type;
        }

        private void SalaryCalculator_Load(object sender, EventArgs e)
        {
            readFromCSV();
            SetComboBoxValues();
            CustomizeBackButton();

        }

        private void CustomizeBackButton()
        {
            // 1) Flat görünüm, kenarlık yok
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.FlatAppearance.BorderSize = 0;

            // 2) Mor arkaplan, beyaz yazı rengi
            backButton.BackColor = Color.MediumSlateBlue;
            backButton.ForeColor = Color.White;

            // 3) Yazı tipini biraz büyütmek isterseniz
            backButton.Font = new Font(backButton.Font.FontFamily, 10, FontStyle.Regular);

            // 4) Pill–shape (yarıçap = yükseklik/2)
            int radius = backButton.Height / 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            // sol üst çeyrek
            path.AddArc(0, 0, radius, radius, 180, 90);
            // sağ üst çeyrek
            path.AddArc(backButton.Width - radius, 0, radius, radius, 270, 90);
            // sağ alt çeyrek
            path.AddArc(backButton.Width - radius, backButton.Height - radius, radius, radius, 0, 90);
            // sol alt çeyrek
            path.AddArc(0, backButton.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            backButton.Region = new Region(path);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                expFactor = 0.60;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                expFactor = 1.00;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                expFactor = 1.20;
            }
            else if (comboBox1.SelectedIndex == 3)
            {
                expFactor = 1.35;
            }
            else if (comboBox1.SelectedIndex == 4)
            {
                expFactor = 1.50;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                resFactor = 0.30;
            }
            else if (comboBox2.SelectedIndex == 1)
            {
                resFactor = 0.20;
            }
            else if (comboBox2.SelectedIndex == 2)
            {
                resFactor = 0.20;
            }
            else if (comboBox2.SelectedIndex == 3)
            {
                resFactor = 0.10;
            }
            else if (comboBox2.SelectedIndex == 4)
            {
                resFactor = 0.10;
            }
            else if (comboBox2.SelectedIndex == 5)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 6)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 7)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 8)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 9)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 10)
            {
                resFactor = 0.05;
            }
            else if (comboBox2.SelectedIndex == 11)
            {
                resFactor = 0.00;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                academicFactor = 0.30;
            }
            else if (comboBox3.SelectedIndex == 1)
            {
                academicFactor = 0.10;
            }
            else if (comboBox3.SelectedIndex == 2)
            {
                academicFactor = 0.35;
            }
            else if (comboBox3.SelectedIndex == 3)
            {
                academicFactor = 0.05;
            }
            else if (comboBox3.SelectedIndex == 4)
            {
                academicFactor = 0.15;
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
            {
                engFactor = 0.20;
            }
            else if (comboBox4.SelectedIndex == 1)
            {
                engFactor = 0.20;
            }
            else if (comboBox4.SelectedIndex == 2)
            {
                engFactor = 0.05;
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox6.SelectedIndex == 0)
            {
                familyFactor = 0.20;
            }
            else if (comboBox6.SelectedIndex == 1)
            {
                familyFactor = 0.20;
            }
            else if (comboBox6.SelectedIndex == 2)
            {
                familyFactor = 0.30;
            }
            else if (comboBox6.SelectedIndex == 3)
            {
                familyFactor = 0.40;
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex == 0)
            {
                posFactor = 0.50;
            }
            else if (comboBox5.SelectedIndex == 1)
            {
                posFactor = 0.75;
            }
            else if (comboBox5.SelectedIndex == 2)
            {
                posFactor = 0.85;
            }
            else if (comboBox6.SelectedIndex == 3)
            {
                posFactor = 1.00;
            }
            else if (comboBox5.SelectedIndex == 4)
            {
                posFactor = 0.40;
            }
            else if (comboBox6.SelectedIndex == 5)
            {
                posFactor = 0.60;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveValues();
        }

        private void saveValues()
        {
            calculateSalary();
            string[] savedValues = new string[]
            {
                comboBox1.SelectedItem?.ToString(),
                comboBox2.SelectedItem?.ToString(),
                comboBox3.SelectedItem?.ToString(),
                comboBox4.SelectedItem?.ToString(),
                comboBox5.SelectedItem?.ToString(),
                comboBox6.SelectedItem?.ToString(),
                salary.ToString()
            };
            salaryValues.Clear();
            salaryValues.Add(savedValues);
            writeToCSV();
        }

        public void writeToCSV()
        {
            if (!File.Exists(filePath))
            {

                using (StreamWriter sw = File.CreateText(filePath))
                {

                }
            }

            else
            {
                using (StreamWriter _sw = new StreamWriter(filePath))
                {

                    _sw.Write("");
                }
            }

            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                foreach (var item in salaryValues)
                {
                    string line = string.Join("$", item);
                    sw.WriteLine(line);
                }
            }
        }

        public void readFromCSV()
        {
            if (File.Exists(filePath))
            {
                salaryValues.Clear();

                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split('$');
                        salaryValues.Add(fields);
                    }
                }
            }
        }

        private void SetComboBoxValues()
        {
            if (salaryValues.Count > 0)
            {
                var savedValues = salaryValues[0]; // İlk satırdaki değerleri al

                if (savedValues.Length >= 6)
                {
                    comboBox1.SelectedItem = savedValues[0];
                    comboBox2.SelectedItem = savedValues[1];
                    comboBox3.SelectedItem = savedValues[2];
                    comboBox4.SelectedItem = savedValues[3];
                    comboBox5.SelectedItem = savedValues[4];
                    comboBox6.SelectedItem = savedValues[5];
                }
            }
        }

        private void calculateSalary()
        {
            salary = baseSalary * (expFactor + resFactor + academicFactor + engFactor + familyFactor + posFactor);
            if (Session.UserType == UserType.PartTimeUser)
            {
                salary = salary / 2;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();

            var container = this.Parent;  // UserControl'leri içeren container
            var um = container
                     .Controls
                     .OfType<Personal_Organizer_Last.UserManagement>()
                     .FirstOrDefault();

            if (um != null)
            {
                um.Show();
                um.BringToFront();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 1) Faktörleri baz alarak maaşı hesapla
            calculateSalary();

        
            label9.Text = salary.ToString("N2");

           
        }
    }
}
