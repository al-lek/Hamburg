using System;
using System.Collections.Generic;
using System.Text;

namespace Hamburg_namespace
{
    class ModbusException : SystemException
    {
        public ModbusException(string message)
            : base(message)
        {
        }
    }
}
