﻿module Control.Monad.StateT

open Prelude
open Control.Monad
open Control.Monad.State
open Control.Monad.Trans

type StateT< ^s, ^m> = StateT of (^s -> ^m) with
    static member inline ( ? ) (StateT m, _Functor:Fmap                 ) = fun f -> StateT <| fun s -> do'{
        let! (x, s') = m s
        return (f x, s')}

    static member inline (?<-) (_:Return, _Monad  :Return, t:StateT<_,_>) = fun a -> StateT (fun s -> return' (a, s))
    static member inline (?<-) (StateT m, _Monad  :Bind  , t:StateT<_,_>) =
        let inline runStateT (StateT x) = x
        fun k -> StateT <| fun s -> do'{
            let! (a, s') = m s
            return! runStateT (k a) s'}

    static member inline (?<-) (_       , _MonadPlus:Mzero, t:StateT<_,_>) = StateT <| fun _ -> mzero()
    static member inline (?<-) (StateT m, _MonadPlus:Mplus, StateT n     ) = StateT <| fun s -> mplus (m s) (n s)

    static member inline (?<-) (m       , _MonadTrans:Lift, t:StateT<_,_>) = StateT <| fun s -> m >>= fun a -> return' (a,s)
    
    static member inline (?<-) (_ , _MonadState:Get, t:StateT<_,_> ) =          StateT (fun s -> return' (s , s))
    static member inline (?<-) (_ , _MonadState:Put, t:StateT<_,_> ) = fun x -> StateT (fun _ -> return' ((), x))

    static member inline (?<-) (x:IO<_>, _MonadIO:LiftIO, t:StateT<_,_>) = lift x

let inline mapStateT f (StateT m) = StateT (f << m)
let inline withContT f (StateT m) = StateT (m << f)
let inline runStateT (StateT x) = x