# Graph Report - PRNFINAL  (2026-11-16)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 1036 nodes · 1867 edges · 53 communities (43 shown, 10 thin omitted)
- Extraction: 82% EXTRACTED · 18% INFERRED · 0% AMBIGUOUS · INFERRED: 331 edges (avg confidence: 0.69)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 32
- Community 33
- Community 34
- Community 35
- Community 36
- Community 37
- Community 39
- Community 40
- Community 41
- Community 42
- Community 43
- Community 44
- Community 45
- Community 46
- Community 47
- Community 49
- Community 50
- Community 51

## God Nodes (most connected - your core abstractions)
1. `UserControl` - 46 edges
2. `ChatViewModel` - 41 edges
3. `ChatMessage` - 37 edges
4. `UserControl` - 37 edges
5. `FlashcardsViewModel` - 36 edges
6. `ProgressViewModel` - 36 edges
7. `UserControl` - 35 edges
8. `DocumentsViewModel` - 34 edges
9. `IDatabaseService` - 33 edges
10. `UserControl` - 32 edges

## Surprising Connections (you probably didn't know these)
- `Phase 6: User Story 4 - Bộ công cụ Năng suất Sinh viên (Priority: P2)` --references--> `StudyNote`  [INFERRED]
  specs/001-smart-study-assistant/tasks.md → src/SmartStudyAssistant.Core/Models/ProductivityModels.cs
- `II. Pluggable Multi-Provider AI Engine (`IAiProvider`)` --references--> `IAiProvider`  [INFERRED]
  .specify/memory/constitution.md → src/SmartStudyAssistant.Core/Services/Ai/IAiProvider.cs
- `AI Provider Engine` --references--> `IAiProvider`  [INFERRED]
  specs/001-smart-study-assistant/spec.md → src/SmartStudyAssistant.Core/Services/Ai/IAiProvider.cs
- `Phases & Status` --references--> `DocumentItem`  [INFERRED]
  task_plan.md → src/SmartStudyAssistant.Core/Models/DocumentItem.cs
- `Phases & Status` --references--> `Flashcard`  [INFERRED]
  task_plan.md → src/SmartStudyAssistant.Core/Models/ProductivityModels.cs

## Import Cycles
- None detected.

## Communities (53 total, 10 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (42): AiProviderConfig, ApiKey, BaseUrl, DisplayName, IsDefault, IsEnabled, ModelName, ProviderType (+34 more)

### Community 1 - "Community 1"
Cohesion: 0.09
Nodes (8): SmartStudyAssistant.Core.Services.Database, SmartStudyAssistant.Core.Services.Rag, SmartStudyAssistant.Core.Services.Parsers, SmartStudyAssistant.Wpf.ViewModels, SmartStudyAssistant.Core.Services.Security, SmartStudyAssistant.Core.Services.Ai, SmartStudyAssistant.Core.Models, SmartStudyAssistant.Tests

### Community 2 - "Community 2"
Cohesion: 0.07
Nodes (18): SmartStudyAssistant.Core.Services.Productivity, Phase 8: Kiểm thử Tự động & Nghiệm thu (Superpowers TDD), PomodoroMode, LongBreak, ShortBreak, Work, PomodoroStateMachine, CompletedSessions (+10 more)

### Community 3 - "Community 3"
Cohesion: 0.06
Nodes (43): AddChecklistItemCommand, AddNoteCommand, AddSubjectCommand, Code, DataContext.DeleteNoteCommand, DataContext.ToggleChecklistItemCommand, IsCompleted, Name (+35 more)

### Community 4 - "Community 4"
Cohesion: 0.05
Nodes (32): ActiveProviderType, AppTitle, ChatVm, TabToVisibilityConverter, DocumentsVm, FlashcardsVm, NavigateCommand, PomodoroVm (+24 more)

