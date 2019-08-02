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
    public struct HeaterDevice
    {
        public HamburgBOX_RLY_ID RelayId;
        public HeaterDevice(HamburgBOX_RLY_ID RelayId)
        {
            this.RelayId = RelayId;
        }
    };
    public partial class Heater : UserControl
    {
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private HamburgBOX_HTR_ID UC_HTR_ID;
        private double MAX_TEMPERATURE;
        private double ACCEPTABLE_T_VARIATION;
        private bool GLOBAL_POWER_CHANGE_ENABLED;

        static HeaterDevice[] HeaterDeviceArray = new HeaterDevice[2]
        {
            new HeaterDevice(HamburgBOX_RLY_ID.RLY_ID_HEATER1),
            new HeaterDevice(HamburgBOX_RLY_ID.RLY_ID_HEATER2),
        };

        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
            ACCEPTABLE_T_VARIATION = 10.0;
            MAX_TEMPERATURE = 300.0;
            GLOBAL_POWER_CHANGE_ENABLED = false;
        }
        private void InitializeValues()
        {
            numericTextBoxWithoutSign1.MaximumDValue = MAX_TEMPERATURE;
        }
        private void RegisterEventMethods()
        {
            InstrumentCtrlInterface.PowerStateChange_GLB_Event += PowerStateChange_GLB_EventMethod;
            InstrumentCtrlInterface.HamburgBoxPowerStates_UPD_Event += HamburgBoxPowerStates_UPD_EventMethod;
            InstrumentCtrlInterface.HamburgBoxHtrParams_UPD_Event += HamburgBoxHtrParams_UPD_EventMethod;
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
        public HamburgBOX_HTR_ID HTR_ID
        {
            set
            {
                UC_HTR_ID = value;
            }
            get
            {
                return (UC_HTR_ID);
            }
        }
        public double MaxTemperature
        {
            set
            {
                MAX_TEMPERATURE = value;
            }
            get
            {
                return (MAX_TEMPERATURE);
            }
        }
        public double PercentTempVariation
        {
            set
            {
                ACCEPTABLE_T_VARIATION = value;
            }
            get
            {
                return (ACCEPTABLE_T_VARIATION);
            }
        }
        public bool GlobalPowerChangeEnabled
        {
            set
            {
                GLOBAL_POWER_CHANGE_ENABLED = value;
            }
            get
            {
                return (GLOBAL_POWER_CHANGE_ENABLED);
            }
        }
        #endregion

        #region Low level Methods
        private double GetHtrTemp(BOX_ADDRESS boxAddress)
        {
           double tempval=0;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                    tempval = box.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_T);                                    // get the readback you need    
                    return tempval;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return tempval;
                }
            }
            return tempval;
        }
        private PWR_STATE GetHtrPwrState(BOX_ADDRESS boxAddress)
        {
            PWR_STATE state = PWR_STATE.OFF;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                //find the right box  
                    state = box.getPowerState(HeaterDeviceArray[(byte)UC_HTR_ID].RelayId);                                // get the readback you need    
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
        private bool SetHtrTemperature(double value)
        {
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      //find the right box and cast it into the previous object 
                box.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts, (ushort)value);
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void SetHtrPwrState(PWR_STATE state)
        {
            try
            {
                HamburgBoxInterface box;                                                                  // create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      // find the right box and cast it into the previous object 
                box.setPowerState(HeaterDeviceArray[(byte)UC_HTR_ID].RelayId, state);                     // calculate the relay 16 bit vector and the function sends it afterwards
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}

    