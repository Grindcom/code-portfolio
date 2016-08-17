///////////////////////////////////////////////////////////
//  queue.h
//  Implementation of the Class queue
//  Created on:      5/14/2014
//  Original author: Greg Ford, B.Sc.
///////////////////////////////////////////////////////////
#ifndef QUEUE_H
#define QUEUE_H

class Queue
{
  public:
    Queue();
    virtual ~Queue();
    /**********************************************************
    * 
    * FIFO Buffer Public FUNCTIONS
    *
    ***********************************************************/
    //
    // Check the uart buffer to see if it is empty
    //
    bool bufferEmpty(); 
    //
    // Check the uart buffer to see if it is full
    //   
    bool bufferFull();
    //
    // Add a character to the back, the Queue will wrap 
    //  when buffer size is reached
    //  If the buffer is full nothing is added
    //
    void enqueue(char c);
    //
    // Take the front character off the queue
    //
    char dequeue();
    //
    // Look at next character without removing from queue
    //
    char peekFront();
    //
    // Look at the last character on the queue
    //
    char peekBack();
    //
    // Look through queue for a specified character
    //  return true if found before the end of the buffer
    //  false if not
    bool peekFor(char c);
    //
    // Reset the front, back and size markers to
    //  the beginning of the array.
    //
    void makeEmpty();
  private:
    /**********************************************************
    * 
    * FIFO Buffer Private Variables and FUNCTIONS
    *
    ***********************************************************/
    
    int const UBUFFERSIZE;// 1024
    int currentSize;
    int front;
    int back;
    char Buffer[1024];
    //
    // Increment front or back and loop to 0 if buffer size
    //  is reached.
    //
    void increment(int &x);
    //
    // Recursive helper for peekFor(...)
    //
    bool peekForHlp(char c, int &start);
    
};
#endif