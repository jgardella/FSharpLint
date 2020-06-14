module FSharpLint.Rules.MaxLinesInMatchLambdaFunction

open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules
open FSharpLint.Rules.Helper.SourceLength

let [<Literal>] private DefaultMaxLinesInMatchLambda = 100

let private runner (config:Helper.SourceLength.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.Expression(SynExpr.MatchLambda(_, _, _, _, range)) ->
        Helper.SourceLength.checkSourceLengthRule config range "Match lambda function"
    | _ -> Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInMatchLambdaFunction"
      Identifier = Identifiers.MaxLinesInMatchLambdaFunction
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto DefaultMaxLinesInMatchLambda config); Cleanup = ignore } }
    |> AstNodeRule