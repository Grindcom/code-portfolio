//***********************
// HarvesterValve Header       LAKO VERSION
//***********************
// Designer: Greg Ford
// Date: Aug. 12, 2005
//
// File name: "LakoValve.h"
// Property of: Gregory Industrial Computer ltd.
//
// Last Modified: February, 2015 (G.Ford)

//
// For use as a command interpreter for the Harvester Head Module
//
#ifndef HarvesterValve_h
#define HarvesterValve_h


#include <stdlib.h>   // Library for use of "atoi" function.
#include <string.h>
#include <mc9s12xdt512.h>
#include "SPI_file.h"
#include "SCI_file.h"
#include "stddefs.h"
#include "global_vars.h"
#include "xgate_pwm.h"
//#include "eeprom.h"

#include "HMCMcmd.h"

//-------------------------------------------------------------------------
//******************* Command sizes ***************************************
#define CMDSIZE 5 // includes '$' and ','
#define PARAMSIZE 9 // does not include ','

//#define LOCATE (*(volatile short *) 0x3F01)
// High Current Valve DAC address location
//#define HCV0   0x03 // Butt Knife Pressure Relief
//#define HCV1   0x02 // Wheel arm pressure relief
//#define HCV2   0x01 // Top Knife Pressure Relief
//#define HCV3   0x0B

//
//******************* End Command sizes ***********************************
//-------------------------------------------------------------------------
//----------------------------------------------------------------------------------


//**********************************************************************************
//************************* Operational Flag Register ******************************
//**********************************************************************************
//----------------------------------------------------------------------------------
//
// TILT PORT
//
#define     TILT_UP         PORTA_PA5 // ON/OFF 6   ->  CH B
#define     TILT_DOWN       PORTA_PA4	// ON/OFF 5 -> CH A
//
// MAIN SAW PORT
//
#define     SAW_MAIN_DOWN     PORTA_PA1 // ON/OFF 2 -> CH A
#define     SAW_MAIN_UP             PORTA_PA0 // ON/OFF 1  -> CH B
//
// TOP SAW PORT
//
#define     SAW_TOP_DOWN      PORTA_PA3 // ON/OFF 4   -> CH A
#define     SAW_TOP_UP    PORTA_PA2 // ON/OFF 3   -> CH B
//
// WHEEL ARM PORT
//
#define     ARM_WHEEL_OPEN  PORTE_PE4 // ON/OFF 9   -> CH B
#define     ARM_WHEEL_CLOSE PORTE_PE5 // ON/OFF 10   -> CH A
//
// BOTTOM KNIFE PORT
//
#define     KNIFE_BOTTOM_OPEN   PORTA_PA7 // ON/OFF 8 -> CH B
#define     KNIFE_BOTTOM_CLOSE  PORTA_PA6 // ON/OFF 7 -> CH A
//
// TOP KNIFE PORT
//
#define     KNIFE_TOP_OPEN      PORTE_PE3 // ON/OFF 12 -> CH A
#define     KNIFE_TOP_CLOSE     PORTE_PE2 // ON/OFF 11-> CH B




//**********************************************************************************
//************************* END Operational Flag Register **************************
//**********************************************************************************
//----------------------------------------------------------------------------------
//**************** Command word names and bit positions *******************
#define FOR		0x8000 //15
#define REV		0x4000 //14
#define HDOP	0x2000 //13
#define HDCL	0x1000 //12
#define TKOP	0x0800 //11
#define TKCL	0x0400 //10
#define BKOP	0x0200 //9
#define BKCL	0x0100 //8
#define WHOP	0x0080 //7
#define WHCL	0x0040 //6
#define	SAWM	0x0020 //5
#define SAWT	0x0010 //4
#define TIUP	0x0008 //3
#define	TIDO	0x0004 //2
#define	R_2		0x0002 //1
#define R_1		0x0001 //0

#define VALVEON     0    // On/Off circuits are active w/ reverse logic
#define VALVEOFF   1
//
// !!!! to be set when either Main or Top saw sensor is active, from their

//
int testCMD=1;  // For use with testing if commands are error free.
//



//--------------------------------------------------------------------------------
//***************** Calibration, Speed and Pressure Relief ***********************
// Short integer variables 
//
// Wheel speeds, initialized for maximum 
//
volatile int  SPEEDF = 0xBF;//;E1//C6;   //FORWARD  204 ~> 18.5v @ Vin > 21.1vdc
volatile int  SPEEDR = 0x40;   //36REVEARSE 68  ~>  6v

volatile int  SLOW_SPEED = 0x20;
//
volatile int  NEUTRAL = 0x7F;//0x89;  //neutral voltage; 12vdc
volatile const int  MID     = 0x7F;//0x89;  // 12vdc
volatile int  SPEEDMAX = 0x3F; // Maximum speed change above or below NEUTRAL
#define WHEEL_A 0x00 // Address of wheel A proportional
#define WHEEL_B 0x00 // Address of wheel B proportional
//
// Wheel speeds are adjusted using a proportional valve whose neutral 
// position is ~12 volts DC.  The 'SPEEDF' is added to spNEUTRAL for
// forward speeds, and 'SPEEDR' is subtracted from spNEUTRAL for revearse
// operation.
//
//  Default pressure value.  This value is based on the maximum allowable
//      duty cycle.  The larger the value the higher the pressure
//
volatile const short DEFAULTPRESS = 450;
volatile PWMPERIOD_ADDEND = 350; // Value intended to fine tune the multiplier result.

//
// Maximum pressures (Select based on the criteria above)
//
short MAXTKPRES = 450;
short MAXWAPRES = 450;
short MAXBKPRES = 450;
short MAXOTPRES = 450;
volatile const short MINPRES = 350; /* Aprox 12% of PWMPERIOD */
//
// Pressure relief, initialized to Maximum pressure
//        default values ~based on Eliuk baseline tests
//
volatile int tkPR = 450;//380;	//Top Knife
volatile int bkPR = 450;//268; //Bottom Knife
volatile int whPR = 450;//385; //Wheel Arm
//
// Pressure relief valves have full pressure or 'zero' relief with a 
// 24 volt value; the pressure relief value is 
//
// ie. MAXPRES + tkPR => pressure value on top knife
//

