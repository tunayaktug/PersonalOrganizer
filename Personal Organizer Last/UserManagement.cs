using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Personal_Organizer_Last.AdminPanel;
using Personal_Organizer_Last.NoteBook;
using Personal_Organizer_Last.Personal_Information;
using static System.Collections.Specialized.BitVector32;

namespace Personal_Organizer_Last
{
    public partial class UserManagement : UserControl
    {
        public event EventHandler logoutClicked;

        private string[] personal_file_path;
        private Notes noteController;
        private PhoneBook.PhoneBook phoneBookController;
        private PersonalInformation personalInfoController;
        private SalaryCalculator.SalaryCalculator salaryController;
        private adminPanel admin_panel;
        private Reminder.Reminder reminderControl;

        private List<string[]> reminders;
        private string[] info;
        private bool status = false;

        public UserManagement(string[] _info, string[] personal_file_path)
        {
            InitializeComponent();
            this.info = _info;
            this.personal_file_path = personal_file_path;
            reminders = new List<string[]>();
            adminPanelButton.Visible = false;

            checkIfAdmin();
        }

        private void UserManagement_Load(object sender, EventArgs e)
        {
            
            dataGridViewHomeReminders.Columns.Add("Title", "Başlık");
            dataGridViewHomeReminders.Columns.Add("Date", "Tarih");
            dataGridViewHomeReminders.Columns.Add("Time", "Saat");
            dataGridViewHomeReminders.Columns.Add("Category", "Kategori");
            dataGridViewHomeReminders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewHomeReminders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.WindowState = FormWindowState.Maximized;
                parentForm.StartPosition = FormStartPosition.CenterScreen;
            }
            timer1.Start();
            SetButtonStyles();

            // Load'da köşeleri yuvarlat:
            RoundCorners(labelWelcome, 20);
            RoundCorners(label3, 15);

            RefreshHomeReminders();

            label3.Text = "What are we doing today?";

            //  Sağ üstte kullanıcı bilgisi
            if (!string.IsNullOrEmpty(Session.FullName))
            {
                string firstName = Session.FullName.Split(' ')[0];
                labelWelcome.Text = $"Hey, {firstName}";
                

            }

            if (Session.UserPhoto != null)
            {
                pictureBoxProfile.Image = Session.UserPhoto;
                pictureBoxProfile.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxProfile.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private void RefreshHomeReminders()
        {
            // Grid’i temizle
            dataGridViewHomeReminders.Rows.Clear();

            foreach (var r in reminders)
            {
                // r[0] = "dd/MM/yyyy HH:mm:ss"
                DateTime dt = DateTime.ParseExact(r[0], "dd/MM/yyyy HH:mm:ss", null);

                // Sadece gelecekteki (veya içinde bulunduğumuz) hatırlatmaları göster:
                if (dt >= DateTime.Now)
                {
                    string date = dt.ToString("dd.MM.yyyy");
                    string time = dt.ToString("HH:mm:ss");
                    string title = r[2];  
                    string category = r[1];

                    dataGridViewHomeReminders.Rows.Add(title, date, time, category);
                }
            }
        }

        private void RoundCorners(Control ctrl, int radius)
        {
            var path = new GraphicsPath();
            int w = ctrl.Width;
            int h = ctrl.Height;

            // Dört köşeyi çeyrek daire ile ekle
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(w - radius, 0, radius, radius, 270, 90);
            path.AddArc(w - radius, h - radius, radius, radius, 0, 90);
            path.AddArc(0, h - radius, radius, radius, 90, 90);
            path.CloseFigure();

            ctrl.Region = new Region(path);
        }

        private void SetButtonStyles()
        {
            var buttons = new[] {
                notebookButton, salaryCalculatorButton, phonebookbutton,
                personInfoButton, adminPanelButton, reminderButton
            };

            foreach (var btn in buttons)
            {
                btn.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
            }
        }

        private void notebookButton_Click(object sender, EventArgs e)
        {
            

            if (noteController == null || noteController.IsDisposed)
            {
                noteController = new Notes(personal_file_path[0], personal_file_path[4])
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                this.FindForm()?.Controls.Add(noteController);
            }

            this.Hide();
            noteController.BringToFront();
            noteController.Show();
        }

        private void phonebookbutton_Click(object sender, EventArgs e)
        {
           

            if (phoneBookController == null || phoneBookController.IsDisposed)
            {
                phoneBookController = new PhoneBook.PhoneBook(personal_file_path[1], personal_file_path[4])
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                this.FindForm()?.Controls.Add(phoneBookController);
            }

            this.Hide();
            phoneBookController.BringToFront();
            phoneBookController.Show();
        }

        private void personInfoButton_Click(object sender, EventArgs e)
        {
            if (personalInfoController == null || personalInfoController.IsDisposed)
            {
                personalInfoController = new PersonalInformation(info, personal_file_path[2])
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                personalInfoController.InformationSaved += (s, ev) =>
                {
                    status = true;
                };

                this.FindForm()?.Controls.Add(personalInfoController);
            }

            this.Hide();
            personalInfoController.BringToFront();
            personalInfoController.Show();
        }

        private void salaryCalculatorButton_Click(object sender, EventArgs e)
        {
            

            chanceUserType();

            if (salaryController == null || salaryController.IsDisposed)
            {
                salaryController = new SalaryCalculator.SalaryCalculator(info[2], personal_file_path[3])
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                this.FindForm()?.Controls.Add(salaryController);
            }

            this.Hide();
            salaryController.BringToFront();
            salaryController.Show();
        }

        private void adminPanelButton_Click(object sender, EventArgs e)
        {
            
           

            // 2) Sadece Admin rolündekiler geçebilsin
            if (Session.UserType != UserType.Admin)
            {
                MessageBox.Show("You do not have permission to access Admin Panel.");
                return;
            }

            // 3) Paneli daha önce yaratmadıysak oluştur
            if (admin_panel == null || admin_panel.IsDisposed)
            {
                admin_panel = new AdminPanel.adminPanel()
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                // Mevcut Form'un kontrollerine ekle
                this.FindForm()?.Controls.Add(admin_panel);
            }

            // 4) UserManagement ekranını gizle, admin_panel'i göster
            this.Hide();
            admin_panel.BringToFront();
            admin_panel.Show();
        }

        private void reminderButton_Click(object sender, EventArgs e)
        {
            

            if (reminderControl == null || reminderControl.IsDisposed)
            {
                reminderControl = new Reminder.Reminder(personal_file_path[4])
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                this.FindForm()?.Controls.Add(reminderControl);
            }

            this.Hide();
            reminderControl.BringToFront();
            reminderControl.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString();
            checkIfReminder();
            RefreshHomeReminders();  // sonra grid’i yeniden doldur
        }

        private void checkIfAdmin()
        {
            if (info[2] == "Admin")
            {
                adminPanelButton.Visible = true;
                return;
            }

            string _filePath = personal_file_path[2];
            if (File.Exists(_filePath))
            {
                using (var sr = new StreamReader(_filePath))
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var fields = line.Split(',');
                        if (fields.Length >= 3 && fields[2].Trim() == "Admin")
                            adminPanelButton.Visible = true;
                    }
                }
            }
        }

