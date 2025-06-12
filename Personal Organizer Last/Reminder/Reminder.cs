using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace Personal_Organizer_Last.Reminder
{
    public partial class Reminder : Form
    {
        DateTime selectedDateTime;
        string category;
        string summary;
        string description;
        readonly string filePath;
        List<string[]> reminders;

        public Reminder(string filePath)
        {
            InitializeComponent();
           
           



            this.filePath = filePath;
            this.reminders = new List<string[]>();

            // --- DataGridView Tanımı ---
            dataGridView1.Columns.Clear();

            // 1) Done checkbox sütunu
            var doneCol = new DataGridViewCheckBoxColumn()
            {
                Name = "Done",
                HeaderText = "Done"
            };
            dataGridView1.Columns.Add(doneCol);

            // 2) Diğer sütunlar
            dataGridView1.Columns.Add("Title", "Title");
            dataGridView1.Columns.Add("Description", "Description");
            dataGridView1.Columns.Add("Summary", "Summary");
            dataGridView1.Columns.Add("Date", "Date");
            dataGridView1.Columns.Add("Time", "Time");
            dataGridView1.Columns.Add("Category", "Category");

            // Seçim ve boyut ayarları
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // Checkbox değişimini yakala
            dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;

            // CSV’den oku
            ReadFromCSV();
        }

        private void Reminder_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
            timer1.Start();
            CustomizeBackButton();
            // Kolonları eşit şekilde yaymak
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        // “Add” butonu: sadece listeye + CSV’ye ekler, grid’i güncellemez
        private void addButton_Click(object sender, EventArgs e)
        {
            selectedDateTime = dateTimePicker1.Value;
            category = categoryBox.SelectedIndex >= 0
                                 ? categoryBox.SelectedItem.ToString()
                                 : "";
            summary = summaryTextBox.Text.Trim();
            description = descriptionTextBox.Text.Trim();

            if (string.IsNullOrEmpty(category) ||
                string.IsNullOrEmpty(summary) ||
                string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Summary benzersizliği
            if (reminders.Any(r => r[2] == summary))
            {
                MessageBox.Show("Aynı summary’e sahip kayıt var, başka bir isim verin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Yeni kaydı ekle (done başlangıçta False)
            reminders.Add(new string[]{
                selectedDateTime.ToString("dd/MM/yyyy HH:mm:ss"), // 0: timestamp
                category,   // 1
                summary,    // 2
                description,// 3
                "False"     // 4: done
            });

            WriteToCSV();
           
        }

        // “List” butonu
        private void listButton_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        // Grid’i dolduran metod
        private void RefreshGrid()
        {
            dataGridView1.Rows.Clear();

            foreach (var r in reminders)
            {
                // r[0] = "dd/MM/yyyy HH:mm:ss"
                var dt = DateTime.ParseExact(r[0], "dd/MM/yyyy HH:mm:ss", null);
                var date = dt.ToString("dd.MM.yyyy");
                var time = dt.ToString("HH:mm:ss");

                dataGridView1.Rows.Add(
                    bool.Parse(r[4]), // Done
                    r[2],             // Title
                    r[3],             // Description
                    r[2],             // Summary (istersen mapping’i değiştir)
                    date,             // Date
                    time,             // Time
                    r[1]              // Category
                );
            }
        }

        
        private void updateButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Güncellemek için bir satır seçin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int idx = dataGridView1.SelectedRows[0].Index;

            // Liste kaydını güncelle (done r[4] el değmeden kalır)
            var dt = dateTimePicker1.Value.ToString("dd/MM/yyyy HH:mm:ss");
            reminders[idx][0] = dt;
            reminders[idx][1] = categoryBox.Text.Trim();
            reminders[idx][2] = summaryTextBox.Text.Trim();
            reminders[idx][3] = descriptionTextBox.Text.Trim();

            WriteToCSV();
            RefreshGrid();
        }

        // “Delete” butonu
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silmek için bir satır seçin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int idx = dataGridView1.SelectedRows[0].Index;
            reminders.RemoveAt(idx);
            WriteToCSV();
            RefreshGrid();
        }

        // CSV okuma
        private void ReadFromCSV()
        {
            reminders.Clear();
            if (!File.Exists(filePath)) return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var f = line.Split('£');
                if (f.Length == 5)
                    reminders.Add(f);
            }
        }

        // CSV yazma
        private void WriteToCSV()
        {
            File.WriteAllText(filePath, String.Empty);
            using (var sw = new StreamWriter(filePath, true))
                foreach (var r in reminders)
                    sw.WriteLine(string.Join("£", r));
        }

        // Checkbox commit (CommitEdit) için
        private void DataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty &&
                dataGridView1.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Checkbox değeri değiştiğinde listeye ve CSV’ye kaydet
        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dataGridView1.Columns[e.ColumnIndex].Name != "Done")
                return;

            bool isDone = (bool)dataGridView1.Rows[e.RowIndex].Cells["Done"].Value;
            reminders[e.RowIndex][4] = isDone.ToString();
            WriteToCSV();
        }

        // Timer tick: saati güncelle + reminder kontrolü
        private void timer1_Tick(object sender, EventArgs e)
        {
            labelNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            CheckReminders();
        }

        // Süresi gelenleri titreştirip bildir
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
                    RefreshGrid();

                    ShakeScreen();
                    MessageBox.Show(
                        $"Reminder for «{sum}» is due now!",
                        "Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
        }

        private void ShakeScreen()
        {
            var rand = new Random();
            for (int i = 0; i < 20; i++)
            {
                this.Location = new Point(
                    this.Location.X + rand.Next(-20, 20),
                    this.Location.Y + rand.Next(-20, 20)
                );
                Thread.Sleep(20);
            }
        }

        // Seçili satırı textBox’lara doldur
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;
            var row = dataGridView1.SelectedRows[0];

            // Date + Time’ı parse et
            var date = row.Cells["Date"].Value?.ToString();
            var time = row.Cells["Time"].Value?.ToString();
            if (DateTime.TryParseExact(
                    $"{date} {time}",
                    "dd.MM.yyyy HH:mm:ss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var dt))
            {
                dateTimePicker1.Value = dt;
            }

            categoryBox.Text = row.Cells["Category"].Value?.ToString() ?? "";
            summaryTextBox.Text = row.Cells["Summary"].Value?.ToString() ?? "";
            descriptionTextBox.Text = row.Cells["Description"].Value?.ToString() ?? "";
        }

        // “Back” butonu
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

        // “£” karakterini engelle (istersen diğer KeyPress event’lerini sil)
        private void categoryBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '£') e.Handled = true;
        }
        private void summaryTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '£') e.Handled = true;
        }
        private void descriptionTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '£') e.Handled = true;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

      
    }
}