/***********************************
* stopWindow change record
* Added 10.26.2006
* Changed: 03.08.2006
***********************************/
unsigned int minTarget = 0;
unsigned int maxTarget = 0;
/*********************************
* Ramping flags and distance
******************************/
//short useRamps = 0;
volatile short RAMPSTART_SPEED = 0;
volatile short RAMPCUR_SPEED = 0;
int rampUp = 0;
int rampDown = 0;
int rampDownPoint = 0;
short rampTicSize = 0;/*Pulses to wait before increment/decriment speed*/
volatile static  rampTic = 0;
/****************************
* Finding the butt automatically
********************************/
short findButt = 0;
//
short settleTime = 2000;
//**************** End Calibration, Speed and Pressure Relief **********************
//----------------------------------------------------------------------------------
//******************** Command Declaration *****************************************
//
// Used for interpretting simple, predefined commands
void cmd_HV(const char data[]);
//
// Used to set all or selected, variables used for pressure or speeds 
void set(char* data);  
//
//special command; allows unusual operational configurations
void spc(char* data);
//
// Stop movement and set all to neutral, if a saw is out set it to return voltage 
void stp(); 
// Stop movement and sqeeze grapple
void squzHold();
// Lock all on offs and set wheels to neutral
void lockAll();
//*************** End Command Function Declaration ************************
//-------------------------------------------------------------------------
//*********************** Secondary Command Declarations ******************
//
//************************* CMD Set function Declarations *****************
void forward(void);
void slowFor(void);
void reverse(void);
void slowRev(void);
void openAll(void);
void closeAll(void);
// less than 0 is close, More than 0 is open for all openClose variables
void topK(short openClose);
void botK(short openClose);
void whArms(short openClose);
// speed = voltage level 0 to 0x7E, add to or subtract  
// from neutral volt level    
void wheels(short volatile direction);
void slowWheels(short volatile direction, short volatile speed);
// less than 0 is Down, More than 0 is Up for all upDown variables
void tilt(short upDown);
void sawM(short upDown);
void sawT(short upDown);
//************************ CMD service functions ***************************
void setFeedVolts(void);
void Init_RampUp(short);
void Init_RampDown(short);
void RampTic(int,int);
void OutputSettings(void);
//
//**************** End CMD Set Function Declarations ***********************
//
//******************* End Secondary Command Declarations *******************
//--------------------------------------------------------------------------
//--------------------------------------------------------------------------
//******************** Command Definitions *********************************
//
//******************** CMD Function Definitions ****************************
//
void cmd_HV(const char data[])
{
    // Used for interpretting simple, predefined commands.
	
	char op[CMDSIZE+1] = "clos,";  //5
	short i,j=0;
	
	//Reading the 2nd part of a $cmd messagei<=CMDSIZE*2
	for(i = CMDSIZE; data[i] != ',';i++)
	{
		op[j++] = data[i];
	}
    
	
	// A Switch statement is not useful with string arrays;
	// a series of if statemnts identify the command requested.
	if(!strncmp(op,FORWARD,CMDSIZE)) 
    {
        testCMD=1;
		(void)forward();  	  
    }
	else if(!strncmp(op,REVERSE,CMDSIZE))
    {   	  
  		(void)reverse();
  		testCMD=1;		  
    }
	else if(!strncmp(op,SLOW_FORWARD,CMDSIZE))
    {
	    slowFor();
	    testCMD = 1; 
    } 
	else if(!strncmp(op,SLOW_REVERSE,CMDSIZE))
    {
	    slowRev();
	    testCMD = 1;
    }
	else if(!strncmp(op,HEADOP,CMDSIZE)) 
    {
        testCMD=1;
		(void)openAll(); 	  
    }
	else if(!strncmp(op,HEADCL,CMDSIZE)) 
    {
        testCMD=1;
		(void)closeAll(); 	  
    }
	else if(!strncmp(op,TKOPEN,CMDSIZE)) 
    {
        testCMD=1;
		(void)topK(1); 	  
    }
	else if(!strncmp(op,TKCLOS,CMDSIZE)) 
    {
        testCMD=1;
		(void)topK(-1);  	  
    }
	else if(!strncmp(op,BKOPEN,CMDSIZE))
    {
        testCMD=1;
		(void)botK(1); 	  
    }
	else if(!strncmp(op,BKCLOS,CMDSIZE))
    {	  
        testCMD=1;
		(void)botK(-1);    	  
    }
	else if(!strncmp(op,WHOPEN,CMDSIZE)) 
    {
        testCMD=1;
		(void)whArms(1); 	  
    }
	else if(!strncmp(op,WHCLOS,CMDSIZE)) 
    {
        testCMD=1;
		(void)whArms(-1);  	  
    }
	else if(!strncmp(op,MSAW,CMDSIZE)) 
    {
        testCMD=1;
		(void)sawM(-1); 	  
    } 
	else if(!strncmp(op,RETURN,CMDSIZE))
    {
	    testCMD = 1;
	    (void)sawM(1);
	    (void)sawT(1);
    }
	else if(!strncmp(op,TSAW,CMDSIZE)) 
    {
        testCMD=1;
		(void)sawT(-1); 	  
    }
	else if(!strncmp(op,TILTUP,CMDSIZE)) 
    {
		(void)tilt(1);
		testCMD=1;
    }
	else if(!strncmp(op,TILTDO,CMDSIZE)) 
    {	
        testCMD=1;  
		(void)tilt(-1);  		
    } 
	else if(!strncmp(op,TILTFLOAT,CMDSIZE))
    {
	    testCMD = 1;
	    (void)tilt(0);
    }
	else if(!strncmp(op,CLRL,CMDSIZE))
    {        
        length_rotation_int=0;
        //
        // Clear CUI index
        //
        indexCUI = 0;
        //
        //  Clear ppr
        //
        ppr = 0;
        //
        //
        //
        testCMD=1;
    }else if(!strncmp(op,CLRD,CMDSIZE))
    {
        diam1_int=0;
        diam2_int=0;
        testCMD=1;      
    }
    else if(!strncmp(op,ENDAUTO,CMDSIZE))
    {
        FEATURE1_USEAUTOFEED = FALSE;
        testCMD = 1;
    }
	else 
        testCMD=0;
}
//************************* End CMD Function ********************************
//---------------------------------------------------------------------------
//***************** SET function Definition *********************************
void set(char* data)
{
    // Used to set all or selected, variables used for
	// pressure or speeds or calibration.
	
	char op[CMDSIZE+1];
	char setChar[10] = "0000000000";
	short i,j=0;
	int set;
	unsigned temp = 0;
	float tempf = 0.0;
        //	
        // Reading the 2nd part of a $set message.
        //
        for(i = CMDSIZE; i< CMDSIZE*2 ;i++)
        {
            op[j++] = data[i];
            if(data[i] == ',')    
            {
                i++;
                break;
            }
        }
	//
	// Extract short integer from 3rd part of command.
	//
	for(i,j = 0; j < PARAMSIZE;j++)
        {
            setChar[j] = data[i++];
            if(data[i] == ',')    
            {
                i++;
                setChar[j+1] = '\0';
                break;
            }
        }
  	//
  	// Change 3rd param to long int
  	//
	set = atoi(setChar);
	
	// Use if else statements to select set option.
	if(!strncmp(op,TOPK,CMDSIZE)) 
        {  
            tkPR = (int)(set * PWMPERIOD_MULTIPLIER);/* set is the fraction of duty cycle */
            tkPR = tkPR + (int)PWMPERIOD_ADDEND;
            if(tkPR > MAXTKPRES)
            {
                tkPR = MAXTKPRES;
            }else if(tkPR < MINPRES)
            {
                tkPR = MINPRES;
            }
            testCMD=1;
        } else if(!strncmp(op,TOPKMAX,CMDSIZE))
        {
            set = (int)set * PWMPERIOD_MULTIPLIER;
            set += PWMPERIOD_ADDEND; /* Set using a percentage of the system; ASSUMED SYSTEM VOLTAGE */
            if(set > 0)
            {
                PWM1_DUTYCYCLE = MAXTKPRES = (int)set; /* the change will take affect immediately */
            }
        }else if( !strncmp(op,BOTK,CMDSIZE)) 
        {
            bkPR = (int)set * PWMPERIOD_MULTIPLIER;/* set is the fraction of duty cycle */
            bkPR += PWMPERIOD_ADDEND;
            if(bkPR > MAXBKPRES)
            {
                bkPR = MAXBKPRES; 
            }else if(bkPR < MINPRES)
            {
                bkPR = MINPRES;
            }
            testCMD=1;
        }else if(!strncmp(op,BOTKMAX,CMDSIZE))
        {
            set = (int)set * PWMPERIOD_MULTIPLIER;
            set += PWMPERIOD_ADDEND; /* Set using a percentage of the system; ASSUMED SYSTEM VOLTAGE */
            if(set > 0)
            {
                PWM0_DUTYCYCLE = MAXBKPRES = (int)set;/* the change will take affect immediately */
            }	    
            testCMD=1;
        }
        else if( !strncmp(op,WHAR,CMDSIZE)) 
        {	 
            whPR = (int)set * PWMPERIOD_MULTIPLIER;/* set is the fraction of duty cycle */
            whPR += PWMPERIOD_ADDEND;
            if(whPR > MAXWAPRES)
            {
                whPR = MAXWAPRES; 
            }else if(whPR < MINPRES)
            {
                whPR = MINPRES;
            }
            testCMD=1;    
        }else if(!strncmp(op,WHARMAX,CMDSIZE))
        {
            set = (int)set * PWMPERIOD_MULTIPLIER;
            set += PWMPERIOD_ADDEND; /* Set using a percentage of the system; ASSUMED SYSTEM VOLTAGE */

            if(set < PWMPERIOD)
            {
                PWM2_DUTYCYCLE = MAXWAPRES = (int)set; /* the change will take affect immediately */
            }
            testCMD=1;	    
        } else if(!strncmp(op,OTMAX,CMDSIZE))
        {
            set = (int)set * PWMPERIOD_MULTIPLIER;
            set +=PWMPERIOD_ADDEND; /* Set using a percentage of the system; ASSUMED SYSTEM VOLTAGE */
            if(set < PWMPERIOD)
            {
                PWM3_DUTYCYCLE = MAXOTPRES = (int)set; 
            }
        }
        else if( !strncmp(op,WHSP,CMDSIZE)) 
        {
            if(set <= SPEEDMAX)
            {
                SLOW_SPEED = (int)set;
            }else 
            {
                SLOW_SPEED = SPEEDMAX;
            }
            testCMD=1;
        } else if( !strncmp(op,MIDDLE,CMDSIZE))
        {
            if(set <= SPEEDMAX)
            {
                NEUTRAL = MID + (int)set;
                //EESectorModify(&temp,&NEUTRAL);
                SPEEDF = NEUTRAL + 0x40;/*25% of 0xFF*/
                //EESectorModify(&temp,&SPEEDF);
                SPEEDR = NEUTRAL - 0x40;
                //EESectorModify(&temp,&SPEEDR);
            }else
            {
                NEUTRAL = SPEEDMAX;
            }
            testCMD = 1;
        } else if(!strncmp(op,MINTARGET,CMDSIZE))
        {
            //
            // set has the minimum target in pulses
            //    
            minTarget = (int)set;
            //
            // Target is only used in conjunction 
            // with auto functions, so it is 
            // combined here.
            //
            FEATURE1_USEAUTOFEED = TRUE; 
            testCMD=1; 
        }else if(!strncmp(op,MAXTARGET,CMDSIZE))
        {
            //
            // set has the maximum target in pulses
            //
            maxTarget = (int)set;
            //
            // Target is only used in conjunction 
            // with auto functions, so it is 
            // combined here.
            //
            FEATURE1_USEAUTOFEED = TRUE;
            testCMD = 1; 
        } else if(!strncmp(op,TARGET,CMDSIZE))
        {
            //
            // set has the minimum target in pulses
            //    
            minTarget = (int)set;
            //
            // Make sure the delimeter is by-passed
            //
            if(data[i] == ',' )
            {
                i++;
            }
            //
            // Extract short integer from 3rd part of command.
            //        
            for(i,j = 0; j < PARAMSIZE;j++)
            {
                //
                // the next delimiter indicates the end
                // of the parameter
                //
                if(data[i] == ',' || data[i] == '\0')
                {
                    break;
                }
                setChar[j] = data[i++];
            }
            //
            // Change 4th param to long int
            //
            set = atoi(setChar);
            //            
            //
            // set has the maximum target in pulses
            //
            maxTarget = (int)set;
            //
            // Target is only used in conjunction 
            // with auto functions, so it is 
            // combined here.
            //
            FEATURE1_USEAUTOFEED = TRUE;
            testCMD = 1; 
        }else if(!strncmp(op,BOUNCDEL,CMDSIZE))
        {
            settleTime = (int)set;
            testCMD=1;
        } 
        else if(!strncmp(op,AUTOFEED,CMDSIZE)) 
        {
            FEATURE1_USEAUTOFEED = TRUE;
            testCMD=1;
        } 
        else if(!strncmp(op,LOGPROTECT,CMDSIZE))
        {
            FEATURE1_PROTECTLOG = (int)set;
            testCMD=1;       
        }
        else if(!strncmp(op,RAMPTO,CMDSIZE))
        {
            //
            // Distance to ramp up over
            //
            rampUp = (int)set;
            testCMD=1;       
        }
        else if(!strncmp(op,RAMPDO,CMDSIZE))
        {
            //
            // Distance before stop point to
            // Start ramping down
            //
            rampDown = (int)set;
            testCMD=1;       
        }    
        else if(!strncmp(op,USERAMPS,CMDSIZE))
        {
            //
            // set is true/false
            //
            FEATURE1_USEINGRAMPS = (int)set;
            testCMD=1;       
        } 
        else if(!strncmp(op,RAMPSTART,CMDSIZE))
        {
            RAMPSTART_SPEED = (int)set;
            //
            // Find the step distance
            // 
            testCMD=1;       
        }
        else if(!strncmp(op,FINDBUTT,CMDSIZE))
        {
            findButt = (int)set;
            FEATURE1_FOUNDBUTT = 0;
            testCMD=1;       
        }else if(!strncmp(op,PROFILE,CMDSIZE))
        {
            //
            // send the escape set to Bluetooth
            //
            SCI0_WT41_ESC();
            //
            // Wait for confirmation (READY)
            //
            while(!SCI0_Read_WT41_Word()); /*WARNING Possible infinite loop*/
            //
            // send set profile command to Blutooth device
            //
            SCI0_WT41_Output_Word("SET PROFILE OTA ",16);
            SCI0_WT41_Output_Word(setChar,4);
            SCI0_WT41_Output_Word("\r\n",2);
            SCI0_WT41_Output_Word("RESET\r\n",7);

        }else
        {
            testCMD=0;
        }
}
//
//***************** End SET function Definition ****************************
//--------------------------------------------------------------------------
//***************** START SPC function Definition **************************
// Special command; allows unusual operational configurations
// WARNING: This function removes some safety features and should be used with
//			caution.  Safety protocols will be left to the command module
void spc(char* data)
{// Used for interpretting simple, predefined commands
	char op[CMDSIZE];
	short i,j=0;
	short function = 0;
	
	testCMD=0;
	
	// Reading the 2nd part of a $cmd message, using the ',' 
	// allows unusual lengths for function requests
	for(i = CMDSIZE; data[i]!= ',';i++)
	{
		op[j++] = data[i];
	}
    
	function = atoi(op);
	
	// compare function request and set valid operations
    
	if((FOR & function) == FOR)
	{
        (void)setFeedVolts();		
		// Set Wheel speed/proportional valve voltage FORWARD.
		(void)wheels(1);
		testCMD=1;
	}
	if((REV & function) == REV)
	{
		(void)setFeedVolts();		
		// Set Wheel speed/proportional valve voltage REVEARSE.
		(void)wheels(-1);
		testCMD=1;
	}
    
	if((HDCL & function) == HDCL)
    {
        (void)closeAll();
        testCMD=1;
    }
	if((HDOP & function) == HDOP)
    {	
        (void)openAll();
        testCMD=1;
    }
	if((TKCL & function) == TKCL)
    {
        (void)topK(-1);
        testCMD=1;
    }
	if((TKOP & function) == TKOP)
    {
        (void)topK(1);
        testCMD=1;
    }
	if((BKCL & function) == BKCL)
    {
        (void)botK(-1);
        testCMD=1;
    }
	if((BKOP & function) == BKOP)
    {
        (void)botK(1);
        testCMD=1;
    }
	if((WHCL & function) == WHCL)
    {
        (void)whArms(-1);
        testCMD=1;
    }
	if((WHOP & function) == WHOP)
    {
        (void)whArms(1);
        testCMD=1;
    }
	if((SAWM & function) == SAWM)
    {
        (void)sawM(-1);
        testCMD=1;
    }
	if((SAWT & function) == SAWT)
    {
        (void)sawT(-1);
        testCMD=1;
    }
	if((TIDO & function) == TIDO)
    {
        (void)tilt(-1);
        testCMD=1;
    }
	if((TIUP & function) == TIUP)
    {
        (void)tilt(1);
        testCMD=1;
    }
    
    // future R_2
    // future R_1
    
}
//********************* End SPC function Definition ************************
//--------------------------------------------------------------------------
//*********************** STP function Definition **************************
/***************
* BigO: 2.88uS
* With 36.8Mhz bus
******************/
void stp()
{
        (void)wheels(0);/*1.06mS*/
        (void)topK(0); /*3x516nS*/
        (void)botK(0);
        (void)whArms(0);

        if(!PTT_PTT0)// Main saw out 
        sawM(1);  //return it.
        else
            sawM(0);
        
        if(!PTT_PTT1)
            sawT(1);
        else
            sawT(0);
    	//
        // clear ramping up flag 
        //
        FEATURE1_RAMPINGUP = 0;
        //
        // Clear ramping down flag
        //
        FEATURE1_RAMPINGDOWN = 0;
        //
        // Successful operation
        //
    	testCMD=1;
}
//********************* End STP function Definition *************************
//---------------------------------------------------------------------------
//********************* squzHold function Definition ************************
void squzHold()
{
    int i = 0;
    // Stop wheel movement
    wheels(0);
    // Squeeze 
    closeAll();
    for(i = 0; i<500;i++)
    {
        // delay to allow valves to activate 
    }
}
//********************* End squzHold function Definition ********************
//---------------------------------------------------------------------------
//********************* lockAll function definition *************************
void lockAll()
{
    // Set wheel motion valve to NEUTRAL.
    //Set wheel to neutral (at whatever speed).
    // send NEUTRAL again	
    SPI1_Output_Word(WHEEL_B,(char)NEUTRAL);
    SPI1_Output_Word(WHEEL_A,(char)NEUTRAL);
    //
    // Set Top knife lock voltage
    KNIFE_TOP_CLOSE = 1;
    KNIFE_TOP_OPEN = 1;  
    // Set Bottom knife LOCK voltage:
    KNIFE_BOTTOM_CLOSE= 1;//v4.x=>PT1AD0_PT1AD06 
    KNIFE_BOTTOM_OPEN= 1;//v4.x=>PT1AD0_PT1AD07 
    // Set top saw LOCK voltage: 
    SAW_TOP_DOWN = 1;// PORTE_PE0
    SAW_TOP_UP = 1;// PORTE_PE1
    // Set tilt valve to LOCK voltage.	
    TILT_DOWN = 1;//v4.x=>PT1AD0_PT1AD04 
    TILT_UP = 1;//v4.x=>PT1AD0_PT1AD05 
    // Set main saw valve voltage to NEUTRAL voltage.	
    SAW_MAIN_DOWN = 1;
    SAW_MAIN_UP = 1;
    // Set Wheel arm valve voltage to NEUTRAL.	
    ARM_WHEEL_CLOSE = 1;
    ARM_WHEEL_OPEN = 1;
    
}




