using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Globalization;
using System.Timers;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using FTD2XX_NET;


namespace Hamburg_namespace
{
    public enum INSTRUMENT_CONN_STATE : byte
    {
        Connected = 0x00,
        Disconnected = 0x01,
    }
    public static class InstrumentCtrlInterface
    {

        #region Object Declarations

        private static readonly object _locker = new object();
        public static modbus mb = new modbus();
        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static System.Diagnostics.Stopwatch stopWatch2 = new System.Diagnostics.Stopwatch();

        public static HamburgBoxInterface HamburgBox_1 = new HamburgBoxInterface(mb, _locker, BOX_ADDRESS.HamburgBox_1);
        public static object[] objArray = new object[2]; 
   
        #endregion

        #region Variable Declarations

        public static FTDI FTDIPort;
        public static String[] Device_description;
        public static INSTRUMENT_CONN_STATE InstrumentCtrlStatus = INSTRUMENT_CONN_STATE.Disconnected;
        #endregion

        #region Global Events Declarations

        public delegate void ConnectionState_GLB_EventHandler(INSTRUMENT_CONN_STATE State);
        public static event ConnectionState_GLB_EventHandler ConnectionState_GLB_Event;

        public delegate void PowerStateChange_GLB_EventHandler(PWR_STATE State);
        public static event PowerStateChange_GLB_EventHandler PowerStateChange_GLB_Event;
        #endregion

        #region Low Level Update Events Declarations

        public delegate void HamburgBoxAdcValues_UPD_EventHandler(BOX_ADDRESS x);
        public static event HamburgBoxAdcValues_UPD_EventHandler HamburgBoxAdcValues_UPD_Event;

        public delegate void HamburgBoxPowerStates_UPD_EventHandler(BOX_ADDRESS x);
        public static event HamburgBoxPowerStates_UPD_EventHandler HamburgBoxPowerStates_UPD_Event;

        public delegate void HamburgBoxTurboStatus_UPD_EventHandler(BOX_ADDRESS x);
        public static event HamburgBoxTurboStatus_UPD_EventHandler HamburgBoxTurboStatus_UPD_Event;

        public delegate void HamburgBoxTriggerParams_UPD_EventHandler(BOX_ADDRESS x);
        public static event HamburgBoxTriggerParams_UPD_EventHandler HamburgBoxTriggerParams_UPD_Event;

        public delegate void HamburgBoxHtrParams_UPD_EventHandler(BOX_ADDRESS x);
        public static event HamburgBoxHtrParams_UPD_EventHandler HamburgBoxHtrParams_UPD_Event;



        #endregion

        #region Thread Declaration
        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        public static volatile bool _shouldStop = false;

        /* UPDATE THREAD
         * Started in Connect(), Aborted in Disconnect()
         */
        public static Thread LOW_LEVEL_UPDATE;
        #endregion

        #region GetAvailiablePorts / Connect / Disconnect / GlobalPowerOn / GlobalPowerOff

        /* Fills in the array with the names of availiable serial ports
         * 
         * void GetAvailiablePorts(ref string[] ports)
         */
        public static string[] GetAvailiablePorts()
        {
            mb.Check_for_devices(ref Device_description);
            return (Device_description);

        }

        /* Connects with Modbus.
         * 
         * public void connect(string portName)
         */
        public static void Connect()
        {
            if (mb.Check_for_devices(ref Device_description) == CE_FTDIStatus.CE_FTDI_Ok)
            {
                if (mb.Open_Device(Device_description[0]) == CE_FTDIStatus.CE_FTDI_Ok)
                {
                    lock (_locker)
                    {
                        mb.Purge_Buffers();
                        try
                        {
                            HamburgBox_1.MB_Connect();
                            LOW_LEVEL_UPDATE = new Thread(LowLevelUpdate);
                            LOW_LEVEL_UPDATE.IsBackground = true;
                            LOW_LEVEL_UPDATE.Start();
                            while (!LOW_LEVEL_UPDATE.IsAlive) ;
                            Thread.Sleep(1);
                            HamburgBox_1.SetConnectionStatus(BOX_CONN_STATE.Connecting);
                            InstrumentCtrlStatus = INSTRUMENT_CONN_STATE.Connected;
                            ConnectionState_GLB_Event(InstrumentCtrlStatus);
                        }
                        catch (HamburgBoxException ex)
                        {
                            mb.Close_Device();
                            throw new InstrumentInterfaceException(ex.Message);
                        }
                    }
                }
                else
                {
                    InstrumentCtrlStatus = INSTRUMENT_CONN_STATE.Disconnected;
                    throw new InstrumentInterfaceException("InstrumentCtrl _Connect_ Error : OpenDevice failure");
                }
            }
            else
            {
                InstrumentCtrlStatus = INSTRUMENT_CONN_STATE.Disconnected;
                throw new InstrumentInterfaceException("InstrumentCtrl _Connect_ Error : DeviceNotFound failure");
            }
        }

