using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hamburg_namespace
{
    /* General UserControl Enumerations */
    public enum PSU_RANGE : byte
    {
        _300V            = 0,
        _Plus_Minus_150V = 1,
    }


    /*General enums for boxes */
    public enum BOX_ADDRESS : byte
    {
        HamburgBox_1 = 0x01,

    }
    public enum BOX_CONN_STATE : byte
    {
        Connected = 0x00,
        Disconnected = 0x01,
        Connecting = 0x02,
        Disconecting = 0x03,
    }
    public enum PWR_STATE : byte
    {
        OFF = 0x00,
        ON = 0x01,
    }
    public enum HTR_PARAM_ID : byte
    {
        Heater_Param_ID_Kp = 0x00,
        Heater_Param_ID_Ki = 0x01,
        Heater_Param_ID_Kd = 0x02,
        Heater_Param_ID_Ts = 0x03,
        Heater_Param_ID_Tz = 0x04,
        Heater_Param_ID_T = 0x05,
    }


}
