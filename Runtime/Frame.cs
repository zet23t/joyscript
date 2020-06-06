using System;
using System.Collections.Generic;

namespace JoyScript
{
    public class Frame
    {
        private readonly Frame parent;
        private readonly int depth;

        private List<Value> registerValues = new List<Value>();

        public int Count => registerValues.Count;

        public Frame(Frame parent)
        {
            this.parent = parent;
            this.depth = parent == null ? 0 : parent.depth + 1;
        }

        public Value GetRegister(int id)
        {
            if (id < registerValues.Count)
            {
                return registerValues[id];
            }
            return Value.Nil;
        }

        public void PushValues(Frame frame, int args)
        {
            for (int i = frame.registerValues.Count - args; i < frame.registerValues.Count; i += 1)
            {
                registerValues.Add(frame.registerValues[i]);
            }
        }

        public void SetRegister(int id, Value value)
        {
            while (id >= registerValues.Count)
            {
                registerValues.Add(Value.Nil);
            }

            registerValues[id] = value;
        }

        public Value Pop()
        {
            if (registerValues.Count == 0)
            {
                return Value.Nil;
            }

            Value v = registerValues[registerValues.Count - 1];
            registerValues.RemoveAt(registerValues.Count - 1);
            return v;
        }

        public void Push(Value value) => registerValues.Add(value);
        public Value Peek(int fromBack = 0) => registerValues.Last(fromBack, Value.Nil);

    }
}