#include <hidef.h>      /* common defines and macros */
#include <mc9s12xdt512.h>     /* derivative information */
#pragma LINK_INFO DERIVATIVE "mc9s12xdt512"

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 
//
// TEMPORARY CODE CHANGES
//       THIS IS TO ACCOMADATE A 
//          PARTIALLY FAILED TEST BOARD
//
// In File xgate.cxgate
// Line 87: Comment out PWM3-> OTHERPW duty cycle logic
// Line 122: Add  OTHERPW = 0 ->  to mirror with Bottom knife
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//*********************
// Main Program C File
//*********************

// Designer: Greg Ford, B.Sc.; Chris Nicol, M.Eng.,B.Sc.
// Date: 04.09.2010
//
// File name: "main.c"
// Property of: Gregory Industrial Computer ltd.
//
// Use these defines for testing the Head Module only.
// Comment out for production software
//
//#define _FIRST_TIME_START
//#define _TEST_UNIT
#define _TEST_UNIT2

//**********************
// # DEFINES / INCLUDES 
//**********************
//#include "6812dp256_r01.h"
#include "stddefs.h"

#include "crg.h"
#include "global_vars.h"    // All global variables.
#include "IC_file.h"        // All code regarding input capture (IC).
#include "SCI_file.h"       // All code regarding serial interface (SCI).
#include "SPI_file.h"       // All code regarding serial peripheral interface (SPI).
#include "LakoValve.h" // All code regarding valve control.
#include "xgate_pwm.h"
//
//=====================================================================
#ifdef _TEST_UNIT
//
// Test sequences
//
// On/Off Test; if on off period is to short state does not change
//
void TestA()
{ 
    short st = 1;
    int i = 0;
  
    while(1)
    {
       ON_OFF_01  = st;  
       ON_OFF_02 = st;
       ON_OFF_03 = st;
       ON_OFF_04  = st;
       
        st = !st;
        
        for(i = 0; i < 50;i++)
        {
            ;
        }
    }
    
};

