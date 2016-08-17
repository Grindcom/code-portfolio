//***********************************************************
//
// persist_lib.h - Persistant library functions, used to store 
//                  parameters in flash memory
//
//  Copyright (c) 2011 - 2020 Gregory Industrial Computer Ltd. (Grindcom)
//  All rights reserved
//
//************************************************************************

#ifndef _PERSIST_LIB_H
#define _PERSIST_LIB_H
#include <stdint.h>
#include <stdbool.h>

#include "ComUART.h"

enum inputLevel {LEVEL_1,LEVEL_2,PRESET};

enum PB_OFFSETS{  
  //****************
  // Input command locations
  L1_ADDRESS_OFFSET=0x02, L2_ADDRESS_OFFSET=0x82, PRESET_ADDRESS_OFFSET=0x102,
  //***************
  // Other parameter locations
  HM_ADDRESS_OFFSET         =0x200, 
  WHEEL_SPEED_OFFSET        =0x211, 
  MIDDLE_SPEED_OFFSET       =0x213,
  TOPK_PRESS_OFFSET         =0x215, 
  TOPKMAX_PRESS_OFFSET      =0x217,
  BOTK_PRESS_OFFSET         =0x219,
  BOTKMAX_PRESS_OFFEST      =0x21B, 
  WHEELARM_PRESS_OFFSET     =0x21D, 
  WHEELARMMAX_PRESS_OFFSET  =0x21F, 
  OTHER_PRESS_OFFSET        =0x221, 
  OTHERMAX_PRESS_OFFSET     =0x223,
  OVER_TARGET_OFFSET        =0x225,
  UNDER_TARGET_OFFSET       =0x229,
  // next must start at     =0x233
};
//*********************************************
// Container to hold head module settings
//
struct HM_SETTINGS{
  char hm_address[18];
  char wh_speed[3];
  char wh_mid[3];
  char tk_press[3];
  char tk_max[3];
  char bk_press[3];
  char bk_max[3];
  char wa_press[3];
  char wa_max[3];
  char other_press[3];
  char other_max[3];
  char over_target[5];
  char under_target[5];
};
class cmd_persist
{
public:
  cmd_persist(ComUART *comPort);
  ~cmd_persist();
  //***************************************************
  // Command Line functions
  //
  int PersistCMDline(char *pcCmdLine);  
  //***************************************************
  // Flash functions
  //
  int ProcessTest( int argc, char *argv[]);
  int ProcessParameterBlock(int argc, char *argv[]);
  int ProcessPBSave(int argc, char *argv[]);
  int ProcessChangeCP(int argc, char *argv[]);
  int ProcessPBReset(int argc, char *argv[]);
  int ProcessGetCP(int argc, char *argv[]);
  //***************************************************
  // Send raw commands
  //
  void sendRaw(int input, inputLevel il, int uart);
  void sendRawL1(int input, int uart);
  void sendRawL2(int input, int uart);
  void sendRawPreSet(int index, int uart);
  void sendRawMinTarget(int index, int uart);
  void sendRawMaxTarget(int index, int uart);
  void sendRawSpeed(int uart);
  void sendRawMid(int uart);
  void sendRawTKP(int uart); //Top Knife Pressure
  void sendRawBKP(int uart); //Butt Knife Pressue
  void sendRawWAP(int uart); //Wheel Arm Pressure
  //***************************************************
  // Compare commands
  //
  bool cmpL1(int input, char *cmd, int cmdlen);
  bool cmdL2(int input, char *cmd, int cmdlen);
  bool notNONE(int input, inputLevel il);
  //***************************************************
  // Peek commands, check command at input index
  //
  bool peekShift(int input, inputLevel lvl=LEVEL_1);
  bool peekMainSaw(int input, inputLevel lvl);
  bool peekTopSaw(int input, inputLevel lvl);
  bool peekPreSet(int input, inputLevel lvl=LEVEL_1);
  bool peekPumpCMD(int input, inputLevel lvl=LEVEL_1);
  bool peekLock(int input, inputLevel lvl);
  bool peekNeut(int input, inputLevel lvl);

  //***************************************************
  // Load a parameter from Flash parameter block
  //
  void getParameter(PB_OFFSETS p_offset, char *param, int p_size);
  //***************************************************
  // Get pre set index value
  //
  int getPreSetIndex(inputLevel levelSelect, int input);
  //*******************************************************************
  // Globals used to access Flash locations.
  // Use in conjuction with getParameter(...) function
  //

private:
  ComUART *uartPTR;
  char L1cmd[32][5], L2cmd[32][5], preSet[31][9];
//  /* Any changes to the above array sizes will mandate changes
//  in the functions where they are addressed:
//  ProcessPBInit, ProcessPBSave, ProcessChangeCP, initFlash */
  //*******************************************
  // setting pointers
  //
  HM_SETTINGS *head_settings; 
  //***************************************************
  // Flash location
  //
  const unsigned long FLSTRT;
  const uint32_t CMDSTRT; 
  const unsigned long FLEND;
  const unsigned long FLBLKEND;
  const unsigned long FLBLKSIZE;// 1kb flash block
  const unsigned int PBSIZE;  
  //
  //
  //***************************************************
  //
  int minTarget;
  int maxTarget;

  

  //***************************************************
  // Private functions
  //
  bool ProcessPBInit(void);
  //
  bool FirstTimePBsetup(char cmdInit[], char presInit[], 
                        char hm_addr[],char hm_settings[]);
  //
  int ProcessWriteFlash( int argc, char *argv[] );
  int ProcessReadFlash(int argc, char *argv[]);
  int ProcessFlashErase();//int argc, char *argv[]
  //
  void sendArray(char *arrToSend, int sizeofArr);
  //
  void fillArray(char *arrToFill, PB_OFFSETS arr_offset, int p_size);
  //
  // Load a large array
  //
  int loadArr(char * seedArr,char * targetArr, 
               int sizeoftarget);
  
};

#endif //_PERSIST_LIB_H