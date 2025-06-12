using System;
using System.Collections.Generic;
using System.IO;

namespace Personal_Organizer_Last.NoteBook
{
    public class NotesController
    {
        private List<string[]> notes;    // Notlar
        private string filePath;         // Dosya yolu

        // Undo/Redo için yığınlar
        private Stack<List<string[]>> undoStack = new Stack<List<string[]>>();
        private Stack<List<string[]>> redoStack = new Stack<List<string[]>>();

        public NotesController(string filePath)
        {
            this.filePath = filePath;
            notes = new List<string[]>();
        }

        // Notları CSV dosyasından okur
        public void readFromCSV()
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                notes.Clear();
                foreach (string line in lines)
                {
                    string[] parts = line.Split('μ');
                    notes.Add(parts);
                }
            }
        }

        // Notları CSV dosyasına yazar
        public void writeToCSV()
        {
            List<string> lines = new List<string>();
            foreach (var item in notes)
            {
                lines.Add(string.Join("μ", item));  // Notları 'μ' ile ayırarak yazıyoruz
            }
            File.WriteAllLines(filePath, lines);
        }

        // Not ekler ve Undo yığınını günceller
        public void createNodes(string title, string content)
        {
            // Önceki durumu Undo yığınına ekle
            undoStack.Push(new List<string[]>(notes));

            // Yeni notu ekle
            notes.Add(new string[] { title, content });

            // Redo yığınına temizle
            redoStack.Clear();
        }

        // Notları listeler
        public List<string[]> listNodes()
        {
            return notes;
        }

        // Not siler ve Undo yığınını günceller
        public void deleteNodes(string title, string content)
        {
            // Önceki durumu Undo yığınına ekle
            undoStack.Push(new List<string[]>(notes));

            // Silinecek notu bulup sil
            for (int i = 0; i < notes.Count; i++)
            {
                string[] note = notes[i];
                if (note[0] == title && note[1] == content)
                {
                    notes.RemoveAt(i);
                    break;
                }
            }

            // Redo yığınına temizle
            redoStack.Clear();
        }

        // Notu günceller ve Undo yığınına ekler
        public void updateNodes(string title, string content, int index)
        {
            // Önceki durumu Undo yığınına ekle
            undoStack.Push(new List<string[]>(notes));

            // Seçilen notu güncelle
            notes[index][0] = title;
            notes[index][1] = content;

            // Redo yığınına temizle
            redoStack.Clear();
        }

        // Undo işlemi
        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(new List<string[]>(notes));  // Mevcut durumu redoStack'e ekle
                notes = undoStack.Pop();  // Önceki durumu al
            }
        }

        // Redo işlemi
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(new List<string[]>(notes));  // Mevcut durumu undoStack'e ekle
                notes = redoStack.Pop();  // Yeniden yapılacak işlemi al
            }
        }
    }
}
