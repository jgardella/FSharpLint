module FSharpLint.Rules.TupleParentheses

open System
open FSharp.Compiler.Ast
open FSharpLint.Framework
open FSharpLint.Framework.Analyser
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper

let checkTupleHasParentheses args _ range parentNode =
    match parentNode with
    | Some (AstNode.Expression (SynExpr.Paren _)) ->
        Array.empty
    | _ ->
        ExpressionUtilities.tryFindTextOfRange range args.fileContent
        |> Option.map (fun text ->
            let suggestedFix = lazy(
                { FromRange = range; FromText = text; ToText = "(" + text + ")" }
                |> Some)
            { Range = range
              Message = Resources.GetString("RulesFormattingTupleParenthesesError")
              SuggestedFix = Some suggestedFix
              TypeChecks = [] })
        |> Option.toArray

let runner (args : AstNodeRuleParams) = TupleFormatting.isActualTuple args checkTupleHasParentheses
    
let rule =
    { name = "TupleParentheses" 
      identifier = None
      runner = runner }