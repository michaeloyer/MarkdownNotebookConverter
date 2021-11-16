module MarkdownNotebookConverter.Program

open System.Text
open Argu
open System.IO
open FsToolkit.ErrorHandling

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
    result {
        let! arguments =
            try
                Ok <| ArgumentParser.Create<CliArgs>().Parse(args)
            with ex ->
                Error ex.Message

        let! inputFile = arguments.GetResult(Input) |> function
            | file when File.Exists(file) -> Ok file
            | _ -> Error "Input file was not found"
        let outputFile = arguments.GetResult(Output)
                         |> Option.defaultValue(Path.ChangeExtension(inputFile, ".dib"))

        Directory.SetCurrentDirectory(Path.GetDirectoryName inputFile)

        let blocks = IO.parseNotebookSections File.OpenRead inputFile

        use outputStream = File.Open(outputFile, FileMode.Truncate)
        use writer = new StreamWriter(outputStream, Encoding.Default)

        IO.writeBlocks writer blocks
    }
    |> function
        | Ok () -> 0
        | Error message ->
            eprintfn $"%s{message}"
            1