//---------------------------------------------------------------------------
//************************* Secondary Command Definitions *******************
void openAll()
{
	// Set all votages to open valve with full values.
	(void)topK(1);
	(void)botK(1);
	(void)whArms(1);
}
//****************************************************************************
void closeAll()
{
	// Set all votages to  close valve with full values.
	(void)topK(-1);
	(void)botK(-1);
	(void)whArms(-1);
}
/****************************************************************************
* Forward
* Changes:
* removed sawM(1) on 03.09.2010
***********/
void forward()
{
    int i = 0;
    short dir = 1;/*initialize to forward direction*/
    //----
    if( !FEATURE1_SAWOUT )
    {
        //
        // if using find butt, and have not found it yet, 
        //  and photo eye is high, set
        // proper direction; i.e. if high go back to butt
        //
        if(!FEATURE1_FOUNDBUTT && findButt && PTT_PTT2)
        {
            dir = -1;/*Reverse direction*/      
        }
        //
        // If using ramps, use slow wheels and
        // starting ramp speed                      
        //
        if(FEATURE1_USEINGRAMPS)
        {
                Init_RampUp(dir);
                slowWheels(dir,RAMPCUR_SPEED);
        }else
        {
                //
                // Otherwise use full speed.
                //
                wheels(dir);
        }

        // Delay
        for(i = 0; i<100;i++)
        {
            ;
        }
            setFeedVolts(); 
    }
	
}
/************************************
* Initialize for ramping up
************************************/
void Init_RampUp(short direction)
{
    //
    // If direction is positive add to middle
    //
    if(direction > 0)
    {
        RAMPCUR_SPEED = RAMPSTART_SPEED + NEUTRAL; 
        //
        // Calculate ramping steps
        //
        rampTicSize = rampUp/(SPEEDF - RAMPCUR_SPEED); 
    }else if(direction < 0)
    { //
        // Otherwise subtract from middle
        //
        RAMPCUR_SPEED = NEUTRAL - RAMPSTART_SPEED;
        //
        // Calculate ramping steps
        //
        rampTicSize = rampUp/(RAMPCUR_SPEED - SPEEDR);
    }
    //
    // Set ramping up flag 
    //
    FEATURE1_RAMPINGUP = 1;
    //
    // Clear ramping down flag
    //
    FEATURE1_RAMPINGDOWN = 0;
    
}
/***********************************
* Initialize for raming down
* Calculate the ramp down point and
* Ramp down is inextricable from auto functions
***********************************/
void Init_RampDown(short direction)
{
    //
    // If going up in length
    //
    if(direction > 0)
    {
        rampDownPoint = ((minTarget + maxTarget)/2) - rampDown;
    }else if(direction < 0)
    { //
        // If going down in length
        //
        rampDownPoint = rampDown + ((minTarget + maxTarget)/2);
    }
    
    
}
/***********************************************************
* Ramp tics 
* Used for ramping up or down
* Checks direction and compares the current length with the
* ramp down point; if reached the ramp down flag is set
* BigO 2.15uS 
* With a 36.8Mhz bus
*********************************/
void RampTic(int dir, int currentLength)
{
    //
    // If the ramping down point is reached
    //
    if((dir > 0) && (currentLength >= rampDownPoint))
    {/*configure for ramping down*/
        FEATURE1_RAMPINGDOWN = 1;
        FEATURE1_RAMPINGUP = 0;
        
    }else if(dir < 0 && (currentLength <= rampDownPoint))
    {/*configure for ramping down*/
        FEATURE1_RAMPINGDOWN = 1;
        FEATURE1_RAMPINGUP = 0;    
    }  /*6*/
    //
    // Ticking logic
    //
    if(!FEATURE1_RAMPINGUP && !FEATURE1_RAMPINGDOWN)
    {/*If Maximum ramp up speed is reached do nothing*/
        //
        //
    } else if(dir > 0 && RAMPCUR_SPEED >= SPEEDF && FEATURE1_RAMPINGUP)
    {/*If ramping up and top forward speed is reaced, end ramping*/
        FEATURE1_RAMPINGDOWN = 0;
        FEATURE1_RAMPINGUP = 0;   
        //
        // 
    } else if(dir < 0 && RAMPCUR_SPEED <= SPEEDR && FEATURE1_RAMPINGUP)
    { /*If ramping up and top reverse speed is reaced, end ramping*/
        FEATURE1_RAMPINGDOWN = 0;
        FEATURE1_RAMPINGUP = 0;   
        //
        //
    }else if(++rampTic >= rampTicSize) /*2*/
    { //
        // If tic equals ticSize, then.. 
        // 
        // If ramping up
        //
        if(FEATURE1_RAMPINGUP)
        {
            //
            // Increase ramping speed
            // depending on direction
            //
            if(dir > 0)
            {
                RAMPCUR_SPEED++; 
            }else if(dir < 0)
            {
                RAMPCUR_SPEED--;
            }
            
            
        }else if(FEATURE1_RAMPINGDOWN)
        { //
            // If ramping down
            //
            
            //
            // Presume current ramp speed is valid
            //
            if(dir > 0)
            {
                RAMPCUR_SPEED--;
            }else if(dir < 0)
            {
                RAMPCUR_SPEED++;
            }
        } /*5*/ 
        slowWheels(dir,RAMPCUR_SPEED);/*1.77uS*/
        rampTic = 0;/*reset ramp ticker*/  
    }/*14 + 1.77uS = 2.15uS*/
    
}
//******************
void slowFor(void)
{
    if( !FEATURE1_SAWOUT )
    {
        setFeedVolts();
        slowWheels(1,SLOW_SPEED);
    }
}
/****************************************************************************
* Reverse
* Changes:
* added sawM(1) on 01.05.2010
*************/
void reverse()
{
    int i = 0;
    // Hold saw up during feed operations
    sawM(1);
    //-----------
    if( !FEATURE1_SAWOUT )
    {
        //Cause the Bottom knife close command to be ignored for the 
        // feed sequence
        OPFLG1_BKNIFE = 1;		   		
        // Set Wheel speed/proportional valve voltage REVEARSE.
        (void)wheels(-1);
        //
        // Delay
        //
        for(i = 0; i<100; i++)
        { ; }
        (void)setFeedVolts();
        OPFLG1_BKNIFE = 0;
    }
}
/****************************************************************************
* Slow Reverse
****************************************************************************/
void slowRev(void)
{
    if( !FEATURE1_SAWOUT )
    {
        setFeedVolts();
        slowWheels(-1,SLOW_SPEED);
    }
}
//****************************************************************************
/***************************************************
* Top Knife activate
* Big O: 5 + SPI2_Output_Word(...) => 136nS + 380nS = 516nS
***************************************************/
void topK(short openClose)
{ 
    // Less than 0 is close, more than 0 is open.
	
    // Set top knife pressure relief to maximum.
    // ie max pressure available.
    
    // Associated valves:
    //  -DAC5
    //  -ON/OFF1a
    //  -ON/OFF1b
    
    //
    // Set maximum pressure (max duty cycle).
    //
    PWM1_DUTYCYCLE = MAXTKPRES;
    //
    if(openClose > 0)
    { //
            // Set Top knife open voltage:
            // 
            KNIFE_TOP_CLOSE = 0;
            KNIFE_TOP_OPEN = 1;
            //
            // Set movement flag high
            //
            OPFLG1_TKNIFE = 1;
        
    }else if(openClose < 0)
    {
            // Set Top knife close voltage:
            KNIFE_TOP_CLOSE = 1;
            KNIFE_TOP_OPEN = 0;
            //
            // Set movement flag high	
            //
            OPFLG1_TKNIFE = 1;
    }
    if(openClose == 0)    
    {
            // Set Top knife neutral voltage
            KNIFE_TOP_CLOSE = 1;
            KNIFE_TOP_OPEN = 1;
            //
            // Set movement flag low, stopped
            //
            OPFLG1_TKNIFE = 0;  
    }
    
}
//****************************************************************************
/***************************************************
* Bottom Knife activate
* Big O: 5 + SPI2_Output_Word(...)=> 136nS + 380nS = 516nS
***************************************************/
void botK(short openClose)
{
    // Less than 0 is close More than 0 is open.
	
    // Set bottom knife pressure relief to maximum.
    // ie max pressure available.
    
    // Associated valves:
    //  -ON/OFF2a
    //  -ON/OFF2b
	
    //
    // Set maximum pressure 
    //
    PWM0_DUTYCYCLE = MAXBKPRES;
	
    if(openClose > 0)
    { //
            // Set Bottom knife open voltage:
            //
            KNIFE_BOTTOM_CLOSE = 0;// v4.x=>PT1AD0_PT1AD06
            KNIFE_BOTTOM_OPEN = 1;// v4.x=>PT1AD0_PT1AD07
            //
            // Set movement flag high
            //
            OPFLG1_BKNIFE = 1;
    }else if(openClose < 0)
    { //
            // Set Bottom knife close voltage:
            //
            KNIFE_BOTTOM_CLOSE = 1;// v4.x=>PT1AD0_PT1AD06
            KNIFE_BOTTOM_OPEN = 0;// v4.x=>PT1AD0_PT1AD07
            // Set movement flag high
            OPFLG1_BKNIFE = 1;
    }else 
    { //
            // Set Bottom knife NEUTRAL voltage:
            //
            KNIFE_BOTTOM_CLOSE = 1;// v4.x=>PT1AD0_PT1AD06
            KNIFE_BOTTOM_OPEN = 1;// v4.x=>PT1AD0_PT1AD07
            //
            // Set movement flag low, stopped
            //
            OPFLG1_BKNIFE = 0;
    }
    
}
//****************************************************************************
/***************************************************
* Wheel Arm activate
* Big O: 5 + SPI2_Output_Word(...) => 136nS + 380nS = 516nS
***************************************************/
void whArms(short openClose)
{
    // Set Wheel arm  pressure relief to maximum.
    // ie max pressure available.

    // Associated valves:
    //  -DAC2
    //  -ON/OFF3a
    //  -ON/OFF3b
    
    // Set maximum pressure 
    //
    PWM2_DUTYCYCLE = MAXWAPRES;/*AD0 is Other, added temp for a burnt port*/	

    if(openClose > 0)
    {  //
        // Set Wheel Arm  open voltage:
        //
        ARM_WHEEL_CLOSE = 0;
        ARM_WHEEL_OPEN = 1;
        //
        // Set movement flag high
        //
        OPFLG1_WARM = 1;
    }else if(openClose < 0)
    { //
        // Set Wheel Arm close voltage:
        //
        ARM_WHEEL_CLOSE = 1;
        ARM_WHEEL_OPEN = 0;
        //
        // Set movement flag high
        //
        OPFLG1_WARM = 1;		
    }else  
    { //
        // Set Wheel arm NEUTRAL voltage: 
        //
        ARM_WHEEL_CLOSE = VALVEON;
        ARM_WHEEL_OPEN = VALVEON;
        //
        // Set movement flag low, Stopped
        //
        OPFLG1_WARM = 0; 
    }
    
}
//****************************************************************************
// Wheels function
// Changes from 10.23.2006
// Movements are set to the same direction.
// In order for wheels to be oppisite, as required, the flow lines
// must be physically changed.
// BigO: 4 + SPI1_Output_Word(...) =  136nS + 924nS = 1.06uS

