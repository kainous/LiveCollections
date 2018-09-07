namespace System.Collections.LiveCollections

open System
open System.Reactive.Subjects

[<AutoOpen>]
module LiveCollectionModule =

  let keepChanges (source : IProperty<'T>) =
    let mutable cache = None
    let subject = new Subject<'T * 'T>()
    let event = Event<_,_>()
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
          subject.Dispose()
        member this.Value = cache.Value
        [<CLIEvent>]
        member this.PropertyChanged = event.Publish }

  let onChanges source =
    new FilterByChangesProperty<_>(source) :> IProperty<'T>

  let map (mapping:'TSource -> 'TResult) (source:ILiveCollection<'TKey, 'TSource>) : ILiveCollection<'TKey, 'TResult> =
    failwith "Not yet implemented"

  let reduce (aggregator : 'TSource -> 'TSource -> 'TSource) (source:ILiveCollection<'TKey, 'TSource>) =
    failwith "Not yet implemented"