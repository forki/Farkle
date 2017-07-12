// Copyright (c) 2017 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace Farkle.Grammar

open Chessie.ErrorHandling
open Farkle

type Indexed<'a> = Indexed<'a, uint16>

type TableCounts =
    {
        SymbolTables: uint16
        CharSetTables: uint16
        ProductionTables: uint16
        DFATables: uint16
        LALRTables: uint16
        GroupTables: uint16
    }

type GrammarError =
    | EGTReadError of EGTReadError
    | InvalidSymbolType of uint16
    | InvalidAdvanceMode of uint16
    | InvalidEndingMode of uint16
    | InvalidLALRActionType of uint16
    | InvalidTableCounts of expected: TableCounts * actual: TableCounts
    | IndexOutOfRange of uint16

type Properties = Properties of Map<string, string>

type CharSet = RangeSet<char>

type SymbolType =
    | Nonterminal
    | Terminal
    | Noise
    | EndOfFile
    | GroupStart
    | GroundEnd
    // 6 is deprecated
    | Error

module SymbolType =
    let ofUInt16 =
        function
        | 0us -> ok Nonterminal
        | 1us -> ok Terminal
        | 2us -> ok Noise
        | 3us -> ok EndOfFile
        | 4us -> ok GroupStart
        | 5us -> ok GroundEnd
        // 6 is deprecated
        | 7us -> ok Error
        | x -> x |> InvalidSymbolType |> fail

type Symbol =
    {
        Name: string
        Kind: SymbolType
    }
type AdvanceMode =
    | Token
    | Character

module AdvanceMode =
    let create =
        function
        | 0us -> ok Token
        | 1us -> ok Character
        | x -> x |> InvalidAdvanceMode |> fail

type EndingMode =
    | Open
    | Closed

module EndingMode =
    let create =
        function
        | 0us -> ok Open
        | 1us -> ok Closed
        | x -> x |> InvalidEndingMode |> fail

type Group =
    {
        Name: string
        ContainerSymbol: Indexed<Symbol>
        StartSymbol: Indexed<Symbol>
        EndSymbol: Indexed<Symbol>
        AdvanceMode: AdvanceMode
        EndingMode: EndingMode
        Nesting: Set<Indexed<Group>>
    }

type Production =
    {
        Nonterminal: Indexed<Symbol>
        Symbols: Indexed<Symbol> list
    }

type InitialStates =
    {
        DFA: uint16
        LALR: uint16
    }

type DFAState =
    {
        AcceptState: Indexed<Symbol> option
        Edges: Set<Indexed<CharSet> * Indexed<DFAState>>
    }

type LALRState =
    {
        Actions: Map<Indexed<Symbol>, LALRActionType>
    }

and LALRActionType =
    | Shift of Indexed<LALRState>
    | Reduce of Indexed<Production>
    | Goto of Indexed<LALRState>
    | Accept

module LALRActionType =
    let create index =
        function
        | 1us -> index |> Indexed |> Shift |> ok
        | 2us -> index |> Indexed |> Reduce |> ok
        | 3us -> index |> Indexed |> Goto |> ok
        | 4us -> Accept |> ok
        | x -> x |> InvalidLALRActionType |> fail

type Grammar =
    private
        {
            _Properties: Properties
            _CharSets: CharSet list
            _Symbols: Symbol list
            _Groups: Group list
            _Productions: Production list
            _LALRStates: LALRState list
            _DFAStates: DFAState list
        }
    with
        member x.Properties = x._Properties
        member x.CharSets = x._CharSets
        member x.Symbols = x._Symbols
        member x.Groups = x._Groups
        member x.Productions = x._Productions
        member x.LALRStates = x._LALRStates
        member x.DFAStates = x._DFAStates

module Grammar =
    let counts (x: Grammar) =
        {
            SymbolTables = x.Symbols.Length |> uint16
            CharSetTables = x.CharSets.Length |> uint16
            ProductionTables = x.Productions.Length |> uint16
            DFATables = x.DFAStates.Length |> uint16
            LALRTables = x.LALRStates.Length |> uint16
            GroupTables = x.Groups.Length |> uint16
        }
    
    let create properties symbols charSets prods dfas lalrs groups _counts =
        let g =
            {
                _Properties = properties
                _Symbols = symbols
                _CharSets = charSets
                _Productions = prods
                _DFAStates = dfas
                _LALRStates = lalrs
                _Groups = groups
            }
        let counts = counts g
        if counts = _counts then
            ok g
        else
            (_counts, counts) |> InvalidTableCounts |> fail