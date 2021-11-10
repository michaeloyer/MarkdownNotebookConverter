module MarkdownNotebookConverter.Program

open Argu
open System
open System.IO

[<CliPrefix(CliPrefix.DoubleDash)>]
type CliArgs =
    | [<Mandatory; MainCommand>] Input of string
    | [<AltCommandLine("-o"); Unique>] Output of string option
    interface IArgParserTemplate with
        member self.Usage =
            match self with
            | Input _ -> "The input markdown file to read from"
            | Output _ -> "The output notebook file to write to"

let args = Environment.GetCommandLineArgs() |> Array.skip 1

let arguments = ArgumentParser.Create<CliArgs>().Parse(args)

let inputFile = arguments.GetResult(Input)
let outputFile = arguments.GetResult(Output) |> Option.defaultValue(Path.ChangeExtension(inputFile, ".dib"))

let inputStream = File.OpenRead inputFile
let outputStream = File.Open(outputFile, FileMode.Truncate)

let blocks = Parsing.ParseMarkdown inputStream

FileIO.writeBlocks outputStream blocks

inputStream.Dispose()
outputStream.Dispose()

