namespace System.Collections.LiveCollections

open System
open System.ComponentModel
open System.Reactive.Linq
open System.Reactive.Subjects

type KeyedCollectionNotification<'TKey, 'TItem> =
| Insertion of key:'TKey * item:'TItem
| Deletion  of key:'TKey
| Reset

type SourceUpdate<'T> =
| Modifications of seq<'T>
| SourceError   of exn
| Disconnected

type IObservableHost<'T> =
  abstract GetObservable : unit -> IObservable<'T>

type ILiveCollection<'TKey, 'TItem> =
  inherit IObservableHost<SourceUpdate<KeyedCollectionNotification<'TKey, 'TItem>>>

