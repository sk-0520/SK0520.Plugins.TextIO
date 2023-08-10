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
            * @parameters:name1#require=true:string
            * @parameters:name2#require=false:integer
            @parameters:name3#require=true:decimal
            @parameters:name4#require=false:datetime
            * @parameters:name5#require=true:boolean
            */
            BODY
            ");

            Assert.AreEqual("NAME", actual1.Head.Name);

            Assert.AreEqual("name1", actual1.Head.Parameters[0].Name);
            Assert.IsTrue(actual1.Head.Parameters[0].Required);
            Assert.AreEqual(ScriptParameterKind.String, actual1.Head.Parameters[0].Kind);

            Assert.AreEqual("name2", actual1.Head.Parameters[1].Name);
            Assert.IsFalse(actual1.Head.Parameters[1].Required);
            Assert.AreEqual(ScriptParameterKind.Integer, actual1.Head.Parameters[1].Kind);

            Assert.AreEqual("name3", actual1.Head.Parameters[2].Name);
            Assert.IsTrue(actual1.Head.Parameters[2].Required);
            Assert.AreEqual(ScriptParameterKind.Decimal, actual1.Head.Parameters[2].Kind);

            Assert.AreEqual("name4", actual1.Head.Parameters[3].Name);
            Assert.IsFalse(actual1.Head.Parameters[3].Required);
            Assert.AreEqual(ScriptParameterKind.DateTime, actual1.Head.Parameters[3].Kind);

            Assert.AreEqual("name5", actual1.Head.Parameters[4].Name);
            Assert.IsTrue(actual1.Head.Parameters[4].Required);
            Assert.AreEqual(ScriptParameterKind.Boolean, actual1.Head.Parameters[4].Kind);

            Assert.AreEqual("BODY", actual1.Body.Source);
        }
    }
}
