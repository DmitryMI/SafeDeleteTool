using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SafeDeleteTool.SafeDeleting
{
    class CryptoRandomProvider : IReplacementProvider
    {
        public void FillBuffer(byte[] buffer, int length)
        {
            RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();

            cryptoServiceProvider.GetBytes(buffer, 0, length);
        }
    }
}
