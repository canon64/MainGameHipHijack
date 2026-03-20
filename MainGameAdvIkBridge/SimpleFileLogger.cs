using System;
using System.IO;
using System.Text;

namespace MainGameAdvIkBridge
{
    internal sealed class SimpleFileLogger
    {
        private readonly string _path;
        private readonly object _lockObj = new object();

        public SimpleFileLogger(string path, bool resetOnStart)
        {
            _path = path;

            string dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (resetOnStart)
                File.WriteAllText(_path, string.Empty, Encoding.UTF8);
        }

        public void Write(string level, string message)
        {
            lock (_lockObj)
            {
                string line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [" + level + "] " + message + Environment.NewLine;
                File.AppendAllText(_path, line, Encoding.UTF8);
            }
        }
    }
}
