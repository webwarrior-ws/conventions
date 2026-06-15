#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.9.99--date20260615-1007.git-0e932e5"
#load "../src/FileConventions/Helpers.fs"

Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments = sprintf "new tool-manifest --force"
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

// we need to install specific version because of this bug: https://github.com/dotnet/sdk/issues/24037
Fsdk
    .Process
    .Execute(
        {
            Command = "dotnet"
            Arguments =
                sprintf
                    "tool install fsxc --version 0.9.99--date20260615-1007.git-0e932e5"
        },
        Fsdk.Process.Echo.All
    )
    .UnwrapDefault()
|> ignore<string>

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir, _ = Helpers.PreferLessDeeplyNestedDir currentDir rootDir

Helpers.GetFiles targetDir "*.fsx"
|> Seq.iter(fun fileInfo ->
    Fsdk
        .Process
        .Execute(
            {
                Command = "dotnet"
                Arguments = sprintf "fsxc %s" fileInfo.FullName
            },
            Fsdk.Process.Echo.All
        )
        .UnwrapDefault()
    |> ignore<string>
)
