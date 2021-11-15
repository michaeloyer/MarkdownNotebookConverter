module MarkdownNotebookConverter.IO

open System.IO
open Types
open System.Text
open FParsec

module internal Parsers =
    let file =
        skipString "<!--file(" >>. manyCharsTill anyChar (skipString ")-->")
        |>> File

    let comment =
        skipString "<!--" >>. skipManyTill anyChar (skipString "-->")
        |>> fun () -> Comment

    let code language =
        skipString ("```" + language) >>. (manyCharsTill anyChar (skipString "```"))
        |>> fun text -> Code (language, (text.Trim()))

    let nonMarkdown =
        lookAhead (choice [
            skipString "```"
            skipString "<!--"
            eof
        ])

    let markdown =
        manyCharsTill anyChar nonMarkdown
        |>> function
            | text when System.String.IsNullOrWhiteSpace text -> Empty
            | text -> Markdown (text.Trim())

    let parsedBlock =
        choice [
            code "fsharp"
            file
            comment
            markdown
        ]
        |>> function
            | Markdown text -> Some(NotebookSection(MarkdownSection(text)))
            | Code(language, text) -> Some(NotebookSection(CodeSection(language, text)))
            | File path -> Some(FileBlock(path))
            | Empty -> None
            | Comment -> None

    let parsedBlocks = manyTill parsedBlock eof |>> List.choose id

let private parseMarkdown stream =
    runParserOnStream Parsers.parsedBlocks "" "stream" stream Encoding.Default
    |> function
        | Success (result, _, _) -> result
        | Failure (message, _, _) -> failwith message

let parseNotebookSections openFile filePath =
    let fileBlock = FileBlock filePath

    let rec parseNotebookSections parsedBlocks = [
        for parsedBlock in parsedBlocks do
            match parsedBlock with
            | NotebookSection notebookSection -> yield notebookSection
            | FileBlock path ->
                use stream = openFile path
                yield! parseNotebookSections (parseMarkdown stream)
    ]

    parseNotebookSections [fileBlock]

let writeBlocks (writer:#TextWriter) blocks =
    let writeBlock (magicCommand:string) (text:string) =
        writer.WriteLine(magicCommand)
        writer.WriteLine()
        writer.WriteLine(text)
        writer.WriteLine()

    for block in blocks do
        match block with
        | CodeSection("fsharp", text) -> writeBlock "#!fsharp" text
        | CodeSection _ -> ()
        | MarkdownSection text -> writeBlock "#!markdown" text