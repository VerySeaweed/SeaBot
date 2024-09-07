using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SeaBot;

internal static class Files
{
    internal static void WriteInFiles(string text, string path)
    {
        using StreamWriter Writer = new StreamWriter(path, true);
        Writer.WriteLine(text);
        Writer.Close();
    }
    internal static string ReadInFiles(string path)
    {
        string text;
        using (StreamReader Reader = new(path))
        {
            text = Reader.ReadToEnd();
            Reader.Close();
        }
        return text;
    }
    internal static void Create(string path)
    {
        using StreamWriter Creater = File.CreateText(path);
        Creater.Close();
    }
}
