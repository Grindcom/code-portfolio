/******************
 * ReminderDetailActivity
 * Code By: Greg Ford, B.Sc.
 * All Rights Reserved, 2016
 */
package com.facet_it.android.remind_me;

import android.content.DialogInterface;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.design.widget.CollapsingToolbarLayout;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AlertDialog;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.Toolbar;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.app.ActionBar;
import android.view.MenuItem;
import android.view.ViewGroup;
import android.view.inputmethod.InputMethodManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.ListAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.facet_it.android.remind_me.support.ReminderContent;
import com.google.android.gms.appindexing.Action;
import com.google.android.gms.appindexing.AppIndex;
import com.google.android.gms.common.api.GoogleApiClient;

import java.util.ArrayList;
import java.util.List;

/**
 * An activity representing a single Reminder detail screen. This
 * activity is only used narrow width devices. On tablet-size devices,
 * item details are presented side-by-side with a list of items
 * in a {@link ReminderMainActivity}.
 */
public class ReminderDetailActivity extends AppCompatActivity
{
    public final static String ACTIVITY_ID = "REMINDER_DETAIL_ACTIVITY";
    public final static String COLUMN_ID = "column_id";
    public final static String SELECT_ID = "select";
    public final static String POSITION_ID = "position";
    public static final String HINT_ID = "hint_id";
    public static final String REM_ID = "rem_id";

    // Edit Text Hints
    public final String PROMPT_ADD = "Enter New Reminder";
    public final String PROMPT_EDIT = "Edit Reminder";
    public final String PROMPT_SEARCH = "Search Reminders";
    //
    private static Boolean editing = false;
    private static int edit_row_id = 0;

    // Reminder Row ID
    private String remRow_id = null;
    private int selected_remRow_id = 0;
    // Reminder Type
    private String remType = null;
    // Reminder hint description
    public String reminderHint = "No Hint Available";
    //
    private static RecyclerView mRecyclerView;
    private static ReminderDetailRecyclerViewAdapter mReminderDetailRecyclerViewAdapter;
    private static ReminderContent reminderContent;

    //
    private EditText reminderInputText;
    //
    private RemDBHandler dbHandler;
    //
    // Touch Variables
    private float prevX = Float.NaN;
    private float prevY = Float.NaN;
    static final int DELTA = 50;

    enum DIRECTION
    {
        NO_SWIPE, LEFT, RIGHT
    }

    private DIRECTION swipeDirection = DIRECTION.NO_SWIPE;

    //
    enum EDITMODE
    {
        SEARCH, EDIT, ADD
    }

    private EDITMODE eMode = EDITMODE.SEARCH;
    //
    /**
     * ATTENTION: This was auto-generated to implement the App Indexing API.
     * See https://g.co/AppIndexing/AndroidStudio for more information.
     */
    private GoogleApiClient client;

