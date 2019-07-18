using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    public interface ILogger
    {
        void Log(string format, params object[] vars);
    }
}
