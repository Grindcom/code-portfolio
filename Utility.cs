using System;
using System.Data;
using System.Text;

namespace Utility
{
    // Tri-State values
    public enum TriVal
    {
        triHI, triLO, triNu
    }

    // BushApe Machine Functions
    public enum MachineOps
    {
        Direction, TopKnife, WheelArm, ButtKnife, Tilt, MainSaw, TopSaw, Rotate
    }
    //===============================================================================
    /// <summary>
    /// 'InfoType' & 'Info'
    /// An enum and struct used for communication information.
    /// </summary>
    public enum InfoType { GPIO = 1, COMM, TIMEOUT, ERROR }
    public class Info: Object
    {
        //===============================================================================
        private InfoType m_from = InfoType.GPIO;
        /// <summary>
        /// 'from' indicates which thread the information type is from
        /// </summary>
        public InfoType from
        {// When the information is set it is also indicated to
            // have changed
            set
            {
                m_from = value;
                m_changed = m_from_changed = true;
            }
            get { return m_from; }
        }

        private bool m_from_changed = false;
        /// <summary>
        /// 'FromStatus' indicates true if the 'from' property has changed since
        /// the last read of this property
        /// </summary>
        public bool FromStatus
        {// If the information has changed it is considered changed only until
            // the Changed property has been read One time.
            get
            {
                bool temp = m_from_changed;
                m_from_changed = m_changed = false;
                return temp;
            }
        }
        //===============================================================================
        private string m_cmsent = "none";
        /// <summary>
        /// String value from cabin module, recieved with index/command.
        /// A change here is not reflected in the command status variable.
        /// </summary>
        public string CMsent
        {
            set
            {
                m_cmsent = value;
            }
            get { return m_cmsent; }

        }
        //===============================================================================
        /// <summary>
        /// 
        /// </summary>
        private uint m_command = 0;
        /// <summary>
        /// command combines the values of commandA and commandB
        /// with commandA as the high word or top 2 bytes.  It
        /// is automatically updated when A or B is set and 
        /// vice-versa.
        /// </summary>
        public uint command
        {
            set
            {
                uint temp = value;
                m_command = temp;
                m_command_changed = m_changed = true;
                // set indavidual commands
                commandB = temp & 0xFFFF;
                temp >>= 16;
                commandA = temp;
            }
            get { return m_command; }
        }
        private bool m_command_changed = false;
        /// <summary>
        /// Indicates if either or both command sets have changed 
        /// since the last read.
        /// </summary>
        public bool CommandStatus
        {
            get
            {
                bool temp = m_command_changed;
                m_command_changed = m_changed = false;
                return temp;
            }
        }
        ///<summary>
        ///True if command is changed.  Does not reset the status.
        ///</summary>
        public bool CommandStatusPeek
        {
            get { return m_changed; }
        }
        //===============================================================================
        private uint m_commandA = 0;
        public uint commandA
        {
            set
            {
                uint temp = value;
                m_commandA = value;
                m_changed = m_commandA_changed = true;
                temp <<= 16;
                // clear top two bytes
                m_command &= 0xFFFF;
                // add temp to the upper bytes
                m_command |= temp;
                m_command_changed = true;
            }
            get { return m_commandA; }
        }
        private bool m_commandA_changed = false;
        /// <summary>
        /// 'CommandAStatus' indicates true if command has changed since the last
        /// read of this property
        /// </summary>
        public bool CommandAStatus
        {// If the information has changed it is considered changed only until
            // the Changed property has been read One time.
            get
            {
                bool temp = m_commandA_changed;
                m_commandA_changed = m_changed = false;
                return temp;
            }
        }
        //===============================================================================
        private uint m_commandB = 0;
        public uint commandB
        {
            set
            {
                uint temp = value;
                m_commandB = value;
                m_commandB_changed = m_changed = true;
                // clear lower two bytes
                m_command &= 0xFFFF0000;
                // add temp to the lower bytes
                m_command |= temp;
                m_command_changed = true;
            }
            get { return m_commandB; }
        }
        private bool m_commandB_changed = false;
        /// <summary>
        /// 'CommandBStatus' indicates true if the command has changed since the last
        /// read of this property
        /// </summary>
        public bool CommandBStatus
        {// If the information has changed it is considered changed only until
            // the Changed property has been read One time.
            get
            {
                bool temp = m_commandB_changed;
                m_commandB_changed = m_changed = false;
                return temp;
            }
        }
        //===============================================================================
        private String m_rx_tx = "un-initialized";
        /// <summary>
        /// String sent to or recieved from head.
        /// </summary>
        public String rx_tx
        {
            set
            {
                m_rx_tx = value;
                m_rx_tx_changed = m_changed = true;
            }
            get { return m_rx_tx; }
        }
        private bool m_rx_tx_changed = false;
        /// <summary>
        /// 'Rx_tx_Status' indicates true if any of the information has changed since
        /// the last read of this property.
        /// </summary>
        public bool Rx_tx_Status
        {// If the information has changed it is considered changed only until
            // the Changed property has been read One time.
            get
            {
                bool temp = m_rx_tx_changed;
                m_rx_tx_changed = m_changed = false;
                return temp;
            }
        }
        //===============================================================================
        // m_changed is only set to true by changes to the other information properties
        private bool m_changed = false;
        /// <summary>
        /// 'Changed' indicates true if any of the information has changed since
        /// the last read of this property.  This property is also set to false if
        /// any of the individual Status properties are read.
        /// </summary>
        public bool Changed
        {// If the information has changed it is considered changed only until
            // the Changed property has been read One time.
            get
            {
                bool temp = m_changed;
                m_changed = false;
                return temp;
            }
        }
        //===============================================================================
        /// <summary>
        /// if set to true, this clears all the status properties to indicate
        /// no change.
        /// </summary>
        public bool ResetStatus
        {
            set
            {
                if (value)
                {
                    m_changed = m_rx_tx_changed = m_commandB_changed =
                        m_commandA_changed = m_command_changed = m_from_changed = false;
                }
            }
        }
        public Info()
        {
        }
        //===============================================================================
        // overload operator !=
        public static bool operator !=(Info a, Info b)
        {
            if (a.commandA != b.commandA)
                return true;
            if (a.commandB != b.commandB)
                return true;
            if (a.rx_tx != null && b.rx_tx != null)
                if (!a.rx_tx.EndsWith(b.rx_tx))
                    return true;
            if (a.from != b.from)
                return true;
            return false;
        }

