namespace System.Reactive.Collections

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open Nito.AsyncEx
open System.Threading.Tasks
open System.Linq
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open System.ComponentModel
open FSharp.Control.Reactive

open Helpers
open System.Collections.Concurrent

type ReactiveCollectionSource<'T>(items:seq<'T>, comparer:IEqualityComparer<'T>) =
  let _lock  = AsyncReaderWriterLock()
  let _items = HashSet<'T>(items, comparer)
  let _subject = new Subject<ICollectionChangeData<'T>>()

  new items = new ReactiveCollectionSource<'T>(items, EqualityComparer<'T>.Default)

  member __.Add items =
    let changes =
      use __ = _lock.WriterLock()
      [ for item in items do
          if _items.Add item
            then yield item ]
    
    changes |> NotificationFunctor Insert |> _subject.OnNext

  member __.Remove items =
    let changes =
      use __ = _lock.WriterLock()
      [ for item in items do
          if _items.Remove item
            then yield item ]
    changes |> NotificationFunctor Remove |> _subject.OnNext

  interface IDisposable with
    member __.Dispose() = 
      _subject.Dispose()

  interface IReactiveCollection<'T> with
    member __.ToList() =
      use __ = _lock.ReaderLock()
      upcast items.ToList()

    member __.ToNotificationChanges() =
      { new ICollectionChangeObservable<'T> with
        member __.Subscribe obs = 
          let disp = _subject.Subscribe obs
          let changes =
            use __ = _lock.ReaderLock()
            _items |> Seq.toList
          obs.OnNext(NotificationFunctor Insert changes)
          disp }

    member __.ToReadOnlyNotificationCollection context =
      upcast new ReactiveObservableCollection<'T>(_subject, context)

//type TItem = class end

type ReactiveCollectionCollector<'TItem>(obs : IReactiveCollection<IReactiveCollection<'TItem>>) as this =
  let _collections = ConcurrentDictionary<IReactiveCollection<'TItem>, IDisposable * IDisposable>()
  let _subject = new Subject<IEnumerable<CollectionChangeNotification<'TItem>>>()

  let OnItemChanged (changes : IEnumerable<CollectionChangeNotification<'TItem>>) =
    _subject.OnNext changes 

  let OnItemsCompleted() = 
    this.Dispose()

  let OnCollectionChanged (changes : IEnumerable<CollectionChangeNotification<IReactiveCollection<'TItem>>>) =
    for change in changes do
      match change with
      | Insert col ->       
        let subscribe (c:IReactiveCollection<_>) = 
          let changes = c.ToNotificationChanges().Replay()
          let connection = changes.Connect()
          let subscription = changes.Subscribe(OnItemChanged, OnItemsCompleted)
          (subscription, connection)

        _collections.AddOrUpdate(col, subscribe, (fun _ v -> v)) |> ignore
      | Remove col ->
        match _collections.TryRemove col with
        | true, (a, b) -> 
          a.Dispose()
          b.Dispose()
        | false, _ -> 
          ()
        
  let _subscription, _connection =
    let changes = obs.ToNotificationChanges().Replay()
    let connection = changes.Connect();
    let subscription = changes.Subscribe OnCollectionChanged
    (subscription, connection)

  member __.Dispose() =
    _collections.Values |> Seq.iter (fun (a, b) -> a.Dispose(); b.Dispose())
    _collections.Clear()
    _subject.Dispose()
    _subscription.Dispose()
    _connection.Dispose()

  interface IReactiveCollection<'TItem> with
    member this.Dispose() =
      this.Dispose();


module ReactiveCollectionBuilders =
  let toChangeObservable (obs:IReactiveCollection<'T>) =
    obs.ToNotificationChanges() |> Observable.map(fun changes -> changes :> IEnumerable<_>)

  let map f (obs:IReactiveCollection<_>) =
    new ReactiveCollectionSelector<_,_>(toChangeObservable obs, (fun _ -> true), f)
    :> IReactiveCollection<_>

  let filter f (obs:IReactiveCollection<_>) =
    new ReactiveCollectionSelector<_,_>(toChangeObservable obs, f, id)
    

  type ReactiveCollectionBuilder() =
    member this.Yield x =
      let changes = Enumerable.Repeat(Insert x, 1)
      let obs = Observable.Return changes
      new ReactiveCollectionSelector<'T,'T>(obs, (fun _ -> true), id)
      :> IReactiveCollection<_>
    member this.YieldFrom (x:IReactiveCollection<'T>) = 
      x
    member this.Combine(x:IReactiveCollection<'T>, y:IReactiveCollection<'T>) =
      let changes = Observable.merge (toChangeObservable x) (toChangeObservable y)
      new ReactiveCollectionSelector<'T, 'T>(changes, (fun _ -> true), id)
      :> IReactiveCollection<'T>
    member this.Delay f = f()
    member this.Zero() =
      new ReactiveCollectionSelector<'T, 'T>(Observable.empty, (fun _ -> true), id)
      :> IReactiveCollection<'T>
    member this.Where(obs:IReactiveCollection<_>, predicate) =
      obs |> filter predicate
    member this.SelectMany<'TSource, 'TCollection, 'TResult>(obs:IReactiveCollection<'TSource>, collectionSelector: 'TSource -> IReactiveCollection<'TCollection>, resultSelector:'TCollection -> 'TResult) =
      let s1 = new ReactiveCollectionSelector<_, _>(toChangeObservable obs, (fun _ -> true), collectionSelector)
      let s2 = (ReactiveCollectionCollector(s1) :> IReactiveCollection<_>).ToNotificationChanges()
      let s3 = new ReactiveCollectionSelector<_, _>(toChangeObservable s, (fun _ -> true), resultSelector)
      s3
 
  let rxc = ReactiveCollectionBuilder()

  let sdfs = rxc {
    yield 1
    yield 2
    yield! rxc {
      if true = true then
        yield 3
    }
  }