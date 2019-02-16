namespace Consus

open System.Linq
open System.Runtime.CompilerServices

module Prelude =
  let inline Const k _ = k

open Prelude
open System.Collections.Generic

type IMultiValueDictionary<'TKey, 'TValue> =
  inherit ILookup<'TKey, 'TValue>
  abstract Add : items:ILookup<'TKey, 'TValue> -> unit
  abstract Clear : unit -> unit

[<Extension>]
type MultiValueDictionary() =
  [<Extension>]
  static member Add (dictionary:IMultiValueDictionary<_, _>, key:'TKey, values:seq<'TValue>) =
    dictionary.Add(values.ToLookup(Const key, id))

  [<Extension>]
  static member Add (dictionary:IMultiValueDictionary<_, _>, items:seq<'TKey * 'TValue>) =
    dictionary.Add(items.ToLookup(fst, snd))

type IKeyedCollection<'TKey, 'TValue> =
  inherit ICollection<'TValue>
  abstract Key : 'TKey

type MultiValueDictionary<'TKey, 'TValue>(?items, ?collectionConstructor : IEnumerable<'TValue> -> ICollection<'TValue>, ?dictionary) =  
  let collectionConstructor = 
    defaultArg collectionConstructor <| fun a -> upcast List<'TValue> a
  
  let createKeyedCollection key items =
    let collection = collectionConstructor items
    { new IKeyedCollection<'TKey, 'TValue> with
        member this.Key = key
        member this.Add(item: 'TValue): unit = collection.Add item
        member this.Clear(): unit = collection.Clear()
        member this.Contains(item: 'TValue): bool = collection.Contains item
        member this.CopyTo(array: 'TValue [], arrayIndex: int): unit = collection.CopyTo(array, arrayIndex)
        member this.Count: int = collection.Count
        member this.GetEnumerator(): IEnumerator<'TValue> = collection.GetEnumerator()
        member this.GetEnumerator(): System.Collections.IEnumerator = upcast collection.GetEnumerator()
        member this.IsReadOnly: bool = collection.IsReadOnly
        member this.Remove(item: 'TValue): bool = collection.Remove item }

  let items = 
    let items = Unchecked.defaultof<'TKey> |> Const |> Enumerable.Empty().ToLookup |> defaultArg items
    let dict = defaultArg dictionary <| Dictionary<'TKey, IKeyedCollection<'TKey, 'TValue>> EqualityComparer.Default :> IDictionary<_,_>
    items |> Seq.iter (fun item -> dict.Add(item.Key, createKeyedCollection item.Key item))
    dict

  member this.Keys = items.Keys
  member this.Clear() = items.Clear()
  member this.KeyCount = items.Count
  member this.ContainsKey key = items.ContainsKey key
  member this.GetEnumerator() = items |> Seq.groupBy()

  interface ILookup<'TKey, 'TValue> with
    member this.Contains(key: 'TKey): bool = 
      raise <| System.NotImplementedException()
    member this.Count: int = 
      this.KeyCount
    member this.GetEnumerator(): IEnumerator<IGrouping<'TKey,'TValue>> = 
      raise <| System.NotImplementedException()
    member this.GetEnumerator(): System.Collections.IEnumerator = 
      raise <| System.NotImplementedException()
    member this.Item
      with get (key: 'TKey): IEnumerable<'TValue> = 
        raise <| System.NotImplementedException()