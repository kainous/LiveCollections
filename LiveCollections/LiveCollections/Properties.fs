namespace System

open System.Collections.LiveCollections
open System.ComponentModel
open System.Reactive.Subjects


type IProperty<'T> =
  inherit IObservableHost<'T>
  inherit IDisposable
  inherit INotifyPropertyChanged
  abstract Value : 'T with get

type private FilterByChangesProperty<'T when 'T : equality>(source : IProperty<'T>) as this =
  let mutable cache = None
  let event = Event<_,_>()
  let subject = new Subject<'T>()


  let onNext next =
    let send item =
      subject.OnNext item
      cache <- Some item
      event.Trigger(this, PropertyChangedEventArgs "Value")

    match cache with
    | None -> 
      send next

    | Some previous when previous <> next ->
      send next

    | _ -> 
      ()

  let subscription =
    source.GetObservable().Subscribe(onNext, fun () -> ())
  
  interface IProperty<'T> with
    member __.GetObservable() =
      subject :> IObservable<'T>
    member __.Dispose() =
      subscription.Dispose()
      subject.Dispose() 
    member __.Value = 
      cache.Value
    [<CLIEvent>]
    member __.PropertyChanged = 
      event.Publish

type PreviousValueProperty<'T>(source:IProperty<'T>) as this =
    let mutable cache = None : Option<'T>
    let mutable valueCache = None : Option<'T * 'T>
    let subject = new Subject<'T * 'T>()
    let event = Event<_,_>()
    let onNext (next:'T) =
      match cache with
      | None ->
        cache <- Some next

      | Some previous ->
        subject.OnNext(previous, next)
        valueCache <- Some (previous, next)
        cache <- Some next
        event.Trigger(this, PropertyChangedEventArgs "Value")

    let subscription = source.GetObservable().Subscribe(onNext, fun () -> ())

    interface IProperty<'T * 'T> with 
      member this.GetObservable() =
        subject :> IObservable<'T * 'T>
      member this.Dispose() =
        subscription.Dispose()
        subject.Dispose()
      member this.Value = 
        valueCache.Value
      [<CLIEvent>]
      member this.PropertyChanged =
        event.Publish