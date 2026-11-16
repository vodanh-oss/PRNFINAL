# Findings & Architectural Decisions

## Runtime & Frameworks
- **Host**: Windows 10/11 x64
- **.NET SDKs**: .NET 8.0/10.0/11.0 installed; targeting `net8.0-windows` for maximum stability and long-term compatibility.
- **WPF MVVM**: `CommunityToolkit.Mvvm` provides source generators for `[ObservableProperty]` and `[RelayCommand]`, minimizing boilerplate.
- **Security**: Windows DPAPI (`ProtectedData.Protect` / `Unprotect` with `DataProtectionScope.CurrentUser`) ensures keys are tied to the logged-in Windows user account and never stored in plain text.
- **RAG Architecture**:
  - `PdfPig` handles slide and page text extraction cleanly without external native C++ dependencies.
  - Chunk metadata keeps track of `PageOrSlideNumber` and `DocumentTitle`.
  - In addition to online embeddings (Gemini/OpenAI), an in-memory TF-IDF/BM25 Cosine Similarity fallback ensures offline document querying even if no API key has embedding credits.
- **Providers**:
  - `Gemini`: `v1beta/models/{model}:generateContent` and `:streamGenerateContent`.
  - `OpenAiCompatible`: `v1/chat/completions` (OpenAI, Groq, Ollama `http://localhost:11434/v1`).
  - `Claude`: `v1/messages` with `anthropic-version: 2023-06-01`.
