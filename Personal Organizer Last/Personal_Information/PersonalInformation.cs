using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Personal_Organizer_Last.Personal_Information
{
    public partial class PersonalInformation : Form
    {


        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private bool isInternalChange = false;


        public event EventHandler InformationSaved;

        string filePath;
        string[] info;
        personalInfoController infoController;
        bool status = false;
        public event EventHandler BackButtonClicked;
        public bool IsInformationSaved => status;

        private void OnAnyTextChanged(object sender, EventArgs e)
        {
            if (isInternalChange) return;
            undoStack.Push(GetCurrentState());
            redoStack.Clear();
        }

        private string GetCurrentState()
        {
            string[] vals = {
        emailTextBox.Text,
        passwordTextBox.Text,
        userTypeTextBox.Text,
        nameTextBox.Text,
        surnameTextBox.Text,
        maskedTextBox1.Text,
        addressTextBox.Text
    };
            return string.Join("|~|", vals);
        }

        private void SetState(string state)
        {
            var parts = state.Split(new[] { "|~|" }, StringSplitOptions.None);
            if (parts.Length >= 7)
            {
                emailTextBox.Text = parts[0];
                passwordTextBox.Text = parts[1];
                userTypeTextBox.Text = parts[2];
                nameTextBox.Text = parts[3];
                surnameTextBox.Text = parts[4];
                maskedTextBox1.Text = parts[5];
                addressTextBox.Text = parts[6];
            }
            emailTextBox.SelectionStart = emailTextBox.Text.Length;
        }


        public PersonalInformation(string[] info, string filePath)
        {
            InitializeComponent();

            
            this.info = info;
            this.filePath = filePath;

            infoController = new personalInfoController(info, filePath);
            infoController.readFromCSV();
            chanceSalaryValue();

            backButton.Click += (s, e) => BackButtonClicked?.Invoke(this, EventArgs.Empty);

            this.KeyPreview = true;
            this.KeyDown += PersonalInformation_KeyDown;

            // TextChanged olaylarını yakala
            emailTextBox.TextChanged += OnAnyTextChanged;
            passwordTextBox.TextChanged += OnAnyTextChanged;
            userTypeTextBox.TextChanged += OnAnyTextChanged;
            nameTextBox.TextChanged += OnAnyTextChanged;
            surnameTextBox.TextChanged += OnAnyTextChanged;
            maskedTextBox1.TextChanged += OnAnyTextChanged;
            addressTextBox.TextChanged += OnAnyTextChanged;
        }


        private void PersonalInformation_Load(object sender, EventArgs e)
        {
            fillTheTextBox();
            CustomizeBackButton();


            string loginFilePath = "User Login Information.csv";
            if (File.Exists(loginFilePath))
            {
                string[] allLines = File.ReadAllLines(loginFilePath);
                foreach (string line in allLines)
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 13 && fields[0].Trim().Equals(info[0].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        // fields[0] → email eşleştiğinde:
                        passwordTextBox.Text = fields[1];     // password
                        nameTextBox.Text = fields[8];         // name
                        surnameTextBox.Text = fields[9];      // surname
                        maskedTextBox1.Text = fields[10];       // phonenum
                        addressTextBox.Text = fields[11];     // address
                        userTypeTextBox.Text = fields[12];
                        if (!string.IsNullOrEmpty(fields[7])) // base64Image
                        {
                            try
                            {
                                userPhoto.Image = Base64ToImage(fields[7]);
                            }
                            catch { /* Görsel hatasını yutabiliriz */ }
                        }

                        break;
                    }
                }
            }
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
        private void fillTheTextBox()
        {
            // Giriş sırasında gelen info dizisini kullanarak email, password, userType doldur
            if (info != null && info.Length >= 3)
            {
                emailTextBox.Text = info[0];
                passwordTextBox.Text = info[1];
                userTypeTextBox.Text = info[2];
                userPhoto.Image = Session.UserPhoto;
            }

            // Kalan veriler CSV’den çekilir
            List<string[]> _tempInfo = infoController.listInformation();

            if (_tempInfo.Count > 0)
            {
                string[] firstLine = _tempInfo[0];

                if (firstLine.Length > 3) nameTextBox.Text = firstLine[3];
                if (firstLine.Length > 4) surnameTextBox.Text = firstLine[4];
                if (firstLine.Length > 5) maskedTextBox1.Text = firstLine[5];
                if (firstLine.Length > 6) addressTextBox.Text = firstLine[6];

                if (firstLine.Length > 7 && !string.IsNullOrEmpty(firstLine[7]))
                {
                    userPhoto.Image = Base64ToImage(firstLine[7]);
                }
            }
        }

        private void PersonalInformation_FormClosing(object sender, FormClosingEventArgs e)
        {
            infoController.writeToCSV();

            
        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(emailTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordTextBox.Text) ||
                string.IsNullOrWhiteSpace(userTypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(surnameTextBox.Text) ||
                string.IsNullOrWhiteSpace(maskedTextBox1.Text) ||
                string.IsNullOrWhiteSpace(addressTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

        

            string base64Image = userPhoto.Image == null ? "" : ImageToBase64(userPhoto.Image);

            string[] updatedInfo =
            {
            emailTextBox.Text,
            passwordTextBox.Text,
            userTypeTextBox.Text,
            nameTextBox.Text,
            surnameTextBox.Text,
            maskedTextBox1.Text,
            addressTextBox.Text,
            base64Image
        };

            infoController.addAllInfo(updatedInfo);
            changeUserInfo();
            status = true;
            InformationSaved?.Invoke(this, EventArgs.Empty);

            MessageBox.Show("Your information has been successfully saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void changeUserInfo()
        {
            string loginFilePath = "User Login Information.csv";
            if (File.Exists(loginFilePath))
            {
                string[] allLines = File.ReadAllLines(loginFilePath);
                for (int i = 0; i < allLines.Length; i++)
                {
                    string[] fields = allLines[i].Split(',');

                    if (fields.Length >= 13 && fields[0].Trim().Equals(emailTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        // Güncelle
                        fields[1] = passwordTextBox.Text;   // password
                        fields[8] = nameTextBox.Text;       // name
                        fields[9] = surnameTextBox.Text;    // surname
                        fields[10] = maskedTextBox1.Text;     // phone
                        fields[11] = addressTextBox.Text;   // address
                        fields[12] = userTypeTextBox.Text;

                        // Fotoğraf boş değilse dönüştür
                        if (userPhoto.Image != null)
                        {
                            fields[7] = ImageToBase64(userPhoto.Image); // base64 image
                        }

                        allLines[i] = string.Join(",", fields);
                        break;
                    }
                }

                File.WriteAllLines(loginFilePath, allLines);
            }
        }

        private void userPhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog1.Title = "Choose Photo";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    userPhoto.Image = Image.FromFile(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There is a mistake while photo uploaded: " + ex.Message);
                }
            }
        }

        private void userPhoto_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, userPhoto.Width - 1, userPhoto.Height - 1));
        }

        private void photoRemoveButton_Click(object sender, EventArgs e)
        {
            userPhoto.Image = null;
        }

        private static string ImageToBase64(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(image))
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }


        public static Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                return Image.FromStream(ms, true);
            }
        }

        public void chanceSalaryValue()
        {
            string salary = infoController.getSalaryValue();
            if (salary == "0")
            {
                salaryLabel.ForeColor = Color.White;
                salaryLabel.Text = "*To see the salary please fill the Salary Calculator Information";
            }
            else
            {
                salaryLabel.ForeColor = Color.Green;
                salaryLabel.Text = "Salary : " + salary;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^(\d{3})\s(\d{3})\s(\d{2})\s(\d{2})$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private void addressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',') e.Handled = true;
        }

   

        private void surnameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',') e.Handled = true;
        }

        private void nameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',') e.Handled = true;
        }

        private void passwordTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',') e.Handled = true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            var container = this.Parent;
            var um = container.Controls.OfType<Personal_Organizer_Last.UserManagement>().FirstOrDefault();
            if (um != null)
            {
                um.Show();
                um.BringToFront();
            }
        }

       

        // >> EKLENDİ <<
        private void PersonalInformation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z && undoStack.Count > 0)
            {
                isInternalChange = true;
                redoStack.Push(GetCurrentState());
                var prev = undoStack.Pop();
                SetState(prev);
                isInternalChange = false;
                e.SuppressKeyPress = true;
                //e.SuppressKeyxPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y && redoStack.Count > 0)
            {
                isInternalChange = true;
                undoStack.Push(GetCurrentState());
                var next = redoStack.Pop();
                SetState(next);
                isInternalChange = false;
                e.SuppressKeyPress = true;
            }
        }

       

        private void maskedTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '(' && e.KeyChar != ')')
            {
                e.Handled = true;
            }
        }

        private void maskedTextBox1_Validating(object sender, CancelEventArgs e)
        {
            string digitsOnly = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length != 10)
            {
                e.Cancel = true;
                MessageBox.Show("Telefon numarası 10 haneli olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                maskedTextBox1.Focus();
            }
        }
    }
}
