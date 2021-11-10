module MarkdownNotebookConverter.FileIO

open System.IO
open MarkdownNotebookConverter.Types

let writeBlocks (stream:Stream) blocks =
    use writer = new StreamWriter(stream)
    for block in blocks do
        match block with
        | Code("fsharp", text) ->
            writer.WriteLine("#!fsharp")
            writer.WriteLine()
            writer.WriteLine(text)
            writer.WriteLine()
        | Markdown text ->
            writer.WriteLine("#!markdown")
            writer.WriteLine()
            writer.WriteLine(text)
            writer.WriteLine()
