namespace System.Reactive.Collections

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open System.Reactive
open System.Reactive.Collections
open System.Reactive.Linq
open System.Reactive.Subjects
open Nito.AsyncEx
open System.Threading.Tasks
open System.Linq
open System.Threading
open System.ComponentModel

open Helpers

type ReactiveCollectionSource<'T>(items:seq<'T>, comparer:IEqualityComparer<'T>) =
  let _lock  = AsyncReaderWriterLock()
  let _items = HashSet<'T>(items, comparer)
  let _subject = new Subject<ICollectionChangeData<'T>>()

  new items = new ReactiveCollectionSource<'T>(items, EqualityComparer<'T>.Default)

  member this.Add items =
    let changes =
      use __ = _lock.WriterLock()
      [ for item in items do
          if _items.Add item
            then yield item ]
    
    changes |> NotificationFunctor Insert |> _subject.OnNext

  member this.Remove items =
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
    member this.ToList() =
      use __ = _lock.ReaderLock()
      upcast items.ToList()

    member this.ToNotificationChanges() =
      { new ICollectionChangeObservable<'T> with
        member this.Subscribe obs = 
          let disp = _subject.Subscribe obs
          let changes =
            use __ = _lock.ReaderLock()
            _items |> Seq.toList
          obs.OnNext(NotificationFunctor Insert changes)
          disp }

    member this.ToReadOnlyNotificationCollection context =
      upcast new ReactiveObservableCollection<'T>(_subject, context)

//type ReactiveCollectionBuilder() =
//  member this.Yield x =
//    new ReactiveCollectionSource<_>([x])
//  member this.YieldFrom (x:IReactiveCollection<'T>) = 
//    x
//  member this.
 
//let rxc = ReactiveCollectionBuilder()

//let sdfs = rxc {
//  yield 1
//  yield 2
//}