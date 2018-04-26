namespace System.Reactive.Collections

open System
open System.Collections
open System.Collections.Generic
open System.ComponentModel
open System.Collections.Specialized
open System.Threading
open System.Runtime.CompilerServices

type ITagData<'T> =
  abstract Value     : 'T
  abstract Timestamp : DateTimeOffset
  abstract DataType  : Type
  abstract CloneWith : value:'T * timestamp:DateTimeOffset -> ITagData<'T>

type ITag<'T> =
  abstract ClientData : ITagData<'T>
  abstract ServerData : ITagData<'T>

type CollectionChangeNotification<'T> =
| Insert of 'T
| Remove of 'T

type ICollectionChangeData<'T> =
  inherit IEnumerable<CollectionChangeNotification<'T>>

type ICollectionChangeObservable<'T> =
  inherit IObservable<ICollectionChangeData<'T>>

type ICollectionChangeObserver<'T> =
  inherit IObserver<ICollectionChangeData<'T>>

type IReadOnlyNotificationCollection<'T> =
  inherit IDisposable
  inherit INotifyPropertyChanged
  inherit INotifyCollectionChanged
  inherit IReadOnlyCollection<'T>

type IReactiveCollection<'T> =
  abstract ToList : unit -> IList<'T>
  abstract ToNotificationChanges : unit -> ICollectionChangeObservable<'T>
  abstract ToReadOnlyNotificationCollection : context:SynchronizationContext -> IReadOnlyNotificationCollection<'T>



[<AutoOpen>]
[<Extension>]
type ReactiveCollections = class end
  //[<Extension>]
  //static member Select(items:IReactiveCollection<'T>, selector:'T -> 'R) =
  //  { new IReactiveCollection<'R> with
      