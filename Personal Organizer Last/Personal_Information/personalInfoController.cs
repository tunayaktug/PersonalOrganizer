using System;
using System.Collections.Generic;
using System.IO;

namespace Personal_Organizer_Last.Personal_Information
{
    public class personalInfoController
    {
        private string[] info;
        private string filePath;
        private List<string[]> infoList;


        // Undo/Redo için eklenen yığınlar
        private Stack<string[]> undoStack = new Stack<string[]>();
        private Stack<string[]> redoStack = new Stack<string[]>();

        public personalInfoController(string[] info, string filePath)
        {
            this.info = info;
            this.filePath = filePath;
            this.infoList = new List<string[]>();
        }

        public void readFromCSV()
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                infoList.Clear();
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    infoList.Add(parts);
                }
            }
        }

        public void writeToCSV()
        {
            List<string> lines = new List<string>();
            foreach (var item in infoList)
            {
                lines.Add(string.Join(",", item));
            }
            File.WriteAllLines(filePath, lines);
        }

        public void addAllInfo(string[] newInfo)
        {
            if (infoList.Count > 0)
            {
                // Mevcut bilgiyi Undo yığınına ekle
                undoStack.Push((string[])infoList[0].Clone());
            }

            
            redoStack.Clear();

            infoList.Clear();
            infoList.Add(newInfo);
        }

        public List<string[]> listInformation()
        {
            return infoList;
        }

        public string getSalaryValue()
        {
            if (infoList.Count > 0 && infoList[0].Length > 8)
            {
                return infoList[0][8];
            }
            return "0";
        }

        // >>> Undo işlemi <<<
        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                string[] current = infoList.Count > 0 ? infoList[0] : new string[0];
                redoStack.Push((string[])current.Clone());
                infoList.Clear();
                infoList.Add(undoStack.Pop());
            }
        }

        // >>> Redo işlemi <<<
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                string[] current = infoList.Count > 0 ? infoList[0] : new string[0];
                undoStack.Push((string[])current.Clone());
                infoList.Clear();
                infoList.Add(redoStack.Pop());
            }
        }
    }
}
