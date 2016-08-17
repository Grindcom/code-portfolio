using System;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.Sql;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Xml;


namespace SilverbackXP
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SilverbackDB
    {
        public enum MachineInfo
        {
            MainSawEnc = 0, PhotoEye, tSawPresent, tSawEnc, PWM, DiaEnc,
            TiltFloat, MultiFunc, StandardMeas, semiAuto, AutoFeed, oneSawButton,
            ProtectLog, AudibleInZone, UseRamps, FindButt
        }
        public enum MachineSettings
        {
            Slow_Speed = 1, Botk_Press, TopK_Press, WheelA_Press, Mid_Press, RmpUp_Distance, RmpDown_Distance, RmpStart_Speed
        }
        public enum RowUpDown
        { Up = 1, Down }
        /// <summary>
        /// Data base file
        /// </summary>
        private static string dbFilename = @"SBDB.xml";//Changed: 01.21.2010
        /// <summary>
        /// Piececount file
        /// </summary>
        private string countFile = @"DataLog\\piece_count.txt";
        private StreamWriter swcountFile;
        //

        //private static string cmdsetFilename = @"CMDSET.xml";/* Command and Settings data file */
        //
        // Current users directory path, less their silverback path
        //
        private static string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string userName = Environment.UserName;

        //------------------------------------------------------------
        /// <summary>
        /// Connection and Command
        /// </summary>
        //private static SqlConnection connDB = new SqlConnection();
        //private static SqlCommand cmndDB = new SqlCommand();
        //------------------------------------------------------------
        /// <summary>
        /// Data set(s), adapter, table, Comand Builder(s)
        /// </summary>
        private XmlDataDocument xmlDBdoc;
        private TypedDataSet dsetDB;
        //========= Bucking Spec related ==============================
        private int pSelectedOperator = 0;
        private int pSelectedSpecies = 0;
        private int pSelectedBuck = 0;
        private decimal pSelectedLength = 0.0m;
        private decimal pSelectedMinDi = 0.0m;
        private decimal pSelectedMaxDi = 0.0m;
        private string pSelctedSpeciesName;
        private decimal pOverLenWindow = 0.0m;
        private decimal pUnderLenWindow = 0.0m;
        //-------------------------------------------------------------
        /// <summary>
        /// Data base flag
        /// </summary>
        private static bool exists = false;
        //private static bool useDefault = false; /* use default set up */
        /// <summary>
        /// Database Destructor
        /// </summary>
        ~SilverbackDB()
        {
            try
            {
                //swcountFile.Dispose();
            }
            catch (Exception destrExc)
            {
                MessageBox.Show("Database Destructor: " + destrExc.Message, "ERROR Closing");
            }
        }
        /// <summary>
        /// Data base constructor
        /// </summary>
        public SilverbackDB()
        {

            try
            {
                //
                // Create the data set
                //
                dsetDB = new TypedDataSet();
                xmlDBdoc = new XmlDataDocument(dsetDB);
                //Remove engine refs: 01.21.2010
                // Check if Bush Ape Data base is present, Create if not
                if (!File.Exists(dbFilename))
                {
                    MessageBox.Show("No data base file!!!\r\nRebuilding with default values.", "SBDB: Constructor");
                    // Fill data base with default values
                    FillDefaultDB();
                    // Create the schema file with default data
                    dsetDB.WriteXml(dbFilename, XmlWriteMode.WriteSchema);
                }
                else
                {// If the document exists, load it.
                    xmlDBdoc.Load(dbFilename);
                    //*****************
                    // Create Schema file
                    //
                    dsetDB.WriteXmlSchema(@"SXPDB_Schema.xsd");
                    dsetDB.WriteXml(@"SXPDB_Data_Schema.xsd", XmlWriteMode.WriteSchema);
                }
                //
                // Open the piece count file
                //
                if (!File.Exists(countFile))
                {
                    File.Create(countFile).Close();
                }
                // File IO-----------------------------------

                swcountFile = new StreamWriter(new FileStream(countFile, FileMode.Append), System.Text.Encoding.ASCII);
                swcountFile.NewLine = "\r\n";
                //*********************************************************************
                //
                // Get user information
                //
                int id = GetUserID(userName);
                if (id < 0)
                {
                    id = AddOperator("", "", userName);
                }
                //*********************
                // Make sure it got added properly
                //
                if (id < 0)
                {
                    exists = false;
                    return;
                }
                //**************************
                // Now the user exists
                //
                exists = true;
                //**************************
                // Set the current user
                //
                this.SelectedOperator = id;
                //*************************
                // Update stem counter
                // 
                this.UpdateStemCount();
                //***********************
                // Update piece counter
                //

            }
            catch (System.Security.SecurityException se)
            {
                MessageBox.Show(se.Message, "SB: Security Construct");
            }
            catch (Exception e)
            {
                string message = "Unknown error in Data Base, " +
                    " Reason: " +
                    e;
                MessageBox.Show(message, "SB: Constructor");
                Error_Stamp(message);
            }
        }
       
       
/// <summary>
/// Change the assignment of a bucking spec to a different input
/// </summary>
/// <param name="input">Input value</param>
/// <param name="cid">Command id</param>
/// <param name="uid">User id</param>
/// <param name="bsid">Bucking spec id</param>
        //public void ChangeBuckingSpec_Select(uint input, int cid, int uid, int bsid)
        //{
        //    try
        //    {
        //        //
        //        // Get the appropriate Cammand row to change
        //        //
        //        TypedDataSet.CommandRow row = (TypedDataSet.CommandRow)dsetDB.Command.Rows.Find(cid);
        //        if (row.IsNull())
        //            return;
        //        //
        //        // Change the Command, which in this case is a reference to the bucking spec id.
        //        //
        //        row.BeginEdit();
        //        row.Command = bsid.ToString();
        //        row.EndEdit();
        //        //
        //        // Accept the changes to the data base
        //        dsetDB.Command.AcceptChanges();
        //        //
        //        // Call for the rest of the changes
        //        //
        //        ChangeUser_Command(input, cid, uid);

        //    }
        //    catch (Exception exbs)
        //    {
        //        MessageBox.Show(exbs.Message, "Change Bucking Spec select");
        //    }
        //}
        /// <summary>
        /// Changes an inputs binding
        /// </summary>
        /// <param name="input">New input to bind</param>
        /// <param name="cid">Command table row ID</param>
        /// <param name="uid">User ID</param>
        public void ChangeUser_Command(uint input, int cid, int uid, int bsid)
        {
            try
            {
                ClearUser_Command = true;
                // Locate the ID for the selected input
                
                TypedDataSet.User_CommandRow[] rows = (TypedDataSet.User_CommandRow[])dsetDB.User_Command.Select("Input = " + input, "I_ID ASC", DataViewRowState.CurrentRows);
                foreach (TypedDataSet.User_CommandRow row in rows)
                {
                    if (row.Input == input)
                    {
                        int a = row.I_ID;//["I_ID"];
                        // Use input index to change the correct User_Command row dsetDB,"User_Command"
                        dsetDB.User_Command.Rows[a].BeginEdit();
                        dsetDB.User_Command.Rows[a]["Input"] = input;
                        dsetDB.User_Command.Rows[a]["C_ID"] = cid;
                        dsetDB.User_Command.Rows[a]["U_ID"] = uid;
                        dsetDB.User_Command.Rows[a]["BS_ID"] = bsid;
                        dsetDB.User_Command.Rows[a].EndEdit();

                        // Enter the changes to the data set
                        dsetDB.User_Command.AcceptChanges();
                        // Save the changes to data file
                        //xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
                        dsetDB.WriteXml(dbFilename, XmlWriteMode.WriteSchema);

                    }
                }
            }
            catch (SqlException exSQL)
            {
                string message = "Unable to change command, " +
                    " Reason: " +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Determine whether an input is already assigned.
        /// </summary>
        /// <param name="OpInput">Input to compare</param>
        /// <returns>True if OpInput is in table</returns>
        public bool IsAssigned(uint OpInput)
        {
            try
            {
                // Locate the ID for the selected input
                TypedDataSet.User_CommandRow[] UCrows = (TypedDataSet.User_CommandRow[])dsetDB.User_Command.Select();
                foreach (TypedDataSet.User_CommandRow row in UCrows)
                {
                    if (row.Input == OpInput)
                        return true;
                }
                return false;
            }
            catch (SqlException exSQL)
            {
                string message = "Unable to determine if input is assigned, " +
                    " Reason: " +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                return false;
            }
        }
        /// <summary>
        /// Show a message box with the contents of the Bucking Spec table;
        /// Filtered by species.
        /// </summary>
        /// <param name="species">Species of tree to filter by; null is no filter</param>
        /// <param name="hdr">String in header of message box</param>
        public void CheckBuckTable(int species, string hdr)
        {
            DataRow[] drows;
            if (species >= 0)
                drows = dsetDB.BuckingSpec.Select(" Species = " + species, "BSpec_ID DESC");
            else
                drows = dsetDB.BuckingSpec.Select();
            string Stable = "";
            foreach (DataRow drow in drows)
            {
                foreach (DataColumn col in dsetDB.BuckingSpec.Columns)
                {
                    Stable += drow[col].ToString() + ", ";
                }
                Stable += "\r\n";
            }
            MessageBox.Show(Stable, "Data table: " + hdr);
        }            
        /// <summary>
        /// Add a new bucking record to bucking spec table.
        /// New record becomes the current spec.
        /// </summary>
        /// <param name="species"></param>
        /// <param name="len"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="note"></param>
        public void AddBuckingSpec(int species, int grade, decimal len, decimal min, decimal max, string note, decimal over, decimal under)
        {
            // get number of last row
            //int lastRow = dtabBuckingSpec.Rows.Count;
            //------
            //dsetDB.BuckingSpec.DefaultView.RowFilter = "Species = " + species;
            DataRow[] drows = dsetDB.BuckingSpec.Select(" Species = " + species, "BSpec_ID DESC");
            string Stable = "";
            foreach (DataRow drow in drows)
            {
                foreach (DataColumn col in dsetDB.BuckingSpec.Columns)
                {
                    Stable += drow[col].ToString()+", ";
                }
                Stable += "\r\n";
            }
            //
            // Get a new bucking spec row
            //
            TypedDataSet.BuckingSpecRow row = dsetDB.BuckingSpec.NewBuckingSpecRow();
            //
            // Add the new information to the row
            //
            row.BeginEdit();
            row.Species = species;
            row.BSpec_Grade = grade;// 
            row.Len = len;
            row.Min_Di = min;
            row.Max_Di = max;
            row.Note = note;
            row.Over_Window = over;
            row.Under_Window = under;
            row.EndEdit();
            //
            // Add the new row the the bucking spec table
            //
            dsetDB.BuckingSpec.AddBuckingSpecRow(row);
            //
            // Enter the changes to the data set
            //
            dsetDB.BuckingSpec.AcceptChanges();
            //
            // Save the changes to data file
            //
            xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
            //
            //RefreshBucking();// Possibly not necessary
            // new Row becomes current spec
            // Sort descending by spec ID will give the most recent row added.
            //DataRow[] rows = dsetDB.BuckingSpec.Select(" Species = " + species, "BSpec_ID DESC");

            dsetDB.BuckingSpec.DefaultView.Sort = "BSpec_ID DESC";

            if (dsetDB.BuckingSpec.DefaultView.Count <= 0)
                MessageBox.Show("Spec not added");

        }
        /// <summary>
        /// Add a new user command to the User_Command table
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cid"></param>
        /// <param name="uid"></param>
        /// <param name="bsid">The bucking spec id related to this command; -1 if none</param>
        public void AddUser_Command(uint input, int cid, int uid, int bsid)
        {
            try
            {
                //DataTableReader ucRdr = dsetDB.User_Command.CreateDataReader();
                //while (ucRdr.NextResult())
                //{/*Find the last row*/
                //}
                TypedDataSet.User_CommandRow row = dsetDB.User_Command.NewUser_CommandRow();
                
                row.BeginEdit();
                row.Input = input;
                row.C_ID = cid;
                row.U_ID = uid;
                row.BS_ID = bsid;
                row.EndEdit();

                //dsetDB.User_Command.BeginLoadData();
                dsetDB.User_Command.Rows.Add(row);
                //dsetDB.User_Command.EndLoadData();
                // Enter the changes to the data set
                dsetDB.User_Command.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);////WriteTableXMLfile(dbFilename);
            }
            catch (Exception ex)
            {
                string message = "Unable to add a user command, " +
                    " Reason: " +
                    ex.Message;
                MessageBox.Show(message,"Add User Command");
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Add an operator profile
        /// </summary>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="username"></param>
        /// <returns>The new operator id; -1 if not successful</returns>
        public int AddOperator(string firstname, string lastname, string username)
        {
            try
            {
                int newId;
                TypedDataSet.OperatorRow row = (TypedDataSet.OperatorRow)dsetDB.Operator.NewRow();
                
                //row.BeginEdit();
                ////row.User_ID++;
                //row.First = firstname;
                //row.Last = lastname;
                //row.User_Name = username;
                //row.EndEdit();

                //dsetDB.User_Command.BeginLoadData();
                dsetDB.Operator.AddOperatorRow(firstname, lastname, 1, "na", username);
                //dsetDB.User_Command.EndLoadData();
                // Enter the changes to the data set
                //dsetDB.Operator.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);////WriteTableXMLfile(dbFilename);
                //
                // Get the new user id
                //
                newId = GetUserID(username);
                if (newId <= 0)
                    return newId;/* Operation failed */
                //
                // Make default user_command table, based on Admin/default commands
                //
                TypedDataSet.User_CommandRow[] rows = (TypedDataSet.User_CommandRow[])dsetDB.User_Command.Select();

                foreach (TypedDataSet.User_CommandRow ucrow in rows)
                {
                    if (row.User_ID.Equals(0))
                    {
                        AddUser_Command((uint)ucrow.Input, ucrow.C_ID, newId, ucrow.BS_ID);
                    }
                }
                //
                // Return with the new user id
                //
                return newId;
            }
            catch (Exception ex)
            {
                string message = "Unable to add Operator profile, " +
                    " Reason: " +
                    ex.Message;
                MessageBox.Show(message,"Add Operator");
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Add a new record to the Information table
        /// </summary>
        /// <param name="isPresent">0 if not present</param>
        /// <param name="descrip">Describe the information</param>
        public void AddInfo(bool isPresent, string descrip)
        {
            try
            {
                TypedDataSet.InfoRow row = dsetDB.Info.NewInfoRow();
                row.BeginEdit();
                row.Is_Present = isPresent;
                row.Descrip = descrip;
                row.EndEdit();
                dsetDB.Info.AddInfoRow(row);
                // Enter the changes to the data set
                dsetDB.Info.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
            }
            catch (SqlException exSQL)
            {
                string message = "Unable to add Information record" +
                    ", Reason" +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Add a new record to the Command table
        /// </summary>
        /// <param name="type">0 - 6</param>
        /// <param name="cmd">Max 4 letters</param>
        /// <param name="descrip"></param>
        public void AddCommand(int type, string cmd, string descrip)
        {
            try
            {
                //Char[] tcmd = cmd.TrimEnd(' ').ToCharArray();
                TypedDataSet.CommandRow row = dsetDB.Command.NewCommandRow();
                row.BeginEdit();
                row.CommandType = type;
                row.Command = cmd;
                row.Description= descrip;
                row.EndEdit();
                dsetDB.Command.AddCommandRow(row);
                // Enter the changes to the data set
                dsetDB.Command.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
            }
            catch (SqlException exSQL)
            {
                string message = "Unable to add Information record" +
                    ", Reason" +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Currently selected stem
        /// </summary>
        private int pCurrentStemID;
        /// <summary>
        /// Current stem id
        /// </summary>
        public int CurrentStemID
        {
            get
            {
                this.UpdateStem();
                return pCurrentStemID;
            }
        }
        /// <summary>
        /// Update the stem id number to the current.
        /// </summary>
        public void UpdateStem()
        {
            //****************************************************************
            // Sort descending by spec ID will give the most recent row added.
            DataRow[] rows = dsetDB.Stem.Select(" Stem_Operator_ID = " + this.SelectedOperator, "Stem_ID DESC");
            //*********************************
            // Set the stem id
            //
            pCurrentStemID = (int)rows[0]["Stem_ID"];
        }
        private int pstem_count;
        public int User_Stem_Count
        {
            get
            {
                return pstem_count;
            }
        }
        public void UpdateStemCount()
        {
            //***************************
            // Filter for user
            //

            //***************************
            // Count visible rows
            //

        }
        /// <summary>
        /// Increment the stem id count by one and add the user to the record.
        /// </summary>
        /// <param name="user">Current Operator</param>
        /// <returns>The stem number</returns>
        public int AddStem()
        {
            try
            {
                TypedDataSet.StemRow row = dsetDB.Stem.NewStemRow();
                row.BeginEdit();
                row.Stem_Operator_ID = this.SelectedOperator;
                row.EndEdit();
                dsetDB.Stem.Rows.Add(row);
                // Enter the changes to the data set
                dsetDB.Stem.AcceptChanges();
                // Save the changes to data file
                dsetDB.WriteXml(dbFilename);

                return CurrentStemID;
            }
            catch (SqlException ase)
            {
                string message = "unable to add stem, Reason: " + ase.Errors[0].Message;
                MessageBox.Show(message);
                return 0;////Change from -1 on 09.08.2009
            }

        }
        /// <summary>
        /// Add a row to the Piece Profile table
        /// </summary>
        /// <param name="length">Length of peice</param>
        /// <param name="vol">Calculated volume</param>
        /// <returns>a 1 for success, a 0 for not</returns>
        public int AddPiece(Decimal length, Decimal vol )
        {
            try
            {

                //
                // Write data to file
                //
                swcountFile.WriteLine(DateTime.Now + " ," + length.ToString() + "," + vol.ToString());// DateTime is a system level information type
                swcountFile.Flush();
                //=============================================
                //
                // if adding piece fails
                //
                return 1;
            }
            catch (SqlException sqlEx)
            {
                return 0;//Change from -1 on 09.08.2009
            }
        }

        /// <summary>
        /// Add a piece profile to the xml data base
        /// </summary>
        /// <param name="stm">id of current stem</param>
        /// <param name="spec">Related bucking spec</param>
        /// <param name="length">current length</param>
        /// <param name="vol">calclated volume</param>
        /// <returns>The number of entries in the table</returns>
        public int AddPiece(int stm, int spec,Decimal length, Decimal vol )
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // XML data base
                //
                TypedDataSet.Piece_ProfileRow row = dsetDB.Piece_Profile.NewPiece_ProfileRow();
                row.BeginEdit();
                row.Profile_Spec = spec;
                row.Main_Stem = stm;
                row.Length = length;
                row.Volume = vol;
                row.Date_Cut = System.DateTime.Now;
                row.EndEdit();
                dsetDB.Piece_Profile.Rows.Add(row);
                // Enter the changes to the data set
                dsetDB.Piece_Profile.AcceptChanges();
                // Save the changes to data file
                dsetDB.WriteXml(dbFilename);


                DataRow[] rows = dsetDB.Piece_Profile.Select("Main_Stem = " + stm, "Profile_ID DESC");
                if (rows.Length <= 0)
                {
                    MessageBox.Show("Piece not added");
                    return 0;//Change from -1 on 09.08.2009
                }
                return (int)rows[0]["Profile_ID"];

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Could not add Volume, Reason: " + sqlEx.Message, "Add Piece");
                return 0;//Change from -1 on 09.08.2009
            }
        }
        /// <summary>
        /// Filters the User_Command table for the given input.
        /// Returns true if there is anything left to view.
        /// </summary>
        /// <param name="command"></param>
        public bool FilterUser_Command(uint input)
        {
            try
            {
                dsetDB.User_Command.DefaultView.RowFilter = "Input = " + input;//  + " OR C_ID = " + command;

                if (dsetDB.User_Command.DefaultView.Count == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to filter user command, " +
                    " Reason: " +
                    ex.Message,"Filter User_Command");
                return false;
            }

        }

        /// <summary>
        /// Filter the BuckingSpec table by the species
        /// returns true if there were any species in the
        /// filter.
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        public bool FilterBuckingBy(int species)
        {
            try
            {
                // set filter to current species
                dsetDB.BuckingSpec.DefaultView.RowFilter = "Species = " + species;
                if (dsetDB.BuckingSpec.DefaultView.Count == 0)
                    return false;
                // set current bucking spec to first row in filtered set

                //---------
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Filter BuckingSpec, " +
                    " Reason: " + ex.Message,"Filter Bucking Spec");
                return false;
            }
        }
        //======================================================================================
        //===================== PROPERTIES =====================================================

        /// <summary>
        /// Set the Command table filter to show only a given type
        /// </summary>
        public int FilterCommand
        {
            set
            {
                try
                {
                    // set filter to only display selected commands
                    dsetDB.Command.DefaultView.RowFilter = "CommandType = " + value;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Unable to Filter Command, " +
                        " Reason: " + ex.Message,"Filter Command");
                }

            }
        }

        /// <summary>
        /// set true to remove User_Command table filter
        /// </summary>
        /// <param name="filter"></param>
        public bool ClearUser_Command
        {
            set
            {
                try
                {
                    if (value)
                        dsetDB.User_Command.DefaultView.RowFilter = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to Filter User_Command, " +
                        " Reason: " + ex.Message, "Clear Filter User Command");
                }
            }
        }
        /// <summary>
        /// Clear any filter on the Command table
        /// </summary>
        public bool ClearFilterCommand
        {
            set
            {
                try
                {
                    if (value)
                        dsetDB.Command.DefaultView.RowFilter = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to clear Filter Command, " +
                        " Reason: " + ex.Message, "Clear Filter Command");
                }
            }

        }
        /// <summary>
        /// Clear any filter on the Bucking specification table;
        /// if 'true'
        /// </summary>
        public bool ClearFilterBucking
        {
            set
            {
                try
                {
                    if (value)
                        dsetDB.BuckingSpec.DefaultView.RowFilter = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to clear Bucking spec Filter , " +
                        " Reason: " + ex.Message,"Clear Filter Bucking Spec");
                }
            }
        }

        /// <summary>
        /// Currently selected operator
        /// </summary>
        public int SelectedOperator
        {
            get
            {
                return pSelectedOperator;
            }
            set
            {
                pSelectedOperator = value;
            }
        }
        /// <summary>
        /// Currently selected species
        /// </summary>
        public string SelectedSpeciesName
        {
            get 
            {
                return pSelctedSpeciesName;
            }
        }
        /// <summary>
        /// Selected species ID
        /// </summary>
        public int SelectedSpecies
        {
            get
            {
                return pSelectedSpecies;
            }
            set
            {
                try
                {
                    TypedDataSet.Tree_TypeRow[] rows = (TypedDataSet.Tree_TypeRow[])dsetDB.Tree_Type.Select();
                    foreach (TypedDataSet.Tree_TypeRow row in rows)
                    {
                        if (row.Type_ID == value)
                        {
                            pSelctedSpeciesName = row.Type_Name;
                            pSelectedSpecies = value;
                            return;
                        }
                            
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Problem selecting species, Reason: " + e.Message, "Select Species");
                }
            }
        }
        /// <summary>
        /// Selected bucking spec ID
        /// </summary>
        public int SelectedBuckingSpec
        {
            get
            {
                return pSelectedBuck;
            }
            set
            {
                try
                {
                    
                    if (dsetDB.BuckingSpec.Rows.Contains(value))
                    {// if value is valid make changes accordingly.
                        TypedDataSet.BuckingSpecRow row = (TypedDataSet.BuckingSpecRow)dsetDB.BuckingSpec.Rows.Find(value);
                        pSelectedBuck = value;
                        pSelectedLength = row.Len;
                        pSelectedMinDi = row.Min_Di;
                        pSelectedMaxDi = row.Max_Di;
                        SelectedSpecies = row.Species;
                        pOverLenWindow = row.Over_Window;
                        pUnderLenWindow = row.Under_Window;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error selecting bucking spec, Reason: " + e.Message,"Set Buck Spec");
                }

            }
        }
        /// <summary>
        /// Selected bucking length, based
        /// on selected species
        /// </summary>
        public decimal SelectedLength
        {
            get
            {
                return pSelectedLength;
            }
        }
        /// <summary>
        /// Selected minimum diameter, based 
        /// on selected species
        /// </summary>
        public decimal SelectedMinDi
        {
            get
            {
                return pSelectedMinDi;
            }
        }
        /// <summary>
        /// Selected maximum diameter, based 
        /// on selected species
        /// </summary>
        public decimal SelectedMaxDi
        {
            get
            {
                return pSelectedMaxDi;
            }
        }
        /// <summary>
        /// The selected bucking specs, 
        /// allowed overrun window.
        /// </summary>
        public decimal SelectedOverLengthWindow
        {
            get
            {
                return pOverLenWindow;
            }
        }

        public decimal SelectedUnderLengthWindow
        {
            get
            {
                return pUnderLenWindow;
            }
        }

        ///======================== END PROPERTIES ===============================================
        ///=======================================================================================

        /// <summary>
        /// Create a table style for the bucking spec table, 
        /// that does not show the row ID
        /// </summary>
        /// <param name="bdg"></param>
        public void CreateBuckTblStyle(ref DataGrid bdg)
        {
            DataGridTableStyle dgtsStyle;
            DataGridTextBoxColumn dgtsColumn;
            dgtsStyle = new DataGridTableStyle();
            dgtsStyle.MappingName = "BuckingSpec";
         
            //-------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Species";
            dgtsColumn.HeaderText = "Species";
            dgtsColumn.Width = 75;
            dgtsColumn.TextBox.TextAlign = HorizontalAlignment.Center;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //-------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Len";
            dgtsColumn.HeaderText = "Length";
            dgtsColumn.Width = 45;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //--------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Min_Di";
            dgtsColumn.HeaderText = "Min dia";
            dgtsColumn.Width = 45;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //---------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Max_Di";
            dgtsColumn.HeaderText = "Max dia";
            dgtsColumn.Width = 45;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //---------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Note";
            dgtsColumn.HeaderText = "Notes";
            dgtsColumn.Width = 75;
            dgtsColumn.TextBox.TextAlign = HorizontalAlignment.Center;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //---------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Over_Window";
            dgtsColumn.HeaderText = "Over Shoot";
            dgtsColumn.Width = 100;
            dgtsColumn.TextBox.TextAlign = HorizontalAlignment.Right;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //---------
            dgtsColumn = new DataGridTextBoxColumn();
            dgtsColumn.MappingName = "Under_Window";
            dgtsColumn.HeaderText = "Under Shoot";
            dgtsColumn.Width = 100;
            dgtsColumn.TextBox.TextAlign = HorizontalAlignment.Right;
            dgtsStyle.GridColumnStyles.Add(dgtsColumn);
            //-----------
            bdg.TableStyles.Add(dgtsStyle);
            
        }
        /// <summary>
        /// Delete a bucking record by specification ID.
        /// </summary>
        /// <param name="specID">Bucking specification ID</param>
        public void DeletRecordBucking(int specID)
        {
            
            TypedDataSet.BuckingSpecRow[] rows = (TypedDataSet.BuckingSpecRow[])dsetDB.BuckingSpec.Select("BSpec_ID = " + specID);
            foreach (TypedDataSet.BuckingSpecRow row in rows)
            {
                dsetDB.BuckingSpec.Rows.Remove(row);
            }
         
            // Enter the changes to the data set
            dsetDB.BuckingSpec.AcceptChanges();
            // Save the changes to data file
            xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
        }
        public void LoadUser_CommandGrid(ref DataGridView ucdg)
        {
            try
            {
                if (exists)
                {
                    ucdg.DataSource = dsetDB.User_Command;
                }
            }
            catch (Exception edg)
            {
                MessageBox.Show(edg.Message, "Load User_Command Grid");
            }
        }
        /// <summary>
        /// Load the dataSource for a Command table data grid view
        /// </summary>
        /// <param name="cdg"></param>
        public void LoadCommandGrid(ref DataGridView cdg)
        {
            try
            {
                if (exists)
                {
                    cdg.DataSource = dsetDB.Command;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Command Grid");
            }
        }
        /// <summary>
        /// Load Bucking spec table into data grid.
        /// Data grids data source.
        /// </summary>
        /// <param name="bdg"></param>
        public void LoadBuckingGrid(ref DataGridView bdg)
        {
            try
            {
                if (exists)
                {
                    bdg.DataSource = dsetDB.BuckingSpec;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Load Bucking Grid, " +
                    " Reason: " +
                    ex.Message, "Load Buck Grid");
            }
        }

        /// <summary>
        /// Load Bucking spec table into data grid.
        /// Data grids data source.
        /// </summary>
        /// <param name="bdg"></param>
        public void LoadBuckingGrid(ref DataGrid bdg)
        {
            try
            {
                if (exists)
                {
                    bdg.DataSource = dsetDB.BuckingSpec;
                   
                    CreateBuckTblStyle(ref bdg);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Load Bucking Grid, " +
                    " Reason: " +
                    ex.Message,"Load Buck Grid");
            }
        }

        /// <summary>
        /// Assign the Bucking spec data source to textbox's.
        /// </summary>
        /// <param name="lenTB">Length text box</param>
        /// <param name="minTB">Minimum diameter text box</param>
        /// <param name="maxTB">Maximum diameter text box</param>
        /// <param name="noteTB">Notes text box</param>
        public void LoadBuckingGrid(ref TextBox lenTB, ref TextBox minTB,
            ref TextBox maxTB, ref TextBox underWinTB, ref TextBox overWinTB)
        {
            try
            {
                if (lenTB != null)
                    lenTB.DataBindings.Add(new Binding("Text", dsetDB.BuckingSpec, "Len"));
                if (minTB != null)
                    minTB.DataBindings.Add(new Binding("Text", dsetDB.BuckingSpec, "Min_Di"));
                if (maxTB != null)
                    maxTB.DataBindings.Add(new Binding("Text", dsetDB.BuckingSpec, "Max_Di"));
                if(underWinTB != null)
                    underWinTB.DataBindings.Add(new Binding("Text", dsetDB.BuckingSpec, "Under_Window"));
                if(overWinTB != null)
                    overWinTB.DataBindings.Add(new Binding("Text",dsetDB.BuckingSpec, "Over_Window"));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Bucking text boxes, Reason: " + ex.Message,"Load BG source");
            }
        }
        /// <summary>
        /// Load a Data Grid with the Command Table data
        /// </summary>
        /// <param name="cdg">Data Grid</param>
        public void LoadCommandGrid(ref DataGrid cdg)
        {

            try
            {
                if (exists)
                {
                    cdg.DataSource = dsetDB.Command;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Load Command Grid, " +
                    " Reason: " +
                    ex.Message,"Load Command Grid");
            }

        }
        /// <summary>
        /// Loads a DataGrid with from the User_Command Table
        /// </summary>
        /// <param name="ucGrid"></param>
        public void LoadUser_CommandGrid(ref DataGrid ucGrid)
        {
            if (exists)
            {
                try
                {
                    ucGrid.DataSource = dsetDB.User_Command;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to Load User Command Grid" +
                        " Reason: " + ex.Message,"Load U_C Grid");
                }
            }
        }
        /// <summary>
        /// Load the Settings Tab page.  This function loads the settings
        /// table into the data set as a side effect
        /// </summary>
        public void LoadSettings(ref NumericUpDown bUpDn, ref NumericUpDown wUpDn,
                                        ref NumericUpDown tUpDn, ref NumericUpDown spd, 
                                        ref NumericUpDown mid, ref NumericUpDown rUp,
                                        ref NumericUpDown rDown, ref NumericUpDown rsSpeed)
        {

            try
            {
                if (exists)
                {
                    // Bind bottom Setting
                    bUpDn.DataBindings.Add(new Binding("Value", dsetDB.Settings, "BotK_Press"));
                    // Bind top Setting
                    tUpDn.DataBindings.Add("Value", dsetDB.Settings, "TopK_Press");
                    // Bind Wheel Setting
                    wUpDn.DataBindings.Add("Value", dsetDB.Settings, "WheelA_Press");
                    // Bind Speed Setting
                    spd.DataBindings.Add("Value", dsetDB.Settings, "Slow_Speed");
                    // Bind middle speed adjustment
                    mid.DataBindings.Add("Value", dsetDB.Settings, "Mid_Press");
                    // Bind Ramp Up distance
                    rUp.DataBindings.Add("Value", dsetDB.Settings, "RmpUp_Distance");
                    // Bind Ramp Down distance
                    rDown.DataBindings.Add("Value", dsetDB.Settings, "RmpDown_Distance");
                    // Bind Ramp Start Speed
                    rsSpeed.DataBindings.Add("Value", dsetDB.Settings, "RmpStart_Speed");
                }
            }
            catch (SqlException exSQL)
            {
                MessageBox.Show("Unable Load Settings, " +
                    " Reason: " +
                    exSQL.Errors[0].Message);
            }

        }

        /// <summary>
        /// Populate the Tree_Type combo box
        /// </summary>
        public void LoadTypeCombo(ref ComboBox cb, ref DataGrid dg) 
        {

            try
            {
                if (exists)
                {
                    //
                    // clear combo box
                    //
                    cb.Items.Clear();
                    //
                    // load the combo box
                    //
                    LoadTypeCombo(ref cb);
                    //
                    // Set the data source
                    //
                    dg.DataSource = dsetDB.Tree_Type;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Load Tree_Type Combo Box and grid, " +
                    " Reason: " +
                    ex.Message,"Load Type Combo & DG");
            }



        }
        /// <summary>
        /// Load a combo box to access the tree type table
        /// </summary>
        /// <param name="cb"></param>
        public void LoadTypeCombo(ref ComboBox cb)
        {
            try
            {
                // Bind the comboBox with the category names
                cb.DataSource = dsetDB.Tree_Type;
                cb.DisplayMember = "Type_Name";
                cb.ValueMember = "Type_ID";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Load Tree_Type Combo Box, " +
                    " Reason: " +
                    ex.Message,"Load Type combo");
            }
        }
        /// <summary>
        /// Loads a combo box with the information from
        /// the 'Description' column of the 'Command' table;
        /// Only type 2 and type 5 commands are added.
        /// </summary>
        /// <param name="cb">reference to a combo box</param>
        public void LoadCommandNote(ref ComboBox cb)
        {
            try
            {
                DataRow[] rows = dsetDB.Command.Select("CommandType = 2 OR CommandType = 5");
                cb.Items.Clear();
                //dt.DefaultView.RowFilter = "Command_ID = 16";
                // Bind the comboBox with the category names
                //cb.DataSource = dtabCom; //.DefaultView;

                //cb.DisplayMember = "Description";
                //cb.ValueMember = "Command_ID";
                foreach (DataRow i in rows)
                    cb.Items.Add(i["Description"]);

                cb.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                string message = "Unable to Load User Command Descriptions, " +
                    " Reason: " + ex.Message;
                MessageBox.Show(message,"Load Command Note");
                //Error_Stamp(message);
            }

        }
        /// <summary>
        /// Loads a combo box with information from
        /// 'Name' column of the 'Type' table
        /// </summary>
        /// <param name="cb"></param>
        public void LoadTypeName(ref ComboBox cb)
        {            
            
            // If there are no rows in the Type table
            //  return without changing the combo box
            if (dsetDB.Tree_Type.Rows.Count == 0)
            {
                return;
            }
            // clear combo box
            cb.Items.Clear();
            //

            // Read ea ro. Add the contents 
            //	to the combo box
            //	close the reader when done
            foreach (TypedDataSet.Tree_TypeRow row in dsetDB.Tree_Type.Rows)
            {
                cb.Items.Add(row.Type_Name);
            }
            // Start responding to ComboBox's
            //	SelectedIndexChanged events
            cb.SelectedIndex = 0;
        }
        /// <summary>
        /// Takes a reference to a check box and loads it with Info
        /// tables' Is_Present information at the MachineInfo location
        /// </summary>
        /// <param name="chkbx">reference to a check box</param>
        /// <param name="mi">MachineInfo enumerated datatype</param>
        public void LoadInfo(ref CheckBox chkbx, MachineInfo mi)
        {
            try
            {
                int i = (int)mi;
                dsetDB.Info.DefaultView.RowFilter = null;
                TypedDataSet.InfoRow row;
                row = (TypedDataSet.InfoRow)dsetDB.Info.Rows[i];
                //MessageBox.Show(row.ToString());
                if (dsetDB.Info.DefaultView.Count <= i)
                {
                    // Add the row?
                    MessageBox.Show("Row Does Not exist!");
                    AddInfo(false, chkbx.Text);
                }
                chkbx.Checked = row.Is_Present;
            }
            catch (Exception ex)
            {
                string message = "Load Info exception: " + ex.Message;
                MessageBox.Show(message, "LoadInfo");
                Error_Stamp(message);
            }
            //finally
            //{
            //    MessageBox.Show("Last at Info: ");
            //}
        }
        /// <summary>
        /// Takes a reference to a number updown box and loads it with
        /// Info tables' 'Total' information at the MachineInfo location
        /// </summary>
        /// <param name="nud">reference to NumericUpDown box</param>
        /// <param name="mi">MachineInfo enumerated datatype</param>
        public void LoadInfo(ref NumericUpDown nud, MachineInfo mi)
        {
            try
            {
                TypedDataSet.InfoRow row = dsetDB.Info.FindByInfo_ID((int)mi);
                nud.Value = (decimal)row.Total;

            }
            catch (Exception udExc)
            {
                string message = "Load Info: " + udExc.Message;
                MessageBox.Show(message,"LoadInfo: numeric");
                Error_Stamp(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">Command (or Bucking Spec id)</param>
        /// <returns></returns>
        public int GetCommandIDbyCommand(string command)
        {
            try
            {

                TypedDataSet.CommandRow[] rows = (TypedDataSet.CommandRow[])dsetDB.Command.Select();

                foreach (TypedDataSet.CommandRow row in rows)
                {
                    if (row.Command.Equals(command))
                        return row.Command_ID;
                }
                return -1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Get CommandID by command");
                Error_Stamp(e.Message);
                return -1;
            }
        }
        public int GetCommandTypebyCommand(string command)
        {
            try
            {

                TypedDataSet.CommandRow[] rows = (TypedDataSet.CommandRow[])dsetDB.Command.Select();

                //
                // if the command has 'ps..' it is a preset
                // selection.
                //
                if (command.StartsWith("ps"))
                    return 6;

                foreach (TypedDataSet.CommandRow row in rows)
                {
                    if (command.StartsWith(row.Command))
                        return row.CommandType;
                }
                return -1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Get GetCommandType by command");
                Error_Stamp(e.Message);
                return -1;
            }
        }
        /// <summary>
        /// Return the Command ID based on its description
        /// </summary>
        /// <param name="description"></param>
        /// <returns>The command id related to the description or -1 if none exists</returns>
        public int GetCommandID(string description)
        {
            try
            {

                TypedDataSet.CommandRow[] rows = (TypedDataSet.CommandRow[])dsetDB.Command.Select();

                foreach (TypedDataSet.CommandRow row in rows)
                {
                    if (row.Description.Equals(description))
                        return row.Command_ID;
                }
                    return -1;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Command, Reason: " + e;
                MessageBox.Show(message,"GetCommandID");
                Error_Stamp(message);
                return -1;
            }

        }
        /// <summary>
        /// Get a user id from the Operator table
        /// </summary>
        /// <param name="username"></param>
        /// <returns>user id</returns>
        public int GetUserID(string uname)
        {
            try
            {

                TypedDataSet.OperatorRow[] rows = (TypedDataSet.OperatorRow[])dsetDB.Operator.Select();

                foreach (TypedDataSet.OperatorRow row in rows)
                {
                    if (row.User_Name.Equals(uname))
                        return row.User_ID;
                }
                    return -1;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Command, Reason: " + e;
                MessageBox.Show(message,"GetCommandID");
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Takes an operator input command and returns the string it corresponds to.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetCmndStrng(uint input)
        {
            try
            {
                DataRow[] rows = dsetDB.User_Command.Select("Input = " + input, "I_ID ASC", DataViewRowState.CurrentRows);
                if (rows.Length > 0)
                {
                    int c = (int)rows[0]["C_ID"];

                    rows = dsetDB.Command.Select("Command_ID = " + c);

                    string send = (string)rows[0]["Command"];
                    //				MessageBox.Show("In GetCmndStrng: command ID " + send);
                    send = send.TrimEnd(null);
                    return send;
                }
                return null;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Command string, Reason: " + e;
                MessageBox.Show(message,"GetCmndString");
                Error_Stamp(message);
                return null;
            }
        }
        /// <summary>
        /// Get the description of a command
        /// </summary>
        /// <param name="input">Associated input</param>
        /// <returns>Command description string</returns>
        public string GetCmndDescrip(uint input)
        {
            try
            {
                TypedDataSet.User_CommandRow[] UCrows = (TypedDataSet.User_CommandRow[])dsetDB.User_Command.Select("Input = " + input, "I_ID ASC", DataViewRowState.CurrentRows);
                int c = 0;
                foreach (TypedDataSet.User_CommandRow row in UCrows)
                {
                    if (row.Input == input)
                    {
                        c = row.C_ID;
                        break;
                    }
                }
                TypedDataSet.CommandRow[] Crows = (TypedDataSet.CommandRow[])dsetDB.Command.Select();
                string send = "NA";
                foreach (TypedDataSet.CommandRow row in Crows)
                {
                    if (row.Command_ID == c)
                    {
                        send = row.Description;
                        send = send.TrimEnd(null);
                        break;
                    }
                    
                }
                return send;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Command description, Reason: " + e;
                MessageBox.Show(message,"Get Descrip Command");
                Error_Stamp(message);
                return null;
            }
        }
        /// <summary>
        /// This type refers to head operations, movements or settings.
        /// Takes an operator input command and returns the type as an integer.
        /// Primary Commands (type 1), Macro operational commands i.e. (1, 'cmd', 'Command')
        /// Secondary Commands (type 2) are combined with type 1 commands for 
        ///		individual operation commands i.e. 2, ('forw', 'Forward')
        /// Type 3 commands are for information purposes i.e (3, 'enln', 'Length encoder pulses per revolution')
        /// Type 4 commands are used for pressure and encoder pulse changes i.e. ( 4, 'topk', 'Top Knife Pressure')
        /// Returns -1 if there is no match found.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Command type, -1 for no match</returns>
        public int GetCmndType(uint input)
        {
            try
            {
                // Set the current row to match 'input
                DataRow[] rows = dsetDB.User_Command.Select("Input = " + input, "I_ID ASC", DataViewRowState.CurrentRows);
                if (rows.Length > 0)
                {
                    int c = (int)rows[0]["C_ID"];

                    rows = dsetDB.Command.Select("Command_ID = " + c);
                    int temp = (int)rows[0]["CommandType"];
                    return temp;
                }
                return -1;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Command string, Reason: " + e;
                MessageBox.Show(message,"Get Type Command");
                Error_Stamp(message);
                return -1;
            }
        }
        ///<summary>
        /// Get the input value associated with the command
        /// </summary>
        /// <param name="cmd">command string to look for</param>
        public long GetInput(string cmd)
        {
            try
            {
                // Set the current row to match 'cmd'
                TypedDataSet.CommandRow[] rows = (TypedDataSet.CommandRow[])dsetDB.Command.Select("Command = '" + cmd + "'");//, "Command_ID ASC", DataViewRowState.CurrentRows);

                if (rows.Length > 0)
                {// get the command ID at row 0
                    int c = rows[0].Command_ID;// (int)rows[0]["Command_ID"];
                    // set rows to the User_Cmd table with the same 
                    // value as Command ID
                   // Get the input value
                    TypedDataSet.User_CommandRow[] ucrows =
                        (TypedDataSet.User_CommandRow[])dsetDB.User_Command.Select("C_ID = " + c.ToString(),
                        "I_ID ASC", DataViewRowState.CurrentRows);
                    
                    if (ucrows.Length > 0)
                    {
                        long temp = (long)ucrows[0].Input;//["Input"];
                        return temp;
                    }

                }
                return -1;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Input, Reason: " + e;
                MessageBox.Show(message, "GetInput: cmd");
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Get the pressure/speed settings for the current tree type
        /// </summary>
        /// <param name="wheels"></param>
        /// <param name="topK"></param>
        /// <param name="bottomK"></param>
        /// <param name="speed"></param>
        public void GetSettings(ref int wheels, ref int topK, ref int bottomK, ref int speed)
        {
            try
            {
                DataRow[] rows = dsetDB.Settings.Select();//,"Species ASC", DataViewRowState.CurrentRows

                if (rows.Length > 0)
                {
                    wheels = (int)rows[0]["WheelA_Press"];
                    topK = (int)rows[0]["TopK_Press"];
                    bottomK = (int)rows[0]["BotK_Press"];
                    speed = (int)rows[0]["Slow_Speed"];
                }
                else
                {
                    wheels = topK = bottomK = speed = (-1);
                }
            }
            catch (SqlException e)
            {
                string message = "Unable to get Settings, " +
                    "Reason: " + e.Message;
                MessageBox.Show(message, "Get Settings");
                Error_Stamp(message);
            }

        }
        /// <summary>
        /// The type name string of the selected species
        /// </summary>
        /// <returns></returns>
        public string GetSelectedSpecies()
        {

            return Convert.ToString(dsetDB.Tree_Type.Rows[pSelectedSpecies]["Type_Name"]);

        }
        //----------------------BUCKING SPEC RELATED-----------------------------------
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Goes to the next species with bucking specs available.
        /// </summary>
        /// <returns></returns>
        public int NextSpecies()
        {
            try
            {
                int index;
                int currentType = SelectedSpecies;
                DataRow[] rows = dsetDB.Tree_Type.Select("Type_ID = " + pSelectedSpecies);

                index = dsetDB.Tree_Type.Rows.IndexOf(rows[0]);
                // Search thru spec table for a species with bucking specs assigned
                // only go thru table rows one time.
                for (int i = 0; i < dsetDB.Tree_Type.Rows.Count; i++)
                {
                    // Compare the currently selected species ID to the table row count
                    if (++index >= dsetDB.Tree_Type.Rows.Count)
                    {// if the length is greater, make the current species the next ID
                        index = 0;
                    }
                    SelectedSpecies = System.Convert.ToInt16(dsetDB.Tree_Type.Rows[index]["Type_ID"]);
                    SelectedBuckingSpec = 0;
                    if (currentType != SelectedSpecies)
                        return SelectedSpecies;
                }

                // if the length is equal, make the first ID the next ID
                return SelectedSpecies;
            }
            catch (SqlException sqlE)
            {
                string message = "Problem going to next species data, Reason: " + sqlE.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
            catch (Exception e)
            {
                string message = "Problem going to next species, Reason: " + e.Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Find the previous species to the one currently selected,
        /// select it iff it has bucking specs in it.
        /// </summary>
        /// <returns></returns>
        public int PrevSpecies()
        {
            try
            {
                int index;
                int currentType = SelectedSpecies;
                DataRow[] rows = dsetDB.Tree_Type.Select("Type_ID = " + pSelectedSpecies);
                index = dsetDB.Tree_Type.Rows.IndexOf(rows[0]);
                // Search thru spec table for a species with bucking specs assigned
                // only go thru table rows one time.
                for (int i = 0; i < dsetDB.Tree_Type.Rows.Count; i++)
                {
                    // Compare the currently selected species ID to the table row count
                    if (--index < 0)
                    {// if the index is greater or equal than the number of rows, make the current species the next ID
                        index = (dsetDB.Tree_Type.Rows.Count - 1);
                    }
                    else
                    {// Select the new species
                        SelectedSpecies = System.Convert.ToInt16(dsetDB.Tree_Type.Rows[index]["Type_ID"]);
                        SelectedBuckingSpec = 0;
                    }
                    // if the selected species changes, return
                    if (currentType != SelectedSpecies)
                        return SelectedSpecies;
                }

                // if the length is equal, make the first ID the next ID
                return SelectedSpecies;
            }
            catch (SqlException sqlE)
            {
                string message = "Problem going to next species data, Reason: " + sqlE.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
            catch (Exception e)
            {
                string message = "Problem going to next species, Reason: " + e.Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
        }

        /// <summary>
        /// Find the next bucking spec to the one currently selected
        /// </summary>
        /// <returns></returns>
        public int NextBuckSpec(RowUpDown dir)
        {
            try
            {
                int index = 0;
                int currentSpec = SelectedBuckingSpec;
                dsetDB.BuckingSpec.DefaultView.RowFilter = "Species = " + pSelectedSpecies.ToString();// ;
                if (dir == RowUpDown.Up)
                    dsetDB.BuckingSpec.DefaultView.Sort = "Length ASC";
                else if (dir == RowUpDown.Down)
                    dsetDB.BuckingSpec.DefaultView.Sort = "Length DESC";

                foreach (DataRowView j in dsetDB.BuckingSpec.DefaultView)
                {
                    if (pSelectedBuck == Convert.ToInt32(j["Spec_ID"]))
                        break;
                    index++;
                }
                // Use a data row to hold unsorted rows of the selected species
                DataRow[] rawSpecs = dsetDB.BuckingSpec.Select("Species = " + pSelectedSpecies);
                int rowCount = rawSpecs.Length;

                // Search thru spec table for a species with bucking specs assigned
                // only go thru table rows one time.
                for (int i = 0; i < rowCount; i++)
                {
                    // Compare the currently selected species ID to the table row count
                    if (++index >= rowCount)
                    {// if the length is greater, make the current species the next ID
                        index = 0;
                    }                    
                    SelectedBuckingSpec = System.Convert.ToInt16(dsetDB.BuckingSpec.DefaultView[index]["Spec_ID"]);
                    if (currentSpec != SelectedBuckingSpec)
                        return SelectedBuckingSpec;
                }

                // if the length is equal, make the first ID the next ID
                return SelectedBuckingSpec;
            }
            catch (SqlException sqlE)
            {
                string message = "Problem going to next Bucking spec. data, Reason: " + sqlE.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
            catch (Exception e)
            {
                string message = "Problem going to next Bucking spec., Reason: " + e.Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
        }

        public int NearestBuckSpec(float currLen)
        {
            // set the filter for the current species 
            dsetDB.BuckingSpec.DefaultView.RowFilter = "Species = " + pSelectedSpecies.ToString();
            // Use a data row to hold unsorted rows of the selected species
            DataRow[] rawSpecs = dsetDB.BuckingSpec.Select("Species = " + pSelectedSpecies);
            int rowCount = rawSpecs.Length;
            int i = 1;
            for (; i < rowCount; i++)
            {// Compare the currLen to the current selected bucking spec length
                if (currLen >= (float)dsetDB.BuckingSpec.DefaultView[i-1]["Length"] &&
                    currLen < (float)dsetDB.BuckingSpec.DefaultView[i]["Length"])
                {
                    break;
                }
            }
            // if checked all rows there is no change
            if (i >= rowCount)
                return SelectedBuckingSpec;
            // find absolute distances between length i and length i-1
            float a, b; // distances
            a = Math.Abs((float)dsetDB.BuckingSpec.DefaultView[i]["Length"] - currLen);// distance to larger spec
            b = Math.Abs(currLen - (float)dsetDB.BuckingSpec.DefaultView[i - 1]["Length"]);// distance to smaller spec
            // if the distance to the longer spec is shorter, b - a is positive
            if ((b - a) >= 0)
                return SelectedBuckingSpec = i;
            // else 
            return SelectedBuckingSpec = (i - 1);

        }
        /// <summary>
        /// Get (parse) the bucking information from the DataRow 'row'
        /// </summary>
        /// <param name="length">Length data is placed here</param>
        /// <param name="min">Minimum diameter data is placed here</param>
        /// <param name="max">Maximum diameter data is placed here</param>
        /// <param name="row">DataRow containing bucking info to parse</param>
        /// <returns>The Bucking spec ID</returns>
        private int GetSpecFromRow(ref float length, ref float min, ref float max, DataRow row)
        {
            try
            {
                if (!length.Equals(null))
                    length = Convert.ToSingle(row["Length"]);
                if (!min.Equals(null))
                    min = Convert.ToSingle(row["Min_Di"]);
                if (!max.Equals(null))
                {
                    if (!row.IsNull("Max_Di"))
                        max = Convert.ToSingle(row["Max_Di"]);
                }
                return Convert.ToInt16(row["Spec_ID"]);
            }
            catch (Exception e)
            {
                string message = "Unable to get spec from row, " +
                    "Reason: " + e.Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Ensure there is a command table row related to the 
        /// bucking spec id.  If there is not one, this function
        /// will add a new row.
        /// </summary>
        /// <param name="specID">Bucking spec table ID to validate</param>
        /// <returns>Return Command table row ID for specID, -1 if operation failed</returns>
        public int ValidateBuckCmd(int buckSpecID)
        {
            try
            {
                //
                // Get the row for the spec id
                //
                TypedDataSet.BuckingSpecRow buckRow = (TypedDataSet.BuckingSpecRow)
                    dsetDB.BuckingSpec.Rows.Find(buckSpecID);
                //
                // Find the specific command id based on the command string
                //
                //TypedDataSet.CommandRow[] commandRows = (TypedDataSet.CommandRow[])dsetDB.Command.Select();
                int cid = GetCommandIDbyCommand(buckSpecID.ToString());
                //
                // Confirm cid is valid
                //
                if (cid > 0)
                    return cid;/*Return the command related to the bucking spec*/
                //
                // If execution reaches here, there is no row in the command table
                // related to the bucking spec ID
                // Make a type 6 with for the bucking spec ID
                //
                //add a row to the command table
                //
                AddCommand(6, buckSpecID.ToString(), (string)buckRow.Note);
                //RefreshCommand();
                //
                // Get the new command ID
                //
                cid = GetCommandIDbyCommand(buckSpecID.ToString());
                //
                // Confirm Command ID again
                //
                if (cid > 0)
                    return cid;
                return -1;/*Return failed operations*/

            }
            catch (SqlException e)
            {
                string message = "Cannot validate. Reason: " + e.Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Cause the Tree table to be filtered by type and grade.
        /// </summary>
        /// <param name="tType"></param>
        /// <param name="grade"></param>
        public void SelectTreeType(string tType, string grade)
        {

        }
        //============================ END BUCKING RELATED ======================================================
        //--------------------------------------------------------------------------------------------------------
       
        
        /// <summary>
        /// Get the scaler for length calibration.
        /// </summary>
        /// <param name="type">Tree Species</param>
        /// <returns>Scaler value for type</returns>
        public decimal GetLengthScaler(int type)
        {
            try
            {
                TypedDataSet.CalibrationRow[] rows = (TypedDataSet.CalibrationRow[])dsetDB.Calibration.Select("Cal_Species = " + type.ToString());//,"Species ASC", DataViewRowState.CurrentRows

                if (rows.Length > 0)
                {
                    decimal temp = rows[0].LengthScaler;// System.Convert.ToSingle(rows[0]["LengthScaler"]);

                    return temp;
                }
                else return -1.0m;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Length Scaler, Reason: " + e;
                MessageBox.Show(message,"Get Scaler, Length");
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public decimal GetDiaScaler(int type)
        {
            try
            {
                TypedDataSet.CalibrationRow[] rows = (TypedDataSet.CalibrationRow[])dsetDB.Calibration.Select("Cal_Species = " + type.ToString());//,"Species ASC", DataViewRowState.CurrentRows

                if (rows.Length > 0)
                {
                    decimal temp = rows[0].DiaScaler;// System.Convert.ToSingle(rows[0]["DiaScaler"]);

                    return temp;
                }
                else return -1.0m;
            }
            catch (Exception e)
            {
                string message = "Final exception at Get Diameter Scaler, Reason: " + e;
                MessageBox.Show(message,"Get Scaler, Dia");
                Error_Stamp(message);
                return -1;
            }
        }
        /// <summary>
        /// Get the length scaler for the current tree type
        /// </summary>

        /// <summary>
        /// Set the Length scaler for the current tree type
        /// </summary>
        public void reSetLengthScaler(float s, int type)
        {
            try
            {
                //
                // scaler is a valid number
                //
                Decimal scaler;
                if (s == 0 || float.IsNaN(s) || float.IsInfinity(s))
                    scaler = 1;
                else
                    scaler = (Decimal)s;
                //
                TypedDataSet.CalibrationRow[] cRows = (TypedDataSet.CalibrationRow[])
                    dsetDB.Calibration.Select("Cal_Species = " + type.ToString());
                //
                if (cRows.Length > 0)
                {
                    foreach (TypedDataSet.CalibrationRow row in cRows)
                    {
                        if (row.Cal_Species == type)
                        {
                            row.BeginEdit();
                            row.LengthScaler = scaler;
                            row.EndEdit();
                        }
                    }
                }
                else
                {
                    TypedDataSet.CalibrationRow cr = dsetDB.Calibration.NewCalibrationRow();
                    cr.BeginEdit();
                    cr.Cal_Species = type;
                    cr.LengthScaler = scaler;
                    cr.DiaScaler = 0.002m;
                    cr.Slice = 5;
                    cr.EndEdit();
                    dsetDB.Calibration.AddCalibrationRow(cr);
                }
                
                //// Locate the ID for the selected input
                ////DataRow[] cRow = dsetDB.Calibration.Select("Cal_Species = " + type.ToString(), "Cal_ID ASC", DataViewRowState.CurrentRows);//				
                //int a = (int)cRow[0]["Cal_ID"];
                //cRow[0]["LengthScaler"] = scaler;
                //object[] rowArr = new object[5];
                //rowArr[0] = a;
                //rowArr[1] = cRow[0][1];
                //rowArr[2] = cRow[0][2];
                //rowArr[3] = cRow[0][3];
                //rowArr[4] = cRow[0][4];
                //// Load Data Row will find an existing type or add a new row
                //dsetDB.Calibration.BeginLoadData();
                //dsetDB.Calibration.LoadDataRow(rowArr, true);
                //dsetDB.Calibration.EndLoadData();
                //// Ensure the data is stored
                //// Enter the changes to the data set
                dsetDB.AcceptChanges();
                // Save the changes to data file
               //WriteTableXMLfile(dbFilename);
               dsetDB.WriteXml(dbFilename, XmlWriteMode.WriteSchema);

            }
            catch (SqlException exSQL)
            {
                string message = "Unable to change Length Scaler, " +
                    " Reason: " +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                // To be Added --> a setLengthScaler funcion call
            }
            catch (Exception any)
            {
                MessageBox.Show(any.Message, "Re-Set length Cal.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        public void reSetDiaScaler(float s, int type) 
        {
            try
            {
                // scaler is a valid number
                decimal scaler;
                if (s == 0 || float.IsNaN(s) || float.IsInfinity(s))
                    scaler = 1;
                else
                    scaler = (decimal)s;
                // Locate the ID for the selected input
                TypedDataSet.CalibrationRow[] cRows = (TypedDataSet.CalibrationRow[])
                    dsetDB.Calibration.Select("Cal_Species = " + type.ToString());

                //
                if (cRows.Length > 0)
                {
                    foreach (TypedDataSet.CalibrationRow row in cRows)
                    {
                        if (row.Cal_Species == type)
                        {
                            row.BeginEdit();
                            row.DiaScaler = scaler;
                            row.EndEdit();
                        }
                    }
                }
                else
                {
                    TypedDataSet.CalibrationRow cr = dsetDB.Calibration.NewCalibrationRow();
                    cr.BeginEdit();
                    cr.Cal_Species = type;
                    cr.LengthScaler = 1.0m;
                    cr.DiaScaler = scaler;
                    cr.Slice = 5;
                    cr.EndEdit();
                    dsetDB.Calibration.AddCalibrationRow(cr);
                }
                //DataRow[] cRow = dsetDB.Calibration.Select("Species = " + type.ToString(), "Cal_ID ASC", DataViewRowState.CurrentRows);//				
                //int a = (int)cRow[0]["Cal_ID"];
                //cRow[0]["DiaScaler"] = scaler;
                //object[] rowArr = new object[5];
                //rowArr[0] = a;
                //rowArr[1] = cRow[0][1];
                //rowArr[2] = cRow[0][2];
                //rowArr[3] = cRow[0][3];
                //rowArr[4] = cRow[0][4];
                // Load Data Row will find an existing type or add a new row
                //dsetDB.Calibration.BeginLoadData();
                //dsetDB.Calibration.LoadDataRow(rowArr, true);
                //dsetDB.Calibration.EndLoadData();
                //// Ensure the data is stored
                //// Enter the changes to the data set
                //dsetDB.AcceptChanges();
                //// Save the changes to data file
                //xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
                //
                // Accept changes to db
                dsetDB.AcceptChanges();
                // Save the changes to data file
                //WriteTableXMLfile(dbFilename);
                //
                // Save to file
                //
                dsetDB.WriteXml(dbFilename, XmlWriteMode.WriteSchema);

            }
            catch (SqlException exSQL)
            {
                string message = "Unable to change Diameter Scaler, " +
                    " Reason: " +
                    exSQL.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
                // To be Added --> a setLengthScaler funcion call
            }        
        }
        /// <summary>
        /// Clears the bucking spec table and fills again from source.
        /// </summary>

        /// <summary>
        /// Clears volume table and fills again from source.
        /// </summary>
        public void ResetPieceProfileTable()
        {
            dsetDB.Piece_Profile.Reset();
        }
        /// <summary>
        /// Clears the Command table and fills it again from source.
        /// </summary>
        public void RefreshCommand()
        {
            dsetDB.Command.Reset();
            
        }
        /// <summary>        
        /// Cause any changes to the settings information to
        /// be propogated the database.
        /// 02.12.2009
        /// Should add the ability to change settings by tree type
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="mi"></param> 
        public void SetSettings(ref NumericUpDown ud, int setting, MachineSettings mi)//int setting,MachineSettings mi
        {
            try
            {
                ud.DataBindings.Clear();
                dsetDB.Settings.Rows[0].BeginEdit();
                dsetDB.Settings.Rows[0][(int)mi] = setting;
                dsetDB.Settings.Rows[0].EndEdit();
                //daptSettings.Update(dtabSettings);
                // Added: 01.28.2010
                // Update XSD file
                xmlDBdoc.Save(dbFilename);//dsetDB.WriteXml(dbFilename);
                

                //ud.DataBindings.Add("Value", dtabSettings,(int)mi);
            }
            catch (SqlException setEx)
            {
                string message = "Set Settings: " + setEx.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Set the machines Is_Present information at index mi with
        /// the 'check' boolean value.
        /// </summary>
        /// <param name="check">Present if true</param>
        /// <param name="mi">Machine information index</param>
        public void SetInfo(bool check, MachineInfo mi)
        {
            try
            {
                int chk = Convert.ToInt32(check);
                dsetDB.Info.Rows[(int)mi].BeginEdit();
                dsetDB.Info.Rows[(int)mi]["Is_Present"] = chk;
                dsetDB.Info.Rows[(int)mi].EndEdit();

                // Enter the changes to the data set
                dsetDB.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);//WriteTableXMLfile(dbFilename);
            }
            catch (SqlException siEx)
            {
                string message = "Set Info(a): " + siEx.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Set the machines Total (count of sensors etc.) information 
        /// at index mi with the tot integer.
        /// </summary>
        /// <param name="tot">Total</param>
        /// <param name="mi">Machine information index</param>
        public void SetInfo(int tot, MachineInfo mi)
        {
            try
            {
                dsetDB.Info.Rows[(int)mi]["Total"] = tot;
                // Enter the changes to the data set
                dsetDB.AcceptChanges();
                // Save the changes to data file
                xmlDBdoc.Save(dbFilename);// WriteTableXMLfile(dbFilename);
            }
            catch (SqlException siExc)
            {
                string message = "Set Info(b): " + siExc.Errors[0].Message;
                MessageBox.Show(message);
                Error_Stamp(message);
            }
        }
        /// <summary>
        /// Update BuckingSpec table source
        /// </summary>
        public void UpdateBuckingSpec()
        {
            //daptBuckingSpec.Update(dsetDB, "BuckingSpec");
            MessageBox.Show("Update Bucking needs tending");

        }
        ///<summary>
        ///Export the database to XML format
        ///</summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public string WriteTableXMLfile(string filename)
        {
            return "Not Valid Function!";
            //try
            //{
            //    string temp = "Hello";
            //    // the following is directly from the MSDN
            //    if (dsetDB == null) { return "bad"; }

            //    // Create the FileStream to write with.
            //    System.IO.FileStream stream = new System.IO.FileStream
            //        (filename, System.IO.FileMode.Create);

            //    // Create an XmlTextWriter with the fileStream.
            //    System.Xml.XmlTextWriter xmlWriter =
            //        new System.Xml.XmlTextWriter(stream,
            //        System.Text.Encoding.Unicode);

            //    // Write to the file with the WriteXml method.,XmlWriteMode.WriteSchema
            //    dsetDB.WriteXml(xmlWriter,XmlWriteMode.WriteSchema);// WriteXml(xmlWriter);
            //    xmlWriter.Close();
            //    //----------------------------------------------End MSDN
            //    return temp;

            //}
            //catch (SqlException exSQL)
            //{
            //    string message = "Unable to get XML string, " +
            //        " Reason: " + exSQL.Errors[0].Message;
            //    MessageBox.Show(message);
            //    Error_Stamp(message);
            //    return null;
            //}
        }

        private static void Error_Stamp(String error)
        {
            //=============================================
            string errorFile = @"DataLog\\error_DB.txt";
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
        /// Fill the data set with default values
        /// </summary>
        private void FillDefaultDB()
        {
            try
            {
                /**************************
                 * Fill User Table
                 * ***********************/
                dsetDB.Operator.AddOperatorRow("Default", "Operator", 5, "sbxp1","Admin");/*level five is high*/
                dsetDB.Operator.AcceptChanges();
                /*************************
                 * Fill Tree_Type Table
                 * **********************/
                dsetDB.Tree_Type.AddTree_TypeRow("All", "All Species");
                dsetDB.Tree_Type.AddTree_TypeRow("Unknown", "NA");
                dsetDB.Tree_Type.AddTree_TypeRow("Rot", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Fir", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Pine", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Aspen", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Hemlock", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Cedar", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Poplar", "");
                dsetDB.Tree_Type.AddTree_TypeRow("Fire Wood", "");
                dsetDB.Tree_Type.AcceptChanges();
                /***********************
                 * Fill Default Settings Table
                 * ********************/
                dsetDB.Settings.AddSettingsRow(50, 45, 80, 95, 0, 25, 25, 10);
                dsetDB.Settings.AddSettingsRow(95, 75, 60, 0, 0, 35, 30, 15);
                dsetDB.Settings.AcceptChanges();
                /***********************
                 * Fill Tree_Settings Table
                 * **************************/
                TypedDataSet.Tree_SettingsRow tr = dsetDB.Tree_Settings.NewTree_SettingsRow();
                tr.BeginEdit();
                tr.T_T_ID = 0;
                tr.S_ID = 0;
                tr.EndEdit();
                dsetDB.Tree_Settings.AddTree_SettingsRow(tr);
                tr = dsetDB.Tree_Settings.NewTree_SettingsRow();
                tr.BeginEdit();
                tr.T_T_ID = 1;
                tr.S_ID = 1;
                tr.EndEdit();
                dsetDB.Tree_Settings.AddTree_SettingsRow(tr);
                dsetDB.Tree_Settings.AcceptChanges();
                /******************************
                 * Fill Command Table
                 * ***************************/
                // Type 1, primary commands
                dsetDB.Command.AddCommandRow(1, "cmd", "Command");
                dsetDB.Command.AddCommandRow(1, "set", "Set Levels");
                dsetDB.Command.AddCommandRow(1, "spc", "Special Command");
                dsetDB.Command.AddCommandRow(1, "stp", "Stop");
                dsetDB.Command.AddCommandRow(1, "err", "Error");
                dsetDB.Command.AddCommandRow(1, "nop", "No Operation");
                dsetDB.Command.AddCommandRow(1, "rep", "Reply");
                dsetDB.Command.AddCommandRow(1, "dat", "Data");
                dsetDB.Command.AddCommandRow(1, "agn", "Send Again");
                dsetDB.Command.AddCommandRow(1, "clr", "Clear");
                dsetDB.Command.AddCommandRow(1, "ej", "Input report from e and j");
                dsetDB.Command.AddCommandRow(1, "abcd", "Input report from a, b, c & d");
                dsetDB.Command.AcceptChanges();
                // Type 2, individual operation commands
                dsetDB.Command.AddCommandRow(2, "fobd", "Slow by one when in forward");
                dsetDB.Command.AddCommandRow(2, "rebd", "Slow by one when in reverse");
                dsetDB.Command.AddCommandRow(2, "forw", "Forward");
                dsetDB.Command.AddCommandRow(2, "fosl", "Slow Forward");
                dsetDB.Command.AddCommandRow(2, "reve", "Reverse");
                dsetDB.Command.AddCommandRow(2, "resl", "Slow Reverse");
                dsetDB.Command.AddCommandRow(2, "open", "Head Open");
                dsetDB.Command.AddCommandRow(2, "clos", "Head Close");
                dsetDB.Command.AddCommandRow(2, "tkop", "Top Knife Open");
                dsetDB.Command.AddCommandRow(2, "tkcl", "Top Knife Close");
                dsetDB.Command.AddCommandRow(2, "bkop", "Bottom Knife Open");
                dsetDB.Command.AddCommandRow(2, "bkcl", "Bottom Knife Close");
                dsetDB.Command.AddCommandRow(2, "whop", "Wheels Open");
                dsetDB.Command.AddCommandRow(2, "whcl", "Wheels Close");
                dsetDB.Command.AddCommandRow(2, "sawm", "Activate Main Saw");
                dsetDB.Command.AddCommandRow(2, "sawt", "Activate Top Saw");
                dsetDB.Command.AddCommandRow(2, "tiup", "Tilt Up");
                dsetDB.Command.AddCommandRow(2, "tido", "Tilt Down");
                dsetDB.Command.AddCommandRow(2, "tifl", "Tilt Float");
                dsetDB.Command.AddCommandRow(2, "eaut", "Clear Bucking Selection");
                // Type 3, information purposes
                dsetDB.Command.AddCommandRow(3, "phoh", "Photo eye active; wood present");
                dsetDB.Command.AddCommandRow(3, "phol", "Photo eye inactive; no wood present");
                dsetDB.Command.AddCommandRow(3, "encs", "Saw encoder information");
                //Type 4, pressure and speed settings
                dsetDB.Command.AddCommandRow(4, "topk", "Top Knife Pressure");
                dsetDB.Command.AddCommandRow(4, "botk", "Bottom Knife Pressure");
                dsetDB.Command.AddCommandRow(4, "whar", "Wheel Arm Pressure");
                dsetDB.Command.AddCommandRow(4, "whsp", "Slow Wheel Speed");
                dsetDB.Command.AddCommandRow(4, "buup", "Bump speed up by one");
                dsetDB.Command.AddCommandRow(4, "budo", "Bump speed down by one");
                dsetDB.Command.AddCommandRow(4, "rmst", "Ramp starting speed");
                dsetDB.Command.AddCommandRow(4, "rmto", "Ramp up distance");
                dsetDB.Command.AddCommandRow(4, "rmdo", "Ramp down distance");
                dsetDB.Command.AddCommandRow(4, "rmus", "Use ramps");
                dsetDB.Command.AddCommandRow(4, "finb", "Use the Find butt feature");
                //Type 5, Cabin logic commands
                dsetDB.Command.AddCommandRow(5, "uptr", "Next tree type");
                dsetDB.Command.AddCommandRow(5, "dntr", "Previous tree type");
                dsetDB.Command.AddCommandRow(5, "upbu", "Next bucking spec.");
                dsetDB.Command.AddCommandRow(5, "dnbu", "Previous bucking spec.");
                dsetDB.Command.AddCommandRow(5, "near", "Nearest bucking spec.");
                dsetDB.Command.AddCommandRow(5, "cler", "Clear Length");
                dsetDB.Command.AddCommandRow(5, "togl", "Toggle Length between butt & top saws");
                dsetDB.Command.AddCommandRow(5, "sawb", "Combined Top & Butt saw button");
                dsetDB.Command.AddCommandRow(5, "ckop", "Both Knives Open");
                dsetDB.Command.AddCommandRow(5, "ckcl", "Both Knives Close");
                dsetDB.Command.AddCommandRow(5, "news", "New Stem or Tree");
                dsetDB.Command.AddCommandRow(5, "over", "Override Auto Feed");
                dsetDB.Command.AddCommandRow(5, "shif", "Shift to next input level");
                dsetDB.Command.AcceptChanges();
                /**********************************************************
                 * Fill Grade Table
                 * **********************/
                dsetDB.Grade.AddGradeRow("merch", "Meets minimum grade for lumber");
                dsetDB.Grade.AddGradeRow("Rot", "Rotten or punky log");
                dsetDB.Grade.AddGradeRow("Fiber", "Usabe for bio-fuel only");
                dsetDB.Grade.AcceptChanges();
                /************************************************************
                 * Fill BuckingSpec Table
                 * ***************************/
                // Requires more complex row creation
                TypedDataSet.BuckingSpecRow br = dsetDB.BuckingSpec.NewBuckingSpecRow();
                br.BeginEdit();
                br.Species = 4;
                br.BSpec_Grade = 0;
                br.Len = 5.0M;
                br.Min_Di = 5;
                br.Max_Di = 0;
                br.Note = "5.0m";
                br.Over_Window = 2.5m;
                br.Under_Window = 0.0m;
                br.EndEdit();
                dsetDB.BuckingSpec.AddBuckingSpecRow(br);
                br = dsetDB.BuckingSpec.NewBuckingSpecRow();
                br.BeginEdit();
                br.Species = 4;
                br.BSpec_Grade = 0;
                br.Len = 4.4M;
                br.Min_Di = 5;
                br.Max_Di = 0;
                br.Note = "4.4m";
                br.Over_Window = 2.5m;
                br.Under_Window = 0.0m;
                br.EndEdit();
                dsetDB.BuckingSpec.AddBuckingSpecRow(br);
                br = dsetDB.BuckingSpec.NewBuckingSpecRow();
                br.BeginEdit();
                br.Species = 4;
                br.BSpec_Grade = 0;
                br.Len = 3.8M;
                br.Min_Di = 5;
                br.Max_Di = 0;
                br.Note = "3.8m";
                br.Over_Window = 2.5m;
                br.Under_Window = 0.0m;
                br.EndEdit();
                dsetDB.BuckingSpec.AddBuckingSpecRow(br);
                dsetDB.BuckingSpec.AcceptChanges();
                /*************************************************************
                 * Fill User_Command Table
                 * This can be either Default or user
                 * **********************************************************/
                TypedDataSet.User_CommandRow ur = dsetDB.User_Command.NewUser_CommandRow();
                ur.BeginEdit();
                ur.Input = 0;
                ur.C_ID = 3;/*Stop command*/
                ur.U_ID = 0;
                ur.EndEdit();
                dsetDB.User_Command.AddUser_CommandRow(ur);
                dsetDB.User_Command.AcceptChanges();
                /***************************************************
                 * Fill Calibration Table
                 * ***********************/
                TypedDataSet.CalibrationRow cr = dsetDB.Calibration.NewCalibrationRow();
                cr.BeginEdit();
                cr.Cal_Species = 0;
                cr.LengthScaler = 0.00145m;
                cr.DiaScaler = 0.002m;
                cr.Slice = 5;
                cr.EndEdit();
                dsetDB.Calibration.AddCalibrationRow(cr);
                cr = dsetDB.Calibration.NewCalibrationRow();
                cr.BeginEdit();
                cr.Cal_Species = 1;
                cr.LengthScaler = 0.00145m;
                cr.DiaScaler = 0.002m;
                cr.Slice = 5;
                cr.EndEdit();
                dsetDB.Calibration.AddCalibrationRow(cr);
                dsetDB.Calibration.AcceptChanges();
                /*************************************************************
                 * Fill Information Table
                 * ************************/
                dsetDB.Info.AddInfoRow(false, "Main Saw Encoder Present?", 0);
                dsetDB.Info.AddInfoRow(true, "Photo Eye Present", 0);
                dsetDB.Info.AddInfoRow(false, "Top Saw Present?", 0);
                dsetDB.Info.AddInfoRow(false, "Top Saw Encoder Present?", 0);
                dsetDB.Info.AddInfoRow(false, "Head uses PWM Valves?", 0);
                dsetDB.Info.AddInfoRow(true, "Number of Diameter Encoders?", 2);
                dsetDB.Info.AddInfoRow(true, "Head Should float when not holding tilt button?",0);
                dsetDB.Info.AddInfoRow(true, "Buttons held at same time will be checked for seperate functions?",0);
                dsetDB.Info.AddInfoRow(false, "Use U.S. Standard for measurements?",0);
                dsetDB.Info.AddInfoRow(true, "Use Semi-Auto feed?",0);
                dsetDB.Info.AddInfoRow(false, "Use Automatic feed?", 0);
                dsetDB.Info.AddInfoRow(true, "Butt and Top saws activate with the same button: select by length display",0);
                // Added on 02.23.2010
                dsetDB.Info.AddInfoRow(true, "Only saw within selected length window",0);
                dsetDB.Info.AddInfoRow(true, "Audible in-window indicator",0);
                dsetDB.Info.AddInfoRow(false, "Use ramps when feeding", 0);
                dsetDB.Info.AddInfoRow(false, "Search for butt in auto feed?", 0);
                dsetDB.Info.AcceptChanges();
                /***********************************************************
                 * Fill Stem Table
                 * ***************************/
                TypedDataSet.StemRow sr = dsetDB.Stem.NewStemRow();
                sr.BeginEdit();
                sr.Stem_Operator_ID = 0;
                sr.EndEdit();
                dsetDB.Stem.AddStemRow(sr);
                dsetDB.Stem.AcceptChanges();
                /*****************************************************
                 * Fill Piece_Profile Table
                 * ************************/
                
                TypedDataSet.Piece_ProfileRow pr = dsetDB.Piece_Profile.NewPiece_ProfileRow();
                pr.BeginEdit();
                pr.Main_Stem = 0;
                pr.Profile_Spec = 0;
                pr.Length = 5.0m;
                pr.Volume = 0;
                pr.Date_Cut = System.DateTime.Today;
                pr.EndEdit();
                dsetDB.Piece_Profile.AddPiece_ProfileRow(pr);
                dsetDB.Piece_Profile.AcceptChanges();



            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Default Fill");
            }
        }
    
    }
}