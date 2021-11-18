module MarkdownNotebookConverter.Types

type SupportedLanguage = FSharp

type FilePath = string
type FileContents = string

type MarkdownBlock =
    | Markdown of text:FileContents
    | Code of language:SupportedLanguage * text:FileContents
    | File of path:FilePath
    | CodeFile of language:SupportedLanguage * path:FilePath
    | Comment
    | Empty

type NotebookSection =
    | MarkdownSection of text:FileContents
    | CodeSection of language:SupportedLanguage * text:FileContents

type ParsedBlock =
    | NotebookSection of NotebookSection
    | FileBlock of path:FilePath
    | CodeFileBlock of SupportedLanguage * path:FilePath

type FileOpenResult =
    | FileExists of System.IO.Stream
    | FileNotFound
