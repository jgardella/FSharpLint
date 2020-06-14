﻿module FSharpLint.Rules.TypedItemSpacing

open System
open FSharp.Compiler.Range
open FSharp.Compiler.SyntaxTree
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules

type TypedItemStyle =
    | NoSpaces = 0
    | SpaceAfter = 1
    | SpacesAround = 2

[<RequireQualifiedAccess>]
type ConfigDto = { TypedItemStyle:TypedItemStyle option }

type private Config = { TypedItemStyle:TypedItemStyle }
with
    static member Default = {
        TypedItemStyle = TypedItemStyle.SpacesAround
    }

let private configOfDto (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { TypedItemStyle = dto.TypedItemStyle |> Option.defaultValue Config.Default.TypedItemStyle })
    |> Option.defaultValue Config.Default

let private getLeadingSpaces (s:string) =
    let rec loop i =
        if i < s.Length && s.[i] = ' '
        then loop (i + 1)
        else i

    loop 0

let private getTrailingSpaces (s:string) =
    let rec loop i count =
        if i >= 0 && s.[i] = ' '
        then loop (i - 1) (count + 1)
        else count

    (loop (s.Length - 1) 0)

let private expectedSpacesFromConfig (typedItemStyle:TypedItemStyle) =
    match typedItemStyle with
    | TypedItemStyle.NoSpaces -> (0, 0)
    | TypedItemStyle.SpaceAfter -> (0, 1)
    | TypedItemStyle.SpacesAround -> (1, 1)
    | _ -> (0, 0)

/// Checks the provided range, containing a typed item, has valid spacing.
let private checkRange (config:Config) (args:AstNodeRuleParams) (range:range) =
    let (expectedSpacesBefore, expectedSpacesAfter) =
        expectedSpacesFromConfig config.TypedItemStyle

    ExpressionUtilities.tryFindTextOfRange range args.FileContent
    |> Option.bind (fun text ->
        match text.Split(':') with
        | [|otherText; typeText|] ->
            let spacesBeforeColon = getTrailingSpaces otherText
            let spacesAfterColon = getLeadingSpaces typeText
            if spacesBeforeColon <> expectedSpacesBefore || spacesAfterColon <> expectedSpacesAfter then
                let trimmedOtherText = otherText.TrimEnd(' ')
                let trimmedTypeText = typeText.TrimStart(' ')
                let spacesBeforeString = " " |> String.replicate expectedSpacesBefore
                let spacesAfterString = " " |> String.replicate expectedSpacesAfter
                let suggestedFix = lazy(
                    { FromRange = range; FromText = text; ToText = trimmedOtherText + spacesBeforeString + ":" + spacesAfterString + trimmedTypeText }
                    |> Some)
                let errorFormatString = Resources.GetString("RulesFormattingTypedItemSpacingError")
                Some
                    { Range = range
                      Message = String.Format(errorFormatString, expectedSpacesBefore, expectedSpacesAfter)
                      SuggestedFix = Some suggestedFix
                      TypeChecks = [] }
                else
                    None
        | _ -> None)

/// Checks for correct spacing around colon of a typed item.
let private runner (config:Config) (args:AstNodeRuleParams) =
    match args.AstNode with
    | AstNode.Pattern (SynPat.Typed (range=range))
    | AstNode.Field (SynField.Field (range=range)) ->
        // NOTE: This currently does not work for fields in union cases, since the range on union case fields is incorrect,
        // only including the type and not the field name. (https://github.com/dotnet/fsharp/issues/9279)
        checkRange config args range |> Option.toArray
    | _ -> [||]

let rule (config:ConfigDto option) =
    { Name = "TypedItemSpacing"
      Identifier = Identifiers.TypedItemSpacing
      RuleConfig = { AstNodeRuleConfig.Runner = runner (configOfDto config); Cleanup = ignore } }
    |> AstNodeRule