// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace Farkle.Grammar.EgtReader

open Chessie.ErrorHandling
open Farkle
open Farkle.Grammar
open Farkle.Monads
open System
open System.Text

type internal Entry =
    | Empty
    | Byte of byte
    | Boolean of bool
    | UInt16 of uint16
    | String of string

module internal LowLevel =

    open StateResult

    let takeByte: StateResult<byte, _, _> = List.takeOne() |> mapFailure ListError

    let takeBytes count: StateResult<byte list, _, _> = count |> List.takeM |> mapFailure ListError

    let ensureLittleEndian x =
        if BitConverter.IsLittleEndian then
            x
        else
            ((x &&& 0xffus) <<< 8) ||| ((x >>> 8) &&& 0xffus)

    let takeUInt16 = sresult {
        let! bytes = takeBytes 2 <!> Array.ofList
        return BitConverter.ToUInt16(bytes, 0) |> ensureLittleEndian
    }

    let takeString = sresult {
        let! len =
            get
            <!> Seq.pairs
            <!> Seq.tryFindIndex (fun (x, y) -> x = 0uy && y = 0uy)
            <!> failIfNone UnterminatedString
            >>= liftResult
            <!> ((*) 2)
        let! result = takeBytes len <!> Array.ofSeq <!> Encoding.Unicode.GetString
        let! terminator = takeUInt16
        if terminator = 0us then
            return result
        else
            return! fail TakeStringBug
    }

    let readEntry = sresult {
        let! entryCode = takeByte
        match entryCode with
        | 'E'B -> return Empty
        | 'b'B -> return! takeByte <!> Byte
        | 'B'B ->
            let! value = takeByte
            match value with
            | 0uy -> return Boolean false
            | 1uy -> return Boolean true
            | x -> return! x |> InvalidBoolValue |> fail
        | 'I'B -> return! takeUInt16 <!> UInt16
        | 'S'B -> return! takeString <!> String
        | x -> return! x |> InvalidEntryCode |> fail
    }
