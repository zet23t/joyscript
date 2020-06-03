using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JoyScript
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Value
    {
        [FieldOffset(0)]
        public readonly DataType DataType;
        [FieldOffset(2)]
        public readonly short DataTypeInfo;

        // overlapping value objects
        [FieldOffset(4)]
        public readonly bool vBool;

        [FieldOffset(4)]
        public readonly int vInt;

        [FieldOffset(4)]
        public readonly float vFloat;

        // overlapping object values
        [FieldOffset(8)]
        public readonly string vString;

        [FieldOffset(8)]
        public readonly object vObject;

        [FieldOffset(8)]
        public readonly Function vFunction;

        [FieldOffset(8)]
        public readonly Frame vFrame;

        [FieldOffset(8)]
        public readonly Dictionary<Value, Value> vTable;

        public static readonly Value Nil = new Value(DataType.Nil);
        public static readonly Value True = new Value(true);
        public static readonly Value False = new Value(false);

        public Value(DataType d, string str) : this()
        {
            DataType = d;
            vString = str;
        }

        public Value(DataType d, short info = 0) : this()
        {
            DataType = d;
            DataTypeInfo = info;
            if (d == DataType.Table)
            {
                vTable = new Dictionary<Value, Value>();
            }
        }

        public Value(bool v) : this(DataType.Bool) => vBool = v;
        public Value(int v, short info = 0) : this(DataType.Int, info) => vInt = v;
        public Value(float v) : this(DataType.Float) => vFloat = v;
        public Value(string v) : this(DataType.String) => vString = v;
        public Value(object v) : this(DataType.Object) => vObject = v;
        public Value(Frame frame, int reference) : this(DataType.Reference)
        {
            vFrame = frame;
            vInt = reference;
        }
        public Value(OpCode op) : this() { DataType = DataType.OpCode; vInt = (int) op; }

        public Value(DataType type, object obj) : this(type)
        {
            vObject = obj;
        }

        public static Value AddressRef(string str) => new Value(DataType.AddressRef, str);
        public static Value Address(string str) => new Value(DataType.Address, str);
        public static Value Label(string str) => new Value(DataType.Label, str);

        public Value Resolve()
        {
            if (DataType == DataType.Reference)
            {
                return vFrame.GetRegister(vInt).Resolve();
            }

            return this;
        }

        public void SetValue(Value key, Value val)
        {
            if (DataType != DataType.Table)
            {
                throw new ValueAccessError("Value is " + DataType + ", expected " + DataType.Table);
            }
            vTable[key] = val;
        }
        public Value GetValue(Value key)
        {
            if (DataType != DataType.Table)
            {
                throw new ValueAccessError("Value is " + DataType + ", expected " + DataType.Table);
            }
            Value value = vTable.TryGetValue(key, out Value v) ? v : Nil;
            return value;
        }

        public int Call(VM vm, int argCount)
        {
            switch (DataType)
            {
                case DataType.NativeFunction:
                    Type objType = vObject.GetType();
                    if (vObject is Action<string> actionStr)
                    {
                        Value v = vm.GetTop(argCount);
                        if (v.DataType == DataType.Nil)
                        {
                            actionStr(null);
                        }
                        else
                        {
                            actionStr(v.ToString());
                        }
                        return 0;
                    }
                    throw new ExecutionError("don't know how to handle call on " + vObject);
                default:
                    throw new ExecutionError("Cannot call value as function: " + this.ToString());
            }
        }

        public static implicit operator Value(string str) => new Value(str);
        public static implicit operator Value(bool b) => new Value(b);
        public static implicit operator Value(int i) => new Value(i);
        public static implicit operator Value(OpCode op) => new Value(op);
        public static implicit operator bool(Value a)
        {
            switch (a.DataType)
            {
                case DataType.Nil:
                    return false;
                case DataType.Float:
                case DataType.Int:
                    return true;
                case DataType.Bool:
                    return a.vBool;
                case DataType.Object:
                    return a.vObject != null;
                case DataType.String:
                    return a.vString != null;
                case DataType.Reference:
                    return a.Resolve();
            }

            throw new ExecutionError("Invalid Value type can't be cast to bool: " + a.DataType);
        }

        public static Value operator -(Value a)
        {
            switch (a.DataType)
            {
                case DataType.Int:
                    return new Value(-a.vInt);
                case DataType.Float:
                    return new Value(-a.vFloat);
                case DataType.Reference:
                    return -a.Resolve();
            }
            throw new ExecutionError("Operation - " + a.DataType + " not supported");
        }

        public static Value operator <(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return a.vInt < b.vInt;
                    case DataType.Float:
                        return a.vFloat < b.vFloat;
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " < " + b.DataType + " not supported");
        }

        public static Value operator <=(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return a.vInt <= b.vInt;
                    case DataType.Float:
                        return a.vFloat <= b.vFloat;
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " <= " + b.DataType + " not supported");
        }

        public static Value operator >(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return a.vInt > b.vInt;
                    case DataType.Float:
                        return a.vFloat > b.vFloat;
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " > " + b.DataType + " not supported");
        }

        public static Value operator >=(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return a.vInt >= b.vInt;
                    case DataType.Float:
                        return a.vFloat >= b.vFloat;
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " >= " + b.DataType + " not supported");
        }

        public static Value operator -(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return new Value(a.vInt - b.vInt);
                    case DataType.Float:
                        return new Value(a.vFloat - b.vFloat);
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " - " + b.DataType + " not supported");
        }

        public static Value operator *(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return new Value(a.vInt * b.vInt);
                    case DataType.Float:
                        return new Value(a.vFloat * b.vFloat);
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " * " + b.DataType + " not supported");
        }

        public static Value operator /(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return new Value(a.vInt / b.vInt);
                    case DataType.Float:
                        return new Value(a.vFloat / b.vFloat);
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " / " + b.DataType + " not supported");
        }

        public static Value operator %(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return new Value(a.vInt % b.vInt);
                    case DataType.Float:
                        return new Value(a.vFloat % b.vFloat);
                }
            }
            throw new ExecutionError("Operation " + a.DataType + " % " + b.DataType + " not supported");
        }

        public static Value operator +(Value a, Value b)
        {
            a = a.Resolve();
            b = b.Resolve();
            if (a.DataType == b.DataType)
            {
                switch (a.DataType)
                {
                    case DataType.Int:
                        return new Value(a.vInt + b.vInt);
                    case DataType.Float:
                        return new Value(a.vFloat + b.vFloat);
                    case DataType.String:
                        return new Value(a.vString + b.vString);
                }
            }
            if (a.DataType == DataType.Int && b.DataType == DataType.Float)
            {
                return new Value(a.vInt + b.vFloat);
            }
            if (b.DataType == DataType.Int && a.DataType == DataType.Float)
            {
                return new Value(b.vInt + a.vFloat);
            }
            if (a.DataType == DataType.String && b.DataType == DataType.Float)
            {
                return new Value(b.vString + a.vFloat);
            }
            if (a.DataType == DataType.String && b.DataType == DataType.Int)
            {
                return new Value(b.vString + a.vInt);
            }
            throw new ExecutionError("Operation " + a.DataType + " + " + b.DataType + " not supported");

        }

        public override string ToString()
        {
            Value s = Resolve();
            switch (s.DataType)
            {
                case DataType.Nil:
                    return "nil";
                case DataType.Table:
                    return "table";
                case DataType.Bool:
                    return s.vBool.ToString();
                case DataType.Int:
                    return s.vInt.ToString();
                case DataType.Float:
                    return s.vFloat.ToString();
                case DataType.String:
                    return s.vString;
                case DataType.Address:
                    return "[Address: " + s.vString + "]";
                case DataType.AddressRef:
                    return "[AddressRef: " + s.vString + "]";
                case DataType.OpCode:
                    return "[Opcode: " + ((OpCode) s.vInt) + "]";
                default:
                    return "??";
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Value v) || v.DataType != DataType)
            {
                return false;
            }

            Value s = Resolve();
            s = s.Resolve();
            v = v.Resolve();

            switch (s.DataType)
            {
                default : return v.vInt == s.vInt;
                case DataType.String:
                        return v.vString == s.vString;
                case DataType.Object:
                        return v.vObject == s.vObject;
                case DataType.Table:
                        return v.vTable == s.vTable;
            }
        }

        public override int GetHashCode()
        {
            switch (DataType)
            {
                case DataType.Nil:
                    return 0;
                case DataType.Int:
                    return vInt;
                case DataType.Float:
                    return vInt;
                case DataType.Object:
                    return vObject.GetHashCode();
                case DataType.String:
                    return vString.GetHashCode();
                case DataType.Table:
                    return vTable.GetHashCode();
                case DataType.Reference:
                    return Resolve().GetHashCode();
            }
            throw new ExecutionError("Invalid datatype: " + DataType);
        }
    }
}