        //===============================================================================
        // overload operator ==
        public static bool operator ==(Info a, Info b)
        {
            if ((a.commandA == b.commandA) && (a.commandB == b.commandB)
                && (a.rx_tx.EndsWith(b.rx_tx)) && (a.from == b.from))
                return true;
            return false;
        }
        //================================================================================
        // The 'Must override's'
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return base.ToString();
        }
        //===============================================================================
    }
    //================== End of Info Class ==============================================
    /// <summary>
    /// 'StemInfo'
    /// A struct that will contain the profile information of a stem.
    /// </summary>
    public struct StemInfo
    {
        public double length;
        public int dia1;
        public int dia2;

    }
    /// <summary>
    /// 'OperationInfo'
    /// A Struct that will contain all possible functions and sensors,
    /// for processing operations
    /// </summary>
    public struct OperationInfo
    {
        // functions
        public bool topKop;
        public bool topKcl;
        public bool botKop;
        public bool botKcl;
        public bool wheelOp;
        public bool wheelCl;
        public bool forward;
        public bool revearse;
        public bool tiltUp;
        public bool tiltDn;
        public bool rotL;
        public bool rorR;
        // sensors
        public bool topSaw;
        public bool mainSaw;
        public bool photoEye;
    }
    /// <summary>
    /// 'CommandPrep'
    /// A Class of functions that will prepare communication strings.
    /// </summary>
    public class CommandPrep
    {
        public CommandPrep()
        {
        }
        /// <summary>
        /// Add the secondary function to '$cmd,___'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string type2command(string c)
        {
            c = c.TrimEnd(null);
            return "wt41 $cmd," + c + ",";
        }
        /// <summary>
        /// Top level command i.e. '$___,'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string type1command(string c)
        {
            c = c.TrimEnd(null);
            return "wt41 $" + c + ",";
        }
        /// <summary>
        /// Adds the setting and pressure to a '$set,___,___' command.
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="pressure"></param>
        /// <returns></returns>
        public string setCommand(string setting, int pressure)
        {
            setting = setting.TrimEnd(null);
            return "wt41 $set " + setting + " " + pressure.ToString();// +",";
        }
        public string setCommand(string setting, string pressure)
        {
            setting = setting.TrimEnd(null);
            return "wt41 $set," + setting + "," + pressure + ",";
        }
    }

}
