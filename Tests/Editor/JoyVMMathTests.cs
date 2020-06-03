using System.Collections.Generic;
using NUnit.Framework;

namespace JoyScript
{
    public class JoyVMMathTests : JoyVMTests
    {
        [Test]
        public static void Mul()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.PushValueLiteral,
                    new Value(3),
                    OpCode.Mul
            });

            Assert.AreEqual(new Value(6), vm.GetTop());
        }

        [Test]
        public static void Inc()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.Inc,
                    OpCode.Inc,
                    OpCode.Inc,
            });

            Assert.AreEqual(new Value(5), vm.GetTop());
        }

        [Test]
        public static void Dec()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.Dec,
                    OpCode.Dec,
                    OpCode.Dec,
            });

            Assert.AreEqual(new Value(-1), vm.GetTop());
        }

        [Test]
        public static void Div()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.PushValueLiteral,
                    new Value(6),
                    OpCode.Div
            });

            Assert.AreEqual(new Value(3), vm.GetTop());
        }

        [Test]
        public static void Mod()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.PushValueLiteral,
                    new Value(5),
                    OpCode.Mod
            });

            Assert.AreEqual(new Value(1), vm.GetTop());
        }

        [Test]
        public static void Sub()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.PushValueLiteral,
                    new Value(3),
                    OpCode.Sub
            });

            Assert.AreEqual(new Value(1), vm.GetTop());
        }

        [Test]
        public static void Add()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.PushValueLiteral,
                    new Value(3),
                    OpCode.Add
            });

            Assert.AreEqual(new Value(5), vm.GetTop());
        }

        [Test]
        public static void Neg()
        {
            VM vm = CreateAndExecuteVM(new List<Value>()
            {
                OpCode.PushValueLiteral,
                    new Value(2),
                    OpCode.Neg
            });

            Assert.AreEqual(new Value(-2), vm.GetTop());
        }
    }
}