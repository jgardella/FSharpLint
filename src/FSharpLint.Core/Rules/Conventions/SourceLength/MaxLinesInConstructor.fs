module FSharpLint.Rules.MaxLinesInConstructor

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.AstInfo
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInConstructor = 100

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.Binding(SynBinding.Binding(_, _, _, _, _, _, valData, _, _, _, _, _) as binding) ->
        match identifierTypeFromValData valData with
        | Constructor -> Helper.SourceLength.checkSourceLengthRule config binding.RangeOfBindingAndRhs "Constructor"
        | _ -> Array.empty
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInConstructor"
      Identifier = Identifiers.MaxLinesInConstructor
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInConstructor config); Cleanup = ignore } }
    |> AstNodeRule