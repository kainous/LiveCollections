using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Transactions;

namespace Halliburton.IC.SpecializedCollections {
    //public enum NotificationKind {
    //    Addition,
    //    Deletion
    //}

    //public class Notification<T> {
    //    public NotificationKind Kind { get; }
    //    public T Item { get; }

    //    public Notification(T item, NotificationKind kind) {
    //        Kind = kind;
    //        Item = item;
    //    }
    //}

    ////public class Addition


    //public class TransactionalDictionary<TKey, TValue> {
        

    //    private class DictionaryEnlistment : IPromotableSinglePhaseNotification {
    //        ConcurrentQueue<Notification<(TKey, TValue)>

    //        public void Initialize() {
    //            throw new System.NotImplementedException();
    //        }

    //        public byte[] Promote() {
    //            throw new System.NotImplementedException();
    //        }

    //        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment) {
    //            throw new System.NotImplementedException();
    //        }

    //        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment) {
    //            throw new System.NotImplementedException();
    //        }
    //    }

    //    public void Add(IEnumerable<(TKey, TValue)> items) {
    //        Transaction.Current.EnlistPromotableSinglePhase(new DictionaryEnlistment())
    //    }


    //    void Test() {



    //        using (var scope = new TransactionScope()) {
    //            Transaction.Current.En
    //        }
    //    }
    //}
}
