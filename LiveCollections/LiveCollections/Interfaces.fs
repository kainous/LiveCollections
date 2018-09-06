namespace System.Collections.LiveCollections

open System
open System.Reactive.Subjects
open System.Reactive.Linq

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

type IProperty<'T> =
  inherit IObservableHost<'T>
  inherit IDisposable

type ILiveCollection<'TKey, 'TItem> =
  inherit IObservableHost<SourceUpdate<KeyedCollectionNotification<'TKey, 'TItem>>>

[<AutoOpen>]
module LiveCollectionModule =
  let keepChanges (source : IProperty<'T>) =
    let mutable cache = None
    let subject = new Subject<'T * 'T>()
    let onNext next =
      match cache with
      | None ->
        cache <- Some next

      | Some previous ->
        subject.OnNext(previous, next)
        cache <- Some next

    let subscription = source.GetObservable().Subscribe(onNext, fun () -> ())

    { new IProperty<'T * 'T> with 
        member this.GetObservable() =
          subject :> IObservable<'T * 'T>
        member this.Dispose() =
          subscription.Dispose()
          subject.Dispose() }

  let onChanges (source : IProperty<'T>) =
    let mutable cache = None
    let subject = new Subject<'T>()
    let onNext next =
      match cache with
      | None ->
        subject.OnNext next
        cache <- Some next

      | Some previous when previous <> next ->
        subject.OnNext next
        cache <- Some next

      | _ -> 
        ()

    let subscription = source.GetObservable().Subscribe(onNext, fun () -> ())

    { new IProperty<'T> with 
        member this.GetObservable() =
          subject :> IObservable<'T>
        member this.Dispose() =
          subscription.Dispose()
          subject.Dispose() }


  let map (mapping:'TSource -> 'TResult) (source:ILiveCollection<'TKey, 'TSource>) : ILiveCollection<'TKey, 'TResult> =
    failwith "Not yet implemented"

  let reduce (aggregator : 'TSource -> 'TSource -> 'TSource) (source:ILiveCollection<'TKey, 'TSource>) =
    failwith "Not yet implemented"