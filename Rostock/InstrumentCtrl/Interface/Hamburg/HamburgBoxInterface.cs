using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Hamburg_namespace
{

//*****************************************************************************//
//-------------------------------ENUMERATIONS----------------------------------//
//*****************************************************************************//

    /* Hamburg box specific enums*/
    public enum HamburgBOX_DAC_ID : byte
    {
        DAC_ID_RF2		    	= 0x00,
        DAC_ID_RF1			    = 0x01,
        DAC_ID_PSU800V	    	= 0x02,
        DAC_ID_OUT1A_SLOT1		= 0x03,
        DAC_ID_OUT1B_SLOT1		= 0x04,
        DAC_ID_OUT2A_SLOT1		= 0x05,
        DAC_ID_OUT2B_SLOT1		= 0x06,
        DAC_ID_OUT3A_SLOT1		= 0x07,
        DAC_ID_OUT3B_SLOT1		= 0x08,
        DAC_ID_OUT4A_SLOT1		= 0x09,
        DAC_ID_OUT4B_SLOT1		= 0x0A,
        DAC_ID_OUT1_SLOT2		= 0x0B,
        DAC_ID_OUT2_SLOT2		= 0x0C,
        DAC_ID_OUT3_SLOT2		= 0x0D,
        DAC_ID_OUT4_SLOT2		= 0x0E,
        DAC_ID_OUT1_SLOT3		= 0x0F,
        DAC_ID_OUT2_SLOT3		= 0x10,
        DAC_ID_OUT3_SLOT3		= 0x11,
        DAC_ID_OUT4_SLOT3		= 0x12,
        DAC_ID_OUT1_SLOT4	    = 0x13,
        DAC_ID_OUT2_SLOT4		= 0x14,
        DAC_ID_OUT3_SLOT4		= 0x15,
        DAC_ID_OUT4_SLOT4		= 0x16,   
        
    }
    public enum HamburgBOX_ADC_ID : byte 
    {
        ADC_ID_PRG_Val4         = 0x00,
        ADC_ID_PRG_Val3         = 0x01,
        ADC_ID_PRG_Val2         = 0x02,
        ADC_ID_PRG_Val1         = 0x03,
        ADC_ID_PRG_Id4          = 0x04,
        ADC_ID_PRG_Id3          = 0x05,
        ADC_ID_PRG_Id2          = 0x06,
        ADC_ID_PRG_Id1          = 0x07,
        ADC_ID_Vrdb_RF1		    = 0x08,
        ADC_ID_Vrdb_RF2		    = 0x09,
        ADC_ID_Vrdb_PSU800V     = 0x0A,
        ADC_ID_OUT1_SLOT1	    = 0x0B,
        ADC_ID_OUT2_SLOT1	    = 0x0C,
        ADC_ID_OUT3_SLOT1	    = 0x0D,
        ADC_ID_OUT4_SLOT1	    = 0x0E,
        ADC_ID_OUT1_SLOT2	    = 0x0F,
        ADC_ID_OUT2_SLOT2	    = 0x10,
        ADC_ID_OUT3_SLOT2	    = 0x11,
        ADC_ID_OUT4_SLOT2	    = 0x12,
        ADC_ID_OUT1_SLOT3	    = 0x13,
        ADC_ID_OUT2_SLOT3	    = 0x14,
        ADC_ID_OUT3_SLOT3	    = 0x15,
        ADC_ID_OUT4_SLOT3	    = 0x16,
        ADC_ID_OUT1_SLOT4	    = 0x17,
        ADC_ID_OUT2_SLOT4	    = 0x18,
        ADC_ID_OUT3_SLOT4	    = 0x19,
        ADC_ID_OUT4_SLOT4	    = 0x1A,
        ADC_ID_Turbo1		    = 0x1B,
        ADC_ID_Turbo2		    = 0x1C,
    }                           
    public enum HamburgBOX_RLY_ID : byte 
    {
        RLY_ID_PSU800V      = 0x00,
        RLY_ID_M30D200V     = 0x01,
        RLY_ID_M30S300V     = 0x02,
        RLY_ID_RF1          = 0x03,
        RLY_ID_RF2          = 0x04,
        RLY_ID_ROTARY1      = 0x05,
        RLY_ID_ROTARY2      = 0x06,
        RLY_ID_TURBO1       = 0x07,
        RLY_ID_TURBO2       = 0x08,
        RLY_ID_SPARE        = 0x09,
        RLY_ID_HEATER1      = 0x0A,
        RLY_ID_HEATER2      = 0x0B,
    }
    public enum HamburgBOX_HTR_ID : byte
    {
        HTR1_ID = 0x00,
        HTR2_ID = 0x01,
    }
    public enum Hamburg_TRIGGER_MODE : byte
    {
        Internal = 0x00,
        External = 0x01,
    }
    public enum HamburgBOX_DMCU : uint
    {
        DMCU1 = 0x00,
        DMCU2 = 0x01,
        DMCU3 = 0x02,
    }

//*****************************************************************************//
//-------------------------------CLASS DEFINITION------------------------------//
//*****************************************************************************//

    public class HamburgBoxInterface{
        private static byte[] RTUArray;

        private modbus mb;
        //--------- CHANGE THIS ONLY ----------//////////////////////////////////////////////////////////////////////////////
        private const int MaxPDUSize = 218;             //FCODE+DATA = 1BYTE + xDATA BYTES +1BYTE (needed in Modbus code)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //--- NO CHANGE HERE ---//
        private const int MaxRTUSize = MaxPDUSize +3;
        private const int MinRTUSize = 4;             //ADDRESS(1BYTE) + FCODE(1BYTE) + DATA(1BYTE) + CRC(2BYTES)  = 4BYTES + 1DATA BYTE      
        private const int AddrOFF = 0;
        private const int FcodeOFF = 1;
        private const int DataOFF = 2;
        /////////////////////////////////////////
        private readonly object _locker;                                                        //locker
        private static System.Timers.Timer HamburgIntTimer = new System.Timers.Timer(500);   //Readback update timer
        public BOX_CONN_STATE Connection_Status = BOX_CONN_STATE.Disconnected;    //Box connection status variable
       
        // Data Variables //
        private BOX_ADDRESS Address;                    //Box address variable
        /**** Readback DATA ****/
        private ushort[] AdcValue = new ushort[29];
        private ushort RLYsPowerStates;                 // 16 bit vector - each bit one RLY state. 
        private uint[,] DMCUParam = new uint[3, 18];     //(x3 DMCUs * x18 32bit_Params (6 sequences))  
        private Hamburg_TRIGGER_MODE TriggerMode;
        private ushort TrigICR_Value;
        private byte TriggerRunningFlag;
        private ushort[,] HTRParam = new ushort[2, 6];  // x2 heaters - x6 (16bit) parameters
        private byte Turbo1_status, Turbo2_status;

        // Saved & Sent DATA (they are not verified with readbacks, just saved here and sent over to the slaves)
        private ushort[] DacValue = new ushort[23];  

        private enum FunctionCodes : byte{
            MB_Connect				= 0x01,
            MB_Reset				= 0x02,
            MB_getAdcValues         = 0x03,
            MB_updDacValue          = 0x04,
            MB_powerOn              = 0x05,
            MB_powerOff             = 0x06,
            MB_setPowerStates       = 0x07,
            MB_getPowerStates       = 0x08,
            MB_updDelayParams       = 0x09,
            MB_getDelayParams       = 0x0A,
            MB_updTriggerMode       = 0x0B,
            MB_getTriggerParams     = 0x0C,
            MB_startTrigger         = 0x0D,
            MB_stopTrigger          = 0x0E,
            MB_updHtrParameter      = 0x0F,
            MB_updHtrParams         = 0x10,
            MB_getHtrParameter      = 0x11,
            MB_getHtrParams         = 0x12,
            MB_getTurboStatus       = 0x13,
        }

        #region Constructors / Deconstructor
        public HamburgBoxInterface(modbus modb, Object LockObject, BOX_ADDRESS boxAddr)
        {
            mb = modb;
            RTUArray = new byte[MaxRTUSize];
            _locker = LockObject;
            HamburgIntTimer.Elapsed += new ElapsedEventHandler(HamburgTimerEvent);
            Address = boxAddr;
        }
        public HamburgBoxInterface()
        { }
        ~HamburgBoxInterface() { }
        #endregion

        #region Set connection Status
        /* Set connection status to disconnect imediatly or to connect after 2 seconds from the call
         * 
         */
        public void SetConnectionStatus(BOX_CONN_STATE newState)
        {
            if (Connection_Status == BOX_CONN_STATE.Disconecting)
            {
                if (newState == BOX_CONN_STATE.Disconnected)
                {
                    lock (_locker)
                    {
                        Connection_Status = BOX_CONN_STATE.Disconnected;
                    }
                }
            }

            else if (Connection_Status == BOX_CONN_STATE.Disconnected)
            {
                if (newState == BOX_CONN_STATE.Connecting)
                {
                    HamburgIntTimer.Enabled = true;
                    lock (_locker)
                    {
                        Connection_Status = BOX_CONN_STATE.Connecting;
                    }
                }
            }

            else if (Connection_Status == BOX_CONN_STATE.Connecting)
            {
                if (newState == BOX_CONN_STATE.Disconecting || newState == BOX_CONN_STATE.Disconnected)
                {
                    HamburgIntTimer.Enabled = false;
                    lock (_locker)
                    {
                        Connection_Status = newState;
                    }
                }
            }

            else if (Connection_Status == BOX_CONN_STATE.Connected)
            {
                if (newState == BOX_CONN_STATE.Disconecting || newState == BOX_CONN_STATE.Disconnected)
                {
                    lock (_locker)
                    {
                        Connection_Status = newState;
                    }
                }
            }
        }

        /* Set connection status to connect when the timer fires
         * 
         */
        private void HamburgTimerEvent(object source, ElapsedEventArgs e)
        {
            lock (_locker)
            {
                Connection_Status = BOX_CONN_STATE.Connected;
            }
            HamburgIntTimer.Enabled = false;
        }
        #endregion

        #region Set/Get Methods

       /***************Get Methods**************************/
        public BOX_ADDRESS getAddress()
        {
            return Address;
        }
        public ushort getReadback(HamburgBOX_ADC_ID id)
        {
            return AdcValue[(ushort)id];
        }
        public PWR_STATE getPowerState(HamburgBOX_RLY_ID id)
        {
            byte b;
            b = (byte)((RLYsPowerStates >> (byte)id) & 0x01);
            return (PWR_STATE)b;
        }
        public bool getTriggerRunningFlag()
        {
            if (TriggerRunningFlag == 1)
                return true;
            else
                return false;
        }

        public Hamburg_TRIGGER_MODE getTriggerMode()
        {
            if (TriggerMode == Hamburg_TRIGGER_MODE.Internal)
                return Hamburg_TRIGGER_MODE.Internal;
            else
                return Hamburg_TRIGGER_MODE.External;
        }
        public ushort getHtrParam(HamburgBOX_HTR_ID htr_id, HTR_PARAM_ID paramId)
        {
            return HTRParam[(byte)htr_id, (byte)paramId];
        }

        /* Get Delay Generators Parameters
         * Second Index is to select Parameter : Toffset, Twidth, TIdle in Array index order
         * Example: Parameters[0,0] = Toffset for Delay Gen 0, Parameters[0,1] = Twidth for Delay Gen 0...
         * Twidth 2 MSBits : if "00"-> output negative pulse, if "01"-> output positive pulse, if "10"-> output allways '0', if "11"-> output allways '1'
        */
        public void getDmcuParameters(HamburgBOX_DMCU DMCU_id, ref uint[] Parameters)
        {
            try
            {
                MB_getDelayParams();
                ushort i;
                for(i=0; i<18; i++)
                {
                    Parameters[i] = DMCUParam[(byte)DMCU_id, i];
                }
            }
            catch (HamburgBoxException ex)
            {
                throw new HamburgBoxException(ex.Message);
            }
        }

        public void getALLDmcuParameters(ref uint[,] Parameters)
        {
            MB_getDelayParams();
            Parameters = DMCUParam;
        }

        /***************Set Methods**************************/
        public void setPowerState(HamburgBOX_RLY_ID id, PWR_STATE state)
        {
            ushort tmp = 0;
            ushort states = RLYsPowerStates;
            tmp = (ushort)(tmp | (ushort)(1 << (byte)id));
            if (state == PWR_STATE.ON)
            {
               // tmp = (ushort)(tmp | (ushort)(1 << (byte)id));
                states |= tmp;
            }
            else
            {
                tmp = (ushort)(0xFFFF ^ tmp);
                states &= tmp;
            }
            MB_setPowerStates(states);
        }
        /* Update Delay Generators Parameters
    * First Index is to choose Desired Delay Generator
    * Second Index is to select Parameter : Toffset, Twidth, TIdle in Array index order
    * Example: Parameters[0,0] = Toffset for Delay Gen 0, Parameters[0,1] = Twidth for Delay Gen 0...
    * Twidth 2 MSBits : if "00"-> output negative pulse, if "01"-> output positive pulse, if "10"-> output allways '0', if "11"-> output allways '1'
    */
        public  void setDmcuParameters(HamburgBOX_DMCU DMCU_id, ref uint[] Parameters)
        {
            try
            {
                for (int i = 0; i < 18; i++)
                {
                    DMCUParam[(byte)DMCU_id, i] = Parameters[i];
                }
                MB_updDelayParams(ref DMCUParam);
            }
            catch (HamburgBoxException ex)
            {
                throw new HamburgBoxException(ex.Message);
            }
        }

        public void setALLDmcuParameters(ref uint[,] Parameters)
        {
            DMCUParam = Parameters;
            MB_updDelayParams(ref DMCUParam);
        }

        /* Sets Trigger Mode
           * First Argument Selects between "Synchronous with tof pulser Triggering" and "Asynchronous"
           * In Asynchronous Mode the second Argument is for the frequency IN HERTZ we want to run (from 250Hz up to 20KHz)
         */
        public void setTriggerMode(Hamburg_TRIGGER_MODE Mode, uint Frequency)
        {
            double temp;
            temp = Math.Round(230400.0 / Frequency); //230400.0 = (14745600/64)  clk/64
            if (temp < 0)
                temp = 0;

            /* Setup Parameters for the three delay generators*/
            try
            {
                MB_updTriggerMode(Mode, (ushort)temp);
            }
            catch (HamburgBoxException ex)
            {
                throw new HamburgBoxException(ex.Message);
            }
        }
        #endregion 

        #region  API Firmaware Methods
        /***************SCELETON***************
              
   
      public void MB_xxx(){
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave+ 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker){
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_xxx;
          
                    try {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex) {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
    
   
         */

        /**********************General*********************/
        public void MB_Connect()
        {
            UInt16 tmp = 0;
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker){
                //--BYTES TO SLAVE--//
                RTUArray[AddrOFF] = (byte)Address;
                RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_Connect;
                try 
                {
                    tmp = (ushort)RTUArray[DataOFF];
                    mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                    tmp = 0;
                    //--BYTES FROM SLAVE--//
                    // an empty byte
                    
                }
                catch (ModbusException ex) {
                    throw new HamburgBoxException("TresCantos_Connect_Error: " + ex.Message);
                }
            }
        }
        public void MB_Reset()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////

            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker){
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_Reset;
                    try {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex) {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }  // unsed
        /*************************ADCs************************************************/
        public void MB_getAdcValues(){
            int i, j = 0;
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;     //no data byte
            int DataBytes_FromSlave = 0x3A; //29 readbacks-> 58bytes
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getAdcValues;

                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                      for (i = 0; i < 29; i++)
                        {
                            AdcValue[i] = (ushort)RTUArray[DataOFF + j];
                            AdcValue[i] |= (ushort)(((ushort)RTUArray[DataOFF + j + 1]) << 8);
                            j = j + 2;
                        }                                       
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        /*************************DACs*************************************************/
        public void MB_updDacValue(HamburgBOX_DAC_ID DacId, ushort DacVal) {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x03;   //DAC_ID, LSByte, MSByte
            int DataBytes_FromSlave = 0x01;    //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_updDacValue;
                    RTUArray[DataOFF] = (byte)DacId;
                    RTUArray[DataOFF + 1] = (byte)(DacVal & 0xFF);
                    RTUArray[DataOFF + 2] = (byte)(DacVal >> 8);
                    DacValue[(byte)DacId] = DacVal;  // save this value to the DacValue[] array.
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        /**************************DIGITAL*********************************************/
        public void MB_powerOn(HamburgBOX_RLY_ID RlyId){
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x01;   //RLY_ID
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_powerOn;
                    RTUArray[DataOFF] = (byte)RlyId;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }    // unsed
        public void MB_powerOff(HamburgBOX_RLY_ID RlyId)
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x01;   //RLY_ID
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_powerOff;
                    RTUArray[DataOFF] = (byte)RlyId;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }   // unsed
        public void MB_setPowerStates(ushort RLYstates)
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x02;  //  powerStates ->  16bit vector
            int DataBytes_FromSlave = 0x01; // one empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            ushort tmp=5;
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_setPowerStates;
                    RTUArray[DataOFF] = (byte)(RLYstates & 0xFF);
                    RTUArray[DataOFF + 1] = (byte)(RLYstates >> 8);
                    RLYsPowerStates = RLYstates;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        tmp = RTUArray[DataOFF];
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_getPowerStates()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x02; // powerStates  -> 16bit vector
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getPowerStates;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        RLYsPowerStates  = (ushort)RTUArray[DataOFF];
                        RLYsPowerStates |= (ushort)((ushort)RTUArray[DataOFF + 1] << 8);
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        /**************************DMCUs*********************************************/
        public void MB_updDelayParams(ref uint[,] Parameter)
        {
            int i = 0, j = 0, k = 0;
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0xD8; //216 params(3 DMCUs * 72bytes).  Params are in order DMCU1..3
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_updDelayParams;
                    for (i = 0; i < 3; i++)     //x3 DMCUs
                    {
                        for (j = 0; j < 18; j++) //x18 - 32bit parameters
                        {
                            RTUArray[DataOFF + k] = (byte)(Parameter[i, j] & 0xFF);
                            RTUArray[DataOFF + k + 1] = (byte)((Parameter[i, j] >> 8) & 0xFF);
                            RTUArray[DataOFF + k + 2] = (byte)((Parameter[i, j] >> 16) & 0xFF);
                            RTUArray[DataOFF + k + 3] = (byte)((Parameter[i, j] >> 24) & 0xFF);                         
                            k = k + 4;
                        }
                    }
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_getDelayParams()
        {
            int i = 0, j = 0, k = 0;
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0xD8;  //216 params(3 DMCUs * 72bytes).  Params are in order DMCU1..3
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes

            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getDelayParams;

                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        for (i = 0; i < 3; i++)
                        {
                            for (j = 0; j < 18; j++)
                            {
                                DMCUParam[i, j] = (uint)RTUArray[DataOFF + k];
                                DMCUParam[i, j] |= (((uint)RTUArray[DataOFF + k + 1]) << 8);
                                DMCUParam[i, j] |= (((uint)RTUArray[DataOFF + k + 2]) << 16);
                                DMCUParam[i, j] |= (((uint)RTUArray[DataOFF + k + 3]) << 24);
                                k = k + 4;
                            }
                        }
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_updTriggerMode(Hamburg_TRIGGER_MODE trigMode, ushort ICRVal)
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x03;   // Mode(1byte), ICR1_Val(2bytes)
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes  
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_updTriggerMode;
                    RTUArray[DataOFF] = (byte)trigMode;
                    RTUArray[DataOFF + 1] = (byte)(ICRVal & 0xFF);
                    RTUArray[DataOFF + 2] = (byte)(ICRVal >> 8);
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_getTriggerParams()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x04; //triggerMode, runningFlag, ICR_LSByte, ICR_MSByte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getTriggerParams;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        TriggerMode = (Hamburg_TRIGGER_MODE)RTUArray[DataOFF];
                        TriggerRunningFlag = RTUArray[DataOFF + 1];
                        TrigICR_Value = (ushort)RTUArray[DataOFF + 2];
                        TrigICR_Value |= (ushort)((ushort)RTUArray[DataOFF + 3] << 8);
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        } 
        public void MB_startTrigger()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_startTrigger;

                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_stopTrigger()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_stopTrigger;

                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        /**************************Heaters*********************************************/
        public void MB_updHtrParameter(HamburgBOX_HTR_ID HtrId, HTR_PARAM_ID HtrParamId, ushort value)
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x04;   //uint8_t HtrId, uint8_t ParameterId, uint16_t Val
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_updHtrParameter;
                    RTUArray[DataOFF] = (byte)HtrId;
                    RTUArray[DataOFF + 1] = (byte)HtrParamId;
                    RTUArray[DataOFF + 2] = (byte)(value & 0xFF);
                    RTUArray[DataOFF + 3] = (byte)(value >> 8);
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_updHtrParams(HamburgBOX_HTR_ID HtrId, ref ushort[] Htrparameter){ // x6 16bit parameters 
            int i = 0, j = 0;
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x0D;   //uint8_t HtrId, 6 params - 12 bytes
            int DataBytes_FromSlave = 0x01; //an empty byte
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_updHtrParams;
                    RTUArray[DataOFF] = (byte)HtrId;
                    for (i = 0; i < 6; i++)
                    {
                        RTUArray[DataOFF + 1 + j] = (byte)(Htrparameter[i] & 0xFF);
                        RTUArray[DataOFF + 2 + j] = (byte)(Htrparameter[i] >> 8);
                        j = j + 2;
                    }
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        // an empty byte
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_getHtrParameter(HamburgBOX_HTR_ID HtrId, HTR_PARAM_ID HtrParamId)
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x02;   //uint8_t HtrId, uint8_t ParameterId
            int DataBytes_FromSlave = 0x02; //parameter Value 2 bytes
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getHtrParameter;
                    RTUArray[DataOFF] = (byte)HtrId;
                    RTUArray[DataOFF + 1] = (byte)HtrParamId;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        HTRParam[(byte)HtrId, (byte)HtrParamId] = (ushort)RTUArray[DataOFF];  // save new heater parameters
                        HTRParam[(byte)HtrId, (byte)HtrParamId] |= (ushort)((ushort)RTUArray[DataOFF + 1] << 8);
                        int a = 0;
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }
        public void MB_getHtrParams(HamburgBOX_HTR_ID HtrId){
            int i = 0, j = 0; 
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x01;   // uint8_t Heater Id
            int DataBytes_FromSlave = 0x0C; // 12 params
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave + 1;  //+1 -> ena extra byte empty - needed in MODBUS.cs code
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getHtrParams;
                    RTUArray[DataOFF] = (byte)HtrId;
                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        for (i = 0; i < 6; i++)
                        {
                            HTRParam[(byte)HtrId, i] = (ushort)RTUArray[DataOFF + j];  // save new heater parameters
                            HTRParam[(byte)HtrId, i] |= (ushort)((ushort)RTUArray[DataOFF + j + 1] << 8);
                            j = j + 2;
                        }
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }

        /**************************Turbos*********************************************/
        public void MB_getTurboStatus()
        {
            //////////////////--CHANGE HERE--/////////////////////
            int DataBytes_ToSlave = 0x00;   //no data byte
            int DataBytes_FromSlave = 0x02; //2 status bytes for the 2 Turbos
            //////////////////////////////////////////////////////
            int BytesFromSlave = MinRTUSize + DataBytes_FromSlave;
            int RTUArraySize_ToSlave = DataBytes_ToSlave + 2; // +2 --> Address, Fcode bytes
            lock (_locker)
            {
                if (Connection_Status == BOX_CONN_STATE.Connected)
                {
                    //--BYTES TO SLAVE--//
                    RTUArray[AddrOFF] = (byte)Address;
                    RTUArray[FcodeOFF] = (byte)FunctionCodes.MB_getTurboStatus;

                    try
                    {
                        mb.SendReceiveFrame(AddrOFF, FcodeOFF, ref RTUArray, RTUArraySize_ToSlave, ref BytesFromSlave);
                        //--BYTES FROM SLAVE--//
                        Turbo1_status = RTUArray[DataOFF];
                        Turbo2_status = RTUArray[DataOFF + 1];
                    }
                    catch (ModbusException ex)
                    {
                        throw new HamburgBoxException("TresCantos_Reset_error : " + ex.Message);
                    }
                }
            }
        }

   #endregion

    }
}