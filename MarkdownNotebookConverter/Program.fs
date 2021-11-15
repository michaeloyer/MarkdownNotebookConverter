module MarkdownNotebookConverter.Program

open System.Text
open Argu
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

[<EntryPoint>]
let main args =
    let arguments = ArgumentParser.Create<CliArgs>().Parse(args)

    let inputFile = arguments.GetResult(Input)
    let outputFile = arguments.GetResult(Output) |> Option.defaultValue(Path.ChangeExtension(inputFile, ".dib"))

    let blocks = IO.parseNotebookSections File.OpenRead inputFile

    use outputStream = File.Open(outputFile, FileMode.Truncate)
    use writer = new StreamWriter(outputStream, Encoding.Default)

    IO.writeBlocks writer blocks

    0

