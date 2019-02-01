namespace SpecializedCollections {
    public class MultiValueDictionary<TKey, TValue> : IMultiValueDictionary<TKey, TValue> {


    }

    public static class MultiValueDictionary {
        private static T Id<T>(T value) => value;
        
        public static void Add<TKey, TValue>(this IMultiValueDictionary dictionary, TKey key)
    }
}
