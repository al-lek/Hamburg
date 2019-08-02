using System;
using System.Collections.Generic;
using System.Text;

namespace Hamburg_namespace
{
    class InstrumentInterfaceException : SystemException
    {
        public InstrumentInterfaceException(string message)
            : base(message)
        {
        }
    }
}