### Community 5 - "Community 5"
Cohesion: 0.06
Nodes (39): AddCardCommand, AiGenerateTopic, CurrentCard.Back, CurrentCard.Front, CurrentCard.SubjectName, CurrentCardIndex, Flashcards.Count, FlipCardCommand (+31 more)

### Community 6 - "Community 6"
Cohesion: 0.07
Nodes (35): DisplayName, GlobalStatusMessage, IsBusy, IsDefault, ModelName, Providers, SaveSettingsCommand, SelectedProvider (+27 more)

### Community 7 - "Community 7"
Cohesion: 0.05
Nodes (37): args, inputs, integration, spec, speckit_review_plan_verdict, speckit_review_spec_verdict, default, prompt (+29 more)

### Community 8 - "Community 8"
Cohesion: 0.08
Nodes (27): CompletedWorkSessionsToday, IsRunning, ModeTitle, ResetCommand, SetModeCommand, StartPauseCommand, TimeDisplay, TotalFocusMinutesToday (+19 more)

### Community 9 - "Community 9"
Cohesion: 0.06
Nodes (32): BadgeText, Citations, Citations.Count, CloseCitationModalCommand, RoleToAlignmentConverter, RoleToBackgroundConverter, CreateNewSessionCommand, CurrentProviderName (+24 more)

### Community 11 - "Community 11"
Cohesion: 0.10
Nodes (16): DocxDocumentParser, DocumentPage, PageNumber, Text, IDocumentParser, ParsedDocumentResult, FileName, Pages (+8 more)

### Community 12 - "Community 12"
Cohesion: 0.16
Nodes (19): Find-SpecifyRoot(), Format-SpecKitCommand(), Get-CurrentBranch(), Get-FeaturePathsEnv(), Get-InvokeSeparator(), Get-NormalizedPriority(), Get-Python3Command(), Get-RepoRoot() (+11 more)

### Community 13 - "Community 13"
Cohesion: 0.07
Nodes (25): 1. Summary, 2. Technical Context, 3. Constitution Alignment, 4. Architecture & Project Layout, Implementation Plan: Smart Study Assistant (WPF .NET 8), 1. Executive Summary & Objective, 2. User Scenarios & Prioritized User Stories, 3. Detailed Functional Requirements (+17 more)

### Community 14 - "Community 14"
Cohesion: 0.07
Nodes (26): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Implementation for User Story 1, Implementation for User Story 2, Implementation for User Story 3, Implementation Strategy, Incremental Delivery, MVP First (User Story 1 Only) (+18 more)

### Community 15 - "Community 15"
Cohesion: 0.08
Nodes (25): 1. Initialize Analysis Context, 2. Load Artifacts (Progressive Disclosure), 3. Build Semantic Models, 4. Detection Passes (Token-Efficient Analysis), 5. Severity Assignment, 6. Produce Compact Analysis Report, 7. Provide Next Actions, 8. Offer Remediation (+17 more)

### Community 16 - "Community 16"
Cohesion: 0.11
Nodes (8): ChatSession, CreatedAt, Id, IsRagEnabled, LastModifiedAt, PreferredProvider, Title, IDatabaseService

### Community 17 - "Community 17"
Cohesion: 0.11
Nodes (18): ChecklistItem, DueDate, Id, IsCompleted, Title, PomodoroStats, CompletedSessions, Id (+10 more)

### Community 18 - "Community 18"
Cohesion: 0.10
Nodes (18): coverlet.collector (6.0.0), DocumentFormat.OpenXml (3.2.0), FluentAssertions (6.12.2), Microsoft.Data.Sqlite (8.0.11), Microsoft.Extensions.DependencyInjection (8.0.1), Microsoft.NET.Test.Sdk (17.8.0), PdfPig (0.1.9), System.Security.Cryptography.ProtectedData (8.0.0) (+10 more)

