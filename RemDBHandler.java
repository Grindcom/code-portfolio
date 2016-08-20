/******************
 * RemDBHandler
 * Code By: Greg Ford, B.Sc.
 * All Rights Reserved, 2016
 */
package com.facet_it.android.remind_me;

/**
 * Created by admin on 2016-04-13.
 */

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteException;
import android.database.sqlite.SQLiteOpenHelper;
import android.support.annotation.NonNull;
import android.widget.Toast;

import com.facet_it.android.remind_me.support.FilterSet;
import com.facet_it.android.remind_me.support.MemoryContent;
import com.facet_it.android.remind_me.support.ReminderContent;

import java.util.ArrayList;
import java.util.List;


public class RemDBHandler extends SQLiteOpenHelper
{
    private static int DATABASE_VERSION = 3;// Refers to the structure of the database
    private static final String DATABASE_NAME = "remind_me.db";// Must have .db extension
    private static final String REM_TABLE_NAME = "ReminderTable";
    private static final String MEM_TABLE_NAME = "MemoryTable";
    //
    // Make constant
    //
    private static final String ROW_ID = "_id";
    private static final String PLACE_REM_TYPE = "Place";
    private static final String PERSON_REM_TYPE = "Person";
    private static final String THING_REM_TYPE = "Thing";
    private static final String SENS_REM_TYPE = "Sensation";
    //
    // Reminder table column names

    public static String getRemColType()
    {
        return REM_COL_TYPE;
    }

    //
    private static final String REM_COL_TYPE = "ReminderType";
    private static final String REM_COL_NAME = "ReminderName";
    //
    // Memory table column names
    //
    private static final String MEM_COL_PER = "Person_id";
    private static final String MEM_COL_PLA = "Place_id";
    private static final String MEM_COL_THI = "Thing_id";
    private static final String MEM_COL_SEN = "Sensation_id";
    private static final String MEM_COL_MEMORY = "Memory";
    private static final String MEM_COL_BLOB = "Blob";
    //
    //
    //
    private static List<ReminderContent.ReminderRow> rowList = new ArrayList<ReminderContent
            .ReminderRow>();
    private static List<MemoryContent.MemRow> memList = new ArrayList<MemoryContent.MemRow>();

    public List<MemoryContent.MemRow> getMemList()
    {
        return memList;
    }

    public List<ReminderContent.ReminderRow> getRemList()
    {
        return rowList;
    }
    //
    // Must have constructor
    //

    /*********************************
     * @param context
     * @param name    is the database type and is hardcast
     * @param factory
     * @param version is the database version and is hardcast
     */
    public RemDBHandler(
            Context context, String name, SQLiteDatabase.CursorFactory factory, int
            version
    )
    {
        //
        super(context, DATABASE_NAME, factory, DATABASE_VERSION);
        //
    }
    //
    // Getters
    //

    public static String getPlacesType()
    {
        return PLACE_REM_TYPE;
    }

    //
    // Must override OnCreate and OnUpgrade
    //

    /**********************************************
     * @param db
     */
    @Override
    public void onCreate(SQLiteDatabase db)
    {
        try
        {
            //
            // Create Reminder Table
            //
            String query = "CREATE TABLE " + REM_TABLE_NAME + " (" +
                    ROW_ID + " INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    REM_COL_TYPE + " TEXT, " +
                    REM_COL_NAME + " TEXT UNIQUE" +
                    ");";
            // Execute query
            db.execSQL(query);
            //
            insertDefaultReminders(db);
            //
            // Create Memory Table
            //
            String query2 = "CREATE TABLE " + MEM_TABLE_NAME + " (" +
                    ROW_ID + " INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    MEM_COL_PER + " INTEGER DEFAULT 0, " +
                    MEM_COL_PLA + " INTEGER DEFAULT 0, " +
                    MEM_COL_THI + " INTEGER DEFAULT 0, " +
                    MEM_COL_SEN + " INTEGER DEFAULT 0, " +
                    MEM_COL_MEMORY + " TEXT, " +
                    MEM_COL_BLOB + " BLOB " +
                    ");";
            //
            db.execSQL(query2);
            //
        } catch (SQLException e)
        {
            e.printStackTrace();
        }
    }

