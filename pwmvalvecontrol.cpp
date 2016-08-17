/********************************************************************
*
*  pwmvalvecontrol.cpp is a library of functions that are intended
*   to control high current hydraulic valves with a pwm signal
*
*   Created by: Greg Ford, B.Sc.
*   Grindcom
*   Copyright 2014, all rights reserved
*
**********************************************************************/
#include "pwmvalvecontrol.h"
#include <stdint.h>
#include <stdbool.h>
#include "inc/hw_memmap.h"
#include "inc/hw_types.h"
#include "driverlib/sysctl.h"
#include "driverlib/pwm.h"
#include "driverlib/gpio.h"
#include "driverlib/pin_map.h"


/*********************************************************************
*
*           CONSTRUCTOR
*
**********************************************************************/
pwmvalvecontrol::pwmvalvecontrol(float c_p0dc, float c_p1dc,
                                 float c_p2dc, float c_p3dc)
{
    //
    //ERROR NOTE: THE DUTY CYCLE VARIABLES DO NOT CORRESPOND TO THE
    //  EXPECTED PUMP PORTS and PUMP3 DOESN'T RESPOND TO DEACTIVATION
    //
    //
    // Set Duty cycle variables
    //
    p0dc = c_p0dc;
    p1dc = c_p1dc;
    p2dc = c_p2dc;
    p3dc = c_p3dc;
    
    //********************************************************
    // Enable the PWM's used here; PF0 & PF1
    //
    SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOH);  
    SysCtlPeripheralEnable(SYSCTL_PERIPH_PWM0);
    //********************************************************
    // Set the GPIO PH[0..3] as PWM pins. 
    //
    GPIOPinTypePWM(GPIO_PORTH_BASE, (GPIO_PIN_0|GPIO_PIN_1|
                                     GPIO_PIN_2|GPIO_PIN_3));
    //********************************************************
    // Configure F0 and F1 for PWM
    //
    GPIOPinConfigure(GPIO_PH0_M0PWM0);
    GPIOPinConfigure(GPIO_PH1_M0PWM1);
    GPIOPinConfigure(GPIO_PH2_M0PWM2);
    GPIOPinConfigure(GPIO_PH3_M0PWM3);
    //********************************************************
    // Get the period value based on the system clock
    //
    ulPeriod = SysCtlClockGet() / 50000;
    //********************************************************
    // Set the PWM period to kHz
    //
    PWMGenConfigure(PWM0_BASE, PWM_GEN_0,PWM_GEN_MODE_UP_DOWN|
                    PWM_GEN_MODE_NO_SYNC|PWM_GEN_MODE_DBG_RUN/**/);
    
    //********************************************************
    PWMGenConfigure(PWM0_BASE, PWM_GEN_1,PWM_GEN_MODE_UP_DOWN|
                    PWM_GEN_MODE_NO_SYNC|PWM_GEN_MODE_DBG_RUN/**/);
    
    
    PWMGenPeriodSet(PWM0_BASE, PWM_GEN_0, ulPeriod);  
    PWMGenPeriodSet(PWM0_BASE, PWM_GEN_1, ulPeriod);  
    //********************************************************
    // Set duty cycles
    //
    setDutyCycle(PWM_OUT_0, p0dc);
    setDutyCycle(PWM_OUT_1, p1dc);
    setDutyCycle(PWM_OUT_2, p2dc);
    setDutyCycle(PWM_OUT_3, p3dc);
    
    //  PWMPulseWidthSet(PWM0_BASE, PWM_OUT_0, int(ulPeriod * 0.5));/* 75% */
    //  PWMPulseWidthSet(PWM0_BASE, PWM_OUT_1, int(ulPeriod * 0.76));/* 75% 3 / 4*/
    //  PWMPulseWidthSet(PWM0_BASE, PWM_OUT_2, int(ulPeriod * 0.5));/* 75% */
    //  PWMPulseWidthSet(PWM0_BASE, PWM_OUT_3, int(ulPeriod * 0.45));/* 75% */
    //********************************************************
    // Enable the pwm generator
    //
    PWMGenEnable(PWM0_BASE, PWM_GEN_0);  
    PWMGenEnable(PWM0_BASE, PWM_GEN_1);
    //
    // Start pwm for test purpose's
    //
    PWMOutputState(PWM0_BASE, (/**/PWM_OUT_0_BIT),true);
    PWMOutputState(PWM0_BASE, (/**/PWM_OUT_1_BIT),true);
    PWMOutputState(PWM0_BASE, (/**/PWM_OUT_2_BIT),true);
    PWMOutputState(PWM0_BASE, (/**/PWM_OUT_3_BIT),true);
}
/*********************************************************************
*
*           DETRUCTOR
*
**********************************************************************/
pwmvalvecontrol::~pwmvalvecontrol()
{
    
}
/**********************************************************
*
* Activate pump pwm's
*
**********************************************************/

void pwmvalvecontrol::PWM_setState(uint8_t pump, bool state)
{
    //********************************************************
    // set the pwm's
    //
    PWMOutputState(PWM0_BASE, pump, state);
}

/**********************************************************
*
*       SET DUTY CYCLE
*
**********************************************************/
void pwmvalvecontrol::setDutyCycle(uint8_t pump, float pw)
{
    PWMPulseWidthSet(PWM0_BASE, pump, int(ulPeriod*pw));
    //
    // Update pump duty cycle
    //
    switch(pump)
    {
      case PWM_OUT_0:
        p0dc = pw;
        break;
      case PWM_OUT_1:
        p1dc = pw;
        break;
      case PWM_OUT_2:
        p2dc = pw;
        break;
      case PWM_OUT_3:
        p3dc = pw;
        break;
      default:
        break;
    }
    
}

/**********************************************************
*
*       GET DUTY CYCLE FOR SELECTED PUMP
*
**********************************************************/
float pwmvalvecontrol::getDutyCycle(uint8_t pump)
{
    switch(pump)
    {
      case PUMP0:
        return p0dc;
        break;
      case PUMP1:
        return p1dc;
        break;
      case PUMP2:
        return p2dc;
        break;
      case PUMP3:
        return p3dc;
        break;
      default:
        break;
    }
    return 0.0;
}