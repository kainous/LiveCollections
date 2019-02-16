// Learn more about F# at http://fsharp.org

open System
open LiveProperties
open Itc4net
open System.Reactive.Subjects

[<EntryPoint>]
let main argv =
  let stamp = Stamp()
  use obs = new Subject<string * int>()
  let property = LiveProperty(stamp, obs) :> IReadableLiveProperty<_,_,_>
  property.

  printfn "Hello World from F#!"
  0 // return an integer exit code
