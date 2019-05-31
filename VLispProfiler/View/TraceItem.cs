using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class TraceItem : IEquatable<TraceItem>
    {
        public int Id { get; set; }
        public TimeSpan InElapsed { get; set; }
        public TimeSpan OutElapsed { get; set; }
        public TimeSpan Elapsed => OutElapsed - InElapsed;
        public IList<TraceItem> Items { get; set; }

        public TraceItem() { }

        public bool Equals(TraceItem other)
        {
            if (other == null)
                return false;

            if (Id == other.Id && InElapsed == other.InElapsed && OutElapsed == other.OutElapsed)
            {
                if (Items == null && other.Items == null)
                    return true;

                if (Items?.Count == other.Items?.Count)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (!Items[i].Equals(other.Items[i]))
                            return false;
                    }
                    return true;
                }

            }

            return false;
        }
    }
}
