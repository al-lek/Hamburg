using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Timers;
using FTD2XX_NET;

namespace Hamburg_namespace {

        /// <summary>
        /// Status codes enumeration
        /// </summary>
        public enum CE_FTDIStatus {
            CE_FTDI_Ok,
            CE_FTDI_DevListError,
            CE_FTDI_DevNotFound,
            CE_FTDI_CeDevNotFound,
            CE_FTDI_OpenUsbPortError,
            CE_FTDI_SetBaudrateError,
            CE_FTDI_SetDataBitsError,
            CE_FTDISetFlowCtrlError,
            CE_FTDISetTimeoutError,
            CE_FTDI_ClosingPortError,
            CE_FTDI_Purge_Buffers_Error,
            CE_FTDI_ReadBytesError,
            CE_FTDI_WriteBytesError
        };        
        
        public class modbus {
            #region Static CRC
            /*---------------------------------------------------------------------------*/
            /************  STATIC VARIABLE DEFINITION FOR CRC CALCULATION ****************/
            /*---------------------------------------------------------------------------*/
            private static byte[] aucCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40
            };

            private static byte[] aucCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
            0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
            0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
            0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
            0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
            0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
            0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 
            0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
            0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
            0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
            0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 
            0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
            0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
            0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
            0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
            0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
            0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
            0x41, 0x81, 0x80, 0x40
            };

            #endregion

            #region FTDi Declarations
            private UInt32 ftdiDeviceCount;
            private FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;
            private FTDI FtdiPort;
            private FTDI.FT_STATUS ftStatus;
            #endregion

            #region Modbus Declarations
            public string modbusStatus;
            private int Time3_5t_ms = 50;      // 100 
            static readonly object _locker = new object();
            #endregion

            #region Constructor / Deconstructor
            public modbus(){
                FtdiPort = new FTDI();
            }
            ~modbus() { }
            #endregion

            #region FTDI Methods
            /// <summary>
            /// check if a device with serial number "" is connected to PC
            /// </summary>
            /// <param name="DeviceDescription">The String Array to store description of devices with serial number "" </param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus Check_for_devices(ref String[] DeviceDescription) {
                UInt32[] deviceindex;
                UInt32 CE_DeviceCount, i;
                FTDI FtdiDevice = new FTDI();

                CE_DeviceCount = 0;
                ftdiDeviceCount = 0;
                ftStatus = FTDI.FT_STATUS.FT_OK;

                // Determine the number of FTDI devices connected to the machine
                ftStatus = FtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);

                // Check Status
                if (ftStatus == FTDI.FT_STATUS.FT_OK && ftdiDeviceCount != 0) {
                    // Allocate storage for device info list
                    ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
                    deviceindex = new UInt32[ftdiDeviceCount];

                    // Populate our device list
                    ftStatus = FtdiDevice.GetDeviceList(ftdiDeviceList);

                    if (ftStatus == FTDI.FT_STATUS.FT_OK) {
                        for (i = 0; i < ftdiDeviceCount; i++) {
                            if (ftdiDeviceList[i].SerialNumber.ToString() == "FT2SUV03") {
                                deviceindex[CE_DeviceCount] = i;
                                CE_DeviceCount = CE_DeviceCount + 1;
                            }
                        }
                        if (CE_DeviceCount != 0) {
                            DeviceDescription = new String[CE_DeviceCount];
                            for (i = 0; i < CE_DeviceCount; i++) {
                                DeviceDescription[i] = ftdiDeviceList[deviceindex[i]].Description.ToString();
                            }
                            return (CE_FTDIStatus.CE_FTDI_Ok);
                        }
                        else {
                            return (CE_FTDIStatus.CE_FTDI_CeDevNotFound);
                        }
                    }
                    else {
                        return (CE_FTDIStatus.CE_FTDI_DevListError);
                    }
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_DevNotFound);
                }
            }

            /// <summary>
            /// Open device using Device description taken from check_for_device method
            /// This method opens both ports of ft232H and makes Handshake with device
            /// </summary>
            /// <param name="Dname">Device description string</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus Open_Device(String Dname) {
                CE_FTDIStatus status = CE_FTDIStatus.CE_FTDI_Ok;

                //open USBport to comunicate with Electrometer
                status = OpenUsbPort(Dname);
                if (status != CE_FTDIStatus.CE_FTDI_Ok) {
                    return (status);
                }

                //Now initialize comport
                status = Initialize_Comport();
                if (status != CE_FTDIStatus.CE_FTDI_Ok) {
                    Close_Device();
                }
                return (status);
            }

            /// <summary>
            /// Open usb port of device with description Dname taken from Check_for_device method
            /// </summary>
            /// <param name="Dname">Device description string</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            private CE_FTDIStatus OpenUsbPort(String Dname) {
                String[] Devicedesc = { "" };
                bool device_found = false;
                int i;
                CE_FTDIStatus status = CE_FTDIStatus.CE_FTDI_Ok;
                FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

                //First check if device with Dname exists
                status = Check_for_devices(ref Devicedesc);
                if (status != CE_FTDIStatus.CE_FTDI_Ok) {
                    return (status);
                }

                for (i = 0; i < Devicedesc.Length; i++) {
                    if (Dname == Devicedesc[i]) {
                        device_found = true;
                        break;
                    }
                }

                if (device_found == true) {
                    //First open port to comunicate with CE
                    ftStatus = FtdiPort.OpenByDescription(Dname);
                    if (ftStatus == FTDI.FT_STATUS.FT_OK) {
                        return (CE_FTDIStatus.CE_FTDI_Ok);
                    }
                    else {
                        Close_Device();
                        return (CE_FTDIStatus.CE_FTDI_OpenUsbPortError);
                    }
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_CeDevNotFound);
                }
            }

            /// <summary>
            /// Initialize Comport at 921600 8 bits and no flow ctrl
            /// </summary>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            private CE_FTDIStatus Initialize_Comport() {

                // Set Baud rate to 921600
                ftStatus = FtdiPort.SetBaudRate(115200);//921600);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_SetBaudrateError);
                }

                // Set data characteristics - Data bits, Stop bits, Parity
                ftStatus = FtdiPort.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_SetDataBitsError);
                }

                // Set flow control - set RTS/CTS flow control
                ftStatus = FtdiPort.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x11, 0x13);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDISetFlowCtrlError);
                }

                // Set read timeout to 5 seconds, write timeout to 2sec
                ftStatus = FtdiPort.SetTimeouts(4000, 2000);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDISetTimeoutError);
                }
                return (CE_FTDIStatus.CE_FTDI_Ok);
            }

            /// <summary>
            /// Close Device Method
            /// </summary>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus Close_Device() {
                if (FtdiPort.Close() == FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_Ok);
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_ClosingPortError);
                }
            }

            /// <summary>
            /// This method returns FTDI handle
            /// </summary>
            /// <returns>FTDI Handle</returns>
            public FTDI GetPort() {
                return (FtdiPort);
            }

            /// <summary>
            /// This method clears transmit and receive buffers of ftdi
            /// </summary>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus Purge_Buffers() {
                FTDI.FT_STATUS ftstat = FTDI.FT_STATUS.FT_OK;

                ftstat = FtdiPort.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
                if (ftstat == FTDI.FT_STATUS.FT_OK) {
                    ftstat = FtdiPort.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
                    if (ftstat == FTDI.FT_STATUS.FT_OK) {
                        return (CE_FTDIStatus.CE_FTDI_Ok);
                    }
                }
                return (CE_FTDIStatus.CE_FTDI_Purge_Buffers_Error);
            }

            /// <summary>
            /// use this Method to read data from FTDI
            /// </summary>
            /// <param name="res">An array of bytes which will be populated with the data read from the device.</param>
            /// <param name="req_Byte">The number of bytes requested from the device.</param>
            /// <param name="num_read">The number of bytes actually read.</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus ReadBytes(byte[] res, UInt32 req_Byte, ref UInt32 num_read) {
                ftStatus = FtdiPort.Read(res, req_Byte, ref num_read);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_ReadBytesError);
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_Ok);
                }
            }

            /// <summary>
            /// use this Method to read one byte from FTDI
            /// </summary>
            /// <param name="res">A byte which will be populated with the data read from the device.</param>
            /// <param name="num_read">The number of bytes actually read.</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus ReadByte(ref byte res, ref UInt32 num_read) {
                byte[] temp = new byte[1];

                ftStatus = FtdiPort.Read(temp, 1, ref num_read);
                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_ReadBytesError);
                }
                else {
                    res = temp[0];
                    return (CE_FTDIStatus.CE_FTDI_Ok);
                }
            }

            /// <summary>
            /// use this Method to Send data to FTDI
            /// </summary>
            /// <param name="res">An array of bytes which contains the data to be written to the device.</param>
            /// <param name="req_Byte">The number of bytes to be written to the device.</param>
            /// <param name="num_written">The number of bytes actually written to the device.</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus WriteBytes(byte[] res, Int32 req_Byte, ref UInt32 num_written) {
                ftStatus = FtdiPort.Write(res, req_Byte, ref num_written);

                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_WriteBytesError);
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_Ok);
                }
            }

            /// <summary>
            /// use this Method to Send a byte to FTDI
            /// </summary>
            /// <param name="res">A bytes which contains the data to be written to the device.</param>
            /// <param name="num_written">The number of bytes actually written to the device.</param>
            /// <returns>FTDIStatus Value from FTDIStatus enum</returns>
            public CE_FTDIStatus WriteByte(byte res, ref UInt32 num_written) {
                byte[] temp = new byte[1];

                temp[0] = res;

                ftStatus = FtdiPort.Write(temp, 1, ref num_written);

                if (ftStatus != FTDI.FT_STATUS.FT_OK) {
                    return (CE_FTDIStatus.CE_FTDI_WriteBytesError);
                }
                else {
                    return (CE_FTDIStatus.CE_FTDI_Ok);
                }
            }
            #endregion
            
            #region CRC Computation
            private void GetCRC(byte[] message, int usLen, ref byte[] CRC)
            {
                //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
                //return the CRC values:

                byte ucCRCHi = 0xFF;
                byte ucCRCLo = 0xFF;
                byte iIndex;
                int i;

                i = 0;
                while (usLen != 0)
                {
                    usLen = usLen - 1;
                    iIndex = (byte)(ucCRCLo ^ (message[i]));
                    ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                    ucCRCHi = aucCRCLo[iIndex];
                    i++;
                }
                CRC[0] = ucCRCLo;
                CRC[1] = ucCRCHi;
            }
            #endregion

            #region CheckResponse
            private void CheckResponse(byte Addr, int AddrOFF, int FcodeOFF, byte functionCode, byte[] response, int actualLength, ref int responselength)
            {
                int temp;

                //Perform a basic CRC check:
                byte[] CRC = new byte[2];
                GetCRC(response, actualLength, ref CRC);
                if (CRC[0] == 0 && CRC[1] == 0)
                {
                    if (response[AddrOFF] == Addr)
                    {
                        temp = response[FcodeOFF] & 127;
                        if (temp == functionCode)
                        {
                            temp = response[FcodeOFF] & 128;
                            if (temp == 0)
                            {
                                responselength = actualLength;
                                modbusStatus = "Function Read Succesfull";
                            }
                            else
                            {
                                responselength = actualLength;
                                modbusStatus = "MB _CheckResponse_ Error Frame Received From Hardware: " + response[FcodeOFF + 1].ToString() + " code";
                                throw new ModbusException(modbusStatus);
                            }
                        }
                        else
                        {
                            modbusStatus = "MB _CheckResponse_ Invalid Function Code On Response Error";
                            throw new ModbusException(modbusStatus);
                        }
                    }
                    else
                    {
                        modbusStatus = "MB _CheckResponse_ Invalid Address While Reading Response Error";
                        throw new ModbusException(modbusStatus);
                    }
                }
                else
                {
                    modbusStatus = "MB _Check Response_ CRC Response Error";
                    throw new ModbusException(modbusStatus);
                }
            }
            #endregion

            #region SendReceiveFrame
            /*
             * Message containd function code in second position index=1, data following function code
             * The length variable has the length of address, function code and data
             * ResponseLength varable will contain the length of response
             */
            public void SendReceiveFrame(int AddrOFF, int FcodeOFF, ref byte[] Message, int length, ref int ResponseLength){
                byte temp = 0;
                uint actualread = 0, numwritten = 0;
                byte[] response = new byte[ResponseLength];
                byte[] CRC = new byte[2];                                           //Array to receive CRC bytes
                byte functionCode, Address;
                int i;

                if (FtdiPort.IsOpen == true){
                    Address = Message[AddrOFF];                                      //Store Address send for later check
                    functionCode = Message[FcodeOFF];                               //Store function code send for later check

                    GetCRC(Message, length, ref CRC);                               //calculate CRC of message
                    Message[length] = CRC[0];                                   //Store CRC to message
                    Message[length + 1] = CRC[1];                                   //Store CRC to message
                    length = length + 2;                                            //update length variable

                    /*****************************************************/
                    /** check for idle state befor sending the message ***/
                    /*****************************************************/
                    FtdiPort.SetTimeouts((uint)Time3_5t_ms, (uint)2000);
                    Purge_Buffers();

                    while (true){
                        ReadByte(ref temp, ref actualread);
                        if (actualread == 0) {
                            break;
                        }
                    }

                    /*****************************************************/
                    /************** Try send the message *****************/
                    /*****************************************************/
                    if (WriteBytes(Message, length, ref numwritten) != CE_FTDIStatus.CE_FTDI_Ok || numwritten != length) {
                        modbusStatus = "MB _SendReceiveFrame_ Sending Data To Serial Error";
                        throw new ModbusException(modbusStatus);
                    }


                    /******************************************************/
                    /** wait to read first byte for max 4 sec. Then start */
                    /* reading all bytes and exit after 3.5T from the last*/
                    /** Byte read. i has the length of response           */
                    /******************************************************/
                    FtdiPort.SetTimeouts((uint)2000, (uint)2000);                                             //set read timeout=2s
                    i = 0;

                    while (true)
                    {
                        ReadByte(ref temp, ref  actualread);
                        if(actualread == 1){
                            Message[i] = temp;
                            FtdiPort.SetTimeouts((uint)Time3_5t_ms, (uint)2000);                                             //set read timeout=2s
                            i = i + 1;
                        }
                        else{
                            if (i == 0)
                            {
                                modbusStatus = "MB _SendReceiveFrame_ Timeout Occured While Reading (Function code = " + Message[FcodeOFF].ToString() + " ) Error";
                                throw new ModbusException(modbusStatus);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }


                    /******************************************************/
                    /*** check for response: crc, adress, function code, **/
                    /*********  length and posible error codes ************/
                    try
                    {
                        CheckResponse(Address, AddrOFF, FcodeOFF, functionCode, Message, i, ref ResponseLength);
                    }
                    catch (ModbusException)
                    {
                        throw;
                    }
                }
                else{
                    modbusStatus = "MB _SendReceiveFrame_ Serial Port Is Closed Error";
                    throw new ModbusException(modbusStatus);
                }
            }
            #endregion
        }
    }
