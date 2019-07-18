using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafeDeleteTool.Workers
{
    public class WorkManager
    {
        private int _maxThreads;
        private Queue<Job> _actionList = new Queue<Job>();

        private int _activeActions;

        private struct Job
        {
            public Action<object> Action;
            public object Argument;
        }

        public WorkManager()
        {
            _maxThreads = Int32.MaxValue;
        }

        public WorkManager(int maxThreadCount)
        {
            _maxThreads = maxThreadCount;
        }

        public void RegisterAction(Action<object> action, object arg)
        {
            if (IsReady())
            {
                Job job = new Job(){Action = action, Argument = arg};
                _actionList.Enqueue(job);
            }
            else
            {
                throw new InvalidOperationException("Worker is busy");
            }
        }

        public bool Ready => IsReady();

        public void Launch()
        {
            LaunchInternal();
        }

        private bool IsReady()
        {
            return _activeActions == 0;
        }

        public int ActiveActionsCount => _activeActions;

        private void LaunchInternal()
        {
            while (_actionList.Count > 0 && _activeActions < _maxThreads)
            {
                ScheduleWork();
            }
        }

        private void DoJob(object jobWrapper)
        {
            Job job = (Job)jobWrapper;
            try
            {
                job.Action(job.Argument);
                ReportActionFinished();
                return;
            }
            catch (Exception)
            {
                ReportActionFailed();
                //Console.WriteLine(e);
                throw;
            }
        }
        private void ReportActionFinished()
        {
            _activeActions--;
            if (_activeActions < _maxThreads && _actionList.Count > 0)
            {
                ScheduleWork();
            }
        }
        private void ReportActionFailed()
        {
            _activeActions--;

            if (_activeActions < _maxThreads && _actionList.Count > 0)
            {
                ScheduleWork();
            }
        }

        private void ScheduleWork()
        {
            Job job = _actionList.Dequeue();
            Task task = new Task(DoJob, job);
            task.Start();
            _activeActions++;
        }
    }
}
