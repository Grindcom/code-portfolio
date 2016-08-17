//***********************************************************
//
// GPIO.cpp - Library to utilize GPIO pins for operator input
//                 
//
//  Copyright (c) 2011 - 2020 Gregory Industrial Computer Ltd. (Grindcom)
//  All rights reserved
//
//************************************************************************

#include "GPIO.h"

#include <stdint.h>
#include <stdbool.h>
#include "inc/hw_memmap.h"
#include "driverlib/gpio.h"
#include "inc/hw_ints.h"
#include "driverlib/rom.h"
#include "driverlib/pin_map.h"
#include "driverlib/sysctl.h"


/********************************
* CONSTRUCTOR
*******************************/
GPIO::GPIO()
: PORTSIZE(8),pFindex(0),pKindex(8),
pNindex(16),pJindex(24)
{
    GPIO_init();
    gpio_int_enable();
    portF[PORTSIZE] = '\0';
    portK[PORTSIZE] = '\0';
    portN[PORTSIZE] = '\0';
    portJ[PORTSIZE] = '\0';
    for(int i = 0; i < 32; i++)
    {
        combinedPort[i] = '0';
        combinedPort[i+1]='\0';
    }
    //if(1){}
}
/********************************
* DESTRUCTOR
*******************************/
GPIO::~GPIO()
{
    
}
/******************************************************************
*
*       CONSTRUCTOR HELPERS
*
*******************************************************************/
/*******************************************************
//  
// GPIO enable operations
//
********************************************************/
void GPIO::GPIO_init(void)
{
    //**********************************************
    // Initialize/Enable GPIO ports being used
    //
    ROM_SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOF);    
    ROM_SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOK);
    ROM_SysCtlPeripheralEnable(SYSCTL_PERIPH_GPION);
    ROM_SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOJ);
    //********************************************************
    // Enable Port K for input and interrupt on both edges
    //    
    //
    GPIOPinTypeGPIOInput(GPIO_PORTK_BASE, PORTUSEBITS);// 0xFF -> All pins
    GPIOPadConfigSet(GPIO_PORTK_BASE, PORTUSEBITS, GPIO_STRENGTH_2MA,
                     GPIO_PIN_TYPE_STD_WPD/*GPIO_PIN_TYPE_OD_WPD */);
    GPIOIntTypeSet(GPIO_PORTK_BASE, PORTUSEBITS, GPIO_BOTH_EDGES);//GPIO_RISING_EDGE
    GPIOIntEnable(GPIO_PORTK_BASE, PORTUSEBITS);
    
    //********************************************************
    // Enable Port F for input and interrupt on both edges
    //
    GPIOPinTypeGPIOInput(GPIO_PORTF_BASE, PORTUSEBITS);/* 0xFF -> ALL PINS */
    GPIOPadConfigSet(GPIO_PORTF_BASE, PORTUSEBITS, GPIO_STRENGTH_2MA,
                     GPIO_PIN_TYPE_STD_WPD);    
    GPIOIntTypeSet(GPIO_PORTF_BASE, PORTUSEBITS, GPIO_BOTH_EDGES);
    GPIOIntEnable(GPIO_PORTF_BASE, PORTUSEBITS);   
  
    //********************************************************
    // Enable Port J for input 
    //  and interrupt on both edges
    //
    GPIOPinTypeGPIOInput(GPIO_PORTJ_BASE, PORTUSEBITS);
    GPIOPadConfigSet(GPIO_PORTJ_BASE, PORTUSEBITS, GPIO_STRENGTH_2MA, 
                     GPIO_PIN_TYPE_STD_WPD);  
    GPIOIntTypeSet(GPIO_PORTJ_BASE, PORTUSEBITS, GPIO_BOTH_EDGES);
    GPIOIntEnable(GPIO_PORTJ_BASE, PORTUSEBITS);   
    
    //********************************************************
    // Enable Port N for input and interrupt 
    //   on both edges
    //
    GPIOPinTypeGPIOInput(GPIO_PORTN_BASE, PORTUSEBITS);
    GPIOPadConfigSet(GPIO_PORTN_BASE, PORTUSEBITS, GPIO_STRENGTH_2MA, 
                     GPIO_PIN_TYPE_STD_WPD);  
    GPIOIntTypeSet(GPIO_PORTN_BASE, PORTUSEBITS, GPIO_BOTH_EDGES);
    GPIOIntEnable(GPIO_PORTN_BASE, PORTUSEBITS);   
    
    //*************************************************************
    // Safety Ports
    //
    ROM_SysCtlPeripheralEnable(SYSCTL_PERIPH_GPIOA);
    GPIOPinTypeGPIOInput(GPIO_PORTA_BASE, GPIO_PIN_6 | GPIO_PIN_7);
    GPIOPadConfigSet(GPIO_PORTA_BASE, GPIO_PIN_6 | GPIO_PIN_7, 
                     GPIO_STRENGTH_2MA, GPIO_PIN_TYPE_STD_WPD);  
    GPIOIntTypeSet(GPIO_PORTA_BASE, GPIO_PIN_6 | GPIO_PIN_7, GPIO_BOTH_EDGES);
    GPIOIntEnable(GPIO_PORTA_BASE, GPIO_PIN_6 | GPIO_PIN_7);     

    //
    // Enable interrupts
    //
    gpio_int_enable();
    //
    // Initialize port char arrays
    //
    for(int z = 0; z < PORTSIZE; z++)
    {
        portF[z] = portK[z] = portN[z] = portJ[z] = '0';
    }
    portF[PORTSIZE] = portK[PORTSIZE] = 
        portN[PORTSIZE] = portJ[PORTSIZE] = '\0';
    for(int k = 0; k < 32; k++)
    {
        combinedPort[k] = '0';
    }
    
    
}// End of GPIO intialize
//*********************************************************
// Enable GPIO interrupts for indicated ports
//
void GPIO::gpio_int_enable(void)
{
    ROM_IntEnable(INT_GPIOJ);
    ROM_IntEnable(INT_GPION);
    ROM_IntEnable(INT_GPIOF); 
    ROM_IntEnable(INT_GPIOK);  
    ROM_IntEnable(INT_GPIOA);
}