### Community 19 - "Community 19"
Cohesion: 0.09
Nodes (21): BrowseAndAddFilesCommand, ChunkIndex, ChunksForSelectedDoc, ChunksForSelectedDoc.Count, DataContext.DeleteDocumentCommand, DataContext.ToggleRagSelectionCommand, Documents, FileName (+13 more)

### Community 20 - "Community 20"
Cohesion: 0.19
Nodes (6): SmartStudyAssistant.Wpf.Converters, BooleanToVisibilityConverter, InverseBooleanConverter, RoleToAlignmentConverter, RoleToBackgroundConverter, TabToVisibilityConverter

### Community 21 - "Community 21"
Cohesion: 0.10
Nodes (18): ChatViewModel, CloseCitationModalCommand, CreateNewSessionCommand, CurrentProviderName, CurrentSession, DeleteSessionCommand, InspectCitationCommand, InspectedCitation (+10 more)

### Community 22 - "Community 22"
Cohesion: 0.13
Nodes (12): Flashcard, Back, CreatedAt, EaseLevel, Front, Hint, Id, IsDueForReview (+4 more)

### Community 23 - "Community 23"
Cohesion: 0.12
Nodes (15): 1. Initialize Convergence Context, 2. Load Artifacts (Progressive Disclosure), 3. Build the Intent Inventory, 4. Assess the Codebase and Classify Findings, 5. Assign Severity, 6. Present the In-Session Findings Summary, 7. Append Convergence Tasks (or report converged), 8. Provide Next Actions (Handoff) (+7 more)

### Community 24 - "Community 24"
Cohesion: 0.20
Nodes (6): SmartStudyAssistant.Wpf.Views, ChatView, FlashcardsView, PomodoroView, ProgressView, SettingsView

### Community 25 - "Community 25"
Cohesion: 0.18
Nodes (8): StudyNote, Content, CreatedAt, Id, SubjectName, Tags, Title, UpdatedAt

### Community 26 - "Community 26"
Cohesion: 0.15
Nodes (12): Assumptions, Edge Cases, Feature Specification: [FEATURE NAME], Functional Requirements, Key Entities *(include if feature involves data)*, Measurable Outcomes, Requirements *(mandatory)*, Success Criteria *(mandatory)* (+4 more)

### Community 27 - "Community 27"
Cohesion: 0.17
Nodes (11): DocumentsViewModel, BrowseAndAddFilesCommand, ChunksForSelectedDoc, DeleteDocumentCommand, Documents, IsProcessing, LoadDocumentsCommand, ProcessingProgress (+3 more)

### Community 28 - "Community 28"
Cohesion: 0.18
Nodes (10): Completion Report, Done When, Key rules, Mandatory Post-Execution Hooks, Outline, Phase 0: Outline & Research, Phase 1: Design & Contracts, Phases (+2 more)

### Community 29 - "Community 29"
Cohesion: 0.18
Nodes (10): Completion Report, Done When, For AI Generation, Mandatory Post-Execution Hooks, Outline, Pre-Execution Checks, Quick Guidelines, Section Requirements (+2 more)

### Community 30 - "Community 30"
Cohesion: 0.18
Nodes (10): Checklist Format (REQUIRED), Completion Report, Done When, Mandatory Post-Execution Hooks, Outline, Phase Structure, Pre-Execution Checks, Task Generation Rules (+2 more)

### Community 32 - "Community 32"
Cohesion: 0.18
Nodes (10): Core Principles, Governance, I. Clean Architecture & Strict MVVM, II. Pluggable Multi-Provider AI Engine (`IAiProvider`), III. Local-First Privacy & Credential Security, IV. Transparent RAG & Verifiable Citations, Smart Study Assistant Constitution, Technology Stack Standards (+2 more)

### Community 33 - "Community 33"
Cohesion: 0.18
Nodes (10): Core Principles, Governance, [PRINCIPLE_1_NAME], [PRINCIPLE_2_NAME], [PRINCIPLE_3_NAME], [PRINCIPLE_4_NAME], [PRINCIPLE_5_NAME], [PROJECT_NAME] Constitution (+2 more)

