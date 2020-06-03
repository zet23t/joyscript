using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace JoyScript
{
    public enum DataType : byte
    {
        Nil,
        Bool,
        Int,
        Float,
        Object,
        Table,
        String,
        Function,
        NativeFunction,
        Reference,
        Label,
        Address,
        AddressRef,
        OpCode,
    }

    public class VM
    {
        private Value globalTable = new Value(DataType.Table);
        private FrameStack executionStack = new FrameStack();
        private List<Value> instructions;
        private int ip;

        public void SetGlobal(string key, Value value)
        {
            globalTable.SetValue(key, value);
        }

        public void Load(List<Value> instructions)
        {
            Dictionary<string, int> addresses = new Dictionary<string, int>();
            for (int i = 0; i < instructions.Count; i += 1)
            {
                if (instructions[i].DataType == DataType.Address)
                {
                    addresses[instructions[i].vString] = i + 1;
                }
            }
            for (int i = 0; i < instructions.Count; i += 1)
            {
                if (instructions[i].DataType == DataType.AddressRef)
                {
                    instructions[i] = addresses[instructions[i].vString];
                }
            }
            // for (int i = 0; i < instructions.Count; i += 1)
            // {
            //     Debug.Log(i + ": " + instructions[i].ToString());
            // }
            this.instructions = instructions;

        }

        public Frame GetCurrentFrame()
        {
            return executionStack.GetCurrentFrame();
        }

        public Value GetTop(int fromBack = 0)
        {
            return executionStack.PeekValue(fromBack);
        }

        public void Execute()
        {
            try
            {
                ExecuteInternal();
            }
            catch (ExecutionError e)
            {
                throw new ExecutionError("Error @" + ip + ": " + e.Message);
            }
            catch (ValueAccessError e)
            {
                throw new ExecutionError("Error @" + ip + ": " + e.Message);
            }
        }
        private void ExecuteInternal()
        {
            ip = 0;
            int execCap = 1000;
            while (ip < instructions.Count)
            {
                if (execCap-- < 0)
                {
                    throw new ExecutionError("endless loop");
                }
                Value instruction = GetNext();
                switch ((OpCode) instruction.vInt)
                {
                    case OpCode.NOP:
                    case OpCode.Label:
                        continue;
                    case OpCode.DuplicateTop:
                        executionStack.PushValue(executionStack.PeekValue());
                        break;
                    case OpCode.Pop:
                        executionStack.PopValue();
                        break;
                    case OpCode.PushValue:
                        executionStack.PushValue(executionStack.GetCurrentFrame().GetRegister(GetNext().vInt));
                        break;
                    case OpCode.PushValueLiteral:
                        executionStack.PushValue(GetNext());
                        break;
                    case OpCode.Neg:
                        executionStack.PushValue(-executionStack.PopValue());
                        break;
                    case OpCode.Div:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a / b);
                        }
                        break;
                    case OpCode.Mod:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a % b);
                        }
                        break;
                    case OpCode.Mul:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a * b);
                        }
                        break;
                    case OpCode.Add:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a + b);
                        }
                        break;
                    case OpCode.Sub:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a - b);
                        }
                        break;
                    case OpCode.Inc:
                        executionStack.PushValue(executionStack.PopValue() + 1);
                        break;
                    case OpCode.Dec:
                        executionStack.PushValue(executionStack.PopValue() - 1);
                        break;
                    case OpCode.Equal:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a == b);
                        }
                        break;
                    case OpCode.LowerThan:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a < b);
                        }
                        break;
                    case OpCode.LowerEqualThan:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a <= b);
                        }
                        break;
                    case OpCode.GreaterThan:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a > b);
                        }
                        break;
                    case OpCode.GreaterEqualThan:
                        {
                            Value a = executionStack.PopValue();
                            Value b = executionStack.PopValue();
                            executionStack.PushValue(a >= b);
                        }
                        break;
                    case OpCode.Call:
                        {
                            // Debug.Log("call");
                            int argCount = executionStack.PopValue().vInt;
                            int targetIP = executionStack.PopValue().vInt;
                            executionStack.PushCall(argCount, ip + 1);
                            ip = targetIP;
                        }
                        break;
                    case OpCode.Return:
                        {
                            // Debug.Log("return");
                            int retCount = executionStack.PopValue().vInt;
                            ip = executionStack.PopCall(retCount);
                        }
                        break;
                    case OpCode.Jump:
                        ip = GetNext().vInt;
                        break;
                    case OpCode.JumpIf:
                        if (executionStack.PopValue())
                        {
                            ip = GetNext().vInt;
                        }
                        break;

                    case OpCode.LoadTableKeyLiteral:
                        {
                            Value key = GetNext();
                            executionStack.PushValue(executionStack.PeekValue().GetValue(key));
                        }
                        break;

                    case OpCode.StoreTableKVLiteral:
                        {
                            Value key = GetNext();
                            Value value = GetNext();
                            executionStack.PeekValue().SetValue(key, value);
                        }
                        break;

                    case OpCode.LoadTableKey:
                        {
                            Value key = executionStack.PopValue();
                            Value obj = executionStack.PeekValue();
                            executionStack.PushValue(obj.GetValue(key));
                        }
                        break;
                    case OpCode.StoreTableKeyLiteral:
                        {
                            Value val = executionStack.PopValue();
                            Value key = GetNext();
                            Value obj = executionStack.PeekValue();
                            obj.SetValue(key, val);
                        }
                        break;
                    case OpCode.StoreTableKey:
                        {
                            Value val = executionStack.PopValue();
                            Value key = executionStack.PopValue();
                            Value obj = executionStack.PeekValue();
                            obj.SetValue(key, val);
                        }
                        break;
                }
            }
        }

        private Value GetNext()
        {
            // Debug.Log(ip);
            return instructions[ip++];
        }
    }
}