using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace Petzold.RecordKeystrokes
{
    public class RecordKeystrokes : Window
    {
        string path = @"C:\Users\kosty\source\repos\sayhello\sayhello\in.txt";
        StringBuilder build = new StringBuilder();

        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new RecordKeystrokes());
        }

        public RecordKeystrokes()
        {
            Title = "Record Keystrokes";
            Content = build.ToString();
            // Запись при закрытии окна
            //Closed += (sender, e) => SaveToFile();
        }

        protected override void OnTextInput(TextCompositionEventArgs args)
        {
            base.OnTextInput(args);

            if (args.Text == "\b") // Backspace
            {
                if (build.Length > 0)
                    build.Remove(build.Length - 1, 1);
            }
            else if (build.Length < 3 && args.Text.All(char.IsDigit))
            {
                build.Append(args.Text);
            }

            Content = build.ToString();
            SaveToFile(); // Сохраняем после каждого изменения
        }

        void SaveToFile()
        {
            try
            {
                File.WriteAllText(path, build.ToString());
                Console.WriteLine($"Сохранено: {build}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи: {ex.Message}");
            }
        }
    }
}