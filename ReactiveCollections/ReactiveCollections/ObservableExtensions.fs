module System.Reactive.ObservableExtensions

open System.Reactive.Linq

type ObservableBuilder() =
  member this.Zero() =
    Observable.Empty()
  member this.Yield x =
    Observable.Return x
  member this.YieldFrom x = 
    x
  member this.Combine(a, b) =
    Observable.Merge([a; b])
  member this.Delay f = f()
 
let obs = ObservableBuilder()

