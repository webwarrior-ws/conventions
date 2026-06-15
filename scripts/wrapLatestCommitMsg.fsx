#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions

#r "nuget: Mono.Unix, Version=7.1.0-final.1.21458.1"
#r "nuget: YamlDotNet, Version=16.1.3"

#load "../src/FileConventions/Library.fs"

#r "nuget: Fsdk, Version=0.9.99--date20260615-1007.git-0e932e5"

open Fsdk
open Fsdk.Process

let commitMsg =
    Process
        .ExecDefault("git log -1 --format=%B", echo = Echo.Off)
        .UnwrapDefault()
        .Trim()

let header, maybeBody =
    let singleEolToJustSeparateLines = 1u

    let lines =
        FileConventions.SplitByEOLs commitMsg singleEolToJustSeparateLines

    if lines.Length = 1 then
        commitMsg, None
    else
        let body = String.Join(Environment.NewLine, lines.Skip 2)
        lines.[0], Some body

let maxCharsPerLine = 64

let maybeWrappedBody =
    match maybeBody with
    | Some body -> Some(FileConventions.WrapText body maxCharsPerLine)
    | _ -> None

let EscapeDoubleQuotes(text: string) =
    Regex.Replace(text, @"([^\\])""", @"$1\""")

let newCommitMsg =
    match maybeWrappedBody with
    | Some wrappedBody ->
        header + Environment.NewLine + Environment.NewLine + wrappedBody
    | _ -> header

Process
    .ExecDefault(
        $"git commit --amend --message \"{EscapeDoubleQuotes newCommitMsg}\"",
        echo = Echo.OutputOnly
    )
    .UnwrapDefault()
    .Trim()
|> ignore<string>
