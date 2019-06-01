using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class LispFileTests
    {
        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("    ", "    ")]
        [DataRow("\t", "        ")]
        [DataRow("       \t", "        ")]
        [DataRow("\t\t", "                ")]
        [DataRow("\ta\t", "        a       ")]
        [DataRow("a\tbb\tccc\t", "a       bb      ccc     ")]
        [DataRow("a\tbb\r\nccc\tdddd", "a       bb\r\nccc     dddd")]
        public void TestConvertTabsToSpaces(string input, string expected)
        {
            // Arrange

            // Act
            var output = LispFile.ConvertTabsToSpaces(input, tabWidth: 8);

            // Assert
            Assert.AreEqual(expected, output);
        }
    }
}
