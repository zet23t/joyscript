using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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
                    instructions[i] = Value.AddressRef(instructions[i].vString, addresses[instructions[i].vString]);
                }
            }
            // PrintInstructions(instructions);
            this.instructions = instructions;

        }

        public static void PrintInstructions(List<Value> instructions)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Program [" + instructions.Count + "]:");
            for (int i = 0; i < instructions.Count; i += 1)
            {
                sb.AppendLine("  " + i + ": " + instructions[i].ToString());
            }
            Debug.Log(sb);
        }

        public Frame GetCurrentFrame()
        {
            return executionStack.GetCurrentFrame();
        }

        public Value Pop(int fromBack = 1)
        {
            return executionStack.PopValue(fromBack);
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
                throw new ExecutionError("Error @" + ip + ": " + e.Message+" "+e.StackTrace);
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
                    case OpCode.CallMethod:
                        {
                            int argCount = executionStack.PopValue().vInt;
                            Value obj = executionStack.PopValue();
                            string method = executionStack.PopValue().vString;
                            obj.Call(this, method, argCount);
                        }
                        break;
                    case OpCode.Call:
                        {
                            int argCount = executionStack.PopValue().vInt;
                            Value callee = executionStack.PopValue();
                            if (callee.DataType == DataType.AddressRef)
                            {
                                int targetIP = callee.vInt;
                                executionStack.PushCall(argCount, ip + 1);
                                ip = targetIP;
                            }
                            else
                            {
                                callee.Call(this, argCount);
                            }
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
                    case OpCode.LoadGlobal:
                        {
                            Value key = executionStack.PopValue();
                            executionStack.PushValue(globalTable.GetValue(key));
                        }
                        break;
                    case OpCode.LoadGlobalKeyLiteral:
                        executionStack.PushValue(globalTable.GetValue(GetNext()));
                        break;
                    case OpCode.StoreGlobal:
                        globalTable.SetValue(executionStack.PopValue(), executionStack.PopValue());
                        break;
                    case OpCode.StoreGlobalKeyLiteral:
                        globalTable.SetValue(GetNext(), executionStack.PopValue());
                        break;
                    case OpCode.StoreGlobalKVLiteral:
                        globalTable.SetValue(GetNext(), GetNext());
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