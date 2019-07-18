using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    class SafeDeleteExecutor
    {
        public const int BufferSize = 1024 * 1024;

        private FileInfo _fileInfo;
        private IProgressReporter _progressReporter;
        private IReplacementProvider _replacementProvider;
        private ILogger _logger;

        public SafeDeleteExecutor(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _replacementProvider = new ZerosProvider();
        }

        public SafeDeleteExecutor(FileInfo fileInfo, IProgressReporter progressReporter)
        {
            _fileInfo = fileInfo;
            _replacementProvider = new ZerosProvider();
            _progressReporter = progressReporter;
        }

        public void SetReplacementProvider(IReplacementProvider provider)
        {
            _replacementProvider = provider;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Execute()
        {
            ExecuteInternal();
        }

        private void Log(string format, params object[] vars)
        {
            if(_logger == null)
                return;
            
            _logger.Log(format, vars);
        }


        private void ExecuteInternal()
        {
            try
            {
                FileStream stream = _fileInfo.OpenWrite();

                int length = (int) stream.Length;

                byte[] buffer = new byte[BufferSize];

                for (int i = 0; i < length; i++)
                {
                    int iterationLength = Math.Min(length - i, BufferSize);
                    _replacementProvider.FillBuffer(buffer, (int) iterationLength);

                    stream.Write(buffer, 0, iterationLength);
                    i += iterationLength;

                    if (_progressReporter != null)
                    {
                        double progress = (double) i / length;
                        _progressReporter.Report(_fileInfo.Name, progress);
                    }
                }

                stream.Flush();
                stream.Close();

                _fileInfo.Delete();

                if (_progressReporter != null)
                {
                    _progressReporter.ReportFinish(_fileInfo.Name, true);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Log("ERROR: Access denied to file " + _fileInfo.Name);
                if (_progressReporter != null)
                {
                    _progressReporter.ReportFinish(_fileInfo.Name, false);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                Log("ERROR: File " + _fileInfo.Name + " does not exist");
                if (_progressReporter != null)
                {
                    _progressReporter.ReportFinish(_fileInfo.Name, false);
                }
            }
            
        }
    }
}
