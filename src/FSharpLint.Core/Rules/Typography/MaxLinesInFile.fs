module FSharpLint.Rules.MaxLinesInFile

open System
open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharpLint.Framework.Rules
open FSharp.Compiler.Range

[<RequireQualifiedAccess>]
type ConfigDto = { MaxLinesInFile:int option }

type private Config = { MaxLinesInFile:int }
with
    static member Default = {
        MaxLinesInFile = 1000
    }

let private configOfDto (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { MaxLinesInFile = dto.MaxLinesInFile |> Option.defaultValue Config.Default.MaxLinesInFile })
    |> Option.defaultValue Config.Default

let private checkNumberOfLinesInFile numberOfLines line maxLines =
    if numberOfLines > maxLines then
        let errorFormatString = Resources.GetString("RulesTypographyFileLengthError")
        { Range = mkRange "" (mkPos (maxLines + 1) 0) (mkPos numberOfLines (String.length line))
          Message = String.Format(errorFormatString, (maxLines + 1))
          SuggestedFix = None
          TypeChecks = [] } |> Array.singleton
    else
        Array.empty

let private checkMaxLinesInFile (config:Config) (args:LineRuleParams) =
    if args.IsLastLine then
        checkNumberOfLinesInFile args.LineNumber args.Line config.MaxLinesInFile
    else
        Array.empty

let rule (config:ConfigDto option) =
    { Name = "MaxLinesInFile"
      Identifier = Identifiers.MaxLinesInFile
      RuleConfig = { LineRuleConfig.Runner = checkMaxLinesInFile (configOfDto config) } }
    |> LineRule