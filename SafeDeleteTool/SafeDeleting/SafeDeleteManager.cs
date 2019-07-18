using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SafeDeleteTool.Workers;

namespace SafeDeleteTool.SafeDeleting
{
    public class SafeDeleteManager
    {
        private List<string> _FilePaths;
        private IEnumerable<FileInfo> _fileInfos;
        private IProgressReporter _reporter;
        private IReplacementProvider _replacementProvider;
        private ILogger _logger;
        private WorkManager _workManager;

        public SafeDeleteManager(IEnumerable<string> filePaths)
        {
            _FilePaths = new List<string>(filePaths);
        }

        public SafeDeleteManager(IEnumerable<FileInfo> files)
        {
            _fileInfos = files;
        }

        public void SetProgressReporter(IProgressReporter reporter)
        {
            _reporter = reporter;
        }

        public void SetReplacementProvider(IReplacementProvider provider)
        {
            _replacementProvider = provider;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        private bool IsAsyncReady()
        {
            if (_workManager == null)
                return true;

            return _workManager.Ready;
        }

        public bool Finished => IsAsyncReady();


        private void Log(string format, params object[] vars)
        {
            _logger?.Log(format, vars);
        }

        public void ProcessSynchronously()
        {
            if (_fileInfos == null)
            {
                foreach (var fileName in _FilePaths)
                {
                    FileInfo fileInfo = new FileInfo(fileName);

                    if (fileInfo.Exists)
                    {
                        ProcessFileInfo(fileInfo);
                    }
                    else
                    {
                        Log("File {0} does not exits. Skipping.", fileInfo.Name);
                    }
                }
            }
            else
            {
                foreach (var fileInfo in _fileInfos)
                {
                    if (fileInfo.Exists)
                    {
                        ProcessFileInfo(fileInfo);
                    }
                    else
                    {
                        Log("File {0} does not exits. Skipping.", fileInfo.Name);
                    }
                }
            }
        }

        public void ProcessAsynchronously()
        {
            if(!IsAsyncReady())
                throw new InvalidOperationException("Async work in progress!");

            
            if (_fileInfos == null)
            {
                Queue<FileInfo> fileInfos = new Queue<FileInfo>(_FilePaths.Count);
                foreach (var fileName in _FilePaths)
                {
                    FileInfo fileInfo = new FileInfo(fileName);
                    if (fileInfo.Exists)
                        fileInfos.Enqueue(fileInfo);
                    else
                    {
                        Log("File {0} does not exits. Skipping.", fileInfo.Name);
                    }
                }
            }
            else
            {
                Queue<FileInfo> fileInfos = new Queue<FileInfo>(_fileInfos.Count());
                foreach (var fileInfo in _fileInfos)
                {
                    if (fileInfo.Exists)
                        fileInfos.Enqueue(fileInfo);
                    else
                    {
                        Log("File {0} does not exits. Skipping.", fileInfo.Name);
                    }
                }
            }

            Workers.WorkManager workManager = new WorkManager(Environment.ProcessorCount);

            foreach (var fileInfo in _fileInfos)
            {
                workManager.RegisterAction(ProcessFileInfoAsync, fileInfo);
            }

            workManager.Launch();

            _workManager = workManager;
        }

        private void ProcessFileInfoAsync(object wrappedFileInfo)
        {
            ProcessFileInfo(wrappedFileInfo as FileInfo);
        }

        private void ProcessFileInfo(FileInfo fileInfo)
        {
            SafeDeleteExecutor executor = new SafeDeleteExecutor(fileInfo, _reporter);
            if (_replacementProvider != null)
                executor.SetReplacementProvider(_replacementProvider);
            if (_logger != null)
                executor.SetLogger(_logger);

            executor.Execute();
        }
    }
}
