/******************
 * ReminderMainActivity
 * Code By: Greg Ford, B.Sc.
 * All Rights Reserved, 2016
 */
package com.facet_it.android.remind_me;

import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;

import com.facet_it.android.remind_me.support.FilterSet;
import com.facet_it.android.remind_me.support.MemoryContent;
import com.facet_it.android.remind_me.support.ReminderContent;

import java.util.List;

/**
 * An activity representing a list of Reminders. This activity
 * has different presentations for handset and tablet-size devices. On
 * handsets, the activity presents a list of items, which when touched,
 * lead to a {@link ReminderDetailActivity} representing
 * item details. On tablets, the activity presents the list of items and
 * item details side-by-side using two vertical panes.
 */
public class ReminderMainActivity extends AppCompatActivity
{

    //
    private static final FilterSet filterMap = new FilterSet();
    private static Boolean editing = false;
    private static int edit_row_id = 0;
    //
    private FloatingActionButton memory_fab;
    /**
     * Whether or not the activity is in two-pane mode, i.e. running on a tablet
     * device.
     */
    private boolean mTwoPane;
    private static List<ReminderContent.ReminderType> items = null;
    private static MemoryContent memItems;
    //
    private static MemoryItemRecyclerViewAdapter memoryItemRecyclerViewAdapter;
    private static RecyclerView mRecyclerView;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);//main_reminder_list
        setContentView(R.layout.activity_reminder_main);//
        //
        // Check orientation and make changes if necessary
        //
        try
        {
            int orientation = getResources().getConfiguration().orientation;
            switch (orientation)
            {
                case Configuration.ORIENTATION_LANDSCAPE:
                    setTitle("");
                    break;
                case Configuration.ORIENTATION_PORTRAIT:
                    setTitle(getTitle());
                    break;
                default:
            }
            //
            // Tool Bar setup
            //
            Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
            setSupportActionBar(toolbar);
            //toolbar.setTitle(getTitle());//
            toolbar.setLogo(R.mipmap.ic_launcher);
            toolbar.requestFocus();

            //
            // Initialize action buttons
            //
            init_floating_action_buttons();
            //

            //
            // Check for extras
            //
            Bundle extras = getIntent().getExtras();
            if (extras != null)
            {
                handle_IntentExtras(extras);
            }
            //
            // Check for two pane view
            //
            if (findViewById(R.id.reminder_detail_container) != null)
            {
                // The detail container view will be present only in the
                // large-screen layouts (res/values-w900dp).
                // If this view is present, then the
                // activity should be in two-pane mode.
                mTwoPane = true;
            }
            //
            // If editing set the memory fab icon to save
            if (editing)
            {
                memory_fab.setImageDrawable(ContextCompat.getDrawable(
                        this, android.R.drawable.ic_menu_save)
                );
            }
            //
            //
            if (items == null)
            {
                initReminderTypes();
            }


            //
            // Set up reminder view
            //
            initReminderRecyclerView();
            //
            // Set up the memory view
            //
            initMemRecyclerView();

        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }


    /**************************************************
     * Initialize the items for reminder types
     */
    private int initReminderTypes()
    {
        items = ReminderContent.ITEMS;
        RemDBHandler dbHandler = new RemDBHandler(this, null, null, 1);

        //
        List<String> typeList = dbHandler.getReminderTypeList();
        int size = typeList.size();
        int result_size = 0;
        for (int i = 0; i < size; i++)
        {
            ReminderContent.ReminderType reminderType =
                    new ReminderContent.ReminderType(Integer.toString(i),
                            typeList.get(i).toString(), typeList.get(i).toString(), "NA"
                    );
            items.add(reminderType);
            result_size = items.size();
        }
        return result_size;
    }

    /**
     * Handle any extras sent to activity via Intent
     *
     * @param extras
     */
    private void handle_IntentExtras(Bundle extras)
    {
        try
        {
            /**************
             * Reminder extra handler
             */
            Boolean rem_detail_extras =
                    extras.getBoolean(ReminderDetailActivity.ACTIVITY_ID);
            if (rem_detail_extras)
            {
                String selection = extras.getString(ReminderDetailActivity.SELECT_ID);
                //
                String selectionPos = extras.getString(ReminderDetailActivity.POSITION_ID);
                //extras.getInt(ReminderDetailActivity.POSITION_ID);//
                int i = Integer.parseInt(selectionPos);
                //
                int rem_id = extras.getInt(ReminderDetailActivity.REM_ID);
                //
                String temp = items.get(i).toString();
                // update the filter map
                filterMap.put(temp, rem_id);
                // Show the selected filter in the reminder item
                items.get(i).setFilter(selection);
                // If in editing mode
                if (editing)
                {
                    TextView memoryTextLabel = (TextView) findViewById(R.id
                            .memoryTextLabel);
                    String title = getString(R.string.title_memory_main);
                    memoryTextLabel.setText(title + ": EDITING ");
                }
                // show and added indicator
                //Toast.makeText(this, selection, Toast.LENGTH_LONG).show();
                return;
            }
            /**********************
             * Memory extra handler
             */
            String memory = extras.getString(MemoryDetailActivity.MEM_TEXT_ID);
            //
            Boolean new_mem_extras = extras.getBoolean(MemoryDetailActivity.ADD_MEM);
            if (new_mem_extras)
            {
                addMemory(filterMap, memory);
                return;
            }
            Boolean edit_mem_extras = extras.getBoolean(MemoryDetailActivity.EDIT_MEM);
            if (edit_mem_extras)
            {
                int active_row = extras.getInt(MemoryDetailActivity.ROW_ID);
                //
                updateMemory(active_row, filterMap, memory);
            }
        } catch (Exception e)
        {
            e.printStackTrace();

        }
    }

    /**
     * Initialize the activities action buttons
     */
    private void init_floating_action_buttons()
    {
        FloatingActionButton clrFilter_fab = (FloatingActionButton) findViewById(R.id
                .clrFilter_fab);
        clrFilter_fab.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View view)
            {
                Snackbar.make(view, "Clearing All Filters", Snackbar.LENGTH_LONG)
                        .setAction("Action", null).show();
                clearAllFilters();
                initReminderRecyclerView();
            }
        });
        memory_fab = (FloatingActionButton) findViewById(R.id.addMemory_fab);
        memory_fab.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                try
                {
                    Snackbar.make(v, "Adding a new Memory", Snackbar.LENGTH_LONG).setAction(
                            "Action",
                            null
                    ).show();
                    //
                    if (!editing)
                    {// Go to Add a new memory
                        Context context = v.getContext();
                        Intent newMem_intent = new Intent(context, MemoryDetailActivity.class);

                        context.startActivity(newMem_intent);
                    }
                    else
                    {
                        MemoryContent.MemRow memRow = memoryItemRecyclerViewAdapter.getValueAt(0);
                        //
                        Boolean success = updateMemory(memRow.get_id(), filterMap, memRow
                                .getMemory());
                        if (!success)
                        {
                            Toast.makeText(v.getContext(), "Update Failed",
                                    Toast.LENGTH_LONG
                            ).show();
                        }
                        //
                        memory_fab.setImageDrawable(ContextCompat.getDrawable(
                                v.getContext(), android.R.drawable.ic_input_add)
                        );
                        //
                        editing = false;
                        //
                        initMemRecyclerView();
                    }

                } catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
        });
    }

    /*********************************************************************
     * Initialize the Recycler View
     */
    private void initReminderRecyclerView()
    {
        View recyclerView = findViewById(R.id.reminder_list);
        assert recyclerView != null;
        setupReminderRecyclerView((RecyclerView) recyclerView, items);//ReminderContent.ITEMS
    }

    /****************************************************************
     * @param recyclerView
     * @param items
     */
    private void setupReminderRecyclerView(
            @NonNull RecyclerView recyclerView,
            List<ReminderContent.ReminderType> items
    )
    {//
        try
        {
            recyclerView.setAdapter(new ReminderItemRecyclerViewAdapter(items));
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /**************************************************************************
     * Clear All Filters whether set or not
     * Make edit mode normal
     * refresh the memory view
     */
    public void clearAllFilters()
    {
        try
        {
            for (ReminderContent.ReminderType i : items)
            {
                i.filter = "NA";
            }
            filterMap.setPERSONid(0);
            filterMap.setPLACEid(0);
            filterMap.setTHINGid(0);
            filterMap.setSENSATIONid(0);
            //
            // Clear edit related information
            //
            editing = false;
            edit_row_id = 0;
            //
            // refresh the memory view
            //
            initMemRecyclerView();
            //
            // Reset the Memory Text Label
            //
            TextView memoryTextLabel = (TextView) findViewById(R.id.memoryTextLabel);
            memoryTextLabel.setText(R.string.title_memory_main);
            //
            // Change fab icon back
            //
            //
            memory_fab.setImageDrawable(ContextCompat.getDrawable(
                    this, android.R.drawable.ic_input_add));
            //
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /**
     * Set the current row filter to
     *
     * @param rowid this row ID
     */
    public void setRow(int rowid)
    {
        try
        {
            refreshFilter(rowid);
            //
            // Refresh Memory View
            //
            initMemRecyclerView();
            //
            // Refresh Reminder View
            //
            initReminderRecyclerView();
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /****************************************
     * Refresh the Filter for the indicated row ID
     *
     * @param rowID
     */
    public void refreshFilter(int rowID)
    {
        try
        {
            RemDBHandler dbHandler = new RemDBHandler(this, null, null, 1);

            MemoryContent.MemRow row = dbHandler.getMemRow(rowID);
            //
            // Set the Person Filter
            //
            filterMap.setPERSONid(row.getPerson_id());
            if (row.getPerson_id() > 0)
            {
                String n = dbHandler.getRemNameByID(row.getPerson_id());
                items.get(0).setFilter(n);
            }
            //
            // Set the Place Filter
            //
            filterMap.setPLACEid(row.getPlace_id());
            if (row.getPlace_id() > 0)
            {
                String p = dbHandler.getRemNameByID(row.getPlace_id());
                items.get(1).setFilter(p);
            }
            //
            // Set the Thing Filter
            //
            filterMap.setTHINGid(row.getThing_id());
            if (row.getThing_id() > 0)
            {
                String t = dbHandler.getRemNameByID(row.getThing_id());
                items.get(2).setFilter(t);
            }
            //
            // Set the Sensation Filter
            //
            filterMap.setSENSATIONid(row.getSensation_id());
            if (row.getSensation_id() > 0)
            {
                String s = dbHandler.getRemNameByID(row.getSensation_id());
                items.get(3).setFilter(s);
            }
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /*********************************************************************
     * Add a memory to the database
     *
     * @param filter Filter set to include in the memory
     * @param memory Memory to add
     */
    private Boolean addMemory(FilterSet filter, String memory)
    {
        try
        {
            int personId = filter.getPERSONid();
            // Make a memory row
            MemoryContent.MemRow row = new MemoryContent.MemRow(personId, filter.getPLACEid(),
                    filter.getTHINGid(),
                    filter.getSENSATIONid(), memory
            );
            // get a data base handler
            RemDBHandler dbHandler = new RemDBHandler(this, null, null, 1);
            if (!dbHandler.addRow(row))
            {
                Toast.makeText(this, "Add row Failed", Toast.LENGTH_LONG);
                return false;
            }
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return true;
    }

    /****************************************************************
     * Update the memory of a given row
     *
     * @param rowID
     * @param memory
     * @return true if successful
     */
    private Boolean updateMemory(int rowID, FilterSet filter, String memory)
    {
        try
        {
            // Make a memory row
            MemoryContent.MemRow row = new MemoryContent.MemRow(filter.getPERSONid(), filter
                    .getPLACEid(), filter.getTHINGid(),
                    filter.getSENSATIONid(), memory
            );
            RemDBHandler dbHandler = new RemDBHandler(this, null, null, 1);
            //
            if (!dbHandler.updateRow(Integer.toString(rowID), row))
            {
                Toast.makeText(this, "Edit Memory Failed", Toast.LENGTH_LONG);
                return false;
            }
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return true;
    }

    /************************************************************
     * inititialize the Memory Recycler View
     * Checks for editing of memory and if it is limits the view to
     * that single memory
     */
    private void initMemRecyclerView()
    {
        try
        {
            View memory_list_view = findViewById(R.id.memory_list_view);
            assert memory_list_view != null;
            //
            RemDBHandler dbHandler = new RemDBHandler(this, null, null, 1);

            if (!editing || memItems == null)
            {
                memItems = dbHandler.getMemoryMap(filterMap);

            }
            else
            {// Limit memItems to single selected without using filter.
                Toast.makeText(this, "Show row id: " + memItems.ITEMS.get(0).getMemory(), Toast
                        .LENGTH_LONG).show();
                // Get a memory row for the edit id
                MemoryContent.MemRow tmpRow = dbHandler.getMemRow(edit_row_id);//
                // Clear the Memory Content
                memItems.clear();
                // Add the row getting edited
                memItems.addItem(tmpRow);
                //
            }
            //
            setupMemRecyclerView((RecyclerView) memory_list_view, memItems);
            //
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /*********************************************************************************
     * @param recyclerView
     * @param items
     */
    private void setupMemRecyclerView(
            @NonNull RecyclerView recyclerView,
            MemoryContent items
    )
    {
        memoryItemRecyclerViewAdapter = new MemoryItemRecyclerViewAdapter(items);
        mRecyclerView = recyclerView;
        mRecyclerView.setAdapter(memoryItemRecyclerViewAdapter);
    }

    /*********************************************************************************
     * Recycle View Adapter for Memory Items
     */
    enum DIRECTION
    {
        NO_SWIPE, LEFT, RIGHT
    }

    /*********************************************************************************
     * Recycle View Adapter for Reminder Items
     */
    public class ReminderItemRecyclerViewAdapter
            extends RecyclerView.Adapter<ReminderItemRecyclerViewAdapter.ViewHolder>
    {

        private final List<ReminderContent.ReminderType> mValues;

        public ReminderItemRecyclerViewAdapter(List<ReminderContent.ReminderType> items)
        {
            mValues = items;
        }

        @Override
        public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.from(parent.getContext())
                    .inflate(R.layout.reminder_list_content, parent, false);
            return new ViewHolder(view);
        }

        @Override
        public void onBindViewHolder(final ViewHolder holder, int position)
        {
            try
            {
                holder.mItem = mValues.get(position);
                holder.mIdView.setText("");//mValues.get(position).id
                holder.mContentView.setText(mValues.get(position).type);
                final String tempFilter = mValues.get(position).filter;
                holder.mFilterView.setText(tempFilter);


                holder.mView.setOnClickListener(new View.OnClickListener()
                {
                    @Override
                    public void onClick(View v)
                    {

                        try
                        {
                            //
                            if (mTwoPane)
                            {
                                Bundle arguments = new Bundle();
                                arguments.putString(ReminderDetailFragment.ARG_ITEM_ID, holder
                                        .mItem.id);
                                ReminderDetailFragment fragment = new ReminderDetailFragment();
                                fragment.setArguments(arguments);
                                getSupportFragmentManager().beginTransaction()
                                        .replace(R.id.reminder_detail_container, fragment)
                                        .commit();
                            }
                            else
                            {
                                Context context = v.getContext();
                                Intent intent = new Intent(context, ReminderDetailActivity.class);
                                intent.putExtra(ReminderDetailFragment.ARG_ITEM_ID, holder.mItem
                                        .id);
                                // Include reminder type
                                intent.putExtra(ReminderDetailActivity.COLUMN_ID, holder.mItem
                                        .type);
                                // Include reminder description
                                intent.putExtra(ReminderDetailActivity.HINT_ID, holder.mItem
                                        .details);

                                context.startActivity(intent);

                            }
                        } catch (Exception e)
                        {
                            e.printStackTrace();
                        }
                    }
                });
            } catch (Exception e)
            {
                e.printStackTrace();
            }
        }

        @Override
        public int getItemCount()
        {
            return mValues.size();
        }

        public class ViewHolder extends RecyclerView.ViewHolder
        {
            public final View mView;
            public final TextView mIdView;
            public final TextView mContentView;
            public TextView mFilterView;
            public ReminderContent.ReminderType mItem;

            public ViewHolder(View view)
            {
                super(view);
                mView = view;
                mIdView = (TextView) view.findViewById(R.id.id);
                mContentView = (TextView) view.findViewById(R.id.content);
                mFilterView = (TextView) view.findViewById(R.id.filterByText);
            }

            @Override
            public String toString()
            {
                return super.toString() + " '" + mContentView.getText() + "'";
            }


        }
    }

    //
    public class MemoryItemRecyclerViewAdapter extends RecyclerView
            .Adapter<MemoryItemRecyclerViewAdapter.ViewHolder>
    {
        private static final int DELTA = 50;// Distance before swipe gesture is recognized
        private final MemoryContent mValues;
        //
        //
        // Touch Variables
        private float prevX = Float.NaN;
        private float prevY = Float.NaN;
        private DIRECTION swipeDirection = DIRECTION.NO_SWIPE;

        public MemoryItemRecyclerViewAdapter(MemoryContent items)
        {
            mValues = items;
        }

        @Override
        public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.from(parent.getContext()).inflate(R.layout
                    .memory_list_content, parent, false);
            ViewHolder viewHolder = new ViewHolder(view);
            return viewHolder;
        }

        @Override
        public void onBindViewHolder(final ViewHolder holder, int position)
        {
            // Set the current holder's memory text
            holder.mContentView.setText(mValues.ITEMS.get(position).getMemory());
            // Set the current holder's item row id
            holder.rowID = mValues.ITEMS.get(position).get_id();
            // Set the current holders image


            //
            holder.mView.setOnLongClickListener(
                    new View.OnLongClickListener()
                    {
                        @Override
                        public boolean onLongClick(View v)
                        {

                            holder.mEditing = !holder.mEditing;
                            if (holder.mEditing)
                            {// indicate editing with fab icon change
                                memory_fab.setImageDrawable(ContextCompat.getDrawable(
                                        v.getContext(), android.R.drawable.ic_menu_save)
                                );
                                //
                                TextView memoryTextLabel = (TextView) findViewById(R.id
                                        .memoryTextLabel);
                                String title = getString(R.string.title_memory_main);
                                memoryTextLabel.setText(title + ": EDITING ");
                                // Show current filters
                                refreshFilter(holder.rowID);
                                //
                                initReminderRecyclerView();
                                // make current row selected
                                holder.mContentView.setSelected(true);
                                // set row id to be edited
                                edit_row_id = holder.rowID;
                                // Indicate editing to user
                                Toast.makeText(v.getContext(), "Editing Highlighted Mem Item; id:" +
                                                " " + holder.rowID,
                                        Toast.LENGTH_LONG
                                ).show();
                            }
                            else
                            {
                                memory_fab.setImageDrawable(ContextCompat.getDrawable(v.getContext
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
                                                // if yes, update memory row
                                                updateMemory(holder.rowID, filterMap, holder
                                                        .mContentView.getText().toString());
                                                // Update the memory list
                                                initMemRecyclerView();
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
                                edit_row_id = 0;
                                //
                                TextView memoryTextLabel = (TextView) findViewById(R.id
                                        .memoryTextLabel);
                                memoryTextLabel.setText(getString(R.string.title_memory_main));
                                //
                                initMemRecyclerView();
                                //

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
                    Toast.makeText(v.getContext(), "Mem Item onClick", Toast
                            .LENGTH_SHORT).show();
                    if (mTwoPane)
                    {

                    }
                    else
                    {
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
                                // Get the item id that is clicked
                                setRow(holder.rowID);
                                break;
                            case RIGHT:// Edit Memory
                                // get the current memory text
                                String cur_mem = holder.mContentView.getText().toString();
                                int _row = holder.rowID;
                                editMemDetail(v, cur_mem, _row);

                                break;
                            case LEFT:// Delete Memory

                                break;
                            default:
                                break;
                        }

                        // Set filter by that row
                        //items.get(id).
                        // refresh the view to show only the row information
                    }
                }
            });
            //
            // Set up the ImageButton
            //
            holder.mImageButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder
                            (v.getContext());
                    builder.setMessage("Yet to be implemented for " + holder.mContentView.getText
                            ().toString() + ", but Clicking the image can either go to a website " +
                            "link or make " +
                            "the image larger etc...");
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
            return mValues.ITEM_MAP.size();
        }

        public void clearValues()
        {
            mValues.clear();
        }

        public MemoryContent.MemRow getValueAt(int _position)
        {
            return mValues.ITEMS.get(_position);
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
                mContentView = (TextView) itemView.findViewById(R.id.memory_textView);
                mImageButton = (ImageButton) itemView.findViewById(R.id.mem_imageButton);
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

    private void editMemDetail(View v, String cur_mem, int _row)
    {
        // Create an intent
        Context context = v.getContext();
        Intent intent = new Intent(context, MemoryDetailActivity.class);
        // add edit mode intent
        intent.putExtra(MemoryDetailActivity.EDIT_MODE, "EDIT");
        // add memory text intent
        intent.putExtra(MemoryDetailActivity.MEM_TEXT_ID, cur_mem);
        // add the row id
        intent.putExtra(MemoryDetailActivity.ROW_ID, _row);
        // generate activity
        context.startActivity(intent);
    }
}
