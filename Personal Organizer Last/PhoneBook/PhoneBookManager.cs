using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personal_Organizer_Last.PhoneBook
{
    public abstract class PhoneBookManager
    {
        public abstract List<string[]> listRecord();
        public abstract void createRecord(string name, string surname, string phoneNumber, string email, string address, string description);
        public abstract void updateRecord(string name, string surname, string phoneNumber, string email, string address, string description, int index);
        public abstract void deleteRecord(string name, string surname, string phoneNumber, string email, string address, string description);

    }
}
