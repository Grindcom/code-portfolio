/***********************************************************************************
*
* All information necessary to setup the XGATE for use as PWM
*
* Greg Ford, B.Sc.
* 04.13.2010
*
***********************************************************************************/


#ifndef XGATE_PWM_H
#define XGATE_PWM_H


//
// XGATE OPERATIONS
//
#include <string.h>
#include "xgate.h"

//
// Multiplier for period; use to rectify a dutycycle to the period ratio
//
volatile const short PWMPERIOD_MULTIPLIER = 1; /*if PWMPERIOD changes this number must as well */
//
/* this variable definition is to demonstrate how to share data between XGATE and S12X */
#pragma DATA_SEG SHARED_DATA
//
// Shared counter, possibly accessed from both cores.
//
volatile int shared_counter; /* volatile because both cores are accessing it. */
//
// PWM PERIOD used to set the period for all pwm ports
//
volatile int PWMPERIOD = 800;
//
//
//
volatile int PWMENABLE_DUTYCYCLE = 400;/* Enable */
//
// Duty cycles for indavidual PWM ports
//
volatile int PWM0_DUTYCYCLE = 200;
volatile int PWM1_DUTYCYCLE = 200;
volatile int PWM2_DUTYCYCLE = 200;
volatile int PWM3_DUTYCYCLE = 200;
#pragma DATA_SEG DEFAULT


#define ROUTE_INTERRUPT(vec_adr, cfdata)                \
  INT_CFADDR= (vec_adr) & 0xF0;                         \
  INT_CFDATA_ARR[((vec_adr) & 0x0F) >> 1]= (cfdata)

#define SOFTWARETRIGGER0_VEC  0x72 /* vector address= 2 * channel id */
#define PIT0_VEC 0x7A
#define PIT1_VEC 0x78
#define PIT2_VEC 0x76
#define PIT3_VEC 0x74
static void SetupXGATE(void) 
  {
  /* initialize the XGATE vector block and
     set the XGVBR register to its start address */
  XGVBR= (unsigned int)(void*__far)(XGATE_VectorTable - XGATE_VECTOR_OFFSET);

  /* switch software trigger 0 interrupt to XGATE */
  ROUTE_INTERRUPT(SOFTWARETRIGGER0_VEC, 0x81); /* RQST=1 and PRIO=1 */
  //
  // Swithch Periodic interrupt timer 2 interrupt to XGATE
  //
  ROUTE_INTERRUPT(PIT2_VEC,0x81);
  
  /* enable XGATE mode and interrupts */
  XGMCTL= 0xFBC1; /* XGE | XGFRZ | XGIE */

  /* force execution of software trigger 0 handler */
  XGSWT= 0x0101;

}
//
// END XGATE OPERATIONS
//
//
// Initialize PIT
//
void PIT2_Init(void)
{
    PITCE_PCE2 = 1;				         // Enable PIT channel 2 
    PITMTLD1 = 0;				           // Divide by 1 
    PITMUX_PMUX2 = 1;              // Assign PIT channel 2 to microtimer 1  
    //PIT.pitld2.word = 1320-1;		 // 150Hz @ 0.5% -> 33us/25ns   = 1320 
    PITLD2 = 74;
    // 
    // Enable the PIT module and for reload of the micro counter
    //
    //PIT.pitcflmt.byte = PITE | PITFRZ | PFLMT1;	// Enable the PIT module and force reload of the micro counter
    PITCFLMT_PITE = 1;
    PITCFLMT_PITFRZ = 1;
    PITCFLMT_PFLMT1 = 1;
    //PIT.pitflt.bit.pflt2 = 1;			              // Force reload of counter 2    
    PITFLT_PFLT2 = 1; 
    //PIT.pitinte.bit.pinte2 = 1;		              // Enable interrupts from channel 2 
    PITINTE_PINTE2 = 1;
  
  
}
//
// Initialize PWM duty cycles
//
void Init_DutyCycle(short pwm0, short pwm1, short pwm2, short pwm3)
{
    PWM0_DUTYCYCLE = pwm0;
    PWM1_DUTYCYCLE = pwm1;
    PWM2_DUTYCYCLE = pwm2;
    PWM3_DUTYCYCLE = pwm3;
}
#endif