using System;
using System.Collections.Generic;
using System.Text;

namespace Hamburg_namespace
{
    class HamburgBoxException : SystemException
    {
        public HamburgBoxException(string message)
            : base(message)
        {
        }
    }
}