### Community 34 - "Community 34"
Cohesion: 0.18
Nodes (11): DocumentItem, FileName, FilePath, FileSizeBytes, FileType, FormattedFileSize, Id, IsSelectedForRag (+3 more)

### Community 35 - "Community 35"
Cohesion: 0.20
Nodes (9): DocumentChunk, ChunkIndex, CreatedAt, DocumentId, DocumentName, Embedding, Id, PageOrSlideNumber (+1 more)

### Community 36 - "Community 36"
Cohesion: 0.22
Nodes (8): Complexity Tracking, Constitution Check, Documentation (this feature), Implementation Plan: [FEATURE], Project Structure, Source Code (repository root), Summary, Technical Context

### Community 37 - "Community 37"
Cohesion: 0.25
Nodes (7): Anti-Examples: What NOT To Do, Checklist Purpose: "Unit Tests for English", Example Checklist Types & Sample Items, Execution Steps, Post-Execution Checks, Pre-Execution Checks, User Input

### Community 39 - "Community 39"
Cohesion: 0.29
Nodes (6): 1. Specification-Driven Development (Spec Kit), 2. Codebase Understanding & Architecture (Graphify), 3. UI/UX & Frontend Excellence (Impeccable & UI/UX Pro Max), 4. Kỹ nghệ phần mềm & Kiểm thử (Superpowers), 5. Quản lý tiến độ (Planning with Files), Operational Guidelines & Installed Skills Protocol

### Community 40 - "Community 40"
Cohesion: 0.29
Nodes (6): Completion Report, Done When, Mandatory Post-Execution Hooks, Outline, Pre-Execution Checks, User Input

### Community 41 - "Community 41"
Cohesion: 0.29
Nodes (6): Completion Report, Done When, Mandatory Post-Execution Hooks, Outline, Pre-Execution Checks, User Input

### Community 42 - "Community 42"
Cohesion: 0.29
Nodes (6): CitationItem, BadgeText, DocumentName, PageOrSlideNumber, SimilarityScore, Snippet

### Community 43 - "Community 43"
Cohesion: 0.33
Nodes (5): Outline, Post-Execution Checks, Pre-Execution Checks, Scope Guard, User Input

### Community 44 - "Community 44"
Cohesion: 0.33
Nodes (5): border, PART_ContentHost, ResourceDictionary, Border, ScrollViewer

### Community 45 - "Community 45"
Cohesion: 0.40
Nodes (4): [Category 1], [Category 2], [CHECKLIST TYPE] Checklist: [FEATURE NAME], Notes

## Knowledge Gaps
- **305 isolated node(s):** `Gemini`, `OpenAi`, `Claude`, `Groq`, `Ollama` (+300 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 545 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ChatViewModel` connect `Community 21` to `Community 0`, `Community 1`, `Community 4`, `Community 6`, `Community 9`, `Community 42`, `Community 16`, `Community 25`?**
  _High betweenness centrality (0.091) - this node is a cross-community bridge._
- **Are the 12 inferred relationships involving `UserControl` (e.g. with `CloseCitationModalCommand` and `CreateNewSessionCommand`) actually correct?**
  _`UserControl` has 12 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Gemini`, `OpenAi`, `Claude` to the rest of the system?**
  _305 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.05512016718913271 - nodes in this community are weakly interconnected._
- **Why does `MainViewModel` connect `Community 4` to `Community 0`, `Community 1`, `Community 3`, `Community 5`, `Community 6`, `Community 8`, `Community 16`, `Community 21`, `Community 27`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Are the 18 inferred relationships involving `ChatViewModel` (e.g. with `.GetCurrentProvider()` and `CloseCitationModalCommand`) actually correct?**
  _`ChatViewModel` has 18 INFERRED edges - model-reasoned connections that need verification._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.09254901960784313 - nodes in this community are weakly interconnected._