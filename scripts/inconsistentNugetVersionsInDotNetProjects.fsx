#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "System.Core.dll"
#r "System.Xml.Linq.dll"

#r "nuget: Fsdk, Version=0.9.99--date20260525-0605.git-a5cfc39"

open Fsdk

#r "nuget: Microsoft.Build, Version=17.8.43"

#load "../src/FileConventions/Helpers.fs"
#load "../src/FileConventions/NugetVersionsCheck.fs"

open NugetVersionsCheck

let errUsage =
    (1, sprintf "Usage: dotnet fsi %s [example.sln(optional)]" __SOURCE_FILE__)

let ErrDirectoryNotAllowed arg =
    (2, sprintf "Use an .sln file instead of directory: '%s'" arg)

let ErrFileNotFound arg =
    (3, sprintf "'%s' does not exist." arg)

let ErrInvalidArgument arg =
    (4,
     sprintf
         "'%s' argument is invalid. You should enter an .sln file or run this script on current directory"
         arg)

let args = Misc.FsxOnlyArguments()
let currentDirectory = Directory.GetCurrentDirectory()

if args.Length > 1 then
    let exitCode, errMsg = errUsage
    Console.Error.WriteLine errMsg
    Environment.Exit exitCode

let target =
    if args.IsEmpty then
        currentDirectory |> DirectoryInfo |> ScriptTarget.Folder
    else
        let singleArg = args.[0]

        if Directory.Exists singleArg then
            let exitCode, errMsg = ErrDirectoryNotAllowed singleArg
            Console.Error.WriteLine errMsg
            Environment.Exit exitCode
            failwith <| "Unreachable because of: " + errMsg
        elif not(File.Exists singleArg) then
            let exitCode, errMsg = ErrFileNotFound singleArg
            Console.Error.WriteLine errMsg
            Environment.Exit exitCode
            failwith <| "Unreachable because of: " + errMsg
        elif singleArg.EndsWith ".sln" then
            singleArg |> FileInfo |> ScriptTarget.Solution
        else
            let exitCode, errMsg = ErrInvalidArgument singleArg
            Console.Error.WriteLine errMsg
            Environment.Exit exitCode
            failwith <| "Unreachable because of: " + errMsg


let nugetSolutionPackagesDir =
    Path.Combine(currentDirectory, "packages") |> DirectoryInfo

let nugetPackageConfigDir =
    Path.Combine(currentDirectory, "NuGet.config") |> FileInfo

if not nugetPackageConfigDir.Exists || not nugetSolutionPackagesDir.Exists then
    failwithf
        "NuGet.config not found in '%s', please create it with the `globalPackagesFolder` key as `packages`."
        nugetPackageConfigDir.FullName

SanityCheckNugetPackages target currentDirectory nugetSolutionPackagesDir
