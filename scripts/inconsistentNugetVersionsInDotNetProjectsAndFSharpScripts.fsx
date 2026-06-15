#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "System.Core.dll"
#r "System.Xml.Linq.dll"

#r "nuget: Fsdk, Version=0.9.99--date20260615-1007.git-0e932e5"
#r "nuget: Microsoft.Build, Version=17.8.43"
#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/NugetVersionsCheck.fs"
#load "../src/FileConventions/CombinedVersionCheck.fs"

let errInconsistentVersions =
    (1,
     "Inconsistent NuGet package versions detected between F# scripts and .NET projects")

let currentDir = Directory.GetCurrentDirectory()

let targetDirs =
    let args =
        Fsdk.Misc.FsxOnlyArguments()
        |> List.filter(fun arg -> not(arg.StartsWith "-"))

    match args with
    | [] -> List.singleton(DirectoryInfo currentDir)
    | paths ->
        let dirs = paths |> List.map DirectoryInfo

        for dir in dirs do
            if not dir.Exists then
                failwith $"Not a directory: {dir}"

        dirs

let versionsMap =
    let fsxFiles, projectFiles =
        CombinedVersionCheck.GetFsxAndProjectFilesForDirectories targetDirs

    CombinedVersionCheck.GetVersionsMapForNugetRefsInFSharpScriptsAndProjects
        fsxFiles
        projectFiles

let packagesWithMultipleVersions =
    versionsMap |> Map.filter(fun _ versions -> versions.Count > 1)

if not packagesWithMultipleVersions.IsEmpty then
    for KeyValue(packageName, versions) in packagesWithMultipleVersions do
        let versionsString =
            String.Join(
                ", ",
                versions |> Seq.map(fun version -> $"\"{version}\"")
            )

        Console.Error.WriteLine
            $"Found {versions.Count} different versions of package \"{packageName}\": {versionsString}"

    let exitCode, errMsg = errInconsistentVersions
    Console.Error.WriteLine errMsg
    Environment.Exit exitCode
