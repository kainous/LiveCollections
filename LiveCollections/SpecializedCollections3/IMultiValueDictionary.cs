using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecializedCollections {
    public interface IMultiValueDictionary <TKey, TValue> : ILookup<TKey, TValue>, ILookupCollection<TKey, TValue> {
        Task Add(IEnumerable<Tuple<TKey, TValue>> items);
        void Clear();
    }
}
