using System;
using System.Collections.Generic;

namespace JoyScript
{
    public class FrameStack
    {
        private List<Frame> stack = new List<Frame>();
    
        public FrameStack()
        {
            stack.Add(new Frame(null));
        }

        public void PushCall(int args, int srcPos)
        {
            Frame current = stack.Last();
            Frame frame = new Frame(current);
            frame.PushValues(current, args);
            stack.Add(frame);
            current.Push(srcPos);
        }

        public int PopCall(int retCount)
        {
            Frame popped = stack.Pop();
            int retIp = stack.Last().Pop().vInt;
            stack.Last().PushValues(popped, retCount);
            return retIp;
        }

        public void PushValue(Value value)
        {
            stack.Last().Push(value);
        }

        public Value PopValue(int amount = 1)
        {
            if (stack.Count == 0) throw new ExecutionError("Stack underflow");
            if (amount-- > 1)
            {
                stack.RemoveRange(stack.Count - amount, amount);
            }
            return stack.Last().Pop();
        }

        public Value PeekValue(int fromBack = 0)
        {
            if (stack.Count <= 0)
            {
                throw new ExecutionError("Stack underflow");
            }
            return stack.Last().Peek(fromBack);
        }

        public Frame GetCurrentFrame()
        {
            return stack.Last();
        }
    }
}