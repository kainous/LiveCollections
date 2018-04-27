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