        /* Disconnects from Hamburg Box
         * 
         * public void Disconnect()
         */
        public static void Disconnect()
        {
            HamburgBox_1.SetConnectionStatus(BOX_CONN_STATE.Disconecting);
            RequestStop();
        }

        /* Trigger Global Power On Event
         * 
         */
        public static void GlobalPowerOn()
        {
            if (PowerStateChange_GLB_Event != null)
            {
                PowerStateChange_GLB_Event(PWR_STATE.ON);
            }
        }

        /* Trigger Global Power Off Event
         * 
         */
        public static void GlobalPowerOff()
        {
            if (PowerStateChange_GLB_Event != null)
            {
                PowerStateChange_GLB_Event(PWR_STATE.OFF);
            }
        }

        #endregion

        #region UPDATE
        /* public void RequestStop()
         * 
         */
        public static void RequestStop()
        {
            _shouldStop = true;

        }

        /* Low Level Update Thread
         * 
         */
        private static void LowLevelUpdate()
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            while (!_shouldStop)
            {
                
                Thread.Sleep(1000);                                                        // Wait for 1 sec between updates. At startup wait for interface class to be connected and then ask for readbacks
                if (InstrumentCtrlStatus == INSTRUMENT_CONN_STATE.Connected)
                {
                    
                    #region HamburgBox_1
                    //Update ADCs Readback Values  
                    try
                    {
                        HamburgBox_1.MB_getAdcValues();
                        if (HamburgBoxAdcValues_UPD_Event != null)
                        {
                            HamburgBoxAdcValues_UPD_Event(HamburgBox_1.getAddress());                   // inform all controlls that implement this event
                        }
                    }
                    catch (HamburgBoxException ex)
                    {
                        throw new InstrumentInterfaceException(ex.Message);
                    } 
                    //Update powerStates
                    try
                    {
                        HamburgBox_1.MB_getPowerStates();
                        if (HamburgBoxPowerStates_UPD_Event != null)
                        {
                            HamburgBoxPowerStates_UPD_Event(HamburgBox_1.getAddress());                   // inform all controlls that implement this event
                        }
                    }
                    catch (HamburgBoxException ex)
                    {
                        throw new InstrumentInterfaceException(ex.Message);
                    }
                    /*Update TurboStatus
                    try
                    {
                        HamburgBox_1.MB_getTurboStatus();
                        if (HamburgBoxTurboStatus_UPD_Event != null)
                        {
                            HamburgBoxTurboStatus_UPD_Event(HamburgBox_1.getAddress());                   // inform all controlls that implement this event
                        }
                    }
                    catch (HamburgBoxException ex)
                    {
                        throw new InstrumentInterfaceException(ex.Message);
                    }
                     * */
                    //Update Trigger Params
                    try
                    {
                        HamburgBox_1.MB_getTriggerParams();
                        if (HamburgBoxTriggerParams_UPD_Event != null)
                        {
                            HamburgBoxTriggerParams_UPD_Event(HamburgBox_1.getAddress());                   // inform all controlls that implement this event
                        }
                    }
                    catch (HamburgBoxException ex)
                    {
                        throw new InstrumentInterfaceException(ex.Message);
                    }
                  
                    //Update Heater Params
                    try
                    {

                        HamburgBox_1.MB_getHtrParams(HamburgBOX_HTR_ID.HTR1_ID);
                        HamburgBox_1.MB_getHtrParams(HamburgBOX_HTR_ID.HTR2_ID);
                        if (HamburgBoxHtrParams_UPD_Event != null)
                        {
                            HamburgBoxHtrParams_UPD_Event(HamburgBox_1.getAddress());                   // inform all controlls that implement this event
                        }
                    }
                    catch (HamburgBoxException ex)
                    {
                        throw new InstrumentInterfaceException(ex.Message);
                    }
                    #endregion

                }
            }
                   
            #region Close Ports and terminate
            try
            {
                lock (_locker)
                {
                    mb.Close_Device();
                    HamburgBox_1.SetConnectionStatus(BOX_CONN_STATE.Disconnected);
                    InstrumentCtrlStatus = INSTRUMENT_CONN_STATE.Disconnected;
                    ConnectionState_GLB_Event(InstrumentCtrlStatus);
                }
            }
            catch (ModbusException ex)
            {
                throw new InstrumentInterfaceException("InstrumentCtrl _Disconnect_ Error: " + ex.Message);
            }
            #endregion
            _shouldStop = false;
        }
        #endregion
    }
}

