module FSharpLint.Rules.MaxLinesInEnum

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInEnum = 500

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.TypeDefinition(SynTypeDefn.TypeDefn(_, repr, _, range)) ->
        match repr with
        | SynTypeDefnRepr.Simple(simpleRepr, _) ->
            match simpleRepr with
            | SynTypeDefnSimpleRepr.Enum(_) ->
                Helper.SourceLength.checkSourceLengthRule config range "Enum"
            | _ -> Array.empty
        | _ -> Array.empty
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInEnum"
      Identifier = Identifiers.MaxLinesInEnum
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInEnum config); Cleanup = ignore } }
    |> AstNodeRule