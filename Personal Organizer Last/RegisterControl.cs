using Personal_Organizer_Last.AdminPanel;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Personal_Organizer_Last
{
    public partial class RegisterControl : UserControl
    {
        public event EventHandler backButtonClicked;

        public RegisterControl()
        {
            InitializeComponent();

            textBox2.PasswordChar = '*';
            textBoxName.KeyPress += OnlyLetters_KeyPress;
            textBoxSurname.KeyPress += OnlyLetters_KeyPress;

            uploadPhotoButton.Click += UploadPhotoButton_Click;
            backButton.Click += (s, e) => backButtonClicked?.Invoke(this, EventArgs.Empty);
            showPasswordButton.Click += ShowPasswordButton_Click;
            registerButton.Click += registerButton_Click;
        }

        private void RegisterControl_Load(object sender, EventArgs e)
        {
            pictureBoxEmailIcon.Visible = true;
            pictureBoxPasswordIcon.Visible = true;

            StylizeButton(registerButton, Color.MediumSlateBlue);
            StylizeButton(backButton, Color.MediumSlateBlue);
            StylizeButton(uploadPhotoButton, Color.MediumSlateBlue);
        }

        private void StylizeButton(Button button, Color bgColor)
        {
            MakeButtonRounded(button, 35);
            button.BackColor = bgColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        private void MakeButtonRounded(Button button, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = button.ClientRectangle;

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            button.Region = new Region(path);
        }

        private void UploadPhotoButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBoxProfile.Image = Image.FromFile(openFileDialog.FileName);
                    pictureBoxProfile.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private int GetCurrentFileNumber(string counterFilePath, int defaultStart = 18)
        {
            int currentNumber = defaultStart;

            try
            {
                if (File.Exists(counterFilePath))
                {
                    string content = File.ReadAllText(counterFilePath);
                    if (int.TryParse(content, out int savedNumber))
                    {
                        currentNumber = savedNumber + 1;
                    }
                }

                File.WriteAllText(counterFilePath, currentNumber.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Counter file error: {ex.Message}");
            }

            return currentNumber;
        }

        private void ShowPasswordButton_Click(object sender, EventArgs e)
        {
            textBox2.PasswordChar = textBox2.PasswordChar == '*' ? '\0' : '*';
        }

        private void OnlyLetters_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Only letters are allowed in this field.");
            }
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            string surname = textBoxSurname.Text.Trim();
            string email = textBoxEmail.Text.Trim();
            string password = textBox2.Text;
            string address = textBox6.Text;
            string phonenum = maskedTextBox1.Text;
            Image profileImage = pictureBoxProfile.Image;
            Session.Name = name;
            Session.Surname = surname;
            Session.Phone = phonenum;
            Session.Address = address;


            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phonenum) ||
                profileImage == null)
            {
                MessageBox.Show("Please fill all fields and upload a profile photo.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format. Example: yourname@mail.com");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Password must be at least 6 characters, include one uppercase letter and one digit.");
                return;
            }

            try
            {
                // CSV dosyası yolu
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "User Login Information.csv");

                // Mevcut satır sayısını al (boş/güçlü olmayan satırları atlayarak)
                int existingCount = 0;
                if (File.Exists(filePath))
                    existingCount = File
                        .ReadAllLines(filePath)
                        .Count(l => !string.IsNullOrWhiteSpace(l));

                // İlk kayıt Admin, diğerleri User
                string role = existingCount == 0 ? "Admin" : "User";

                // Counter dosyasından modül dosya numaralarını al
                string counterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "counter_value.txt");
                int currentNoteBookNumber = GetCurrentFileNumber(counterPath);
                int currentPhoneBookNumber = GetCurrentFileNumber(counterPath);
                int currentPersonalInfoNumber = GetCurrentFileNumber(counterPath);
                int currentSalaryCalculatorNumber = GetCurrentFileNumber(counterPath);
                int currentReminderNumber = GetCurrentFileNumber(counterPath);

                string noteBookFileName = $"note_book{currentNoteBookNumber}.csv";
                string phoneBookFileName = $"phone_book{currentPhoneBookNumber}.csv";
                string personalInfoFileName = $"personal_information{currentPersonalInfoNumber}.csv";
                string salaryCalculatorFileName = $"salary_calculator{currentSalaryCalculatorNumber}.csv";
                string reminderFileName = $"reminder{currentReminderNumber}.csv";

                // Fotoğrafı base64'e çevir
                string base64Image = "";
                if (pictureBoxProfile.Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        pictureBoxProfile.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        base64Image = Convert.ToBase64String(ms.ToArray());
                    }
                }

                // Kullanıcı login bilgileri CSV'ye **13 kolon** olarak yaz
                using (var sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(
                        $"{email}," +
                        $"{password}," +
                        $"{noteBookFileName}," +
                        $"{phoneBookFileName}," +
                        $"{personalInfoFileName}," +
                        $"{salaryCalculatorFileName}," +
                        $"{reminderFileName}," +
                        $"{base64Image}," +
                        $"{name}," +
                        $"{surname}," +
                        $"{phonenum}," +
                        $"{address}," +
                        $"{role}"           // <-- sondaki 13. sütun UserType
                    );
                }

                // Personal info dosyasına yazma (aynı önceki kodunuz)
                string personalInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, personalInfoFileName);
                using (var swInfo = new StreamWriter(personalInfoPath, false))
                {
                    swInfo.WriteLine("Name,Surname,PhoneNumber,Address");
                    swInfo.WriteLine($"{name},{surname},{phonenum},{address}");
                }

                // Session atamaları ve geriye dönüş
                Session.FullName = $"{name} {surname}";
                Session.UserPhoto = profileImage;
                Session.Name = name;
                Session.Surname = surname;
                Session.Email = email;
                Session.Phone = phonenum;
                Session.Address = address;

                MessageBox.Show("Registration successful!");
                backButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving user: " + ex.Message);
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"\d");
        }

        private void textBoxName_TextChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
