using System.Text.Json;

namespace SongBaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string base_path = "guess_song_base";
            if (!Directory.Exists(base_path))
            {
                Directory.CreateDirectory(base_path);
            }
            string tempstr = Files.ReadInFiles(Console.ReadLine());
            var temp = new SongBase();
            string[] strings_1 = tempstr.Split('\n');
            for (int i = 0; i < strings_1.Length; i++)
            {
                string[] strs = strings_1[i].Split(';');
                temp.Songs.Add(new Song()
                {
                    Name = strs[0],
                    Composer = strs[1]
                });
            }
            Console.WriteLine("name:");
            string name = Console.ReadLine();
            temp.Name = name;
            string path = Path.Combine(base_path, name + ".json");
            Files.Create(path);
            Files.WriteInFiles(JsonSerializer.Serialize(temp), path);
        }

        public class SongBase
        {
            public string Name { get; set; }
            public List<Song> Songs { get; set; } = new();
            public SongBase()
            {
                this.Name = string.Empty;
            }
        }

        public class Song
        {
            public string Name { get; set; }
            public string Composer { get; set; }
            public Song()
            {
                this.Name = string.Empty;
                this.Composer = string.Empty;
            }
        }
    }
}
