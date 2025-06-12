using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public partial class ForgotPasswordForm : Form
{
    private const string CsvPath = "User Login Information.csv";
    private TextBox txtEmail;
    private Button backButton;
    private Label label1;
    private List<string[]> allUsers;

    public ForgotPasswordForm()
    {
        InitializeComponent();
    }

    private async void btnSend_Click_1(object sender, EventArgs e)
    {
        // 1) E-posta al
        string email = txtEmail.Text.Trim();
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            MessageBox.Show("Lütfen geçerli bir e-posta girin.");
            return;
        }

        // 2) CSV yolunu oluştur ve varlığını kontrol et
        string relativePath = "User Login Information.csv";
        string fullPath = Path.Combine(Application.StartupPath, relativePath);
        if (!File.Exists(fullPath))
        {
            MessageBox.Show("CSV dosyası bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 3) Satırları oku
        var lines = File.ReadAllLines(fullPath);
        allUsers = lines
                       .Where(l => !string.IsNullOrWhiteSpace(l))
                       .Select(l => l.Split(','))
                       .ToList();

        // 4) Email ile kullanıcıyı bul
        int idx = allUsers.FindIndex(u =>
            string.Equals(u[0], email, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
        {
            MessageBox.Show("Eğer kayıtlı bir e-posta ise yeni şifre gönderildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            return;
        }

        // 5) Yeni şifre üret ve CSV’yi güncelle
        string newPass = PasswordHelper.Generate(10);
        allUsers[idx][1] = newPass;
        File.WriteAllLines(fullPath,
            allUsers.Select(a => string.Join(",", a)));

        // 6) Şifreyi girilen adrese yolla
        try
        {
            await EmailHelper.SendNewPasswordAsync(email, newPass);
            MessageBox.Show("Yeni şifre mail adresinize gönderildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("E-posta gönderilemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        this.Close();
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
    private void InitializeComponent()
    {
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.backButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.Gainsboro;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtEmail.Location = new System.Drawing.Point(131, 102);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(274, 30);
            this.txtEmail.TabIndex = 0;
            // 
            // backButton
            // 
            this.backButton.Location = new System.Drawing.Point(215, 138);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(89, 32);
            this.backButton.TabIndex = 1;
            this.backButton.Text = "SEND";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.btnSend_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(127, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Email : ";
            // 
            // ForgotPasswordForm
            // 
            this.BackColor = System.Drawing.Color.DarkSlateGray;
            this.ClientSize = new System.Drawing.Size(528, 253);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.txtEmail);
            this.Name = "ForgotPasswordForm";
            this.Load += new System.EventHandler(this.ForgotPasswordForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private void ForgotPasswordForm_Load(object sender, EventArgs e)
    {
        CustomizeBackButton();
    }
}
