using System.Collections.Generic;
using System.Linq;

namespace SpecializedCollections {
    public interface IGroupingCollection<TKey, TValue> : ICollection<TValue>, IGrouping<TKey, TValue> {
    }
}
