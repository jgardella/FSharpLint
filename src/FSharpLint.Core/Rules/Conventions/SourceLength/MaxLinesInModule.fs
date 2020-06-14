module FSharpLint.Rules.MaxLinesInModule

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInModule = 1000

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.ModuleOrNamespace(SynModuleOrNamespace.SynModuleOrNamespace(_, _, (NamedModule | AnonModule), _, _, _, _, range)) ->
        Helper.SourceLength.checkSourceLengthRule config range "Module"
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInModule"
      Identifier = Identifiers.MaxLinesInModule
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInModule config); Cleanup = ignore } }
    |> AstNodeRule