using System;
using System.Collections.Generic;
using UnityEngine;

namespace JoyScript
{
    public class JoyVMTests
    {
        protected static VM CreateAndExecuteVM(string code, Dictionary<string, object> globals = null)
        {
            List<Value> instructions = new Compiler().Compile(code);
            VM.PrintInstructions(instructions);
            return CreateAndExecuteVM(instructions, globals);
        }

        protected static VM CreateAndExecuteVM(List<Value> instructions, Dictionary<string, object> globals = null)
        {
            VM vm = new VM();
            if (globals != null)
            {
                foreach (var entry in globals)
                {
                    vm.SetGlobal(entry.Key, new Value(DataType.NativeFunction, entry.Value));
                }
            }
            vm.Load(instructions);
            vm.Execute();
            return vm;
        }
    }
}