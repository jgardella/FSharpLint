/// Adapted from https://github.com/fable-compiler/Fable/blob/68ecdee99875c121d3facecf0449f12512b5f9b8/src/dotnet/Fable.Compiler/CLI/ProjectCoreCracker.fs
/// ... which was adapted from https://github.com/fsharp/FsAutoComplete/blob/45bf4a7255f8856b0164f722a82a17108ae64981/src/FsAutoComplete.Core/ProjectCoreCracker.fs
module FSharpLint.ProjectCoreCracker

open System
open System.IO

open Microsoft.FSharp.Compiler.SourceCodeServices

module MSBuildPrj = Dotnet.ProjInfo.Inspect

type NavigateProjectSM =
    | NoCrossTargeting of NoCrossTargetingData
    | CrossTargeting of string list
and NoCrossTargetingData = { FscArgs: string list; P2PRefs: MSBuildPrj.ResolvedP2PRefsInfo list; Properties: Map<string,string> }

let private runProcess (workingDir: string) (exePath: string) (args: string) =
    let psi = System.Diagnostics.ProcessStartInfo()
    psi.FileName <- exePath
    psi.WorkingDirectory <- workingDir
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.Arguments <- args
    psi.CreateNoWindow <- true
    psi.UseShellExecute <- false

    use p = new System.Diagnostics.Process()
    p.StartInfo <- psi

    let sbOut = System.Text.StringBuilder()
    p.OutputDataReceived.Add(fun ea -> sbOut.AppendLine(ea.Data) |> ignore)

    let sbErr = System.Text.StringBuilder()
    p.ErrorDataReceived.Add(fun ea -> sbErr.AppendLine(ea.Data) |> ignore)

    p.Start() |> ignore
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()

    let exitCode = p.ExitCode
    exitCode, (workingDir, exePath, args)

let private msbuildPropBool (s: string) =
  match s.Trim() with
  | "" -> None
  | MSBuildPrj.MSBuild.ConditionEquals "True" -> Some true
  | _ -> Some false

let private msbuildPropStringList (s: string) =
  match s.Trim() with
  | "" -> []
  | MSBuildPrj.MSBuild.StringList list  -> list
  | _ -> []

let rec projInfo additionalMSBuildProps (file: string) =
  let projDir = Path.GetDirectoryName file

  let projectAssetsJsonPath = Path.Combine(projDir, "obj", "project.assets.json")
  if not(File.Exists(projectAssetsJsonPath)) then
     failwithf "Cannot find restored info for project %s" file

  let getFscArgs = Dotnet.ProjInfo.Inspect.getFscArgs
  let getP2PRefs = Dotnet.ProjInfo.Inspect.getResolvedP2PRefs
  let gp () = Dotnet.ProjInfo.Inspect.getProperties (["TargetPath"; "IsCrossTargetingBuild"; "TargetFrameworks"; "TargetFramework"])

  let results =
      let runCmd exePath args = runProcess projDir exePath (args |> String.concat " ")

      let msbuildExec = Dotnet.ProjInfo.Inspect.dotnetMsbuild runCmd
      let log = ignore

      let additionalArgs = additionalMSBuildProps |> List.map (Dotnet.ProjInfo.Inspect.MSBuild.MSbuildCli.Property)

      file
      |> Dotnet.ProjInfo.Inspect.getProjectInfos log msbuildExec [getFscArgs; getP2PRefs; gp] additionalArgs

#if NETSTANDARD2_0
  let todo =
      match results with
      | Ok [getFscArgsResult; getP2PRefsResult; gpResult] ->
          match getFscArgsResult, getP2PRefsResult, gpResult with
          | Error (MSBuildPrj.MSBuildSkippedTarget), Error (MSBuildPrj.MSBuildSkippedTarget), Ok(MSBuildPrj.GetResult.Properties props) ->
              // Projects with multiple target frameworks, fails if the target framework is not choosen
              let prop key = props |> Map.ofList |> Map.tryFind key

              match prop "IsCrossTargetingBuild", prop "TargetFrameworks" with
              | Some (MSBuildPrj.MSBuild.ConditionEquals "true"), Some (MSBuildPrj.MSBuild.StringList tfms) ->
                  CrossTargeting tfms
              | _ ->
                  failwithf "error getting msbuild info: some targets skipped, found props: %A" props
          | Ok(MSBuildPrj.GetResult.FscArgs fa), Ok(MSBuildPrj.GetResult.ResolvedP2PRefs p2p), Ok(MSBuildPrj.GetResult.Properties p) ->
              NoCrossTargeting { FscArgs = fa; P2PRefs = p2p; Properties = p |> Map.ofList }
          | r ->
              failwithf "error getting msbuild info: %A" r
      | Ok r ->
          failwithf "error getting msbuild info: internal error, more info returned than expected %A" r
      | Error r ->
          match r with
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.MSBuildSkippedTarget ->
              failwithf "Unexpected MSBuild result, all targets skipped"
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.UnexpectedMSBuildResult(r) ->
              failwithf "Unexpected MSBuild result %s" r
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.MSBuildFailed(exitCode, (workDir, exePath, args)) ->
              [ sprintf "MSBuild failed with exitCode %i" exitCode
                sprintf "Working Directory: '%s'" workDir
                sprintf "Exe Path: '%s'" exePath
                sprintf "Args: '%s'" args ]
              |> String.concat " "
              |> failwith
