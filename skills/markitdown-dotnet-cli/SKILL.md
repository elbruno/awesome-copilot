---
name: markitdown-dotnet-cli
description: 'Convert 15+ file formats to Markdown using the markitdown .NET CLI tool. Use when converting PDF, DOCX, XLSX, PPTX, HTML, CSV, XML, YAML, RTF, EPUB, images, audio, or URLs to Markdown — for AI pipelines, RAG ingestion, documentation workflows, batch processing, or piping document content into other tools.'
---

# markitdown — .NET CLI for Document-to-Markdown Conversion

A .NET global tool that converts 15+ file formats to clean, structured Markdown. Output goes to stdout (for piping) or to a file with `-o`.

> **Mental model:** `markitdown` is a universal document reader — point it at any supported file, directory, or URL and it produces clean Markdown suitable for AI consumption, documentation, or further processing.

Built on the [ElBruno.MarkItDotNet](https://www.nuget.org/packages/ElBruno.MarkItDotNet) library.

---

## 1. When to Use This Skill

Use this skill when the user needs to:

- **Convert a document** (PDF, DOCX, XLSX, PPTX, HTML, CSV, XML, YAML, RTF, EPUB) to Markdown
- **Extract text from images** (OCR via AI satellite package)
- **Transcribe audio** files to text (via AI or Whisper satellite packages)
- **Convert a web page** (URL) to Markdown
- **Batch convert** a folder of documents recursively
- **Pipe document content** into other CLI tools or AI pipelines
- **Get structured metadata** (word count, title) in JSON format
- **Prepare documents for RAG ingestion** or vector database indexing

**Trigger phrases:** "convert to markdown", "extract text from PDF", "batch convert documents", "markitdown", "document to markdown", "convert DOCX", "convert spreadsheet", "web page to markdown", "RAG ingestion pipeline"

---

## 2. Prerequisites & Installation

| Requirement | Details |
|---|---|
| **.NET SDK** | 8.0 or later. Verify: `dotnet --version` |

### Global Installation (recommended)

```bash
dotnet tool install -g ElBruno.MarkItDotNet.Cli
```

### Local (project-scoped) Installation

For project-scoped use, install without `-g`:

```bash
dotnet tool install ElBruno.MarkItDotNet.Cli
```

Then invoke via: `dotnet markitdown <file>`

### Verify

```bash
markitdown --version
```

### Self-Discovery

Use `--help` on any command for full options:

```bash
markitdown --help
markitdown batch --help
markitdown url --help
```

---

## 3. Command Decision Flow

Choose the right command for the task:

| Scenario | Command |
|---|---|
| Convert one file | `markitdown <file>` |
| Convert all files in a directory | `markitdown batch <directory> -o <output-dir>` |
| Convert a web page | `markitdown url <url>` |
| Check what formats are supported | `markitdown formats` |
| Get metadata alongside content | Add `--format json` to any command |
| Process large files (100+ MB) | Add `--streaming` to single-file conversion |

---

## 4. Commands Reference

### `markitdown <file>` — Single File Conversion

Convert one file to Markdown. Output goes to stdout by default.

```bash
markitdown <file> [options]
```

| Option | Description |
|---|---|
| `-o, --output <path>` | Write output to file instead of stdout |
| `--format <format>` | `markdown` (default) or `json` |
| `--streaming` | Stream large files chunk-by-chunk (memory-efficient for large PDFs) |
| `-q, --quiet` | Suppress progress/status messages |
| `-v, --verbose` | Show detailed conversion logs |

```bash
# Convert PDF and print to stdout
markitdown report.pdf

# Save conversion to file
markitdown report.pdf -o report.md

# Get JSON output with metadata
markitdown data.csv --format json | jq .metadata

# Stream a large PDF (lower memory usage)
markitdown large.pdf --streaming -o large.md

# Quiet mode (no status messages)
markitdown document.docx -q
```

---

### `markitdown batch <directory>` — Batch Conversion

Convert all matching files in a directory. Output directory is required.

```bash
markitdown batch <directory> [options]
```

| Option | Description |
|---|---|
| `-o, --output <path>` | Output directory (**required**). Files are saved as `{original}.md` |
| `-r, --recursive` | Include subdirectories |
| `--pattern <glob>` | File glob filter (default: `*.*`). Example: `*.pdf`, `*.{docx,pdf}` |
| `--parallel <count>` | Parallel conversions (default: CPU core count). Use `1` for sequential |
| `--format <format>` | `markdown` (default) or `json` |
| `-q, --quiet` | Suppress progress output |
| `-v, --verbose` | Show detailed conversion logs |

```bash
# Convert all files in a directory
markitdown batch ./documents -o ./output

# Recursive with PDF filter
markitdown batch ./docs -o ./md -r --pattern "*.pdf"

# Convert Word and PDF only
markitdown batch ./mixed -o ./converted -r --pattern "*.{docx,pdf}"

# Limit parallelism (slower but lower memory)
markitdown batch ./large -o ./large-md -r --parallel 2

# Verbose output for troubleshooting
markitdown batch ./docs -o ./md -r -v
```

---

### `markitdown url <url>` — Web Page Conversion

Fetch a web page, strip navigation/scripts/styles, and convert content to Markdown.

```bash
markitdown url <url> [options]
```

| Option | Description |
|---|---|
| `-o, --output <path>` | Save output to file |
| `--format <format>` | `markdown` (default) or `json` |
| `-q, --quiet` | Suppress progress output |
| `-v, --verbose` | Show detailed conversion logs |

```bash
# Print web page as Markdown
markitdown url https://example.com

# Save to file
markitdown url https://example.com/article -o article.md

# Get JSON with metadata (word count, title, etc.)
markitdown url https://example.com --format json | jq .metadata
```

---

### `markitdown formats` — List Supported Formats

Print all registered file formats, extensions, and converter details.

```bash
markitdown formats

# Filter by extension
markitdown formats | grep pdf
```

---

## 5. Supported Formats

| Format | Extensions | Package |
|---|---|---|
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

The CLI ships with **Core + Excel + PowerPoint** converters. AI and Whisper converters require additional satellite packages.

---

## 6. Output Formats

### Markdown (default)

```bash
markitdown report.pdf
```

Returns clean Markdown text suitable for direct use in AI prompts, documentation, or further processing.

### JSON (structured metadata)

```bash
markitdown report.pdf --format json
```

Returns a JSON object with `content` (the Markdown) and `metadata` (word count, title, conversion info):

```bash
# Extract word count
markitdown data.csv --format json | jq .metadata.wordCount

# Extract just the Markdown content
markitdown report.pdf --format json | jq -r .content

# Get title and word count together
markitdown report.pdf --format json | jq '{title: .metadata.title, words: .metadata.wordCount}'
```

---

## 7. Exit Codes

| Code | Meaning | Details |
|---|---|---|
| `0` | Success | File(s) converted without errors |
| `1` | Conversion Error | File content is corrupted or conversion failed |
| `2` | File Not Found | Input file or directory does not exist |
| `3` | Unsupported Format | File extension has no registered converter |

Check exit codes in scripts:

```bash
markitdown report.pdf -o report.md
if [ $? -ne 0 ]; then echo "Conversion failed with exit code $?"; fi
```

---

## 8. Common Agent Patterns

### RAG Ingestion Pipeline

Batch convert a documentation folder for vector database ingestion:

```bash
markitdown batch ./company-docs -o ./ingestion -r --pattern "*.{pdf,docx,xlsx}" -q
```

With JSON metadata for more control:

```bash
markitdown batch ./company-docs -o ./json-output -r --format json
```

### Capture Metadata in Scripts

Extract conversion metadata programmatically:

```bash
result=$(markitdown document.pdf --format json)
word_count=$(echo "$result" | jq .metadata.wordCount)
echo "Converted document has $word_count words"
```

### Stdout Pipeline Chaining

Pipe conversion output directly into other tools:

```bash
# Extract first 20 lines of converted document
markitdown report.pdf | head -20

# Word count of converted markdown
markitdown data.csv | wc -w

# Extract links from converted HTML
markitdown page.html | grep -E '^\['

# Count paragraphs in converted document
markitdown article.docx | grep -c '^$'
```

### Selective Batch Processing

Target specific file types in mixed directories:

```bash
# Only Office documents
markitdown batch ./mixed -o ./out -r --pattern "*.{docx,pptx,xlsx}"

# Only PDFs
markitdown batch ./archive -o ./md -r --pattern "*.pdf"

# All documents except images
markitdown batch ./docs -o ./out -r --pattern "*.{pdf,docx,txt}"
```

### Memory-Efficient Large File Conversion

For large PDFs (100+ MB), use streaming to process page-by-page:

```bash
markitdown large-report.pdf --streaming -o large-report.md
```

### Batch Processing Pipeline Script

Process documents and upload to storage:

```bash
#!/bin/bash
for file in results/*.{pdf,docx,xlsx}; do
    echo "Converting $file..."
    output="${file%.*}.md"
    markitdown "$file" -o "$output"
done
```

### Batch with Low Resource Usage

Reduce parallelism on constrained environments:

```bash
markitdown batch ./corpus -o ./output -r --parallel 1 -q
```

---

## 9. Troubleshooting

| Problem | Error | Fix |
|---|---|---|
| File not found | `Error: File not found: <path>` (exit code 2) | Verify path exists. Use absolute paths if relative fails. |
| Unsupported format | `Error: Unsupported format: .<ext>` (exit code 3) | Run `markitdown formats` to check supported extensions. |
| Conversion failed | `Error: Conversion failed for <file>` (exit code 1) | File may be corrupted or password-protected. Run with `-v` for details. |
| Out of memory on batch | Process killed or hangs | Reduce `--parallel` count (e.g., `--parallel 1`). |
| Tool not found after install | `markitdown: command not found` | Ensure `~/.dotnet/tools` is on PATH. Run `dotnet tool list -g` to verify. |
| Empty output | No content in output file | Run with `-v` for verbose logs. File may be empty or unsupported internally. |

Debug any conversion issue with verbose mode:

```bash
markitdown problem-file.pdf -v
```

---

## 10. Key URLs

| Resource | URL |
|---|---|
| NuGet Package | https://www.nuget.org/packages/ElBruno.MarkItDotNet.Cli |
| GitHub Repository | https://github.com/elbruno/ElBruno.MarkItDotNet |
| Full CLI Documentation | https://github.com/elbruno/ElBruno.MarkItDotNet/blob/main/docs/cli.md |
