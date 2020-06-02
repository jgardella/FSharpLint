module FSharpLint.Rules.MaxNumberOfMembers

open System
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules

let private getMembers (members:SynMemberDefn list) =
    let isPublic = function
        | Some(SynAccess.Public) | None -> true
        | Some(_) -> false

    let isPublicMember = function
        | SynMemberDefn.AbstractSlot(_) -> true
        | SynMemberDefn.Member(SynBinding.Binding(access, _, _, _, _, _, _, _, _, _, _, _), _)
        | SynMemberDefn.AutoProperty(_, _, _, _, _, _, _, access, _, _, _) -> isPublic access
        | _ -> false

    members
    |> List.filter isPublicMember

let private validateType (maxMembers:int) members typeRepresentation =
    let members =
        match typeRepresentation with
        | SynTypeDefnRepr.Simple(_) | SynTypeDefnRepr.Exception(_) -> members
        | SynTypeDefnRepr.ObjectModel(_, members, _) -> members
        |> getMembers

    if List.length members > maxMembers then
        {
            Range = members.[maxMembers].Range
            Message = Resources.Format("RulesNumberOfItemsClassMembersError", maxMembers)
            SuggestedFix = None
            TypeChecks = []
        } |> Array.singleton
    else
        Array.empty

let private runner (config:Helper.NumberOfItems.Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.TypeDefinition(SynTypeDefn.TypeDefn(_, typeRepresentation, members, _)) ->
        validateType config.MaxItems members typeRepresentation
    | _ -> Array.empty

let rule config =
    { Name = "MaxNumberOfMembers"
      Identifier = Identifiers.MaxNumberOfMembers
      RuleConfig = { AstNodeRuleConfig.Runner = runner config; Cleanup = ignore } }
    |> AstNodeRule
