module FSharpLint.Rules.MaxLinesInLambdaFunction

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInLambda = 7

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.Expression(SynExpr.Lambda(_, _, _, _, range)) ->
        Helper.SourceLength.checkSourceLengthRule config range "Lambda function"
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInLambdaFunction"
      Identifier = Identifiers.MaxLinesInLambdaFunction
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInLambda config); Cleanup = ignore } }
    |> AstNodeRule