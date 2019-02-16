namespace LiveProperties

open System
open System.Collections
open System.Linq







type ILivePropertyUpdateChunk<'TClock, 'TId, 'TValue when 'TClock : comparison and 'TId : equality > =
  inherit ILookup<'TClock * 'TId, 'TValue>

type IReadableLiveProperty<'TClock, 'TId, 'TValue when 'TClock : comparison and 'TId : equality > =
  abstract PropertyIdentifier : 'TId
  abstract CurrentSnapshot    : Option<'TClock * 'TValue>
  abstract FilteredObservable : unit -> IObservable<'TValue>
  abstract GetSource          : unit -> IObservable<seq<'TClock * 'TId * 'TValue>>

type IWriteableLiveProperty<'TClock, 'TId, 'TValue when 'TClock : comparison and 'TId : equality > =
  inherit IReadableLiveProperty<'TClock, 'TId, 'TValue>

type LiveProperty<'TClock, 'TId, 'TValue when 'TClock : comparison and 'TId : equality >(id, source:IObservable<'TId * 'TValue>) =
  interface IReadableLiveProperty<'TClock, 'TId, 'TValue> with
    member this.CurrentSnapshot: Option<'TClock * 'TValue> = 
      raise (System.NotImplementedException())
    member this.FilteredObservable(): IObservable<'TValue> = 
      raise (System.NotImplementedException())
    member this.GetSource(): IObservable<seq<'TClock * 'TId * 'TValue>> = 
      raise (System.NotImplementedException())
    member this.PropertyIdentifier: 'TId = 
      raise (System.NotImplementedException())