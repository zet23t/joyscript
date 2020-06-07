using System.Collections.Generic;
using NUnit.Framework;

namespace JoyScript
{
    public class JoyCompilerFunctionCallTests : JoyVMTests
    {
        [Test]
        public static void TestFunctionCall()
        {
            string result = null;
            CreateAndExecuteVM(@"print(""hello world"")", (str) => result = str);
            Assert.AreEqual("hello world", result);
        }
    }
}