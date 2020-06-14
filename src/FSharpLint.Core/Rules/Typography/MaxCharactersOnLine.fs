module FSharpLint.Rules.MaxCharactersOnLine

open System
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharpLint.Framework.Rules
open FSharp.Compiler.Range

[<RequireQualifiedAccess>]
type ConfigDto = { MaxCharactersOnLine:int option }

type private Config = { MaxCharactersOnLine:int }
with
    static member Default = {
        MaxCharactersOnLine = 120
    }

let private configOfDto (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { Config.MaxCharactersOnLine = dto.MaxCharactersOnLine |> Option.defaultValue Config.Default.MaxCharactersOnLine })
    |> Option.defaultValue Config.Default

let private checkMaxCharactersOnLine (config:Config) (args:LineRuleParams) =
    let maxCharacters = config.MaxCharactersOnLine
    let lineLength = String.length args.Line
    if lineLength > maxCharacters then
        let range = mkRange "" (mkPos args.LineNumber (maxCharacters + 1)) (mkPos args.LineNumber lineLength)
        let errorFormatString = Resources.GetString("RulesTypographyLineLengthError")
        { Range = range
          Message = String.Format(errorFormatString, (maxCharacters + 1))
          SuggestedFix = None
          TypeChecks = [] } |> Array.singleton
    else
        Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxCharactersOnLine"
      Identifier = Identifiers.MaxCharactersOnLine
      RuleConfig = { LineRuleConfig.Runner = checkMaxCharactersOnLine (configOfDto config) } }
    |> LineRule