using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices; // DllImport
using Utility;
using YaoDurant.Allocator;
using System.Text;

namespace BushApe
{
	/// <summary>
	/// Overloaded BushApe Event Handler
	/// </summary>
	public delegate void BushApeEventHandler(object sender, BushApeEventArgs fe);


	/// <summary>
	/// Overloaded event argument class
	/// </summary>
	public class BushApeEventArgs: EventArgs
	{
		// event information
		String device;

		/// <summary>
		/// BushApeEventArgs Constructor
		/// </summary>
		/// <param name="device"></param>
		public BushApeEventArgs(String device)
		{
			this.device = device;
		}


	}
	/// <summary>
	/// SerialComm Controls the serial port
	/// </summary>
	public class SerialComm{
		private const int m_BUFFERSIZE = 512;
		private const int m_COMM1 = 0;
		private const int m_COMM2 = 1;
		private bool m_comm1open = false;
		private bool m_comm2open = false;



		private int m_portNumber = m_COMM1;
		private int m_newbaudRate;
		private int m_baudRate = 4; // 0 => 9600, 1 => 19200 etc.
		private int m_dataBits = 8;
		private int m_parity = 2;
		private int m_stopBits = 1;
		private int m_flow = 0; // not implemented
		// -------------------------------------------
		/// <summary>
		/// Get and Set functions
		/// </summary>
		public int BUFFERSIZE
		{
			get{ return m_BUFFERSIZE; }
		}
		public int COMM1
		{
			get{ return m_COMM1; }
		}
		public int COMM2
		{
			get{ return m_COMM2; }
		}

		public bool comm1open
		{
			get{ return m_comm1open; }
		}
		public bool comm2open
		{
			get{ return m_comm2open; }
		}
		public int portNumber
		{
			get{ return m_portNumber; }
			set{ m_portNumber = value; }
		}
		public int baudRate
		{
			get{ return m_baudRate; } 
			set{ m_baudRate = value; }
		}
		public int dataBits
		{
			get{ return m_dataBits; }
			set{ m_dataBits = value; }
		}
		public int parity
		{
			get{ return m_parity; }
			set{ m_parity = value; }
		}
		public int stopBits 
		{
			get{ return m_stopBits; }
			set{ m_stopBits = value; }
		}
		public int flow
		{
			get{ return m_flow; }
			set{ m_flow = value; }
		}
		/// <summary>
		/// Constructor
		/// </summary>
		public SerialComm()
		{
			
			m_comm1open = RLC_OpenCommPort(portNumber);
			if(!comm1open)
				MessageBox.Show("Port not Opened");			
			if(!RLC_SetCommPortSettings(portNumber,baudRate,dataBits,parity,stopBits,flow))
			{
				MessageBox.Show("Settings Failure");
			}
		
			if(!RLC_GetCommPortSettings(portNumber,ref m_newbaudRate,ref m_dataBits,ref m_parity,ref m_stopBits,ref m_flow))
				MessageBox.Show("No Settings: " + portNumber);
//			else 
//				MessageBox.Show("Comm: " + portNumber + ", Baud Rate: " + newbaudRate);
		}
		/// <summary>
		/// Check to see if port is opened
		/// </summary>
		public bool commOpen(int port)
		{
			switch(port)
			{
				case 1:
					if(comm1open)
						return true;
					break;
				case 2:
					if(comm2open)
						return true;
					break;
			}
			return false;
		}
		/// <summary>
		/// The following imports allow access to the RLC-ARM
		/// Serial ports
		/// </summary>
        //private string RLCDLL ="\\IPSM\\GrindcomDrivers\\RLCSerial.dll";//\\
		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll", EntryPoint="RLC_OpenCommPort")]
		public static extern bool RLC_OpenCommPort(int portNumber);

		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll", EntryPoint="RLC_WriteToCommPort")]
		public static extern bool RLC_WriteToCommPort(int portNumber, String Data);

		public bool Send(string message)
		{
			return RLC_WriteToCommPort(this.portNumber, message);
		}

		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll", EntryPoint="RLC_SetCommPortSettings")]
		public static extern bool RLC_SetCommPortSettings(int portNumber, int baudRate,
															int dataBits, int parity,
															int stopBits, int flow);
//        , EntryPoint="RLC_GetCommPortSettings"
		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll")]
		public static extern bool RLC_GetCommPortSettings(int portNumber, ref int baudRate,
															ref int dataBits, ref int parity,
															ref int stopBits, ref int flow);

		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll", EntryPoint="RLC_CloseCommPort")]
		public static extern bool RLC_CloseCommPort(int portNumber);

		[DllImport("\\IPSM\\GrindcomDrivers\\RLCSerial.dll", EntryPoint="RLC_GetData")]
		private static extern bool RLC_GetData(int portNumber, IntPtr ptr);

		public static String getData(int portNumber)
		{
			String retData = "This is from 'getData', but it Did Not Work!!!";
			// Create character array 
//			char[] charray = new char[m_BUFFERSIZE];
			// Allocate unmanaged buffer
			IntPtr data = MarshalAssist.AllocHGlobal(m_BUFFERSIZE * 
							Marshal.SystemDefaultCharSize);
			try
			{
				if(RLC_GetData(portNumber,data))
				{
					// for debugging
					//				MessageBox.Show("Before get");
				
					// Convert to mannaged memory
					retData = Marshal.PtrToStringUni(data);

					// Release memory
					MarshalAssist.FreeHGlobal(data);

					return retData;
					// for debugging
					//				MessageBox.Show("After get");				
				}
				// Release un-mannaged memory
				MarshalAssist.FreeHGlobal(data);
				return null;
			}
			catch(Exception e)
			{
                //MessageBox.Show("Caught in SerialComm::getData, Reason: " + e.Message);
                Error_Stamp("Caught in SerialComm::getData, Reason: " + e.Message);
				return null;
			}
		}

