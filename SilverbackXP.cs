using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using BushApe;
using Utility;
using System.Media;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
//using System.ComponentModel;
//using System.IO.Ports;
using EGIS.ShapeFileLib;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;

using CTL_HotKey;


namespace SilverbackXP
{
    public partial class silverbackXP : Form
    {
        /// <summary>
        /// MAC Address of intended target for software.
        ///  Case sensitive; All CAPS only.02B8C5198701 Chad's:0022645EAB30, ,
        /// </summary>
        //private string  DISPLAYMAC= "00015301884A";
        //private string PCMAC = "";
        private string UNITMACADDRESS1 = "001F3A43CE47";//
        private string UNITMACADDRESS2 = "001F3A43CE47";
        private string UNITMACADDRESS3 = "00015301884A";
        /// <summary>
        /// Initialize everything to do with the main form.
        /// </summary>
        /// 
        public string selectIcon = null;

        /// <summary>
        /// Private variables
        /// </summary>
        /// 
        private bool DemoMode = false;
        private string ComSettingsFilename = @"Settings\\com_settings.txt";
        private string HeadParametersFilename = @"Settings\\head_parameters.txt";

        private string DefaultPortForGPS;
        //private SerialPort serialPort2 = null;
        private double StartingZoomLevel;
        private double SelectingStaticRadious;

        #region fullScreenVariable
        [DllImport("user32.dll")]
        private static extern int FindWindow(string lpszClassName, string lpszWindowName);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;
        private bool fullscreenState;
        #endregion
        #region const and dll functions for moving form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
            int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion



        public silverbackXP()
        {

            CTL_HotKey.CTL_HotKey key = new CTL_HotKey.CTL_HotKey();
            key.SetFdelegate(F_message);

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            int nic_size = nics.Length - 1;
            PhysicalAddress macs;
            do
            {
                macs = nics[nic_size--].GetPhysicalAddress();
                if (String.Compare(UNITMACADDRESS1, macs.ToString()) == 0)
                {
                    //MessageBox.Show(macs.ToString(), "MAC Address");
                    break;
                }
                else if (String.Compare(UNITMACADDRESS2, macs.ToString()) == 0)
                {
                    //MessageBox.Show(macs.ToString(), "MAC Address");
                    break;
                }
                else if (String.Compare(UNITMACADDRESS3, macs.ToString()) == 0)
                {
                    //MessageBox.Show(macs.ToString(), "MAC Address");
                    break;
                }
                else if (nic_size < 0)
                {
                    MessageBox.Show("Please Contact Your CTL Rep For Assistance", "INVALID SOFTWARE");
                    return;
                }
            } while (nic_size >= 0);



            fullscreenState = false;
            //DefaultPortForGPS = "COM9";
            StartingZoomLevel = 0.0;
            //For setting ZoomLevel * 1 the SelectingStaticRadious should be 60.00
            SelectingStaticRadious = 65.00;
            try
            {
                InitializeComponent();

                //ComPortScan scanports = new ComPortScan();
                //string allComAndLTPports = scanports.allPortNames();
                //string gpsPortName = scanports.selectGpsPort(allComAndLTPports);
                string gpsPortName = "com10";
                //MessageBox.Show(""+gpsPortName);

                //
                // Change this to use a set up text file

                if (string.IsNullOrEmpty(gpsPortName))
                {
                    MessageBox.Show("Gps device not found\nUsing Default port COM9");
                    DefaultPortForGPS = "COM9";
                }

                else
                {
                    DefaultPortForGPS = gpsPortName;
                }

                try
                {
                    //IContainer components = new Container();
                    //GPSserialPort2 = new SerialPort(components);
                    GPSserialPort2.BaudRate = 4800;
                    GPSserialPort2.PortName = DefaultPortForGPS; //default for device

                }
                catch (Exception ex_initComp)
                {
                    MessageBox.Show(ex_initComp.Message, "Initialize Component");
                }


                //***********************************************************
                // Default main form size
                //
                defaultSize = new Size(805, 492);
                //defaultSize = new Size(803, 498);

                //***********************************************************
                // Default Font sizes
                //
                defDiaFont = diaDisplay.Font;
                defLenFont = lengthDisplay.Font;
                //**********************************************************
                // Default group locations
                //
                defgrpDiaLocation = grpDia.Location;

                //*******************************************************
                // Indicate a new startup
                //
                Error_Stamp("Start Main Form");
                // Gets a NumberFormatInfo associated with the en-US culture.
                nfi.NumberDecimalDigits = 4;
                //--------------------------------------------
                currentCommand = "wt41 $nop,\r\n";
                //--------------------------------------------
                //masterThread = System.Threading.Thread.CurrentThread;
                //masterThread.Priority = System.Threading.ThreadPriority.Normal;

                dataBase = new SilverbackDB();
                dataBase.LoadCommandNote(ref suCommandComboBox);

                // LOAD SETTINGS ------------------------------------------------
                //
                // Must be on the Settings tab to get correct settings
                //
                tabControl1.SelectedIndex = 3;
                //
                // Load from the database
                //
                dataBase.LoadSettings(ref bottomUpDown1, ref wheelUpDown1,
                    ref topUpDown1, ref speedUpDown, ref midUpDown,
                    ref udRampUp, ref udRampDwn, ref udRampStartSpeed);
                btmPres_trkBar.DataBindings.Add("Value", bottomUpDown1, "Value");
                wheelPres_trkBar.DataBindings.Add("Value", wheelUpDown1, "Value");
                topPres_trkBar.DataBindings.Add("Value", topUpDown1, "Value");
                speed_trkBar.DataBindings.Add("Value", speedUpDown, "Value");
                mid_trkBar.DataBindings.Add("Value", midUpDown, "Value");
                //
                // Test track bar bindings
                ////
                //tbMaxTKPR.DataBindings.Add("Value", tkMaxPRnud, "Value");
                //tbMaxWAPR.DataBindings.Add("Value", waMaxPRnud, "Value");
                //tbMaxBKPR.DataBindings.Add("Value", bkMaxPRnud, "Value");
                //
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
                dataBase.LoadInfo(ref oneSawSelectCB, SilverbackDB.MachineInfo.oneSawButton);
                dataBase.LoadInfo(ref diaUpDown, SilverbackDB.MachineInfo.DiaEnc);
                dataBase.LoadInfo(ref cbUseRamps, SilverbackDB.MachineInfo.UseRamps);
                //----------------------------------------------------------------
                //LOAD Bucking specs
                dataBase.LoadBuckingGrid(ref buckDG);
                //dataBase.LoadBuckingGrid(ref lenTxtBx, ref minTxtBx, ref maxTxtBx, ref underWinTxtBx, ref overWinTxtBx);

                //
                // Load the bucking tab combo box
                //
                dataBase.LoadTypeName(ref cbBuckSpecies);
                cbBuckSpecies.SelectedIndex = 0;
                //
                // Load the type combo box on the Calibrate tab
                //
                dataBase.LoadTypeName(ref cbTypeCalTab);
                //-----------------------------------------------------------------
                setUpFeedbacks();
                // -----------------------
                gpioData = new Utility.Info();
                //-----------------------
                lastComm = new Utility.Info();

                // Syncronize communication with head module
                replyTimer.Interval = (int)replyUpDwn.Value; // 
                nopCntDwn = (int)nopUpDwn.Value; // 
                //speedCntDwn = 1;// 1 millisecond
                //autoCntDwn = 10;// 50 millisecond
                //zeroSpan = 0;
                timeOutCnt = 0;// initialize
                nopCnt = 0;// initialize
                speedTimeMS = 0;// initialize
                //autoTmrSentCnt = 0; //initialize
                /***************************************************
                 * Serial port, presets
                 * *****************/
                CMserialPort1.NewLine = "\r\n";

                string tsdFile = ComSettingsFilename;
                DemoMode = SetupComs(tsdFile);


                //***********************************************************
                // User terminal
                //
                //terminal = new terminal_SXP(ref serialPort1);
                /***************************************************
                 * Open com port
                 * *****************/
                // Make sure the com port exists before opening it
                //
                string[] ports = SerialPort.GetPortNames();
                bool validCMPort = DemoMode;
                bool validGPSPort = DemoMode;
                //************************************
                // Confirm the ports exist and open without error
                //
                ///
                /// This needs work, the logic doesn't test the presence of the com device as desired.
                /// 

                foreach (string port in ports)
                {
                    //
                    // Check for Cabin module port availability
                    //
                    if (port.StartsWith(CMserialPort1.PortName))
                    {
                        try
                        {
                            CMserialPort1.Open();
                            validCMPort = true;
                        }
                        catch (IOException e_serialPort)
                        {
                            MessageBox.Show(e_serialPort.Message + ": Cab Module Communication are in invalid state.", "SXP Lite - Com Error");
                        }

                    }
                    //
                    // Check for GPS port availability
                    //
                    if (port.StartsWith(GPSserialPort2.PortName))
                    {
                        try
                        {
                            GPSserialPort2.Open();
                            validGPSPort = true;
                        }
                        catch (IOException e_serialPort)
                        {
                            MessageBox.Show(e_serialPort.Message + ": GPS Communication are in an invalid state.", "SXP Lite - Com Error");
                        }

                    }
                }
                //***********************************
                // Perform initial synchronize with head module
                //
                if (validCMPort)
                {
                    string confirm = "";
                    //string ready;
                    //int index, count = 0;
                    //ready ;// "$rdy,";// ready is used for direct to
                    currentCommand = "wt41 $rdy";//= ready;
                    if (WriteLine_CM(currentCommand))
                    {
                        /*WriteLine adds \r\n*/
                        //WriteLine_CM(ready);
                        //WriteLine_CM(ready);
                        //WriteLine_CM(ready);
                        //replyTimer.Start();
                    }
                    lastComm.rx_tx = confirm;
                }
                else
                {
                    MessageBox.Show("Selected com port does not exist, proceed with Demo mode start.", "SXP Lite - Com Error");
                }


                //***************************************************************
                // Create a GPIO input variable
                //
                input = new GPIOService(gpioData);
                //------------------------------------------------------------------------
                //**********************************************************************
                // Get/Set Length scaler
                //
                lengthScaler = dataBase.GetLengthScaler(1);
                diaScaler = dataBase.GetDiaScaler(1);

                lenRatioTxtBox.Text = lengthScaler.ToString("N", nfi);
                diaRatioTxtBox.Text = diaScaler.ToString("N", nfi);
                //***********************************************************************
                // Bucking specifications
                //
                dataBase.SelectedSpecies = 1;
                dataBase.SelectedBuckingSpec = 1;
                //------------------------------------------------------------------------------------

                // Find the location of the saw input button
                sawInput = dataBase.GetInput("sawm");

                if (tsawCB.Checked)
                    tsawInput = dataBase.GetInput("sawt");

                buttSawRB.Checked = true;/*Default Selected Saw*/
                nudSawToSaw.Value = SAWtoSAW;/*main saw to top saw.*/
                SAWoffset = 0.0m;/*changes to SAWtoSAW value when top saw is selected.*/
                //***********************************************************
                // Set minimum piece length to store
                //
                nudMinLenStore.Value = MinLengthToStore;

                //***********************************************************
                // initialize bucking
                //
                dataBase.NextSpecies();
                //
                RefreshTarget();
                //--------------------------------------------------------------------------------------
                allPumpFlag = false;
                pump1Flag = false;
                pump2Flag = false;
                //--------------------------------------------------------------------------------------
                // Tree and Piece Count variables and flags
                addLog = false;
                maxTopLength = SAWtoSAW;
                //----------------------------***************************
                //Ramping initialization; include conversion to m from cm
                //
                rmpUpStage = udRampUp.Value / (RAMPSTAGES * 100);
                rmpDwnStage = udRampDwn.Value / (RAMPSTAGES * 100);
                rmpStagePoint = 0;
                //*****************************************************
                //rmpDwnLenStage =
                pbSpeed.Maximum = RAMPSTAGES;

                diaOffset = System.Convert.ToDecimal(closDiaTxtBox.Text);

                if (validCMPort)
                {
                    DialogResult result;
                    //*********************************
                    // Displays the MessageBox.
                    //
                    result = MessageBox.Show("Close Head and Press OK", System.DateTime.Now.ToString(),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    //**************************************
                    // When ok is selected clear diameter
                    switch (result)
                    {
                        case DialogResult.OK:
                            // clear diameter when head is closed
                            WriteLine_CM("wt41 $cmd,clrd,");
                            break;

                    }
                }
                //
                // Initialize length group box back color
                //
                grpLength.BackColor = phol_back_color;
                //
                /*Possible sounds for length window*/
                SystemSounds.Beep.Play();
                SystemSounds.Hand.Play();/*Favourite*/
                SystemSounds.Question.Play();
                //
                // Set up F Keys
                //

                //
                // Unavailable Features
                //
                // Tabpages 5 and 6; to allow access go to tabControl1_Selected(...) on line 3409

            }
            catch (ThreadAbortException te)
            {
                Error_Stamp("thread abort, " + te.Message);
            }
            catch (IOException e_serialPort)
            {
                MessageBox.Show(e_serialPort.Message + ": Proceedin in invalid state.", "SXP Lite - Com Error");
            }
            catch (Exception e)
            {
                string message = "Unable to initialize at main form, " +
                    " Reason: " +
                    e.Message;
                MessageBox.Show(message, "SilverbackXP.mainform");
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Setup com ports; assignable from a text file,
        /// If the file exists the ports are set with the information it contains.
        /// If the file doesn't exitst one is created using the default settings.
        /// </summary>
        /// <param name="tsdFile">Name, including sub-folder, of the com setting file</param>
        /// <returns>True if filename DOES NOT exist, indicating go directly to demo mode</returns>
        private bool SetupComs(string tsdFile)
        {
            if (!File.Exists(tsdFile))
            {
                /***********************
                 * Com setting file missing
                 */
                MessageBox.Show("COM File missing, Creating Default", "File Missing");
                /***********************
                 * Create Com settings file
                 */
                File.CreateText(tsdFile).Close();
                /***********************
                 * Populate with default settings
                 */
                StreamWriter sw = new StreamWriter(new FileStream(tsdFile, FileMode.Append), System.Text.Encoding.ASCII);
                /***********************
                 * Cabin Module COM port default settings
                 */
                sw.Write("Cabin Module," + CMserialPort1.PortName.ToString() + ",");
                sw.WriteLine(CMserialPort1.BaudRate.ToString());
                //
                // GPS Port Settings
                //
                sw.Write("GPS," + GPSserialPort2.PortName.ToString() + ",");
                sw.WriteLine(GPSserialPort2.BaudRate.ToString());
                //
                // Auto generation stamp
                //
                sw.WriteLine("\r\n### Default File, Generated on " + DateTime.Now + " ###");

                sw.Close();
                /****************************
                 * Set to demonstration mode
                 */
                return true;
            }
            else
            {
                /***************************
                 * Read the Comm settings
                 */
                //
                // Create File reader
                //
                StreamReader sr = new StreamReader(tsdFile);
                //
                // Read the first line
                //
                string tempLine = sr.ReadLine();
                //
                // Split text line by comma
                //
                string[] tempText = tempLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //
                // Make sure the first line starts correctly and then
                // use the port settings
                // OR indicate using default settings.
                //
                if (tempText[0].StartsWith("Cabin Module"))
                {
                    CMserialPort1.PortName = tempText[1];
                    CMserialPort1.BaudRate = System.Convert.ToInt32(tempText[2]);
                }
                else
                {
                    /*********************
                     * Error message
                     */
                    MessageBox.Show("COM File has incorrect structure for Cabin module\r\nUsing default settings...", "File Error");
                }
                //
                // Read the next line
                //
                tempLine = sr.ReadLine();
                tempText = tempLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //
                // Make sure the Second line starts correctly and then
                // use the port settings
                // OR indicate using default settings.
                //
                if (tempText[0].StartsWith("GPS"))
                {
                    GPSserialPort2.PortName = tempText[1];
                    GPSserialPort2.BaudRate = System.Convert.ToInt32(tempText[2]);
                }
                else
                {
                    /*********************
                     * Error message
                     */
                    MessageBox.Show("COM File has incorrect structure for GPS\r\nUsing default settings...", "File Error");
                }

                sr.Close();
                /****************************
* Set to normal operation mode
*/
                return false;
            }
        }

        /// <summary>
        /// Open the Head and Input com port and display the information
        /// </summary>
        private void full_maximize(object sender, EventArgs e)
        {
            // First, Hide the taskbar

            int hWnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hWnd, SW_HIDE);

            // Then, format and size the window. 
            // Important: Borderstyle -must- be first, 
            // if placed after the sizing functions, 
            // it'll strangely firm up the taskbar distance.
            FormBorderStyle = FormBorderStyle.None;
            this.Location = new Point(0, 0);
            this.WindowState = FormWindowState.Maximized;

        }

        private void OpenComPort()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            if (ports == null)
                return;
            foreach (string port in ports)
            {
                if (port.StartsWith("COM1"))
                {
                    MessageBox.Show("Com1", "OpenComPort");
                    CMserialPort1.PortName = port;
                    CMserialPort1.BaudRate = 115200;// 230400;
                    CMserialPort1.Open();
                    if (!CMserialPort1.IsOpen)
                        MessageBox.Show("Not Open", "Head Module Port");
                    else
                    {
                        tsslblPort1.Text = CMserialPort1.PortName;
                        tsslblBPS.Text = CMserialPort1.BaudRate.ToString() + "bps";
                        tsslblDBits.Text = CMserialPort1.DataBits.ToString();
                        tsslblParity.Text = CMserialPort1.Parity.ToString();
                        tsslblStpB.Text = CMserialPort1.StopBits.ToString();
                        tsslblFlCon.Text = CMserialPort1.RtsEnable.ToString();
                        tsslblConnectP1.Text = "Open";
                        CMserialPort1.DiscardInBuffer();/*Clear the input buffer*/
                    }
                }
            }
        }

        #region resize
        private void panel1_resize_manual(int width, int height)
        {

            double calculate = (796.0 / 805.0) * width;
            panel1.Width = (int)Math.Ceiling(calculate);
            CrossBtn.Location = new Point(panel1.Width - 36, CrossBtn.Location.Y);
            MxMinBtn.Location = new Point(panel1.Width - 71, CrossBtn.Location.Y);
            minimizeBtn.Location = new Point(panel1.Width - 106, CrossBtn.Location.Y);
        }

        private void tabControl1_resize_manual(int width, int height)
        {

            double calculate = (800.0 / 803.0) * width;
            tabControl1.Width = (int)Math.Ceiling(calculate);
            calculate = (430.0 / 520.0) * height;
            tabControl1.Height = (int)Math.Ceiling(calculate);
        }


        private double line_length(int x1, int y1, int x2, int y2)
        {

            return Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));
        }

        private void MovementSettings_resize_manual(int width, int height)
        {
        }

        private void Calibrate_resize_manual(int width, int height)
        {
        }

