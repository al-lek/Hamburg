using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hamburg_namespace
{
   public enum PUMP_ID : byte
   {
       TURBO_1 = 0x00,
       TURBO_2 = 0x01,
       ROTARY_1 = 0x02,
       ROTARY_2 = 0x03,
   }   
   public struct PumpDevice
   {
       public HamburgBOX_RLY_ID RelayId;
       public PumpDevice(HamburgBOX_RLY_ID RelayId)
       {
           this.RelayId = RelayId;
       }
   };
    public partial class Pump : UserControl
    {
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private PUMP_ID UC_PUMP_ID;
        static PumpDevice[] PumpDeviceArray = new PumpDevice[4] 
        {   new PumpDevice(HamburgBOX_RLY_ID.RLY_ID_TURBO1),   
            new PumpDevice(HamburgBOX_RLY_ID.RLY_ID_TURBO2),
            new PumpDevice(HamburgBOX_RLY_ID.RLY_ID_ROTARY1),
            new PumpDevice(HamburgBOX_RLY_ID.RLY_ID_ROTARY2),
        };
        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
        }
        private void InitializeValues()
        {

        }
        private void RegisterEventMethods()
        {
            InstrumentCtrlInterface.PowerStateChange_GLB_Event += PowerStateChange_GLB_EventMethod;
            InstrumentCtrlInterface.HamburgBoxPowerStates_UPD_Event += HamburgBoxPowerStates_UPD_EventMethod;
        }
        #endregion

        #region Properties set/get
        public BOX_ADDRESS Hamburgbox_Address
        {
            set
            {
                UC_BOX_ADDRESS = value;
            }
            get
            {
                return (UC_BOX_ADDRESS);
            }
        }
        public PUMP_ID Pump_Id
        {
            set
            {
                UC_PUMP_ID = value;
            }
            get
            {
                return (UC_PUMP_ID);
            }
        }
        #endregion

        #region Low level Methods
        private PWR_STATE GetPwrState(BOX_ADDRESS boxAddress)
        {
            PWR_STATE state = PWR_STATE.OFF;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                    state = box.getPowerState(PumpDeviceArray[(byte)UC_PUMP_ID].RelayId);                                // get the readback you need    
                    return state;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return state;
                }
            }
            return state;
        }
        private byte SetPwrState(PWR_STATE state)
        {
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      //find the right box and cast it into the previous object 
                box.setPowerState(PumpDeviceArray[(byte)UC_PUMP_ID].RelayId, state);
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        #endregion
    }
}
