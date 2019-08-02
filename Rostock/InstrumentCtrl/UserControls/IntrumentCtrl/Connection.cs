using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Hamburg_namespace
{
    public partial class Connection : UserControl
    {

        #region Constructor
        public Connection()
        {
            InitializeProperties();
            InitializeComponent();

            // register an event to notify UI that connected state has changed ( is raised after the SUCCESFUL connetion/disconnection)
            this.button1.EnabledChanged += (s, e) => { signal_mainForm(!button1.Enabled); };
        }

        /* On Form Load Get Parrent and register events
       * 
       */
        private void Connection_Load(object sender, EventArgs e)
        {
            GetParrentRegisterEvent();
        }
        #endregion

        #region Connect Disconnect Button
        /* Action to perform when hitting the Connect button click
         * 
         */
        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
        }
        /* Action to perform when hitting the Disconnect button click
         * 
         */
        private void button2_Click(object sender, EventArgs e)
        {
            Disconnect();
        }
        #endregion

        #region Readbacks
        /* Update Button State (Enabled, Disabled) According to new CEState
         * 
         */
        public void UpdateStates_GLB_EventMethod(INSTRUMENT_CONN_STATE State)
        {
            if (State == INSTRUMENT_CONN_STATE.Connected)
            {
                if (this.button1.InvokeRequired)
                    this.button1.Invoke(new Action(() => button1.Enabled = false));
                else
                    this.button1.Enabled = false;

                if (this.button2.InvokeRequired)
                    this.button2.Invoke(new Action(() => button2.Enabled = true));
                else
                    this.button2.Enabled = true;
            }
            else if (State == INSTRUMENT_CONN_STATE.Disconnected)
            {
                if (this.button1.InvokeRequired)
                    this.button1.Invoke(new Action(() => button1.Enabled = true));
                else
                    this.button1.Enabled = true;

                if (this.button2.InvokeRequired)
                    this.button2.Invoke(new Action(() => button2.Enabled = false));
                else
                    this.button2.Enabled = false;
            }
            else
            {
                MessageBox.Show("Invalid ConnectionState");
            }
        }
        #endregion

        #region Resize Code
        /* Set fixed heigh and adjustable width in designer and runtime
         * 
         */
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 58, specified);
        }
        #endregion

        #region Event to signal MainForm on connection state change
        private void signal_mainForm(bool connected)
        {
            // this will signal mainform that the connection status has changed.
            // mainForm will deceide what to do with the rest UI controls.
            try
            {
                Type type = typeof(Form1);
                Form form1 = Application.OpenForms["Form1"];
                MethodInfo pass_changes_to_mainForm_method = type.GetMethod("connection_change");
                pass_changes_to_mainForm_method.Invoke(form1, new object[] { connected });
            }
            catch { }
        }
        #endregion

    }
}