        private void OperationTab_resize_manual(int width, int height)
        {

            grpLength.Width = (int)line_length(grpLength.Location.X, grpLength.Location.Y, grpDia.Location.X - 13, grpDia.Location.Y);
            grpFeedback.Width = (int)line_length(grpFeedback.Location.X, grpFeedback.Location.Y, grpProduction.Location.X - 10, grpProduction.Location.Y);
            lengthDisplay.Size = new Size((int)((300 * grpLength.Size.Width) / 520), (int)((135 * grpLength.Size.Height) / 250));
            lengthDisplay.Font = new Font(lengthDisplay.Font.Name, (int)((65 * grpLength.Size.Height) / 250), lengthDisplay.Font.Style);
            AltLenDisplay.Size = new Size((int)(AltLenDisplay.Size.Width), (int)((93 * grpLength.Size.Height) / 250));
        }


        private void default_movement_setting(Control control, Point p, Size s)
        {

            control.Location = p;
            control.Size = s;
        }

        double Ratio_calculation(int control_parameter, int wrapper_parameter)
        {

            return control_parameter / wrapper_parameter;
        }


        private void SilverbackXP_Resize(object sender, EventArgs e)
        {
            try
            {
                tabControl1.Refresh();

                Control control = (Control)sender;

                #region panel and TabControl resize
                tabControl1_resize_manual(control.Width, control.Height);
                panel1_resize_manual(control.Width, control.Height);
                #endregion

                #region Calibration global
                grpLenCal.Width = (this.tabControl1.Width / 2) - 10;
                grpDiaCal.Location = new Point((grpLenCal.Location.X + grpLenCal.Width) + 10, grpDiaCal.Location.Y);
                grpDiaCal.Width = ((this.tabControl1.Width - this.grpLenCal.Width) - 20) - 5;
                #endregion

                if (control.Size.Height > defaultSize.Height && fullscreenState)
                {

                    tabControl1.Refresh();

                    #region Operation tab
                    int ratio_Font_lengthDisplay = tabControl1.Height - grpFeedback.Height;
                    lengthDisplay.Font = new Font(lengthDisplay.Font.Name, (int)((65 * ratio_Font_lengthDisplay) / 250), lengthDisplay.Font.Style);
                    mSawLabel.Location = new Point(mSawTx.Location.X + 3, mSawLabel.Location.Y);
                    tSawLabel.Location = new Point(tSawTx.Location.X + 3, tSawLabel.Location.Y);
                    tiltLabel.Location = new Point(tiltRx.Location.X - 5, tiltLabel.Location.Y);
                    dirLabel.Location = new Point(dirTx.Location.X + 3, dirLabel.Location.Y);
                    revLabel.Location = new Point(topTx.Location.X + 3, revLabel.Location.Y);
                    whLabel.Location = new Point(whTx.Location.X + 3, whLabel.Location.Y);
                    butLabel.Location = new Point(butTx.Location.X + 3, butLabel.Location.Y);
                    lblP1.Location = new Point(fbPump1.Location.X + 3, lblP1.Location.Y);
                    lblP2.Location = new Point(fbPump2.Location.X + 3, lblP2.Location.Y);
                    #endregion

                    #region calibrate

                    movpulslbl.Location = new Point(label28.Location.X + 100, label28.Location.Y + 80);
                    lenMovPulses.Location = new Point(movpulslbl.Location.X + movpulslbl.Width + 15, movpulslbl.Location.Y);

                    plsTargetlbl.Location = new Point(movpulslbl.Location.X, movpulslbl.Location.Y + 50);
                    lenPlsTarget.Location = new Point(plsTargetlbl.Location.X + plsTargetlbl.Width + 15, plsTargetlbl.Location.Y);

                    label27.Location = new Point(plsTargetlbl.Location.X, plsTargetlbl.Location.Y + 50);
                    grpDispLen.Location = new Point(label27.Location.X + label27.Width + 1, label27.Location.Y - 9);

                    ratiolbl.Location = new Point(label27.Location.X, label27.Location.Y + 70);
                    lenRatioTxtBox.Location = new Point(grpDispLen.Location.X, ratiolbl.Location.Y);
                    btnClearPulse.Location = new Point(lenRatioTxtBox.Location.X + lenRatioTxtBox.Width + 30, lenRatioTxtBox.Location.Y);

                    measlbl.Location = new Point(ratiolbl.Location.X, ratiolbl.Location.Y + 50);
                    txtBoxMl.Location = new Point(lenRatioTxtBox.Location.X, measlbl.Location.Y + 15);
                    label18.Location = new Point(txtBoxMl.Location.X + txtBoxMl.Width + 5, txtBoxMl.Location.Y);
                    lblMetric.Location = new Point(measlbl.Location.X, txtBoxMl.Location.Y + txtBoxMl.Height + 5);

                    calAcceptBtn.Location = new Point(txtBoxMl.Location.X, lblMetric.Location.Y + 50);



                    diaMovlbl.Location = new Point(label29.Location.X + 100, label29.Location.Y + 80);
                    diaPulsMoved.Location = new Point(diaMovlbl.Location.X + diaMovlbl.Width + 10, diaMovlbl.Location.Y);

                    btnDiaClr.Location = new Point(diaMovlbl.Location.X + 30, diaMovlbl.Location.Y + diaMovlbl.Height + 50);

                    clsDialbl.Location = new Point(btnDiaClr.Location.X + 38, btnDiaClr.Location.Y + 60);
                    closDiaTxtBox.Location = new Point(clsDialbl.Location.X + clsDialbl.Width + 10, clsDialbl.Location.Y);

                    measDialbl.Location = new Point(btnDiaClr.Location.X - 10, clsDialbl.Location.Y + 60);
                    measDiaTxtBox.Location = new Point(closDiaTxtBox.Location.X, measDialbl.Location.Y);

                    label2.Location = new Point(measDialbl.Location.X + 63, measDialbl.Location.Y + 60);
                    diaRatioTxtBox.Location = new Point(measDiaTxtBox.Location.X, label2.Location.Y);

                    calAcceptDiaBtn.Location = new Point(btnDiaClr.Location.X, calAcceptBtn.Location.Y);






                    //this.movpulslbl.Location = new Point(9, (this.label28.Location.Y + 0x2c) + 5);
                    //this.lenMovPulses.Location = new Point(0x88, (this.label28.Location.Y + 0x2c) + 8);
                    //this.plsTargetlbl.Location = new Point(0x16, (this.lenMovPulses.Location.Y + 0x25) + 5);
                    //this.lenPlsTarget.Location = new Point(0x88, (this.lenMovPulses.Location.Y + 0x25) + 5);

                    //this.label27.Location = new Point(6, (this.plsTargetlbl.Location.Y + 0x23) + 5);
                    //this.grpDispLen.Location = new Point(120, this.plsTargetlbl.Location.Y + 0x23);

                    //this.ratiolbl.Location = new Point(20, (this.label27.Location.Y + 0x2f) + 5);
                    //this.lenRatioTxtBox.Location = new Point(120, ((this.label27.Location.Y + 0x2f) + 5) - 3);
                    //this.btnClearPulse.Location = new Point(0xf3, ((this.label27.Location.Y + 0x2f) + 5) - 3);
                    //this.measlbl.Location = new Point(0x10, (this.ratiolbl.Location.Y + 0x27) + 5);
                    //this.txtBoxMl.Location = new Point(120, ((this.ratiolbl.Location.Y + 0x27) + 5) + 15);
                    //this.label18.Location = new Point(0xe8, this.txtBoxMl.Location.Y + 5);
                    //this.lblMetric.Location = new Point(0x10, (this.measlbl.Location.Y + 0x2c) + 5);
                    //this.calAcceptBtn.Location = new Point(0x77, (this.lblMetric.Location.Y + 15) + 5);

                    //this.diaMovlbl.Location = new Point(14, (this.label29.Location.Y + 0x2c) + 5);
                    //this.diaPulsMoved.Location = new Point(0xc2, (this.label29.Location.Y + 0x2c) + 8);
                    //this.btnDiaClr.Location = new Point(0x27, (this.diaMovlbl.Location.Y + 0x25) + 5);
                    //this.clsDialbl.Location = new Point(0x4e, (this.btnDiaClr.Location.Y + 0x2e) + 5);
                    //this.closDiaTxtBox.Location = new Point(0xb2, (this.btnDiaClr.Location.Y + 0x2e) + 5);
                    //this.measDialbl.Location = new Point(0x1d, (this.clsDialbl.Location.Y + 0x27) + 5);
                    //this.measDiaTxtBox.Location = new Point(0xb2, (this.clsDialbl.Location.Y + 0x27) + 5);
                    //this.label2.Location = new Point(0x5c, (this.measDialbl.Location.Y + 0x27) + 5);
                    //this.diaRatioTxtBox.Location = new Point(0xb2, (this.measDialbl.Location.Y + 0x27) + 5);
                    //this.calAcceptDiaBtn.Location = new Point(0x51, this.calAcceptBtn.Location.Y);
                    #endregion

                    #region Movement Settings

                    int x = tabControl1.Width / 2;
                    int y = tabControl1.Height / 2;

                    #region Operation Settings

                    groupBox3.Width = (2 * (int)(x / 3)) - 5;
                    groupBox3.Height = y - 50;

                    semiAutoCB.Location = new Point(label14.Location.X + 20, label14.Location.Y + 60);
                    floatBox.Location = new Point(semiAutoCB.Location.X, semiAutoCB.Location.Y + 50);
                    usLenCB.Location = new Point(floatBox.Location.X, floatBox.Location.Y + 50);
                    cbAudibleInZone.Location = new Point(usLenCB.Location.X, usLenCB.Location.Y + 50);
                    #endregion
                    #region Accept Changes
                    spAcceptBtn.Location = new Point(groupBox3.Location.X + groupBox3.Width + 10, spAcceptBtn.Location.Y);
                    spAcceptBtn.Width = (int)(x / 3);
                    spAcceptBtn.Height = groupBox3.Height - 15;
                    #endregion
                    #region Directional Valve Settings

                    groupBox2.Location = new Point(spAcceptBtn.Location.X + spAcceptBtn.Width + 10, groupBox2.Location.Y);
                    groupBox2.Width = tabControl1.Width - groupBox3.Width - 10 - spAcceptBtn.Width - 10 - 20;
                    groupBox2.Height = groupBox3.Height;

                    speed_lbl.Location = new Point(label33.Location.X + 50, label33.Location.Y + 70);
                    midLbl.Location = new Point(speed_lbl.Location.X, speed_lbl.Location.Y + 100);

                    speed_trkBar.Location = new Point(speed_lbl.Location.X + speed_lbl.Width + 11, speed_lbl.Location.Y);
                    mid_trkBar.Location = new Point(midLbl.Location.X + midLbl.Width, midLbl.Location.Y);

                    speed_trkBar.Width = (int)((groupBox2.Width * 153) / 332);
                    mid_trkBar.Width = speed_trkBar.Width;

                    speedUpDown.Location = new Point(speed_trkBar.Location.X + speed_trkBar.Width + 15, speed_trkBar.Location.Y);
                    midUpDown.Location = new Point(mid_trkBar.Location.X + mid_trkBar.Width + 15, mid_trkBar.Location.Y);

                    label23.Location = new Point(speedUpDown.Location.X + speedUpDown.Width + 10, speedUpDown.Location.Y + 3);
                    #endregion
                    #region Feed ramp
                    grpRamp.Location = new Point(grpRamp.Location.X, groupBox3.Location.Y + groupBox3.Height + 6);
                    grpRamp.Width = groupBox3.Width + 10 + spAcceptBtn.Width;
                    grpRamp.Height = (tabControl1.Height - grpRamp.Location.Y - 45);


                    lblRampSpeed.Location = new Point(label32.Location.X + 50, label32.Location.Y + 70);
                    lblRampUp.Location = new Point(label32.Location.X + 50, lblRampSpeed.Location.Y + 70);
                    lblRampDwn.Location = new Point(label32.Location.X + 50, lblRampUp.Location.Y + 70);

                    tbRampSpeed.Location = new Point(lblRampSpeed.Location.X + lblRampSpeed.Width + 6, lblRampSpeed.Location.Y);
                    tbRampUpLen.Location = new Point(lblRampUp.Location.X + lblRampUp.Width + 2, lblRampUp.Location.Y);
                    tbRampDwnLen.Location = new Point(lblRampDwn.Location.X + lblRampDwn.Width + 6, lblRampDwn.Location.Y);

                    tbRampSpeed.Width = (int)((grpRamp.Width * 177) / 443);
                    tbRampUpLen.Width = tbRampSpeed.Width;
                    tbRampDwnLen.Width = tbRampSpeed.Width;

                    udRampStartSpeed.Location = new Point(tbRampSpeed.Location.X + tbRampSpeed.Width + 15, tbRampSpeed.Location.Y);
                    label22.Location = new Point(udRampStartSpeed.Location.X + udRampStartSpeed.Width + 10, udRampStartSpeed.Location.Y + 3);

                    udRampUp.Location = new Point(tbRampUpLen.Location.X + tbRampUpLen.Width + 15, tbRampUpLen.Location.Y);
                    label20.Location = new Point(udRampUp.Location.X + udRampUp.Width + 10, udRampUp.Location.Y + 3);

                    udRampDwn.Location = new Point(tbRampDwnLen.Location.X + tbRampDwnLen.Width + 15, tbRampDwnLen.Location.Y);
                    label21.Location = new Point(udRampDwn.Location.X + udRampDwn.Width + 10, udRampDwn.Location.Y + 3);
                    #endregion
                    #region Pressure Settings

                    grpPressure.Location = new Point(groupBox2.Location.X, grpRamp.Location.Y);
                    grpPressure.Width = groupBox2.Width;
                    grpPressure.Height = grpRamp.Height;

                    topPres_trkBar.Location = new Point(label19.Location.X + 150, label19.Location.Y + 70);
                    topUpDown1.Location = new Point(topPres_trkBar.Location.X + topPres_trkBar.Width + 10, topPres_trkBar.Location.Y + (int)(topPres_trkBar.Height / 2) - 20);
                    toplbl.Location = new Point(topPres_trkBar.Location.X - 10, topPres_trkBar.Location.Y - 15);

                    wheelPres_trkBar.Location = new Point(topUpDown1.Location.X + topUpDown1.Width + 40, topPres_trkBar.Location.Y);
                    wheelUpDown1.Location = new Point(wheelPres_trkBar.Location.X + wheelPres_trkBar.Width + 10, topUpDown1.Location.Y);
                    wheellbl.Location = new Point(wheelPres_trkBar.Location.X - 22, toplbl.Location.Y);

                    btmPres_trkBar.Location = new Point(wheelUpDown1.Location.X + wheelUpDown1.Width + 40, wheelPres_trkBar.Location.Y);
                    bottomUpDown1.Location = new Point(btmPres_trkBar.Location.X + btmPres_trkBar.Width + 10, wheelUpDown1.Location.Y);
                    bottomlbl.Location = new Point(btmPres_trkBar.Location.X - 18, wheellbl.Location.Y);

                    #endregion

                    #endregion

                    #region Machine info
                    photoCB.Font = new Font(photoCB.Font.Name, 14, photoCB.Font.Style);
                    msEncCB.Font = new Font(msEncCB.Font.Name, 14, msEncCB.Font.Style);
                    oneSawSelectCB.Font = new Font(oneSawSelectCB.Font.Name, 14, oneSawSelectCB.Font.Style);
                    tsawCB.Font = new Font(tsawCB.Font.Name, 14, tsawCB.Font.Style);
                    tsEncCB.Font = new Font(tsEncCB.Font.Name, 14, tsEncCB.Font.Style);

                    label7.Font = new Font(label7.Font.Name, 13, label7.Font.Style);
                    label26.Font = new Font(label26.Font.Name, 13, label26.Font.Style);
                    photoCB.Location = new Point(label34.Location.X + 50, photoCB.Location.Y + 20);
                    msEncCB.Location = new Point(photoCB.Location.X, photoCB.Location.Y + 50);
                    oneSawSelectCB.Location = new Point(msEncCB.Location.X, msEncCB.Location.Y + 50);
                    tsawCB.Location = new Point(oneSawSelectCB.Location.X, oneSawSelectCB.Location.Y + 50);
                    tsEncCB.Location = new Point(tsawCB.Location.X, tsawCB.Location.Y + 50);

                    diaUpDown.Location = new Point(diaUpDown.Location.X + 10, tsEncCB.Location.Y + 100);
                    label7.Location = new Point(diaUpDown.Location.X + diaUpDown.Width + 5, diaUpDown.Location.Y);

                    nudSawToSaw.Location = new Point(diaUpDown.Location.X, diaUpDown.Location.Y + diaUpDown.Height + 30);
                    label25.Location = new Point(nudSawToSaw.Location.X + nudSawToSaw.Width + 5, nudSawToSaw.Location.Y);
                    label26.Location = new Point(label25.Location.X + label25.Width, nudSawToSaw.Location.Y);
                    #endregion

                    #region Com ports
                    grpComPort.Width = (int)(tabControl1.Width / 2);
                    grpComPort.Height = tabControl1.Height - 50;
                    grpTx.Location = new Point(grpComPort.Location.X + grpComPort.Width + 10, grpComPort.Location.Y);
                    grpTx.Width = tabControl1.Width - grpComPort.Location.X - grpComPort.Width - 35;
                    grpTx.Height = (grpComPort.Height / 2) - 10;
                    groupBox1.Location = new Point(grpTx.Location.X, grpTx.Location.Y + grpTx.Height + 5);
                    groupBox1.Width = grpTx.Width;
                    groupBox1.Height = grpComPort.Height - grpTx.Height - 5;
                    #endregion

                    return;
                }


                if (control.Size.Height > defaultSize.Height && !fullscreenState)
                {

                    tabControl1.Refresh();

                    #region Operation tab
                    int ratio_Font_lengthDisplay = tabControl1.Height - grpFeedback.Height;
                    lengthDisplay.Font = new Font(lengthDisplay.Font.Name, (int)((65 * ratio_Font_lengthDisplay) / 250), lengthDisplay.Font.Style);
                    mSawLabel.Location = new Point(mSawTx.Location.X + 3, mSawLabel.Location.Y);
                    tSawLabel.Location = new Point(tSawTx.Location.X + 3, tSawLabel.Location.Y);
                    tiltLabel.Location = new Point(tiltRx.Location.X - 5, tiltLabel.Location.Y);
                    dirLabel.Location = new Point(dirTx.Location.X + 3, dirLabel.Location.Y);
                    revLabel.Location = new Point(topTx.Location.X + 3, revLabel.Location.Y);
                    whLabel.Location = new Point(whTx.Location.X + 3, whLabel.Location.Y);
                    butLabel.Location = new Point(butTx.Location.X + 3, butLabel.Location.Y);
                    lblP1.Location = new Point(fbPump1.Location.X + 3, lblP1.Location.Y);
                    lblP2.Location = new Point(fbPump2.Location.X + 3, lblP2.Location.Y);
                    #endregion

                    #region calibratoin

                    //movpulslbl.Location = new Point(10, 58);
                    //lenMovPulses.Location = new Point(132, 58);
                    //plsTargetlbl.Location = new Point(10, 95);
                    //lenPlsTarget.Location = new Point(132, 95);
                    //label27.Location = new Point(6, 130);
                    //grpDispLen.Location = new Point(120, 121);
                    //ratiolbl.Location = new Point(10, 179);
                    //lenRatioTxtBox.Location = new Point(120, 180);
                    //btnClearPulse.Location = new Point(243, 179);
                    //measlbl.Location = new Point(16, 216);
                    //txtBoxMl.Location = new Point(232, 240);
                    //label18.Location = new Point(232, 240);
                    //lblMetric.Location = new Point(19, 260);
                    //calAcceptBtn.Location = new Point(119, 291);

                    //diaMovlbl.Location = new Point(14, 58);
                    //diaPulsMoved.Location = new Point(194, 59);
                    //btnDiaClr.Location = new Point(39, 95);
                    //clsDialbl.Location = new Point(78, 141);
                    //closDiaTxtBox.Location = new Point(178, 141);
                    //measDialbl.Location = new Point(29, 177);
                    //measDiaTxtBox.Location = new Point(178, 180);
                    //label2.Location = new Point(92, 219);
                    //diaRatioTxtBox.Location = new Point(178, 216);
                    //calAcceptDiaBtn.Location = new Point(81, 281);



                    this.movpulslbl.Location = new Point(9, (this.label28.Location.Y + 0x2c) + 5);
                    this.lenMovPulses.Location = new Point(0x88, (this.label28.Location.Y + 0x2c) + 8);
                    this.plsTargetlbl.Location = new Point(0x16, (this.lenMovPulses.Location.Y + 0x25) + 5);
                    this.lenPlsTarget.Location = new Point(0x88, (this.lenMovPulses.Location.Y + 0x25) + 5);
                    this.label27.Location = new Point(6, (this.plsTargetlbl.Location.Y + 0x23) + 5);
                    this.grpDispLen.Location = new Point(120, this.plsTargetlbl.Location.Y + 0x23);
                    this.ratiolbl.Location = new Point(20, (this.label27.Location.Y + 0x2f) + 5);
                    this.lenRatioTxtBox.Location = new Point(120, ((this.label27.Location.Y + 0x2f) + 5) - 3);
                    this.btnClearPulse.Location = new Point(0xf3, ((this.label27.Location.Y + 0x2f) + 5) - 3);
                    this.measlbl.Location = new Point(0x10, (this.ratiolbl.Location.Y + 0x27) + 5);
                    this.txtBoxMl.Location = new Point(120, ((this.ratiolbl.Location.Y + 0x27) + 5) + 15);
                    this.label18.Location = new Point(0xe8, this.txtBoxMl.Location.Y + 5);
                    this.lblMetric.Location = new Point(0x10, (this.measlbl.Location.Y + 0x2c) + 5);
                    this.calAcceptBtn.Location = new Point(0x77, (this.lblMetric.Location.Y + 15) + 5);

                    this.diaMovlbl.Location = new Point(14, (this.label29.Location.Y + 0x2c) + 5);
                    this.diaPulsMoved.Location = new Point(0xc2, (this.label29.Location.Y + 0x2c) + 8);
                    this.btnDiaClr.Location = new Point(0x27, (this.diaMovlbl.Location.Y + 0x25) + 5);
                    this.clsDialbl.Location = new Point(0x4e, (this.btnDiaClr.Location.Y + 0x2e) + 5);
                    this.closDiaTxtBox.Location = new Point(0xb2, (this.btnDiaClr.Location.Y + 0x2e) + 5);
                    this.measDialbl.Location = new Point(0x1d, (this.clsDialbl.Location.Y + 0x27) + 5);
                    this.measDiaTxtBox.Location = new Point(0xb2, (this.clsDialbl.Location.Y + 0x27) + 5);
                    this.label2.Location = new Point(0x5c, (this.measDialbl.Location.Y + 0x27) + 5);
                    this.diaRatioTxtBox.Location = new Point(0xb2, (this.measDialbl.Location.Y + 0x27) + 5);
                    this.calAcceptDiaBtn.Location = new Point(0x51, this.calAcceptBtn.Location.Y);
                    #endregion

                    #region Movement Settings

                    int x = tabControl1.Width / 2;
                    int y = tabControl1.Height / 2;


                    groupBox3.Width = (2 * (int)(x / 3)) - 5;
                    groupBox3.Height = y - 50;

                    semiAutoCB.Location = new Point(label14.Location.X + 10, label14.Location.Y + 30);
                    floatBox.Location = new Point(semiAutoCB.Location.X, semiAutoCB.Location.Y + 30);
                    usLenCB.Location = new Point(floatBox.Location.X, floatBox.Location.Y + 30);
                    cbAudibleInZone.Location = new Point(usLenCB.Location.X, usLenCB.Location.Y + 30);

                    spAcceptBtn.Location = new Point(groupBox3.Location.X + groupBox3.Width + 10, spAcceptBtn.Location.Y);
                    spAcceptBtn.Width = (int)(x / 3);
                    spAcceptBtn.Height = groupBox3.Height - 15;

                    #region groupbox2
                    groupBox2.Location = new Point(spAcceptBtn.Location.X + spAcceptBtn.Width + 10, groupBox2.Location.Y);
                    groupBox2.Width = tabControl1.Width - groupBox3.Width - 10 - spAcceptBtn.Width - 10 - 20;
                    groupBox2.Height = groupBox3.Height;

                    speed_lbl.Location = new Point(9, 65);
                    midLbl.Location = new Point(5, 130);

                    speed_trkBar.Location = new Point(speed_lbl.Location.X + speed_lbl.Width + 11, speed_lbl.Location.Y);
                    mid_trkBar.Location = new Point(midLbl.Location.X + midLbl.Width + 6, midLbl.Location.Y - 12);

                    speed_trkBar.Width = (int)((groupBox2.Width * 153) / 332);
                    mid_trkBar.Width = speed_trkBar.Width;

                    speedUpDown.Location = new Point(speed_trkBar.Location.X + speed_trkBar.Width + 5, speed_trkBar.Location.Y);
                    midUpDown.Location = new Point(mid_trkBar.Location.X + mid_trkBar.Width + 5, mid_trkBar.Location.Y);

                    label23.Location = new Point(speedUpDown.Location.X + speedUpDown.Width + 5, speedUpDown.Location.Y + 3);
                    #endregion

                    #region Feed Ramp
                    grpRamp.Location = new Point(grpRamp.Location.X, groupBox3.Location.Y + groupBox3.Height + 6);
                    grpRamp.Width = groupBox3.Width + 10 + spAcceptBtn.Width;
                    grpRamp.Height = (tabControl1.Height - grpRamp.Location.Y - 45);

                    lblRampSpeed.Location = new Point(8, 70);
                    lblRampUp.Location = new Point(8, 118);
                    lblRampDwn.Location = new Point(8, 160);

                    tbRampSpeed.Location = new Point(148, 65);
                    tbRampUpLen.Location = new Point(148, 113);
                    tbRampDwnLen.Location = new Point(148, 157);

                    tbRampSpeed.Width = (int)((grpRamp.Width * 177) / 443);
                    tbRampUpLen.Width = tbRampSpeed.Width;
                    tbRampDwnLen.Width = tbRampSpeed.Width;

                    udRampStartSpeed.Location = new Point(tbRampSpeed.Location.X + tbRampSpeed.Width + 5, tbRampSpeed.Location.Y);
                    label22.Location = new Point(udRampStartSpeed.Location.X + udRampStartSpeed.Width, udRampStartSpeed.Location.Y + 3);

                    udRampUp.Location = new Point(tbRampUpLen.Location.X + tbRampUpLen.Width + 5, tbRampUpLen.Location.Y);
                    label20.Location = new Point(udRampUp.Location.X + udRampUp.Width, udRampUp.Location.Y + 3);

                    udRampDwn.Location = new Point(tbRampDwnLen.Location.X + tbRampDwnLen.Width + 5, tbRampDwnLen.Location.Y);
                    label21.Location = new Point(udRampDwn.Location.X + udRampDwn.Width, udRampDwn.Location.Y + 3);
                    #endregion

                    #region pressure settings
                    grpPressure.Location = new Point(groupBox2.Location.X, grpRamp.Location.Y);
                    grpPressure.Width = groupBox2.Width;
                    grpPressure.Height = grpRamp.Height;

                    topPres_trkBar.Location = new Point(11, 86);
                    topUpDown1.Location = new Point(62, 125);
                    toplbl.Location = new Point(4, 65);

                    wheelPres_trkBar.Location = new Point(125, 86);
                    wheelUpDown1.Location = new Point(176, 125);
                    wheellbl.Location = new Point(103, 65);

                    btmPres_trkBar.Location = new Point(233, 86);
                    bottomUpDown1.Location = new Point(278, 125);
                    bottomlbl.Location = new Point(215, 65);
                    #endregion

                    #endregion

                    #region Machine info
                    photoCB.Font = new Font(photoCB.Font.Name, 12, photoCB.Font.Style);
                    msEncCB.Font = new Font(msEncCB.Font.Name, 12, msEncCB.Font.Style);
                    oneSawSelectCB.Font = new Font(oneSawSelectCB.Font.Name, 12, oneSawSelectCB.Font.Style);
                    tsawCB.Font = new Font(tsawCB.Font.Name, 12, tsawCB.Font.Style);
                    tsEncCB.Font = new Font(tsEncCB.Font.Name, 12, tsEncCB.Font.Style);

                    label7.Font = new Font(label7.Font.Name, 12, label7.Font.Style);
                    label26.Font = new Font(label26.Font.Name, 12, label26.Font.Style);

                    photoCB.Location = new Point(6, 66);
                    msEncCB.Location = new Point(6, 105);
                    oneSawSelectCB.Location = new Point(6, 144);
                    tsawCB.Location = new Point(6, 184);
                    tsEncCB.Location = new Point(6, 220);

                    diaUpDown.Location = new Point(6, 265);
                    label7.Location = new Point(71, 265);

                    nudSawToSaw.Location = new Point(6, 313);
                    label25.Location = new Point(71, 313);
                    label26.Location = new Point(96, 313);
                    #endregion

                    #region Com ports
                    grpComPort.Width = 525;
                    grpComPort.Height = 355;
                    grpTx.Location = new Point(539, 3);
                    grpTx.Width = 250;
                    grpTx.Height = 166;
                    groupBox1.Location = new Point(539, 178);
                    groupBox1.Width = 245;
                    groupBox1.Height = 180;
                    #endregion

                    return;
                }

                #region This part work is only for Device

                #region Machine Info Tab
                photoCB.Font = new Font(photoCB.Font.Name, 12, photoCB.Font.Style);
                msEncCB.Font = new Font(msEncCB.Font.Name, 12, msEncCB.Font.Style);
                oneSawSelectCB.Font = new Font(oneSawSelectCB.Font.Name, 12, oneSawSelectCB.Font.Style);
                tsawCB.Font = new Font(tsawCB.Font.Name, 12, tsawCB.Font.Style);
                tsEncCB.Font = new Font(tsEncCB.Font.Name, 12, tsEncCB.Font.Style);
                #endregion

                #region Calibrate
                this.movpulslbl.Location = new Point(9, 0x3a);
                this.lenMovPulses.Location = new Point(0x88, 0x3d);
                this.plsTargetlbl.Location = new Point(0x16, 0x5f);
                this.lenPlsTarget.Location = new Point(0x84, 0x5f);
                this.label27.Location = new Point(6, 130);
                this.grpDispLen.Location = new Point(120, 0x79);
                this.ratiolbl.Location = new Point(20, 0xb1);
                this.lenRatioTxtBox.Location = new Point(120, 180);
                this.btnClearPulse.Location = new Point(0xf3, 0xb3);
                this.measlbl.Location = new Point(0x10, 0xd8);
                this.txtBoxMl.Location = new Point(120, 0xe7);
                this.label18.Location = new Point(0xe8, 0xf2);
                this.lblMetric.Location = new Point(0x10, 260);
                this.calAcceptBtn.Location = new Point(0x77, this.lblMetric.Location.Y + 10);

                this.diaMovlbl.Location = new Point(14, 0x3a);
                this.diaPulsMoved.Location = new Point(0xc2, 0x3d);
                this.btnDiaClr.Location = new Point(0x27, 0x5f);
                this.clsDialbl.Location = new Point(0x4e, 0x8d);
                this.closDiaTxtBox.Location = new Point(0xb2, 0x8d);
                this.measDialbl.Location = new Point(0x1d, 180);
                this.measDiaTxtBox.Location = new Point(0xb2, 180);
                this.label2.Location = new Point(0x5c, 0xdb);
                this.diaRatioTxtBox.Location = new Point(0xb2, 0xdb);
                this.calAcceptDiaBtn.Location = new Point(0x51, calAcceptBtn.Location.Y);
                #endregion

                #region Operation tab
                lengthDisplay.Font = new Font(lengthDisplay.Font.Name, (int)((65 * grpLength.Size.Height) / 250), lengthDisplay.Font.Style);
                mSawLabel.Location = new Point(mSawTx.Location.X + 3, mSawLabel.Location.Y);
                tSawLabel.Location = new Point(tSawTx.Location.X + 3, tSawLabel.Location.Y);
                tiltLabel.Location = new Point(tiltRx.Location.X - 5, tiltLabel.Location.Y);
                dirLabel.Location = new Point(dirTx.Location.X + 3, dirLabel.Location.Y);
                revLabel.Location = new Point(topTx.Location.X + 3, revLabel.Location.Y);
                whLabel.Location = new Point(whTx.Location.X + 3, whLabel.Location.Y);
                butLabel.Location = new Point(butTx.Location.X + 3, butLabel.Location.Y);
                lblP1.Location = new Point(fbPump1.Location.X + 3, lblP1.Location.Y);
                lblP2.Location = new Point(fbPump2.Location.X + 3, lblP2.Location.Y);
                #endregion

                #region Movement Settings


                #endregion

                #endregion


            }
            catch (Exception e_SilverbackXP_Resize)
            {
                MessageBox.Show(e_SilverbackXP_Resize.Message, "Error in SilverbackXP Resize");
            }

        }
        #endregion

