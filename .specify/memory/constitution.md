# Smart Study Assistant Constitution

## Core Principles

### I. Clean Architecture & Strict MVVM
- Strict separation between UI (XAML Views), Presentation Logic (ViewModels via `CommunityToolkit.Mvvm`), and Domain/Infrastructure Services.
- Views never directly call AI endpoints, database contexts, or file I/O; all operations flow through injected interfaces via Dependency Injection (`Microsoft.Extensions.DependencyInjection`).

### II. Pluggable Multi-Provider AI Engine (`IAiProvider`)
- Unify OpenAI, Google Gemini, Anthropic Claude, and Groq/Ollama under a robust `IAiProvider` contract supporting:
  - Text completion & streaming responses (`IAsyncEnumerable<string>`).
  - Embeddings generation for RAG chunking.
  - Runtime provider hot-swapping without losing conversation context.
- Fallback gracefully with descriptive user guidance when API keys are unconfigured or provider rates are throttled.

### III. Local-First Privacy & Credential Security
- API keys are never stored in plaintext; encrypted using Windows DPAPI (`System.Security.Cryptography.ProtectedData`).
- Student documents, flashcards, notes, and chat transcripts are stored locally in SQLite (`Microsoft.Data.Sqlite` / EF Core SQLite). No student data is sent to external servers other than the selected AI provider.

### IV. Transparent RAG & Verifiable Citations
- Supported formats: PDF, DOCX, TXT.
- Chunks preserve source metadata (Document Title, Page/Slide Number, Section Header).
- AI responses based on RAG must explicitly cite sources (e.g., `[Slide 14 - KienTrucPhanMem.pdf]`) and render interactive citation badges allowing students to jump to or view the cited passage.

### V. Anti-Slop UI/UX Excellence (Impeccable & UI/UX Pro Max)
- Polished WPF interface adhering to modern desktop design principles (Fluent / Modern Dark & Light modes).
- Strict visual hierarchy, purposeful color accents (avoiding generic gradients), legible typography (`Segoe UI Variable` / `Inter`), and WCAG AA contrast compliance.
- Non-blocking asynchronous UI (`async/await`) for document processing and AI streaming; progress bars and cancellation tokens for all heavy operations.

### VI. Test-Driven Verification & Quality (Superpowers)
- Core algorithmic modules (document chunking, cosine vector similarity, Pomodoro state machine, flashcard SRS scheduling) must be backed by automated unit tests (`xUnit` / `MSTest`).
- Zero unhandled exceptions in UI; comprehensive error boundaries with user-friendly toast notifications.

## Technology Stack Standards
- **Runtime**: .NET 8.0 Windows (`net8.0-windows`)
- **Presentation**: WPF with `CommunityToolkit.Mvvm` (8.x)
- **Local Persistence**: SQLite (`Microsoft.Data.Sqlite` / Dapper or EF Core)
- **Document Parsing**: `PdfPig` / `UglyToad.PdfPig` (PDF), `DocumentFormat.OpenXml` (DOCX)
- **Vector Search**: In-memory / SQLite vector embedding index with cosine similarity calculation

## Governance
- This Constitution is the authoritative standard for all specifications, architectural plans, and code implementations.
- Features must trace directly to requirements in `specs/001-smart-study-assistant/spec.md`.

**Version**: 1.0.0 | **Ratified**: 2026-11-16
