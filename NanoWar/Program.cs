namespace NanoWar
{
    using System;
    using System.IO;

    internal class Program
    {
        private static int Main(string[] args)
        {
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    var sw = new StreamWriter("logs.txt", true);
                    var ex = eventArgs.ExceptionObject as Exception;
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n" + ex.Message + "\n"
                        + (ex.InnerException != null ? ex.InnerException.Message : string.Empty) + "\n\n"
                        + ex.StackTrace + "\r\n\r\n\r\n");
                    sw.Close();
                };
#endif

            Game.Instance.Start();
            return 0;
        }
    }
}