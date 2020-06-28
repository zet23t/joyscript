using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace JoyScript
{
    public class JoyCompilerFunctionCallTests : JoyVMTests
    {
        [Test]
        public static void TestFunctionCall1Arg()
        {
            string result = null;
            CreateAndExecuteVM(@"print(""hello world\nHow are you?"")", new Dictionary<string, object>() { 
                { "print", new Action<string>((str) => result = str) } });
            Assert.AreEqual("hello world\nHow are you?", result);
        }

        [Test]
        public static void TestFunctionCall2Args()
        {
            string a = null;
            string b = null;
            CreateAndExecuteVM(@"call(""a"", ""b"")", new Dictionary<string, object>() { 
                { "call", new Action<string, string>((aa, bb) => { a = aa; b = bb;}) } });
            Assert.AreEqual("a", a);
            Assert.AreEqual("b", b);

        }
    }
}