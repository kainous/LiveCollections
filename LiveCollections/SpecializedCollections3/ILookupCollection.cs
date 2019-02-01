using System.Linq;

namespace SpecializedCollections {
    public interface ILookupCollection<TKey, TValue> : ILookup<TKey, TValue> {
        new IGroupingCollection<TKey, TValue> this[TKey key] { get; }
    }    
}
