/********************************************************************
*
*  pwmvalvecontrol.h is a library of functions that are intended
*   to control high current hydraulic valves with a pwm signal
*
*   Created by: Greg Ford, B.Sc.
*   Grindcom
*   Copyright 2014, all rights reserved
*
**********************************************************************/
#ifndef PWMVALVCONTROL_H
#define PWMVALVCONTROL_H
#include <stdint.h>
#include <stdbool.h>
#include "inc/hw_types.h"
#include "driverlib/sysctl.h"
#include "driverlib/pwm.h"
#include "driverlib/gpio.h"

/**********************************************
* PWM stuff
**********************************************/
#define PUMP0 PWM_OUT_2 //PWM_OUT_0
#define PUMP1 PWM_OUT_4    
#define PUMP2 PWM_OUT_0
#define PUMP3 PWM_OUT_3




class pwmvalvecontrol
{
  public:
  pwmvalvecontrol(float p0dc = 0.90, float p1dc = 0.90,
                  float p2dc = 0.90, float p3dc = 0.90);
  ~pwmvalvecontrol();
  
  void PWM_setState(uint8_t pump, bool state);
  void setDutyCycle(uint8_t pump, float pw = 0.75);
  float getDutyCycle(uint8_t pump);
  
  protected:
  
  private:
  float p0dc, p1dc, p2dc, p3dc;
  uint32_t ulPeriod;
  
};


#endif