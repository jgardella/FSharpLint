module FSharpLint.Rules.MaxLinesInRecord

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInRecord = 500

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.TypeDefinition(SynTypeDefn.TypeDefn(_, repr, _, range)) ->
        match repr with
        | SynTypeDefnRepr.Simple(simpleRepr, _) ->
            match simpleRepr with
            | SynTypeDefnSimpleRepr.Record(_) ->
                Helper.SourceLength.checkSourceLengthRule config range "Record"
            | _ -> Array.empty
        | _ -> Array.empty
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInRecord"
      Identifier = Identifiers.MaxLinesInRecord
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInRecord config); Cleanup = ignore } }
    |> AstNodeRule