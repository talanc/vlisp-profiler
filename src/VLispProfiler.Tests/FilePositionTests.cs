using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class FilePositionTests
    {
        [DataTestMethod]
        [DataRow("1:1")]
        public void TestParse(string s)
        {
            // Arrange

            // Act
            var fp = FilePosition.Parse(s);

            // Assert
            Assert.AreEqual(s, fp.ToString());
        }
    }
}