    /*********************************************************************************
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.reminder_detail_activity);// //old_reminder_detail_activity
        Toolbar toolbar = (Toolbar) findViewById(R.id.detail_toolbar);
        setSupportActionBar(toolbar);
        toolbar.setLogo(R.mipmap.ic_launcher);
        //
        //
        initReminderInputText();
        //
        //
        //
        dbHandler = new RemDBHandler(this, null, null, 1);
        // Show the Up button in the action bar.
        ActionBar actionBar = getSupportActionBar();
        if (actionBar != null)
        {
            actionBar.setDisplayHomeAsUpEnabled(true);

        }
        //
        // added from ReminderDetailFragment
        //
        CollapsingToolbarLayout appBarLayout = (CollapsingToolbarLayout) findViewById(R.id
                .toolbar_layout);

        // savedInstanceState is non-null when there is fragment state
        // saved from previous configurations of this activity
        // (e.g. when rotating the screen from portrait to landscape).
        // In this case, the fragment will automatically be re-added
        // to its container so we don't need to manually add it.
        // For more information, see the Fragments API guide at:
        //
        // http://developer.android.com/guide/components/fragments.html
        //
        if (savedInstanceState == null)
        {
            // Create the detail fragment and add it to the activity
            // using a fragment transaction.
            final Bundle reminderListExtras = getIntent().getExtras();//new Bundle();

            if (reminderListExtras != null)
            {
                try
                {
                    //
                    // Get the first argument sent to this bundle
                    //
                    final String arg1 = reminderListExtras.getString(ReminderDetailFragment
                            .ARG_ITEM_ID);
                    remRow_id = arg1;
                    //
                    // Get the second argument sent to this bundle
                    //
                    final String arg2 = reminderListExtras.getString(COLUMN_ID);
                    remType = arg2;
                    if (appBarLayout != null)
                    {
                        appBarLayout.setTitle(remType);
                    }
                    //
                    // Get the third argument
                    final String arg3 = reminderListExtras.getString(HINT_ID);
                    this.reminderHint = arg3;
                    //
//                    reminderListExtras.putString(ReminderDetailFragment.ARG_ITEM_ID, arg1);
//                    //
//                    ReminderDetailFragment fragment = new ReminderDetailFragment();
//                    //
//                    // Send extras to fragment
//                    //
//                    fragment.setArguments(reminderListExtras);
//                    // add the reminder detail container to the fragment
//                    getSupportFragmentManager().beginTransaction()
//                            .add(R.id.reminder_detail_content, fragment)
//                            .commit();
                    //
                } catch (Exception e)
                {
                    e.printStackTrace();
                }

            }
            init_floating_action_buttons();

            //
            // Initiate List view
            //
//            InitRemListView(remType);
            // Initialize the Recycler View
            initReminderDetailRecyclerView(remType);


        }
        // ATTENTION: This was auto-generated to implement the App Indexing API.
        // See https://g.co/AppIndexing/AndroidStudio for more information.
        client = new GoogleApiClient.Builder(this).addApi(AppIndex.API).build();
    }

    /**
     * Initialize the activities action buttons
     */
    private void init_floating_action_buttons()
    {
        //
        // Set up the hint button
        //
        FloatingActionButton clrFilter_fab = (FloatingActionButton) findViewById(R.id
                .clrFilter_fab);
        clrFilter_fab.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                clearFilter(view);
            }
        });
        FloatingActionButton newRem_fab = (FloatingActionButton) findViewById(R.id.newRem_fab);
        assert newRem_fab != null;
        newRem_fab.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                //addReminder(view);
                Toast.makeText(ReminderDetailActivity.this, "Now in Add Reminder Mode", Toast
                        .LENGTH_SHORT).show();
                //
                setEditLayoutVisible(true);
                //
                reminderInputText.setText(PROMPT_ADD);
                //
                eMode = EDITMODE.ADD;
                //
                reminderInputText.requestFocus();
                //
                reminderInputText.selectAll();
                //
                toggleKeyboard(view);
            }

        });
        //
        FloatingActionButton searchRem_fab = (FloatingActionButton) findViewById(R.id
                .searchRem_fab);
        searchRem_fab.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                //searchReminder(v);
                Toast.makeText(ReminderDetailActivity.this, "Now in Search Mode", Toast
                        .LENGTH_LONG).show();
                //
                //
                setEditLayoutVisible(true);
                //
                reminderInputText.setText(PROMPT_SEARCH);
                //
                eMode = EDITMODE.SEARCH;
                //
                reminderInputText.requestFocus();
                //
                reminderInputText.selectAll();
                //
                toggleKeyboard(v);
            }
        });
    }

    /***************************************************************
     * Set the Edit Layout Visible
     *
     * @param _visible true to make visible
     */
    private void setEditLayoutVisible(Boolean _visible)
    {
        LinearLayout layout = (LinearLayout) findViewById(R.id.editLayout);
        if (_visible)
        {
            layout.setVisibility(View.VISIBLE);
            //
            // Set the add fab icon to normal
            //
            FloatingActionButton newRem_fab = (FloatingActionButton) findViewById(R.id.newRem_fab);
            newRem_fab.setImageDrawable(ContextCompat.getDrawable(
                    this, android.R.drawable.ic_input_add));
        }
        else
        {
            layout.setVisibility(View.INVISIBLE);
            LinearLayout layout1 = (LinearLayout) findViewById(R.id.reminder_layout);
            layout1.requestFocus();
        }


    }

    /****************************************************
     * Initialize Reminder Input Text
     */
    private void initReminderInputText()
    {
        reminderInputText = (EditText) findViewById(R.id.reminderInputText);
        assert reminderInputText != null;
/*        reminderInputText.setOnTouchListener(new View.OnTouchListener()
        {
            @Override
            public boolean onTouch(View v, MotionEvent event)
            {
                return false;
            }
        });*/
        reminderInputText.setOnKeyListener(new View.OnKeyListener()
        {
            @Override
            public boolean onKey(View v, int keyCode, KeyEvent event)
            {
                if ((event.getAction() == KeyEvent.ACTION_UP) &&
                        (keyCode == KeyEvent.KEYCODE_ENTER))
                {
                    switch (eMode)
                    {
                        case EDIT: // if in edit mode
                            editReminder(v);
                            break;
                        case ADD:// if in add mode
                            addReminder(v);
                            break;
                        case SEARCH:// if in search mode
                            searchReminder(v);
                            break;
                        default:
                            break;
                    }
                    //
                    toggleKeyboard(v);
//
                    restoreNormalView();
                    //
                    return true;
                }

                return false;
            }

        });
        //
        // Set edit text reminder
        //
        reminderInputText.setText(PROMPT_SEARCH);
    }

    /***********************************
     * Toggle the keyboard show or hide
     *
     * @param v
     */
    private boolean toggleKeyboard(View v)
    {
        InputMethodManager imm = (InputMethodManager) getSystemService(
                v.getContext().INPUT_METHOD_SERVICE);

        imm.toggleSoftInput(InputMethodManager.SHOW_FORCED, 0);
        //
        Boolean status = imm.isActive();
        return status;
    }

    /*****************************************************************************
     * Initialize the column list view, also creates an onclick listener
     *
     * @param type reminder type to filter for and show column
     */
    private void InitRemListView(String type)
    {
        try
        {
            ListView columnListView = getRemListView(type);
            assert columnListView != null;
//
            columnListView.requestFocus();
            //
            columnListView.setOnTouchListener(new View.OnTouchListener()
            {
                @Override
                public boolean onTouch(View v, MotionEvent event)
                {
                    switch (event.getAction())
                    {
                        case MotionEvent.ACTION_DOWN:
                            prevX = event.getX();
                            prevY = event.getY();
                            break;
                        case MotionEvent.ACTION_UP:
                            if (event.getX() - prevX < -DELTA)
                            {
                                swipeDirection = DIRECTION.LEFT;
                            }
                            else if (event.getX() - prevX > DELTA)
                            {
                                swipeDirection = DIRECTION.RIGHT;
//                                return true;// cause click listenter to ignore event
                            }
                            else
                            {
                                swipeDirection = DIRECTION.NO_SWIPE;
                            }

                            break;

                        default:
                            swipeDirection = DIRECTION.NO_SWIPE;
                            return false;
                    }
                    return false;
                }
            });
            //
            columnListView.setOnItemClickListener(
                    new AdapterView.OnItemClickListener()
                    {

                        @Override
                        public void onItemClick(
                                AdapterView<?> parent, View view, int position,
                                long id
                        )
                        {
                            try
                            {
                                String reminder = String.valueOf(parent.getItemAtPosition
                                        (position));
                                final int tmpPos = position;

                                switch (swipeDirection)
                                {
                                    case NO_SWIPE:
                                        eMode = EDITMODE.SEARCH;
                                        //
                                        Toast.makeText(view.getContext(), reminder, Toast
                                                .LENGTH_LONG).show();
                                        //
                                        Intent intent = new Intent(
                                                view.getContext(),
                                                ReminderMainActivity.class
                                        );
                                        //
                                        intent.putExtra(ACTIVITY_ID, true);
                                        //indicate
                                        // there are
                                        // extras
                                        String selection = (String) parent.getAdapter().getItem
                                                (position);
                                        intent.putExtra(SELECT_ID, selection);
                                        intent.putExtra(POSITION_ID, remRow_id);
                                        //
                                        intent.putExtra(REM_ID, dbHandler.getRemList().get(position)
                                                .get_id());
                                        //
                                        navigateUpTo(intent);

                                        break;
                                    case RIGHT:
                                        Snackbar.make(view, "Edit: " + reminder, Snackbar
                                                .LENGTH_LONG)
                                                .setAction("Action", null).show();
                                        // Set to edit mode
                                        eMode = EDITMODE.EDIT;

                                        // Set the selected reminder row
                                        selected_remRow_id = dbHandler.getRemList().get(tmpPos)
                                                .get_id();
                                        // Set the input text hint message
                                        String remToEdit = dbHandler.getRemList().get(tmpPos)
                                                .getRemName();
                                        reminderInputText.setText(remToEdit);
                                        //
                                        toggleKeyboard(view);
                                        break;
                                    case LEFT:
                                        eMode = EDITMODE.SEARCH;
                                        // Confirm delete with a dialog
//                                        AlertDialog.Builder builder1 = new AlertDialog.Builder
//                                                (view.getContext());
//                                        builder1.setMessage("Delete: " + reminder);
//                                        builder1.setCancelable(true);
//
//                                        builder1.setPositiveButton(
//                                                "Yes",
//                                                new DialogInterface.OnClickListener()
//                                                {
//                                                    public void onClick(
//                                                            DialogInterface dialog,
//                                                            int id
//                                                    )
//                                                    {
//                                                        dialog.cancel();
//                                                        // if yes, delete reminder at position
//                                                        int row = dbHandler.getRemList().get
//                                                                (tmpPos).get_id();
//                                                        dbHandler.deleteRemRow(String.valueOf
// (row));
//                                                        // Refresh listview
//                                                        InitRemListView(remType);
//
//                                                    }
//                                                }
//                                        );
//
//                                        builder1.setNegativeButton(
//                                                "No",
//                                                new DialogInterface.OnClickListener()
//                                                {
//                                                    public void onClick(
//                                                            DialogInterface dialog,
//                                                            int id
//                                                    )
//                                                    {// Do nothing
//                                                        dialog.cancel();
//                                                    }
//                                                }
//                                        );
//
//                                        AlertDialog dialog = builder1.create();
//                                        dialog.show();

                                        break;
                                    default:
                                        break;
                                }

                            } catch (Exception e)
                            {
                                e.printStackTrace();
                            }

                        }
                    }
            );
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /*************************************************************
     * Clear the filter and return to main
     *
     * @param view
     */
    private void clearFilter(View view)
    {
        try
        {
            Snackbar.make(view, reminderHint, Snackbar.LENGTH_LONG)
                    .setAction("Action", null).show();
            Intent intent = new Intent(view.getContext(), ReminderMainActivity.class);
            intent.putExtra(ACTIVITY_ID, true);//indicate
            // there are
            // extras
            intent.putExtra(SELECT_ID, "NA");//Clear filter
            intent.putExtra(POSITION_ID, remRow_id);//
            navigateUpTo(intent);
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /**************************************************************
     * Make a new Reminder entry in the table
     *
     * @param view
     */
    private void addReminder(View view)
    {

        try
        {
            Snackbar.make(view, "Adding new reminder", Snackbar.LENGTH_LONG)
                    .setAction("Action", null).show();
            //
            // Create new reminder entry
            //
            String input = reminderInputText.getText().toString();
            if (input.equals("Enter New Reminder"))
            {
                Toast.makeText(ReminderDetailActivity.this, "Enter a reminder", Toast
                        .LENGTH_LONG).show();
                return;
            }
            //
            ReminderContent.ReminderRow row = new ReminderContent.ReminderRow(remType, input);
            if (!dbHandler.addRow(row))
            {
                Toast.makeText(ReminderDetailActivity.this, "Unable to add Reminder", Toast
                        .LENGTH_SHORT).show();
                return;
            }
            //
            // Refresh List view
            //
//            ListView remListView = getRemListView(remType);
//            int len = remListView.getCount();
//            remListView.setItemChecked(len, true);
//            //
//            // Highlight the new Reminder
//            //
//            remListView.setSelection(Integer.parseInt(remRow_id));
            initReminderDetailRecyclerView(remType);

        } catch (Exception e)
        {
            e.printStackTrace();
        }

    }

    /*****************************************************
     * Search the current row for the text in the input text
     *
     * @param view
     */
    private void searchReminder(View view)
    {
        try
        {
            String input = reminderInputText.getText().toString();
            if (input.equals("Enter New Reminder"))
            {
                Toast.makeText(view.getContext(), "Enter a reminder", Toast
                        .LENGTH_LONG).show();
                return;
            }
            ArrayList<String> list = dbHandler.searchRemTableData(remType, input);
            //
            reminderContent.ROW_ITEMS = dbHandler.getRemList();
            //
            refreshReminderAdapter(reminderContent);
            //
//            setSearchRemListView(list);
            //
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /*******************************************************
     * Edit the selected reminder
     *
     * @param v
     */
    private void editReminder(View v)
    {
        try
        {
            // Change selected row id to String
            String id = Integer.toString(selected_remRow_id);
            // Get the new reminder from input
            String new_reminder = reminderInputText.getText().toString();
            // Get the current row
            ReminderContent.ReminderRow row = dbHandler.getRemRow(id);
            // change the current reminder to the new one
            row.setRemName(new_reminder);
            // Update the row
            dbHandler.updateRow(id, row);
        } catch (Exception e)
        {
            e.printStackTrace();
        }

    }


    /*******************************************************
     * Get a ListView from the Reminder Table
     *
     * @param type filter for listView
     * @return ListView containing the column list of type, Null if there is a problem
     */
    @NonNull
    private ListView getRemListView(String type)
    {
        ;
        try
        {
            // get column data
//            String[] colData = {"Van", "Mt. Robson", "The Puddle", "PG", "Rodeo"};
            List<String> colList = dbHandler.getRemTableData(type);
            // Filter list

            // create list adapter
            ListAdapter columnAdapter = new ArrayAdapter<String>(ReminderDetailActivity.this,
                    android.R.layout.simple_list_item_1, colList
            );

            // get the list view reference from the design view
            ListView columnListView = (ListView) findViewById(R.id.columnListView);
            //
            // place the column data intor the list view
            columnListView.setAdapter(columnAdapter);
            //
            columnListView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
            //
            return columnListView;
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return null;
    }

    private void setSearchRemListView(ArrayList<String> list)
    {
        try
        {
            // Clear the reminder content rows
            reminderContent.clearRows();
            //
            reminderContent.ROW_ITEMS = dbHandler.getRemList();
            // create list adapter
//            ListAdapter columnAdapter = new ArrayAdapter<String>(ReminderDetailActivity.this,
//                    android.R.layout.simple_list_item_1, list
//            );

//            // get the list view reference from the design view
//            ListView columnListView = (ListView) findViewById(R.id.columnListView);
//            // place the column data into the list view
//            columnListView.setAdapter(columnAdapter);
            //
            // Set the recyclerview to the new list
            //

        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    private void initReminderDetailRecyclerView(String _type)
    {
        View reminder_list_view = findViewById(R.id.reminder_list_view);
        assert reminder_list_view != null;
        //
        reminderContent = dbHandler.getReminderMap(_type);
        //
        setupReminderDetailRecyclerView((RecyclerView) reminder_list_view, reminderContent);

    }

    private void setupReminderDetailRecyclerView(
            RecyclerView _recyclerView, ReminderContent items
    )
    {
        try
        {
            mReminderDetailRecyclerViewAdapter = new ReminderDetailRecyclerViewAdapter(items);
            mRecyclerView = _recyclerView;
            mRecyclerView.setAdapter(mReminderDetailRecyclerViewAdapter);
            //
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    private void refreshReminderAdapter(ReminderContent _items)
    {
        try
        {
            mRecyclerView.setAdapter(mReminderDetailRecyclerViewAdapter);
            //
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public class ReminderDetailRecyclerViewAdapter extends RecyclerView
            .Adapter<ReminderDetailRecyclerViewAdapter.ViewHolder>
    {
        private static final int DELTA = 50;// Distance before swipe gesture is recognized
        private final ReminderContent mValues;
        //
        //
        // Touch Variables
        private float prevX = Float.NaN;
        private float prevY = Float.NaN;
        private DIRECTION swipeDirection = DIRECTION.NO_SWIPE;

        public ReminderDetailRecyclerViewAdapter(ReminderContent items)
        {
            mValues = items;
        }

        @Override
        public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.from(parent.getContext()).inflate(R.layout
                    .reminder_detail_content, parent, false);
            ViewHolder viewHolder = new ViewHolder(view);
            return viewHolder;
        }

        @Override
        public void onBindViewHolder(final ViewHolder holder, final int position)
        {
            // Set the current holder's memory text
            holder.mContentView.setText(mValues.ROW_ITEMS.get(position).getRemName());
            // Set the current holder's item row id
            holder.rowID = mValues.ROW_ITEMS.get(position).get_id();
            // Set the current holders image


            //
            holder.mView.setOnLongClickListener(
                    new View.OnLongClickListener()
                    {
                        @Override
                        public boolean onLongClick(View v)
                        {
                            /******************************
                             * This if statement is a work around;
                             * mEditing switches back to false for some reason.
                             * Refer to ReminderMainActivity line 729 for
                             * correct version.
                             */
                            if (editing && edit_row_id == holder.rowID)
                            {
                                holder.mEditing = true;
                            }
                            //
                            holder.mEditing = !holder.mEditing;
                            //
                            FloatingActionButton newRem_fab = (FloatingActionButton) findViewById
                                    (R.id.newRem_fab);
//
                            if (holder.mEditing)
                            {// indicate editing with fab icon change
                                newRem_fab.setImageDrawable(ContextCompat.getDrawable(
                                        v.getContext(), android.R.drawable.ic_menu_save)
                                );
                                //
                                // Hide other fabs
                                //
                                FloatingActionButton search_fab = (FloatingActionButton)
                                        findViewById(R.id.searchRem_fab);
                                FloatingActionButton clr_fab = (FloatingActionButton)
                                        findViewById(R.id.clrFilter_fab);
                                //
                                search_fab.setVisibility(View.INVISIBLE);
                                clr_fab.setVisibility(View.INVISIBLE);
                                //
                                // Show the edit layout
                                //
                                LinearLayout layout = (LinearLayout) findViewById(R.id.editLayout);
                                layout.setVisibility(View.VISIBLE);
                                //
                                // Set the label to indicate editing
                                //
                                TextView remInputLabel = (TextView) findViewById(R.id
                                        .remInputLabel);
                                String title = getString(R.string.reminder_input_label);
                                remInputLabel.setText(title + ": EDITING ");
                                //
                                // make current row selected
                                //
                                holder.mContentView.setSelected(true);
                                edit_row_id = holder.rowID;
                                eMode = EDITMODE.EDIT;
                                //
                                // store the selected row id
                                selected_remRow_id = mValues.ROW_ITEMS.get(position).get_id();
                                //
                                // Set the edit text to the selected content
                                //
                                reminderInputText.setText(holder.mContentView.getText().toString());
                                //
                                // restrict to the selected reminder only
                                //
                                searchReminder(v);
                                //
                                // Indicate editing to user
                                //
                                Toast.makeText(v.getContext(), "Editing Highlighted Reminder " +
                                                "Item; id:" +
                                                " " + holder.rowID,
                                        Toast.LENGTH_SHORT
                                ).show();
                            }
                            else
                            {
                                newRem_fab.setImageDrawable(ContextCompat.getDrawable(v.getContext
                                        (), android.R.drawable.ic_input_add));
                                //
                                // Accept changes dialog
                                //
                                AlertDialog.Builder builder = new AlertDialog.Builder
                                        (v.getContext());
                                builder.setMessage("Update Memory reminders?");
                                builder.setCancelable(true);

                                builder.setPositiveButton(
                                        "Yes",
                                        new DialogInterface.OnClickListener()
                                        {
                                            public void onClick(
                                                    DialogInterface dialog,
                                                    int id
                                            )
                                            {
                                                dialog.cancel();
                                                //
                                                ReminderContent.ReminderRow reminderRow = new
                                                        ReminderContent.ReminderRow(
                                                        remRow_id, holder.mContentView.getText()
                                                        .toString());
                                                reminderRow.set_id(holder.rowID);
                                                //
                                                dbHandler.updateRow(
                                                        Integer.toString(holder.rowID),
                                                        reminderRow
                                                );
                                                // Update the memory list
                                                initReminderDetailRecyclerView(remType);
                                            }
                                        }
                                );

                                builder.setNegativeButton(
                                        "No",
                                        new DialogInterface.OnClickListener()
                                        {
                                            public void onClick(
                                                    DialogInterface dialog,
                                                    int id
                                            )
                                            {// Do nothing
                                                dialog.cancel();
                                            }
                                        }
                                );

                                AlertDialog dialog = builder.create();
                                dialog.show();
                                //
                                TextView memoryTextLabel = (TextView) findViewById(R.id
                                        .memoryTextLabel);
                                //
                                initReminderDetailRecyclerView(remType);
                                //
                                edit_row_id = 0;
                                eMode = EDITMODE.SEARCH;

                            }
                            // Set Parent to same editing mode
                            editing = holder.mEditing;

                            return true;
                        }
                    }
            );
            //
            holder.mView.setOnTouchListener(
                    new View.OnTouchListener()
                    {
                        @Override
                        public boolean onTouch(View v, MotionEvent event)
                        {

                            switch (event.getAction())
                            {
                                case MotionEvent.ACTION_DOWN:
                                    prevX = event.getX();
                                    prevY = event.getY();
                                    break;
                                case MotionEvent.ACTION_UP:
                                    if (event.getX() - prevX < -DELTA)
                                    {
                                        swipeDirection = DIRECTION.LEFT;
                                    }
                                    else if (event.getX() - prevX > DELTA)
                                    {
                                        swipeDirection = DIRECTION.RIGHT;
//                                return true;// cause click listenter to ignore event
                                    }
                                    else
                                    {
                                        swipeDirection = DIRECTION.NO_SWIPE;
                                    }

                                    break;

                                default:
                                    swipeDirection = DIRECTION.NO_SWIPE;
                                    return false;
                            }
                            return false;
                        }
                    }
            );
            //
            holder.mView.setOnClickListener(new View.OnClickListener()
            {

                @Override
                public void onClick(View v)
                {
                    try
                    {
                        Toast.makeText(v.getContext(), "Reminder Item onClick", Toast
                                .LENGTH_SHORT).show();

//                        AlertDialog.Builder builder = new AlertDialog.Builder(v.getContext());
//
//                        builder.setPositiveButton(
//                                "OK",
//                                new DialogInterface.OnClickListener()
//                                {
//                                    public void onClick(DialogInterface dialog, int id)
//                                    {
//                                        dialog.cancel();
//                                    }
//                                }
//                        );
//                        AlertDialog alert = builder.create();
//                        alert.setMessage("Row - " + holder.rowID + ": " + holder.mContentView
//                                .getText().toString());
//                        alert.show();
                        switch (swipeDirection)
                        {
                            case NO_SWIPE:
                                // Select the reminder and return to main
                                Toast.makeText(v.getContext(), mValues.ROW_ITEMS.get(position)
                                        .getRemName(), Toast
                                        .LENGTH_LONG).show();
                                //
                                Intent intent = new Intent(
                                        v.getContext(),
                                        ReminderMainActivity.class
                                );
                                //
                                intent.putExtra(ACTIVITY_ID, true);
                                //indicate
                                // there are
                                // extras
                                String selection = mValues.ROW_ITEMS.get(position).getRemName();
                                intent.putExtra(SELECT_ID, selection);
                                intent.putExtra(POSITION_ID, remRow_id);
                                //
                                intent.putExtra(REM_ID, mValues.ROW_ITEMS.get(position).get_id())
                                ;// dbHandler.getRemList().get(position).get_id());
                                //
                                navigateUpTo(intent);
                                break;
                            case RIGHT:// Edit Memory
                                /*********************************
                                 * Clear any filter or search, but remain in detail activity
                                 */
                                restoreNormalView();

                                //
                                break;
                            case LEFT:// Delete Reminder

                                break;
                            default:
                                break;


                            // Set filter by that row
                            //items.get(id).
                            // refresh the view to show only the row information
                        }
                    } catch (Exception e)
                    {
                        e.printStackTrace();
                    }
                }
            });
            //
            //
            holder.mImageButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder
                            (v.getContext());
                    builder.setMessage("Yet to be implemented for " + holder.mContentView.getText
                            ().toString() + ", but Clicking the image could display an expanded " +
                            "description for the reminder, for example...");
                    builder.setCancelable(true);

                    builder.setPositiveButton(
                            "OK",
                            new DialogInterface.OnClickListener()
                            {
                                public void onClick(
                                        DialogInterface dialog,
                                        int id
                                )
                                {
                                    dialog.cancel();
                                }
                            }
                    );

                    AlertDialog dialog = builder.create();
                    dialog.show();
                }
            });
        }

        @Override
        public int getItemCount()
        {
            return mValues.ROW_ITEMS.size();
        }

        /**************************************
         * Clear all Items in mValues
         */
        public void clearValues()
        {
            mValues.clearItems();
            mValues.clearRows();
        }

        public ReminderContent.ReminderRow getValueAt(int _position)
        {
            return mValues.ROW_ITEMS.get(_position);
        }

        public class ViewHolder extends RecyclerView.ViewHolder
        {
            public final View mView;
            public final TextView mContentView;
            public final ImageButton mImageButton;
            public int rowID;
            public boolean mEditing;

            public ViewHolder(View itemView)
            {
                super(itemView);
                //
                mView = itemView;
                mContentView = (TextView) itemView.findViewById(R.id.reminder_textView);
                mImageButton = (ImageButton) itemView.findViewById(R.id.reminder_imageButton);
                mEditing = false;

            }
            //

            @Override
            public String toString()
            {
                return super.toString() + " '" + mContentView.getText().toString() + "'";
            }

        }
    }

    private void restoreNormalView()
    {
        //
        // Set the label back to normal
        //
        TextView remInputLabel = (TextView) findViewById(R.id
                .remInputLabel);
        String title = getString(R.string.reminder_input_label);
        //
        remInputLabel.setText(title);
        //
        initReminderDetailRecyclerView(remType);
        //
        edit_row_id = 0;
        eMode = EDITMODE.SEARCH;
        selected_remRow_id = 0;
        //
        setEditLayoutVisible(true);
        //
        // Set fabs back to normal
        //
        FloatingActionButton search_fab = (FloatingActionButton) findViewById
                (R.id.searchRem_fab);
        FloatingActionButton clr_fab = (FloatingActionButton) findViewById(R
                .id.clrFilter_fab);
        //
        search_fab.setVisibility(View.VISIBLE);
        clr_fab.setVisibility(View.VISIBLE);
    }

    /**********************************************************************************
     * @param item
     * @return
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item)
    {
        try
        {
            int id = item.getItemId();
            if (id == android.R.id.home)
            {
                Intent intent = new Intent(this, ReminderMainActivity.class);
                //
                // Add extras, if any
                //

                // This ID represents the Home or Up button. In the case of this
                // activity, the Up button is shown. For
                // more details, see the Navigation pattern on Android Design:
                //
                // http://developer.android.com/design/patterns/navigation.html#up-vs-back
                //
                navigateUpTo(intent);
                return true;
            }
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return super.onOptionsItemSelected(item);
    }


    @Override
    public void onStart()
    {
        super.onStart();

        // ATTENTION: This was auto-generated to implement the App Indexing API.
        // See https://g.co/AppIndexing/AndroidStudio for more information.
        client.connect();
        Action viewAction = Action.newAction(
                Action.TYPE_VIEW, // TODO: choose an action type.
                "ReminderDetail Page", // TODO: Define a title for the content shown.
                // TODO: If you have web page content that matches this app activity's content,
                // make sure this auto-generated web page URL is correct.
                // Otherwise, set the URL to null.
                Uri.parse("http://host/path"),
                // TODO: Make sure this auto-generated app URL is correct.
                Uri.parse("android-app://com.facet_it.android.remind_me/http/host/path")
        );
        AppIndex.AppIndexApi.start(client, viewAction);
    }

    @Override
    public void onStop()
    {
        super.onStop();

        // ATTENTION: This was auto-generated to implement the App Indexing API.
        // See https://g.co/AppIndexing/AndroidStudio for more information.
        Action viewAction = Action.newAction(
                Action.TYPE_VIEW, // TODO: choose an action type.
                "ReminderDetail Page", // TODO: Define a title for the content shown.
                // TODO: If you have web page content that matches this app activity's content,
                // make sure this auto-generated web page URL is correct.
                // Otherwise, set the URL to null.
                Uri.parse("http://host/path"),
                // TODO: Make sure this auto-generated app URL is correct.
                Uri.parse("android-app://com.facet_it.android.remind_me/http/host/path")
        );
        AppIndex.AppIndexApi.end(client, viewAction);
        client.disconnect();
    }
}
