module FSharpLint.Rules.Helper.NumberOfItems

[<RequireQualifiedAccess>]
type ConfigDto = { MaxItems:int option }

type internal Config = { MaxItems:int }

let internal configOfDto (defaultValue:int) (dto:ConfigDto option) =
    dto
    |> Option.map (fun dto ->
        { MaxItems = dto.MaxItems |> Option.defaultValue defaultValue })
    |> Option.defaultValue { MaxItems = defaultValue }