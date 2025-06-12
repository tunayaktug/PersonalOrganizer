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

namespace Personal_Organizer_Last.NoteBook
{
    public partial class Notes : Form
    {
        private string filePath;
        private string reminderFilePath;
        private NotesController notesControl;
        private List<string[]> reminders;
        private string selectedRow;

        public event EventHandler BackButtonClicked;

        public Notes(string filePath, string reminderFilePath)
        {
            InitializeComponent();
            CustomizeBackButton();
            // --- "New" butonu Designer'dan tamamen silindiğini varsayıyoruz

            // DataGridView sütunlarını baştan tanımla: ID, Title, Content
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns.Add("Title", "Title");
            dataGridView1.Columns.Add("Content", "Content");
            dataGridView1.Columns["ID"].ReadOnly = true;    // ID sadece okunur olsun
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            // Controller ve dosya yolları
            this.filePath = filePath;
            this.reminderFilePath = reminderFilePath;
            notesControl = new NotesController(filePath);
            notesControl.readFromCSV();

            reminders = new List<string[]>();
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
        private void Notes_Load(object sender, EventArgs e)
        {
            timer1.Start();
            
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

          
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            checkIfReminder();
        }

        private void checkIfReminder()
        {
            if (File.Exists(reminderFilePath))
            {
                reminders.Clear();
                using (var sr = new StreamReader(reminderFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        reminders.Add(line.Split('£'));
                }
            }
            WriteReminderToCSV();
            CheckReminders();
        }

        private void CheckReminders()
        {
            bool shouldShake = false;
            string[] due = null;

            for (int i = 0; i < reminders.Count; i++)
            {
                var rem = reminders[i];
                var t = DateTime.ParseExact(rem[0], "dd/MM/yyyy HH:mm:ss", null);
                if (t <= DateTime.Now)
                {
                    shouldShake = true;
                    due = rem;
                    reminders.RemoveAt(i);
                    break;
                }
            }

            if (shouldShake && due != null)
            {
                ShakeScreen();
                this.Text = $"Reminder for {due[2]} - Time's up!";
            }
        }

        private void ShakeScreen()
        {
            var rand = new Random();
            const int intensity = 10;
            for (int i = 0; i < 10; i++)
            {
                this.Location = new Point(
                    this.Location.X + rand.Next(-intensity, intensity),
                    this.Location.Y + rand.Next(-intensity, intensity)
                );
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


        private void clearTextBox()
        {
            titleTextBox.Clear();
            noteTextBox.Clear();
        }

        /// <summary>
        /// Grid'i ID+Title+Content ile doldurur
        /// </summary>
        private void RefreshGrid()
        {
            dataGridView1.Rows.Clear();
            var list = notesControl.listNodes();
            for (int i = 0; i < list.Count; i++)
            {
                var note = list[i];
               
                dataGridView1.Rows.Add(i + 1, note[0], note[1]);
            }
        }

        // === SAVE: Not ekle, ama GRID'E ekleme yapma
        private void saveButton_Click(object sender, EventArgs e)
        {
            var title = titleTextBox.Text.Trim();
            var content = noteTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Lütfen başlık ve içerik doldurun.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (notesControl.listNodes().Any(n => n[0] == title))
            {
                MessageBox.Show("Bu başlık zaten var, lütfen farklı bir başlık girin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            notesControl.createNodes(title, content);
            notesControl.writeToCSV();  // Yeni eklenen notu hemen kaydet
            clearTextBox();
            // <— burada RefreshGrid() yok, not listede gözükmez
        }

        // === LIST: Tüm notları ID+Title+Content ile göster
        private void listNotes_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        // === SELECTION CHANGED: Seçilenin başlığını ve içeriğini TextBox'lara aktar
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;

            var row = dataGridView1.SelectedRows[0];
            titleTextBox.Text = row.Cells["Title"].Value?.ToString() ?? "";
            noteTextBox.Text = row.Cells["Content"].Value?.ToString() ?? "";
            selectedRow = row.Index.ToString();  // 0-based index, controller'da kullanacağız
        }

        // === UPDATE: Seçilen index'e göre güncelle
        private void updateButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellenecek bir not seçin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var index = Convert.ToInt32(selectedRow);
            var title = titleTextBox.Text.Trim();
            var content = noteTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Başlık ve içerik boş olamaz.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            notesControl.updateNodes(title, content, index);
            notesControl.writeToCSV();  // Güncellenen notu hemen kaydet
            RefreshGrid();
            clearTextBox();
            MessageBox.Show("Not güncellendi.", "Bilgi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // === DELETE: Seçilen index'e göre sil
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silinecek bir not seçin.", "Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = dataGridView1.SelectedRows[0];
            var title = row.Cells["Title"].Value.ToString();
            var content = row.Cells["Content"].Value.ToString();

            var dr = MessageBox.Show("Bu notu silmek istediğinize emin misiniz?", "Uyarı",
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) return;

            notesControl.deleteNodes(title, content);
            notesControl.writeToCSV();  // Güncellenen notu hemen kaydet
            RefreshGrid();
            clearTextBox();
        }

        private void Notes_FormClosing(object sender, FormClosingEventArgs e)
        {
            notesControl.writeToCSV();
        }

        // === TEMİZLE butonu (varsa)
        private void refreshButton_Click(object sender, EventArgs e)
        {
            clearTextBox();
        }

        // === Karakter engelleme
        private void noteTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'μ') e.Handled = true;
        }
        private void titleTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'μ') e.Handled = true;
        }
        private void noteTextBox_TextChanged(object sender, EventArgs e)
        {
            if (titleTextBox.Text.Contains("μ"))
            {
                titleTextBox.Text = titleTextBox.Text.Replace("μ", "");
                MessageBox.Show("Başlıkta 'μ' karakteri olamaz.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void titleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (titleTextBox.Text.Contains("μ"))
            {
                titleTextBox.Text = titleTextBox.Text.Replace("μ", "");
                MessageBox.Show("Başlıkta 'μ' karakteri olamaz.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // === Back butonu
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
