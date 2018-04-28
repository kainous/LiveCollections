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

module Helpers =
  let inline NotificationFunctor (func:'T -> CollectionChangeNotification<'T>) items =
    let stuff = items |> Seq.map func
    { new ICollectionChangeData<'T> with
        member __.GetEnumerator() = stuff.GetEnumerator()
      interface IEnumerable with
        member __.GetEnumerator() = upcast stuff.GetEnumerator() }

  let inline Post this (context:SynchronizationContext) (method:Event<_,_>) args =
    context.Post((fun _ -> method.Trigger(this, args)), state = null)

open Helpers

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

type ReactiveCollectionSelector<'TSource,'TResult>(changes:IObservable<IEnumerable<CollectionChangeNotification<'TSource>>>, filter:'TSource -> bool, selector:'TSource -> 'TResult, comparer:IEqualityComparer<'TSource>) as this =
  let _items = Dictionary<'TSource,'TResult>(comparer)
  let _lock = AsyncReaderWriterLock()
  let _subject = new Subject<ICollectionChangeData<'TResult>>()

  let OnNext changes =
    let results = 
      use __ = _lock.WriterLock()
      seq { for item in changes do
              match item with
              | Insert x ->
                if filter x then
                  let y = selector x
                  _items.[x] <- y
                  yield Insert y
              | Remove x ->
                match _items.TryGetValue x with
                | false, _ -> ()
                | true, y ->
                  yield Remove y }
    { new ICollectionChangeData<_> with
        member __.GetEnumerator() =
          upcast results.GetEnumerator()
      interface IEnumerable with
        member __.GetEnumerator() =
          upcast results.GetEnumerator() } |> _subject.OnNext

  let _subscription = changes.Subscribe(OnNext, this.Dispose)

  new(changes, filter, selector) = new ReactiveCollectionSelector<'TSource, 'TResult>(changes, filter, selector, EqualityComparer<'TSource>.Default)

  member __.Dispose() =
    _subscription.Dispose()

  interface IReactiveCollection<'TResult> with
    member __.ToList() =
      use __ = _lock.ReaderLock()
      upcast _items.Values.ToList()
    member this.Dispose() =
      this.Dispose()
    member __.ToNotificationChanges() =
      { new ICollectionChangeObservable<_> with
        member __.Subscribe obs = 
          let disp = _subject.Subscribe obs
          let changes =
            use __ = _lock.ReaderLock()
            _items.Values.ToList()
          obs.OnNext(NotificationFunctor Insert changes)
          disp }
    member __.ToReadOnlyNotificationCollection context =      
      upcast new ReactiveObservableCollection<_>(_subject, context)