#else
  let todo =
      match results with
      | Choice1Of2 [getFscArgsResult; getP2PRefsResult; gpResult] ->
          match getFscArgsResult, getP2PRefsResult, gpResult with
          | Choice2Of2 (MSBuildPrj.MSBuildSkippedTarget), Choice2Of2 (MSBuildPrj.MSBuildSkippedTarget), Choice1Of2(MSBuildPrj.GetResult.Properties props) ->
              // Projects with multiple target frameworks, fails if the target framework is not choosen
              let prop key = props |> Map.ofList |> Map.tryFind key

              match prop "IsCrossTargetingBuild", prop "TargetFrameworks" with
              | Some (MSBuildPrj.MSBuild.ConditionEquals "true"), Some (MSBuildPrj.MSBuild.StringList tfms) ->
                  CrossTargeting tfms
              | _ ->
                  failwithf "error getting msbuild info: some targets skipped, found props: %A" props
          | Choice1Of2(MSBuildPrj.GetResult.FscArgs fa), Choice1Of2(MSBuildPrj.GetResult.ResolvedP2PRefs p2p), Choice1Of2(MSBuildPrj.GetResult.Properties p) ->
              NoCrossTargeting { FscArgs = fa; P2PRefs = p2p; Properties = p |> Map.ofList }
          | r ->
              failwithf "error getting msbuild info: %A" r
      | Choice1Of2 r ->
          failwithf "error getting msbuild info: internal error, more info returned than expected %A" r
      | Choice2Of2 r ->
          match r with
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.MSBuildSkippedTarget ->
              failwithf "Unexpected MSBuild result, all targets skipped"
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.UnexpectedMSBuildResult(r) ->
              failwithf "Unexpected MSBuild result %s" r
          | Dotnet.ProjInfo.Inspect.GetProjectInfoErrors.MSBuildFailed(exitCode, (workDir, exePath, args)) ->
              [ sprintf "MSBuild failed with exitCode %i" exitCode
                sprintf "Working Directory: '%s'" workDir
                sprintf "Exe Path: '%s'" exePath
                sprintf "Args: '%s'" args ]
              |> String.concat " "
              |> failwith
#endif

  match todo with
  | CrossTargeting (tfm :: _) ->
      // Atm setting a preferenece is not supported in FSAC
      // As workaround, lets choose the first of the target frameworks and use that
      file |> projInfo ["TargetFramework", tfm]
  | CrossTargeting [] ->
      failwithf "Unexpected, found cross targeting but empty target frameworks list"
  | NoCrossTargeting { FscArgs = rsp; P2PRefs = p2ps; Properties = props } ->

      //TODO cache projects info of p2p ref
    //   let p2pProjects =
    //       p2ps
    //       // do not follow others lang project, is not supported by FCS anyway
    //       |> List.filter (fun p2p -> p2p.ProjectReferenceFullPath.ToLower().EndsWith(".fsproj"))
    //       |> List.map (fun p2p -> p2p.ProjectReferenceFullPath |> projInfo ["TargetFramework", p2p.TargetFramework] )

      //let tar =
           //match props |> Map.tryFind "TargetPath" with
           //| Some t -> t
           //| None -> failwith "error, 'TargetPath' property not found"

      let compileFilesToAbsolutePath (f: string) =
          if f.EndsWith(".fs") then
              if Path.IsPathRooted f then f else Path.Combine(projDir, f)
          else
              f

      let projOptions: FSharpProjectOptions =
          {
              ProjectFileName = file
              SourceFiles =
                rsp
                |> List.filter (fun s -> s.EndsWith(".fs"))
                |> List.filter (fun s -> not <| s.EndsWith("AssemblyInfo.fs"))
                |> List.map compileFilesToAbsolutePath
                |> Array.ofList
              OtherOptions = rsp |> List.map compileFilesToAbsolutePath |> Array.ofList
              ReferencedProjects = [||] //p2pProjects |> Array.ofList
              IsIncompleteTypeCheckEnvironment = false
              UseScriptResolutionRules = false
              LoadTime = DateTime.Now
              UnresolvedReferences = None;
              OriginalLoadReferences = []
              ExtraProjectInfo = None
              Stamp = None
          }
      let projRefs = p2ps |> List.map (fun p2p -> p2p.ProjectReferenceFullPath)
      projOptions, projRefs, props

let GetProjectOptionsFromProjectFile (file : string) =
  projInfo [] file