using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace JoyScript
{
    public static class JoyVMOTableTests
    {
        [Test]
        public static void LoadStore()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(DataType.Table),
                    OpCode.PushValueLiteral,
                    new Value("key"),
                    OpCode.PushValueLiteral,
                    new Value("value"),
                    OpCode.StoreTableKey,
                    OpCode.PushValueLiteral,
                    new Value("key"),
                    OpCode.LoadTableKey,
            });

            Assert.AreNotEqual(new Value("key"), vm.GetTop());
            Assert.AreEqual(new Value("value"), vm.GetTop());
        }

        [Test]
        public static void LoadStoreLiteral()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(DataType.Table),
                    OpCode.StoreTableKVLiteral,
                    new Value("key"),
                    new Value("value"),
                    OpCode.LoadTableKeyLiteral,
                    new Value("key"),

            });

            Assert.AreEqual(new Value("value"), vm.GetTop());
        }

        private static VM CreateAndExecuteVM(List<Value> instructions)
        {
            VM vm = new VM();
            vm.Load(instructions);
            vm.Execute();
            return vm;
        }
    }
}