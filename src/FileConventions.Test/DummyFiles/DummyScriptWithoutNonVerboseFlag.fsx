#!/usr/bin/env -S dotnet fsi

#r "nuget: Fsdk, Version=0.9.99--date20260615-1007.git-0e932e5"

open Fsdk
open Fsdk.Process

let gitRemote =
    {
        Command = "git"
        Arguments = "remote --version"
    }

let gitRemoteOutput =
    Process
        .Execute(gitRemote, Echo.All)
        .UnwrapDefault()
