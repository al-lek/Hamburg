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
    public partial class Connection
    {

        #region Get Parrent and register events
        private void InitializeProperties()
        {
        }

        /* On Form Load Get Parrent and register events
         * 
         */
        private void GetParrentRegisterEvent()
        {

            InstrumentCtrlInterface.ConnectionState_GLB_Event += UpdateStates_GLB_EventMethod;
        }
        #endregion

        #region Properties

        #endregion

        #region Set Values
        /* Connect 
         * 
         */
        private void Connect()
        {
            try
            {

                InstrumentCtrlInterface.Connect();
            }
            catch (InstrumentInterfaceException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Disconnect
         * 
         */
        private void Disconnect()
        {
            try
            {
                InstrumentCtrlInterface.Disconnect();
            }
            catch (InstrumentInterfaceException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}
