using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Personal_Organizer_Last.PhoneBook
{
    public class PhoneBookController : PhoneBookManager
    {
        private List<string[]> records;
        string filePath;

        public PhoneBookController(string filePath)
        {
            records = new List<string[]>();
            this.filePath = filePath;
        }

        public override List<string[]> listRecord()
        {
            return records;
        }

        public override void deleteRecord(string name, string surname, string phoneNumber, string email, string address, string description)
        {
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                if (record[0] == name && record[1] == surname && record[2] == phoneNumber && record[3] == email && record[4] == address && record[5] == description)
                {
                    records.RemoveAt(i);
                    break;
                }
                else
                {
                    //MessageBox.Show("Please choose the person again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public override void updateRecord(
        string name,
        string surname,
        string phoneNumber,
        string email,
        string address,
        string description,
        int index)
        {
           
            if (index < 0 || index >= records.Count)
            {
                MessageBox.Show("Geçersiz indeks. Lütfen geçerli bir indeks seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Geçerli indeks olduğunda işlemleri yap
            string[] selectedRecord = records[index];

            selectedRecord[0] = name;
            selectedRecord[1] = surname;
            selectedRecord[2] = phoneNumber;
            selectedRecord[3] = email;
            selectedRecord[4] = address;
            selectedRecord[5] = description;
        }



        public override void createRecord(string name, string surname, string phoneNumber, string email, string address, string description)
        {
           

            if (!isValidEmail(email))
            {
               
                MessageBox.Show("Invalid Email Format.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var record = new string[] { name, surname, phoneNumber, email, address, description };
            records.Add(record);

        }


        public bool isContain(string name, string surname, string phoneNumber, string email, string address, string description)
        {
            foreach (var record in records)
            {
                if (record[0] == name &&
                    record[1] == surname &&
                    record[2] == phoneNumber &&
                    record[3] == email &&
                    record[4] == address &&
                    record[5] == description)
                {
                    return false;
                }
            }
            return true;
        }



        


        private bool isValidEmail(string email)
        {
            var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        public void WriteToCSV()
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
                foreach (var record in records)
                {
                    string line = string.Join(",", record);
                    sw.WriteLine(line);
                }
            }
        }


        public void ReadFromCSV()
        {
            if (File.Exists(filePath))
            {
                records.Clear(); // Önceki kayıtları temizle

                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');
                        records.Add(fields);
                    }
                }
            }
        }
    }
}
