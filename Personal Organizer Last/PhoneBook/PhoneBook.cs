using Personal_Organizer_Last.Reminder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Personal_Organizer_Last.PhoneBook
{
    public partial class PhoneBook : Form
    {
        PhoneBookController p;
        int counter = 1;
        string selectedRow;
        string filePath;
        string reminderFilePath;
        List<string[]> reminders;

        public PhoneBook(string filePath, string reminderFilePath)
        {
            InitializeComponent();
            // 1) Flat görünüme geçir, kenarlığı sıfırla
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.FlatAppearance.BorderSize = 0;

            // 2) Renk ve yazı rengi
            backButton.BackColor = Color.MediumSlateBlue;
            backButton.ForeColor = Color.White;

            // 3) Yazıyı mutlaka ortala
            backButton.TextAlign = ContentAlignment.MiddleCenter;

            // 4) Boyutlanınca tekrar pill‐shape uygulasın
            backButton.Resize += (s, e) => MakePill(backButton);

           
            MakePill(backButton);
           
            reminders = new List<string[]>();
            this.filePath = filePath;
            p = new PhoneBookController(filePath);
            this.reminderFilePath = reminderFilePath;
            p.ReadFromCSV();
            
        }

        private void MakePill(Button btn)
        {
            int radius = btn.Height / 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            // Sol üst
            path.AddArc(0, 0, radius, radius, 180, 90);
            // Sağ üst
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            // Sağ alt
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            // Sol alt
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
        }

        private void checkIfReminder()
        {
            if (File.Exists(reminderFilePath))
            {
                reminders.Clear();

                using (StreamReader sr = new StreamReader(reminderFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split('£');
                        reminders.Add(fields);
                    }
                }
            }
            else
            {

            }

            CheckReminders();


        }

        private void CheckReminders()
        {
            bool shouldShake = false;

            string[] _reminder = { };

            for (int i = 0; i < reminders.Count; i++)
            {
                var reminder = reminders[i];

                // Hatırlatıcı zamanını ve şuanki zamanı al
                DateTime reminderTime = DateTime.ParseExact(reminder[0], "dd/MM/yyyy HH:mm:ss", null);
                DateTime currentTime = DateTime.Now;

                // Eğer hatırlatıcı zamanı şuanki zamandan önceyse, hatırlatıcı uygun demektir
                if (reminderTime <= currentTime)
                {
                    shouldShake = true;
                    reminders.RemoveAt(i);
                    WriteReminderToCSV();
                }
                _reminder = reminder;
                break;
            }

            
            if (shouldShake)
            {
                ShakeScreen();
                shouldShake = false;
                this.Text = $"Reminder for {_reminder[2]} - Time's up!";


            }


        }

        private void ShakeScreen()
        {
            Random rand = new Random();
            int shakeIntensity = 10;

            for (int i = 0; i < 10; i++)
            {
                this.Location = new Point(this.Location.X + rand.Next(-shakeIntensity, shakeIntensity),
                                          this.Location.Y + rand.Next(-shakeIntensity, shakeIntensity));
                System.Threading.Thread.Sleep(20);
            }

        }

        public void WriteReminderToCSV()
        {

            if (!File.Exists(reminderFilePath))
            {

                using (StreamWriter sw = File.CreateText(reminderFilePath))
                {

                }
            }
            else
            {
                using (StreamWriter _sw = new StreamWriter(reminderFilePath))
                {

                    _sw.Write("");
                }
            }


            using (StreamWriter sw = new StreamWriter(reminderFilePath, true))
            {
                foreach (var record in reminders)
                {
                    string line = string.Join("£", record);
                    sw.WriteLine(line);
                }

            }
        }

        private void PhoneBook_Load(object sender, EventArgs e)
        {
            DataGridViewColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.HeaderText = "Name"; // Sütun başlığı
            dataGridView1.Columns.Add(nameColumn); // DataGridView'e sütunu ekle

            DataGridViewColumn surnameColumn = new DataGridViewTextBoxColumn();
            surnameColumn.HeaderText = "Surname";
            dataGridView1.Columns.Add(surnameColumn);

            DataGridViewColumn phoneNumberColumn = new DataGridViewTextBoxColumn();
            phoneNumberColumn.HeaderText = "Phone Number";
            dataGridView1.Columns.Add(phoneNumberColumn);

            DataGridViewColumn emailColumn = new DataGridViewTextBoxColumn();
            emailColumn.HeaderText = "Email";
            dataGridView1.Columns.Add(emailColumn);

            DataGridViewColumn addressColumn = new DataGridViewTextBoxColumn();
            addressColumn.HeaderText = "Address";
            dataGridView1.Columns.Add(addressColumn);

            DataGridViewColumn descriptionColumn = new DataGridViewTextBoxColumn();
            descriptionColumn.HeaderText = "Description";
            dataGridView1.Columns.Add(descriptionColumn);


            // Kolonları eşit şekilde yaymak
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Header stilleri
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Satır hizalama (isteğe bağlı)
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

          
            timer1.Start();

         
            this.Font = new Font("Segoe UI", 10);
  
            deleteRecord.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
            updateRecord.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
            createRecord.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
            listRecord.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
          

        }

        private void listRecord_Click(object sender, EventArgs e)
        {
            List<string[]> myRecord = p.listRecord();

            dataGridView1.Rows.Clear();

            foreach (string[] record in myRecord)
            {
                dataGridView1.Rows.Add(record[0], record[1], record[2], record[3], record[4], record[5]);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedValue = dataGridView1.SelectedRows[0];
                textBoxName.Text = selectedValue.Cells[0].Value.ToString();
                textBoxSurname.Text = selectedValue.Cells[1].Value.ToString();
                textBoxPhoneNum.Text = selectedValue.Cells[2].Value.ToString();
                textBoxEmail.Text = selectedValue.Cells[3].Value.ToString();
                textBoxAddress.Text = selectedValue.Cells[4].Value.ToString();
                textBoxDescription.Text = selectedValue.Cells[5].Value.ToString();

                selectedRow = dataGridView1.CurrentRow.Index.ToString();
            }
        }

        private void createRecord_Click(object sender, EventArgs e)
        {
            string name;
            string surname;
            string phoneNumber;
            string email;
            string address;
            string description;

            name = textBoxName.Text;
            surname = textBoxSurname.Text;
            phoneNumber = textBoxPhoneNum.Text;
            email = textBoxEmail.Text;
            address = textBoxAddress.Text;
            description = textBoxDescription.Text;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please fill the all information.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isValidPhoneNumber(phoneNumber))
            {
                bool isItOK = p.isContain(name, surname, phoneNumber, email, address, description);
                if (isItOK == true)
                {
                    p.createRecord(name, surname, phoneNumber, email, address, description);
                    p.WriteToCSV(); // Veriyi CSV dosyasına kaydet

                }
                else
                {

                    MessageBox.Show
                        ("You already have this record. Please list the new records!", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Invalid phone number format. Please enter a phone number in the format (555) 555 55 55.");
            }


        }

        private void updateRecord_Click(object sender, EventArgs e)
        {
            string name;
            string surname;
            string phoneNumber;
            string email;
            string address;
            string description;

            name = textBoxName.Text;
            surname = textBoxSurname.Text;
            phoneNumber = textBoxPhoneNum.Text;
            email = textBoxEmail.Text;
            address = textBoxAddress.Text;
            description = textBoxDescription.Text;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please choose a record to update.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            p.updateRecord(name, surname, phoneNumber, email, address, description, Convert.ToInt32(selectedRow));
            p.WriteToCSV(); // Veriyi CSV dosyasına kaydet
            List<string[]> myRecord = p.listRecord();

            dataGridView1.Rows.Clear();

            foreach (string[] record in myRecord)
            {
                dataGridView1.Rows.Add(record[0], record[1], record[2], record[3], record[4], record[5]);
            }
           
        }

        private void deleteRecord_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show
                ("Are you sure you want to delete this record?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            string name = textBoxName.Text;
            string surname = textBoxSurname.Text;
            string phoneNumber = textBoxPhoneNum.Text;
            string email = textBoxEmail.Text;
            string address = textBoxAddress.Text;
            string description = textBoxDescription.Text;

            p.deleteRecord(name, surname, phoneNumber, email, address, description);
            p.WriteToCSV(); // Veriyi CSV dosyasına kaydet

            List<string[]> myRecord = p.listRecord();
            dataGridView1.Rows.Clear();

            foreach (string[] record in myRecord)
            {
                dataGridView1.Rows.Add(record[0], record[1], record[2], record[3], record[4], record[5]);
            }
        }

        private void PhoneBook_FormClosing(object sender, FormClosingEventArgs e)
        {

          
            p.WriteToCSV();
        }

        bool isFormatting = false;

        private string FormatPhone(string digits)
        {
            string formatted = "";

            if (digits.Length >= 1) formatted += "(";
            if (digits.Length >= 3) formatted += digits.Substring(0, 3) + ") ";
            else formatted += digits;

            if (digits.Length >= 6) formatted += digits.Substring(3, 3) + " ";
            else if (digits.Length > 3) formatted += digits.Substring(3);

            if (digits.Length >= 8) formatted += digits.Substring(6, 2) + " ";
            else if (digits.Length > 6) formatted += digits.Substring(6);

            if (digits.Length > 8) formatted += digits.Substring(8);

            return formatted;
        }

        // Hangi rakama denk gelen karakterin hangi index'te olduğunu bulur
        private int CalculateCursorFromDigitIndex(int digitIndex, string formattedText)
        {
            int count = 0;
            for (int i = 0; i < formattedText.Length; i++)
            {
                if (char.IsDigit(formattedText[i]))
                    count++;

                if (count >= digitIndex)
                    return i + 1;
            }
            return formattedText.Length;
        }

        private void textBoxPhoneNum_TextChanged(object sender, EventArgs e)
        {
            if (isFormatting) return;
            isFormatting = true;

            // İmleç konumunu hatırla
            int selectionStart = textBoxPhoneNum.SelectionStart;
            int digitsBefore = textBoxPhoneNum.Text.Take(selectionStart).Count(char.IsDigit);

            // Sadece rakamları al
            string digits = new string(textBoxPhoneNum.Text.Where(char.IsDigit).ToArray());

            if (digits.Length > 10)
                digits = digits.Substring(0, 10);

            // Formatla
            string formatted = FormatPhone(digits);

            textBoxPhoneNum.Text = formatted;

            // Yeni imleç konumunu hesapla
            int newSelectionStart = CalculateCursorFromDigitIndex(digitsBefore, formatted);
            textBoxPhoneNum.SelectionStart = newSelectionStart;

            isFormatting = false;
        }

        private void textBoxPhoneNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '(' && e.KeyChar != ')')
            {
                e.Handled = true;
            }
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }   
        }


        private bool isValidPhoneNumber(string phoneNumber)
        {
            // Telefon numarasını biçimlendir
            string digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Eğer telefon numarası 10 haneli değilse, geçersizdir.
            if (digits.Length != 10)
            {
                return false;
            }

            // 10 haneli numara olduğunda formatın doğru olup olmadığını kontrol et
            string formatted = string.Format("({0}) {1} {2} {3}",
                                             digits.Substring(0, 3),
                                             digits.Substring(3, 3),
                                             digits.Substring(6, 2),
                                             digits.Substring(8, 2));

            return formatted == phoneNumber; // Eğer telefon numarası verilen formatla eşleşiyorsa geçerli demektir
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            checkIfReminder();
        }

        private void textBoxName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        private void textBoxSurname_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        private void textBoxEmail_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        private void textBoxAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        private void textBoxDescription_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();

            var container = this.Parent;
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

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBoxSurname_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBoxEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
