#!/usr/bin/env -S dotnet fsi

open System
open System.IO

#r "nuget: Fsdk, Version=0.9.99--date20260615-1007.git-0e932e5"

open Fsdk

#load "../src/FileConventions/Helpers.fs"

Process
    .ExecDefault("dotnet new tool-manifest --force")
    .UnwrapDefault()
|> ignore<string>

// we need to install specific version because of this bug: https://github.com/dotnet/sdk/issues/24037
Process
    .ExecDefault(
        sprintf
            "dotnet tool install fsxc --version 0.9.99--date20260615-1007.git-0e932e5"
    )
    .UnwrapDefault()
|> ignore<string>

let rootDir = Path.Combine(__SOURCE_DIRECTORY__, "..") |> DirectoryInfo
let currentDir = Directory.GetCurrentDirectory() |> DirectoryInfo

let targetDir, _ = Helpers.PreferLessDeeplyNestedDir currentDir rootDir

Helpers.GetFiles targetDir "*.fsx"
|> Seq.iter(fun fileInfo ->
    Process
        .ExecDefault(sprintf "dotnet fsxc %s" fileInfo.FullName)
        .UnwrapDefault()
    |> ignore<string>
)
