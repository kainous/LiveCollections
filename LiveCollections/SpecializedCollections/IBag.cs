using System.Linq;

namespace SpecializedCollections {
    public interface IBag<TKey, TValue> : ILookup<TKey, TValue> {
        void Add(ILookup<TKey, TValue> items);
        void Clear();
        bool ContainsKey(TKey key);
        ILookup<TKey, TValue> ToLookup();
    }
}
