using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using Utility;

namespace SilverbackXP
{
    public class GPIO
    {
        private System.IO.Ports.SerialPort serialPort;
        public GPIO(ref System.IO.Ports.SerialPort sp)
        {
            serialPort = sp;
        }

        ~GPIO()
        {

        }

        public virtual void Dispose()
        {

        }
        public const string GPIOHDR = "#";
        public const string PALL_OFF = "Alloff,";
        public const string PALL_On = "Allon,";
        public const string P1ON = "P1on,";
        public const string P1OFF = "P1off,";
        public const string P2ON = "P2on,";
        public const string P2OFF = "P2off,";


        public enum PumpOps
        {
            ALLOFF = 0x00, P1ON = 0x01, P2ON = 0x02, ALLON = 0x03
        }

        //public void writeCB(ref SerialPort sp, PumpOps po)
        //{
        //    sp.WriteLine("#pump," + po);
        //}

    }
    class GPIOService
    {
        private Info lastData;

        //private bool t_State;
        //private bool p_OneTimeInput = false;

        //private uint rawCom1 = 0;
        //private uint rawCom2 = 0;
        //============================================================
        //private static Info m_iCommand = new Info();
        private Info m_iCommand;
        /// <summary>
        /// Current command information
        /// </summary>
        public Info iCommand
        {
            get { return m_iCommand; }
        }
        //============================================================



        // The caller can terminate the thread
        private bool m_TerminateRequestReceived = false;


        public bool TerminateRequest
        {
            set { m_TerminateRequestReceived = value; }
        }


        ~GPIOService()
        {

        }

        public virtual void Dispose()
        {
            TerminateRequest = true;
        }
        public GPIOService(Utility.Info lstData)
        {
            m_iCommand = new Info();
            // Save parameter info
            this.lastData = lstData;

            m_iCommand = lstData;
            m_iCommand.from = InfoType.GPIO;
            // Create and Start thread...

        }

        /// <summary>
        /// Remove the GPIO information from a related message.
        /// </summary>
        /// <param name="GPIOstrn">Message to parse; including '#' header</param>
        /// <returns>True if message parses</returns>
        public bool GPIOParse(string GPIOstrn)
        {
            try
            {
                // Parse the two uint values from the GPIO string
                string[] tempData = GPIOstrn.Split(new char[] { ',' });
                uint temp = 0;
                //if (!tempData[0].StartsWith("#"))
                //{// if the GPIOstring does not start with '#' do nothing
                //    return false;
                //}
                // read	rxData for any new information
                // data is read from the GPIO as a 'raw' hex number. 
                //		The word is read as a two 16 bit value
                if (tempData[1] != null && tempData[1].Length > 12)
                {
                    //
                    // Compress the string to a uint
                    //
                    foreach (char bit in tempData[1])
                    {
                        if (bit == '1')
                        {/*if the bit is high, add 1 to temp*/
                            temp ^= 0x01;
                            continue;
                        }
                        temp <<= 1;/*shift temp bit left one*/
                    }
                    //
                    // Identify which ports the input is from
                    //
                    if (tempData[0].StartsWith("ej"))
                    {
                        m_iCommand.commandA = temp;/*put the GPIO data in command*/
                    }
                    if (tempData[0].StartsWith("abcd"))
                    {
                        m_iCommand.commandB = temp;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Caught in GPIO ThreadProc: " + e.Message);
                Error_Stamp("Caught in GPIO: " + e.Message + " Parse GPIO");
                return false;
            }
        }
        private static void Error_Stamp(String error)
        {
            //=============================================
            string errorFile = @"DataLog\\error_RLC.txt";
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
    }
}