void wheels(short volatile direction)
{
    // Speed is between 0 and 0x7E.
    // Set direction and speed. 
    if(!FEATURE1_SAWOUT)/* changed from: OPFLG1_MAINSAW, on 05.18.2010 */
    {
        if(direction > 0) //forward&& !sawing
        {  
            
            // DAC6=speed; //Set wheel to forward direction (at whatever speed).
            SPI1_Output_Word(WHEEL_A,(char)SPEEDF);// DAC1
            //
            // Set Ramp speed to be the same
            //
            RAMPCUR_SPEED = (short)SPEEDF;      	    
        }	
        if(direction < 0)   // reverse&& !sawing
        {
            // DAC6=speed; //Set wheel to reverse direction (at whatever speed).
            
            SPI1_Output_Word(WHEEL_A,(char)SPEEDR);	
            //
            // Set Ramp speed to be the same
            //
            RAMPCUR_SPEED = (short)SPEEDR;   		  
        }   
        
    }
  	
	if(direction == 0) {   	  
        // Set wheel motion valve to NEUTRAL.
        //Set wheel to neutral (at whatever speed).
		// send NEUTRAL again	
		//SPI1_Output_Word(WHEEL_B,(char)NEUTRAL);
		SPI1_Output_Word(WHEEL_A,(char)NEUTRAL);		
	}  
    
}
/***************************************************************************
* Slow Wheels function
* BigO 1.77uS
* with 36.8Mhz bus
***************************************************************************/
void slowWheels(volatile short direction, volatile short speed)
{
    switch(direction)
    {
      case -1:
        if(!FEATURE1_SAWING)
        { // if added 02.11.2007 to stop a leep back problem
            //	SPI1_Output_Word(WHEEL_B ,(char)(NEUTRAL - SLOW_SPEED));//SLOWF 0x0D   
            
        	SPI1_Output_Word(WHEEL_A,(char)(NEUTRAL - speed));/*1.6uS*///SLOWR 0x7F0x05 
            
        }
        
        break;
      case 0:
        // Set wheel motion valve to NEUTRAL.
        // DAC6=neutral (0x7F); //Set wheel to neutral (at whatever speed).
        SPI1_Output_Word(WHEEL_A,(char)NEUTRAL);
        
        // set NEUTRAL to zero            		
        // send NEUTRAL again	
        //SPI1_Output_Word(WHEEL_B,(char)NEUTRAL);
        
        break;
      case 1:
        if(!FEATURE1_SAWING)
        {  // if added 02.11.2007 to stop a leep back problem
            // DAC6=speed; //Set wheel to forward direction (at whatever speed).   	  
            //SPI1_Output_Word(WHEEL_A,(char)(NEUTRAL + SLOW_SPEED));// DAC1  SLOWF
            
            SPI1_Output_Word(WHEEL_B,(char)(NEUTRAL + speed));// DAC2 SLOWR
            
        }
        
        break;         
    }
}
//****************************************************************************
void tilt(short upDown)
{	
	
	if(upDown > 0)
	{
        // Set tilt up voltage.
        TILT_DOWN = 0;
        TILT_UP = 1;
        OPFLG1_TUP = 1;
        OPFLG1_TDOWN = 0;
	}else if(upDown < 0)
	{
        // Set tilt down voltage.	
        TILT_DOWN = 1;
        TILT_UP = 0;
        OPFLG1_TUP = 0;
        OPFLG1_TDOWN = 1;
	}else if(upDown == 0)
	{
        // Set tilt valve to NEUTRAL voltage.	
        TILT_DOWN = 1;
        TILT_UP = 1;
        OPFLG1_TUP = 0;
        OPFLG1_TDOWN = 0;  
	}
	
}
//****************************************************************************
// If upDown is more than 0 the saw goes up;
// If upDown is less than 0 the saw goes down;
// If upDown is zero the saw stops
void sawM(short upDown)
{	
	// Associated valves:
	//  -ON/OFF5a
	//  -ON/OFF5b
	
	if(upDown > 0)
	{
            // Set main saw up voltage.
            FEATURE1_SAWING = FALSE;
            SAW_MAIN_DOWN = VALVEOFF;
            SAW_MAIN_UP = VALVEON;
            // Set movement flag high
            // 01.11.2010
            // REMOVED: intend to hold saw up during feeding ops.
            //OPFLG1_MAINSAW = 1;
	}else if(upDown < 0)
	{
            //
            // If protect log is selected and length is outside the zone,
            // send an error and break;
            //
            if(FEATURE1_PROTECTLOG && ((minTarget > length_rotation_int) || (maxTarget < length_rotation_int)))
            {
                SCI0_Output_Word(NOTINWINDOW);
            }else
            {
                //
                // Set Main saw down voltage.
                //
                FEATURE1_SAWING = TRUE;
                FEATURE1_USEAUTOFEED = FALSE;
                //squzHold();
                SAW_MAIN_DOWN = VALVEON;
                SAW_MAIN_UP = VALVEOFF;
                // Set movement flag high
                OPFLG1_MAINSAW = 1;
            }
	}else   
        {
            //
            // Set main saw valve voltage to LOCK  voltage on both sides
            //
            SAW_MAIN_DOWN = VALVEON;
            SAW_MAIN_UP = VALVEON;
            // Set movement flag low, Stopped 
            OPFLG1_MAINSAW = 0;
        }
    
}
//****************************************************************************
void sawT(short upDown)
{	
	// Associated valves:
	//  -ON/OFF6a
	//  -ON/OFF6b
	
	if(upDown > 0)
	{
            // Set top saw up voltage.
            SAW_TOP_DOWN = VALVEOFF;
            SAW_TOP_UP = VALVEON;
            // Set movement flag high
            OPFLG1_TOPSAW = 1;
	}else if(upDown < 0)
	{
            //
            // If protect log is selected and length is outside the zone,
            // send an error and break;
            //
            if(FEATURE1_PROTECTLOG && ((minTarget > length_rotation_int) || (maxTarget < length_rotation_int)))
            {
                SCI0_Output_Word(NOTINWINDOW);
            }else
            {
                //squzHold();
                FEATURE1_USEAUTOFEED = FALSE;
                // Set Main saw down voltage.	
                SAW_TOP_DOWN = VALVEON;
                SAW_TOP_UP = VALVEOFF;
                // Set movement flag high	
                OPFLG1_TOPSAW = 1;
            }
	}else       
	{
            //
            // Set main saw valve voltage to LOCK  voltage on both sides
            //	
            SAW_TOP_DOWN = VALVEON;
            SAW_TOP_UP = VALVEON;
            // Set movement flag low, Stopped
            OPFLG1_TOPSAW = 0;	  
	}
    
}
//*********************************************************************************
//service functions
void setFeedVolts()
{
    // Forward or Reverse feed requires the same set up for the arms.
	// This function does that operation. 
    // If the associated operational flag (OPFLG1_*) is not already high
    // i.e. the arm is not already active, set the feed voltages and movement.
    
    if(!OPFLG1_TKNIFE) 
    {// Set Top knife close voltage.     	
        topK(-1); 
        //
        //set pressure; top knife pressure reducing pwm duty cycle.  
        //          	 
        PWM1_DUTYCYCLE =  tkPR;  
    }
    if(!OPFLG1_BKNIFE)
    {// Set Bottom knife close voltage.   	
        botK(-1);
        // 
        // set pressure; duty cycle for pressure reducing pwm
        //
        PWM0_DUTYCYCLE = bkPR;
    }
    if(!OPFLG1_WARM)
    {// Set Wheel Arm close voltage.    	
        whArms(-1);	                                             
        //  
        // set pressure; duty cycle for pressure reducing pwm
        //
        PWM2_DUTYCYCLE = whPR;
    }   
	
}
/***********************
* returns 0 if movement is continued...1 if motion stopped
***********************/
/************************
* BigO: 3.04uS
* With 36.8Mhz bus
***********************/
short targetHit(int currentLength)
{
    short overrun = 0;
    short direction = 0;
    
    //
    // If length is inside the min and max length
    // stop movement and return true
    //
    if(minTarget < currentLength && maxTarget >= currentLength) 
    {
        // when the overrun distance is less than the stop
        // window stop movement and return '1' for target reached      
        stp();/*2.88uS*/
        return 1;         
    }
    //
    // If the length exceeds the maximum target, reverse
    //
    if(maxTarget > currentLength)
    {
        //
        // Initialize (reset) ramps 
        //
        Init_RampUp(-1);
        Init_RampDown(-1);
        //
        // Begin movement
        //
        slowWheels(-1,RAMPCUR_SPEED);/*MUST ensure ramp speed accuracy*/
        
    }
    
    return 0;
    
    
}
//**********************************************************************************
//************************* END Secondary Command Definitions **********************
void OutputSettings(void)
{
    char ENCODER_OUTPUT[32] = "$nop,sset,";
    char tempC[6] = "";
    int temp = 0;
    //
    // convert setting to char arrays
    //
    (void)_itoa(NEUTRAL,/*array*/tempC,4);
    //
    // Concatinate to message
    //
    (void)strcat(ENCODER_OUTPUT,tempC);
    (void)strcat(ENCODER_OUTPUT,",");
    //
    // convert setting to char arrays
    //
    (void)_itoa(SPEEDF,/*array*/tempC,4);
    //
    // Concatinate to message
    //
    (void)strcat(ENCODER_OUTPUT,tempC);
    (void)strcat(ENCODER_OUTPUT,",");
    //
    // convert setting to char arrays
    //
    (void)_itoa(SPEEDR,/*array*/tempC,4);
    //
    // Concatinate to message
    //
    (void)strcat(ENCODER_OUTPUT,tempC);
    (void)strcat(ENCODER_OUTPUT,",\r\n");
    //
    // Send the information
    //
    SCI0_Output_Word(ENCODER_OUTPUT);  
}

//******************************
// END OF HarvesterValve Header
//******************************
// Test Functions
//
// On Off
//
void Test_OnOffs(short state)
{
    //
    // Set Top knife
    //
    KNIFE_TOP_CLOSE = state;
    KNIFE_TOP_OPEN = state; 
    // 
    // Set Wheel arm	
    //
    ARM_WHEEL_CLOSE = state;
    ARM_WHEEL_OPEN = state; 	
    //
    // Set main saw	
    //
    SAW_MAIN_DOWN = state;
    SAW_MAIN_UP = state;  
    //
    // Set top saw
    //
    SAW_TOP_DOWN = state;
    SAW_TOP_UP = state;
    //
    // Set tilt valve
    //
    TILT_DOWN = state;
    TILT_UP = state;	
    //  
    // Set Bottom knife
    //
    KNIFE_BOTTOM_CLOSE= state;
    KNIFE_BOTTOM_OPEN= state; 
}
//
// PWM
//
void Test_PWM()
{
    
}

#endif