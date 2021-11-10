module MarkdownNotebookConverter.Parsing
open System.Text
open FParsec
open Types

let code language = skipString ("```" + language) >>. (manyCharsTill anyChar (skipString "```")) |>> fun text -> Code (language, (text.Trim()))

let markdown = manyCharsTill anyChar (lookAhead (skipString "```" <|> eof)) |>> fun text -> Markdown (text.Trim())

let NotebookBlock = code "fsharp" <|> markdown

let ParseMarkdown stream =
    runParserOnStream (manyTill NotebookBlock eof) "" "stream" stream Encoding.UTF8
    |> function
        | Success (result, _, _) -> result
        | Failure (message, _, _) -> failwith message
