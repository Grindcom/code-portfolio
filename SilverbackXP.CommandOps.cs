using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Utility;


namespace SilverbackXP
{
    public partial class silverbackXP
    {
        /// <summary>
        /// If a command status has changed this function will show its
        /// description in a text box; if the OpInput is not assigned 
        /// the text box will show the number value of the input.
        /// </summary>
        /// <param name="tb"></param>
        private void ShowInput(ref TextBox tb)
        {
            //*******************************************************************
            // Place the changed command into the text box and 
            // set the new operational input value
            //
            if (gpioData.CommandStatus)
            {

                // if OpInput is assigned
                if (dataBase.FilterUser_Command(OpInput))
                {
                    // show description in text box
                    tb.Text = dataBase.GetCmndDescrip(OpInput);
                    //*****************
                    // Check for continuity between database and CM
                    //
                    if (gpioData.CMsent.StartsWith("shif"))
                    {
                        //***************************************
                        // Set background color of text box
                        //
                        tb.BackColor = System.Drawing.Color.DimGray;
                        //***************************************
                        // Set Level to 2
                        //
                        input_lvl_UpDown.Value = 2;
                    }
                    else if (!gpioData.CMsent.StartsWith(dataBase.GetCmndStrng(OpInput)))
                    {
                        tb.BackColor = System.Drawing.Color.Red;
                    }
                    else { tb.BackColor = System.Drawing.Color.White; }
                }
                else// show number
                {
                    tb.Text = OpInput.ToString();
                    //*****************
                    // Check for continuity between database and CM
                    //
                    if (!gpioData.CMsent.StartsWith("none"))
                    {
                        tb.BackColor = System.Drawing.Color.Red;
                        MessageBox.Show("Not Synchronized with Cabin Module");
                        //
                        // Set the command text box to the CM's assigned command
                        //
                        string[] tempC = gpioData.CMsent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        uint tempCid = (uint)dataBase.GetCommandIDbyCommand(tempC[0]);
                        string descrip = dataBase.GetCmndDescrip(tempCid);
                        for(int i = 0; i < suCommandComboBox.Items.Count; i++)
                        {
                            suCommandComboBox.SelectedIndex = i;
                            if (suCommandComboBox.SelectedText.Equals(descrip))
                            {
                                break;
                            }

                        }
                        
                    }
                    else { tb.BackColor = System.Drawing.Color.White;  }
                }
            }
            gpioData.ResetStatus = true;
        }
        /// <summary>
        /// Interpret information recieved by the input handle thread
        /// </summary>
        public void InterpretGPIO(string gpioString)
        {
            uint pendingCommand = 0;
            try
            {
                string[] temp_line = gpioString.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
                //******************************************************
                // Make sure the index is valid
                //
                if (temp_line.Length < 4)
                    return;
                int index = Convert.ToInt32(temp_line[1]);
                string command = temp_line[3];
                char[] charsTotrim = { ',', ' ' };
                gpioData.CMsent = command.TrimEnd(charsTotrim);
                gpioData = input.iCommand;
                gpioData.command = (uint)index;


                //**********************************************************
                // OpInput is a global variable used in several functions
                // assing the input index to it.
                //
                OpInput = gpioData.command;
                //************************************************************
                // If the 

                //if (suInputTxtBox != null && tabControl1.SelectedIndex == 3)
                //{// Index 3 is the setup page

                //}
                if (tabControl1.SelectedIndex == tabControl1.TabPages.IndexOf(tpSetUp))
                {// Index 4 is the setup and Bucking spec page
                    //
                    // If an input has not been captured
                    // on the current cycle, have it 
                    // displayed in the appropriate text box
                    //
                    //if (!inputCaptured)
                    //{
                        if (assignCommand)
                            ShowInput(ref suInputTxtBox);
                        if (assignBuckspec)
                            ShowInput(ref assBuckTB);
                        //
                        // set the input captured flag
                        //
                        //inputCaptured = true;
                    //}

                }
                else if (dataBase != null)
                {
                    //
                    // Operations should work on all other pages as long 
                    // as the data base is available 
                    //
                    Utility.CommandPrep t2cmnd = new CommandPrep();
                    //
                    // If the current command has changed make it a pending 
                    //
                    if (gpioData.CommandStatus)
                    {
                        pendingCommand = gpioData.command;
                    }
                    //
                    // Display current command (as Last command)
                    //
                    GPIOlbl.Text = currentCommand;
                    //
                    // Get the type of command
                    //
                    int commandType = dataBase.GetCommandTypebyCommand(command);
                    //
                    // If head float is selected send the float command
                    //
                    if (floatBox.Checked && tiltRx.State != BushApe.TriVal.triNu)
                    {
                        if (WriteLine_CM("wt41 $cmd tifl "))
                        {
                            this.FeedbackDisplayTX("tifl");
                        }
                    }
                    //************************************************************
                    //
                    //
                    ExecuteCMD(gpioData.CMsent, commandType);
                    //
                    // Clear any filters present
                    //
                    dataBase.ClearFilterCommand = true;
                    //--------------------------------------------------------
                }
            }
            catch (ThreadAbortException)
            {
                Error_Stamp("Thread Abort Exception in BA 'InputTheadData(...)'");

            }
            catch (OutOfMemoryException)
            {
                //**********************************
                // Ensure pumps are off
                //
                if (WriteLine_CM(GPIO.GPIOHDR + GPIO.PALL_OFF))
                {
                    // try to restart
                    tabControl1.SelectedIndex = 0;
                    //
                    Error_Stamp("Out of Memory: Input Thread Data");
                }
            }
            catch (Exception er)
            {
                string message = "Exception at InputThreadData: " + er.Message;
                //MessageBox.Show(message);
                Error_Stamp(message + "GPIO interpret");
            }
            finally
            {
                //Error_Stamp("finally: Input Thread Data");
                // collect garbage memory
                if (GC.GetTotalMemory(false) > 750)
                {
                    GC.Collect();
                }
            }
        }
        /// <summary>
        /// Will execute the pending command
        /// </summary>
        /// <param name="pendingCommand"></param>
        /// <param name="commandType"></param>
        private void ExecuteCMD(string pendingCommand, int commandType)
        {
            string command = "none";
            int cmdType = 2;
            Info iType = new Info();
            Utility.CommandPrep t2cmnd = new CommandPrep();

            switch (commandType)
            {
                
                case 6:/*Type 6 commands are actually the bucking spec ID*/
                    //
                    // Get the bucking spec id from using the pending command
                    //
                    //string specID = dataBase.GetCmndStrng(pendingCommand);
                    string descrip;
                    if (dataBase.FilterUser_Command(OpInput))
                        descrip = dataBase.GetCmndDescrip(OpInput);
                    
                    //
                    // Change to new selected length
                    //
                    string remove = "ps";
                    command = pendingCommand.TrimStart(remove.ToCharArray());
                    dataBase.SelectedBuckingSpec = Convert.ToInt32(command);
                    decimal tLength = dataBase.SelectedLength;
                    //MessageBox.Show("In type 6!!! Length is " + tLength.ToString());
                    // Removed - call to RefreshTarget() on Aug. 27, 2009
                    // Added a call to SendStopAt() on same day.
                    // when a different bucking spec is selected the new selected
                    // length is sent to the head module;RefreshTarget() is called
                    // in that function.
                    if (semiAutoCB.Checked)
                        SendStopAt();

                    //------
                    //MessageBox.Show("Type 6 command");
                    break;
                case 5:// Type 5 operations involve using tables the tree_type table
                    {
                        //**************************************************************
                        // Get the type 5 operation information
                        //
                        switch (pendingCommand)
                        {
                            case "cler":/*Stop, end auto, reset head module*/
                                    // Sends the stop command and ends the auto function                            
                                    WriteLine_CM("wt41 $stp ");
                                    WriteLine_CM("wt41 $cmd eaut ");
                                    // Clear the length count
                                    //WriteLine_CM("$cmd,cler,\r\n");
                                    WriteLine_CM("wt41 $rst ");
                                break;
                            case "uptr":/*Next tree type*/
                                dataBase.NextSpecies();
                                //MessageBox.Show("The selected Species is " + dataBase.GetSelectedSpecies());
                                break;
                            case "dntr":/*Prev. tree type*/
                                dataBase.PrevSpecies();
                                //MessageBox.Show("The prev Species (now current) is " + dataBase.GetSelectedSpecies());
                                break;
                            case "upbuk":/*Next bucking spec up*/
                                dataBase.NextBuckSpec(SilverbackDB.RowUpDown.Up);
                                //MessageBox.Show("Target Length " + dataBase.SelectedLength.ToString());
                                break;
                            case "dnbuk":/*Next bucking spec down*/
                                dataBase.NextBuckSpec(SilverbackDB.RowUpDown.Down);
                                //MessageBox.Show("Target Length " + dataBase.SelectedLength.ToString());
                                break;
                            case "near":/*Nearest bucking spec.*/
                                float currentLength = System.Convert.ToSingle(lengthDisplay.Text);
                                if ((float)targetLength.GetValue(0) < currentLength)
                                {// the longest length is at index 0;
                                    // if it is smaller than currentLength it is the nearest value
                                    currentLenIndex = 0;
                                    break;
                                }
                                int i = 0;

                                try
                                {
                                    for (; i < targetLength.Length - 1; currentLenIndex = ++i)
                                    {// find the index where currentLength is just less than
                                        if (((float)targetLength.GetValue(i) > currentLength)
                                            && ((float)targetLength.GetValue(i + 1) < currentLength))
                                        {
                                            if (((float)targetLength.GetValue(i) - currentLength)
                                                <= (currentLength - (float)targetLength.GetValue(i + 1)))
                                                currentLenIndex = i;
                                            else
                                                currentLenIndex = i + 1;
                                            break;
                                        }
                                    }
                                }
                                catch (Exception i2err)
                                {
                                    //MessageBox.Show("i2err: " + i2err.Message);
                                    Error_Stamp("i2err: " + i2err.Message);
                                }
                                break;
                            case "prim":/*Primary Length*/
                                currentLenIndex = prefLenIndex;
                                break;
                            case "ckop":/* combine top and bottom open commands*/
                                
                                command = "tkop";
                                SendCurrentCMD(ref command, ref iType, ref t2cmnd, ref cmdType,true);
                                command = "bkop";
                                SendCurrentCMD(ref command, ref iType, ref t2cmnd, ref cmdType,true);
                                break;
                            case "ckcl":/* combine top and bottom close commands*/                                
                                command = "tkcl";
                                SendCurrentCMD(ref command, ref iType, ref t2cmnd, ref cmdType,true);
                                command = "bkcl";
                                SendCurrentCMD(ref command, ref iType, ref t2cmnd, ref cmdType,true);
                                break;
                            case "togl":/*toggle from main saw to top saw*/
                                if (buttSawRB.Checked)
                                    topSawRB.Checked = true;
                                else if (topSawRB.Checked)
                                    buttSawRB.Checked = true;
                                break;
                            case "sawb":/*Send saw command based on saw radio button selected*/
                                //
                                // Check which radio button is selected; butt or top
                                //
                                if (buttSawRB.Checked)
                                {
                                    command = "msaw";
                                }
                                if (topSawRB.Checked)
                                {
                                    command = "tsaw";
                                }
                                //
                                // Send the command
                                //
                                SendCurrentCMD(ref command, ref iType, ref t2cmnd, ref cmdType,true);
                                break;
                            case "news":
                                // Add new record to stem table and show the current tree count
                                treeCntlbl.Text = dataBase.AddStem().ToString();
                                //
                                // Send the Start Command if in Auto
                                //
                                //if (autoCB.Checked)
                                //{
                                //    currentCommand = "wt41 $cmd forw";
                                //    if (WriteLine_CM(currentCommand))
                                //    {
                                //        //
                                //        // send command
                                //        //
                                //        
                                //        // also send the current command to the feedback panels
                                //        this.FeedbackDisplayTX("forw");
                                //        //Start reply timer
                                //        replyTimer.Start();/*Stop during bench testing*/
                                //        //---------
                                //    }

                                //}
                                break;
                            case "over":/*Override auto and stop*/
                                currentCommand = "wt41 $stp";
                                if (WriteLine_CM(currentCommand))
                                {
                                    //		send command
                                    
                                    // also send the current command to the feedback panels
                                    this.FeedbackDisplayTX("stp");
                                    //Start reply timer
                                    replyTimer.Start();/*Stop during bench testing*/
                                    //---------
                                }
                                break;
                            default:
                                //MessageBox.Show("Default: " + op);
                                break;
                        }
                        // SendStopAt() is called when the photo eye goes high, targetLength.GetValue(currentLenIndex)
                        RefreshTarget();


                    }// end case 5	
                    break;
                case 4://  if it is type 4, call the main form function
                    {

                    }// end case 4
                    break;
                //  if it is type 2:
                case 2:
                    {
                        FeedbackDisplayTX(pendingCommand);
                        SendCurrentCMD(ref pendingCommand, ref iType, ref t2cmnd, ref commandType,false);

                    }// end case 2
                    break;
                case 1:
                    {
                        //
                        // stop is pendingCommand 0. Stop should be 
                        // ignored in auto feed mode so the feed button 
                        // can be pressed once to start log feed; unless the saws are out.
                        //
                        //if ((pendingCommand == 0) && autoCB.Checked && (mSawRx.State != BushApe.TriVal.triLO || tSawRx.State != BushApe.TriVal.triLO))
                        //{/*direction is forward ignore stop*/
                        //    if (dirRx.State == BushApe.TriVal.triHI)
                        //    {
                        //        dirTx.State = BushApe.TriVal.triNu;
                        //        break;
                        //    }
                        //}
                        //
                        // Update the current command
                        //
                        SendCurrentCMD(ref pendingCommand, ref iType, ref t2cmnd, ref commandType, false);

                    }// End Case 1
                    break;
                //
                //A negative 1 means the pending command was not found
                //
                case -1:

                    break;
            }// End Switch commandType
        }
        /// <summary>
        /// Change pending command to the current command, 
        /// sends it and starts the reply timer.
        /// </summary>
        /// <param name="pendingCommand">New command</param>
        /// <param name="iType">Information type to extract command string</param>
        /// <param name="t2cmnd">to create entire command</param>
        /// <param name="sendToPort">Whether to send result to COM port</param>
        private void SendCurrentCMD(ref string pendingCommand, ref Info iType,
            ref Utility.CommandPrep t2cmnd, ref int cmdType, bool sendToPort)
        {
            //		get command string
            iType.rx_tx = pendingCommand;
            // Added 07.27.2009
            //		prep a type 2 command with 'iType.command'
            switch (cmdType)
            {
                case 1: currentCommand = t2cmnd.type1command(iType.rx_tx);
                    break;
                case 2:

                    // make a type 2 command
                    currentCommand = t2cmnd.type2command(iType.rx_tx);
                    break;
                default:
                    break;
            }
            // also send the current command to the feedback panels
            this.FeedbackDisplayTX(iType.rx_tx);
            //Start reply timer
            //replyTimer.Start();/*Stop during bench testing*/
            //---------
            if (sendToPort)
            {
                //		send command
                WriteLine_CM(currentCommand);
            }
        }
      

    }
}
