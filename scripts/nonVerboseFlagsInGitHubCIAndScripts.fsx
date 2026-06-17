#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Library.fs"
#load "../src/FileConventions/Helpers.fs"

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir, _ = Helpers.PreferLessDeeplyNestedDir currentDir rootDir

let validExtensions =
    seq {
        ".yml"
        ".fsx"
        ".fs"
        ".sh"
    }

let invalidFilesWithLines =
    validExtensions
    |> Seq.collect(fun ext ->
        Helpers.GetFiles targetDir ("*" + ext)
        |> Seq.map(fun fileInfo ->
            let lines = FileConventions.GetNonVerboseFlagLines fileInfo
            fileInfo, lines
        )
        |> Seq.filter(fun (_, lines) -> not lines.IsEmpty)
    )
    |> Seq.toList

let message = "Please don't use non-verbose flags in the following files:"

if invalidFilesWithLines.Length > 0 then
    let details =
        invalidFilesWithLines
        |> Seq.map(fun (fileInfo, lines) ->
            let lineDetails =
                lines
                |> Seq.map(fun (lineNum, line) ->
                    sprintf "  Line %i: %s" lineNum line
                )
                |> String.concat Environment.NewLine

            fileInfo.FullName + Environment.NewLine + lineDetails
        )
        |> String.concat Environment.NewLine

    failwith(message + Environment.NewLine + details)