    public void insertDefaultReminders(SQLiteDatabase db)
    {
        try
        {
            //
            // Insert Default Reminder Types
            //
            ReminderContent.ReminderRow remRow = new ReminderContent.ReminderRow(
                    "Person",
                    "Anyone who told you about or reminds you of the memory."
            );
            addRow(db, remRow);
            remRow = new ReminderContent.ReminderRow("Place", "The place your memory occured or " +
                    "that reminds you of the memory");
            addRow(db, remRow);
            remRow = new ReminderContent.ReminderRow("Thing", "A thing that reminds you of your " +
                    "memory or that is part of your memory");
            addRow(db, remRow);
            remRow = new ReminderContent.ReminderRow("Sensation", "Cold, sweet, hot... anything " +
                    "that reminds you of your memory");
            addRow(db, remRow);
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }
    //

    /*********************************
     * Called when upgrading to a newer version
     *
     * @param db
     * @param oldVersion
     * @param newVersion
     */
    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
    {
        try
        {
            // This will delete any data in the table!!!
            db.execSQL("DROP TABLE IF EXISTS '" + REM_TABLE_NAME + "'");
            db.execSQL("DROP TABLE IF EXISTS '" + MEM_TABLE_NAME + "'");
            // Create a new table with the upgraded structure
            onCreate(db);
        } catch (SQLException e)
        {
            e.printStackTrace();
        }
    }

    /*******************************************************
     * Add a reminder Row
     *
     * @param reminderRow
     * @return true for success
     */
    public boolean addRow(ReminderContent.ReminderRow reminderRow)
    {
        //
        SQLiteDatabase db = getWritableDatabase();// calls onCreate when opened the first time
        //
        boolean result = addRow(db, reminderRow);
        //
        db.close();
        return result;
    }

    /***********************************************
     * Add a new Reminder Row
     *
     * @param reminderRow
     * @return true if a row is added without error
     */
    private boolean addRow(SQLiteDatabase db, ReminderContent.ReminderRow reminderRow)
    {
        Boolean success = false;

        try
        {
            String type = reminderRow.getRemType();
            String name = reminderRow.getRemName();
            ContentValues values = new ContentValues();
            values.put(REM_COL_TYPE, type);
            values.put(REM_COL_NAME, name);

            //
            long result = db.insert(REM_TABLE_NAME, null, values);
            success = result > 0;
            //
        } catch (SQLiteException sqle)
        {
            sqle.printStackTrace();
        }
        return success;
    }

    public boolean updateRow(String rowID, ReminderContent.ReminderRow reminderRow)
    {
        Boolean success = false;

        try
        {
            ContentValues values = new ContentValues();
            values.put(REM_COL_TYPE, reminderRow.getRemType());
            values.put(REM_COL_NAME, reminderRow.getRemName());
            //
            SQLiteDatabase db = getWritableDatabase();// calls onCreate when opened the first time
            //
            long result = db.update(REM_TABLE_NAME, values, "_id=" + rowID, null);
            success = result > 0;
            //
            db.close();
        } catch (SQLiteException sqle)
        {
            sqle.printStackTrace();
        }
        return success;
    }

    /*********************************************
     * Add a new Memory Row
     *
     * @param memRow
     * @return true if a row is added without error
     */
    public Boolean addRow(MemoryContent.MemRow memRow)
    {
        Boolean success = false;

        try
        {
            ContentValues values = new ContentValues();
            values.put(MEM_COL_PER, memRow.getPerson_id());
            values.put(MEM_COL_PLA, memRow.getPlace_id());
            values.put(MEM_COL_THI, memRow.getThing_id());
            values.put(MEM_COL_SEN, memRow.getSensation_id());
            values.put(MEM_COL_MEMORY, memRow.getMemory());
            //
            SQLiteDatabase db = getWritableDatabase();// calls onCreate when opened the first time
            //
            long result = db.insert(MEM_TABLE_NAME, null, values);
            success = result > 0;
            //
            db.close();
        } catch (SQLiteException sqle)
        {
            sqle.printStackTrace();
        }
        return success;
    }

    /************************************************
     * update  Memory Row at
     *
     * @param rowID
     * @param memRow without row id
     * @return true if successful
     */
    public Boolean updateRow(String rowID, MemoryContent.MemRow memRow)
    {
        Boolean success = false;
        try
        {
            ContentValues values = new ContentValues();
            values.put(MEM_COL_PER, memRow.getPerson_id());
            values.put(MEM_COL_PLA, memRow.getPlace_id());
            values.put(MEM_COL_THI, memRow.getThing_id());
            values.put(MEM_COL_SEN, memRow.getSensation_id());
            values.put(MEM_COL_MEMORY, memRow.getMemory());
            //
            SQLiteDatabase db = getWritableDatabase();// calls onCreate when opened the first time
            //
            long result = db.update(MEM_TABLE_NAME, values, "_id=" + rowID, null);
            success = result > 0;
            //
            db.close();
        } catch (SQLiteException sqle)
        {
            sqle.printStackTrace();
        }
        return success;
    }

    /***********************************************
     * Delete Reminder Row at
     *
     * @param row_id
     * @return returns true if any row's are deleted.
     */
    public Boolean deleteRemRow(String row_id)
    {
        int result = 0;
        try
        {
            SQLiteDatabase db = getWritableDatabase();
            //
            result = db.delete(REM_TABLE_NAME, ROW_ID + "=" + row_id, null);
            //
            db.close();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return result > 0;
    }

    public void deleteMemRow(String row_id)
    {

    }

    //
    // Delete row
    //
    public void deleteRow(String table, String row_id)
    {
        try
        {
            SQLiteDatabase db = getWritableDatabase();
            //
            db.execSQL("DELETE FROM " + table + " WHERE " +
                    ROW_ID + "=\"" + row_id + "\";"
            );
            //
            db.close();
        } catch (SQLException e)
        {
            e.printStackTrace();
        }
    }


    /***************************************************
     * Reset a table to no data
     */
    public void resetTable()
    {
        SQLiteDatabase db = getWritableDatabase();
        //
        // Drop all tables in the database
        //
        db.execSQL("DROP TABLE IF EXIST " + REM_TABLE_NAME);
        db.execSQL("DROP TABLE IF EXIST " + MEM_TABLE_NAME);
        //
        onCreate(db);
        //
        db.close();
    }
    //
    // Get database
    //

    /****************************************
     * Convert whole table to a string, columns are
     * separated by a ',' and rows are separated by a '\n'
     *
     * @return String containing all rows in the default table
     */
    public String remTableToString()
    {
        String dbString = null;
        try
        {
            dbString = "";
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            String query = "SELECT * FROM " + REM_TABLE_NAME + " WHERE 1";// select all rows
            // Cursor points to location in the results
            Cursor c = db.rawQuery(query, null);
            //
            c.moveToFirst();
            //
            while (!c.isAfterLast())
            {
                if (c.getString(c.getColumnIndex(REM_COL_NAME)) != null)
                {
                    dbString += c.getString(c.getColumnIndex(ROW_ID));
                    dbString += ",";
                    dbString += c.getString(c.getColumnIndex(REM_COL_TYPE));
                    dbString += ",";
                    dbString += c.getString(c.getColumnIndex(REM_COL_NAME));
                    dbString += "\n";

                }
                c.moveToNext();
            }

            //
            db.close();
        } catch (SQLException sqle)
        {
            sqle.printStackTrace();

        } catch (Exception e)
        {
            e.printStackTrace();
            //
            Toast.makeText(
                    null, "databaseToString: " + e.getMessage(),
                    Toast.LENGTH_LONG
            ).show();
        }
//
        return dbString;
    }


    /************************************************************************
     * @param remFilter
     * @return
     */
    public ArrayList<String> getRemTableData(String remFilter)
    {
        // Clear remRow before filling
        rowList.clear();
        //
        List<String> colList = new ArrayList<String>();

        ReminderContent.ReminderRow tempRow = new ReminderContent.ReminderRow();
        try
        {
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            String query = "SELECT * FROM " + REM_TABLE_NAME + " WHERE " +
                    REM_COL_TYPE + "=\"" + remFilter + "\" ORDER BY " +
                    ROW_ID + " ASC;";// select all rows
            // Cursor points to location in the results
            Cursor c = db.rawQuery(query, null);
            //
            c.moveToFirst();
            //
            while (!c.isAfterLast())
            {
                if (c.getString(c.getColumnIndex(REM_COL_NAME)) != null)
                {
                    tempRow.set_id(c.getInt(c.getColumnIndex(ROW_ID)));
                    tempRow.setRemType(c.getString(c.getColumnIndex(REM_COL_TYPE)));
                    tempRow.setRemName(c.getString(c.getColumnIndex(REM_COL_NAME)));
                    //Add Row to list
                    rowList.add(tempRow);
                    //

                    colList.add(c.getString(c.getColumnIndex(REM_COL_NAME)));

                }
                c.moveToNext();
                tempRow = null; //attempt to release memory
                tempRow = new ReminderContent.ReminderRow();// Do athis, otherwis all rows in
                // rowList become the same
            }

            //
            db.close();
            tempRow = null;
        } catch (SQLException sqle)
        {
            sqle.printStackTrace();

        }
        return (ArrayList<String>) colList;
    }

    public ArrayList<String> getReminderTypeList()
    {
        String query = "SELECT DISTINCT ReminderType FROM " + REM_TABLE_NAME;
        //
        ArrayList<String> resultList = new ArrayList<String>();

        try
        {
            //
            String _type = "default";
            //
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);
            //
            c.moveToFirst();
            //
            while (!c.isAfterLast() && c != null)
            {

                _type = c.getString(c.getColumnIndex(REM_COL_TYPE));
                //
                resultList.add(_type);
                //
                c.moveToNext();
            }
            //
            db.close();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return resultList;

    }

    /***************************************************************************
     * Search Reminder Table
     *
     * @param type filter by type
     * @param rem  reminder to search for
     * @return an array containing the result
     */
    public ArrayList<String> searchRemTableData(String type, String rem)
    {
        // Clear remRow before filling
        rowList.clear();
        //
        List<String> colList = new ArrayList<String>();
        ReminderContent.ReminderRow tempRow = new ReminderContent.ReminderRow();
        //
        try
        {
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            String query = "SELECT * FROM " + REM_TABLE_NAME + " WHERE (" +
                    REM_COL_TYPE + " LIKE \"" + type + "\") AND (" +
                    REM_COL_NAME + " LIKE \"" + rem + "\")" +
                    ";";// select all rows
            // Cursor points to location in the results
            Cursor c = db.rawQuery(query, null);
            //
            c.moveToFirst();
            //
            while (!c.isAfterLast())
            {
                if (c.getString(c.getColumnIndex(REM_COL_NAME)) != null)
                {
                    tempRow.set_id(c.getInt(c.getColumnIndex(ROW_ID)));
                    tempRow.setRemType(c.getString(c.getColumnIndex(REM_COL_TYPE)));
                    tempRow.setRemName(c.getString(c.getColumnIndex(REM_COL_NAME)));
                    //Add Row to list
                    rowList.add(tempRow);
                    // Add type to column list
                    colList.add(c.getString(c.getColumnIndex(REM_COL_NAME)));

                }
                c.moveToNext();
                tempRow = null; //attempt to release memory
                tempRow = new ReminderContent.ReminderRow();
            }

            //
            db.close();
            tempRow = null; //attemt to release memory
        } catch (SQLException sqle)
        {
            sqle.printStackTrace();

        }
        return (ArrayList<String>) colList;
    }

    /*********************************************************************
     * Get a ReminderContent result, filtered by the type
     *
     * @param type
     * @return ReminderContent
     */
    public ReminderContent getReminderMap(String type)
    {
        try
        {
            String query = "SELECT * FROM " + REM_TABLE_NAME +
                    " WHERE ReminderType=\'" + type + "\';";
            //
            // Get a database
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);
            //
//            c.moveToFirst();
            //
            ReminderContent resultList = new ReminderContent();
            // Make sure rows are clear
            resultList.clearRows();
            //
            ReminderContent.ReminderRow remRow = new ReminderContent.ReminderRow();
            int i = resultList.ROW_ITEMS.size();//
            //

            while (c.moveToNext())
            {
                remRow.set_id(c.getInt(c.getColumnIndex(ROW_ID)));
                remRow.setRemType(c.getString(c.getColumnIndex(REM_COL_TYPE)));
                remRow.setRemName(c.getString(c.getColumnIndex(REM_COL_NAME)));
                // Add row to map
                resultList.addItem(remRow);
                //

                //
                //                memRow = null;
                remRow = new ReminderContent.ReminderRow();
            }
            //
            db.close();
            //
            return resultList;
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return null;
    }

    /*************************************************************
     * Get a MemoryContent result, containing the memories reference by filter
     *
     * @param filter
     * @return MemoryContent with all memories from the filter
     */
    public MemoryContent getMemoryMap(@NonNull FilterSet filter)
    {
        MemoryContent resultList = null;//

        try
        {
            Boolean addWhere = false;
            //
            // Initiate filter queries
            //
            String person_filter_query = "";
            String place_filter_query = "";
            String thing_filter_query = "";
            String sen_filter_query = "";
            int andCount = 0;// used to add enough 'AND's to the query
            List<String> queryList = new ArrayList<String>();
            //
            // Check value of Person ID
            //
            if (filter.getPERSONid() != 0)
            {
                person_filter_query = "(" + MEM_COL_PER + "=" + filter.getPERSONid().toString() +
                        ")";
                queryList.add(person_filter_query);
            }
            //
            // Check value of Place ID and set query
            //
            if (filter.getPLACEid() != 0)
            {
                place_filter_query = "(" + MEM_COL_PLA + "=" + filter.getPLACEid().toString() + ")";

                queryList.add(place_filter_query);
            }
            //
            // Check value of Thing ID and set query
            //
            if (filter.getTHINGid() != 0)
            {
                thing_filter_query = "(" + MEM_COL_THI + "=" + filter.getTHINGid().toString
                        () + ")";
                queryList.add(thing_filter_query);
            }
            // Check value of Sensation ID and set query
            //
            if (filter.getSENSATIONid() != 0)
            {
                sen_filter_query = "(" + MEM_COL_SEN + "=" + filter.getSENSATIONid()
                        .toString() + ")";
                queryList.add(sen_filter_query);
            }
            //
            // Set up filter query
            //
            String query = "SELECT * FROM " + MEM_TABLE_NAME;
            //
            if (queryList.size() > 0)
            {
                query = query + " WHERE " + queryList.get(0);
                //
                if (queryList.size() > 1)
                {
                    for (int i = 1; i < queryList.size(); i++)
                    {
                        query += " AND " + queryList.get(i);
                    }
                }

            }
            //
            // Get a database
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);
            //
            c.moveToFirst();
            //
            resultList = new MemoryContent();
            MemoryContent.MemRow memRow = new MemoryContent.MemRow();
            int i = 1;//
            //
            while (!c.isAfterLast())
            {
                if (c.getString(c.getColumnIndex(MEM_COL_MEMORY)) != null)
                {
                    memRow.set_id(c.getInt(c.getColumnIndex(ROW_ID)));
                    memRow.setPerson_id(c.getInt(c.getColumnIndex(MEM_COL_PER)));
                    memRow.setPlace_id(c.getInt(c.getColumnIndex(MEM_COL_PLA)));
                    memRow.setThing_id(c.getInt(c.getColumnIndex(MEM_COL_THI)));
                    memRow.setSensation_id(c.getInt(c.getColumnIndex(MEM_COL_SEN)));
                    memRow.setMemory(c.getString(c.getColumnIndex(MEM_COL_MEMORY)));
                    // Add row to map
                    resultList.addItem(memRow);
                    //
                }
                c.moveToNext();
                //
//                memRow = null;
                memRow = new MemoryContent.MemRow();
            }
            //
            db.close();
            //
            memRow = null;// release memory
        } catch (SQLException sqle)
        {
            sqle.printStackTrace();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        //
        return resultList;
    }

    public static int getDatabaseVersion()
    {
        return DATABASE_VERSION;
    }

    public static String getMemTableName()
    {
        return MEM_TABLE_NAME;
    }

    public static String getPersonRemType()
    {
        return PERSON_REM_TYPE;
    }

    public static String getPlaceRemType()
    {
        return PLACE_REM_TYPE;
    }

    public static String getRemTableName()
    {
        return REM_TABLE_NAME;
    }

    public static String getSensRemType()
    {
        return SENS_REM_TYPE;
    }

    public static String getThingRemType()
    {
        return THING_REM_TYPE;
    }

    public static String getRemColName()
    {
        return REM_COL_NAME;
    }

    /**
     * Get a Row from the Memory table by the row id
     *
     * @param row_id as a string value
     * @return the memory row at row_id, enpty if none.
     */
    public MemoryContent.MemRow getMemRow(int row_id)
    {
        MemoryContent.MemRow memRow = new MemoryContent.MemRow();

        try
        {
            String query = "SELECT * FROM " + MEM_TABLE_NAME +
                    " WHERE _id=" + Integer.toString(row_id);
            //
            // Get a database
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);

            //
            if (c.getCount() > 0)
            {
                c.moveToFirst();
                //
                int id = c.getColumnIndex(ROW_ID);
                int c_rowid = c.getInt(id);
                memRow.set_id(c_rowid);//

                memRow.setPerson_id(c.getInt(c.getColumnIndex(MEM_COL_PER)));
                memRow.setPlace_id(c.getInt(c.getColumnIndex(MEM_COL_PLA)));
                memRow.setThing_id(c.getInt(c.getColumnIndex(MEM_COL_THI)));
                memRow.setSensation_id(c.getInt(c.getColumnIndex(MEM_COL_SEN)));
                memRow.setMemory(c.getString(c.getColumnIndex(MEM_COL_MEMORY)));
            }
            //
            db.close();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return memRow;
    }

    public ReminderContent.ReminderRow getRemRow(String row_id)
    {
        ReminderContent.ReminderRow reminderRow = null;
        try
        {
            String query = "SELECT * FROM " + REM_TABLE_NAME + " WHERE _id=" + row_id;
            //
            // Get a database
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);

            //
            if (c.moveToFirst())
            {
                //
                reminderRow = new ReminderContent.ReminderRow();
                int id = c.getColumnIndex(ROW_ID);
                int c_rowid = c.getInt(id);
                reminderRow.set_id(c_rowid);//
                reminderRow.setRemName(c.getString(c.getColumnIndex(REM_COL_NAME)));
                reminderRow.setRemType(c.getString(c.getColumnIndex(REM_COL_TYPE)));
            }
            //
            db.close();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return reminderRow;
    }

    /*********************
     * Get the reminder type by
     *
     * @param id
     * @return reminder type
     */
    public String getRemNameByID(int id)
    {
        String name = "";

        try
        {
            String query = "SELECT ReminderName FROM " + REM_TABLE_NAME + " WHERE _id=" + id;
            //
            // Get a database
            //
            SQLiteDatabase db = getWritableDatabase();
            //
            Cursor c = db.rawQuery(query, null);
            //
            if (c.getCount() > 0)
            {
                c.moveToFirst();
                //
                name = c.getString(c.getColumnIndex(REM_COL_NAME));
            }
            db.close();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        //
        return name;
    }

}
