using Microsoft.Extensions.Logging.Abstractions;
using SK0520.Plugins.TextIO.Models;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SK0520.Plugins.TextIO.Test.Models
{
    public class ScriptLoaderTest
    {
        [Fact]
        public void LoadSourceTest_Throw()
        {
            var scriptLoader = new ScriptLoader(NullLoggerFactory.Instance);

            var actual1 = Assert.Throws<ArgumentException>(() => scriptLoader.LoadSource(""));
            Assert.Equal("/**", actual1.Message);

            var actual2 = Assert.Throws<ArgumentException>(() => scriptLoader.LoadSource("/**"));
            Assert.Equal("*/", actual2.Message);
        }

        [Fact]
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

            Assert.Equal("NAME", actual1.Head.Name);

            int currentIndex = 0;
            Assert.Equal("name1", actual1.Head.Parameters[currentIndex].Name);
            Assert.Equal("なまえ1", actual1.Head.Parameters[currentIndex].Display);
            Assert.True(actual1.Head.Parameters[currentIndex].Required);
            Assert.Equal(ScriptParameterKind.String, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 1;
            Assert.Equal("name2", actual1.Head.Parameters[currentIndex].Name);
            Assert.Equal("なまえ2", actual1.Head.Parameters[currentIndex].Display);
            Assert.False(actual1.Head.Parameters[currentIndex].Required);
            Assert.Equal(ScriptParameterKind.Integer, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 2;
            Assert.Equal("name3", actual1.Head.Parameters[currentIndex].Name);
            Assert.Equal("name3", actual1.Head.Parameters[currentIndex].Display);
            Assert.True(actual1.Head.Parameters[currentIndex].Required);
            Assert.Equal(ScriptParameterKind.Decimal, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 3;
            Assert.Equal("name4", actual1.Head.Parameters[currentIndex].Name);
            Assert.Equal("name4", actual1.Head.Parameters[currentIndex].Display);
            Assert.False(actual1.Head.Parameters[currentIndex].Required);
            Assert.Equal(ScriptParameterKind.DateTime, actual1.Head.Parameters[currentIndex].Kind);

            currentIndex = 4;
            Assert.Equal("name5", actual1.Head.Parameters[currentIndex].Name);
            Assert.Equal("name5", actual1.Head.Parameters[currentIndex].Display);
            Assert.True(actual1.Head.Parameters[currentIndex].Required);
            Assert.Equal(ScriptParameterKind.Boolean, actual1.Head.Parameters[currentIndex].Kind);

            Assert.Equal("BODY", actual1.Body.Source);
        }
    }
}
