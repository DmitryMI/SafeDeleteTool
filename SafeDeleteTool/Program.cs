using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SafeDeleteTool.SafeDeleting;

namespace SafeDeleteTool
{
    class Program
    {
        private static Printer _printer = new Printer();
        private static bool _suspend = true;

        class Reporter : IProgressReporter
        {
            private int _total;
            private int _errors;
            private int _successes;

            public int Total
            {
                get { return _total; }
            }

            public int Errors
            {
                get { return _errors; }
            }

            public int Successes
            {
                get { return _successes; }
            }

            public void Report(string message, double progress)
            {
                _printer.WriteLine("Progress of " +message + ": " + (progress * 100).ToString("0.00") + "%");
            }

            public void ReportFinish(string message, bool ok)
            {
                _total = Total + 1;
                if (!ok)
                {
                    _printer.WriteLineWithColor("FAIL: " + message, ConsoleColor.Red);

                    _errors = Errors + 1;
                }
                else
                {
                    _printer.WriteLineWithColor("OK: " + message, ConsoleColor.Green);
                    
                    _successes = Successes + 1;
                }
            }
        }

        class Logger : ILogger
        {
            public void Log(string format, params object[] vars)
            {
                string message = String.Format(format, vars);
                _printer.WriteLine(message);
            }
        }

        class Printer
        {
            private object locker = new object();

            /*public ConsoleColor GetTextColor()
            {
                lock (locker)
                {
                    return Console.ForegroundColor;
                }
            }

            public void SetTextColor(ConsoleColor color)
            {
                lock (locker)
                {
                    Console.ForegroundColor = color;
                }
            }*/

            public void WriteLine(string message)
            {
                lock (locker)
                {
                    Console.WriteLine(message);
                }
            }

            public void WriteLineWithColor(string message, ConsoleColor color)
            {
                lock (locker)
                {
                    ConsoleColor prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(message);
                    Console.ForegroundColor = prevColor;
                }
            }
        }

        static IList<FileInfo> ProcessArguments(string[] args)
        {
            List<FileInfo> resFileInfos = new List<FileInfo>();

            bool allowFolders = false;
            bool recursiveFolders = false;

            foreach (var arg in args)
            {
                if (arg.Equals("/f"))
                {
                    allowFolders = true;
                }
                else if (arg.Equals("/r"))
                {
                    recursiveFolders = true;
                }
                else if (arg.Equals("/nosuspend"))
                {
                    _suspend = false;
                }
                else
                {
                    FileAttributes attr = File.GetAttributes(arg);

                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(arg);
                        if (allowFolders)
                        {
                            int maxLevel = recursiveFolders ? 5 : 1;
                            List<FileInfo> files = new List<FileInfo>();
                            GetFilesRecursively(directoryInfo, files, 0, maxLevel);
                            resFileInfos.AddRange(files);
                        }
                        else
                        {
                            _printer.WriteLine(arg + " is a folder. To allow folder processing use key /f. For recursive folder processing also include /r");
                        }
                    }
                    else
                    {
                        FileInfo file = new FileInfo(arg);
                        resFileInfos.Add(file);
                    }
                }
            }

            return resFileInfos;
        }

        static void GetFilesRecursively(DirectoryInfo currentDirectory, IList<FileInfo> fileList, int level, int maxLevel)
        {
            if(level > maxLevel)
                return;
            
            IEnumerable<DirectoryInfo> subDirs = currentDirectory.EnumerateDirectories();
            IEnumerable<FileInfo> files = currentDirectory.EnumerateFiles();

            foreach (var file in files)
            {
                fileList.Add(file);
            }

            foreach (var subDir in subDirs)
            {
                GetFilesRecursively(subDir, fileList, level + 1, maxLevel);
            }
        }

        static void Main(string[] args)
        {
            IList<FileInfo> files = ProcessArguments(args);

            Reporter reporter = new Reporter();

            if (files != null && files.Count > 0)
            {
                SafeDeleteManager manager = new SafeDeleteManager(files);
                manager.SetProgressReporter(reporter);
                manager.SetReplacementProvider(new ZerosProvider());
                manager.SetLogger(new Logger());

                manager.ProcessAsynchronously();

                while (!manager.Finished)
                {
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine();

            _printer.WriteLine("TOTAL: " + reporter.Total);
            _printer.WriteLine("OK: " + reporter.Successes);
            _printer.WriteLine("FAILED: " + reporter.Errors);

            _printer.WriteLine("Press any key to finish...");

            if(_suspend)
                Console.ReadKey();
        }
    }
}
