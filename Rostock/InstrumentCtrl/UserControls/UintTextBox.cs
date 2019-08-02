using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;


namespace Hamburg_namespace {

    class UintTextBox : TextBox {
        private uint UintValue;

        #region Constructor
        /* Constructor
         * 
         */
        public UintTextBox() {
            UintValue = 0;
            MinimumUintValue = 0;
            MaximumUintValue = 0;
            this.Text = UintValue.ToString("0.0;0.0;0.0", CultureInfo.CreateSpecificCulture("en-US"));
            this.TextAlign = HorizontalAlignment.Center;
        }
        #endregion

        #region Extra properties
        public uint UintegerValue {
            set {
                UintValue = value;
                this.Text = UintValue.ToString("0;0;0", CultureInfo.CreateSpecificCulture("en-US"));
            }
            get {
                return (UintValue);
            }
        }

        public double MinimumUintValue { get; set; }
        public double MaximumUintValue { get; set; }
        #endregion

        #region Events
        /* if this is an invalid character set e.handled to true 
         * 
         */
        protected override void OnKeyPress(KeyPressEventArgs e) {
            // if this is a valid character proceed
            if (IsOKForDecimalTextBox4(e.KeyChar, this) == true) {
                if (e.KeyChar != (char)Keys.Enter) {
                    this.BackColor = Color.MistyRose;
                }
            }
            // else set set e.handled to true so that the framework will not update the textbox with this value
            else {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        /* Set Selection to the end of the text when enter to texbox using tab
         * 
         */
        protected override void OnEnter(EventArgs e) {
            base.OnEnter(e);

            this.SelectionStart = this.Text.Length;
        }

        /* When text changes update Int value
         * 
         */
        protected override void OnTextChanged(EventArgs e) {
            uint temp;
            int possition;

            if (uint.TryParse(this.Text, 0 , CultureInfo.CreateSpecificCulture("en-US"), out temp) == true) {
                if (IsInAcceptableRange(temp) == true) {
                    UintValue = temp;
                }
                else {
                    possition = this.SelectionStart;
                    this.Text = UintValue.ToString("0;0;0", CultureInfo.CreateSpecificCulture("en-US"));
                    if (possition > 0) {
                        this.SelectionStart = possition - 1;
                    }
                    MessageBox.Show("Value Out of Range!! \n" + MinimumUintValue + " < Value < " + MaximumUintValue);
                }
            }
            base.OnTextChanged(e);
        }

        /* When Double Click select decimal value
         * 
         */
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            this.SelectionStart = 0;
            this.SelectionLength = this.Text.Length;
            base.OnMouseDoubleClick(e);
        }
        #endregion

        #region Test char
        /* Evaluate charachter and text
         * return true if character is ok
         */
        private bool IsOKForDecimalTextBox4(char theCharacter, TextBox theTextBox) {

            //Only allow  control characters and digits
            if (!char.IsControl(theCharacter) && !char.IsDigit(theCharacter)) {
                // Then it is NOT a character we want allowed in the text box.
                return false;
            }

            // Otherwise the character is perfectly fine for a decimal value and the character
            // may indeed be placed at the current insertion position.
            return true;
        }

        /* Evaluate IntValue
         * return true if MinimumDValue < UintValue < MaximumDValue
         */
        private bool IsInAcceptableRange(uint Value) {
            if ((MinimumUintValue <= Value) && (Value <= MaximumUintValue)) {
                return (true);
            }
            return (false);
        }
        #endregion
    }
}