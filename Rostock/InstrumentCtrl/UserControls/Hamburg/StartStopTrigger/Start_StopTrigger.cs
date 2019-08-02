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
    public partial class Start_StopTrigger : UserControl
    {
        #region Constructor
        public Start_StopTrigger()
        {
            InitializeComponent();
            InitializeProperties();
        }
        #endregion

        #region Event registring
        private void Start_StopTrigger_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
        }
        #endregion

        #region  UC Action methods
        private void button1_Click(object sender, EventArgs e)  // Start triiger button click
        {
            StartTrigger();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopTrigger();
        }
        #endregion

        #region Event called methods
        public void HamburgBoxTriggerParams_UPD_EventMethod(BOX_ADDRESS addr)
        {
            bool triggerRunningFlag = GetTriggerState(addr);
            Hamburg_TRIGGER_MODE mode = GetTriggerModeState(addr);

            if (triggerRunningFlag == true)
            {
                if (this.button1.InvokeRequired)
                    this.button1.Invoke(new Action(() => button1.Enabled = false));
                else
                    this.button1.Enabled = false;

                if (this.button2.InvokeRequired)
                    this.button2.Invoke(new Action(() => button2.Enabled = true));
                else
                    this.button2.Enabled = true;
            }else
            {
                if (this.button2.InvokeRequired)
                    this.button2.Invoke(new Action(() => button2.Enabled = false));
                else
                    this.button2.Enabled = false;

                if (this.button1.InvokeRequired)
                    this.button1.Invoke(new Action(() => button1.Enabled = true));
                else
                    this.button1.Enabled = true;
            }
        }

        #endregion
  
    }
}
