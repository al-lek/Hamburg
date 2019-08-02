using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace Hamburg_namespace
{
    public enum HV_PSU_ID : byte
    {
        RF1 = 0x00,
        RF2 = 0x01,
        PSU800V = 0x02,
    }
    public struct HVPsu
    {
        public HamburgBOX_RLY_ID RelayId;
        public HamburgBOX_DAC_ID DacId;
        public HamburgBOX_ADC_ID AdcId;
        public double Dacoffset;    // Vout-Vmin(set)
        public double Dacgain;      // Vmax(set) - Vout
        public double Adcoffset;    // Vrdb-Vout(min)
        public double Adcgain;      // Vout(max)- Vrdb

        public HVPsu(HamburgBOX_RLY_ID RelayId, HamburgBOX_DAC_ID DacId, HamburgBOX_ADC_ID AdcId, double Dacoffset, double Dacgain, double Adcoffset, double Adcgain)
        {
            this.RelayId = RelayId;
            this.DacId = DacId;
            this.AdcId = AdcId;
            this.Dacoffset = Dacoffset;
            this.Dacgain = Dacgain;
            this.Adcoffset = Adcoffset;
            this.Adcgain = Adcgain;
        }
    };

    public partial class HV_Psu: UserControl
    {
        static HVPsu[] HVPsusArray = new HVPsu[3] 
           { new HVPsu(HamburgBOX_RLY_ID.RLY_ID_RF1, HamburgBOX_DAC_ID.DAC_ID_RF1, HamburgBOX_ADC_ID.ADC_ID_Vrdb_RF1, 13.25, 0, 0, 0),   
             new HVPsu(HamburgBOX_RLY_ID.RLY_ID_RF2, HamburgBOX_DAC_ID.DAC_ID_RF2, HamburgBOX_ADC_ID.ADC_ID_Vrdb_RF2, 11.5, 0, 0, 0),
             new HVPsu(HamburgBOX_RLY_ID.RLY_ID_PSU800V, HamburgBOX_DAC_ID.DAC_ID_PSU800V, HamburgBOX_ADC_ID.ADC_ID_Vrdb_PSU800V, 0, 0, 0, 0),
        };
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private HV_PSU_ID UC_HV_PSU_ID;
        private double MAX_VOLTAGE;
        private double MIN_VOLTAGE;
        private double ACCEPTABLE_V_VARIATION;
        private static System.Timers.Timer delayTimer = new System.Timers.Timer();
        private PWR_STATE setPWRstate;

        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
           UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
           UC_HV_PSU_ID = HV_PSU_ID.RF1;
           MAX_VOLTAGE = 500;
           ACCEPTABLE_V_VARIATION = 10.0;
           delayTimer.Interval = 1000;// in ms
           delayTimer.Elapsed += new ElapsedEventHandler(delayTimer_EventMethod);
        }
        private void InitializeValues()
        {
           numericTextBoxWithoutSign1.MaximumDValue = MAX_VOLTAGE;
        }
        private void RegisterEventMethods()
        {
            InstrumentCtrlInterface.PowerStateChange_GLB_Event += PowerStateChange_GLB_EventMethod;
            InstrumentCtrlInterface.HamburgBoxAdcValues_UPD_Event += HamburgBoxAdcValues_UPD_EventMethod;
            InstrumentCtrlInterface.HamburgBoxPowerStates_UPD_Event +=HamburgBoxPowerStates_UPD_EventMethod;
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
        public HV_PSU_ID HV_Psu_Id
        {
            set
            {
                UC_HV_PSU_ID = value;
            }
            get
            {
                return (UC_HV_PSU_ID);
            }
        }
        public double Max_Voltage
        {
            set
            {
                MAX_VOLTAGE = value;
            }
            get
            {
                return (MAX_VOLTAGE);
            }
        }
        public double Min_Voltage
        {
            set
            {
                MIN_VOLTAGE = value;
            }
            get
            {
                return (MIN_VOLTAGE);
            }
        }
        public double PercentVoltageVariation
        {
            set
            {
                ACCEPTABLE_V_VARIATION = value;
            }
            get
            {
                return (ACCEPTABLE_V_VARIATION);
            }
        }
    
        #endregion

        #region Low level Methods

        private double shouldBeEdited()
        {
            if(UC_HV_PSU_ID == HV_PSU_ID.RF1  || UC_HV_PSU_ID == HV_PSU_ID.RF2)
            {
                return 1;
            }
            return 0;
        }
        private double GetRdbVoltage(BOX_ADDRESS boxAddress)
        {
            double voltage = 0;
            double adcvalue = 0;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                     HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                     box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);               //find the right box  
                     adcvalue = box.getReadback(HVPsusArray[(byte)UC_HV_PSU_ID].AdcId);                                   // get the readback you need    
                     if (PwrState == PWR_STATE.ON)
                     {
                         voltage = adcvalue * MAX_VOLTAGE / 4095 - HVPsusArray[(byte)UC_HV_PSU_ID].Adcoffset;              // remove Voffset                
                         voltage = voltage + HVPsusArray[(byte)UC_HV_PSU_ID].Adcgain * voltage / MAX_VOLTAGE;
                     }
                     else
                     {
                         return 0;
                     }
                     return voltage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return 0;
                }
            }
            return 0;
        }
        private PWR_STATE GetPwrState(BOX_ADDRESS boxAddress)
        {
            PWR_STATE state = PWR_STATE.OFF;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                     HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                     box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                     state = box.getPowerState(HVPsusArray[(byte)UC_HV_PSU_ID].RelayId);                                       // get the readback you need    
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
        private byte SetVoltage(double Value)
        {
            ushort DacValue = 0;
            double tmp = 0;
            tmp = (Value * 16383) / MAX_VOLTAGE - (HVPsusArray[(byte)UC_HV_PSU_ID].Dacoffset * 16383) / MAX_VOLTAGE; // remove Voffset
            tmp = tmp + ((HVPsusArray[(byte)UC_HV_PSU_ID].Dacgain * tmp) / MAX_VOLTAGE);
            if (tmp < 0)
            {
                DacValue = 0;
            }
            else
            {
                DacValue = (ushort)tmp;
            }
            if (tmp >= 16383)
            {
                DacValue = 16383;
            }
            
            try
            {
               HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
               box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS-1)]);      //find the right box and cast it into the previous object 
               box.MB_updDacValue(HVPsusArray[(byte)UC_HV_PSU_ID].DacId, DacValue);
               return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        private byte SetPwrState(PWR_STATE state)
        {
            try
            {
               HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
               box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS-1)]);      //find the right box and cast it into the previous object 
               double r = (double)UC_HV_PSU_ID;
               HV_PSU_ID rp = UC_HV_PSU_ID;
               setPWRstate = state;
               if (shouldBeEdited()==0)
               {
                   SetVoltage(0);
                   numericTextBoxWithoutSign1.DoubleValue = 0.00;
               }
               box.setPowerState(HVPsusArray[(byte)UC_HV_PSU_ID].RelayId, setPWRstate);
             //  SetVoltage(0);
             // delayTimer.Start();
               //delayTimer.Enabled = true;
               
              // while (delayTimer.Enabled == true)                                
             //  box.setPowerState(HVPsusArray[(byte)UC_HV_PSU_ID].RelayId, state);
               return 1;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        private void delayTimer_EventMethod(object source, ElapsedEventArgs e)
        {
      //      delayTimer.Stop();
      //      HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
       //     box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS - 1)]);      //find the right box and cast it into the previous object 
       //     box.setPowerState(HVPsusArray[(byte)UC_HV_PSU_ID].RelayId, setPWRstate);
        }
        #endregion 
    }
}


