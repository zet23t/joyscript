using System;
using System.Collections.Generic;
using System.Reflection;
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

        public Value(DataType d, string str, int ival = 0) : this()
        {
            DataType = d;
            vString = str;
            vInt = ival;
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

        public static Value NativeFunction(Action action) => new Value(DataType.NativeFunction, action);
        public static Value NativeFunction<T>(Action<T> action) => new Value(DataType.NativeFunction, action);
        public static Value AddressRef(string str, int address = 0) => new Value(DataType.AddressRef, str, address);
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

        public int Call(VM vm, string methodName, int argCount)
        {
            if (DataType != DataType.Object)
            {
                throw new ExecutionError("don't know how to handle method call on " + ToString() + " with method " + methodName);
            }

            var methods = vObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (method.Name != methodName || method.ContainsGenericParameters)
                {
                    continue;
                }
                ParameterInfo[] parameterInfo = method.GetParameters();
                if (parameterInfo.Length != argCount)
                {
                    continue;
                }
                object[] args = new object[parameterInfo.Length];
                for (int i = parameterInfo.Length - 1; i >= 0; i -= 1)
                {
                    Value v = vm.Pop();
                    Type parameterType = parameterInfo[i].ParameterType;
                    if (parameterType == typeof(bool))
                    {
                        args[i] = (bool) v;
                        continue;
                    }
                    if (parameterType == typeof(int) || parameterType == typeof(long))
                    {
                        args[i] = (int) v;
                        continue;
                    }
                    if (parameterType == typeof(float) || parameterType == typeof(double))
                    {
                        args[i] = (float) v;
                        continue;
                    }
                    if (parameterType == typeof(string))
                    {
                        if (v.DataType == DataType.Nil)
                        {
                            args[i] = null;
                        }
                        args[i] = v.ToString();
                        continue;
                    }
                    throw new ExecutionError("Unsupported method argument type #"+i+": "+parameterType);
                }
                var result = method.Invoke(vObject, args);
                var resultParameter = method.ReturnParameter.ParameterType;
                if (resultParameter == typeof(void))
                {
                    return 0;
                }
                if (resultParameter == typeof(bool))
                {
                    vm.GetCurrentFrame().Push((bool) result);
                    return 1;
                }
                if (resultParameter == typeof(int) || resultParameter == typeof(long))
                {
                    vm.GetCurrentFrame().Push((int) result);
                    return 1;
                }
                if (resultParameter == typeof(float) || resultParameter == typeof(double))
                {
                    vm.GetCurrentFrame().Push((float) result);
                    return 1;
                }

                if (resultParameter == typeof(string))
                {
                    vm.GetCurrentFrame().Push((string) result);
                    return 1;
                }

                vm.GetCurrentFrame().Push(new Value(DataType.Object, result));
                return 1;
            }
            throw new ExecutionError("Can't call method "+methodName+" with "+argCount+" arguments on "+vObject);
        }

        public int Call(VM vm, int argCount)
        {
            switch (DataType)
            {
                case DataType.NativeFunction:
                    Type objType = vObject.GetType();

                    switch (vObject)
                    {
                        case Action action:
                            vm.Pop(argCount);
                            action();
                            return 0;

                        case Action<int> action:
                            action(vm.Pop(argCount).vInt);
                            return 0;

                        case Action<string> action:
                            Value v = vm.Pop(argCount);
                            if (v.DataType == DataType.Nil)
                            {
                                action(null);
                            }
                            else
                            {
                                action(v.ToString());
                            }
                            return 0;
                    }

                    throw new ExecutionError("don't know how to handle call on " + vObject);
                default:
                    throw new ExecutionError("Cannot call value as function: " + this.ToString());
            }
        }

        public static explicit operator float(Value v)
        {
            switch (v.DataType)
            {
                case DataType.Int:
                    return (float) v.vInt;
                case DataType.Float:
                    return v.vFloat;
                default:
                    throw new ExecutionError("Can't case " + v.DataType + " to float");
            }
        }

        public static explicit operator int(Value v)
        {
            switch (v.DataType)
            {
                case DataType.Int:
                    return v.vInt;
                case DataType.Float:
                    return (int) v.vFloat;
                default:
                    throw new ExecutionError("Can't case " + v.DataType + " to int");
            }
        }

        public static implicit operator Value(string str) => new Value(str);
        public static implicit operator Value(bool b) => new Value(b);
        public static implicit operator Value(float f) => new Value(f);
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