#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "System.Configuration"
open System.Configuration

#r "nuget: Fsdk, Version=0.9.99--date20260525-0605.git-a5cfc39"
open Fsdk

let errTooManyArgs =
    (1,
     "Can only pass two arguments, with optional flag: replace.fsx --file=a.b oldstring newstring")

let errTooFewArgs =
    (2, "Need to pass two arguments: replace.fsx oldstring newstring")

let ErrFileNotFound path =
    (3, sprintf "File '%s' doesn't exist" path)

let note =
    "NOTE: by default, some kind of files/folders will be excluded, e.g.: .git, *.dll, *.png, ..."

let args = Misc.FsxOnlyArguments()

if args.Length > 3 then
    let exitCode, errMsg = errTooManyArgs
    Console.Error.WriteLine errMsg
    Console.WriteLine note
    Environment.Exit exitCode
elif args.Length < 2 then
    let exitCode, errMsg = errTooFewArgs
    Console.Error.WriteLine errMsg
    Console.WriteLine note
    Environment.Exit exitCode

let firstArg = args.[0]

let particularFile =
    if firstArg.StartsWith "--file=" || firstArg.StartsWith "-f=" then
        let file = firstArg.Substring(firstArg.IndexOf("=") + 1) |> FileInfo

        if not file.Exists then
            let exitCode, errMsg = ErrFileNotFound file.FullName
            Console.Error.WriteLine errMsg
            Environment.Exit exitCode
            failwith <| "Unreachable because of: " + errMsg

        Some file
    else
        if args.Length = 3 then
            let exitCode, errMsg = errTooManyArgs
            Console.Error.WriteLine errMsg
            Console.WriteLine note
            Environment.Exit exitCode
            failwith <| "Unreachable because of: " + errMsg

        None

match particularFile with
| None ->
    let startDir = DirectoryInfo(Directory.GetCurrentDirectory())
    let oldString, newString = args.[0], args.[1]
    Misc.ReplaceTextInDir startDir oldString newString
| Some file ->
    let oldString, newString = args.[1], args.[2]
    Misc.ReplaceTextInFile file oldString newString
