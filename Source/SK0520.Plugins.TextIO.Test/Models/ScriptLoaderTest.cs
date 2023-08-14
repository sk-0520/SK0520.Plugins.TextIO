using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SK0520.Plugins.TextIO.Models;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Test.Models
{
    [TestClass]
    public class ScriptLoaderTest
    {
        [TestMethod]
        public void LoadSourceTest_Throw()
        {
            var scriptLoader = new ScriptLoader(NullLoggerFactory.Instance);

            var actual1 = Assert.ThrowsException<ArgumentException>(() => scriptLoader.LoadSource(""));
            Assert.AreEqual("/**", actual1.Message);

            var actual2 = Assert.ThrowsException<ArgumentException>(() => scriptLoader.LoadSource("/**"));
            Assert.AreEqual("*/", actual2.Message);
        }

        [TestMethod]
        public void LoadSourceTest()
        {
            var scriptLoader = new ScriptLoader(NullLoggerFactory.Instance);

            var actual1 = scriptLoader.LoadSource(@"/**
            * @name: NAME
            * @parameters:name1#display=なまえ1#require=true:string
            * @parameters:name2#display=なまえ2#require=false:integer
            @parameters:name3#require=true:decimal
            @parameters:name4#require=false:datetime
            * @parameters:name5#require=true:boolean
            */
            BODY
            ");

            Assert.AreEqual("NAME", actual1.Head.Name);

            int currentIndex = 0;
            Assert.AreEqual("name1", actual1.Head.Parameters[currentIndex].Name);
            Assert.AreEqual("なまえ1", actual1.Head.Parameters[currentIndex].Display);
            Assert.IsTrue(actual1.Head.Parameters[currentIndex].Required);
            Assert.AreEqual(ScriptParameterKind.String, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 1;
            Assert.AreEqual("name2", actual1.Head.Parameters[currentIndex].Name);
            Assert.AreEqual("なまえ2", actual1.Head.Parameters[currentIndex].Display);
            Assert.IsFalse(actual1.Head.Parameters[currentIndex].Required);
            Assert.AreEqual(ScriptParameterKind.Integer, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 2;
            Assert.AreEqual("name3", actual1.Head.Parameters[currentIndex].Name);
            Assert.AreEqual("name3", actual1.Head.Parameters[currentIndex].Display);
            Assert.IsTrue(actual1.Head.Parameters[currentIndex].Required);
            Assert.AreEqual(ScriptParameterKind.Decimal, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 3;
            Assert.AreEqual("name4", actual1.Head.Parameters[currentIndex].Name);
            Assert.AreEqual("name4", actual1.Head.Parameters[currentIndex].Display);
            Assert.IsFalse(actual1.Head.Parameters[currentIndex].Required);
            Assert.AreEqual(ScriptParameterKind.DateTime, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 4;
            Assert.AreEqual("name5", actual1.Head.Parameters[currentIndex].Name);
            Assert.AreEqual("name5", actual1.Head.Parameters[currentIndex].Display);
            Assert.IsTrue(actual1.Head.Parameters[currentIndex].Required);
            Assert.AreEqual(ScriptParameterKind.Boolean, actual1.Head.Parameters[currentIndex].Kind);

            Assert.AreEqual("BODY", actual1.Body.Source);
        }
    }
}
