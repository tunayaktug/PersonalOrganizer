using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personal_Organizer_Last.NoteBook
{
    public abstract class NotesManagar
    {
        public abstract List<string[]> listNodes();
        public abstract void createNodes(string _title, string _note);
        public abstract void deleteNodes(string title, string note);
        public abstract void updateNodes(string title, string note, int index);

    }
}
