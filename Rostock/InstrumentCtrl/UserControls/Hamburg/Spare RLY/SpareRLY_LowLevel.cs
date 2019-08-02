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
    public partial class SpareRLY : UserControl
    {
        private BOX_ADDRESS UC_BOX_ADDRESS;
        
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
                    state = box.getPowerState(HamburgBOX_RLY_ID.RLY_ID_SPARE);                                           // get the readback you need    
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
                box.setPowerState(HamburgBOX_RLY_ID.RLY_ID_SPARE, state);
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
