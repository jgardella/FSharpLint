module FSharpLint.Rules.TrailingWhitespaceOnLine

open System
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharpLint.Framework.Rules
open FSharp.Compiler.Range

[<RequireQualifiedAccess>]
type ConfigDto =
    { NumberOfSpacesAllowed:int option
      OneSpaceAllowedAfterOperator:bool option
      IgnoreBlankLines:bool option }

type private Config =
    { NumberOfSpacesAllowed:int
      OneSpaceAllowedAfterOperator:bool
      IgnoreBlankLines:bool }
with
    static member Default = {
       NumberOfSpacesAllowed = 1
       OneSpaceAllowedAfterOperator = true
       IgnoreBlankLines = true
    }

let private configOfDto (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { Config.NumberOfSpacesAllowed = dto.NumberOfSpacesAllowed |> Option.defaultValue Config.Default.NumberOfSpacesAllowed
          OneSpaceAllowedAfterOperator = dto.OneSpaceAllowedAfterOperator |> Option.defaultValue Config.Default.OneSpaceAllowedAfterOperator
          IgnoreBlankLines = dto.OneSpaceAllowedAfterOperator |> Option.defaultValue Config.Default.IgnoreBlankLines })
    |> Option.defaultValue Config.Default

let private isSymbol character =
    let symbols =
        [ '>';'<';'+';'-';'*';'=';'~';'%';'&';'|';'@'
          '#';'^';'!';'?';'/';'.';':';',';'(';')';'[';']';'{';'}' ]

    symbols |> List.exists ((=) character)

let private doesStringNotEndWithWhitespace (config:Config) (str:string) =
    match (config.NumberOfSpacesAllowed, config.OneSpaceAllowedAfterOperator) with
    | (numberOfSpacesAllowed, _) when numberOfSpacesAllowed > 0 ->
        str.Length - str.TrimEnd().Length <= numberOfSpacesAllowed
    | (_, isOneSpaceAllowedAfterOperator) when isOneSpaceAllowedAfterOperator ->
        let trimmedStr = str.TrimEnd()

        (trimmedStr.Length = str.Length)
        || (str.Length - trimmedStr.Length = 1
            && trimmedStr.Length > 0
            && isSymbol trimmedStr.[trimmedStr.Length - 1])
    | _ ->
        str.TrimEnd().Length = str.Length

let private lengthOfWhitespaceOnEnd (str:string) = str.Length - str.TrimEnd().Length

let private checkTrailingWhitespaceOnLine (config:Config) (args:LineRuleParams) =
    let line = args.Line
    let lineNumber = args.LineNumber
    let ignoringBlankLinesAndIsBlankLine = config.IgnoreBlankLines && System.String.IsNullOrWhiteSpace(line)

    let stringEndsWithWhitespace =
        not ignoringBlankLinesAndIsBlankLine &&
        not <| doesStringNotEndWithWhitespace config line

    if stringEndsWithWhitespace then
        let whitespaceLength = lengthOfWhitespaceOnEnd line
        let range = mkRange "" (mkPos lineNumber (line.Length - whitespaceLength)) (mkPos lineNumber line.Length)
        { Range = range
          Message = Resources.GetString("RulesTypographyTrailingWhitespaceError")
          SuggestedFix = None
          TypeChecks = [] } |> Array.singleton
    else
        Array.empty

let rule (config:ConfigDto option) =
    { Name = "TrailingWhitespaceOnLine"
      Identifier = Identifiers.TrailingWhitespaceOnLine
      RuleConfig = { LineRuleConfig.Runner = checkTrailingWhitespaceOnLine (configOfDto config) } }
    |> LineRule