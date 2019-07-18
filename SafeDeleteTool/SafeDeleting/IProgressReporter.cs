using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    public interface IProgressReporter
    {
        void Report(string message, double progress);

        void ReportFinish(string message, bool ok);
    }
}