        private static void Error_Stamp(String error)
        {
            //=============================================
            string errorFile = @"IPSM\error_SerialComm.txt";
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
	/// <summary>
	/// SerialCommService allows access to serial communication ports, 
	/// through a thread.
	/// </summary>
	public class SerialCommService {
		private Control caller;

		private EventHandler deleCallback;
		private Queue queueArgs;
		private Info iData = new Info();
		private string polWord;
		private int port;
		private long pol_interval;
		private Thread serialThrd;
		private bool	TerminateRequestRxd = false;

		/// <summary>
		/// SerialComm is meant to run in a multi-threaded environment
		/// Therefore it requires information on who called it, how to call back,
		/// where to put information, and where to get infromation from the caller
		/// </summary>
		/// <param name="p">Port</param>
		/// <param name="queueArgs">Queue to store serial data </param>
		/// <param name="deleCallback">Call back function</param>
		/// <param name="control">Caller of Serial service</param>
		/// <param name="pol">String to poll serial port</param>
		/// <param name="pol_time">Time to wait before polling with 'pol'</param>
		public SerialCommService(Control control, EventHandler deleCallback, Queue queueArgs, int p, string pol, long pol_time)
		{
			
			// Save input params
			this.caller = control;
			this.deleCallback = deleCallback;
			this.queueArgs = queueArgs;
			this.port = p;
			this.iData.from = InfoType.COMM;
			this.polWord = pol;
			this.pol_interval = pol_time;

            try
            {
                // Create & start Thread
                serialThrd = new Thread(new ThreadStart(ThreadProc));
                serialThrd.Start();
                serialThrd.Priority = ThreadPriority.Normal;//.BelowNormal;

            }
            catch (ThreadStateException e)
            {
                //MessageBox.Show("Thread Exception Caught in SerialCommService Constructor: " + e.Message);
                Error_Stamp("Thread Exception Caught in SerialCommService Constructor: " + e.Message);
            }
            catch (ThreadAbortException e)
            {
                Error_Stamp("Thread Abort Exception in SerialComService");

            }
            finally
            {
                //Error_Stamp("Serial Comm Service");
                if (GC.GetTotalMemory(false) > 750)
                    GC.Collect();
            }
		}

		/// <summary>
		/// Control serial port operations
		/// </summary>
		private void ThreadProc()
		{
			// Do safety stuff
			// Send 'outData'
			// Call Control
			int index, length, i = 0;

            try
            {

                do
                {
                    // Get new data.  iData is new every time this thread begins.
                    iData.rx_tx = SerialComm.getData(this.port);
                    length = iData.rx_tx.Length;

                    if (iData.rx_tx.Length > 0)
                    {
                        // The Start or header character is '$', so this if statement
                        //		causes the while loop to continue until the header
                        //		is presant in the communication string
                        index = iData.rx_tx.IndexOf("$");
                        if (index == -1)
                            continue;
                        else if (index > 0)
                        {
                            iData.rx_tx = iData.rx_tx.Substring(index);
                        }
                        // The End or footer character set is \r\n, so this 
                        //		if statement checks for the '\n' character and breaks out
                        //		of the while if it finds one occurance of it.
                        index = iData.rx_tx.IndexOf("\n");
                        if (index > 0)
                        {
                            iData.from = InfoType.COMM;
                            lock (this.queueArgs.SyncRoot)
                            {

                                this.queueArgs.Enqueue(iData);
                                //this.queueArgs.TrimToSize();
                            }
                            // Invoke control thread
                            this.caller.Invoke(this.deleCallback);
                            i = 0;

                            //							break
                        }
                    }
                    //if( i++ == pol_interval )
                    //{
                    //    SerialComm.RLC_WriteToCommPort(this.port, polWord);
                    //    i = 0;
                    //}
                    Thread.Sleep(1);
                } while (!TerminateRequestRxd);



                //				if( ! TerminateRequestRxd )
                //				{
                //
                //
                //				}
                // From .NET Compact Framework Programming with C#
                //		"The .NET Compact Framework has no Thread.Stop, or
                //			comparable, method.  We stop by reaching the end
                //			of the ThreadProc ASAP." (Yao 714)

            }
            catch (ThreadStateException e)
            {
                //MessageBox.Show("Caught Thread Exception in SerialComm: " + e.Message);
                Error_Stamp("Caught Thread Exception in SerialComm: " + e.Message);
            }
            catch (ThreadAbortException e)
            {
                Error_Stamp("Thread Abort Exception in SerialComm: ThreadProc");

            }
            catch (ObjectDisposedException e)
            {
                //MessageBox.Show("Caught Disposal Exception in SerialComm: " + e.Message);
                Error_Stamp("Caught Disposal Exception in SerialComm: " + e.Message);
                GC.Collect();
                // 01.24.2008; caught after each shutdown!!!
            }
            catch (Exception e)
            {
                //MessageBox.Show("Caught in SerialComm ThreadProc: " + e.Message);
                Error_Stamp("Caught in SerialComm ThreadProc: " + e.Message);
                iData.from = InfoType.ERROR;
            }
            finally
            {
                //Error_Stamp("Thread Proc");
                if (GC.GetTotalMemory(false) > 750)
                    GC.Collect();
            }

		}

		/// <summary>
		/// Stop thread activity
		/// </summary>
		public void Stop()
		{
			TerminateRequestRxd = true;
		}

        private static void Error_Stamp(String error)
        {
            //=============================================
            string errorFile = @"IPSM\error_SerialComm_Service.txt";
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

	}//end SerialCommService
}

