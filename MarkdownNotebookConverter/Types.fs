module MarkdownNotebookConverter.Types

type SupportedLanguage = FSharp

type MarkdownBlock =
    | Markdown of text:string
    | Code of language:SupportedLanguage * text:string
    | File of path:string
    | Comment
    | Empty

type NotebookSection =
    | MarkdownSection of text:string
    | CodeSection of language:SupportedLanguage * text:string

type ParsedBlock =
    | NotebookSection of NotebookSection
    | FileBlock of path:string

type FileOpenResult =
    | FileExists of System.IO.Stream
    | FileNotFound
