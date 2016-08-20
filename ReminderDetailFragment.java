/******************
 * ReminderDetailFragment
 * Code By: Greg Ford, B.Sc.
 * All Rights Reserved, 2016
 */

package com.facet_it.android.remind_me;

import android.app.Activity;
import android.content.Context;
import android.support.design.widget.CollapsingToolbarLayout;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import com.facet_it.android.remind_me.support.ReminderContent;

/**
 * A fragment representing a single Reminder detail screen.
 * This fragment is either contained in a {@link ReminderMainActivity}
 * in two-pane mode (on tablets) or a {@link ReminderDetailActivity}
 * on handsets.
 */
public class ReminderDetailFragment extends Fragment
{
    /**
     * The fragment argument representing the item ID that this fragment
     * represents.
     */
    public static final String ARG_ITEM_ID = "item_id";
    /******************************************
     * Implement way to call Parent.Activity
     */
    ReminderDetailFragmenListener activityCommander;

    public interface ReminderDetailFragmenListener
    {
        // Implemented in Parent.Activity
        public void itemChanged(ReminderContent.ReminderType _item);
    }

    @Override
    public void onAttach(Context context)
    {
        super.onAttach(context);
        try{
            activityCommander = (ReminderDetailFragmenListener) getActivity();
        }catch (ClassCastException e){
            throw new ClassCastException(getActivity().toString());
        }
    }

    /**
     * The dummy content this fragment is presenting.
     */
    private ReminderContent.ReminderType mItem;

    /**
     * Mandatory empty constructor for the fragment manager to instantiate the
     * fragment (e.g. upon screen orientation changes).
     */
    public ReminderDetailFragment()
    {
    }

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        if (getArguments().containsKey(ARG_ITEM_ID))
        {
            // Load the dummy content specified by the fragment
            // arguments. In a real-world scenario, use a Loader
            // to load content from a content provider.
            mItem = ReminderContent.ITEM_MAP.get(getArguments().getString(ARG_ITEM_ID));

            Activity activity = this.getActivity();
            CollapsingToolbarLayout appBarLayout = (CollapsingToolbarLayout) activity.
                    findViewById(R.id.toolbar_layout);
            if (appBarLayout != null)
            {
                appBarLayout.setTitle(mItem.type);
            }
        }
    }

    /****************************************************************
     * Called as soon as view is created by Activity.
     *
     * @param inflater
     * @param container
     * @param savedInstanceState
     * @return
     */
    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    )
    {
        try
        {
            View rootView = inflater.inflate(R.layout.old_reminder_detail_activity, container, false);
            //

            // Show the dummy content as text in a TextView.
            if (mItem != null)
            {
                ((TextView) rootView.findViewById(R.id.reminderTextView)).setText(mItem.details);
            }
            //


            return rootView;
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return null;
    }

}
