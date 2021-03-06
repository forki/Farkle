// Copyright (c) 2017 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

module Farkle.Tests.ParserTests

open Expecto
open Expecto.Logging
open Farkle.Parser
open System

let logger = Log.create "Parser tests"

[<Tests>]
let tests =
    testList "Parser tests" [
        test "A simple mathematical expression can be parsed" {
            let parser = GOLDParser "simple.egt"
            let result = parser.ParseString "111*555"
            result.MessagesAsString |> String.concat Environment.NewLine |> Message.eventX |> logger.info
            match result.Simple with
            | Choice1Of2 x -> x |> Reduction.drawReductionTree |> sprintf "Result: %s" |> Message.eventX |> logger.info
            | Choice2Of2 messages -> messages |> failtestf "Error: %s"
        }
    ]
