using Lagrange.Core.Message.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot
{
    enum EMessageLevel
    {
        error,
        warning,
        info,
    }
    internal class Logger
    {
        public ConsoleColor ErrorColor;
        public ConsoleColor WarningColor;
        public ConsoleColor InfoColor;

        public Logger()
        {
            this.ErrorColor = ConsoleColor.Red;
            this.WarningColor = ConsoleColor.Yellow;
            this.InfoColor = ConsoleColor.Green;
        }

        public void Info(string message, string callingName)
        {
            lock (new object())
            {
                this.Log(message, EMessageLevel.info, callingName);
            }
        }
        public void Warning(string message, string callingName)
        {
            lock (new object())
            {
                this.Log(message, EMessageLevel.warning, callingName);
            }
        }
        public void Error(string message, string callingName)
        {
            lock (new object())
            {
                this.Log(message, EMessageLevel.error, callingName);
            }
        }
        public void Log(string message, EMessageLevel level, string callingName)
        {
            string levels = "DEBUG";
            switch (level)
            {
                case EMessageLevel.error:
                    Console.ForegroundColor = this.ErrorColor;
                    levels = "ERROR";
                    break;
                case EMessageLevel.warning:
                    Console.ForegroundColor = this.WarningColor;
                    levels = "WARNING";
                    break;
                case EMessageLevel.info:
                    Console.ForegroundColor = this.InfoColor;
                    levels = "INFO";
                    break;
            }
            Console.Write(levels);
            Console.ForegroundColor = ConsoleColor.White;
            string date = " [" + DateTime.Now.ToString("HH:mm:ss.ffff") + "] ";
            Console.WriteLine(date + "[" + callingName + "] " + message);
            lock (new object())
            {
                LogIntoFile(levels + date + "[" + callingName + "] " + message);
            }
        }
        protected void LogIntoFile(string message)
        {
            Files.WriteInFiles(message, @"log.txt");
        }
    }
}
