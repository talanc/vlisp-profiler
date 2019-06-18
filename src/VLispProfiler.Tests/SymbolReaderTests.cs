using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLispProfiler.View;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class SymbolReaderTests
    {
        [TestMethod]
        public void TestReadSymbolContent()
        {
            // Arrange
            var input = @"SymbolId,SymbolType,StartPos,EndPos,Preview
1,LoadRun,0:0,0:0,
2,Inline,1:1,4:2,(setq osmode (getvar ""...""";

            // Act
            var symbols = SymbolReader.ReadSymbolContent(input);

            // Assert
            Assert.AreEqual(2, symbols.Symbols.Count);
            Assert.AreEqual("(setq osmode (getvar \"...\"", symbols.Symbols[2].Preview);
        }
    }
}