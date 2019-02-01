using System;

namespace SpecializedCollections {
    public interface IKeyValueObservable<out TKey, out TValue> {
        IObservable<IKeyValuePair<TKey, TValue>> GetObservable();
    }

    //public interface IInsertionOrderedDictionary<TKey, TItem> : IList<KeyValuePair<TKey, TItem>>, IDictionary<TKey, TItem> {
    //}

    //[DebuggerDisplay("Count={Count}")]
    //public class InsertionOrderedDictionary<TKey, TItem> : IInsertionOrderedDictionary<TKey, TItem> {

}
