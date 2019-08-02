using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;


namespace Hamburg_namespace
{

    public class NumericTextBoxWithSign : TextBox{
        private double DValue;
        

        #region Constructor
        /* Constructor
         * 
         */
        public NumericTextBoxWithSign()
        {
            DoubleValue = 0.0;
            Format = FormatType.Double;
            MinimumDValue = 0.0;
            MaximumDValue = 0.0;
            this.Text = DValue.ToString("+0.0;-0.0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
            this.TextAlign = HorizontalAlignment.Center;
        }
        #endregion

        #region Extra properties
        public double DoubleValue{
            set{
                DValue = value;
                if (Format == FormatType.Double){
                    this.Text = DValue.ToString("+0.0;-0.0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
                }
                else if (Format == FormatType.Scientific){
                    this.Text = DValue.ToString("+0.#e0;-0.#e0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
                };
            }
            get{
                return (DValue);
            }
        }

        public FormatType Format{ get; set;}

        public double MinimumDValue { get; set; }
        public double MaximumDValue { get; set; }

        #endregion

        #region Events
        /* if this is an invalid character set e.handled to true 
            * 
            */
        protected override void OnKeyPress(KeyPressEventArgs e){
            // if this is a valid character proceed
            if (IsOKForDecimalTextBox2(e.KeyChar, this) == true)
            {
                if (e.KeyChar != (char)Keys.Enter)
                {
                    this.BackColor = Color.MistyRose;
                }
            }
            // else set set e.handled to true so that the framework will not update the textbox with this value
            else
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        /* Set Selection to the end of the text when enter to texbox using tab
            * 
            */
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            this.SelectionStart = this.Text.Length;
        }

        /* When text changes update double value
            * 
            */
        protected override void OnTextChanged(EventArgs e){
            double temp;
            int possition;

            if (double.TryParse(this.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CreateSpecificCulture("en-US"), out temp) == true){
                if (IsInAcceptableRange(temp) == true){
                    DValue = temp;
                }
                else{
                    possition = this.SelectionStart;
                    this.Text = DValue.ToString("+0;-0;+0", CultureInfo.CreateSpecificCulture("en-US"));
                    if (possition > 0){
                        this.SelectionStart = possition - 1;
                    }
                    MessageBox.Show("Value Out of Range!! \n" + MinimumDValue + " < Value < " + MaximumDValue);
                }
            }
            base.OnTextChanged(e);
        }

        /* When Double Click select decimal value
         * 
         */
        protected override void OnMouseDoubleClick(	MouseEventArgs e){
            this.SelectionStart = 0;
            this.SelectionLength = this.Text.Length;
            base.OnMouseDoubleClick(e);
        }
        #endregion

        #region Test char
        /* Evaluate charachter and text
         * return true if character is ok
         */
        private bool IsOKForDecimalTextBox2(char theCharacter, TextBox theTextBox){
            if (theTextBox.SelectionStart == 0 && theTextBox.SelectionLength == theTextBox.Text.Length && ((theCharacter == '-') || (theCharacter == '+'))){
                return (true);
            }
            else{
                //Only allow  control characters, digits, '.', '-' OR '+' 
                if (!char.IsControl(theCharacter) && !char.IsDigit(theCharacter) && (theCharacter != '.') && (theCharacter != '-') && (theCharacter != '+'))
                {
                    // Then it is NOT a character we want allowed in the text box.
                    return false;
                }

                // Only allow one decimal point.
                if (theCharacter == '.' && theTextBox.Text.IndexOf('.') > -1)
                {
                    // Then there is already a decimal point in the text box.
                    return false;
                }

                // Only allow one minus sign.
                if (theCharacter == '-' && theTextBox.Text.IndexOf('-') > -1)
                {
                    // Then there is already a minus sign in the text box.
                    return false;
                }

                // Only allow one plus sign.
                if (theCharacter == '+' && theTextBox.Text.IndexOf('+') > -1)
                {
                    // Then there is already a plus sign in the text box.
                    return false;
                }

                // Only allow one plus sign OR minus sign, but not both.
                if (((theCharacter == '-') || (theCharacter == '+')) && ((theTextBox.Text.IndexOf('-') > -1) || (theTextBox.Text.IndexOf('+') > -1)))
                {
                    // Then the user is trying to enter a plus or minus sign and
                    // there is ALREADY a plus or minus sign in the text box.
                    return false;
                }

                // Only allow a minus or plus sign at the first character position.
                if (((theCharacter == '-') || (theCharacter == '+')) && theTextBox.SelectionStart != 0)
                {
                    // Then the user is trying to enter a minus or plus sign at some position 
                    // OTHER than the first character position in the text box.
                    return false;
                }

                // Only allow digits and decimal point AFTER any existing plus or minus sign
                // Is digit or decimal point                                         // A plus or minus sign EXISTS                        // Attempting to put the character at the beginning of the field.
                if ((char.IsDigit(theCharacter) || (theCharacter == '.')) && ((theTextBox.Text.IndexOf('-') > -1) || (theTextBox.Text.IndexOf('+') > -1)) && theTextBox.SelectionStart == 0)
                {
                    // Then the user is trying to enter a digit or decimal point in front of a minus or plus sign.
                    return false;
                }

                // Only Allow two digits after Decimal Point
                if (theCharacter != (char)Keys.Back && theCharacter != (char)Keys.Enter)
                {
                    string word = theTextBox.Text.Trim();
                    string[] wordArr = word.Split('.');
                    if (wordArr.Length > 1)
                    {
                        string afterDot = wordArr[1];
                        if (afterDot.Length >= 2 && (theTextBox.SelectionStart >= word.Length - 2))
                        {
                            return false;
                        }
                    }
                }

                //If this is the firstplace in the text do not allow digits but only signs
                if ((theTextBox.SelectionStart == 0) && (char.IsDigit(theCharacter) || (theCharacter == '.')))
                {
                    return false;
                }

                // Otherwise the character is perfectly fine for a decimal value and the character
                // may indeed be placed at the current insertion position.
                return true;
            }
        }

        /* Evaluate DValue
         * return true if MinimumDValue < DValue < MaximumDValue
         */
        private bool IsInAcceptableRange(double Value) {
            if ((MinimumDValue <= Value) && (Value <= MaximumDValue)) {
                return (true);
            }
            return(false);
        }
        #endregion
    }
}



