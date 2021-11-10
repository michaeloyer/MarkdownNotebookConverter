module MarkdownNotebookConverter.Types

type NotebookBlock =
    | Markdown of text:string
    | Code of language:string * text:string