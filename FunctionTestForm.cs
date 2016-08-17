using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SilverbackXP
{
    public partial class FunctionTest_SXP : Form
    {
        //*************************************************
        // Delagate function for write communications
        //
        private SendMessage_Delegate WriteLine_CM_delegate;
        //*************************************************
        // Current command sent to WriteLine function
        //
        private string m_currentCommand;
        public string currentCommand
        {
            set
            {
                m_currentCommand = value;
            }
            get
            {
                return m_currentCommand;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="del_func">Delegate function to send information</param>
        public FunctionTest_SXP(SendMessage_Delegate del_func)
        {
            InitializeComponent();
            WriteLine_CM_delegate = del_func;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            //*****************************************
            // Warning message acceptance required
            //
            string message = "Use of this tool incorrectly may cause injury or death and/or could damage equipment if not used properly. Do you accept responsibility?";

            DialogResult result;

            //*********************************
            // Displays the MessageBox.
            //
            result = MessageBox.Show(message, "WARNING! " + System.DateTime.Now.ToString(),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
        /// Delegate declaration for
        /// </summary>
        /// <param name="message">String to be sent</param>
        /// <returns></returns>
        public delegate bool SendMessage_Delegate(string message);
        /// <summary>
        /// Top knife button open, click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTkOp_Click(object sender, EventArgs e)
        {
            // If necessary un-click the paired button
            if (btnTkCl.DialogResult == DialogResult.OK)
                btnTkCl.PerformClick();
            //--
            if (btnTkOp.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,tkop,";
                btnTkOp.BackColor = Color.Green;
                btnTkOp.DialogResult = DialogResult.OK;
            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnTkOp.BackColor = Color.Transparent;
                btnTkOp.DialogResult = DialogResult.None;
            }
            //--
            WriteLine_CM_delegate(currentCommand);
            //***********************************************
            //
            //
        }
        /// <summary>
        /// Top knife button close, click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTkCl_Click(object sender, EventArgs e)
        {
            // If necessary un-click the paired button
            if (btnTkOp.DialogResult == DialogResult.OK)
                btnTkOp.PerformClick();
            //--
            if (btnTkCl.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,tkcl,";
                btnTkCl.BackColor = Color.Green;
                btnTkCl.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnTkCl.BackColor = Color.Transparent;
                btnTkCl.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
            
        }
        /// <summary>*******************************************
        /// Wheel arm button open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWAop_Click(object sender, EventArgs e)
        {
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnWACl.DialogResult == DialogResult.OK)
                btnWACl.PerformClick();
            //--
            if (btnWAop.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,whop,";
                btnWAop.BackColor = Color.Green;
                btnWAop.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnWAop.BackColor = Color.Transparent;
                btnWAop.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Wheel arm button close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWACl_Click(object sender, EventArgs e)
        {
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnWAop.DialogResult == DialogResult.OK)
                btnWAop.PerformClick();
            //--
            if (btnWACl.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,whcl,";
                btnWACl.BackColor = Color.Green;
                btnWACl.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnWACl.BackColor = Color.Transparent;
                btnWACl.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>**********************************************
        /// Butt knife open button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBKOp_Click(object sender, EventArgs e)
        {
            //*****************************************************
            // If necessary un-click the paired button
            //
            if (btnBKCl.DialogResult == DialogResult.OK)
                btnBKCl.PerformClick();
            //--
            if (btnBKOp.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,bkop,";
                btnBKOp.BackColor = Color.Green;
                btnBKOp.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnBKOp.BackColor = Color.Transparent;
                btnBKOp.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>**********************************************
        /// Butt knife closed button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBKCl_Click(object sender, EventArgs e)
        {
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnBKOp.DialogResult == DialogResult.OK)
                btnBKOp.PerformClick();
            //--
            if (btnBKCl.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,bkcl,";
                btnBKCl.BackColor = Color.Green;
                btnBKCl.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnBKCl.BackColor = Color.Transparent;
                btnBKCl.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }

        private void btnWhFor_Click(object sender, EventArgs e)
        {
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnWARev.DialogResult == DialogResult.OK)
                btnWARev.PerformClick();
            //--
            if (btnWhFor.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,forw,";
                btnWhFor.BackColor = Color.Green;
                btnWhFor.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnWhFor.BackColor = Color.Transparent;
                btnWhFor.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }

        private void btnWARev_Click(object sender, EventArgs e)
        {
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnWhFor.DialogResult == DialogResult.OK)
                btnWhFor.PerformClick();
            //--
            if (btnWARev.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,reve,";
                btnWARev.BackColor = Color.Green;
                btnWARev.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnWARev.BackColor = Color.Transparent;
                btnWARev.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>*******************************************
        /// Main saw down button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMsaw_Click(object sender, EventArgs e)
        {

            //--
            if (btnMsaw.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,sawm,";
                btnMsaw.BackColor = Color.Red;
                btnMsaw.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnMsaw.BackColor = Color.Transparent;
                btnMsaw.DialogResult = DialogResult.None;
            }
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnMsawUp.DialogResult == DialogResult.OK)
            {
                btnMsawUp.BackColor = Color.Transparent;
                btnMsawUp.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Main saw up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMsawUp_Click(object sender, EventArgs e)
        {
            //--
            if (btnMsawUp.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $stp";
                btnMsawUp.BackColor = Color.Red;
                btnMsawUp.DialogResult = DialogResult.OK;
            }
            else
            {
                currentCommand = "wt41 $rdy";
                btnMsawUp.BackColor = Color.Transparent;
                btnMsawUp.DialogResult = DialogResult.None;
            }
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnMsaw.DialogResult == DialogResult.OK)
            {
                btnMsaw.BackColor = Color.Transparent;
                btnMsaw.DialogResult = DialogResult.None;
            }

            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Top saw down button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTsaw_Click(object sender, EventArgs e)
        {
            //--
            if (btnTsaw.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $cmd,sawt,";
                btnTsaw.BackColor = Color.Red;
                btnTsaw.DialogResult = DialogResult.OK;
            }
            else
            {
                currentCommand = "wt41 $stp,";
                btnTsaw.BackColor = Color.Transparent;
                btnTsaw.DialogResult = DialogResult.None;
            }
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnTopSawUp.DialogResult == DialogResult.OK)
            {
                btnMsaw.BackColor = Color.Transparent;
                btnMsaw.DialogResult = DialogResult.None;
            }

            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Top saw up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTopSawUp_Click(object sender, EventArgs e)
        {
            //--
            if (btnTopSawUp.DialogResult == DialogResult.None)
            {
                currentCommand = "wt41 $stp";
                btnTopSawUp.BackColor = Color.Red;
                btnTopSawUp.DialogResult = DialogResult.OK;
            }
            else
            {
                currentCommand = "wt41 $rdy";
                btnTopSawUp.BackColor = Color.Transparent;
                btnTopSawUp.DialogResult = DialogResult.None;
            }
            //********************************************
            // If necessary un-click the paired button
            //
            if (btnTsaw.DialogResult == DialogResult.OK)
            {
                btnTsaw.BackColor = Color.Transparent;
                btnTsaw.DialogResult = DialogResult.None;
            }

            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Pump 1 button on/off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPump1_Click(object sender, EventArgs e)
        {
            //--
            if (btnPump1.DialogResult == DialogResult.None)
            {
                currentCommand = "cm p0 on";
                btnPump1.BackColor = Color.Green;
                btnPump1.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "cm p0 off";
                btnPump1.BackColor = Color.Transparent;
                btnPump1.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }
        /// <summary>
        /// Pump 2 button on/off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPump2_Click(object sender, EventArgs e)
        {
            //--
            if (btnPump2.DialogResult == DialogResult.None)
            {
                currentCommand = "cm p1 on";
                btnPump2.BackColor = Color.Green;
                btnPump2.DialogResult = DialogResult.OK;

            }
            else
            {
                currentCommand = "cm p1 off";
                btnPump2.BackColor = Color.Transparent;
                btnPump2.DialogResult = DialogResult.None;
            }
            //****************************************
            // Send the command
            //
            WriteLine_CM_delegate(currentCommand);
        }

        private void FunctionTest_SXP_Load(object sender, EventArgs e)
        {

        }

        private void FunctionTest_SXP_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void FunctionTest_SXP_Enter(object sender, EventArgs e)
        {
            MessageBox.Show("hello");
        }

        private void FunctionTest_SXP_Deactivate(object sender, EventArgs e)
        {

        }

    }
}
