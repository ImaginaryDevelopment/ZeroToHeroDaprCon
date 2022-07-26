open System
open System.IO

open Fake.Core
open Fake.DotNet
// open Fake.Tools
// open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
// open Fake.Core.TargetOperators
// open Fake.Api
// open Fake.BuildServer

let src =
    __SOURCE_DIRECTORY__
    </> ".."
    </> "src"

let slnRootDir = src

let sln =
    slnRootDir
    </> "ContosoCrafts.sln"

let srcProjGlob = src @@ "**/*.??proj"

let invokeAsync f = async { f () }

let failOnBadExitAndPrint (p: ProcessResult) =
    if p.ExitCode <> 0 then
        p.Errors |> Seq.iter Trace.traceError
        failwithf "failed with exitcode %d" p.ExitCode

module dotnet =
    let watch cmdParam program args =
        DotNet.exec cmdParam (sprintf "watch %s" program) args

    let tool optionConfig command args =
        DotNet.exec optionConfig (sprintf "%s" command) args
        |> failOnBadExitAndPrint

let runAll () =
    !!srcProjGlob
    |> Seq.map (fun proj ->
        fun () ->
            dotnet.watch
                (fun opt ->
                                opt
                                |> DotNet.Options.withWorkingDirectory (IO.Path.GetDirectoryName proj)
                )
                "run"
                ""
            |> ignore
    )
    |> Seq.iter (
        invokeAsync
        >> Async.Catch
        >> Async.Ignore
        >> Async.Start
    )

    printfn "Press Ctrl+C (or Ctrl+Break) to stop..."

    let cancelEvent =
        Console.CancelKeyPress
        |> Async.AwaitEvent
        |> Async.RunSynchronously

    cancelEvent.Cancel <- true

let initTargets() =

    Target.create "BuildSln" (fun _ ->
        dotnet.tool (DotNet.Options.withWorkingDirectory slnRootDir) "build" ""
    )
    Target.create "watchrun" (fun _ ->
        runAll()
    )

[<EntryPoint>]
let main argv =

    let fakeContext =
        argv
        |> Array.toList
        |> Context.FakeExecutionContext.Create false "../build.fsx"

    let context = fakeContext |> Context.RuntimeContext.Fake

    context |> Context.setExecutionContext
    Trace.tracefn "main pre-working dir: '%s'" Environment.CurrentDirectory
    Environment.CurrentDirectory <- slnRootDir
    Trace.tracefn "Working dir: '%s'" slnRootDir
    Trace.tracefn "Working dir full: '%s'" <| System.IO.Path.GetFullPath slnRootDir

    initTargets()

    Target.runOrDefaultWithArguments "BuildSln"
    0
