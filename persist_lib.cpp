//***********************************************************
//
// persist_lib.c - Persistant library functions, used to store 
//                  parameters in flash memory
//
// Updated: Jan. 22, 2014
//
//  Copyright (c) 2011 - 2020 Gregory Industrial Computer Ltd. (Grindcom)
//  All rights reserved
//
//    FlashUsecSet(SysCtlClockGet()); // DEPRICATED IN THIS VERSION
// Once testing is complete remove references to function above.
//************************************************************************
#include "cmdline.h"
#include "utils/flash_pb.h"
#include "driverlib/flash.h"
#include "inc/hw_types.h"
#include "HMCMcmd.h"
#include "string_util.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
//
#include "persist_lib.h"
#include "ComUART.h"
//
#include "driverlib/sysctl.h"
//
// cmdline functions
// This function reverses the order of argv as it sends it back
// argc is the number of words in argv[]
// CmdLineProcess(...) has been addapted to remove ',' as well as ' '
// characters when reading the buffer.
// Also prints the help line for the function
//


//**********************************************************************
// Constructor
//
cmd_persist::cmd_persist(ComUART *comPort):
  FLSTRT(0x30000), CMDSTRT(0x30000), FLEND(0x31FFF),
  FLBLKSIZE(0x400), FLBLKEND(0x32000),//FLBLKEND CHANGED FROM 0x30800 
  PBSIZE(1024)
  {
      uartPTR = comPort;
      head_settings = new HM_SETTINGS;
      
      minTarget = 100;
      maxTarget = 100;
      
      //  ProcessFlashErase();// only use to Clear a new Flash
      //********************************************
      // Initialize the flash parameter block AND
      // the accessing array's
      // 
      if(!ProcessPBInit())
      {
          uartPTR->putString("PB MEM Corrupted!!! Attempting to restore.",CMDLINEPORT);
          //***************************
          // if there is no valid parameter block
          // create a default one.
          //FirstTimePBsetup()
          if(FirstTimePBsetup("none","90000001","00:00:00:00:00:00","9291"))
          {
              //***********************************
              // Try again, one time, to inialize
              //
              ProcessPBInit();
              uartPTR->putString("PB MEM Restore Complete.  Values reset to default.",CMDLINEPORT);
              //
          }else
          {
              //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
              // Some how cause a jump to a Fault handler
              // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  
              uartPTR->putString("PB MEM Restore FAIL...",CMDLINEPORT);
          }
      }
      
      
  }
  //
  // Destructor
  //
  cmd_persist::~cmd_persist()
  {
      delete uartPTR; 
      delete head_settings;
  }
  //
  // Testing function
  //
  int cmd_persist::ProcessTest( int argc, char *argv[])
  {
      tCmdLineEntry *pCmdEntry1 = &g_sCmdTable[0];// global found in cmdline.h
      uint32_t* pulData;
      unsigned long memAddress = CMDSTRT;
      char blockStart[]="Block Start";
      char blockEnd[]="Block End";
      int temp = argc;
      while(--temp >= 0)
      {
          uartPTR->putString(argv[temp]);
      }
      uartPTR->putString(pCmdEntry1->pcHelp);
      //
      // Set multiple points in flash
      //
      char *bufptr = blockStart;
      pulData = (uint32_t*)bufptr;
      //
      //Set the uSec value to the system clock value,
      //
      //  FlashUsecSet(SysCtlClockGet()); // DEPRICATED IN THIS VERSION
      temp = 0;
      while(memAddress <= FLBLKEND)
      {
          FlashProgram(pulData, memAddress, sizeof(blockStart) /*bytesTostore*/ );
          memAddress += FLBLKSIZE;    
          temp++;
      }
      
      char *bufptr1 = blockEnd;
      pulData = (uint32_t*) bufptr1;
      FlashProgram(pulData, FLBLKEND - 12, 12 /*bytesTostore*/ );
      
      
      //
      //
      //
      return temp;
  }
  
  //
  // Write Flash
  //
  int cmd_persist::ProcessWriteFlash( int argc, char *argv[] )
  {
      uint32_t *pulData;
      int BitSel = 0;// memory position offset
      const uint32_t bytesTostore = 4;
      char *num;
      num = argv[argc - 2];
      //
      // Process first argument
      //
      BitSel = atoi(num);//
      if(BitSel > 0)
      {
          char *bufptr = argv[argc - 1];//buffer;
          pulData = (uint32_t*)bufptr;
          //
          //Set the uSec value to the system clock value,
          //
          //    FlashUsecSet(SysCtlClockGet()); // DEPRICATED IN THIS VERSION
          
          //
          // Program some data into the newly erased block of the flash.
          //
          //  pulData[0] = long("AB");sizeof(bufptr)
          //  pulData[1] = 0xA;
          return FlashProgram(pulData, CMDSTRT+(4 *(BitSel - 1)), bytesTostore );
      }
      return -1;
  }
  //
  // Read flash
  //
  int cmd_persist::ProcessReadFlash(int argc, char *argv[])
  {
      char *bufptr;
      char buffer[5];
      bufptr = (char*)CMDSTRT;
      for(int i = 0; i < 4; i++)
      {
          buffer[i] = bufptr[i]; 
      }
      buffer[4]='\0';
      uartPTR->putString(buffer,"\r\n");
      
      return 0;
  }
  //
  // Erase the flash
  //
  int cmd_persist::ProcessFlashErase()//int argc, char *argv[]
  {
      unsigned long memAddress = FLSTRT;
      long result = 0;
      
      do{
          //
          // Erase a block of the flashCMDSTRT
          //
          result = FlashErase(memAddress);
          if(result != 0)
              return result;
          memAddress += FLBLKSIZE;
      }while(memAddress < FLBLKEND);
      //
      // Initialize the flash parameter block
      // 
      FlashPBInit(FLSTRT, FLBLKEND, PBSIZE); 
      
      return 0;  
  }
  //
  // Test the use of a parameter block
  //
  int cmd_persist::ProcessParameterBlock(int argc, char *argv[])
  {
      char pucBuffer[4]; 
      uint8_t* pucPB;
      
      //*******************************************
      // Get current PB location
      //
      pucPB = FlashPBGet();
      //*******************************************
      // Load current parameter block in buffer
      //
      if(pucPB)
      {
          for(int i = 0; i < 4; i++)
          {
              pucBuffer[i] = pucPB[i]; 
          }
          uartPTR->putString(pucBuffer,"\r\n");
          return 0;
      }
      
      return -1;
  }
  
  //***************************************************
  // Save a Parameter Block
  //
  int cmd_persist::ProcessPBSave(int argc, char *argv[])
  {
      //*******************************************
      // Single dimensional Write Buffer
      //
      unsigned char writeBuffer[1024];
      //*******************************************
      // Pointer to confirm PB exists
      //
      unsigned char *ptrPB = FlashPBGet();
      if(!ptrPB)
          return -1;//No PB exists
      //*******************************************
      // Load Write Buffer from cmd/preSets
      //
      int count = 2;// Leave room for error checking bytes 
      int i,j;
      //*******************************************
      // Load from L1cmd
      //
      for(i = 0; i < 32; i++)
      {
          for(j = 0; j < 4; j++)
          {
              writeBuffer[count++] = L1cmd[i][j];
          }      
      }
      //*******************************************
      // Load from L2cmd
      //
      for(i = 0; i < 32; i++)
      {
          for(j = 0; j < 4; j++)
          {
              writeBuffer[count++] = L2cmd[i][j];
          }      
      }    
      //*******************************************
      // Load from preSet
      //
      for(i = 0; i < 32; i++)
      {
          for(j = 0; j < 8 && (count < PBSIZE); j++)
          {
              writeBuffer[count++] = preSet[i][j];
          } 
      }  
      //*******************************************
      // Load HM settings
      //
      count = HM_ADDRESS_OFFSET;
      for(i = 0; i < 17; i++)
      {
          writeBuffer[count++] = head_settings->hm_address[i];
      }
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->wh_speed[i];
      } 
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->wh_mid[i];
      }
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->tk_press[i];
      }  
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->tk_max[i];
      }
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->bk_press[i];
      } 
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->bk_max[i];
      }
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->wa_press[i];
      }  
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->wa_max[i];
      }
      //
      // OTHER_PRESS_OFFSET
      //
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->other_press[i];
      } 
      //
      //OTHERMAX_PRESS_OFFSET
      //
      for(i = 0; i < 2; i++)
      {
          writeBuffer[count++] = head_settings->other_max[i];
      }
      //*************************************************
      // Extra memory for:
      //  1. over length window
      //
      for(i = 0; i < 4; i++)
      {
          writeBuffer[count++] = head_settings->over_target[i];
      }
      //
      //2. under length window
      //
      for(i = 0; i < 4; i++)
      {
          writeBuffer[count++] = head_settings->under_target[i];
      }
      //*************************************************
      // Fill the remainder of the write buffer
      //
      for(i = count; i < PBSIZE; i++)
      {
          writeBuffer[i] = 0xFF;
      }
      //*******************************************
      // Save a buffer to the PB
      //
      FlashPBSave(writeBuffer);
      
      //*******************************************
      // Successful operation
      //
      return 0;   
  }
  //*****************************************************
  // Change cmd/preset arrays
  //    
  //
  int cmd_persist::ProcessChangeCP(int argc, char *argv[])
  {
      //*********************************************
      // Set Pressures
      //
      // Wheel speed
      //
      if(!strcmp(argv[1],"speed"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->wh_speed[i] = argv[2][i];
              head_settings->wh_speed[i+1] = '\0';
          } 
          return 0;
      }
      //
      // Middle/center
      //
      if(!strcmp(argv[1],"cent"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->wh_mid[i] = argv[2][i];
              head_settings->wh_mid[i+1] = '\0';
          } 
          return 0;
      }
      //
      // Top knife pressure
      //
      if(!strcmp(argv[1],"topk"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->tk_press[i] = argv[2][i];
              head_settings->tk_press[i+1] = '\0';
          }
          return 0;
      } 
      //
      // Top knife max pressure  
      //
      if(!strcmp(argv[1],"tkmax"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->tk_max[i] = argv[2][i];
              head_settings->tk_max[i+1] = '\0';
          } 
          return 0;
      }   
      //
      // Bottom knife pressure
      // bk_press
      if(!strcmp(argv[1],"botk"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->bk_press[i] = argv[2][i];
              head_settings->bk_press[i+1] = '\0';
          } 
          return 0;
      }   
      //
      // Bottom knife maximum pressure
      //bk_max
      if(!strcmp(argv[1],"bkmax"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->bk_max[i] = argv[2][i];
              head_settings->bk_max[i+1] = '\0';
          } 
          return 0;
      }    
      //
      // Wheel arm pressure
      //wa_press
      if(!strcmp(argv[1],"wharm"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->wa_press[i] = argv[2][i];
              head_settings->wa_press[i+1] = '\0';
          } 
          return 0;
      }   
      //
      // Maximum wheel arm pressure
      //wa_max
      if(!strcmp(argv[1],"wamax"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->wa_max[i] = argv[2][i];
              head_settings->wa_max[i+1] = '\0';
          } 
          return 0;
      }  
      //
      // Other pressure
      //other_press
      if(!strcmp(argv[1],"other"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->other_press[i] = argv[2][i];
              head_settings->other_press[i+1] = '\0';
          } 
          return 0;
      }  
      //
      // Maximum other pressure
      //other_max
      if(!strcmp(argv[1],"omax"))
      {
          for(int i = 0; i < 2; i++)
          {
              head_settings->other_max[i] = argv[2][i];
              head_settings->other_max[i+1] = '\0';
          } 
          return 0;
      }  
      //*********************************************
      // Set Over for lengths
      //
      if(!strcmp(argv[1],"over"))
      {
          //    for(int i = 0; i < 5; i++)
          //    {
          //      head_settings->over_target[i] = argv[2][i];
          //      head_settings->over_target[i+1] = '\0';
          //    }
          //
          // if loadArr returns 0 the array is successfully
          // loaded
          //
          if(0 == loadArr(argv[2],head_settings->over_target,4))
          {
              // 
              // set integer value of under length
              //
              minTarget = atoi(head_settings->over_target);
          }else
              return -1;
          //
          // set integer value of over target
          //
          maxTarget = atoi(head_settings->over_target);
          //
          //
          //
          return 0;
      }
      //*********************************************
      // Set under for lengths
      //
      if(!strcmp(argv[1],"under"))
      {
          //    for(int i = 0; i < 5; i++)
          //    {
          //      head_settings->under_target[i] = argv[2][i];
          //      head_settings->under_target[i+1] = '\0';
          //    } 
          //
          // if loadArr returns 0 the array is successfully
          // loaded
          //
          if(0 == loadArr(argv[2],head_settings->under_target,4))
          {
              // 
              // set integer value of under length
              //
              minTarget = atoi(head_settings->under_target);
          }else
              return -1;
          //
          //
          return 0;
      }
      //*********************************************
      // Set BT address of Head Module
      //
      if(!strcmp(argv[1], "hm"))
      {
          if(!strcmp(argv[2], "address"))
          {
              for(int i = 0; i < 17; i++)
              {
                  head_settings->hm_address[i] = argv[3][i];
                  head_settings->hm_address[i+1] = '\0';
              }  
              return 0;
          }
          return -1;// not enough argumets
      }
      //*************************************************
      // input command changes, related to input location
      // 
      int inputBit;
      bool makeshift = false;
      inputBit = atoi(argv[2]);
      //*******************************************
      // Ensure enough arguments for this operation
      //
      if(argc < 4)
          return -1;// not enough argumets  
      //*******************************************
      // ensure the selected bit is within range
      //
      if(inputBit > 0 && inputBit <= 32)
      {
          inputBit--;
      }else if(inputBit > 32 && inputBit <= 64)
      {
          //****************************
          // Must be a level 2 command
          //
          argv[1][1]='2'; 
          //****************************
          // L2 index is 0 to 31 as well
          //
          inputBit -= 33;
      }
      else
          return -1;// inputBit out of range
      
      //*******************************************
      // Change Level 1 input
      //
      if(!strcmp(argv[1], "L1"))
      {
          //****************************
          // Special case for shif command
          //
          if(!strcmp(argv[3],"shif"))
          {
              makeshift = true;
          }
          //*****************************
          // Load the L1 array
          //
          for(int i = 0; i < 4; i++)
          {
              L1cmd[inputBit][i] = argv[3][i];
              L1cmd[inputBit][i+1] = '\0';
              //*****************
              // if making shift command
              // then the level two index
              // is also made to shift
              if(makeshift)
              {
                  L2cmd[inputBit][i] = argv[3][i];
                  L2cmd[inputBit][i+1] = '\0';       
              }
          }
          //*******************************************
          // return successfully
          //
          return 0;
      }
      //*******************************************
      // Change Level 2 input
      //
      if(!strcmp(argv[1], "L2"))
      {
          for(int i = 0; i < 4; i++)
          {
              L2cmd[inputBit][i] = argv[3][i];
              L2cmd[inputBit][i+1] = '\0';
          }   
          //*******************************************
          // return successfully
          //
          return 0;
      }
      
      //*******************************************
      // Change a pre set input
      //
      if(!strcmp(argv[1], "ps"))
      {
          //*******************************************
          // Only 31 presets are available
          //
          if(inputBit > 30)
              return -1; // inputBit out of range
          //
          //
          //
          return loadArr(argv[3],preSet[inputBit],8);
      }
      //
      //
      //
      return -1;// no valid selection  
  }
  //********************************************************
  // Load a large array
  //
  int cmd_persist::loadArr(char * seedArr,char * targetArr, 
                           int sizeoftarget)
  {
      int j = 0;
      int i = 0;
      //
      // count characters in argv[3]
      //
      while(seedArr[j++] != 0 && j <=(sizeoftarget+1))
      {
          ; 
      }
      j--; // result included null
      //*******************************************
      // Max 8 characters
      //
      if(j>sizeoftarget)
          return -1;
      //*******************************************
      // Load preset array LSB to HSB
      //    
      for(i = sizeoftarget; i >= 0 && j >= 0; i--,j--)
      {
          targetArr[i] = seedArr[j];      
      }  
      //*******************************************
      // fill higher order with 0's if necessary
      //
      if( i >= 0 )
      {
          for(; i >= 0; i--)
          {
              targetArr[i] = '0'; 
          }
      }  
      return 0;
  }
  //*****************************************************
  // Reset the current parameter block to default values
  //
  int cmd_persist::ProcessPBReset(int argc, char *argv[])
  {
      unsigned char* ptrPB;
      FirstTimePBsetup("none","90000001","00:07:80:42:C7:CC","9291"); 
      ptrPB = FlashPBGet();
      if(ptrPB)
          return 0;
      return -1;
  }
  // **************************************************
  // Get the current command set
  //
  int cmd_persist::ProcessGetCP(int argc, char *argv[])
  { 
      bool isAll = false;
      //*******************************************
      // Is it a get all request
      //
      if(!strcmp(argv[1],"all"))
          isAll = true;
      //*******************************************
      // Is it a get hm settings request
      //
      if(isAll || !strcmp(argv[1], "hm"))
      { 
          uartPTR->putString("HM Settings: "); 
          
          uartPTR->putString("Speed ", " "); 
          uartPTR->putString(head_settings->wh_speed);//,"\r\n"     
          uartPTR->putString("Valve Center ", " "); 
          uartPTR->putString(head_settings->wh_mid);
          uartPTR->putString("Top Knife Pressure ", " "); 
          uartPTR->putString(head_settings->tk_press);
          uartPTR->putString("Butt Knife Pressure ", " "); 
          uartPTR->putString(head_settings->bk_press);
          uartPTR->putString("Wheel Arm Pressure ", " "); 
          uartPTR->putString(head_settings->wa_press);
          
          if(!isAll)
              return 0;
      }
      
      //*******************************************
      // Is it a get L1 request
      //
      if(isAll || !strcmp(argv[1], "L1"))
      {     
          for(int i = 0; i < 32; i++)
          {
              uartPTR->putString("L1"," "); 
              uartPTR->putString(i + 1, " "); 
              uartPTR->putString(L1cmd[i]);//,"\r\n"
          }
          if(!isAll)
              return 0;
      }
      //*******************************************
      // Is it a get L2 request
      //
      if(isAll || !strcmp(argv[1],"L2"))
      {
          for(int i = 0; i < 32; i++)
          {
              uartPTR->putString("L2"," "); 
              uartPTR->putString(i + 1, " "); 
              uartPTR->putString(L2cmd[i]);//,"\r\n"
          }  
          if(!isAll)
              return 0;
      }
      //*******************************************
      // Is it a get preSet request
      //
      if(isAll || !strcmp(argv[1],"ps"))
      {
          if(argc == 3)
          {
              int x = 0;
              x = atoi(argv[2]);
              if(x > 0 && x < 32)
              {
                  uartPTR->putString("Pre-set"," "); 
                  uartPTR->putString(argv[2], " "); 
                  uartPTR->putString(preSet[x-1]);//,"\r\n"// array addressing is from 0 to x  
              }else{ return -1; }// invalid position
          }else{
              for(int i = 0; i < 31; i++)
              {
                  uartPTR->putString("Pre-Set"," "); 
                  uartPTR->putString(i + 1, " "); 
                  uartPTR->putString(preSet[i]);//,"\r\n"
              }   
          }
          //*************************************************
          // Is it a get over length target request?
          //
          //**************************************************
          // Is it a get under length target request?
          //
          //**************************************************
          //
          return 0;
      }
      //*******************************************
      // return un successfully
      //
      return -1;
  }
  //*******************************************************************
  // Confirm the command is valid OR not 'NONE'
  //
  
  //*********************************************************************
  // Send raw command functions
  //
  bool cmd_persist::notNONE(int input, inputLevel il)
  {
      switch(il)
      {
        case LEVEL_1:
          if(strcmp(L1cmd[input],"none"))// recall a successful compare returns 0
              return true;//
          break;
        case LEVEL_2:
          if(strcmp(L2cmd[input],"none"))
              return true;// 
          break;
        case PRESET:
        default:
          break;
      }
      return false;
  }
  //*****************************************************************
  // Send Raw , with end tag; start at L1 and if there is a special case do the 
  // appropriate action.
  //
  void cmd_persist::sendRaw(int input, inputLevel il, int uart)
  {
      switch(il)
      {
        case LEVEL_1:
          uartPTR->putString(L1cmd[input],",#",uart);//
          break;
        case LEVEL_2:
          uartPTR->putString(L2cmd[input],",",uart);// 
          break;
        case PRESET:
          uartPTR->putString(preSet[input],",",uart);  
          break;
        default:
          break;
      }
      
  }
  //***************************************************************
  // Send Raw L1 command, with end tag
  //
  void cmd_persist::sendRawL1(int input, int uart)
  {
      if(input > 31 || input < 0)
          return;
      uartPTR->putString(L1cmd[input],",",uart);//   
  }
  //****************************************************************
  // Send Raw L2 command with end tag
  //
  void cmd_persist::sendRawL2(int input, int uart)
  {
      if(input > 31 || input < 0)
          return;
      uartPTR->putString(L1cmd[input],",",uart);//  
  }
  //*******************************************
  // Send Raw pre set from index
  //
  void cmd_persist::sendRawPreSet(int index, int uart)
  {
      if(index > 30 || index < 0)
          return;
      uartPTR->putString(preSet[index],",",uart);// 
  }
  //*******************************************
  // Send Raw minimum target
  //
  void cmd_persist::sendRawMinTarget(int index, int uart)
  {
      //
      // If index is out of bounds return
      //
      if(index > 30 || index < 0)
          return;
      //
      // get the integer value of the selected
      //  target pulses
      //
      int tempTarget = atoi(preSet[index]); 
      
      // 
      // Subtract the minimum targetted value
      //  from the target
      //
      tempTarget -= minTarget;
      // 
      // Convert back to array
      //
      char arrTarget[] = "00000000";
      ItoA(tempTarget,arrTarget);  
      //
      // Send the resulting array
      //
      uartPTR->putString(arrTarget,",",uart);// 
      
  }
  //*******************************************
  // Send Raw maximum target
  //
  void cmd_persist::sendRawMaxTarget(int index, int uart)
  {
      //
      // If index is out of bounds return
      //
      if(index > 30 || index < 0)
          return;
      //
      // get the integer value of the selected
      //  target pulses
      //
      int tempTarget = atoi(preSet[index]); 
      
      // 
      // Subtract the minimum targetted value
      //  from the target
      //
      tempTarget += maxTarget;
      // 
      // Convert back to array
      //
      char arrTarget[] = "00000000";
      ItoA(tempTarget,arrTarget);  
      //
      // Send the resulting array
      //
      uartPTR->putString(arrTarget,",",uart);//  
      uartPTR->putString("",uart);
  }
  //********************************************
  // Send Raw Speed value
  //
  void cmd_persist::sendRawSpeed(int uart)
  {
      uartPTR->putString(head_settings->wh_speed,uart);//,"\r\n"     
      
  }
  //********************************************
  // Send Raw Middle (center) speed valve value
  //
  void cmd_persist::sendRawMid(int uart)
  {
      uartPTR->putString(head_settings->wh_mid,uart); 
  }
  //********************************************
  // Send Raw Top Knife Pressure value
  //
  void cmd_persist::sendRawTKP(int uart) //
  {
      uartPTR->putString(head_settings->tk_press,uart);
  }
  //********************************************
  // Send Raw Butt Knife Pressue value
  //
  void cmd_persist::sendRawBKP(int uart) //
  {
      uartPTR->putString(head_settings->bk_press,uart);
  }
  //********************************************
  // Send Raw Wheel Arm Pressure value
  void cmd_persist::sendRawWAP(int uart) //
  {
      uartPTR->putString(head_settings->wa_press,uart);
  }
  //*******************************************
  // Get pre set index
  //
  int cmd_persist::getPreSetIndex(inputLevel levelSelect, int input)
  {
      char tempI[] = "00";
      switch(levelSelect)
      {
        case LEVEL_1:
          // confirm the selected input is a pre set
          if(!strncmp(L1cmd[input],"ps",2))
          {
              tempI[0] = L1cmd[input][2];
              tempI[1] = L1cmd[input][3];
          }else
              return -1;
          break;
        case LEVEL_2:
          // confirm the selected input is a pre set
          if(!strncmp(L2cmd[input],"ps",2))
          {
              tempI[0] = L2cmd[input][2];
              tempI[1] = L2cmd[input][3];
          }else
              return -1;   
          break;
        default:
          break;
      }
      return atoi(tempI-1);// pre-sets are entered 1 - 32; indexes are 0 - 31
  }
  
  //*****************************************************************
  // Peek functions
  //
  //**************************************
  // Peek at input index to see if it is 'shift'
  //
  bool cmd_persist::peekShift(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strcmp(L1cmd[input],_SHIFT))
              return true;
          break;
        case LEVEL_2:// Should never check here but...
          if(!strcmp(L2cmd[input],_SHIFT))
              return true;
          break;
        case PRESET:
          break;
        default:
          break;
      }
      return false;
  }
  //*******************************************************
  // Peek at input index to see if it is a Top saw command
  //
  bool cmd_persist::peekTopSaw(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strcmp(L1cmd[input],_TSAW))
              return true;
          break;
        case LEVEL_2:
          if(!strcmp(L2cmd[input],_TSAW))
              return true;   
        case PRESET:
          break;
        default:
          break;
      }
      return false;
  }
  //*******************************************************
  // Peek at input index to see if it is a Main saw command
  //
  bool cmd_persist::peekMainSaw(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strcmp(L1cmd[input],_MSAW))
              return true;
          break;
        case LEVEL_2:
          if(!strcmp(L2cmd[input],_MSAW))
              return true;   
        case PRESET:
          break;
        default:
          break;
      }
      return false; 
  }
  //*******************************************************
  // Peek at input index to see if it is a lock function
  //
  bool cmd_persist::peekLock(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strcmp(L1cmd[input],_LOCK))
              return true;
          break;
        case LEVEL_2:
          if(!strcmp(L2cmd[input],_LOCK))
              return true;   
        case PRESET:
          break;
        default:
          break;
      }
      return false;   
  }
  //*******************************************************
  // Peek at input index to see if it is a pump activation
  // related command.
  //
  bool cmd_persist::peekPumpCMD(int input, inputLevel lvl)
  {
      if(peekShift(input,lvl) || peekPreSet(input,lvl))
          return false;
      return true;
  }
  //********************************************************
  // Peek at input index to see if it is a pre-set related
  // input
  //
  bool cmd_persist::peekPreSet(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strncmp(L1cmd[input],"ps",2))
              return true;
          break;
        case LEVEL_2:
          if(!strncmp(L2cmd[input],"ps",2))
              return true;
          break;
        case PRESET:
          break;
        default:
          break;
      }
      return false; 
  }
  //************************************************************************
  // Peek at input index to see if it is a Neutral feed command
  //
  bool cmd_persist::peekNeut(int input, inputLevel lvl)
  {
      switch(lvl)
      {
        case LEVEL_1: 
          if(!strncmp(L1cmd[input],_NEUTRAL,2))
              return true;
          break;
        case LEVEL_2:
          if(!strncmp(L2cmd[input],_NEUTRAL,2))
              return true;
          break;
        case PRESET:
          break;
        default:
          break;
      }
      return false;  
  }
  //************************************************************************
  //
  // Private Functions
  //
  //************************************************************************
  //
  // Load a parameter block's data into appropriate array's
  //
  bool cmd_persist::ProcessPBInit(void)
  {
      unsigned char* ptrPB;
      
      //*****************************************
      // Initialize the flash parameter block
      // 
      //
      FlashPBInit(FLSTRT, FLBLKEND, PBSIZE); 
      //*****************************************
      // Get the current Parameter Block Location
      //
      ptrPB = FlashPBGet();
      //
      // Confirm Parameter Block exists
      //
      if(ptrPB)
      {  
          //
          // 
          //
          int count = 2;// PB uses the first 2 bytes for error checking
          int i,j;
          //
          // Load L1cmd
          //
          for(i = 0; i < 32; i++)
          {
              for(j = 0; j < 4; j++)
              {
                  L1cmd[i][j] = ptrPB[count++];
                  L1cmd[i][j+1] = '\0';
              }      
          }
          //
          // Load L2cmd
          //
          for(i = 0; i < 32 ; i++)
          {
              for(j = 0; j < 4; j++)
              {
                  L2cmd[i][j] = ptrPB[count++];
                  L2cmd[i][j+1] = '\0';
              }      
          }    
          //
          // Load preSet
          //
          for(i = 0; (i < 31) && (count < PBSIZE) ; i++)
          {
              for(j = 0; j < 8; j++)
              {
                  preSet[i][j] = ptrPB[count++];
                  preSet[i][j+1] = '\0';
              } 
          }
          //****************************************
          // Load head module settings
          //
          // Address
          //
          count = HM_ADDRESS_OFFSET;
          for(int i = 0; i < 17 && (count < PBSIZE); i++)
          {
              head_settings->hm_address[i] = ptrPB[count++];
              head_settings->hm_address[i+1] = '\0';
          }  
          //**************
          // Wheel speed
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->wh_speed[i] = ptrPB[count++];
              head_settings->wh_speed[i+1] = '\0';
          }    
          //*************
          // Wheel speed middle
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->wh_mid[i] = ptrPB[count++];
              head_settings->wh_mid[i+1] = '\0';
          }  
          //*******************
          // Top knife pressure
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->tk_press[i] = ptrPB[count++];
              head_settings->tk_press[i+1] = '\0';
          } 
          //***********************
          // Top knife max
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->tk_max[i] = ptrPB[count++];
              head_settings->tk_max[i+1] = '\0';
          }   
          //*********************
          // Bottom knife pressure
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->bk_press[i] = ptrPB[count++];
              head_settings->bk_press[i+1] = '\0';
          }     
          //**************************
          // Bottom knife max
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->bk_max[i] = ptrPB[count++];
              head_settings->bk_max[i+1] = '\0';
          }    
          //**************************
          // Wheel arm pressure
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->wa_press[i] = ptrPB[count++];
              head_settings->wa_press[i+1] = '\0';
          }    
          //**************************
          // Wheal arm max
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->wa_max[i] = ptrPB[count++];
              head_settings->wa_max[i+1] = '\0';
          }  
          //**************************
          // Other port pressure
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->other_press[i] = ptrPB[count++];
              head_settings->other_press[i+1] = '\0';
          }    
          //***********************
          // Other port max
          //
          for(int i = 0; i < 2 && (count < PBSIZE); i++)
          {
              head_settings->other_max[i] = ptrPB[count++];
              head_settings->other_max[i+1] = '\0';
          }     
          //**************************************
          // Over target length window
          //
          for(int i = 0; i < 4 && (count < PBSIZE); i++)
          {
              head_settings->over_target[i] = ptrPB[count++];
              head_settings->over_target[i+1] = '\0';     
          }
          //**************************************
          // Under target length window
          //
          for(int i = 0; i < 4 && (count < PBSIZE); i++)
          {
              head_settings->under_target[i] = ptrPB[count++];
              head_settings->under_target[i+1] = '\0';     
          }    
          //**************************************
          // Success
          //
          return true;
      }
      //
      // Failed; no parameter block
      //
      return false;
  }
  //***********************************************************
  // Initialize a new or current Parameter Block
  // to default values
  //
  bool cmd_persist::FirstTimePBsetup(char cmdInit[], char presInit[], 
                                     char hm_addr[],char hm_settings[])
  {
      
      unsigned char* ptrPB; 
      //      char  = ;
      //      char  = ;
      //      char  = ;
      //      char  = ;
      unsigned char tempArr[1024];
      int count = 2;
      //********************************************
      // Initialize the flash parameter block
      // 
      FlashPBInit(FLSTRT,FLBLKEND, PBSIZE); 
      
      for(; count < (PBSIZE/4);)
      {
          //************************
          // Initialize 4 byte area
          //
          for(int i = 0; i < 4; i++)
          {
              tempArr[count++] = cmdInit[i];
          } 
      }
      for(; count < PBSIZE/2;)
      {
          //********************************************
          // Initialize the 8 byte area
          //  
          for(int i = 0; i < 8 && count < PBSIZE/2; i++)
          {
              tempArr[count++] = presInit[i];
          }
      }
      //****************************************
      // Default HM address
      //
      for(int i = 0; i < 17; i++)
      {
          tempArr[count++] = hm_addr[i]; 
      }
      //***************************************
      // Default values for pressures etc.
      //
      for(int k = 0; k < 10; k++)
      {
          for(int i = 0; i < 4; i++)
          {
              tempArr[count++] = hm_settings[i]; 
          }
      }
      //****************************************
      // Fill the rest with blanks
      //
      while(count < 1024)
      {
          tempArr[count++] = 0xFF;
      }
      tempArr[count] = '\0';
      //
      // Save to flash
      //
      FlashPBSave(tempArr);    
      //********************************************
      // Get the current Parameter Block Location
      //
      ptrPB = FlashPBGet();
      if(ptrPB)
      {   
          return true; 
      }
      return false;
  }
  //*******************************************************
  // Function to send an array of a specified size
  //
  void cmd_persist::sendArray(char *arrToSend, int sizeofArr)
  {
      
  }
  //********************************************************
  // Function to load a char array param of p_size from location
  // p_offset
  // 
  // The following is a code example to use getParameter(...)
  //  char addr[17] = "n/a";
  //  PB_OFFSETS os = HM_ADDRESS_OFFSET;
  //  getParameter(os,addr,17);
  //  uartPTR->putString("Default HM address: ",addr,CMDLINEPORT);
  //
  void cmd_persist::getParameter(PB_OFFSETS p_offset, 
                                 char *param, int p_size)
  {
      switch(p_offset)
      {
        case L1_ADDRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if( (L2_ADDRESS_OFFSET - L1_ADDRESS_OFFSET) < p_size)
          {return;}
          break;
        case L2_ADDRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((PRESET_ADDRESS_OFFSET - L2_ADDRESS_OFFSET) < p_size)
          { return; }
          
          break;
        case PRESET_ADDRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((HM_ADDRESS_OFFSET - PRESET_ADDRESS_OFFSET) < p_size)
          { return; }
          //*****************************
          // fill param array
          //
          fillArray(param,p_offset,p_size);   
          break;
        case HM_ADDRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((WHEEL_SPEED_OFFSET - HM_ADDRESS_OFFSET) < p_size)
          { return; }
          
          break;
        case WHEEL_SPEED_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((MIDDLE_SPEED_OFFSET - WHEEL_SPEED_OFFSET) < p_size)
          { return; }
          
          break;
        case MIDDLE_SPEED_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((TOPK_PRESS_OFFSET - MIDDLE_SPEED_OFFSET) < p_size)
          { return; }
          break;
        case TOPK_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((TOPKMAX_PRESS_OFFSET - TOPK_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case TOPKMAX_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((BOTK_PRESS_OFFSET - TOPKMAX_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case BOTK_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((BOTKMAX_PRESS_OFFEST - BOTK_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case BOTKMAX_PRESS_OFFEST:
          //****************************
          // ensure size is within range
          //
          if((WHEELARM_PRESS_OFFSET - BOTKMAX_PRESS_OFFEST) < p_size)
          { return; }
          break;
        case WHEELARM_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((WHEELARMMAX_PRESS_OFFSET - WHEELARM_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case WHEELARMMAX_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((OTHER_PRESS_OFFSET - WHEELARMMAX_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case OTHER_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if((OTHERMAX_PRESS_OFFSET - OTHER_PRESS_OFFSET) < p_size)
          { return; }
          break;
        case OTHERMAX_PRESS_OFFSET:
          //****************************
          // ensure size is within range
          //
          if(p_size > 2)
          {return;}
          break;
        default:
          return;// Nothing matches
      }
      //******************************************
      // Function variables are valid
      // Fill array from selected location
      //
      fillArray(param,p_offset,p_size);
  }
  //**********************************************************
  // Private function: fill an array of size from an offset
  //
  void cmd_persist::fillArray(char *arrToFill, 
                              PB_OFFSETS arr_offset, int p_size)
  {
      volatile unsigned char *ptrPB = FlashPBGet();
      if(!ptrPB)
      {return;}//no PB available
      ptrPB += (unsigned long)arr_offset;
      for(int i = 0; i < p_size; i++)
      {
          arrToFill[i] = ptrPB[i];
          arrToFill[i+1] = '\0';
      }
      
  }