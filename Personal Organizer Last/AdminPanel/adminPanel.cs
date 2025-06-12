using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Personal_Organizer_Last.AdminPanel
{
    public partial class adminPanel : Form
    {
        string filePath = "User Login Information.csv";
        List<string[]> allInformations;
        int selectedIndex;

        public adminPanel()
        {
            InitializeComponent();
            allInformations = new List<string[]>();
        }

        private void adminPanel_Load(object sender, EventArgs e)
        {
            readFile();
            fillComboBox();
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
        private void readFile()
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File not found: " + filePath);
                return;
            }

            allInformations.Clear();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var fields = line.Split(',');

            
                if (fields.Length == 12)
                {
                    // İlk kayıt Admin, diğerleri User
                    var defaultRole = allInformations.Count == 0 ? "Admin" : "User";
                    fields = fields.Concat(new[] { defaultRole }).ToArray();
                    // Artık fields.Length == 13 oldu, son eleman UserType
                }

                // Header satırını atlamak isterseniz:
                if (fields[0] == "Email" && fields[1] == "Password")
                    continue;

                if (fields.Length >= 13)
                    allInformations.Add(fields);
            }

            
            if (allInformations.Count == 0)
            {
                allInformations.Add(new[]{
            "admin@domain.com",  // [0] Email
            "admin123",          // [1] Password
            "note_book",         // [2]
            "phone_book",        // [3]
            "personal_info",     // [4]
            "salary_calc",       // [5]
            "reminder1",         // [6]
            "/9j/4AAQS",         // [7] profil resmi base64
            "admin",             // [8] Name
            "user",              // [9] Surname
            "(555) 555-0000",    // [10] Phone
            "Head Office",       // [11] Address
            "Admin"              // [12] **UserType** 
        });
                SaveAllToCSV();
            }
        }

        private void fillComboBox()
        {
            comboBox1.Items.Clear();
            foreach (var info in allInformations)
            {
                // info[9]=Name, info[10]=Surname, info[0]=Email
                comboBox1.Items.Add($"{info[9]} {info[10]} ({info[0]})");
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex = comboBox1.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= allInformations.Count) return;

            var info = allInformations[selectedIndex];
            emailTextBox.Text = info[0];
            passwordTextBox.Text = info[1];
            // ... modül alanları vs.
            nameTextBox.Text = info[8];
            surnameTextBox.Text = info[9];
            phoneTextBox.Text = info[10];
            addressTextBox.Text = info[11];
            // UserType artık sondan gelen 13.​ sütunda, yani index 12’de
            userTypeTextBox.Text = info[12];
        }

        private async void saveButton_Click(object sender, EventArgs e)
        {
            if (selectedIndex < 0 || selectedIndex >= allInformations.Count) return;
            var info = allInformations[selectedIndex];

            // ... diğer alanlar
            info[8] = nameTextBox.Text;
            info[9] = surnameTextBox.Text;
            info[10] = phoneTextBox.Text;
            info[11] = addressTextBox.Text;
            // UserType’ı da buraya yazıyoruz:
            info[12] = userTypeTextBox.Text;

            if (checkBox1.Checked)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                saveButton.Enabled = false;
                try
                {
                    await SendEmailAsync(info[0], info[0], info[1]);
                }
                finally
                {
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    saveButton.Enabled = true;
                }
            }

            SaveAllToCSV();
            MessageBox.Show("User information saved successfully.");
        }

        private void SaveAllToCSV()
        {
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                foreach (var data in allInformations)
                    sw.WriteLine(string.Join(",", data));
            }
        }

        private async Task SendEmailAsync(string recipientEmail, string userEmail, string newPassword)
        {
            string senderEmail = "mervetaskiran777@gmail.com";
            string senderPassword = "xxxxxxxxxxx";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(recipientEmail);
                mail.Subject = "User Password Information";
                mail.Body = $"User email: {userEmail}\nNew password: {newPassword}";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    try
                    {
                        await smtp.SendMailAsync(mail);
                        MessageBox.Show("Email sent successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("SMTP Error: " + ex.Message);
                    }
                }
            }
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = Path.GetFileName(filePath); // Varsayılan dosya adı
                saveFileDialog.Filter = "CSV files (.csv)|.csv|All files (.)|.";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(filePath, saveFileDialog.FileName, true);
                        MessageBox.Show("Dosya başarıyla indirildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Dosya indirilirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("İndirilecek dosya bulunamadı.", "Dosya Yok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
    }


}

