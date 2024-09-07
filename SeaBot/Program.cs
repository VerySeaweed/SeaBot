namespace SeaBot
{
    internal class Program
    {
        public static Bot? Bot; 

        static void Main(string[] args)
        {

            var bot = new Bot();
            Bot = bot;
            bot.Start();

        }
    }
}
