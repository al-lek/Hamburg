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
    public partial class Start_StopTrigger
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
            InstrumentCtrlInterface.HamburgBoxTriggerParams_UPD_Event += HamburgBoxTriggerParams_UPD_EventMethod;
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

        private void StartTrigger()
        {
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      //find the right box and cast it into the previous object 
                box.MB_startTrigger();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void StopTrigger()
        {
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      //find the right box and cast it into the previous object 
                box.MB_stopTrigger();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool GetTriggerState(BOX_ADDRESS boxAddress)
        {
            bool flag = false;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                    flag = (bool)box.getTriggerRunningFlag();                                                            // get the readback you need    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return flag;
        }

        private Hamburg_TRIGGER_MODE GetTriggerModeState(BOX_ADDRESS boxAddress)
        {
            Hamburg_TRIGGER_MODE mode = Hamburg_TRIGGER_MODE.Internal;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                    mode = box.getTriggerMode();                                                            // get the readback you need    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return mode;
                }
            }
            return mode;
        }




        
        #endregion
    }
}
