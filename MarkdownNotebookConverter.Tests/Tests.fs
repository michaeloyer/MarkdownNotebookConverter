module FileIO.Tests

open System
open System.IO
open System.Text
open MarkdownNotebookConverter
open MarkdownNotebookConverter.Types
open FsUnit
open Xunit

let getOpenFile map (path:string) =
    match map |> Map.tryFind path with
    | Some (text:string) ->
        let stream = new MemoryStream()
        stream.Write(ReadOnlySpan(Encoding.Default.GetBytes(text)))
        stream.Position <- 0L

        FileExists stream
    | None ->
        FileNotFound

let testWriteBlocksToString writeToWriterFunction blocks =
    use writer = new StringWriter(NewLine="\n")
    writeToWriterFunction writer blocks
    writer.GetStringBuilder().ToString()

let getOpenSingleFile fileName contents =
    getOpenFile (Map [ fileName, contents ])

[<Fact>]
let ``F# Code Section Produces F# Notebook Section`` () =
    let openFile = getOpenSingleFile "test" """
```fsharp
let a = 1
a
```
"""

    IO.parseNotebookSections openFile "test"
    |> testWriteBlocksToString IO.writeBlocks
    |> should equal """#!F#

let a = 1
a

"""

[<Fact>]
let ``Text Section Produces Markdown Notebook Section`` () =
    let openFile = getOpenSingleFile "test" """
This is
some **markdown** text
"""

    IO.parseNotebookSections openFile "test"
    |> testWriteBlocksToString IO.writeBlocks
    |> should equal """#!markdown

This is
some **markdown** text

"""

[<Fact>]
let ``Comments in markdown are ignored and not put in the notebook`` () =
    let openFile = getOpenSingleFile "test" """
Text 1

<!-- Html Comment Here -->
Text 2
"""

    IO.parseNotebookSections openFile "test"
    |> testWriteBlocksToString IO.writeBlocks
    |> should equal """#!markdown

Text 1

#!markdown

Text 2

"""

[<Fact>]
let ``Referenced files are copied and put into a flat notebook`` () =
    let files = Map [
        "test", """This is some Text

<!--file(test2)-->
"""
        "test2", """This is some Text in file 2"""
    ]

    let openFile = getOpenFile files

    IO.parseNotebookSections openFile "test"
    |> testWriteBlocksToString IO.writeBlocks
    |> should equal """#!markdown

This is some Text

#!markdown

This is some Text in file 2

"""

[<Fact>]
let ``Missing file paths are written in the notebook`` () =
    let path = "missing/file/path"
    let openFile = getOpenSingleFile "test" $"""<!--file({path})-->"""

    IO.parseNotebookSections openFile "test"
    |> testWriteBlocksToString IO.writeBlocks
    |> should equal $"""#!markdown

*File Missing: {path}*

"""
