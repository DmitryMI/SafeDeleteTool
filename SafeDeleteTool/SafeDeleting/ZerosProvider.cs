using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    class ZerosProvider : IReplacementProvider
    {
        public void FillBuffer(byte[] buffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                buffer[i] = 0;
            }
        }
    }
}
