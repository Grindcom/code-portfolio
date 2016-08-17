//***********************************************************
//
// HMCMcmd.h - Commands required to communicate between 
//    Cabin module and Head module
//
//  Copyright (c) 2011 - 2020 Gregory Industrial Computer Ltd. (Grindcom)
//  All rights reserved
//
//************************************************************************

#ifndef HMCMCMD_H
#define HMCMCMD_H
//************************************************************************
// Safety Flags
//
enum SAFETY{SAFE_OFF=0,AUTO,MANUAL};
//**************************************************
// Command Flag data type
//
enum cmdFLAG{EMERGENCY=0, CONFIRMED, SENT, PENDING, IGNORE, LOCKED};
//**************************************************
// Mode data type
//
enum opmode{NORMAL,DIAGNOSTIC,OVERRIDE,WT41,HEADLESS};
//
//********************** Primary Commands ***************************************
//
const char COMMAND[]  = "$cmd,";    // 4bytes,4bytes,\r\n
const char _COMMAND[] = "$cmd";
const char SET[]      = "$set,";    // 4bytes,4bytes,2bytes (level value),\r\n	 
const char _SET[]     = "$set";
const char SPC[]      = "$spc,";    // 4bytes,4bytes,\r\n
const char _SPC[]     = "$spc";
const char STOP[]     = "$stp,";    // 4bytes,\r\n
const char _STOP[]    = "$stp";
const char READY[]    = "$rdy,";    // 4bytes,\r\n
const char _READY[]   = "$rdy";
const char ERROR[]    = "$err,\r\n";// 4bytes,\r\n
const char _ERROR[]   = "$err";    // an un-appended err message will 
                                    //cause an All-pump stop Action
const char COP_RESET[] = "$rst,"; // Cause CPU to reset
const char _COP_RESET[]= "$rst";
// ADDED oct 25, 2005
const char NOP[]      = "$nop,";
const char _NOP[]     = "$nop";
// end added
const char DATA[]   = "$dat,";
const char _DATA[]  = "$dat";
const char COMMA[]  = ",";
const char ENDMSG[] = "\r\n\0";
const char SYNCLOOP[] = "$Sync loop,\r\n";
const char _SYNCLOOP[]= "$Sync";
const char NOTINWINDOW[]="$err,niw,\r\n";
const char LOCK[] = "$lok,";// access lockAll() function
const char _LOCK[] = "$lok";//
const char _LOCKCLEAR[] = "lock clear";// Clear lock out
const char _AFTERSYNC[] = "$After";     //Indicates after sync loop
const char _FAIL_CMD[] = "FAIL_CMD";// Failed command
const char TEST[] = "$tst,"; // Directly test a specified port
const char _TEST[] = "$tst";
//
//********************** Secondary Commands *************************************
//
#define SIZESECCMDS 23 // Total number of sendary commands
//
const char FORWARD[]	= "forw,#";  //Forward
const char SLOW_FORWARD[] = "fosl,"; // Slow Forward
const char REVERSE[]	= "reve,";	//Revearse
const char SLOW_REVERSE[] = "resl,";  // Slow Revearse
const char HEADOP[]	= "open,";	//Head Open
const char HEADCL[]	= "clos,";	//Head Close
const char TKOPEN[]	= "tkop,";	//Top knife open
const char TKCLOS[]	= "tkcl,";	//Top knife close
const char BKOPEN[]	= "bkop,";	//Bottom knife open
const char BKCLOS[]	= "bkcl,";	//Bottom knife close
const char WHOPEN[]	= "whop,";	//Wheels open
const char WHCLOS[]	= "whcl,";	//Wheels close
const char MSAW[]	= "sawm,";//Main saw activate
const char _MSAW[]      = "sawm";//
const char TSAW[]	= "sawt,";	//Top saw activate
const char _TSAW[]      = "sawt";//
const char RETURN[]     = "retu,";  // Return both saws
const char TILTUP[]	= "tiup,";	//Tilt up
const char TILTDO[]	= "tido,";	//Tilt down
const char _EMERGENCY[] = "emrg,";    //Emergency Action Stop
const char _SHIFT[]     = "shif"; //Shift to next level input
const char _NEUTRAL[]     = "neut"; // Feed rocker in neutral
//**********************************************
//  Test Related commands
//
const char HIAMP1[] = "ha1,";
const char HIAMP2[] = "ha2,";
const char HIAMP3[] = "ha3,";
const char HIAMP4[] = "ha4,";
const char LOAMP1[] = "la1,";
const char LOAMP2[] = "la2,";
const char LOAMP3[] = "la3,";
const char LOAMP4[] = "la4,";
const char ONOFF1[] = "oo1,";
const char ONOFF2[] = "oo2,";

//**************************
// Non-Pump related commands
//
const char TILTFLOAT[]  = "tifl,";  // Tilt Float ie. neutral position
const char CLRL[]       = "clrl,";  // Clear length, diameter info, 4bytes,\r\n
const char CLRD[]       = "clrd,";  /* Clear diameter pulses */
const char ENDAUTO[]    = "eaut,";  //Clear the auto feed function
//const char RES2[]	= "res2,";	//Researved
//const char RES1[]       = "res1,";	//Researved
//***************************************************************
// Used in 'set' function
//
const char TOPK[]	= "topk,";	//Top knife pressure
const char TOPKMAX[]    = "tkma,";  /* Top knife maximum pressure/voltage */
const char BOTK[]	= "botk,";	//Bottom knife pressure
const char BOTKMAX[]    = "bkma,";  /* Bottom knife maximum pressure/voltage */
const char WHAR[]	= "whar,";	//Wheel Arm pressure
const char WHARMAX[]    = "wama,";  /* Wheel Arm maximum pressure/voltage */
const char WHSP[]	= "whsp,";	//Wheel speed
const char OTMAX[]      = "otma,";  /* Other pwm maximum voltage */
const char TARGET[]     = "targ,";// Length Target
const char MINTARGET[]  = "minT,";  //Minimum Length Target window
const char MAXTARGET[]  = "maxT,";  //Maximum Stopping window// Added 10.26.2006
const char MIDDLE[]     = "neut,";  //Midd range of wheels
const char BOUNCDEL[]   = "bncd,";  //Settle time for inputs
const char AUTOFEED[]   = "autf,";  //Auto feed select
const char LOGPROTECT[] = "lpro,";	/*Log protection*/
const char RAMPSTART[]  = "rmst,";  /*Ramp start speed*/
const char RAMPTO[]     = "rmto,";	/*Ramp to; i.e. for x pulses*/
const char RAMPDO[]     = "rmdo,";	/*Ramp down point; i.e. start ramp down x pulses from*/
const char USERAMPS[]   = "rmus,";  /*Use the ramps*/
const char FINDBUTT[]   = "finb,";  /*Find the butt flag*/
const char PROFILE[]    = "prof,";  /*Bluetooth Profile*/
//********************************************************
// Error messages
//
const char _NOTINWINDOW[]  = "niw";
const char _PHOTOEYE_ON[]  = "phoh";
const char _PHOTOEYE_OFF[] = "phol";
//**************** End Command word names and bit positions **********************



#endif