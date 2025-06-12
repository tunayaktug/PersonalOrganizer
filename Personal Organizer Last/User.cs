using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Personal_Organizer_Last
{
    public enum UserType
    {
        Admin,
        User,
        PartTimeUser,
    };

    public class User
    {
        private string Email;
        private string Password;
        private UserType UserType;
        private string name = "";
        private string surname = "";
        private string phoneNumber = "";
        private string address = "";
        private string[] personal_information_file;



        private UserManagement user_management;
        private string[] personal_file_path;

        
        


        public User(string _email, string _password, string v, string v1, string v2, UserType _userType, string[] _personal_file_path)
            //_personl_file_path içinde notebook phonebook ve personal info filepath kısmı saklanıyor
        {
            Email = _email;
            Password = _password;
            UserType = _userType;
            personal_file_path = _personal_file_path;
            string[] info = {Email, Password, UserType.ToString() };
            user_management = new UserManagement(info, personal_file_path);
            
           

        }

        public string getEmail()
        {
            return Email;
        }

        public string getPassword()
        {
            return Password;
        }

        public void openUser_management()
        {
            Form userForm = new Form();
            userForm.Controls.Add(user_management);
            user_management.Dock = DockStyle.Fill;

            // Form1 ile aynı boyut ve konumda aç (örnek boyut ve konum, istersen parametreyle al)
            userForm.StartPosition = FormStartPosition.CenterScreen;
            userForm.Size = new System.Drawing.Size(900, 600); // örnek değer
            userForm.FormBorderStyle = FormBorderStyle.FixedSingle; // isteğe bağlı: boyutlandırmayı engelle

            userForm.FormClosed += (s, e) => Application.Exit(); // kapatıldığında tüm uygulamayı kapat

            userForm.Show();
        }




    }
}
