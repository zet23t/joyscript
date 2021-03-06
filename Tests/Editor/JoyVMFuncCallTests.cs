using System.Collections.Generic;
using NUnit.Framework;

namespace JoyScript
{
    public class JoyVMFuncCallTests : JoyVMTests
    {
        [Test]
        public static void CallTest()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.Jump, Value.AddressRef("start"),
                    Value.Address("func"),
                    OpCode.PushValueLiteral, 5,
                    OpCode.PushValueLiteral, 1,
                    OpCode.Return,
                    Value.Address("start"),
                    OpCode.PushValueLiteral, Value.AddressRef("func"),
                    OpCode.PushValueLiteral, 0,
                    OpCode.Call,
            });

            Assert.AreEqual(new Value(5), vm.GetTop());
        }

        [Test]
        public static void ActionArgStringTest()
        {
            string result = null;
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, "hello world",
                    OpCode.PushValueLiteral, Value.NativeFunction((string str) => result = str),
                    OpCode.PushValueLiteral, 1,
                    OpCode.Call,
            });

            Assert.AreEqual(0, vm.GetCurrentFrame().Count);
            Assert.AreEqual(Value.Nil, vm.GetTop());
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public static void ActionArgStringNullTest()
        {
            string result = "not null";
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, Value.NativeFunction((string str) => result = str),
                    OpCode.PushValueLiteral, 0,
                    OpCode.Call,
            });
            Assert.AreEqual(null, result);
        }

        [Test]
        public static void ActionArgIntTest()
        {
            int result = int.MinValue;
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, Value.NativeFunction((int ival) => result = ival),
                    OpCode.PushValueLiteral, 0,
                    OpCode.Call,
            });
            Assert.AreEqual(0, result);
        }

        [Test]
        public static void ActionTest()
        {
            bool called = false;
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, Value.NativeFunction(() => called = true),
                    OpCode.PushValueLiteral, 0,
                    OpCode.Call,
            });
            Assert.AreEqual(true, called);
        }

        private class TestClass
        {
            public bool Called;
            public void Call()
            {
                Called = true;
            }
            public bool TestTrue() => true;
        }

        [Test]
        public static void ObjectMethodCallTest()
        {
            var tc = new TestClass();
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, "Call",
                    OpCode.PushValueLiteral, new Value(tc),
                    OpCode.PushValueLiteral, 0,
                    OpCode.CallMethod,
            });
            Assert.AreEqual(0, vm.GetCurrentFrame().Count);
            Assert.AreEqual(true, tc.Called);
        }

        [Test]
        public static void ObjectMethodCallReturnTrueTest()
        {
            var tc = new TestClass();
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, "TestTrue",
                    OpCode.PushValueLiteral, new Value(tc),
                    OpCode.PushValueLiteral, 0,
                    OpCode.CallMethod,
            });
            Assert.AreEqual(Value.True, vm.Pop());
        }

        [Test]
        public static void IfTest()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, true,
                    OpCode.PushValueLiteral, true,
                    OpCode.JumpIf, Value.AddressRef("toc"),
                    OpCode.PushValueLiteral, false,
                    Value.Address("toc"),
                    OpCode.JumpIf, Value.AddressRef("exit"),
                    OpCode.PushValueLiteral, false,
                    Value.Address("exit")
            });

            Assert.AreEqual(0, vm.GetCurrentFrame().Count);
        }

        [Test]
        public static void Loop()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral, new Value(DataType.Table),
                    OpCode.StoreTableKVLiteral, "i", 0,
                    OpCode.PushValueLiteral, 5,
                    Value.Address("loop"),
                    OpCode.Dec,
                    OpCode.PushValue, 0,
                    OpCode.LoadTableKeyLiteral, "i",
                    OpCode.Inc,
                    OpCode.StoreTableKeyLiteral, "i",
                    OpCode.Pop,
                    OpCode.DuplicateTop,
                    OpCode.PushValueLiteral, 0,
                    OpCode.LowerThan,
                    OpCode.JumpIf, Value.AddressRef("loop"),
                    OpCode.Pop,
                    OpCode.LoadTableKeyLiteral, "i"
            });

            Assert.AreEqual(new Value(5), vm.GetTop());
        }

    }
}