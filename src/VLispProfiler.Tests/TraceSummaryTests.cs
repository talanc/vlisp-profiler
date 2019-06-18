using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLispProfiler.View;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class TraceSummaryTests
    {
        [TestMethod]
        public void TestGetTopCalled()
        {
            // Arrange
            var item = MakeTraceItem(1, 0.0, 5.0,
                MakeTraceItem(2, 0.5, 3.0,
                    MakeTraceItem(3, 0.5, 2.5),
                    MakeTraceItem(4, 2.5, 3.0)
                ),
                MakeTraceItem(5, 3.0, 4.5,
                    MakeTraceItem(4, 3.5, 4.0),
                    MakeTraceItem(4, 4.0, 4.5)
                )
            );
            var summary = MakeSummary(item);

            // Act
            var top = summary.GetTopCalled();

            // Assert
            Assert.AreEqual(4, top.ElementAt(0).Id);
            Assert.AreEqual(3, top.ElementAt(0).Called);
        }

        [TestMethod]
        public void TestGetTopElapsed()
        {
            // Arrange
            var item = MakeTraceItem(1, 0.0, 5.0,
                MakeTraceItem(2, 0.5, 3.0,
                    MakeTraceItem(3, 0.5, 2.5),
                    MakeTraceItem(4, 2.5, 3.0)
                ),
                MakeTraceItem(5, 3.0, 4.5,
                    MakeTraceItem(4, 3.5, 4.0),
                    MakeTraceItem(4, 4.0, 4.5)
                )
            );
            var summary = MakeSummary(item);

            // Act
            var top = summary.GetTopElapsed();

            Assert.AreEqual(1, top.ElementAt(0).Id);
            Assert.AreEqual(5, top.ElementAt(0).Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void TestGetTopSelfElapsed()
        {
            // Arrange
            var item = MakeTraceItem(1, 0.0, 5.0,
                MakeTraceItem(2, 0.5, 3.0,
                    MakeTraceItem(3, 0.5, 2.5),
                    MakeTraceItem(4, 2.5, 3.0)
                ),
                MakeTraceItem(5, 3.0, 4.5,
                    MakeTraceItem(4, 3.5, 4.0),
                    MakeTraceItem(4, 4.0, 4.5)
                )
            );
            var summary = MakeSummary(item);

            // Act
            var top = summary.GetTopSelfElapsed();

            // Assert
            Assert.AreEqual(3, top.ElementAt(0).Id);
            Assert.AreEqual(2, top.ElementAt(0).Elapsed.TotalSeconds);
        }

        #region "Helpers"

        private TraceSummary MakeSummary(TraceItem item)
        {
            var summary = new TraceSummary(item);
            return summary;
        }

        private double lastElapsed = 0.0;
        private TraceItem MakeTraceIdItem(int id, params TraceItem[] items)
        {
            lastElapsed++;

            return MakeTraceItem(id, lastElapsed - 1, lastElapsed, items);
        }

        private TraceItem MakeTraceItem(int id, double inElapsed, double outElapsed, params TraceItem[] items)
        {
            var item = new TraceItem()
            {
                Id = id,
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