//***********************************************************
//
// GPIO.h - Library to utilize GPIO pins for operator input
//                 
//
//  Copyright (c) 2011 - 2020 Gregory Industrial Computer Ltd. (Grindcom)
//  All rights reserved
//
//************************************************************************


#ifndef GPIO_H
#define GPIO_H


#define PORTUSEBITS  0xFF
#define K_PORTUSEBITS 0xEF // May need to change with next board version


class GPIO
{
  public:
  short const PORTSIZE;/* Default port size */
  const short pFindex, pKindex, pNindex, pJindex;
  GPIO();
  ~GPIO();
  // Convert port bits to char
  void bToc(const char port, short byte);
  // Get a pointer to the ports string value
  const char* getPort(const char portname);
  //
  // Get a ports char value at position n
  //  Returns '1','0' for bit
  //  An invalid port or position will return
  //    'e'.
  char getBit(const char port, short n);
  //
  // put a value on a port at a given index
  //
  void putPort(const char portname, unsigned short index, 
               char value = 0);
  protected:
  void GPIO_init(void);
  void gpio_int_enable(void);
  //
  // Interrupt Handlers
  //
  void K_GPIOIntHandler(void);
  private:
  //
  // GPIO ports
  //
//  char portE[9];  
  char portF[9];
  char portK[9];
  char portN[9];
  char portJ[9];
  //
  char combinedPort[32];
  
  //
  // Private functions
  //
  void shiftByte(char* portname, short byte, int portsize);
  
};
#endif