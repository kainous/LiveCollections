using System.Collections.Generic;

namespace SpecializedCollections {

    public interface IKeyValueProducer<in TKey, in TValue> {
        void Add(IEnumerable<IKeyValuePair<TKey, TValue>> data);
    }    

    //public interface IInsertionOrderedDictionary<TKey, TItem> : IList<KeyValuePair<TKey, TItem>>, IDictionary<TKey, TItem> {
    //}

    //[DebuggerDisplay("Count={Count}")]
    //public class InsertionOrderedDictionary<TKey, TItem> : IInsertionOrderedDictionary<TKey, TItem> {

}
