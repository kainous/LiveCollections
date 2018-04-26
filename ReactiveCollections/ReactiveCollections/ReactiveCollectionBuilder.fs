module System.Reactive.Collections.ReactiveCollectionExtensions

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

let inline NotificationFunctor (func:'T -> CollectionChangeNotification<'T>) items =
  let stuff = items |> Seq.map func
  { new ICollectionChangeData<'T> with
      member __.GetEnumerator() = stuff.GetEnumerator()
    interface IEnumerable with
      member __.GetEnumerator() = upcast stuff.GetEnumerator() }

let inline Post this (context:SynchronizationContext) (method:Event<_,_>) args =
  context.Post((fun _ -> method.Trigger(this, args)), state = null)

type private ReactiveObservableCollection<'T>(items:IObservable<ICollectionChangeData<'T>>, context) as this =
  let _propertyChangeEvent = Event<_,_>()
  let _collectionChangeEvent = Event<_,_>()
  let _lock = AsyncReaderWriterLock()

  let _items = HashSet<_>()
  
  let OnNext changes =
    let rec splitChanges additions deletions = function
    | [] -> additions, deletions
    | head::tail ->      
      match head with
      | Insert item ->        
        let additions = if _items.Add item
                        then item::additions 
                        else additions
        splitChanges additions deletions tail
      | Remove item -> 
        let deletions = if _items.Remove item
                        then item::deletions
                        else deletions
        splitChanges additions deletions tail

    let (additions, deletions) = 
      use __ = _lock.WriterLock()
      changes |> List.ofSeq |> splitChanges [] []
    
    let add = NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,    List.toArray additions)
    let del = NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, List.toArray deletions)

    Post this context _collectionChangeEvent add
    Post this context _collectionChangeEvent del

    if add.NewItems.Count <> del.NewItems.Count then
      "Count" |> PropertyChangedEventArgs |> Post this context _propertyChangeEvent 
  
  let _subscription = items.Subscribe OnNext
  
  new items = new ReactiveObservableCollection<'T>(items, SynchronizationContext.Current)
  
  member __.Count =
    use __ = _lock.ReaderLock()
    _items.Count

  member __.GetEnumerator() =
    use __ = _lock.ReaderLock()
    _items.ToList().GetEnumerator()

  member __.Dispose() =
    _subscription.Dispose()

  interface IReadOnlyNotificationCollection<'T> with
    member this.Dispose() =
      this.Dispose()
    [<CLIEvent>]
    member __.PropertyChanged = _propertyChangeEvent.Publish
    [<CLIEvent>]
    member __.CollectionChanged = _collectionChangeEvent.Publish

  interface IReadOnlyCollection<'T> with
    member this.Count =
      this.Count
  
  interface IEnumerable<'T> with
    member this.GetEnumerator() =
      upcast this.GetEnumerator()
  
  interface IEnumerable with
    member this.GetEnumerator() =
      upcast this.GetEnumerator()

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