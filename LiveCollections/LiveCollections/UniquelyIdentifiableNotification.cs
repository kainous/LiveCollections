using System.Collections.Generic;

namespace System.Collections.LiveCollections {
    public interface IUniquelyIdentifiable<out TKey> {
        TKey Identifier { get; }
    }

    public abstract class UniquelyIdentifiableNotification<TKey, TItem>
        where TItem : IUniquelyIdentifiable<TKey> {

        protected internal UniquelyIdentifiableNotification() { }

        public sealed class Insert : UniquelyIdentifiableNotification<TKey, TItem> {
            public TItem Item { get; private set; }
            public Insert(TItem item) {
                Item = item;
            }

            public override UniquelyIdentifiableNotification<TKey, TResult> Cast<TResult>() {
                return new UniquelyIdentifiableNotification<TKey, TResult>.Insert((TResult)Item);
            }
        }

        public sealed class Remove : UniquelyIdentifiableNotification<TKey, TItem> {
            public TKey Key { get; private set; }
            public Remove(TKey key) {
                Key = key;
            }

            public override UniquelyIdentifiableNotification<TKey, TResult> Cast<TResult>() {
                return new UniquelyIdentifiableNotification<TKey, TResult>.Remove(Key);
            }
        }

        public sealed class Clear : UniquelyIdentifiableNotification<TKey, TItem> {
            public override UniquelyIdentifiableNotification<TKey, TResult> Cast<TResult>() {
                return new UniquelyIdentifiableNotification<TKey, TResult>.Clear();
            }
        }

        public abstract UniquelyIdentifiableNotification<TKey, TResult> Cast<TResult>()
                where TResult : TItem, IUniquelyIdentifiable<TKey>;
    }

    public abstract class Notification<T> {
        protected internal Notification() { }

        public class Error : Notification<T> {
            public Exception Exception { get; private set; }
            public Error(Exception ex) {
                Exception = ex;
            }
        }

        public class Change : Notification<T> {
            public IEnumerable<T> Changes { get; private set; }
            public Change(IEnumerable<T> changes) {
                Changes = changes;
            }
        }

        public class Message : Notification<T> {
            public string Text { get; private set; }
            public Message(string text) {
                Text = text;
            }
        }

        public class Completed : Notification<T> {
            public Completed() { }
        }
    }

    public abstract class NotificationWithProgress<T> : Notification<T> {
        protected internal NotificationWithProgress() { }

        public class Indeterminate : NotificationWithProgress<T> {
            public Indeterminate() { }
        }

        public class ProgressMaximum : NotificationWithProgress<T> {
            public double MaximumValue { get; private set; }
            public ProgressMaximum(double maximumValue) {
                MaximumValue = maximumValue;
            }
        }

        public class Progress : NotificationWithProgress<T> {
            public double ProgressValue { get; private set; }
            public Progress(double progressValue) {
                ProgressValue = progressValue;
            }
        }
    }

    public interface IObservableHost<T> {
        IObservable<T> GetObservable();
    }

    public interface IUniqueLiveCollection<TKey, TItem> 
        : IObservableHost<Notification<UniquelyIdentifiableNotification<TKey, TItem>>>
        where TItem : IUniquelyIdentifiable<TKey> {
    }


}
