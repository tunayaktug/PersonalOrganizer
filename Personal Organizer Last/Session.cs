using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Personal_Organizer_Last
{
    public static class Session
    {
        public static string FullName { get; set; } // Ad + Soyad
        public static Image UserPhoto { get; set; } // Profil fotoğrafı
 
        public static string Name { get; set; }
        public static string Surname { get; set; }
        public static string Phone { get; set; }
        public static string Address { get; set; }
        public static string Email { get; set; }
        public static UserType UserType { get; set; }
    }
}
