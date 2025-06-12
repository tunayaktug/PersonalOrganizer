using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Personal_Organizer_Last
{
    public static class ImageHelper
    {
        public static Image ConvertBase64ToImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                if (base64String.StartsWith("data:image"))
                    base64String = base64String.Split(',')[1];

                byte[] bytes = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class LoginScreenManager
    {
        private List<string[]> user_list;
        private List<User> users;
        private const string filePath = "User Login Information.csv";
        private const string counterFilePath = "counter_value.txt";
        private static int counter = 0;

        public LoginScreenManager()
        {
            user_list = new List<string[]>();
            users = new List<User>();
            ReadCounterFromFile();
            ReadFromCSV();
        }

        private void ReadFromCSV()
        {
            if (!File.Exists(filePath)) return;
            user_list.Clear();

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("Email,", StringComparison.OrdinalIgnoreCase)) continue;

                var f = line.Split(',');

                // Eski 12 sütunlu kayıtlara default rol ekle
                if (f.Length == 12)
                {
                    var role = user_list.Count == 0 ? "Admin" : "User";
                    f = f.Concat(new[] { role }).ToArray();
                }

                if (f.Length >= 13)
                    user_list.Add(f);
            }

            
        }

        private void WriteToCSV()
        {
            using (var sw = new StreamWriter(filePath, false))
            {
                
                foreach (var f in user_list)
                    sw.WriteLine(string.Join(",", f));
            }
        }

        public void login(string email, string password)
        {
            bool userFound = false;

            for (int i = 0; i < user_list.Count; i++)
            {
                var f = user_list[i];

                if (f[0].Equals(email, StringComparison.OrdinalIgnoreCase)
                 && f[1] == password)
                {
                    // 1) Modül dosya yolları = f[2]..f[6]
                    string[] personalFilePaths = f.Skip(2).Take(5).ToArray();

                    // 2) Rol bilgisi = f[12]
                    var roleText = f[12];
                    UserType userType = roleText.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                        ? UserType.Admin
                        : (roleText.Equals("PartTimeUser", StringComparison.OrdinalIgnoreCase)
                            ? UserType.PartTimeUser
                            : UserType.User);

                    // 3) Profil resmi = f[7]
                    string photoBase64 = f[7];

                    // 4) İsim, soyisim, telefon, adres
                    string name = f[8];
                    string surname = f[9];
                    string phone = f[10];
                    string address = f[11];

                    // 5) User nesnesi
                    var tempUser = new User(
                        email,
                        password,
                        name,
                        surname,
                        photoBase64,
                        userType,
                        personalFilePaths
                    );

                    users.Add(tempUser);
                    Session.FullName = $"{name} {surname}";
                    Session.UserPhoto = ImageHelper.ConvertBase64ToImage(photoBase64);
                    Session.Phone = phone;
                    Session.Address = address;
                    Session.UserType = userType;    // rolü de Session’a kaydet

                    userFound = true;
                    break;
                }
            }

            if (!userFound)
            {
                MessageBox.Show("Invalid email or password.");
                return;
            }

            users[0].openUser_management();
        }

        public void register(string email, string password, string name, string surname, string photoBase64)
        {
            if (user_list.Any(f => f[0].Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("There is a user with the same e-mail.");
                return;
            }

            var newUser = new string[13];
            newUser[0] = email;
            newUser[1] = password;
            newUser[2] = $"note_book{counter}.csv";
            newUser[3] = $"phone_book{counter}.csv";
            newUser[4] = $"personal_information{counter}.csv";
            newUser[5] = $"salary_calculator{counter}.csv";
            newUser[6] = $"reminder{counter}.csv";
            newUser[7] = photoBase64;
            newUser[8] = name;
            newUser[9] = surname;
            newUser[10] = "";  // phone
            newUser[11] = "";  // address
            newUser[12] = user_list.Count == 0 ? "Admin" : "User";

            user_list.Add(newUser);
            WriteToCSV();
        }

        private void ReadCounterFromFile()
        {
            if (!File.Exists(counterFilePath)) return;
            var txt = File.ReadAllText(counterFilePath);
            counter = int.TryParse(txt, out var c) ? c : 0;
        }

        private void WriteCounterToFile()
        {
            File.WriteAllText(counterFilePath, counter.ToString());
        }
    }
}
