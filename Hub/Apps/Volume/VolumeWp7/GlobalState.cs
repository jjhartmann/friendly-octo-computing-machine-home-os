using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;

namespace HomeOS.Hub.Apps.VolumeWp7
{
    public class GlobalState
    {
        static ConsoleMessages consoleMessages = new ConsoleMessages();
        static IsolatedStorageSettings confSettings = IsolatedStorageSettings.ApplicationSettings;

        public static void SetConfSetting(string key, object value)
        {
            if (confSettings.Contains(key))
            {
                confSettings[key] = value;
            }
            else
            {
                confSettings.Add(key, value);
            }
        }

        public static bool ContainsConfSetting(string key)
        {
            return confSettings.Contains(key);
        }

        public static object GetConfSetting(string key)
        {
            if (confSettings.Contains(key))
                return confSettings[key];
            else
                return null;
        }

        public static void AddConsoleMessage(string text)
        {
            consoleMessages.AddMessage(text);
        }

        public static string GetConsoleOutput()
        {
            return consoleMessages.ToString();
        }
    }

    public class ConsoleMessages
    {
        Queue<string> queue;
        int capacity;
        int messageNumber = 0;

        public ConsoleMessages(int capacity = 20)
        {
            queue = new Queue<string>(capacity);
            this.capacity = capacity;
        }

        public string AddMessage(string text)
        {
            while (queue.Count >= capacity)
                queue.Dequeue();

            queue.Enqueue(++messageNumber + ": " + text + "\n");

            return this.ToString();
        }

        public override string ToString()
        {
            string ret = "";

            for (int index = 0; index < queue.Count; index++)
            {
                ret += queue.ElementAt(index);
            }

            return ret;
        }
    }

}