/********************************************************************
*
*           REGULAR FUNCTIONS
*
*********************************************************************/
/***********************************************************
* Get a ports char value at position n
*
***********************************************************/
char GPIO::getBit(const char port, short n)
{
    if(n < 0 || n > PORTSIZE)
        return 'e';
    if(port != 'f' && port != 'k' 
           && port != 'n' && port != 'j' )
        return 'e';
    char* portPTR;    
    switch(port)
    {
      case 'f':
        portPTR = portF;
        break;
      case 'k':
        portPTR = portK;
        break;
      case 'n':
        portPTR = portN;
        break;
      case 'j':
        portPTR = portJ;
        break;
      default:
        return 'e';
        break;
    }  
    return portPTR[n];
}

/*******************************
* Convert port bits to char
*********************************/
void GPIO::bToc(const char port, short byte)
{
    short index = 0;
    char* portPTR;
    switch(port)
    {
      case 'f':
        portPTR = portF;
        index = pFindex;
        break;
      case 'k':
        portPTR = portK;
        index = pKindex;
        break;
      case 'n':
        portPTR = portN;
        index = pNindex;
        break;
      case 'j':
        portPTR = portJ;
        index = pJindex;
        break;
      default:
        break;
    }
    //
    // Shift bytes into seleced port array
    //
    shiftByte(portPTR,byte,PORTSIZE);
    for(int i = 0; i < PORTSIZE; i++)
    {
        combinedPort[index + i] = portPTR[i];
    }    
}
/*******************************************************************
*
* Private function that will shift the indavidual bits of a byte
* one at a time converting the one or zero to a char value and
* placing them into a char array of a given size
*
*******************************************************************/
void GPIO::shiftByte(char* portString, short byte, int portsize)
{
    bool bitVal = false;  
    
    for(int i = portsize - 1; i >= 0; i--)
    {
        // get the value of the least significant bit
        bitVal = byte & 1;
        // if the lsb is high set the char as "1"
        if(bitVal)
        {
            portString[i] = '1';
        }else// otherwise the value is "0"
        {
            portString[i] = '0';
        }
        // rotate to the next lsb
        byte >>= 1;
    }  
}
/*********************************************
* Get a pointer to the ports string value
********************************************/
const char* GPIO::getPort(const char portname)
{
    switch(portname)
    {
      case 'f':
        return portF;
        break;
      case 'k':
        return portK;
        break;
      case 'n':
        return portN;
        break;
      case 'j':
        return portJ;
      case 'z':
        return combinedPort;
      default:
        return 0;
    }
}  
/****************************************
* put a value on a port
*****************************************/
void 
GPIO::putPort(const char portname,unsigned short index, char value)
{
//    switch(portname)
//    {
//      case 'D':// Change to the Danfoss related ports
//        //
//        // put on port e[index]
//        //
//        if(index == 6 || index == 7)
//        {
//            portE[index] = value; 
//        }
//        break;
//      default:
//        break;
//    }
}