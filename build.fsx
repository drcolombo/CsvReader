/// FAKE Build script

#r "paket:
source release/dotnetcore
source https://api.nuget.org/v3/index.json
nuget FSharp.Core 4.3.4 // https://github.com/fsharp/FAKE/issues/2001
nuget System.AppContext prerelease
nuget Paket.Core prerelease
nuget Fake.Core.Target prerelease
nuget Fake.IO.FileSystem prerelease
nuget Fake.Core.ReleaseNotes prerelease
nuget Fake.DotNet.AssemblyInfoFile prerelease
nuget Fake.DotNet.MSBuild prerelease
nuget Fake.DotNet.Cli prerelease
nuget Fake.DotNet.NuGet prerelease
nuget Fake.DotNet.Paket prerelease
nuget Fake.DotNet.FSFormatting prerelease
nuget Fake.DotNet.Testing.MSpec prerelease
nuget Fake.DotNet.Testing.XUnit2 prerelease
nuget Fake.DotNet.Testing.NUnit prerelease
nuget Fake.Tools.Git prerelease
nuget NUnit
nuget Newtonsoft.Json //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.Tools.Git
open Fake.IO
open Fake.DotNet.NuGet

// Version info
let authors = ["S�bastien Lorion"; "Paul Hatcher"; "Spintronic"]

let release = ReleaseNotes.load "RELEASE_NOTES.md"

// Properties
let buildDir = "./build"

// Targets
Target.create "Clean" (fun _ ->
    Shell.cleanDir buildDir
)

Target.create "PackageRestore" (fun _ ->
     Restore.RestorePackages()
)

Target.create "SetVersion" (fun _ ->
    let commitHash = Information.getCurrentHash()
    let infoVersion = String.concat " " [release.AssemblyVersion; commitHash]
    AssemblyInfoFile.createCSharp "./code/SolutionInfo.cs"
        [AssemblyInfo.Version release.AssemblyVersion
         AssemblyInfo.FileVersion release.AssemblyVersion
         AssemblyInfo.InformationalVersion infoVersion]
)

Target.create "Build" (fun _ ->
    DotNet.build (fun o -> 
        { o with
            Configuration = DotNet.BuildConfiguration.Release
        }
    ) "./CsvReader.sln"
)

Target.create "Test" (fun _ ->
    DotNet.test (fun o ->
        { o with
            NoRestore = true
        }
    ) "./code/CsvReader.UnitTests/CsvReader.UnitTests.csproj"
)

Target.create "Pack" (fun _ ->
    NuGet.NuGetPack (fun p ->
        { p with
            Authors = authors
            Version = release.AssemblyVersion
            ReleaseNotes = release.Notes |> String.toLines
            OutputPath = buildDir 
            AccessKey = Environment.environVarOrDefault "nugetkey" ""
            Publish = Environment.hasEnvironVar "nugetkey"
        }
    ) "nuget/LumenWorksCsvReader2.nuspec"
)

Target.create "Release" (fun _ ->
    let tag = String.concat "" ["v"; release.AssemblyVersion] 
    Branches.tag "" tag
    Branches.pushTag "" "origin" tag
)

Target.create "Default" ignore

// Dependencies
open Fake.Core.TargetOperators

"Clean"
    ==> "SetVersion"
    ==> "PackageRestore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Pack"
    ==> "Release"

Target.runOrDefault "Default"