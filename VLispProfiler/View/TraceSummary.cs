using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class TraceSummary
    {
        private Dictionary<int, TraceSummaryItem> lookup;

        public TraceSummary(TraceItem item)
        {
            lookup = new Dictionary<int, TraceSummaryItem>();
            BuildLookup(lookup, item);
        }

        public IEnumerable<TraceSummaryItem> Items => lookup.Values;

        public IEnumerable<TraceSummaryItem> GetTopCalled()
        {
            return Items.OrderByDescending(curr => curr.Called);
        }

        public IEnumerable<TraceSummaryItem> GetTopElapsed()
        {
            return Items.OrderByDescending(curr => curr.Elapsed);
        }

        public IEnumerable<TraceSummaryItem> GetTopSelfElapsed()
        {
            return Items.OrderByDescending(curr => curr.SelfElapsed);
        }

        private static void BuildLookup(Dictionary<int, TraceSummaryItem> lookup, TraceItem curr)
        {
            TraceSummaryItem summary = null;
            if (!lookup.TryGetValue(curr.Id, out summary))
            {
                summary = new TraceSummaryItem()
                {
                    Id = curr.Id,
                    Called = 0,
                    Elapsed = TimeSpan.Zero
                };

                lookup.Add(curr.Id, summary);
            }

            summary.Called++;
            summary.Elapsed += curr.Elapsed;
            summary.SelfElapsed += curr.SelfElapsed;

            if (curr.HasItems)
            {
                foreach (var item in curr.Items)
                {
                    BuildLookup(lookup, item);
                }
            }
        }
    }

    public class TraceSummaryItem
    {
        public int Id { get; set; }
        public int Called { get; set; }
        public TimeSpan Elapsed { get; set; }
        public TimeSpan SelfElapsed { get; set; }
    }
}
