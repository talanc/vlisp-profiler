using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class TraceItem : IEqualityComparer<TraceItem>
    {
        public int Id { get; set; }
        public TimeSpan InElapsed { get; set; }
        public TimeSpan OutElapsed { get; set; }
        public TimeSpan Elapsed => OutElapsed - InElapsed;
        public IList<TraceItem> Items { get; set; }

        public TraceItem() { }

        public bool Equals(TraceItem x, TraceItem y)
        {
            if (x == null && y == null)
                return true;

            if (x?.Id == y?.Id &&
                x.InElapsed == y.InElapsed &&
                x.OutElapsed == y.OutElapsed)
            {
                if (x.Items == null && y.Items == null)
                    return true;

                if (x.Items?.Count == y.Items?.Count)
                {
                    for (var i = 0; i < x.Items.Count; i++)
                    {
                        if (!x.Items[i].Equals(y.Items[i]))
                            return false;
                    }
                    return true;
                }
            
            }
            return false;
        }

        public int GetHashCode(TraceItem obj)
        {
            throw new NotImplementedException();
        }
    }
}