        #region Manual Resize

        private void panel1_resize_manual()
        {

            //panel1.Width = this.Width - 5;

        }


        #endregion

        /// <summary>
        /// Creates or adds to an error text file.
        /// </summary>
        /// <param name="error">String to add into the file</param>
        private static void Error_Stamp(String error)
        {
            try
            {
                //=============================================
                string errorFile = @"DataLog\\start_end_log.txt";
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
            catch (Exception)
            {
                MessageBox.Show("Error_Stamp: " + error, "SilverbackXP.Error_Stamp");
            }
        }
        /// <summary>
        /// Set the pressure of a given arm or set a speed value.
        /// </summary>
        /// <param name="s"> select speed or arm to set,
        /// top = top knife
        /// wheels = wheel arm pressure
        /// bottom = bottom knife
        /// speed = speed 
        /// middle = neutral pressure
        /// </param>
        /// <param name="pressure">set the pressure from 0 to 100 percent as an integer</param>
        private void setStaticValue(string s, int pressure)
        {
            double dbl = 0;
            int temp = 0;
            // Do nothing if pressure is out of range.pressure >= 0 && 
            if (pressure <= 100)
            {// a 0 temp value corresponds to 100% pressue or 0 relief
                //dbl = (255.0 - 255.0 * ((double)pressure / 100));//change from 127.0; mar.30.2009
                //temp = (int)Math.Round(dbl);
                // Only send a command if a valid 's' is given
                string sendCommand = "$nop,\r\n";
                Utility.CommandPrep t2cmnd = new CommandPrep();
                switch (s)
                {
                    case "top":// Set top knife pressure
                        sendCommand = t2cmnd.setCommand("topk", pressure);
                        break;
                    case "wheels":// Set Wheel arm pressure
                        sendCommand = t2cmnd.setCommand("whar", pressure);
                        break;
                    case "bottom":// Set bottom knife pressure
                        sendCommand = t2cmnd.setCommand("botk", pressure);
                        break;
                    case "speed":// Set speed
                        dbl = (64.0 * ((double)pressure / 100));
                        temp = (int)Math.Round(dbl);
                        sendCommand = t2cmnd.setCommand("whsp", temp);
                        break;
                    case "middle":// Set middle pressure for wheels
                        //dbl = 2.55 * (double)pressure;//2.55 is 1% or 255, this will let middle increment by 1% per unit.
                        //temp = (int)Math.Round(dbl);
                        sendCommand = t2cmnd.setCommand("neut", pressure);
                        break;
                    case "rampstart":// Set the ramp speed start position
                        sendCommand = t2cmnd.setCommand("rmst", pressure);
                        break;
                    case "rampTo":
                        temp = (int)Math.Round(udRampUp.Value / lengthScaler);
                        sendCommand = t2cmnd.setCommand("rmto", temp);
                        break;
                    case "rampDown":
                        temp = (int)Math.Round(udRampDwn.Value / lengthScaler);
                        sendCommand = t2cmnd.setCommand("rmdo", temp);
                        break;
                    case "findButt":
                        sendCommand = t2cmnd.setCommand("finb", pressure);
                        break;
                    case "useRamp":
                        sendCommand = t2cmnd.setCommand("rmus", pressure);
                        break;
                }
                WriteLine_CM(sendCommand);
                for (int i = 0; i < 50000000; i++)
                { ; }
            }
        }
        /// <summary>
        /// Sets the most reasonable point to begin ramping down
        /// </summary>
        private void SetRampDwnPt()
        {
            if (dirTx.State == BushApe.TriVal.triHI)
            {
                // Set the point to start ramping from [ udRampDwn is a centimeter value ]
                RampDwnPoint = (decimal)dataBase.SelectedLength - (udRampDwn.Value * 0.01M);
                // If length is greater than the ramp down point stop ramping up, 
                // split the difference between the actual target and the current postition plus the current position;
                // the ramp to point and the ramp down point are then the same.
                if (len > RampDwnPoint)
                {
                    RampDwnPoint = RampToPoint = len + (((decimal)dataBase.SelectedLength - len) / 2);

                }
                return;/*Do not continue;*/
            }
            if (dirTx.State == BushApe.TriVal.triLO)
            {
                // Set the point to start ramping from [ udRampDwn is a centimeter value ]
                RampDwnPoint = (decimal)dataBase.SelectedLength + (udRampDwn.Value * 0.01M);
                // If the length is less than the ramp down point stop ramping up, 
                // split the difference between the actual target and the current postition minus the current position;
                // the ramp to point and the ramp down point are then the same.
                if (len < RampDwnPoint)
                {
                    RampDwnPoint = RampToPoint = (len - (decimal)dataBase.SelectedLength) / 2;

                }
            }
        }
        /// <summary>
        /// Sends the command with the number of pulses
        /// calculated from the current selected length, upper and lower values.
        /// </summary>
        private void SendStopAt()
        {
            try
            {
                Utility.CommandPrep t2cmnd = new CommandPrep();
                int pulses = 0;// used for data base queries in full version
                string sendCommand = "";
                int index = 0;
                //
                // send stop at length to head using the new current, selected length window
                //
                if (CMserialPort1.IsOpen)
                {
                    // send stop at length command

                    /*command also turns on the autofeed flag for the head module*/
                    /***************************************
                     * Set the stop in and protection windows
                     * *************************************/
                    //*********************************************
                    // Find the current preset index
                    //

                    //***********************************************
                    // Find the Minimum pulses
                    //
                    pulses = (int)Math.Round((dataBase.SelectedLength - (0.1m * dataBase.SelectedUnderLengthWindow)) / lengthScaler);/*UnderLengthWindow is in cm*/
                    //
                    // Get the set minimum target pulses command
                    //
                    sendCommand = t2cmnd.setCommand("minT", pulses);
                    //
                    // Send the Minimuum pulses: used for target window and log protection
                    //
                    WriteLine_CM(sendCommand);
                    //
                    // Find the Maximum pulses
                    //
                    pulses = (int)Math.Round((dataBase.SelectedLength + (0.1m * dataBase.SelectedOverLengthWindow)) / lengthScaler);/*OverLengthWindow is in cm*/
                    //
                    // Get the set maximum target pulses command
                    //
                    sendCommand = t2cmnd.setCommand("maxT", pulses);
                    //
                    // Send the Maximum pulses:
                    //
                    WriteLine_CM(sendCommand);
                    //
                    // Show target length
                    //
                    RefreshTarget();
                }/*end if serial open*/
                //
                // if calibrating display the pulse target
                //
                if (tabControl1.SelectedIndex == 2)
                {
                    //
                    // Get exact target pulses
                    //
                    //pulses = (int)Math.Round(dataBase.SelectedLength / lengthScaler);
                    //
                    // Show the targetted pulses
                    //
                    lenPlsTarget.Text = LogPresets[index].MinPulses;// pulses.ToString();
                }
            }
            catch (ThreadAbortException te)
            {
                Error_Stamp("thread abort, " + te.Message);
            }
            catch (Exception i3err)
            {
                //MessageBox.Show("i3err: " + i3err.Message);
                Error_Stamp("i3err: " + i3err.Message);
            }

        }
        /// <summary>
        /// Indicate on the display what the target is.
        /// </summary>
        private void RefreshTarget()
        {
            try
            {
                if (semiAutoCB.Checked)
                {
                    //*************************************
                    // Get the selected length index
                    //
                    //int index = 0;
                    //for (index = 0; index < 31; index++)
                    //{
                    //    if (LogPresets[index].Current)
                    //        break;
                    //}
                    //************************************************
                    // Show target length
                    //
                    //if (index >= 31)
                    //    logLenlbl.Text = "Target: N/A";
                    //else
                    logLenlbl.Text = "Target: " + dataBase.SelectedLength.ToString() + "m, "
                        + dataBase.SelectedSpeciesName;
                }
                else
                    logLenlbl.Text = "Manual Feed";
            }
            catch (Exception e)
            {
                MessageBox.Show("Error at Target Refresh " + e.Message);
            }
        }
        /// <summary>
        /// Interprets saw sensor data as hi or low ( on/off )
        /// </summary>
        /// <param name="tempData">either sath or satl; all else ignored</param>
        private void TopSawSensor(string tempData)
        {
            switch (tempData)
            {// Top Saw
                case "sath":
                    TopSawSensor("hi", false);
                    break;
                case "satl":
                    TopSawSensor("low", false);
                    break;
                default:
                    break;
            }//end top saw switch/NOP
        }
        /// <summary>
        /// Logic to handle Top saw sensor activity, Does not clear length.
        /// </summary>
        /// <param name="tempData">'hi' or 'low' indicates state of top saw sensor</param>
        /// <param name="pumpcall">true will cas the appropriate call to the pumps</param>
        private void TopSawSensor(string tempData, bool pumpcall)
        {
            // head stops movement and closes 
            if (tempData.StartsWith("low"))
            {// 'low' indicates the proximity sensor is non-active, saw is out, 
                // so head stops movement and closes 										
                this.fbRx[(int)MachineOps.TopSaw].State
                    = BushApe.TriVal.triLO;
                this.FeedbackDisplayRX("clos");

            }
            else if (tempData.StartsWith("hi"))
            {// Turn pump off
                if (pumpcall && mSawRx.State == BushApe.TriVal.triHI)// pump related call and main saw is up
                {
                    if (!WriteLine_CM(GPIO.GPIOHDR + GPIO.PALL_OFF.ToString()))
                    {
                        MessageBox.Show("Pump Port Failure!!!", "Top Saw Sensor");
                    }
                    fbPump1.State = BushApe.TriVal.triHI;
                    fbPump2.State = BushApe.TriVal.triHI;
                }
                fbRx[(int)MachineOps.TopSaw].State = BushApe.TriVal.triHI;

                //*************************************************
                // * Store volume when saw return,. if the piece is longer than 10cm and store volume is selected
                //***********************************************/
                Store_Volume();
                //if (len > 0.01m)
                //{// This causes an unacceptable delay during saw return !?System.Convert.ToInt32(treeCntlbl.Text),
                //    //        dataBase.SelectedBuckingSpec, 
                //    pieceCntlbl.Text = dataBase.AddPiece(len, vol).ToString();
                //}
            }
        }
        /// <summary>
        /// Interprets Main saw sensor as hi or low ( on/off )
        /// </summary>
        /// <param name="tempData">either samh or saml; all others ignored</param>
        private void MainSawSensor(string tempData)
        {
            switch (tempData)
            {// Main Saw
                case "samh":
                    MainSawSensor("hi", false);
                    break;
                case "saml":
                    MainSawSensor("low", false);
                    break;
                default:
                    break;
            }//end main saw swith/NOP
        }
        /// <summary>
        /// Logic to handle Main saw sensor 
        /// </summary>
        /// <param name="tempData">'hi' or 'low' indicates the state of Main saw sensor</param>
        /// <param name="pumpcall">true will cause pumps to de-activate, if top saw is up</param>
        private void MainSawSensor(string tempData, bool pumpcall)
        {
            if (tempData.StartsWith("low"))
            {// 'low' indicates the proximity sensor is non-active, saw is out, 
                // so head stops movement and closes 										
                this.fbRx[(int)MachineOps.MainSaw].State
                    = BushApe.TriVal.triLO;
                this.FeedbackDisplayRX("clos");
                // Ensure tilt float
                WriteLine_CM("wt41 $cmd tifl");
                // If the Display is  on the Operational page, Clear encoders
                if (tabControl1.SelectedIndex == 0)
                {
                    //***************************************
                    // Send stop to cause update of length/dia data
                    //
                    WriteLine_CM("wt41 $stp");
                    // Clear Length (only) with clrl, only if on the operational page currentCommand = ;currentCommand	                                                    
                    WriteLine_CM("wt41 $cmd clrl");
                    // Send the end auto command
                    WriteLine_CM("wt41 $cmd eaut");
                    // If there is no photo eye and in auto or semi auto, send the stop at length
                    if (semiAutoCB.Checked && !photoCB.Checked)
                        SendStopAt();

                }
            }
            else if (tempData.StartsWith("hi"))
            {// 'hi' indicates the proximity sensor is active,
                // so the saw is up
                // Turn pump off 0x04
                if (pumpcall && tSawRx.State == BushApe.TriVal.triHI)
                {
                    WriteLine_CM(GPIO.GPIOHDR + GPIO.PALL_OFF);
                    fbPump1.State = BushApe.TriVal.triHI;
                    fbPump2.State = BushApe.TriVal.triHI;
                    this.FeedbackDisplayRX("neut");

                    errorLbl.Text = "Pump off, Main saw up";
                }
                this.fbRx[(int)MachineOps.MainSaw].State
                    = BushApe.TriVal.triHI;

                Store_Volume();
                //
                // Clear Add Log flag
                //
                addLog = false;

                /*************************************************
                 * Store volume when saw return,. if the piece is longer than 10cm and store volume is selected
                 * **********************************************/
                //if (len > 0.1m)
                //{// This causes an unacceptable delay during saw return !?
                //    pieceCntlbl.Text = dataBase.AddPiece(System.Convert.ToInt32(treeCntlbl.Text),
                //        dataBase.SelectedBuckingSpec, len, vol).ToString();
                //}
            }
            else if (tempData.StartsWith("?"))
            {
                //errorLbl.Text = "Proximity sensor error";
            }
        }
        /// <summary>
        /// Store final calculated volume of piece.
        /// </summary>
        private void Store_Volume()
        {
            //******************************************************
            // Final Update for volume
            //
            //**************************
            // Calculate the final current slice length
            //
            current_slice_len = len - len_from_slice;
            Update_Piece_Volume();
            //******************************************************
            // Increment piece count, Store in database and display piece count.
            //  if the piece is longer than 10cm and store volume is selected.
            //
            if (len > 0.1m)
            {
                int temp_count = dataBase.AddPiece(dataBase.CurrentStemID, dataBase.SelectedBuckingSpec, len, vol);
                pieceCntlbl.Text = temp_count.ToString();
            }
            //******************************************************
            // Clear slice variables
            //
            current_slice_len = len_from_slice = 0;
        }
        /// <summary>
        /// Logic to handle Photoe Eye sensor
        /// </summary>
        /// <param name="tempData">'phoh' or 'phol' indicates 'hi' or 'low' on the sensor</param>
        private void PhotoEyeSensor(string tempData)
        {
            switch (tempData)
            {// Photo eye
                case "phoh":
                    //errorLbl.Text = "Photo-Eye is High";

                    if (grpLength.BackColor == phol_back_color)
                    {
                        grpLength.BackColor = phoh_back_color;// Color.White;
                        if (buttSawRB.Checked)
                        {
                            lengthDisplay.ForeColor = buttSaw_fore_color;
                        }
                        else if (topSawRB.Checked)
                        {
                            lengthDisplay.ForeColor = topSaw_fore_color;
                        }
                        logLenlbl.ForeColor = normal_forecolor;
                        lengthDisplay.BackColor = normal_backcolor;
                        // IF semi or full auto;  
                        //  send selected length
                        if (semiAutoCB.Checked)
                            SendStopAt();

                    }
                    break;
                case "phol":
                    //errorLbl.Text = "Photo-Eye is Low";

                    if (grpLength.BackColor == phoh_back_color)
                    {
                        grpLength.BackColor = phol_back_color;
                        lengthDisplay.BackColor = phol_back_color;
                        lengthDisplay.ForeColor = normal_reverse_forecolor;
                        logLenlbl.ForeColor = normal_reverse_forecolor;
                        //IF full-auto 
                        //  send a cler command
                        //if (semiAutoCB.Checked || autoCB.Checked)
                        //    comOut.Send("$cmd,eaut,\r\n");
                        // Stop speed timer
                        //speedTmr.Change(-1, -1);// phol, NOT IMPLEMENTED!!!
                    }
                    break;
                case "":
                    break;
                default:
                    // An unanticipated communication has occurred
                    //errorLbl.ForeColor = System.Drawing.Color.Black;
                    //errorLbl.BackColor = System.Drawing.Color.Beige;
                    break;
            }//end photo eye switch/NOP
        }
        /// <summary>
        /// Interpret data recieved from The head module
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void InterpretHMData(string hmString)
        {
            Utility.Info iType;
            //IEnumerator queueEnum = qPassData.GetEnumerator();
            try
            {
                //errorLbl.ForeColor = System.Drawing.Color.Black;
                //errorLbl.BackColor = System.Drawing.Color.Wheat;
                // update the last communication string
                lastComm.rx_tx = hmString;
                // Split data into its individual parts
                string tempCom = currentCommand;/*removed 05.06.2010.TrimEnd();*/
                string[] tempData = hmString.ToString().Split(new char[] { ' ' });// changed from ',' on 19.1.2012
                switch (tempData[0])
                {
                    case "$err":// Error Response
                        try
                        {
                            switch (tempData[1])
                            {
                                case "prx2":// There is proximity sensor activity,
                                    // Proximity 2 is assumed to be Top saw, by default the 
                                    TopSawSensor(tempData[2], true);
                                    break;
                                case "prx1":// Proximity 1 is assumed to be the Main saw, by default the 
                                    MainSawSensor(tempData[2], true);
                                    break;
                                case "phoh":// Photo eye
                                    PhotoEyeSensor(tempData[1]);
                                    break;
                                case "phol":
                                    PhotoEyeSensor(tempData[1]);
                                    break;
                                case "sync":
                                    WriteLine_CM("wt41 $rdy");
                                    break;
                                case "niw":/*Attempted to saw outside protection Zone*/
                                    SystemSounds.Exclamation.Play();
                                    break;
                                default:// Unusual error
                                    //errorLbl.Text = "Undefined Head Error";
                                    WriteLine_CM("wt41 $rdy");
                                    break;
                            }

                        }
                        catch (Exception ee)
                        {
                            // This hides a Format Exception and should be corrected 7.27.2006
                            if (!ee.Message.StartsWith("FormatException"))
                            {
                                //MessageBox.Show("Exception at RxData (err): " + ee.Message);
                                Error_Stamp("Exception at RxData (err): " + ee.Message);
                            }
                        }
                        break;
                    case "$stp":
                        try
                        {
                            // Pumps are stopped when command is sent, unless the 
                            // previous command is a saw function, then see error 'if' above
                            if (this.mSawRx.State == BushApe.TriVal.triHI &&
                                this.tSawRx.State == BushApe.TriVal.triHI)
                            {
                                // Turn pump off
                                //if (serialPort2.IsOpen)
                                //    serialPort2.WriteLine(GPIO.GPIOHDR + GPIO.PALL_OFF.ToString());
                                //else
                                //    MessageBox.Show("Pump Port Failure!!!", "Stop");
                                fbPump1.State = BushApe.TriVal.triHI;
                                fbPump2.State = BushApe.TriVal.triHI;
                                // Indicate action
                                errorLbl.Text = "Pump(s)OFF, sent";

                            }
                            else
                                errorLbl.Text = "Wait for Saw Return";
                            FeedbackDisplayRX("stp");
                            //
                            // if this is the current command it means the sent  
                            // command is confirmed
                            //
                            if (tempCom.Equals(hmString))
                            {
                                /*Stop Timer*/
                                replyTimer.Stop(); ;
                                /*-----------*/
                                timeOutCnt = 0;
                            }
                            //*************************************************************************
                            // 
                            // If Head module has stopped automatically at a length
                            // it will send an 'eaut' message && autoCB.Checked || semiAutoCB.Checked
                            //
                            if (tempData.Length > 1)
                            {
                                if (tempData[1] == "eaut" && semiAutoCB.Checked)
                                {
                                    //MessageBox.Show("auto stop");
                                    // start timer to compare the length with the target
                                    //autoTmr.Change(0, autoCntDwn); //commented out on 09.11.2009
                                    /*************************************************************
                                     * The following is copied from the autoTimer function
                                     * **********************************************************/
                                    // if this has not been called more than 5 times!!!!!!!
                                    // Make sure the target Length exists
                                    if (targetLength != null)
                                    {
                                        decimal tmpTarget = Convert.ToDecimal((float)targetLength.GetValue(currentLenIndex));
                                        decimal tmpDif = Math.Abs(len - tmpTarget);
                                        int tmp = tmpDif.CompareTo(0.10M);//returns 0 if == value
                                        // if length > or < target by more than 2cm
                                        if (tmp > 0)
                                        {
                                            if (semiAutoCB.Checked)
                                                SendStopAt();
                                            // change current command to 'cmd,forw'
                                            currentCommand = "wt41 $cmd,forw, ";
                                            //// send movement command and return
                                            WriteLine_CM(currentCommand);

                                        }
                                        //
                                        // Change color of length text box to indicate cutting window.
                                        //
                                        lengthDisplay.ForeColor = in_cutting_window_color;
                                    }

                                }
                            }

                            WriteLine_CM("wt41 $rdy");
                        }/*End of 'stp' case*/
                        catch (Exception es)
                        {
                            string message = "Exception at RxData (stp): " + es.Message;
                            // This hides a Format Exception and should be corrected 7.27.2006
                            if (!es.Message.StartsWith("FormatException"))
                            {
                                MessageBox.Show(message);
                                Error_Stamp(message);

                            }
                        }
                        break;
                    case "$set":// A value has been set, call the handling function
                        errorLbl.BackColor = System.Drawing.Color.Purple;
                        errorLbl.ForeColor = System.Drawing.Color.White;
                        /*************************************
                         * Temporary to nail down this issue, set is returned 
                         * with no appearant call
                         * ***********************************/
                        //MessageBox.Show("From set: " + tempData[1] + "," + tempData[2]);

                        break;
                    case "$dat":/*Contains length and diameter info, process the data in the Rx information.*/
                        try
                        {
                            int diaPulse = 0;
                            int Length = 0;
                            if (tempData[1] != null)
                                Length = System.Convert.ToInt32(tempData[1]);
                            if (tempData[3] != null)
                                diaPulse = System.Convert.ToInt32(tempData[2]);
                            //******************************************************************
                            // Calculate length from pulses
                            //
                            len = Math.Round(Length * lengthScaler, 3) + SAWoffset;
                            //*******************************************************************
                            // Calculate diameter from pulses
                            //
                            dia = (decimal)Math.Round(((decimal)diaScaler * (
                                Convert.ToDecimal(closDiaTxtBox.Text) + (decimal)diaPulse)), 1);
                            //******************************************************
                            // Do operations on data depending on tab page selected
                            //
                            if (tabControl1.SelectedIndex == 0)
                            {/*Operational page*/
                                //
                                // If in the middle of ramping activity compare length with ramp distance
                                //
                                if (RampingUp)/*if ramping up...&& (len >= RampToPoint)*/
                                    RampUpOps();
                                else if (RampDwn)/*if ramping down*/
                                    RampDwnOps();

                                // call speed sense with new and current length; 
                                // lengthDisplay has not changed yet.
                                //SpeedSense(len, Convert.ToDecimal(lengthDisplay.Text));

                                //*******************************************************************
                                // The diameter is the total pulses from each diameter encoder
                                // added together.  650 is the maximum centimetres.  900 is maximum
                                // pulses of 2 encoders.	"n",nfi	 "N", nfi  "N", nfi								
                                //

                                //**********************************
                                // Update diameter
                                //
                                diaDisplay.Text = dia.ToString();
                                diaDisplay.Refresh();
                                //***********************************************
                                // Calculate the current slice length
                                //
                                current_slice_len = len - len_from_slice;
                                //***********************************************
                                // if length of slice is reached, Check absolute value
                                //
                                if (Math.Abs(current_slice_len) >= calc_slice)
                                {
                                    Update_Piece_Volume();
                                }

                                //***********************************************
                                // Change length display to new length
                                //
                                lengthDisplay.Text = len.ToString();
                                lengthDisplay.Refresh();
                                //**************************************
                                //* Check for target window and do
                                //* operations
                                //* ***********************************/
                                decimal fraction = 0.01m;
                                if (len < (dataBase.SelectedLength + (dataBase.SelectedOverLengthWindow * fraction)) &&
                                    len > (dataBase.SelectedLength - dataBase.SelectedUnderLengthWindow * fraction))
                                {
                                    lengthDisplay.BackColor = in_cutting_window_color;
                                    lengthDisplay.ForeColor = normal_reverse_forecolor;
                                    if (cbAudibleInZone.Checked)
                                        SystemSounds.Hand.Play();
                                }
                                else
                                {
                                    lengthDisplay.BackColor = normal_backcolor;
                                    lengthDisplay.ForeColor = normal_forecolor;
                                }
                            }
                            else if (tabControl1.SelectedIndex == tabControl1.TabPages.IndexOf(tabPage2))
                            {// Calibration page
                                lenMovPulses.Text = Length.ToString();
                                lenMovPulses.Refresh();
                                lblCalLen.Text = len.ToString();
                                lblCalLen.Refresh();
                                //Diameter
                                diaPulsMoved.Text = diaPulse.ToString();
                                diaPulsMoved.Refresh();
                            }
                        }
                        catch (System.FormatException)
                        {
                            break;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            break;
                        }
                        catch (Exception ed)
                        {
                            // This hides a Format Exception and should be corrected 7.27.2006
                            if (!ed.Message.StartsWith("FormatException"))
                            {
                                //MessageBox.Show("Exception at RxData (dat): " + ed.Message);
                                Error_Stamp("Exception at RxData (dat): " + ed.Message);
                            }
                        }
                        break;
                    case "$rdy":
                        //errorLbl.ForeColor = System.Drawing.Color.White;
                        //errorLbl.BackColor = System.Drawing.Color.Green;
                        // stop sync timer; rx'd ready from head
                        syncTimer.Stop();
                        // set indicator for problem back to normal
                        tabPageOps.BackColor = SystemColors.Control;
                        break;
                    case "$cmd":
                        //if (tempCom.Equals(hmString))
                        //{// Is the Rx information the same as the sent OR current command string							  
                        try
                        {
                            //if it is send turn pumps on command to GPIO 0x40
                            if (allPumpFlag)
                            {
                                //if (serialPort2.IsOpen)
                                //    serialPort2.WriteLine(GPIO.GPIOHDR + GPIO.PALL_On);
                                //else
                                //    MessageBox.Show("Pump Port Failure!!!", "Command: Pall");
                                fbPump1.State = BushApe.TriVal.triLO;
                                fbPump2.State = BushApe.TriVal.triLO;
                                allPumpFlag = false;
                            }
                            else
                            {
                                //if (serialPort2.IsOpen)
                                //    serialPort2.WriteLine(GPIO.GPIOHDR + GPIO.P1ON);
                                //else
                                //    MessageBox.Show("Pump Port Failure!!!", "Command: P1");
                                fbPump1.State = BushApe.TriVal.triLO;
                            }
                            // Indicate the pump is active
                            errorLbl.Text = "Pump(s)ON";
                            //Clear Timer
                            replyTimer.Stop();
                            timeOutCnt = 0;
                            //-----------
                        }
                        catch (Exception ec)
                        {
                            // This hides a Format Exception and should be corrected 7.27.2006
                            if (!ec.Message.StartsWith("FormatException"))
                            {
                                //MessageBox.Show("Exception at RxData (current command): " + ec.Message);
                                Error_Stamp("Exception at RxData (current command): " + ec.Message);

                            }
                        }
                        //}

                        // Always Call the recieved feedback function
                        this.FeedbackDisplayRX(tempData[1]);
                        break;
                    case "$nop":/*extract sensor information*/
                        PhotoEyeSensor(tempData[1]);
                        MainSawSensor(tempData[2]);
                        TopSawSensor(tempData[3]);

                        // stop nop timer
                        nopTmr.Change(-1, -1);
                        nopCnt = 0;
                        break;
                    case "$Before":
                        errorLbl.Text = "Head has been reset!";
                        break;
                    case "$test":
                        WriteLine_CM("wt41 $rdy");
                        break;

                }// End Switch


                iType = null;
                return;
            }
            catch (ThreadAbortException eTA)
            {
                Error_Stamp("Thread Abort Exception in BA 'InterpretTheadData(...)'" + eTA.Message);

            }
            catch (OutOfMemoryException)
            {
                // Ensure pumps are off 0x04
                WriteLine_CM(GPIO.GPIOHDR + GPIO.PALL_OFF.ToString());
                // Collect garbage memory
                GC.Collect();
                // try to restart
                tabControl1.SelectedIndex = 0;
                //
                Error_Stamp("Out of Memory: Interpret Thread Data");
            }
            catch (Exception er)
            {
                // This hides a Format Exception and should be corrected 7.27.2006
                if (!er.Message.StartsWith("FormatException"))
                {
                    //MessageBox.Show("Exception at RxData: " + er.Message);
                    Error_Stamp("Exception at RxData: " + er.Message);

                }
                else
                {
                    if (GC.GetTotalMemory(false) > 750)
                        GC.Collect();
                }

            }
            finally
            {
                //Error_Stamp("finally: Interpret Thread Data");

            }
        }
        /// <summary>
        /// Update the current volume of the current piece.
        /// Form most accurate volume, update current_slice_len before
        /// calling this function
        /// </summary>
        private void Update_Piece_Volume()
        {
            //****************************************
            // Calculate radius
            //
            decimal r = Decimal.Divide(dia, 2);
            //*************************************************
            // Calculate volume, pi * r^2 * height; Using displayed length v. current length as the slice to calculate (may be to accurate)
            //
            vol = vol + ((decimal)Math.PI * r * r * current_slice_len);//Decimal.Multiply(Decimal.Multiply((decimal)Math.PI, (decimal)Math.Pow((double)r, 2)), len - Convert.ToDecimal(lengthDisplay.Text));
            //**********************************************
            // Update the length from the last volume calulation
            //
            len_from_slice = len;
        }
        /// <summary>
        /// Sets the most reasonable point to begin ramping down
        /// </summary>
        /// <summary>
        /// Used to change to forward slow during an auto/semi-auto operation.
        /// </summary>
        private void RampDwnOps()
        {
            // If a length is selected the RampDown flag is true and the RampToPoint is set
            // as the distance from the selected length.

            //If forward feed 
            if (dirRx.State == BushApe.TriVal.triHI)
            {
                if (len >= (decimal)dataBase.SelectedLength)
                {
                    // Change to stop command
                    currentCommand = "wt41 $stp";
                    // Set Ramping flag to false
                    RampDwn = false;
                }
                else if (len >= RampDwnPoint)
                {
                    // Change current command to slow by one bump, reverse
                    currentCommand = "wt41 $cmd,fobd,\r\n";
                    pbSpeed.Value--;
                    pbSpeed.Refresh();
                    RampDwnPoint = len + rmpDwnStage;
                }

            }
            //If revearse feed
            else if (dirRx.State == BushApe.TriVal.triLO)
            {
                if (len <= (decimal)dataBase.SelectedLength)
                {
                    // Change to stop command
                    currentCommand = "wt41 $stp";
                    // Set Ramping flag to false
                    RampDwn = false;
                }
                else if (len <= RampDwnPoint)
                {
                    // Change current command to slow by one bump, reverse
                    currentCommand = "wt41 $cmd,rebd,\r\n";
                    pbSpeed.Value--;
                    pbSpeed.Refresh();
                    RampDwnPoint = len - rmpDwnStage;
                }

            }
            //*******************************************************
            // send the slow speed command for the current direction
            //
            WriteLine_CM(currentCommand);
        }
        /// <summary>
        /// Used to respond to time intervals
        /// </summary>
        private void RampUpOps()
        {
            // If forward feed
            if (dirTx.State == BushApe.TriVal.triHI)
            {

                // If length is past ramp point
                if (len >= RampToPoint)
                {
                    // Change current command to forward
                    currentCommand = "wt41 $cmd,forw,\r\n";
                    // Change ramping up flag to false 
                    RampingUp = false;
                    // Set the RampDown flag so the feed will slow before the length is found
                    RampDwn = true;
                    //Indicate full speed
                    pbSpeed.Value = pbSpeed.Maximum;
                }
                else if (len >= rmpStagePoint)
                {
                    currentCommand = "wt41 $cmd,fora,\r\n";
                    pbSpeed.Value++;
                    pbSpeed.Refresh();
                    rmpStagePoint = len + rmpUpStage;
                    if (WriteLine_CM(currentCommand))
                    {
                        //********************************************************
                        // send the full speed command for the current direction
                        //
                        return;//Skip the rest
                    }
                }
            }
            // If revearse feed
            else if (dirTx.State == BushApe.TriVal.triLO)
            {

                // If the length is less than the ramp to poing end the ramping procedure
                if (len <= RampToPoint)
                {
                    // Change current command to reverese
                    currentCommand = "wt41 $cmd,reve,\r\n";
                    // Change ramping up flag to false 
                    RampingUp = false;
                    // Set the RampDown flag so the feed will slow before the length is found
                    RampDwn = true;
                    //Indicate full speed
                    pbSpeed.Value = pbSpeed.Maximum;
                }// Otherwise continue ramping backwards.
                else if (len < rmpStagePoint) // unless the next stage point is reached
                {
                    currentCommand = "wt41 $cmd,rera,\r\n";
                    pbSpeed.Value++;
                    pbSpeed.Refresh();
                    rmpStagePoint = len - rmpDwnStage;
                    //***********************************************************
                    // send the full speed command for the current direction
                    //
                    WriteLine_CM(currentCommand);
                }
            }
        }
        /// <summary>
        /// This function is entered when a key is pressed while in
        /// the Tx text box; found on the Machine info tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                string[] tempArray = tbTx.Lines;
                int i = (tempArray.Length) - 1;
                string s = tempArray[i];
                WriteLine_CM(s);
            }
        }

        /// <summary>
        /// /// <summary>
        /// Timer operations
        /// </summary>
        ///         
        /// <summary>
        /// This is the syncronize operations to do
        /// when communications are stalled.

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void syncTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Make ready the new current command
                currentCommand = "wt41 $rdy,";
                // Send a ready? request
                CMserialPort1.DiscardOutBuffer();
                WriteLine_CM(currentCommand);
                // alternate from operational to warning color
                //if (tabPage1.BackColor.Equals(SystemColors.Control))
                //    tabPage1.BackColor = Color.Red;
                //else tabPage1.BackColor = SystemColors.Control;
            }
            catch (ThreadAbortException)
            {
                Error_Stamp("Thread Abort Exception in BA: syncOps(...)");

            }
            catch (Exception eb)
            {
                string message = "Caught in Sync ops: " + eb.Message;
                MessageBox.Show(message);
            }
            finally
            {
                //Error_Stamp("finally: syncOps");
                if (GC.GetTotalMemory(false) > 750)
                    GC.Collect();
            }
        }

        private void replyTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // re-send the current command
                WriteLine_CM(currentCommand);
                // for troubleshooting
                if (timeOutCnt++ >= COMMFAIL)
                {
                    //*****************************************
                    // Try to Stop pumps
                    //
                    if (!WriteLine_CM(GPIO.GPIOHDR + GPIO.PALL_OFF.ToString()))
                    {
                        MessageBox.Show("Pump Port Failure!!!", "Reply Timer");
                    }

                    // stop reply timer
                    replyTimer.Stop();
                    timeOutCnt = 0;
                    // start sync timer
                    syncTimer.Start();
                    // Indicating loss of communication
                    //MessageBox.Show("Lost contact with head; Searching...");
                }
            }
            catch (ThreadAbortException)
            {
                Error_Stamp("Thread Abort Exception in BA: 'Re Send(...)'");

            }
            catch (Exception es)
            {
                //MessageBox.Show("Caught in reSend: " + e.Message);
                Error_Stamp("Caught in reSend: " + es.Message);
            }
            finally
            {
                //Error_Stamp("finally: reSend");
                if (GC.GetTotalMemory(false) > 750)
                    GC.Collect();
            }
        }

        private void replyUpDwn_ValueChanged(object sender, EventArgs e)
        {
            replyTimer.Interval = (int)replyUpDwn.Value;
        }

        /// <summary>
        /// This function is called when data is present on the Rx buffer.
        /// It starts a thread that will retrieve the data in a thread safe
        /// manner.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {

                this.workThread1 = new Thread(new ThreadStart(this.ThreadProcReadPort1));
                this.workThread1.Start();
            }
            catch (ThreadAbortException te)
            {
                MessageBox.Show("Data recieved (thread): " + te.Message, "Head Module; Data Rx");
            }
        }

        private void suAcceptBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // read set up input text box 'suInputTxtBox'
                // make sure it does not contain the initial greeting or a '0' input
                if (suInputTxtBox.Text != "Press Input Button")
                {
                    //********************************************************************
                    // Note: The input selected by the one time operation was placed in 'OpInput' by
                    //		the 'InterpretGPIO(...)' functions
                    //
                    //******************************************
                    // get the Command to bind to.
                    //
                    int command = dataBase.GetCommandID(suCommandComboBox.Text);
                    //*******************************************
                    // Check for a valid input and assign it
                    //
                    AssignInput(command, ref suInputTxtBox);
                    //*******************************************
                    // clear filter
                    //
                    dataBase.ClearUser_Command = true;
                    tabControl1.TabPages[3].Refresh();
                }
                else
                {
                    MessageBox.Show("You must press a valid input before accepting!", "Accept Command");
                }
                //
                // Clear assign command flag
                //
                assignCommand = false;
                //
                // Clear input captured flag
                //
                //inputCaptured = false;
                //
                // Clear something changed
                //
                this.changesOnTab = false;
            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message, "Accept; su");
            }

        }
        /// <summary>
        /// Assign global OpInput to commandID; will confirm if the 
        /// assignment already exhists.
        /// </summary>
        /// <param name="commandID">Index of command in Command table</param>
        /// <param name="tb">Reference to text box where input index is located</param>
        private void AssignInput(int commandID, ref TextBox tb)
        {
            try
            {
                //************************************************************************
                // if the input is '0', do not accept change.
                //
                if (OpInput == 0)
                {
                    MessageBox.Show("Cannot Over-ride 'stop' command!");
                    return;
                }
                //*************************************************************************
                // if the input has already been assigned to a position by the user once,
                //		confirm user desire to replace.  REMEMBER...one command can have
                //		more than one input, but one input can have only one command.
                //
                if (dataBase.IsAssigned(OpInput))
                {
                    DialogResult result;
                    //*********************************************************
                    // Displays the MessageBox.
                    //
                    result = MessageBox.Show("Input is already assigned as '" + tb.Text + "'; Replace?", "SilverbackXP",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    //*******************************************************
                    // If user accepts a desire to change the input, update the data base.
                    //
                    if (result.Equals(DialogResult.Yes))
                    {
                        dataBase.ChangeUser_Command(OpInput, commandID, dataBase.SelectedOperator, -1);

                    }
                    else { return; }// end function if no change is desired.
                }
                else// if not in the grid, add a new line to the grid
                {
                    dataBase.AddUser_Command(OpInput, commandID, dataBase.SelectedOperator, -1);
                }
                //***************************************************
                // Reset indexed command in CM
                //
                AssignInput_CM(input_lvl_UpDown.Value.ToString(), OpInput.ToString(), dataBase.GetCmndStrng(OpInput));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Assign Input");
            }
        }
        /// <summary>
        /// This function will perform communications necessary to bind a command
        /// to an index on the Cabin Module.
        /// </summary>
        /// <param name="lvl">Input level; 1 or 2 (if shifted)</param>
        /// <param name="idx">Input index value i.e. button number</param>
        /// <param name="cmd">Command to bind to index</param>
        private void AssignInput_CM(string lvl, string idx, string cmd)
        {
            try
            {
                int lvl_iter = 1;
                int temp_idx = 0;
                //********************************************************
                // Error check: make sure parameters are not 0
                //
                if (lvl.Length == 0 || idx.Length == 0 || cmd.Length == 0)
                    return;
                //*******************************************************
                // Check for change from the shift button
                //
                if (suInputTxtBox.BackColor == System.Drawing.Color.DimGray ||
                    assBuckTB.BackColor == System.Drawing.Color.DimGray ||
                    cmd.StartsWith("shif"))
                {
                    //**********************************
                    // Change both leveles
                    //
                    lvl_iter = 2;
                }
                do
                {
                    //*******************************************************
                    // Prepare set message
                    //
                    string level = @"L" + lvl;
                    string index = " " + idx + " ";// ensure proper spacing
                    string message = "set " + level + index + cmd + " \r\n";
                    //**********************************
                    // Send set command to CM
                    //
                    WriteLine_CM(message);
                    //**********************************
                    // Set Assign input flag
                    //
                    AssignInputCM_flg = true;
                    //********************************************
                    // if assigning an input, save it to Flash
                    //
                    if (AssignInputCM_flg)
                    {
                        WriteLine_CM("pbsave \r\n");
                    }
                    if (--lvl_iter >= 1)
                    {
                        temp_idx = Convert.ToInt32(idx);
                        //***********************
                        // Re-set level and index
                        if (lvl.StartsWith("1"))
                        {
                            lvl = "2";
                            temp_idx += 32;
                        }
                        else
                        {
                            lvl = "1";
                            temp_idx -= 32;
                        }
                        idx = temp_idx.ToString();
                    }
                } while (lvl_iter > 0);


            }
            catch (Exception e_AssignInput_CM)
            {
                MessageBox.Show(e_AssignInput_CM.Message, "Exception in AssignInput_CM");
            }
        }


        /// <summary>
        /// Send a string message to the CM
        /// </summary>
        /// <param name="message">Text sentance to be sent</param>
        /// <returns>True if port was open and line sent, other wise False</returns>
        private bool WriteLine_CM(string message)
        {
            try
            {
                //*******************************************
                // If the terminal window is open
                //  place message on it.
                //
                if (terminalToolStripMenuItem.Checked)
                {
                    terminal.terminalTB_WriteLine("|GUI->" + message);
                }
                //********************************************
                // if the port is open send the message with
                // WriteLine
                //
                if (CMserialPort1.IsOpen)
                {
                    CMserialPort1.WriteLine(message + " \r\n");
                    CMserialPort1.DiscardOutBuffer();
                    return true;
                }
                return false;
            }
            catch (IOException e_WriteLine_CM)
            {
                MessageBox.Show(e_WriteLine_CM.Message, "Write Line CM Error");
                return false;
            }

        }
        /// <summary>
        /// Assign a bucking spec to a command ID.
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="buckSpec"></param>
        /// <param name="tb"></param>
        private void AssignBuckingInput(int cid, int buckSpec, ref TextBox tb)
        {
            try
            {
                // if the input is '0', do not accept change.
                if (OpInput == 0)
                {
                    MessageBox.Show("Cannot Over-ride 'stop' command!");
                    return;
                }
                //
                // if the input has already been assigned to a position by the user once,
                //		confirm user desire to replace.  REMEMBER...one command can have
                //		more than one input, but one input can have only one command.
                //
                if (dataBase.IsAssigned(OpInput))
                {
                    DialogResult result;
                    //
                    // Displays the MessageBox.
                    //
                    result = MessageBox.Show("Input is already assigned as '" + tb.Text + "'; Replace?", "SilverbackXP",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    //
                    // If user accepts a desire to change the input, update the data base.
                    //
                    if (result.Equals(DialogResult.Yes))
                    {

                        dataBase.ChangeUser_Command(OpInput, cid, dataBase.SelectedOperator, buckSpec);
                    }
                }
                else/*if not in the grid, add a new line to the grid*/
                {
                    dataBase.AddUser_Command(OpInput, cid, dataBase.SelectedOperator, buckSpec);
                }
                string preset = "ps";
                if (buckSpec <= 9)
                {
                    preset = preset + @"0" + buckSpec.ToString();
                }
                else
                {
                    preset = preset + buckSpec.ToString();
                }

                AssignInput_CM(input_lvl_UpDown.Value.ToString(), OpInput.ToString(), preset);
            }
            catch (Exception abiex)
            {
                MessageBox.Show(abiex.Message, "Assign Bucking Input");
            }
        }

        /// <summary>
        /// This function starts the set up sequence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void suNewBtn_Click(object sender, System.EventArgs e)
        {
            // put message in text box
            suInputTxtBox.Text = "Press Input Button";
            //// spawn a single thread to wait for input from the user	
            //// Start thread to read GPIO handles
            //input.Start(gpioData, true);
            //----------------------------------------------------------------------
            //
            // Initialize flags
            //
            assignBuckspec = false;
            assignCommand = true;
            //inputCaptured = false;
            //
            // Indicate a value has changed
            //
            ValueChanged(sender, e);

        }

        private void acceptBuckBTN_Click(object sender, EventArgs e)
        {
            try
            {
                // Species (required)
                if (cbBuckSpecies.SelectedIndex == 0)
                {
                    MessageBox.Show("Must select species; No Update Performed.");
                    return;
                }
                // Update bucking spec table
                //dataBase.UpdateBuckingSpec();
                //--------------------------------------------------
                if (assBuckTB.Text.Length > 0 && !assBuckTB.Text.StartsWith("Press"))
                {// if the button assign text box has an input number
                    // get a command ID based on the type and spec ID's
                    // if one does not exhist make it
                    //dataBase.AddCommand(6, "1", cbBuckSpecies.Text + ", " + lenTxtBx.Text + "m");
                    //Clear current table style
                    buckDG.TableStyles.Clear();
                    //
                    // Confirm there is a bucking spec selected in the visible rows
                    //
                    if (buckDG.CurrentRowIndex < 0)
                    {
                        MessageBox.Show("No Bucking Spec. Selected", "Assign Bucking Spec.");
                        return;
                    }
                    //
                    // get the bucking spec row ID, validate it, and assign the command ID
                    //
                    int buckSpecID = (int)buckDG[buckDG.CurrentRowIndex, 0];
                    //
                    // get a command id
                    //
                    int commandID = dataBase.ValidateBuckCmd(buckSpecID);
                    //
                    // Make sure there is a type 6 command in the Command table
                    //

                    if (commandID >= 0)
                    {
                        //******************************************************************
                        // assign the input to the command ID, found in assBuckTB (Text box)
                        //
                        AssignBuckingInput(commandID, buckSpecID, ref assBuckTB);
                        //
                        // reset the bucking table style
                        //
                        dataBase.CreateBuckTblStyle(ref buckDG);
                        RefreshBuckDG();
                    }
                    else
                    {
                        MessageBox.Show("Unable to assign input.", "Invalid input: Accept");
                    }

                }
                //
                // Clear the something changed flag
                //
                this.changesOnTab = false;
            }
            catch (Exception aex)
            {
                MessageBox.Show(aex.Message, "Accept Bucking Spec");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void RefreshBuckDG()
        {
            //
            buckDG.Refresh();
            int selectedIndex = cbBuckSpecies.SelectedIndex;
            // The data grid will not re-display the information 
            // until the species combo box changes its index
            if (selectedIndex == 0)
                cbBuckSpecies.SelectedIndex = 1;
            else
                cbBuckSpecies.SelectedIndex = 0;

            cbBuckSpecies.SelectedIndex = selectedIndex;
        }
        private void addBuckBTN_Click(object sender, EventArgs e)
        {

            // Species (required)
            if (cbBuckSpecies.SelectedIndex == 0)
            {
                MessageBox.Show("Must select species.");
                return;
            }
            //
            // Get Selected Species
            //
            //int species = cbBuckSpecies.SelectedIndex;
            //dataBase.CheckBuckTable(species, "Before txt box read");
            //
            // Length (required)
            //
            if (lenTxtBx.TextLength <= 0)
            {
                MessageBox.Show("Must enter Length.");
                return;
            }
            decimal length = Convert.ToDecimal(lenTxtBx.Text);
            //
            // Min diameter (required)
            //
            if (minTxtBx.TextLength <= 0)
            {
                MessageBox.Show("Must enter Minimum diameter.");
                return;
            }
            decimal min = Convert.ToDecimal(minTxtBx.Text);
            //
            // Max diameter
            //
            decimal max = 0.0m;
            if (maxTxtBx.TextLength > 0)
                max = Convert.ToDecimal(maxTxtBx.Text);
            //
            // Get note
            //
            string note = lenTxtBx.Text + "m";
            //
            // under window
            //
            decimal uw = Convert.ToDecimal(underWinTxtBx.Text);
            //
            // over window
            //
            decimal ow = Convert.ToDecimal(overWinTxtBx.Text);
            //dataBase.CheckBuckTable(cbBuckSpecies.SelectedIndex, "Before call");
            //
            // Add the new spec
            //
            dataBase.AddBuckingSpec(cbBuckSpecies.SelectedIndex, 0 /*for now*/, length, min, max, note, uw, ow);
            //
            // update the bucking grid
            //
            buckDG.Refresh();
        }

        private void assBuckBTN_Click(object sender, EventArgs e)
        {
            //
            // put message in text box
            //
            assBuckTB.Text = "Press Input Button";
            //
            // Initialize flags
            //
            assignCommand = false;
            assignBuckspec = true;
            //inputCaptured = false;
        }

        private void cbBuckSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            // Filter for index value
            // if the selected item is '0' the value is 'All' so 
            // remove filter
            //
            if (cbBuckSpecies.SelectedIndex == 0)
            {
                dataBase.ClearFilterBucking = true;
            }
            else
                dataBase.FilterBuckingBy(cbBuckSpecies.SelectedIndex);
            //
            // Make the selected species the selected index
            //
            dataBase.SelectedSpecies = cbBuckSpecies.SelectedIndex;
        }

        private void delBuckBTN_Click(object sender, EventArgs e)
        {
            buckDG.TableStyles.Clear();
            //Delete currently selected bucking spec
            buckDG.Select(buckDG.CurrentRowIndex);
            dataBase.DeletRecordBucking((int)buckDG[buckDG.CurrentRowIndex, 0]);
            dataBase.CreateBuckTblStyle(ref buckDG);
            buckDG.Refresh();
        }

        private void lenTxtBx_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buckDG.Refresh();
            }
        }

        private void lenTxtBx_Enter(object sender, EventArgs e)
        {

        }

        private void btnPump1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!WriteLine_CM(GPIO.GPIOHDR + GPIO.P1ON))
                    MessageBox.Show("Port is not open", "Pump 1 Test");


            }
            catch (Exception bex)
            {
                MessageBox.Show(bex.Message, "Error, P1 test");
            }
        }

        private void topPres_trkBar_Scroll(object sender, EventArgs e)
        {
            topUpDown1.Value = topPres_trkBar.Value;
        }

        private void wheelPres_trkBar_Scroll(object sender, EventArgs e)
        {
            wheelUpDown1.Value = wheelPres_trkBar.Value;
        }

        private void btmPres_trkBar_Scroll(object sender, EventArgs e)
        {
            bottomUpDown1.Value = btmPres_trkBar.Value;
        }

        private void speed_trkBar_Scroll(object sender, EventArgs e)
        {
            speedUpDown.Value = speed_trkBar.Value;
        }

        private void mid_trkBar_Scroll(object sender, EventArgs e)
        {
            midUpDown.Value = mid_trkBar.Value;
        }

        private void tbRampSpeed_Scroll(object sender, EventArgs e)
        {
            udRampStartSpeed.Value = tbRampSpeed.Value;
        }

        private void tbRampUpLen_Scroll(object sender, EventArgs e)
        {
            udRampUp.Value = tbRampUpLen.Value;
        }

        private void tbRampDwnLen_Scroll(object sender, EventArgs e)
        {
            udRampDwn.Value = tbRampDwnLen.Value;
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            lenRatioTxtBox.Text = dataBase.GetLengthScaler(dataBase.SelectedSpecies).ToString();
            diaRatioTxtBox.Text = dataBase.GetDiaScaler(dataBase.SelectedSpecies).ToString();
            lblCalLen.Text = lengthDisplay.Text;
        }

        private void spAcceptBtn_Click(object sender, EventArgs e)
        {
            //
            // send new setting information to head module and wait for confirmation
            //
            setStaticValue("top", (int)topUpDown1.Value);
            setStaticValue("wheels", (int)wheelUpDown1.Value);
            setStaticValue("bottom", (int)bottomUpDown1.Value);
            setStaticValue("speed", (int)speedUpDown.Value);
            setStaticValue("middle", (int)midUpDown.Value);
            setStaticValue("rampstart", (int)udRampStartSpeed.Value);
            setStaticValue("rampUp", (int)udRampUp.Value);
            setStaticValue("rampDown", (int)udRampDwn.Value);
            //setStaticValue("findButt", Convert.ToInt32(cbFindButt.Checked));
            setStaticValue("useRamp", Convert.ToInt32(cbUseRamps.Checked));
            //
            // Update Database
            //
            //dataBase.SetInfo(mfBox.Checked, SilverbackDB.MachineInfo.MultiFunc);
            dataBase.SetInfo(usLenCB.Checked, SilverbackDB.MachineInfo.StandardMeas);
            dataBase.SetInfo(floatBox.Checked, SilverbackDB.MachineInfo.TiltFloat);
            dataBase.SetInfo(semiAutoCB.Checked, SilverbackDB.MachineInfo.semiAuto);
            //dataBase.SetInfo(autoCB.Checked, SilverbackDB.MachineInfo.AutoFeed);
            //dataBase.SetInfo(cbLogProtect.Checked, SilverbackDB.MachineInfo.ProtectLog);
            dataBase.SetInfo(cbAudibleInZone.Checked, SilverbackDB.MachineInfo.AudibleInZone);
            dataBase.SetInfo(cbUseRamps.Checked, SilverbackDB.MachineInfo.UseRamps);
            //dataBase.SetInfo(cbFindButt.Checked, SilverbackDB.MachineInfo.FindButt);
            //
            //
            //
            dataBase.SetSettings(ref topUpDown1, (int)topUpDown1.Value, SilverbackDB.MachineSettings.TopK_Press);
            dataBase.SetSettings(ref wheelUpDown1, (int)wheelUpDown1.Value, SilverbackDB.MachineSettings.WheelA_Press);
            dataBase.SetSettings(ref bottomUpDown1, (int)bottomUpDown1.Value, SilverbackDB.MachineSettings.Botk_Press);
            dataBase.SetSettings(ref speedUpDown, (int)speedUpDown.Value, SilverbackDB.MachineSettings.Slow_Speed);
            dataBase.SetSettings(ref midUpDown, (int)midUpDown.Value, SilverbackDB.MachineSettings.Mid_Press);

            // Store the ramp information
            //
            dataBase.SetSettings(ref udRampUp, (int)udRampUp.Value, SilverbackDB.MachineSettings.RmpUp_Distance);
            dataBase.SetSettings(ref udRampDwn, (int)udRampDwn.Value, SilverbackDB.MachineSettings.RmpDown_Distance);
            dataBase.SetSettings(ref udRampStartSpeed, (int)udRampStartSpeed.Value, SilverbackDB.MachineSettings.RmpStart_Speed);
            //------------------------

            this.changesOnTab = false;

        }


        private void F_message(string msg, UInt32 fval)
        {

            switch (fval)
            {
                case 1:
                    if (silverbackXP.ActiveForm.TopLevel)
                    {
                        silverbackXP.ActiveForm.TopMost = false;
                    }

                    break;
                case 2:
                    silverbackXP.ActiveForm.TopMost = true;
                    break;
                case 3:
                    //
                    // Scroll through tabs
                    //
                    if (tabControl1.SelectedIndex < (tabControl1.TabCount - 1))
                    {
                        tabControl1.SelectedIndex++;
                    }
                    else
                    {
                        tabControl1.SelectedIndex = 0;
                    }

                    if (tabControl1.SelectedIndex == 0 || tabControl1.SelectedIndex == 6)
                    {
                        tabControl1.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        tabControl1.Dock = DockStyle.None;
                    }
                    break;
                case 4:
                    //tabControl1.SelectedTab.SelectNextControl(tabControl1.ac)
                    SendKeys.Send("{TAB}");
                    break;
                case 5:
                    SendKeys.Send("{ENTER}");


                    break;
                default:
                    MessageBox.Show(msg + " Checked", "F Message");
                    break;
            }
        }

        private void tabPage5_Leave(object sender, EventArgs e)
        {
            if (this.changesOnTab)
            {
                DialogResult result;
                // Displays the MessageBox.
                result = MessageBox.Show("Do you want to save your changes?", "Changes Not Saved",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                // Set backlight brightness
                switch (result)
                {
                    case DialogResult.OK:

                        if (assignBuckspec)
                            suAcceptBtn_Click(sender, e);
                        if (assignCommand)
                            suNewBtn_Click(sender, e);
                        this.changesOnTab = false;

                        break;
                    default:
                        break;
                }
            }
            //*******************************
            // If terminal window is open, close
            ////
            //terminal.Visible = false;
            //
            // Clear the changes tab
            //
            this.changesOnTab = false;
        }

        private void tabPage3_Leave(object sender, EventArgs e)
        {

            if (this.changesOnTab)
            {
                //DialogResult result;
                //// Displays the MessageBox.
                //result = MessageBox.Show("Do you want to save your changes?", "Changes Not Saved",
                //    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                ////
                //switch (result)
                //{
                //    case DialogResult.OK:


                //        break;
                //    default:
                //        break;
                //}
            }
            //
            // Clear the changes flag
            //
            this.changesOnTab = false;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            this.changesOnTab = true;
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            //comboBox_TypeName.SelectedIndex = dataBase.SelectedSpecies;
        }

        private void comboBox_TypeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //dataBase.SelectedSpecies = comboBox_TypeName.SelectedIndex;
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
            try
            {
                cbBuckSpecies.SelectedIndex = dataBase.SelectedSpecies;

            }
            catch (Exception e_tabpg5_enter)
            {
                MessageBox.Show(e_tabpg5_enter.Message, "Error on enter tab page 5");
            }

        }

        private void tabPage2_Leave(object sender, EventArgs e)
        {

            if (this.changesOnTab)
            {
                DialogResult result;
                // Displays the MessageBox.
                result = MessageBox.Show("Do you want to save your changes?", "Changes Not Saved",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                // Set backlight brightness
                switch (result)
                {
                    case DialogResult.OK:
                        if (txtBoxMl.Text != "")
                        {
                            calAcceptBtn_Click(sender, e);
                        }
                        if (measDiaTxtBox.Text != "")
                        {
                            calAcceptDiaBtn_Click(sender, e);
                        }

                        break;
                    default:
                        break;
                }
            }

            //
            // Clear the changes flag
            //
            this.changesOnTab = false;
        }

        private void calAcceptBtn_Click(object sender, EventArgs e)
        {
            try
            {
                decimal measuredValue = 0;
                // Read rotations 'lenMovPulses'	
                // If metric is selected
                if (!usLenCB.Checked)
                {
                    measuredValue = System.Convert.ToDecimal(txtBoxMl.Text);
                }
                else if (usLenCB.Checked)
                {// If US is selected
                    // Convert to feet decimals
                    Dimension convert = new Dimension();
                    measuredValue = convert.cvrtItoF(System.Convert.ToDecimal(txtBoxInch.Text),
                        System.Convert.ToDecimal(txtBox16th.Text)); // Convert inches and sixteenth to decimal feet
                    measuredValue = measuredValue + System.Convert.ToDecimal(txtBoxFt.Text);
                }

                decimal lenpulses = System.Convert.ToDecimal(lenMovPulses.Text);
                if (lenpulses > 0)
                {
                    //
                    // And Calculate lengthScaler => Rotations * lengthScaler = Meters 
                    //
                    lengthScaler = Math.Round(measuredValue / lenpulses, 8);//change to 8 on aug. 27, 2009
                }
                else
                {
                    MessageBox.Show("Length pulses = 0, assuming 1 to 1 ratio");
                    lengthScaler = (decimal)1.0;
                }
                //
                // Store Scaler
                //
                dataBase.reSetLengthScaler((float)lengthScaler, 1/*dataBase.SelectedSpecies*/);// CHANGE TYPE TO SELECTABLE            
                //
                // show lengthScaler in Ratio Text box			
                //
                lenRatioTxtBox.Text = lengthScaler.ToString();

                //
                // Accepted changes
                //
                this.changesOnTab = false;

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Accept Length Cal");
            }
        }

        private void calAcceptDiaBtn_Click(object sender, EventArgs e)
        {
            decimal measuredValue1;
            decimal diapulses;
            measuredValue1 = System.Convert.ToDecimal(measDiaTxtBox.Text);
            diapulses = System.Convert.ToDecimal(diaPulsMoved.Text);
            if (diapulses > 0)
            {
                //
                // Calculate scaler
                //
                diaScaler = Math.Round(measuredValue1 / diapulses, 8);
            }
            else
            {
                MessageBox.Show("Diameter pulses 0, assuming 1 to 1 ratio", "calAcceptDiaBtn");
                diaScaler = (decimal)1.0;
            }
            //
            // Store diameter scaler
            //
            dataBase.reSetDiaScaler((float)diaScaler, 1/*dataBase.SelectedSpecies*/);
            //
            // Show Ratio
            //
            diaRatioTxtBox.Text = diaScaler.ToString();
            //
            // Accepted changes
            //
            this.changesOnTab = false;
        }

        private void cbTypeCalTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataBase.SelectedSpecies = cbTypeCalTab.SelectedIndex;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTx_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string[] tempArray = tbTx.Lines;
                    int i = (tempArray.Length) - 1;
                    string s = tempArray[i];
                    WriteLine_CM(s);
                }
            }
            catch (Exception kdEx)
            {
                MessageBox.Show(kdEx.Message, "tbTx KeyDown");

            }
        }

        private void SilverbackXP_Load(object sender, EventArgs e)
        {
            sfMap1.MouseWheel += new MouseEventHandler(sfmap_mouseWheel);
            var images = Directory.GetFiles(Application.StartupPath + "\\icon\\");
            int y = 19;
            int buttonNumber = 1;
            foreach (var img in images)
            {
                Button buttonAdd = new Button();
                //Initialize Properties
                // buttonAdd.Text = "Add";
                //var img = new Image();
                // drawim
                buttonAdd.Name = Path.GetFileName(img);
                buttonAdd.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
                buttonAdd.ImageAlign = ContentAlignment.MiddleCenter;
                buttonAdd.Size = new System.Drawing.Size(34, 34);
                buttonAdd.BackgroundImage = Image.FromFile(img);
                buttonAdd.BackColor = Color.Silver;
                buttonAdd.FlatStyle = FlatStyle.Flat;
                buttonAdd.FlatAppearance.BorderSize = 0;

                //Make the button show up in the middle of the form and right below the last input box
                buttonAdd.Location = new Point(25, y);
                buttonAdd.TabIndex = 1;
                buttonAdd.TabStop = true;
                //Add an event handler to the button's click event
                buttonAdd.Click += new EventHandler(icon2_Click);
                this.panel2.Controls.Add(buttonAdd);
                y += 57;
                buttonNumber++;
            }


            setStaticValue("top", (int)topUpDown1.Value);
            setStaticValue("wheels", (int)wheelUpDown1.Value);
            setStaticValue("bottom", (int)bottomUpDown1.Value);
            setStaticValue("speed", (int)speedUpDown.Value);
            setStaticValue("middle", (int)midUpDown.Value);
            setStaticValue("rampTo", (int)udRampStartSpeed.Value);
            setStaticValue("rampDown", (int)udRampStartSpeed.Value);
            setStaticValue("rampstart", (int)udRampStartSpeed.Value);
            setStaticValue("useRamp", Convert.ToInt32(cbUseRamps.Checked));
            //
            // Go back to the operations tab
            //
            tabControl1.SelectedIndex = 0;

            //ComPortScan scanports = new ComPortScan();
            //string allComAndLTPports = scanports.allPortNames();
            //string gpsPortName = scanports.selectGpsPort(allComAndLTPports);
            //if (string.IsNullOrEmpty(gpsPortName))
            //{
            //    MessageBox.Show("Gps device not found");
            //    DefaultPortForGPS = "COM9";
            //}

            //else
            //{
            //    DefaultPortForGPS = gpsPortName;
            //}
        }

        private void sfmap_mouseWheel(object sender, MouseEventArgs e)
        {
            // MessageBox.Show(""+sfMap1.ZoomLevel * 10);
        }

        private void cbLogProtect_CheckedChanged(object sender, EventArgs e)
        {

            //
            // If Log protection is selected, send that information
            //
            //    if (cbLogProtect.Checked)
            //    {
            //        WriteLine_CM("$set,lpro,1");
            //    }
            //    else
            //        WriteLine_CM("$set,lpro,0");
        }

        private void nudSawToSaw_Leave(object sender, EventArgs e)
        {
            SAWtoSAW = nudSawToSaw.Value;
        }

        private void tbRampSpeed_Scroll_1(object sender, EventArgs e)
        {
            udRampStartSpeed.Value = tbRampSpeed.Value;
        }

        private void semiAutoCB_CheckedChanged(object sender, EventArgs e)
        {
            //if (semiAutoCB.Checked)
            //    autoCB.Checked = false;

            //
            // Tell parent the values have changed
            //
            ValueChanged(sender, e);
        }

        //private void autoCB_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (autoCB.Checked)
        //        semiAutoCB.Checked = false;

        //    //
        //    // Tell parent the values have changed
        //    //
        //    ValueChanged(sender, e);
        //}




        private void otherMaxPRtb_ValueChanged(object sender, EventArgs e)
        {
            //WriteLine_CM("$set,otma," + otherMaxPRtb.Value.ToString());
        }

        private void tbMaxTKPR_ValueChanged(object sender, EventArgs e)
        {
            //WriteLine_CM("$set,tkma," + tbMaxTKPR.Value.ToString());
        }

        private void tbMaxWAPR_ValueChanged(object sender, EventArgs e)
        {
            //WriteLine_CM("$set,wama," + tbMaxWAPR.Value.ToString());
        }

        private void tbMaxBKPR_ValueChanged(object sender, EventArgs e)
        {
            //WriteLine_CM("$set,bkma," + tbMaxBKPR.Value.ToString());
        }

        private void diaZerob_Click(object sender, EventArgs e)
        {
            WriteLine_CM("$cmd,clrd,");
        }

        private void lengthToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (lengthToolStripMenuItem.Checked)
            {
                grpLength.Visible = true;
            }
            else if (!lengthToolStripMenuItem.Checked)
            {
                grpLength.Visible = false;
            }
        }

        private void diameterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (diameterToolStripMenuItem.Checked)
            {
                grpDia.Visible = true;
            }
            else if (!diameterToolStripMenuItem.Checked)
            {
                grpDia.Visible = false;
            }
        }

        private void productionToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (productionToolStripMenuItem.Checked)
            {
                grpProduction.Visible = true;
            }
            else if (!productionToolStripMenuItem.Checked)
            {
                grpProduction.Visible = false;
            }
        }

        private void sawSelecToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (sawSelToolStripMenuItem.Checked)
            {
                buttSawRB.Visible = true;
                topSawRB.Visible = true;
            }
            else if (!sawSelToolStripMenuItem.Checked)
            {
                buttSawRB.Visible = false;
                topSawRB.Visible = false;
            }
        }

        private void commandStatusToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (commandStatusToolStripMenuItem.Checked)
            {
                grpFeedback.Visible = true;
            }
            else if (!commandStatusToolStripMenuItem.Checked)
            {
                grpFeedback.Visible = false;
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            var indx = e.TabPageIndex;
            if (indx == 0 || indx == 6)
            {
                tabControl1.Dock = DockStyle.Fill;
            }
            else
            {
                tabControl1.Dock = DockStyle.None;
            }
            //grpDiaCal.Location= new Point(588,6);
            //var location = grpDiaCal.Location;

            //MessageBox.Show("Tab control selected.");
            //
            // Lock out tab pages higher than 4
            //
            if (tabControl1.SelectedIndex > 4)
            {
                tabControl1.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// Synchronize feedback menu's
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useFeedRampsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (useFeedRampsToolStripMenuItem.Checked)
            {
                grpRamp.Visible = true;
                cbUseRamps.Checked = true;
                return;
            }

            grpRamp.Visible = false;
            cbUseRamps.Checked = false;
        }
        /// <summary>
        /// Synchronize ramp menu's
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbUseRamps_CheckedChanged(object sender, EventArgs e)
        {
            if (cbUseRamps.Checked)
            {
                useFeedRampsToolStripMenuItem.Checked = true;
                return;
            }
            useFeedRampsToolStripMenuItem.Checked = false;

        }
        /// <summary>
        /// Syncronize photo eye menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void photoEyeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (photoEyeToolStripMenuItem.Checked)
            {
                photoCB.Checked = true;
                return;
            }

            photoCB.Checked = false;
        }
        /// <summary>
        /// Synchronize with menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void photoCB_CheckedChanged(object sender, EventArgs e)
        {
            if (photoCB.Checked)
            {
                photoEyeToolStripMenuItem.Checked = true;
                return;
            }
            photoEyeToolStripMenuItem.Checked = false;
        }

        /// <summary>
        /// Synchronize main saw encoder menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainSawEncoderToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (mainSawEncoderToolStripMenuItem.Checked)
            {
                msEncCB.Checked = true;
                return;
            }
            msEncCB.Checked = false;
        }
        /// <summary>
        /// Synchronize main saw encoder check box with menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msEncCB_CheckedChanged(object sender, EventArgs e)
        {
            if (msEncCB.Checked)
            {
                mainSawEncoderToolStripMenuItem.Checked = true;
                return;
            }

            mainSawEncoderToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Synchronize top saw menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topSawToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (topSawToolStripMenuItem.Checked)
            {
                tsawCB.Checked = true;
                return;
            }
            tsawCB.Checked = false;
        }
        /// <summary>
        /// Synchronize top saw check box with menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsawCB_CheckedChanged(object sender, EventArgs e)
        {
            if (tsawCB.Checked)
            {
                topSawToolStripMenuItem.Checked = true;
                return;
            }

            topSawToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Top saw encoder menu Synch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topSawEncoderToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (topSawEncoderToolStripMenuItem.Checked)
            {
                tsEncCB.Checked = true;
                return;
            }
            tsEncCB.Checked = false;
        }
        /// <summary>
        /// Top saw check box synch to menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsEncCB_CheckedChanged(object sender, EventArgs e)
        {
            if (tsEncCB.Checked)
            {
                topSawEncoderToolStripMenuItem.Checked = true;
                return;
            }

            topSawEncoderToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Synchronize menu with us length check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uSMeasureToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (uSMeasureToolStripMenuItem.Checked)
            {
                usLenCB.Checked = true;
                return;
            }
            usLenCB.Checked = false;
        }
        /// <summary>
        /// us Length check box synch to menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void usLenCB_CheckedChanged(object sender, EventArgs e)
        {
            if (usLenCB.Checked)
            {
                uSMeasureToolStripMenuItem.Checked = true;
                return;
            }
            uSMeasureToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Synch menu to length target tone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lengthTargetToneToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (lengthTargetToneToolStripMenuItem.Checked)
            {
                cbAudibleInZone.Checked = true;
                return;
            }
            cbAudibleInZone.Checked = false;
        }
        /// <summary>
        /// Synch audible check box with menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAudibleInZone_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAudibleInZone.Checked)
            {
                lengthTargetToneToolStripMenuItem.Checked = true;
                return;
            }
            lengthTargetToneToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Combined butt/top saw menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void combineButtTopSawButtonToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (combineButtTopSawButtonToolStripMenuItem.Checked)
            {
                oneSawSelectCB.Checked = true;
                return;
            }
            oneSawSelectCB.Checked = false;
        }
        /// <summary>
        /// Synch combined butt/top saw menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oneSawSelectCB_CheckedChanged(object sender, EventArgs e)
        {
            if (oneSawSelectCB.Checked)
            {
                combineButtTopSawButtonToolStripMenuItem.Checked = true;
                return;
            }
            combineButtTopSawButtonToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Synchronize menu with semi auto check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void semiAutoFeedToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (semiAutoFeedToolStripMenuItem.Checked)
            {
                semiAutoCB.Checked = true;
                return;
            }
            semiAutoCB.Checked = false;
        }
        /// <summary>
        /// Synchronize semiAuto checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void semiAutoCB_CheckedChanged_1(object sender, EventArgs e)
        {
            if (semiAutoCB.Checked)
            {
                semiAutoFeedToolStripMenuItem.Checked = true;
                return;
            }
            semiAutoFeedToolStripMenuItem.Checked = false;
        }
        /// <summary>
        /// Open/close function test form from tools menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void functionTestToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (functionTestToolStripMenuItem.Checked)
            {
                functionTest = new FunctionTest_SXP(WriteLine_CM);

                if (functionTest.Visible)
                {
                    functionTest.TopMost = true;
                    return;
                }
            }
            functionTest.Close();
            functionTestToolStripMenuItem.Checked = false;
        }

        private void rawComToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (rawComToolStripMenuItem.Checked)
            {
                grpInfo.Visible = true;
            }
            else if (!rawComToolStripMenuItem.Checked)
            {
                grpInfo.Visible = false;
            }
        }

        private void terminalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (terminalToolStripMenuItem.Checked)
            {
                //*****************************************************
                // Show terminal window
                //
                terminal = new terminal_SXP(ref CMserialPort1);
                if (terminal.Visible)
                {
                    terminal.Visible = true;
                    terminal.TopLevel = true;
                }

            }
            else if (!terminalToolStripMenuItem.Checked)
            {
                terminal.Close();
                terminalToolStripMenuItem.Checked = false;
                return;
            }
        }
        /// <summary>
        /// Handle a check changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttSawRB_CheckedChanged(object sender, EventArgs e)
        {
            if (buttSawRB.Checked)
            {
                //********************
                // Butt saw no offset
                //
                decimal tempLen = Convert.ToDecimal(lengthDisplay.Text) - SAWoffset;
                SAWoffset = 0.0m;
                //*******************************************************************
                // Update alternate length display before change to length display
                //
                AltLenDisplay.ForeColor = topSaw_fore_color;
                AltLenDisplay.Text = lengthDisplay.Text;
                //**************************
                // Update length
                //
                lengthDisplay.Text = tempLen.ToString();
                //***************************
                // Change color back to default
                //
                lengthDisplay.ForeColor = buttSaw_fore_color;

            }
        }
        /// <summary>
        /// Handle a check changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topSawRB_CheckedChanged(object sender, EventArgs e)
        {
            if (topSawRB.Checked)
            {
                //**************************
                // Top saw requires off set
                //
                SAWoffset = SAWtoSAW;
                //**************************
                //
                //
                decimal tempLen = Convert.ToDecimal(lengthDisplay.Text) + SAWoffset;
                //****************************************************************
                // update alternate length display before changing length display
                //
                AltLenDisplay.ForeColor = buttSaw_fore_color;
                AltLenDisplay.Text = lengthDisplay.Text;
                //***************************
                //Update length
                //
                lengthDisplay.Text = tempLen.ToString();
                //**************************************
                // Change color to top length
                //
                lengthDisplay.ForeColor = topSaw_fore_color;
            }
        }

        private void alternateLengthToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (alternateLengthToolStripMenuItem.Checked)
            {
                AltLenDisplay.Visible = true;
                return;
            }
            AltLenDisplay.Visible = false;
        }

        private void lengthDisplay_TextChanged(object sender, EventArgs e)
        {
            decimal tempLen = Convert.ToDecimal(lengthDisplay.Text);

            if (topSawRB.Checked)
            {
                AltLenDisplay.Text = (tempLen - SAWtoSAW).ToString();
                return;
            }
            if (buttSawRB.Checked)
            {
                AltLenDisplay.Text = (tempLen + SAWtoSAW).ToString();
            }
        }

        private void btnClearPulse_Click(object sender, EventArgs e)
        {
            WriteLine_CM("wt41 $cmd,clrl ");
            WriteLine_CM("wt41 $nop");
        }

        private void CrossBtn_Click(object sender, EventArgs e)
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_SHOW);
            this.Close();
        }

        private void grpDia_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void grpBtnSetup_Enter(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #region MAP Work Begins from here
        /// <summary>
        /// MAP works start here...
        ///     EGIS library from www.easygisdotnet.com
        /// </summary>
        private EGIS.ShapeFileLib.PointD currentMarkerPosition = new EGIS.ShapeFileLib.PointD();
        public EGIS.ShapeFileLib.PointD marker2 = new EGIS.ShapeFileLib.PointD();
        public EGIS.ShapeFileLib.PointD markerWhenShowNote = new EGIS.ShapeFileLib.PointD();

        private const int MarkerWidth = 7;
        private bool portOpen = false;
        private GpsReader gr = null;
        public List<CoordinateData> flag = new List<CoordinateData>();

        //return Output is Kilometer
        public double distanceMeasure(double easting1, double northing1, double easting2, double northing2)
        {
            return Math.Sqrt(((easting1 - easting2) * (easting1 - easting2)) + ((northing1 - northing2) * (northing1 - northing2)));
        }

        private DataAccess dataAccess;

        #region Paint
        private void sfMap1_Paint(object sender, PaintEventArgs e)
        {
            DrawMarkerForMoving(e.Graphics, currentMarkerPosition.X, currentMarkerPosition.Y);
        }

        private void sfMap1_Paint2(object sender, PaintEventArgs e)
        {
            DrawMarkerForMarkPin(e.Graphics, marker2.X, marker2.Y);
            foreach (var data in flag)
            {
                DrawMarkerForPin(e.Graphics, data.getLongatude(), data.getLatitude(), data.getIconName());
            }
        }

        private void sfMap1_Paint3(object sender, PaintEventArgs e)
        {
            DrawMarkerForMarkPin(e.Graphics, markerWhenShowNote.X, markerWhenShowNote.Y);
        }
        #endregion

        #region Draw Marker
        private void DrawMarkerWhenShowNote(Graphics g, double locX, double locY)
        {
            //convert the gis location to pixel coordinates
            Point pt = sfMap1.GisPointToPixelCoord(locX, locY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //draw a marker centered at the gis location
            //alternative is to draw an image/icon
            try
            {
                g.DrawLine(Pens.Black, pt.X, pt.Y - MarkerWidth, pt.X, pt.Y + MarkerWidth);
                g.DrawLine(Pens.Black, pt.X - MarkerWidth, pt.Y, pt.X + MarkerWidth, pt.Y);
                pt.Offset(-MarkerWidth / 2, -MarkerWidth / 2);
                g.FillEllipse(Brushes.Red, pt.X, pt.Y, MarkerWidth, MarkerWidth);
                g.DrawEllipse(Pens.Black, pt.X, pt.Y, MarkerWidth, MarkerWidth);
            }
            catch { }
        }

        //draws a marker at gis location locX,locY for Pin
        private void DrawMarkerForMarkPin(Graphics g, double locX, double locY)
        {
            //convert the gis location to pixel coordinates
            Point pt = sfMap1.GisPointToPixelCoord(locX, locY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            try
            {
                g.DrawLine(Pens.Black, pt.X, pt.Y - MarkerWidth, pt.X, pt.Y + MarkerWidth);
                g.DrawLine(Pens.Black, pt.X - MarkerWidth, pt.Y, pt.X + MarkerWidth, pt.Y);
                pt.Offset(-MarkerWidth / 2, -MarkerWidth / 2);
                g.FillEllipse(Brushes.Yellow, pt.X, pt.Y, MarkerWidth, MarkerWidth);
                g.DrawEllipse(Pens.ForestGreen, pt.X, pt.Y, MarkerWidth, MarkerWidth);
            }
            catch { }

        }

        private void DrawMarkerForPin(Graphics g, double locX, double locY, string iconName)
        {
            //convert the gis location to pixel coordinates
            Point pt = sfMap1.GisPointToPixelCoord(locX, locY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (!string.IsNullOrEmpty(iconName))
            {
                try
                {
                    //Loading icon after starting app
                    Bitmap myBitmap = new Bitmap(Application.StartupPath + "\\icon\\" + iconName);
                    pt.X = pt.X - (myBitmap.Width / 2);
                    pt.Y = pt.Y - (myBitmap.Height / 2);
                    g.DrawImage(myBitmap, pt.X, pt.Y);
                }
                catch (Exception ex)
                {
                    try
                    {
                        //If not find any icon in icon folder it is loading default icon 
                        iconName = "def.png";
                        Bitmap myBitmap = new Bitmap(Application.StartupPath + "\\defaultIcon\\" + iconName);
                        pt.X = pt.X - (myBitmap.Width / 2);
                        pt.Y = pt.Y - (myBitmap.Height / 2);
                        g.DrawImage(myBitmap, pt.X, pt.Y);

                    }
                    catch
                    {
                        MessageBox.Show("Default icon not found.");
                    }
                }
            }

        }


        /// <summary>
        /// draws a marker at gis location locX,locY for moving Object
        /// </summary>
        /// <param name="g"></param>
        /// <param name="locX"></param>
        /// <param name="locY"></param>
        private void DrawMarkerForMoving(Graphics g, double locX, double locY)
        {
            //convert the gis location to pixel coordinates

            Point pt = sfMap1.GisPointToPixelCoord(locX, locY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //draw a marker centered at the gis location
            //alternative is to draw an image/icon
            //Bitmap myBitmap = new Bitmap(Application.StartupPath + "\\icon\\icon3.png");
            //g.DrawImage(myBitmap, pt.X, pt.Y);
            try
            {
                //MessageBox.Show("Blue point: " + locX + " " + locY);
                g.DrawLine(Pens.ForestGreen, pt.X, pt.Y - MarkerWidth, pt.X, pt.Y + MarkerWidth);
                g.DrawLine(Pens.ForestGreen, pt.X - MarkerWidth, pt.Y, pt.X + MarkerWidth, pt.Y);
                pt.Offset(-MarkerWidth / 2, -MarkerWidth / 2);
                g.FillEllipse(Brushes.Blue, pt.X, pt.Y, MarkerWidth, MarkerWidth);
                g.DrawEllipse(Pens.ForestGreen, pt.X, pt.Y, MarkerWidth, MarkerWidth);
            }
            catch
            {
                //MessageBox.Show("Not possible to do Zoom more.");
            }
        }


        private void DrawCircle(Graphics g, double locX, double locY, int radius)
        {
            //convert the gis location to pixel coordinates

            Point pt = sfMap1.GisPointToPixelCoord(locX, locY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //draw a marker centered at the gis location
            //alternative is to draw an image/icon
            //Bitmap myBitmap = new Bitmap(Application.StartupPath + "\\icon\\icon3.png");
            //g.DrawImage(myBitmap, pt.X, pt.Y);
            try
            {
                g.DrawLine(Pens.ForestGreen, pt.X, pt.Y - radius, pt.X, pt.Y + radius);
                g.DrawLine(Pens.ForestGreen, pt.X - radius, pt.Y, pt.X + radius, pt.Y);
                pt.Offset(-radius / 2, -radius / 2);
                g.FillEllipse(Brushes.Blue, pt.X, pt.Y, radius, radius);
                g.DrawEllipse(Pens.ForestGreen, pt.X, pt.Y, radius, radius);
            }
            catch
            {
                //MessageBox.Show("Not possible to do Zoom more.");
            }
        }
        #endregion

        #region ConvertLongatudeAndLatitudeToUTM
        /// <summary>
        /// Convert GPS Longatude and Latitude To UTM Coordinate
        /// </summary>
        /// <param name="latitude"></param>
        /// <returns></returns>
        private static string GetBand(double latitude)
        {
            if (latitude <= 84 && latitude >= 72)
                return "X";
            else if (latitude < 72 && latitude >= 64)
                return "W";
            else if (latitude < 64 && latitude >= 56)
                return "V";
            else if (latitude < 56 && latitude >= 48)
                return "U";
            else if (latitude < 48 && latitude >= 40)
                return "T";
            else if (latitude < 40 && latitude >= 32)
                return "S";
            else if (latitude < 32 && latitude >= 24)
                return "R";
            else if (latitude < 24 && latitude >= 16)
                return "Q";
            else if (latitude < 16 && latitude >= 8)
                return "P";
            else if (latitude < 8 && latitude >= 0)
                return "N";
            else if (latitude < 0 && latitude >= -8)
                return "M";
            else if (latitude < -8 && latitude >= -16)
                return "L";
            else if (latitude < -16 && latitude >= -24)
                return "K";
            else if (latitude < -24 && latitude >= -32)
                return "J";
            else if (latitude < -32 && latitude >= -40)
                return "H";
            else if (latitude < -40 && latitude >= -48)
                return "G";
            else if (latitude < -48 && latitude >= -56)
                return "F";
            else if (latitude < -56 && latitude >= -64)
                return "E";
            else if (latitude < -64 && latitude >= -72)
                return "D";
            else if (latitude < -72 && latitude >= -80)
                return "C";
            else
                return null;
        }

        private static int GetZone(double latitude, double longitude)
        {
            // Norway
            if (latitude >= 56 && latitude < 64 && longitude >= 3 && longitude < 13)
                return 32;

            // Spitsbergen
            if (latitude >= 72 && latitude < 84)
            {
                if (longitude >= 0 && longitude < 9)
                    return 31;
                else if (longitude >= 9 && longitude < 21)
                    return 33;
                if (longitude >= 21 && longitude < 33)
                    return 35;
                if (longitude >= 33 && longitude < 42)
                    return 37;
            }

            return (int)Math.Ceiling((longitude + 180) / 6);
        }

        public void ConvertToUtmCoordinate(double latitude, double longitude, out double utmlatitude, out double utmLongitude)
        {
            if (latitude < -80 || latitude > 84)
            {
                //return null;
                utmlatitude = 0.00;
                utmLongitude = 0.00;
                return;
            }

            int zone = GetZone(latitude, longitude);
            string band = GetBand(latitude);

            //Transform to UTM
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            ICoordinateSystem utm = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(zone, latitude > 0);
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geo, utm);
            double[] pUtm = trans.MathTransform.Transform(new double[] { longitude, latitude });

            double easting = pUtm[0];
            double northing = pUtm[1];

            utmLongitude = easting;
            utmlatitude = northing;
        }

        #endregion
        /// <summary>
        /// Continiously Scanning for gps data and update all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void packetTimer_Tick(object sender, EventArgs e)
        {

            GpsPacket packet = null;
            //Check portopen
            if (portOpen)
            {
                //Get GPS Data
                string gpsData = gr.gpsData(GPSserialPort2);
                if (gpsData != null)
                {
                    packet = new GpsPacket(gpsData);
                }

                if (packet != null)
                {
                    double latitude, longitude;
                    ConvertToUtmCoordinate(packet.Latitude, packet.Longitude, out latitude, out longitude);
                    currentMarkerPosition = new EGIS.ShapeFileLib.PointD(longitude, latitude);

                    foreach (var data in flag)
                    {
                        double dis = distanceMeasure(data.getLongatude(), data.getLatitude(), longitude, latitude);
                        if (dis <= 100.00)
                        {
                            if (!data.getStatus())
                            {
                                PinShow childShow = new PinShow(this, data.getNote());
                                markerWhenShowNote.X = data.getLongatude();
                                markerWhenShowNote.Y = data.getLatitude();
                                data.setStatus(true);
                                childShow.Show();
                            }
                        }
                        else
                        {
                            data.setStatus(false);
                        }
                    }

                    //childShow.note = pinWork.getPinData(  );
                }
                sfMap1.Refresh();
            }

        }

        /// <summary>
        /// Update Xml node in XML File with new pin
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public bool UpdateXmlNodeByID(List<CoordinateData> list, Guid id, string note)
        {
            bool IsUpdatedFromXML = dataAccess.UpdateSingleNodeByID(id, note);
            foreach (var item in list)
            {
                if (item.getID() == id)
                {
                    item.setNote(note);
                    return IsUpdatedFromXML;
                }
            }
            return false;
        }

        /// <summary>
        /// Delete Xml node from XML file and List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveXmlDataByID(List<CoordinateData> list, Guid id)
        {

            bool IsDeletedFromXML = dataAccess.DeleteNodeByID(id);

            foreach (var item in list)
            {

                if (item.getID() == id)
                {

                    list.Remove(item);
                    return IsDeletedFromXML;
                }
            }
            return false;
        }

        /// <summary>
        /// Adding any icon on map it needs to update XML file and Globel list
        /// </summary>
        /// <param name="pinNote"></param>
        public void UpdateMe(string pinNote)
        {
            //Status update.
            Guid Id = Guid.NewGuid();
            CoordinateData data = new CoordinateData(marker2.X, marker2.Y, pinNote, selectIcon, false, Id);
            flag.Add(data);
            data addDataToXmlFile = new data(marker2.X, marker2.Y, pinNote, selectIcon, Id);
            dataAccess.addData(addDataToXmlFile);
            //dataAccess.DeleteNode(addDataToXmlFile);
            selectIcon = null;
            //pinWork.setPinData(marker2.X, marker2.Y, pinNote);
        }

        private void OpenShapefile_Road(string path)
        {
            // clear any shapefiles the map is currently displaying
            this.sfMap1.ClearShapeFiles();

            // open the shapefile passing in the path, display name of the shapefile and
            // the field name to be used when rendering the shapes (we use an empty string
            // as the field name (3rd parameter) can not be null)
            this.sfMap1.AddShapeFile(path, "ShapeFile", "");

            // read the shapefile dbf field names and set the shapefiles's RenderSettings
            // to use the first field to label the shapes.
            EGIS.ShapeFileLib.ShapeFile sf = this.sfMap1[0];
            sf.RenderSettings.FieldName = sf.RenderSettings.DbfReader.GetFieldNames()[0];
        }

        /// <summary>
        /// GIS map Draw Road
        /// </summary>
        /// <param name="path"></param>
        private void OpenRoadShapefile(string path)
        {

            // open the shapefile passing in the path, display name of the shapefile and
            // the field name to be used when rendering the shapes (we use an empty string
            // as the field name (3rd parameter) can not be null)
            EGIS.ShapeFileLib.ShapeFile sf = this.sfMap1.AddShapeFile(path, "ShapeFile", "");
            // Setup a dictionary collection of road types and colors
            // We will use this when creating a RoadTypeCustomRenderSettings class to setup which
            //colors should be used to render each type of road
            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            colors.Add("CP108-4-SP1", Color.DarkOrange);
            colors.Add("CP108-4-SP2", Color.BurlyWood);
            colors.Add("CP108-4-SP3", Color.Chocolate);
            RoadTypeCustomRenderSettings rs = new RoadTypeCustomRenderSettings(sf.RenderSettings, "ROADIDENTI", colors);
            sf.RenderSettings.CustomRenderSettings = rs;

            sf.RenderSettings.Font = new Font(this.Font.FontFamily, 12);
            sf.RenderSettings.UseToolTip = true;
            sf.RenderSettings.ToolTipFieldName = "name";
            sf.RenderSettings.MaxPixelPenWidth = 20;
        }

        /// <summary>
        /// GIS map Draw River
        /// </summary>
        /// <param name="path"></param>
        private void OpenReserveShapefile(string path)
        {

            // open the shapefile passing in the path, display name of the shapefile and
            // the field name to be used when rendering the shapes (we use an empty string
            // as the field name (3rd parameter) can not be null)
            EGIS.ShapeFileLib.ShapeFile sf = this.sfMap1.AddShapeFile(path, "ShapeFile", "");
            // Setup a dictionary collection of road types and colors
            // We will use this when creating a RoadTypeCustomRenderSettings class to setup which
            //colors should be used to render each type of road
            //sf.RenderSettings.FieldName = "ADMIN_NAME";
            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            colors.Add("1209301", Color.Salmon);
            RoadTypeCustomRenderSettings rs = new RoadTypeCustomRenderSettings(sf.RenderSettings, "BLOCKOID", colors);
            sf.RenderSettings.CustomRenderSettings = rs;

            sf.RenderSettings.Font = new Font(this.Font.FontFamily, 12);
            sf.RenderSettings.UseToolTip = true;
            sf.RenderSettings.ToolTipFieldName = "name";
            sf.RenderSettings.MaxPixelPenWidth = 20;
        }

        /// <summary>
        /// GIS map Draw harvest field
        /// </summary>
        /// <param name="path"></param>
        private void OpenHarvestShapefile(string path)
        {

            // open the shapefile passing in the path, display name of the shapefile and
            // the field name to be used when rendering the shapes (we use an empty string
            // as the field name (3rd parameter) can not be null)
            EGIS.ShapeFileLib.ShapeFile sf = this.sfMap1.AddShapeFile(path, "ShapeFile", "");
            // Setup a dictionary collection of road types and colors
            // We will use this when creating a RoadTypeCustomRenderSettings class to setup which
            //colors should be used to render each type of road
            //sf.RenderSettings.FieldName = "ADMIN_NAME";

            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            colors.Add("1209301", Color.LightGreen);
            RoadTypeCustomRenderSettings rs = new RoadTypeCustomRenderSettings(sf.RenderSettings, "BLOCKOID", colors);
            sf.RenderSettings.CustomRenderSettings = rs;

            sf.RenderSettings.Font = new Font(this.Font.FontFamily, 12);
            sf.RenderSettings.UseToolTip = true;
            sf.RenderSettings.ToolTipFieldName = "name";
            sf.RenderSettings.MaxPixelPenWidth = 20;
        }

        private void sfMap1_Load(object sender, EventArgs e)
        {

            try
            {

                gr = new GpsReader();
                //portOpen for GPS
                portOpen = gr.openSerialPort(GPSserialPort2);
                //
                // NOTE: the file must have permession set to all access
                //

                dataAccess = new DataAccess(Application.StartupPath + "GIS_Data\\SaveTheNoteWithPosition.xml");
                List<data> LoadData = new List<data>();
                LoadData = dataAccess.RetrieveData();
                foreach (data d in LoadData)
                {
                    CoordinateData data = new CoordinateData(d.longatude, d.latitude, d.note, d.IconName, false, d.ID);
                    flag.Add(data);
                }

                OpenReserveShapefile(Application.StartupPath + "\\GIS_Data\\108_reserves.shp");
                OpenHarvestShapefile(Application.StartupPath + "\\GIS_Data\\108_harvest_area.shp");
                OpenRoadShapefile(Application.StartupPath + "\\GIS_Data\\108_roads.shp");
                //Set ZoomLevel
                sfMap1.ZoomLevel *= 1;
                StartingZoomLevel = sfMap1.ZoomLevel;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void sfMap1_MouseClick(object sender, MouseEventArgs e)
        {
            FindCoordinate fc = new FindCoordinate(sfMap1.PixelCoordToGisPoint(e.Location).ToString());
            //Select a icon from map
            double currentZoomLevel = sfMap1.ZoomLevel;
            double AreaRadiusIntensity = currentZoomLevel / StartingZoomLevel;
            double AreaRadius = SelectingStaticRadious / AreaRadiusIntensity;

            foreach (var item in flag)
            {
                double distance = distanceMeasure(item.getLongatude(), item.getLatitude(),
                    fc.get_Longitude(), fc.get_Latitude());
                if (distance <= AreaRadius)
                {
                    //
                    // Show icon after click on icon
                    //
                    SelectPin selectPinChild = new SelectPin(this, item.getNote(), item.getLongatude(),
                        item.getLatitude(), item.getID());
                    selectPinChild.TopMost = true;
                    selectPinChild.Show();
                    //this.Enabled = false;
                }
            }

            sfMap1.Refresh();
            if (!string.IsNullOrEmpty(selectIcon))
            {
                marker2.Y = fc.get_Latitude();
                marker2.X = fc.get_Longitude();
                Pin child = new Pin(this);
                child.Show();
                this.Enabled = false;
                //selectIcon = null;
                sfMap1.Refresh();
            }
        }
        /// <summary>
        /// Zoom map view in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfMap1_ZoomIn(object sender, EventArgs e)
        {
            sfMap1.ZoomLevel *= 2;
        }
        /// <summary>
        /// Zoom map view out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfMap1_ZoomOut(object sender, EventArgs e)
        {
            sfMap1.ZoomLevel /= 2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfMap1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sfMap1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
        #endregion

        /// <summary>
        /// Full Screen Control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MxMinBtn_Click(object sender, EventArgs e)
        {
            if (!fullscreenState)
            {
                fullscreenState = true;
                int hwnd = FindWindow("Shell_TrayWnd", "");
                ShowWindow(hwnd, SW_SHOW);
                this.WindowState = FormWindowState.Maximized;
                panel1_resize_manual();
            }
            else
            {
                fullscreenState = false;
                //Used for Tool Bar show.
                int hwnd = FindWindow("Shell_TrayWnd", "");
                ShowWindow(hwnd, SW_SHOW);
                this.WindowState = FormWindowState.Normal;
                panel1_resize_manual();
            }
        }

        private void minimizeBtn_Click(object sender, EventArgs e)
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_SHOW);
            this.WindowState = FormWindowState.Minimized;
        }

        private void spAcceptBtn_Resize(object sender, EventArgs e)
        {


        }

        private void grpRamp_Enter(object sender, EventArgs e)
        {

        }

        private void grpTx_AutoSizeChanged(object sender, EventArgs e)
        {

        }

        private void grpTx_SizeChanged(object sender, EventArgs e)
        {

        }

        private void grpTx_Resize(object sender, EventArgs e)
        {

        }

        private void Example()
        {

        }

        /// <summary>
        /// Icon button for map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void icon2_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            selectIcon = btn.Name;
        }

        private void input_lvl_UpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyEventArgs.Equals(e.KeyCode, Keys.Enter))
            {
                if (input_lvl_UpDown.Value < input_lvl_UpDown.Maximum)
                {
                    input_lvl_UpDown.Value++;
                }
                else
                {
                    input_lvl_UpDown.Value = input_lvl_UpDown.Minimum;
                }
            }


        }

        private void suCommandComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyEventArgs.Equals(e.KeyCode, Keys.Enter))
            {
                if (suCommandComboBox.SelectedIndex < (suCommandComboBox.Items.Count - 1))
                {
                    suCommandComboBox.SelectedIndex++;
                }
                else
                {
                    suCommandComboBox.SelectedIndex = 0;
                }
            }

        }

        private void suCommandComboBox_Enter(object sender, EventArgs e)
        {
            suCommandComboBox.DroppedDown = true;
        }

        private void suCommandComboBox_DragLeave(object sender, EventArgs e)
        {

        }

        private void suCommandComboBox_Leave(object sender, EventArgs e)
        {
            suCommandComboBox.DroppedDown = false;
        }

        private void suCommandComboBox_KeyUp(object sender, KeyEventArgs e)
        {

            suCommandComboBox.DroppedDown = true;
        }

        private void cbBuckSpecies_Enter(object sender, EventArgs e)
        {
            cbBuckSpecies.DroppedDown = true;
        }

        private void cbBuckSpecies_Leave(object sender, EventArgs e)
        {
            cbBuckSpecies.DroppedDown = false;
        }

        private void cbBuckSpecies_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyEventArgs.Equals(e.KeyCode, Keys.Enter))
            {
                if (cbBuckSpecies.SelectedIndex < (cbBuckSpecies.Items.Count - 1))
                {
                    cbBuckSpecies.SelectedIndex++;
                }
                else
                {
                    cbBuckSpecies.SelectedIndex = 0;
                }
            }
        }

        private void cbBuckSpecies_KeyUp(object sender, KeyEventArgs e)
        {
            cbBuckSpecies.DroppedDown = true;
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void nudMinLenStore_Leave(object sender, EventArgs e)
        {
            MinLengthToStore = nudMinLenStore.Value;
        }

    }

}