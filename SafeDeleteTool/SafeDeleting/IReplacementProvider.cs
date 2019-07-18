using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    public interface IReplacementProvider
    {
        void FillBuffer(byte[] buffer, int length);
    }
}
