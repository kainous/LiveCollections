using System.Collections.Generic;
using System.Diagnostics;

namespace SpecializedCollections {
    public interface IInsertionOrderedDictionary<TKey, TItem> : IList<KeyValuePair<TKey, TItem>>, IDictionary<TKey, TItem> {        
    }

    [DebuggerDisplay("Count={Count}")]
    public class InsertionOrderedDictionary<TKey, TItem> : IInsertionOrderedDictionary<TKey, TItem> {

    }
}
