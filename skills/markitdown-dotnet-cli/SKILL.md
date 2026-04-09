---
name: markitdown-dotnet-cli
description: 'Convert 15+ file formats (PDF, DOCX, XLSX, PPTX, HTML, CSV, XML, YAML, RTF, EPUB, images, URLs) to clean Markdown using the markitdown .NET CLI tool. Use when agents need to convert documents for AI pipelines, RAG ingestion, documentation workflows, or batch processing.'
---

# markitdown — .NET CLI Tool

A .NET global tool that converts 15+ file formats to clean, structured Markdown. Handles PDF, DOCX, XLSX, PPTX, HTML, CSV, XML, YAML, RTF, EPUB, images, audio, and web pages. Output goes to stdout (for piping) or to a file with `-o`. Built on the [ElBruno.MarkItDotNet](https://www.nuget.org/packages/ElBruno.MarkItDotNet) library.

## Prerequisites

- .NET SDK 8.0 or later. Verify with `dotnet --version`.

## Installation

```bash
dotnet tool install -g ElBruno.MarkItDotNet.Cli
```

Verify:

```bash
markitdown --version
```

## Commands Reference

### `markitdown <file>` — Single File Conversion

Convert one file to Markdown. Output goes to stdout by default.

**Usage:**

```bash
markitdown <file> [options]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <path>` | Write output to file instead of stdout |
| `--format <format>` | `markdown` (default) or `json` |
| `--streaming` | Stream large files chunk-by-chunk (memory-efficient for large PDFs) |
| `-q, --quiet` | Suppress progress/status messages |
| `-v, --verbose` | Show detailed conversion logs |

**Examples:**

```bash
# Convert PDF and print to stdout
markitdown report.pdf

# Save conversion to file
markitdown report.pdf -o report.md

# Stream a large PDF (lower memory usage)
markitdown large.pdf --streaming -o large.md
```

---

### `markitdown batch <directory>` — Batch Conversion

Convert all matching files in a directory. Output directory is required.

**Usage:**

```bash
markitdown batch <directory> [options]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <path>` | Output directory (**required**). Files are saved as `{original}.md` |
| `-r, --recursive` | Include subdirectories |
| `--pattern <glob>` | File glob filter (default: `*.*`). Example: `*.pdf`, `*.{docx,pdf}` |
| `--parallel <count>` | Parallel conversions (default: CPU core count). Use `1` for sequential |
| `--format <format>` | `markdown` (default) or `json` |
| `-q, --quiet` | Suppress progress output |
| `-v, --verbose` | Show detailed conversion logs |

**Examples:**

```bash
# Convert all files in a directory
markitdown batch ./documents -o ./output

# Recursive with PDF filter
markitdown batch ./docs -o ./md -r --pattern "*.pdf"

# Multiple formats, limited parallelism
markitdown batch ./mixed -o ./converted -r --pattern "*.{docx,pdf,xlsx}" --parallel 2
```

---

### `markitdown url <url>` — Web Page Conversion

Fetch a web page, strip navigation/scripts/styles, and convert content to Markdown.

**Usage:**

```bash
markitdown url <url> [options]
```

**Options:**

| Option | Description |
|--------|-------------|
| `-o, --output <path>` | Save output to file |
| `--format <format>` | `markdown` (default) or `json` |
| `-q, --quiet` | Suppress progress output |
| `-v, --verbose` | Show detailed conversion logs |

**Examples:**

```bash
# Print web page as Markdown
markitdown url https://example.com

# Save to file
markitdown url https://example.com/article -o article.md

# Get JSON with metadata
markitdown url https://example.com --format json
```

---

### `markitdown formats` — List Supported Formats

Print all registered file formats, extensions, and converter details.

```bash
markitdown formats
```

Filter output:

```bash
markitdown formats | grep pdf
```

## Supported Formats

| Format | Extensions | Package |
|--------|-----------|---------|
| Plain Text | `.txt`, `.md`, `.log` | Core |
| JSON | `.json` | Core |
| HTML | `.html`, `.htm` | Core |
| URL (Web Pages) | `.url` | Core |
| Word (DOCX) | `.docx` | Core |
| PDF | `.pdf` | Core |
| CSV | `.csv` | Core |
| XML | `.xml` | Core |
| YAML | `.yaml`, `.yml` | Core |
| RTF | `.rtf` | Core |
| EPUB | `.epub` | Core |
| Images | `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg` | Core |
| Excel (XLSX) | `.xlsx` | Excel satellite |
| PowerPoint (PPTX) | `.pptx` | PowerPoint satellite |
| Images (AI-OCR) | All image formats | AI satellite |
| Audio (AI Transcription) | `.mp3`, `.wav`, `.m4a`, `.ogg` | AI satellite |
| PDF (AI-OCR) | `.pdf` | AI satellite |
| Audio (Local Whisper) | `.wav`, `.mp3`, `.m4a`, `.ogg`, `.flac` | Whisper satellite |

The CLI ships with Core + Excel + PowerPoint converters. AI and Whisper converters require additional configuration.

## Output Formats

### Markdown (default)

```bash
markitdown report.pdf
```

Returns clean Markdown text suitable for direct use in AI prompts, documentation, or further processing.

### JSON

```bash
markitdown report.pdf --format json
```

Returns a JSON object with `content` (the Markdown) and `metadata` (word count, title, conversion info). Use for programmatic access:

```bash
# Extract word count
markitdown data.csv --format json | jq .metadata.wordCount

# Extract just the content
markitdown report.pdf --format json | jq -r .content
```

## Exit Codes

| Code | Meaning | When |
|------|---------|------|
| `0` | Success | File(s) converted without errors |
| `1` | Conversion Error | File content is corrupted or conversion failed |
| `2` | File Not Found | Input file or directory does not exist |
| `3` | Unsupported Format | File extension has no registered converter |

Check exit codes in scripts:

```bash
markitdown report.pdf -o report.md
if [ $? -ne 0 ]; then echo "Conversion failed"; fi
```

## Common Agent Patterns

### RAG Ingestion Pipeline

Batch convert a documentation folder for vector database ingestion:

```bash
markitdown batch ./company-docs -o ./ingestion -r --pattern "*.{pdf,docx,xlsx}" -q
```

### JSON Metadata Extraction

Get structured metadata for indexing or analytics:

```bash
markitdown report.pdf --format json | jq '{words: .metadata.wordCount, title: .metadata.title}'
```

### Stdout Pipeline Chaining

Pipe conversion output directly into other tools:

```bash
# Feed converted content to another process
markitdown report.pdf | head -50

# Word count of converted document
markitdown data.csv | wc -w
```

### Selective Batch Processing

Target specific file types in mixed directories:

```bash
# Only Office documents
markitdown batch ./mixed -o ./out -r --pattern "*.{docx,pptx,xlsx}"

# Only PDFs
markitdown batch ./archive -o ./md -r --pattern "*.pdf"
```

### Memory-Efficient Large File Conversion

For large PDFs (100+ MB), use streaming to avoid high memory usage:

```bash
markitdown large-report.pdf --streaming -o large-report.md
```

### Batch with Low Resource Usage

Reduce parallelism on constrained environments:

```bash
markitdown batch ./corpus -o ./output -r --parallel 1 -q
```

## Troubleshooting

| Problem | Exit Code | Fix |
|---------|-----------|-----|
| File not found | `2` | Verify path exists. Use absolute paths if relative fails. |
| Unsupported format | `3` | Run `markitdown formats` to check supported extensions. |
| Out of memory on batch | — | Reduce `--parallel` count (e.g., `--parallel 1`). |
| Conversion produces empty output | `1` | Run with `-v` for verbose logs. File may be corrupted or password-protected. |
| Tool not found after install | — | Ensure `~/.dotnet/tools` is on PATH. Run `dotnet tool list -g` to verify. |

Debug any conversion issue with verbose mode:

```bash
markitdown problem-file.pdf -v
```

## Key URLs

| Resource | URL |
|----------|-----|
| NuGet Package | https://www.nuget.org/packages/ElBruno.MarkItDotNet.Cli |
| GitHub Repository | https://github.com/elbruno/ElBruno.MarkItDotNet |
| Full CLI Documentation | https://github.com/elbruno/ElBruno.MarkItDotNet/blob/main/docs/cli.md |
