using System;
using System.Collections.Generic;

namespace JoyScript
{
    public class JoyVMTests
    {
        protected static VM CreateAndExecuteVM(string code, Action<string> onPrint = null)
        {
            return CreateAndExecuteVM(new Compiler().Compile(code), onPrint);
        }

        protected static VM CreateAndExecuteVM(List<Value> instructions, Action<string> onPrint = null)
        {
            VM vm = new VM();
            if (onPrint != null)
            {
                vm.SetGlobal("print", new Value(DataType.NativeFunction, onPrint));
            }
            vm.Load(instructions);
            vm.Execute();
            return vm;
        }
    }
}