//
// Output Shared counter
//
void Shared_Out(void)
{
    int i = 0;
    /* int tempShared = shared_counter;
    char sharedC[12];
    
    (void)_itoa(tempShared,/*array*//*sharedC,8);
    
    strcat(sharedC,"\r\n");  
    
    SCI0_Output_Word(sharedC);*/
    //*************************
    // Period Timer
    //
    while(1)
    {
        //******************
        // Period marker
        //
        PORTE_PE5 = 1;
        
        //***********
        //Send to UART
        //
        Output_Length_And_Diameter();
        /*for(i=64;i<191;i++) 
        {
        //******************
        // Send to AD8802
        //    
        SPI1_Output_Word(0x00, (char)i); 
        
    }  */
        //****************
        // Period marker
        //
        PORTE_PE5 = 0;
        //***********
        //Send to UART
        //
        Output_Length_And_Diameter(); 
        /*for(;i>64;i--) 
        {
        //******************
        // Send to AD8802
        //    
        SPI1_Output_Word(0x00, (char)i); 
        
    }  */
        
    }
}
#endif 
//
// Main Program Entry
//  24MHz Clock
//
void main(void) 
{
    //***********************
    // VARIABLE DECLARATIONS
    //***********************   
    int i=0;
    short j=0;
    int test=0;
    short on_off = 1;
    short wait = 1;
    short temp = 0;
    char resistance = 0x00;
    
    
    //*******************
    // INITIALIZE SYSTEM
    //*******************
    //
    // Initialize PLL (Set BUS speed).
    //
    Init_PLL();   
    //
    // Setup X Gate
    //
    SetupXGATE();
    //
    // Initialize the PIT2
    //
    PIT2_Init();
    //
    // Initialize duty cycle
    //
    Init_DutyCycle(DEFAULTPRESS,DEFAULTPRESS,
    DEFAULTPRESS,DEFAULTPRESS);
    //
    // Input Capture
    //   
    IC_Init();    // Initialize input capture.
    //
    // Serial port
    //
    SCI_Init();  // Initialize SCI0 system (BLUETOOTH).      
    //
    // SPI
    //
    SPI_Init();   // Initialize SPI system.
    
    ECLKCTL_NECLK = 1;// Disable ECLK in order to use PE4 for IO
    //DDRM = 0xFF;			
    // Initialize I/O PORTS (0=in,1=out).
    // PORTA pins = 57-64, PORTB pins = 24-31.
    //
    // Set up Port A for output
    //
    DDRA=0xFF;  //PORTA(0-7) is used ON/OFF valve control.
    //***********************************************************
    // Set up Port B 0-3 for input
    //                                                                                                                       
    //DDRB=0x30;  //PORTB(0-3) used for checking encoder direction.
    DDRB_DDRB0 = 0;
    DDRB_DDRB1 = 0;
    DDRB_DDRB2 = 0;
    DDRB_DDRB3 = 0;
    DDRB_DDRB4 = 0;
    //***********************************************************
    // PortB output ports
    //
    DDRB_DDRB7 = 1;//PORTB (7) DAC chip select[v4.x=>PB4]  PORTB(5) SCI-Bluetooth RTS
    //PORTB (6) SCI-Bluetooth CTS 
    //
    // Set up Port E 0-6 for output
    //
    DDRE=0x30;  //PORTE(0-6) is used for On/Off valve control (0:1 ARE INPUT ONLY!)             
    DDRE_DDRE5 = 1; 
    DDRE_DDRE4 = 1;
    DDRE_DDRE3 = 1;
    DDRE_DDRE2 = 1;
    //
    // Port P
    //
    DDRP=0xFF;//PORTP P5 IS OUTPUT
    //
    // Setup Port AD 0,2,and 4 - 7 for output
    //
    DDR1AD0 =0x1F; // GPIO ADO -> Analog to Digital port; all except PAD 01
    
    
    //RDRH=0xFF;
    //
    // Set all valves to lock position
    //
    lockAll();
    //
    // Clear all global variables.
    //
    length_rotation_int=0;                                                                    
    diam1_int=0;
    diam2_int=0;
    
    //
    // Enable interrupts.
    //
    EnableInterrupts;  
    //
    //
    //
    PORTB_PB5 = 0;// RTS
    PORTA_PA7 = 1;/*HOLD MASTER*/
    //***************************
    // SYNCHRONIZE COMMUNICATION
    //***************************
    // Check to make sure READY message is properly sent/recieved.
    j = 0;
    
    //********************************
    // To be used ONE TIME ONLY
    // for each new  HM 
    
#ifdef _FIRST_TIME_START
    //    SCI0_Output_Word("set bt name HM-WT41-E-0002 \r\n");// increment ea iteration 
    //    SCI0_Read_Word();
    SCI0_Output_Word("at \r\n");
    while(!    SCI0_Read_Word() )
    {
        ;
    }
    
    SCI0_Output_Word("set\r\n"); //set bt auth * 486017907457371 ,486017907457370
    while(1)
    {
        if(SCI0_Read_Word() )
        {
            test = !test;
            
        }
    }
#endif
    //
    // Testing unit if defined
    //
#ifdef _TEST_UNIT
TestA();
/*
    while(1)
    {
        
        on_off = !on_off;
        //
        // Call ON/OFF test sequence
        //
        Test_OnOffs(on_off);
        //
        // DELAY
        //
        for(j=0; j < 5000; )
        {
            j=j+1;
        }
        
        //
        // Call PWM test sequence
        //
        if(i++ > 10)
        {
            Test_PWM(200);       
            i = 0;       
        }
        
        //
        // Call Low current adj. test sequence
        //
        Test_DAC();
    }      */
#endif
    
    //********************************
    
    //    SCI0_Output_Word("$hey,\r\n");    
    
    
    //
    // End TEST
    //  
    SCI0_Output_Word("$Before sync,\r\n");     
#ifndef _TEST_UNIT2
   while(!test)
    {
        if(SCI0_Read_Word())
        {
            SCI0_Output_Word("$test sync,\r\n"); 
            SCI0_Output_Word(INPUT_ARRAY);//Temporary for testing
            
            test=String_n_Cmp(INPUT_ARRAY,READY,CMDSIZE);
            if(test==1)
            {	 
                SCI0_Output_Word(INPUT_ARRAY);  // Sends back input array (To confirm correct $rdy command).
                
            }
            else 
            {
                SCI0_Output_Word("$err,sync,\r\n");
                
                //test=1;            
            }        
        }
    }
#endif
    SCI0_Output_Word("$After sync,\r\n");  
    //*************
    //Cop_Start(5);
    //*************
    /*  
    // send state of main saw (Up/Down)
    if(!PTT_PTT0)
    {
        SCI0_Output_Word("$err,prx1,low,Main out without sawing,\r\n"); 
    } else
    {
        SCI0_Output_Word("$err,prx1,hi,Main up,\r\n");
    }
    // send state of top saw (Up/Down)
    
    if(!PTT_PTT1)
    {
        SCI0_Output_Word("$err,prx2,low,Top out without sawing,\r\n"); 
        
    } else
    {
        SCI0_Output_Word("$err,prx2,hi,Top up,\r\n");
    }
    */
    //***********								 
    // MAIN LOOP
    //***********
    for(;;) { /* wait forever */ 
        
        for(i=0; i<10000; i++) {
            //*********************************************************
            // Compare incomming command, call 
            // corresponding function, and send confirmation/error message.
            //
            if(SCI0_Read_Word()) {	             
                
                if(!strncmp(INPUT_ARRAY,COMMAND,CMDSIZE)) {
                    cmd_HV(&INPUT_ARRAY);
                    SCI0_Output_Word(INPUT_ARRAY);
        		    if(!testCMD)
                        SCI0_Output_Word("FAIL_CMD");
        		    //test=1;
                }else if(!strncmp(INPUT_ARRAY,SET,CMDSIZE)) {         	      
        		    set(&INPUT_ARRAY);
        		    if(!testCMD)
        		    {
                        SCI0_Output_Word("FAIL_SET");            		      
        		    }
        		    SCI0_Output_Word(INPUT_ARRAY);  
        		    //test=1;
        	    }else if(!strncmp(INPUT_ARRAY,SPC,CMDSIZE)) {
                    spc(&INPUT_ARRAY);
                    SCI0_Output_Word(INPUT_ARRAY);         		    
        		    if(!testCMD)        		    
                        SCI0_Output_Word("FAIL_SPC");
        		    //test=1;
        	    }else if(!strncmp(INPUT_ARRAY,STOP,CMDSIZE)) {
                    stp();
        	           //
                    SCI0_Output_Word(INPUT_ARRAY); 
                    //
                    // Ensure final data is sent
                    //
                    Output_Length_And_Diameter();
                    //
                    // Send index value
                    //
                    //Output_Index();
                    
                    //test=1;           	    
                }else if(!strncmp(INPUT_ARRAY,NOP,CMDSIZE)) {
                    //SCI0_Output_Word(INPUT_ARRAY);
                    Output_Sensor_Info();
                    OutputSettings();
                    //test = 
                    testCMD = 1;  
        	    }else if(String_n_Cmp(INPUT_ARRAY,READY,CMDSIZE))
        	    {
                    // Confirm Ready
                    SCI0_Output_Word(INPUT_ARRAY);
                    //
                    // Read XGATE
                    //         	      
                    //Shared_Out();/*removed 05.06.2010*/
                    testCMD = 1;
        	    }else if(!strncmp(INPUT_ARRAY,LOCK,CMDSIZE))
        	    {
        	        //**************************
        	        // Lock all outputs
        	        //
        	        lockAll();
        	        //***************************
        	        // Confirm operation
        	        //
                    SCI0_Output_Word(INPUT_ARRAY);
                    //***************************
                    //
                    testCMD = 1;
        	    }else if(!strncmp(INPUT_ARRAY,TEST,CMDSIZE))
        	    {
        	            
        	    
                    //***************************
                    //
                    testCMD = 1;
        	    }else
        	    {
                    SCI0_Output_Word(&ERROR);
                    testCMD = 1;
        	    } // End if
        	    
        	    
        	    
            }// End if 
            //
            // If there is a change in the Main saw sensor
            //
            if(INTCHNG1_MAINSAW)
            {
                if(!PTT_PTT0)  
                {
                    //
                    // Send main saw is low
                    //
                    SCI0_Output_Word("$err,prx1,low,Main Saw,\r\n"); 
                }else
                {
                    //
                    // Send main saw up notice
                    //
                    SCI0_Output_Word("$err,prx1,hi,Main up,\r\n");
                }
                
                //
                // Reset the main saw sensor flag
                //
                INTCHNG1_MAINSAW = 0;
            }
            //
            // If there is a change in the top saw sensor
            //
            if(INTCHNG1_TOPSAW)   
            {
                //
                // Send appropriate message
                //
                if(PTT_PTT1)
                {
                    /* Send saw up notice */
                    SCI0_Output_Word("$err,prx2,hi,Top up,\r\n");
                }else
                {
                    /*Send saw down notice.*/
                    SCI0_Output_Word("$err,prx2,low,TopSaw,\r\n");
                }
                
                //
                // Reset the top saw flag
                //
                INTCHNG1_TOPSAW = 0;
            }
            //
            // If there is a change in the photo eye
            //
            if(INTCHNG1_PHOTOEYE)  
            {
                if(PTT_PTT2)
                {
                    /* send photo high message */
                    SCI0_Output_Word("$err,phoh,\r\n");/*12 x 17.5uS = 21uS*/  
                    /* Find ramp down point */
                    Init_RampDown(1);
                }else
                {
                    /*send photo low message*/
                    SCI0_Output_Word("$err,phol,\r\n");     	      
                }
                //
                // Reset Photo eye flag
                //
                INTCHNG1_PHOTOEYE = 0;
            }
            
            
            //Cop_Reset();
        }// End for
        //SCI0_Output_Word("Waiting,\r\n");
        // *****************************************************************************************************************************
        // If there is movement in the length or diameter encoders
        //
        if(INTCHNG1_LENGTHENCODER || INTCHNG1_DIA1ENCODER || INTCHNG1_DIA2ENCODER)
        {
            Output_Length_And_Diameter();
            //
            // Reset all relevent flags
            //
            INTCHNG1_LENGTHENCODER = INTCHNG1_DIA1ENCODER = INTCHNG1_DIA2ENCODER = 0; //only set in interrupt handler for encoder
        }
        //
        // If there is a change in the Saw encoder
        // 
        if(INTCHNG1_MSAWENC)
        {
            Output_Saw_Encoder();
            //
            // Reset Saw encoder flag
            // 
            INTCHNG1_MSAWENC = 0;
        }
        
        
    }// End For...ever     
    // please make sure that you never leave this function
    
} // END MAIN(void).
//****************************
// END OF Main Program C File
//****************************   
