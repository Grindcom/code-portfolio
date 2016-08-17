using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
//using SilverbackXP.SilverbackDB;
using Utility;

namespace SilverbackXP
{
    /// <summary>
    /// Form initialization section
    /// </summary>
    public partial class SilverbackXP : Form
    {

        public SilverbackXP()//Equivilant to 'mainForm()' in previous BushApe
        {
            InitializeComponent();

            try
            {


                Error_Stamp("Start Main Form");


                // Gets a NumberFormatInfo associated with the en-US culture.
                nfi.NumberDecimalDigits = 4;
                //--------------------------------------------
                currentCommand = "$nop,\r\n";
                //--------------------------------------------

                masterThread = Thread.CurrentThread;
                masterThread.Priority = ThreadPriority.Normal;

                dataBase = new SilverbackDB();


                dataBase.LoadCommandNote(ref suCommandComboBox);

                // LOAD SETTINGS ------------------------------------------------
                dataBase.LoadSettings(ref bottomUpDown1, ref wheelUpDown1,
                    ref topUpDown1, ref speedUpDown, ref midUpDown,
                    ref udRampUp, ref udRampDwn, ref udRampStartSpeed);
                btmPres_trkBar.DataBindings.Add("Value", bottomUpDown1, "Value");
                wheelPres_trkBar.DataBindings.Add("Value", wheelUpDown1, "Value");
                topPres_trkBar.DataBindings.Add("Value", topUpDown1, "Value");
                speed_trkBar.DataBindings.Add("Value", speedUpDown, "Value");
                mid_trkBar.DataBindings.Add("Value", midUpDown, "Value");
                // Ramps
                tbRampSpeed.DataBindings.Add("Value", udRampStartSpeed, "Value");
                tbRampDwnLen.DataBindings.Add("Value", udRampDwn, "Value");
                tbRampUpLen.DataBindings.Add("Value", udRampUp, "Value");
                RampDwn = false;
                RampingUp = false;
                RampToPoint = 0.0M;
                RampDwnPoint = 0.0M;
                // ---------------------------------------------------------------
                //LOAD INFO ------------------------------------------------------
                dataBase.LoadInfo(ref msEncCB, SilverbackDB.MachineInfo.MainSawEnc);
                dataBase.LoadInfo(ref photoCB, SilverbackDB.MachineInfo.PhotoEye);
                dataBase.LoadInfo(ref usLenCB, SilverbackDB.MachineInfo.StandardMeas);
                dataBase.LoadInfo(ref tsawCB, SilverbackDB.MachineInfo.tSawPresent);
                dataBase.LoadInfo(ref tsEncCB, SilverbackDB.MachineInfo.tSawEnc);
                dataBase.LoadInfo(ref pwmCB, SilverbackDB.MachineInfo.PWM);
                dataBase.LoadInfo(ref floatBox, SilverbackDB.MachineInfo.TiltFloat);
                dataBase.LoadInfo(ref mfBox, SilverbackDB.MachineInfo.MultiFunc);
                dataBase.LoadInfo(ref semiAutoCB, SilverbackDB.MachineInfo.semiAuto);
                dataBase.LoadInfo(ref oneSawSelectCB, SilverbackDB.MachineInfo.oneSawButton);
                dataBase.LoadInfo(ref diaUpDown, SilverbackDB.MachineInfo.DiaEnc);
                //----------------------------------------------------------------
                //LOAD Bucking specs
                dataBase.LoadBuckingGrid(ref buckDG);
                dataBase.LoadBuckingGrid(ref lenTxtBx, ref minTxtBx, ref maxTxtBx);
                dataBase.LoadTypeName(ref cbBuckSpecies);
                cbBuckSpecies.Items.Insert(0, "All");// insert all at top of check box
                cbBuckSpecies.SelectedIndex = 0;
                //-----------------------------------------------------------------
                setUpFeedbacks();
                // Added 03/28
                qPassData = new Queue<Object>();
                gpioData = new Info();
                qInputData = new Queue<Object>();
                //-----------------------
                lastComm = new Info();

                // Syncronize communication with head module
                replyCntDwn = (int)replyUpDwn.Value; // 
                nopCntDwn = (int)nopUpDwn.Value; // 
                speedCntDwn = 1;// 1 millisecond
                autoCntDwn = 10;// 50 millisecond
                zeroSpan = 0;
                /*************************************************************
                 * This section may not be necessary
                //replyTmr = new System.Threading.Timer(reSend, null, -1, -1);
                //deleNopTmr = new EventHandler(this.nopTimer);
                //nopTmr = new MessageTimer(this, deleNopTmr, -1, -1, -1);
                //deleSyncTmr = new EventHandler(this.syncOps);
                //syncTmr = new MessageTimer(this, syncOps, -1, -1, -1);
                //deleAutoTmr = new EventHandler(this.autoTimer);
                //autoTmr = new MessageTimer(this, deleAutoTmr, -1, -1, 5);//Create, initialize
                //deleReplyTmr = new EventHandler(this.reSend);
                //replyTmr = new MessageTimer(this, deleReplyTmr, -1, -1, -1);
                 * 
                 * ***********************************************************/



                timeOutCnt = 0;// initialize
                nopCnt = 0;// initialize
                speedTimeMS = 0;// initialize
                autoTmrSentCnt = 0; //initialize
                string confirm = "";
                string ready;
                int index, count = 0;

                do
                {
                    if (count++ == 100)
                        break;
                    ready = "$rdy,\r\n";
                    //comOut.WriteLine(ready);
                    do
                    {
                        ready = confirm;
                        //confirm = SerialComm.getData(comOut.portNumber);
                    } while (!confirm.Equals(""));
                    confirm = ready;
                    index = confirm.IndexOf("$");
                    if (index > 0)
                        confirm = confirm.Substring(index);
                } while (!confirm.StartsWith("$rdy,\r\n"));
                lastComm.rx_tx = confirm;
                if (count >= 100)
                {
                    MessageBox.Show("Bad Connection");
                    Error_Stamp("Bad Connection: main");
                }
                qPassData.Enqueue(lastComm);
                //-----------End Sync Op's-------------------------
                //-------------------------------------------------
                // Begin monitoring head communication, continues in 'InterpretThreadData'
                deleReceiveData = new EventHandler(this.InterpretThreadData);
                this.Invoke(deleReceiveData);
                //comServ = new SerialCommService(this, deleReceiveData, qPassData, comOut.portNumber, "$nop,\r\n", 1500);
                //------------------------------------------------------------------------
                // Start thread to read GPIO handles
                deleInput = new EventHandler(this.InputThreadData);
                //input = new GPIOService(this, deleInput, qInputData, gpioData);
                //------------------------------------------------------------------------
                // Get/Set Pressures
                setPressure("top", (int)topUpDown1.Value);
                setPressure("wheels", (int)wheelUpDown1.Value);
                setPressure("bottom", (int)bottomUpDown1.Value);
                setPressure("speed", (int)speedUpDown.Value);
                setPressure("middle", (int)midUpDown.Value);
                setPressure("rampstart", (int)udRampStartSpeed.Value);
                // Get/Set Ramp speed
                setPressure("rampstart", (int)udRampStartSpeed.Value);
                // Get/Set Length scaler
                lengthScaler = dataBase.GetLengthScaler(1);
                diaScaler = dataBase.GetDiaScaler(1);

                lenRatioTxtBox.Text = lengthScaler.ToString("N", nfi);
                diaRatioTxtBox.Text = diaScaler.ToString("N", nfi);

                // Bucking specifications
                dataBase.SelectedSpecies = 3;
                dataBase.SelectedBuckingSpec = 0;
                //------------------------------------------------------------------------------------

                // Find the location of the saw input button
                sawInput = dataBase.GetInput("sawm");
                tsawInput = dataBase.GetInput("sawt");
                // add the saw toggle
                //dataBase.AddCommand(5, "togl", "Toggle Length from Butt/Top saws");
                //
                buttSawRB.Checked = true;
                SAWtoSAW = 1.35F;//1.35m from main saw to top saw.
                SAWoffset = 0.0F;// changes to SAWtoSAW value when top saw is selected.
                // initialize bucking
                dataBase.NextSpecies();
                RefreshTarget();
                //--------------------------------------------------------------------------------------
                string date = monthCalendar1.TodayDate.ToShortDateString();
                //--------------------------------------------------------------------------------------
                allPumpFlag = false;
                pump1Flag = false;
                pump2Flag = false;
                //----------------------------
                //Ramping initialization; include conversion to m from cm
                rmpUpStage = udRampUp.Value / (RAMPSTAGES * 100);
                rmpDwnStage = udRampDwn.Value / (RAMPSTAGES * 100);
                rmpStagePoint = 0;
                //rmpDwnLenStage =
                pbSpeed.Maximum = RAMPSTAGES;

                diaOffset = System.Convert.ToDecimal(closDiaTxtBox.Text);


                DialogResult result;
                // Displays the MessageBox.
                result = MessageBox.Show("Close Head and Press OK", System.DateTime.Now.ToString(),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                // Set backlight brightness
                switch (result)
                {
                    case DialogResult.OK:
                        // clear diameter when head is closed
                        //comOut.WriteLine("$cmd,cler,\r\n");
                        break;

                }

                //###############################################################
                //
                // Do Operator Selection Here!!!!!!!!!!!!!!
                //
                //###############################################################
            }
            catch (ThreadAbortException te)
            {
                Error_Stamp("thread abort, " + te.Message);
            }
            catch (Exception e)
            {
                string message = "Unable to initialize at main form, " +
                    " Reason: " +
                    e;
                MessageBox.Show(message);
                Error_Stamp(message);
            }

        }
        /// <summary>
        /// Indicate on the display what the target is.
        /// </summary>
        private void RefreshTarget()
        {
            try
            {
                if (semiAutoCB.Checked || autoCB.Checked)
                {
                    // Get the selected length
                    // Show target length
                    logLenlbl.Text = "Target: " + dataBase.SelectedLength.ToString() + " , " + dataBase.SelectedSpeciesName;
                }
                else
                    logLenlbl.Text = "Manual Feed";
            }
            catch (Exception e)
            {
                MessageBox.Show("Error at Target Refresh " + e.Message);
            }
        }//---------------End RefreshTarget-------------------------------------------------------- 
        /// <summary>
        /// Creates and/or adds to a file called error_SB.txt
        /// </summary>
        /// <param name="error">String to add to file</param>
        private static void Error_Stamp(String error)
        {
            //=============================================
            string errorFile = @"error_SB.txt";
            StreamWriter swErrorFile;


            if (!File.Exists(errorFile))
            {
                File.Create(errorFile).Close();
            }
            // File IO-----------------------------------

            swErrorFile = new StreamWriter(new FileStream(errorFile, FileMode.Append), System.Text.Encoding.ASCII);
            swErrorFile.Write(DateTime.Now + ",");// DateTime is a system level information type
            swErrorFile.WriteLine(error);
            swErrorFile.Close();//Calls 'dispose' using, swErrorFile after may cause exception
            //=============================================
        }
        /// <summary>
        /// Set the pressure (or speed) of a related valve.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pressure"></param>
        private void setPressure(string s, int pressure)
        {
            double dbl = 0;
            int temp = 0;
            // Do nothing if pressure is out of range.pressure >= 0 && 
            if (pressure <= 100)
            {// a 0 temp value corresponds to 100% pressue or 0 relief
                //dbl = (255.0 - 255.0 * ((double)pressure / 100));//change from 127.0; mar.30.2009
                //temp = (int)Math.Round(dbl);
                // Only send a command if a valid 's' is given
                string sendCommand;
                Utility.CommandPrep t2cmnd = new CommandPrep();
                switch (s)
                {
                    case "top":// Set top knife pressure
                        sendCommand = t2cmnd.setCommand("topk", pressure);
                        comOut.WriteLine(sendCommand);
                        break;
                    case "wheels":// Set Wheel arm pressure
                        sendCommand = t2cmnd.setCommand("whar", pressure);
                        comOut.WriteLine(sendCommand);
                        break;
                    case "bottom":// Set bottom knife pressure
                        sendCommand = t2cmnd.setCommand("botk", pressure);
                        comOut.WriteLine(sendCommand);
                        break;
                    case "speed":// Set speed
                        dbl = (64.0 * ((double)pressure / 100));
                        temp = (int)Math.Round(dbl);
                        sendCommand = t2cmnd.setCommand("whsp", temp);
                        comOut.WriteLine(sendCommand);
                        break;
                    case "middle":// Set middle pressure for wheels
                        dbl = 2.55 * (double)pressure;//2.55 is 1% or 255, this will let middle increment by 1% per unit.
                        temp = (int)Math.Round(dbl);
                        sendCommand = t2cmnd.setCommand("neut", temp);
                        comOut.WriteLine(sendCommand);
                        break;
                    case "rampstart":// Set the ramp speed start position
                        sendCommand = t2cmnd.setCommand("rmst", pressure);
                        comOut.WriteLine(sendCommand);
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void InterpretThreadData(object s, System.EventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void InputThreadData(object s, System.EventArgs e)
        {

        }

        /// <summary>
        /// Set up the feed back panels
        /// </summary>
        public void setUpFeedbacks()
        {
            // Direction
            dirRx.ID = dirTx.ID = (int)MachineOps.Direction;
            dirRx.State = dirTx.State = BushApe.TriVal.triNu;
            // Top Knife
            topRx.ID = topTx.ID = (int)MachineOps.TopKnife;
            topRx.State = topTx.State = BushApe.TriVal.triNu;
            // Wheel Arm
            whRx.ID = whTx.ID = (int)MachineOps.WheelArm;
            whRx.State = whTx.State = BushApe.TriVal.triNu;
            // Butt Knife
            butRx.ID = butTx.ID = (int)MachineOps.ButtKnife;
            butRx.State = butTx.State = BushApe.TriVal.triNu;
            // Main Saw
            mSawRx.ID = mSawTx.ID = (int)MachineOps.MainSaw;
            mSawRx.State = mSawTx.State = BushApe.TriVal.triHI;
            // Top Saw
            tSawRx.ID = tSawTx.ID = (int)MachineOps.TopSaw;
            tSawRx.State = tSawTx.State = BushApe.TriVal.triHI;
            // Tilt
            tiltRx.ID = tiltTx.ID = (int)MachineOps.Tilt;
            tiltRx.State = tiltTx.State = BushApe.TriVal.triNu;

            // Place all into respective arrays
            fbTx = new BushApe.FeedbackPanel[] { dirTx, topTx, whTx, butTx, tiltTx, mSawTx, tSawTx };
            fbRx = new BushApe.FeedbackPanel[] { dirRx, topRx, whRx, butRx, tiltRx, mSawRx, tSawRx };



        }
        

    }

}