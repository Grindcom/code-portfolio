using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//
// Added by Grindcom
//
using System.IO.Ports;

namespace SilverbackXP
{
    public partial class terminal_SXP : Form
    {
        /// <summary>
        /// Reference to a serial port
        /// </summary>
        public SerialPort ref_serialPort;
        /// <summary>
        /// Terminal constructor, Sets form to visible = true if the warning is accepted.
        /// </summary>
        /// <param name="sp_ref">reference to the associated serial port</param>
        public terminal_SXP(ref SerialPort sp_ref)
        {
            InitializeComponent();
            ref_serialPort = sp_ref;
            //*****************************************
            // Warning message acceptance required
            //
            string message = "Use of this tool incorrectly may cause injury or death and/or could damage equipment if not used properly. Do you accept responsibility?";

            DialogResult result;

            //*********************************
            // Displays the MessageBox.
            //
            result = MessageBox.Show(message, "WARNING! ",MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
            //**************************************
            // When ok is selected clear diameter
            switch (result)
            {
                case DialogResult.No:
                    this.Dispose();
                    break;
                case DialogResult.Yes:
                    this.Visible = true;
                    return;

            }
        }
        /// <summary>
        /// Handler for terminal keyboard entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void terminalTB_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    
                    if (ref_serialPort.IsOpen)
                    {
                        //**************************************************
                        // If serial port is open, send the current line
                        //
                        string tosend;
                        string[] textArr = terminalTB.Lines;
                        if (textArr.Length > 1)
                        {
                            tosend = textArr[textArr.Length - 1] + " ";
                        }
                        else if (!textArr[0].StartsWith(" "))
                        {
                            tosend = textArr[0] + " ";
                        }
                        else return;
                        
                        ref_serialPort.WriteLine(tosend);
                    }
                    else
                    {
                        MessageBox.Show("Port is not open!");
                    }                  
                    
                    break;

            };
        }
        /// <summary>
        /// Write a line to the text box.
        /// </summary>
        /// <param name="msg">Line or character to be added to the text box.</param>
        public void terminalTB_WriteLine(string msg)
        {
            try
            {
                if (msg.EndsWith("\r\n"))
                {
                    //*****************************************
                    // Show cr & lf as characters
                    //
                    string charoff = "\r\n";
                    msg = msg.TrimEnd(charoff.ToCharArray());
                    charoff = msg + @"\r\n";
                    terminalTB.AppendText(charoff + "\r\n");
                }
                else
                {
                    terminalTB.AppendText(msg + "\r\n");
                }
                
            }
            catch (Exception e_TB_WriteLine)
            {
                MessageBox.Show(e_TB_WriteLine.Message, "Error in terminalTB_WriteLine.");
            }
        }

        private void terminal_SXP_Deactivate(object sender, EventArgs e)
        {
            //MessageBox.Show("Deactivating", "In terminal_SXP_Deactivate");
        }
    }
}
