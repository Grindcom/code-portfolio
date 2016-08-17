///////////////////////////////////////////////////////////
//  queue.h
//  Implementation of the Class queue
//  Created on:      5/14/2014
//  Original author: Greg Ford, B.Sc.
///////////////////////////////////////////////////////////
#include "Queue.h"

Queue::Queue()
:UBUFFERSIZE(1024)
{
    makeEmpty();   
}
Queue::~Queue()
{
    
}
//
// Check the uart buffer to see if it is empty
//
bool Queue::bufferEmpty()
{
    if(currentSize > 0)
        return false;
    return true;
}
//
// Check the uart buffer to see if it is full
//   
bool Queue::bufferFull()
{
    if(currentSize < UBUFFERSIZE)
        return false;   
    return true;
}
//
// Add a character to the back of the UART3 buffer;
//  Queue will wrap when buffer size is reached
//  If the buffer is full nothing is added
//
void Queue::enqueue(char c)
{
    if(currentSize >= UBUFFERSIZE)
        return;
    //
    // Don't allow multiple null's
    //
    if( c == '\0' && Buffer[back]=='\0' )
        return;
        
    increment(back);
    Buffer[back] = c;
    currentSize++;
}
//
// Take the front character off the queue
//
char Queue::dequeue()
{
    if(bufferEmpty())
        return -1;
    currentSize--;
    char frontC = Buffer[front];
    increment(front);
    return frontC;
}
//
// Look at next character without removing from queue
//
char Queue::peekFront()
{
    if(bufferEmpty())
        return -1;
    return Buffer[front];
}
//
// Look at the last character on the queue
//
char Queue::peekBack()
{
    if(bufferEmpty())
        return -1;
    return Buffer[back];
}
//
// Look through queue for a specified character
//  return true if found before the end of the buffer
//  false if not
bool Queue::peekFor(char c)
{
    int index = front;
    return peekForHlp(c,index);
}
//
// Recursive helper for peekFor(...)
//
bool Queue::peekForHlp(char c, int &start)
{
    if( ++start == UBUFFERSIZE)
        start = 0;    
    if( Buffer[start] == c)
        return true;    
    if(start == back)
        return false;    

    return peekForHlp(c,start);
}
//
// Reset the front, back and size markers to
//  the beginning of the array.
//
void Queue::makeEmpty()
{
    currentSize = 0;
    front= 0;
    back = -1;
}

//
// Increment front or back and loop to 0 if buffer size
//  is reached.
//
void Queue::increment(int &x)
{
    if( ++x == UBUFFERSIZE)
        x = 0;
}