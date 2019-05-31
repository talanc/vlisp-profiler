﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLispProfiler.View;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class TraceItemTests
    {
        [TestMethod]
        public void TestElapsed()
        {
            // Arrange
            var item = MakeTraceItem(0, 10);

            // Act
            var elapsed = item.Elapsed;

            // Assert
            Assert.AreEqual(10, elapsed.TotalSeconds);
        }

        [TestMethod]
        public void TestSelfElapsed()
        {
            // Arrange
            var item = MakeTraceItem(0, 10, MakeTraceItem(4, 5));

            // Act
            var selfElapsed = item.SelfElapsed;

            // Assert
            Assert.AreEqual(9, selfElapsed.TotalSeconds);
        }

        [TestMethod]
        public void TestSelfElapsed_NoItems_MatchesElapsed()
        {
            // Arrange
            var item = MakeTraceItem(0, 10);

            // Act

            // Assert
            Assert.AreEqual(item.Elapsed, item.SelfElapsed);
        }

        #region "Helpers"

        private int nextId = 0;
        private TraceItem MakeTraceItem(int inElapsed, int outElapsed, params TraceItem[] items)
        {
            nextId++;

            var item = new TraceItem()
            {
                Id = nextId,
                InElapsed = TimeSpan.FromSeconds(inElapsed),
                OutElapsed = TimeSpan.FromSeconds(outElapsed),
            };

            if (items != null && items.Any())
                item.Items = new List<TraceItem>(items);

            return item;
        }

        #endregion
    }

}
