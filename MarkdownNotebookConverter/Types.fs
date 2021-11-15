module MarkdownNotebookConverter.Types

type MarkdownBlock =
    | Markdown of text:string
    | Code of language:string * text:string
    | File of path:string
    | Comment
    | Empty

type NotebookSection =
    | MarkdownSection of text:string
    | CodeSection of language:string * text:string

type ParsedBlock =
    | NotebookSection of NotebookSection
    | FileBlock of path:string