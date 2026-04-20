using System.Diagnostics;

namespace panpan
{
    static class Log
    {
        private static string tag = "panpan";
        private static List<string> log = new();
        public static void Info<T>(T message, string? tag = null, ConsoleColor? col = null)
        {
            col ??= ConsoleColor.Cyan;
            Console.ForegroundColor = col.Value;
            tag ??= Log.tag;
            string lm = $"[{tag}] - {message}";
            log.Add(lm);
            Console.WriteLine(lm);
            Console.ResetColor();
        }
        public static void Error(string message, string? tag = null)
        {
            tag ??= Log.tag;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            Log.Info($"{message}\n{trace}",$"{tag}-ERROR", ConsoleColor.Red);
        }
        
        public static void Warn(string message, string? tag = null)
        {
            tag ??= Log.tag;
            Log.Info($"{message}",$"{tag}-WARNING", ConsoleColor.DarkYellow);
        }

        public static void Assert(bool condition, string message, string? tag = null)
        {
            if(!condition)
            {
                Error(message, tag);
            }
        }

        public static string GetLog()
        {
            string l = string.Join("\n", log);
            return l;
        }

        public static void WriteLogToFile()
        {
            
        }
    } 
}