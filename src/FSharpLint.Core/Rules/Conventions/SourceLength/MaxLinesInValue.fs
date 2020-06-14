module FSharpLint.Rules.MaxLinesInValue

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.AstInfo
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInValue = 100

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.Binding(SynBinding.Binding(_, _, _, _, _, _, valData, _, _, _, _, _) as binding) ->
        match identifierTypeFromValData valData with
        | Value -> Helper.SourceLength.checkSourceLengthRule config binding.RangeOfBindingAndRhs "Value"
        | _ -> Array.empty
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInValue"
      Identifier = Identifiers.MaxLinesInValue
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInValue config); Cleanup = ignore } }
    |> AstNodeRule