        private void chanceUserType()
        {
            string path = personal_file_path[2];
            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path))
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var fields = line.Split(',');
                        if (fields.Length >= 3)
                            info[2] = fields[2];
                    }
                }
            }
        }

        private void checkIfReminder()
        {
            if (File.Exists(personal_file_path[4]))
            {
                reminders.Clear();
                using (var sr = new StreamReader(personal_file_path[4]))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        reminders.Add(line.Split('£'));
                }
            }
            CheckReminders();
            RefreshHomeReminders();
        }
        private void CheckReminders()
        {
            for (int i = 0; i < reminders.Count; i++)
            {
                var r = reminders[i];
                var dt = DateTime.ParseExact(r[0], "dd/MM/yyyy HH:mm:ss", null);
                if (dt <= DateTime.Now)
                {
                    var sum = r[2];
                    reminders.RemoveAt(i);
                    WriteToCSV();

                    ShakeScreen(); // ekran gerçekten sallanacak
              
                    break;
                }
            }
        }

        private Timer shakeTimer;
        private int shakeCount = 0;
        private Point originalLocation;
        private Random rand = new Random();

        private void ShakeScreen()
        {
            shakeCount = 0;
            originalLocation = this.Location;

            shakeTimer = new Timer();
            shakeTimer.Interval = 60;
            shakeTimer.Tick += ShakeTimer_Tick;
            shakeTimer.Start();
        }

        private void ShakeTimer_Tick(object sender, EventArgs e)
        {
            if (shakeCount < 20)
            {
                this.Location = new Point(
                    originalLocation.X + rand.Next(-20, 20),
                    originalLocation.Y + rand.Next(-20, 20)
                );
                shakeCount++;
            }
            else
            {
                shakeTimer.Stop();
                shakeTimer.Tick -= ShakeTimer_Tick;
                this.Location = originalLocation;
            }
        }


        public void WriteToCSV()
        {
            var path = personal_file_path[4];
            using (var sw = new StreamWriter(path, false))
            {
                foreach (var rem in reminders)
                    sw.WriteLine(string.Join("£", rem));
            }
        }

    

        private void phonebookbutton_Click_1(object sender, EventArgs e)
        {

                if (phoneBookController == null || phoneBookController.IsDisposed)
                {
                    phoneBookController = new PhoneBook.PhoneBook(personal_file_path[1], personal_file_path[4])
                    {
                        TopLevel = false,
                        FormBorderStyle = FormBorderStyle.None,
                        Dock = DockStyle.Fill
                    };

                    var parentForm = this.FindForm();
                    parentForm.Controls.Add(phoneBookController);
                }

                this.Hide();
                phoneBookController.BringToFront();
                phoneBookController.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1(); // Giriş formu
            loginForm.Show();

            // Bu UserManagement içindeki parent formu kapat
            Form currentForm = this.FindForm();
            currentForm.Hide(); 
        }

       
    }
}
