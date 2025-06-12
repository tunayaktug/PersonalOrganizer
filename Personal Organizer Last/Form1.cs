using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Personal_Organizer_Last
{
    public partial class Form1 : Form
    {
        private LoginScreenManager loginScreenManager;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            // Paneli ortala
            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
            
            // Sign Up görünümü
            label4.Text = "Doesn't have an account yet?";
            labelRegister.ForeColor = Color.White;
            labelRegister.Font = new Font(labelRegister.Font, FontStyle.Underline);
            labelRegister.Cursor = Cursors.Hand;
            labelRegister.Click += labelRegister_Click;

            // Register butonunu devre dışı bırak (kullanılmıyor)
            registerButton.Visible = false;

            // Giriş ekranını göster
            ShowLoginScreen();
        }

        private void ShowLoginScreen()
        {
            mainPanel.Controls.Clear();

            // Giriş arayüzü elemanları
            mainPanel.Controls.Add(label1);         // Email :
            mainPanel.Controls.Add(label2);         // Password :
            mainPanel.Controls.Add(label3);         // Login başlık
            mainPanel.Controls.Add(label4);         // "Don't have an account yet?"
            mainPanel.Controls.Add(labelRegister);  // Sign Up
            mainPanel.Controls.Add(emailTextBox);
            mainPanel.Controls.Add(passwordTextBox);
            mainPanel.Controls.Add(showPassword);
            mainPanel.Controls.Add(loginButton);
            mainPanel.Controls.Add(pictureBox3);    // Kullanıcı simgesi
            mainPanel.Controls.Add(pictureBoxMail); // Email simgesi
            mainPanel.Controls.Add(pictureBoxPassword); // Şifre simgesi

            // Butonu yuvarlaklaştır
            MakeButtonRounded(loginButton, 25);


            var linkForgot = new LinkLabel
            {
                Text = "Forget Password?",
                AutoSize = true,
                LinkColor = Color.White,
                Left = passwordTextBox.Left,
                Top = loginButton.Bottom + 15,
                Cursor = Cursors.Hand
            };
            linkForgot.Click += (s, e) => OpenForgotPasswordForm();
            mainPanel.Controls.Add(linkForgot);

            // Stil (mor gibi görünmesi için renk ayarı)
            loginButton.BackColor = Color.MediumSlateBlue;
            loginButton.ForeColor = Color.White;
            loginButton.FlatStyle = FlatStyle.Flat;
            loginButton.FlatAppearance.BorderSize = 0;

            mainPanel.BringToFront();
        }

        private void OpenForgotPasswordForm()
        {
            using (var frm = new ForgotPasswordForm())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
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

        private void ShowRegisterScreen()
        {
            RegisterControl registerControl = new RegisterControl();
            registerControl.Dock = DockStyle.Fill;

            registerControl.backButtonClicked += (s, e) =>
            {
                ShowLoginScreen();
            };

            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(registerControl);
            mainPanel.BringToFront();
        }

        private void loginButton_Click_1(object sender, EventArgs e)
        {
            string email = emailTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Lütfen e-posta ve şifre alanlarını doldurunuz.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Hatalı E-posta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Şifre en az 6 karakter uzunluğunda olmalı, en az 1 büyük harf ve 1 rakam içermelidir.", "Geçersiz Şifre", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            loginScreenManager = new LoginScreenManager();
            this.Visible = false;
            loginScreenManager.login(email, password);
        }

        private void labelRegister_Click(object sender, EventArgs e)
        {
            ShowRegisterScreen();
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 && password.Any(char.IsUpper) && password.Any(char.IsDigit);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to close this form?", "Close Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                e.Cancel = true;
        }

        private void showPassword_CheckedChanged_1(object sender, EventArgs e)
        {
            passwordTextBox.PasswordChar = showPassword.Checked ? '\0' : '*';
        }

        private void passwordTextBox_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
                e.Handled = true;
        }

        private void emailTextBox_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
                e.Handled = true;
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
