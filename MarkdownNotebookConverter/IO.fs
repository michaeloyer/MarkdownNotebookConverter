module MarkdownNotebookConverter.IO

open System.IO
open MarkdownNotebookConverter.Types
open System.Text
open FParsec

module internal Parsers =
    let file =
        skipString "<!--file(" >>. manyCharsTill anyChar (skipString ")-->")
        |>> File

    let comment =
        skipString "<!--" >>. skipManyTill anyChar (skipString "-->")
        |>> fun () -> Comment

    let code languageText supportedLanguage =
        skipString ("```" + languageText) >>. (manyCharsTill anyChar (skipString "```"))
        |>> fun text -> Code (supportedLanguage, (text.Trim()))

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
            code "fsharp" FSharp
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
    let rec parseNotebookSections usedPathSet parsedBlocks = [
        for parsedBlock in parsedBlocks do
            match parsedBlock with
            | NotebookSection notebookSection -> yield notebookSection
            | FileBlock path ->
                if usedPathSet |> Set.contains path then
                    yield (MarkdownSection $"*Recursive Path: {path}*")
                else
                    match openFile path with
                    | FileExists stream ->
                        use stream = stream
                        yield! parseNotebookSections (usedPathSet |> Set.add path) (parseMarkdown stream)
                    | FileNotFound ->
                        yield (MarkdownSection $"*File Missing: {path}*")
    ]

    parseNotebookSections Set.empty [FileBlock filePath]

let writeBlocks (writer:#TextWriter) blocks =
    let writeBlock (magicCommand:string) (text:string) =
        writer.WriteLine(magicCommand)
        writer.WriteLine()
        writer.WriteLine(text)
        writer.WriteLine()

    for block in blocks do
        match block with
        | CodeSection(FSharp, text) -> writeBlock "#!F#" text
        | MarkdownSection text -> writeBlock "#!markdown" text
