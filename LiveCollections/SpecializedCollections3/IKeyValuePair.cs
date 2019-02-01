namespace SpecializedCollections {
    public interface IKeyValuePair<out TKey, out TValue> {
        TKey Key { get; }
        TValue Value { get; }
    }

    //public interface IInsertionOrderedDictionary<TKey, TItem> : IList<KeyValuePair<TKey, TItem>>, IDictionary<TKey, TItem> {
    //}

    //[DebuggerDisplay("Count={Count}")]
    //public class InsertionOrderedDictionary<TKey, TItem> : IInsertionOrderedDictionary<TKey, TItem> {

}
