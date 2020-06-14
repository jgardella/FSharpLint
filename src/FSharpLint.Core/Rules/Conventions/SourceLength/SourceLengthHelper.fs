module FSharpLint.Rules.Helper.SourceLength

open System
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharp.Compiler.Range

[<RequireQualifiedAccess>]
type ConfigDto = { MaxLines:int option }

type internal Config = { MaxLines:int }

let private error name i actual =
    let errorFormatString = Resources.GetString("RulesSourceLengthError")
    String.Format(errorFormatString, name, i, actual)

let private length (range:range) = range.EndLine - range.StartLine

let internal configOfDto (defaultValue:int) (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { Config.MaxLines = dto.MaxLines |> Option.defaultValue defaultValue })
    |> Option.defaultValue { Config.MaxLines = defaultValue }

let internal checkSourceLengthRule (config:Config) range errorName =
    let actualLines = length range
    if actualLines > config.MaxLines then
        { Range = range
          Message = error errorName config.MaxLines actualLines
          SuggestedFix = None
          TypeChecks = [] } |> Array.singleton

    else
        Array.empty
