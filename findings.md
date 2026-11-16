# Findings: Smart Study Assistant Rebuild

## Architectural Decisions
1. **Database: SQL Server Management Studio (SSMS)**:
   - Target database: `SmartStudyDb` on `localhost` (default instance `MSSQLSERVER`).
   - Connection string: `Server=localhost;Database=SmartStudyDb;Integrated Security=True;TrustServerCertificate=True;`
   - Using `Microsoft.Data.SqlClient` with simple, straightforward parameterized SQL queries.
   - A standalone `database_setup.sql` script is provided so users can open SSMS and run it with F5 directly.
   - The app also automatically auto-creates the database, tables, and sample data on first run if not already present.

2. **RAG Performance**:
   - Primary indexing uses lightweight TF-IDF lexical vectors (< 0.1s for 100+ chunks).
   - Prevents hangs/freezes by never probing Ollama or remote endpoints unless explicitly requested.

3. **Flashcards Grouping**:
   - Flashcards are segregated into distinct subject decks (`PRN211`, `AI301`, `Chung`, etc.).
   - UI features a deck selector chip bar: `[ Tất cả (25) ]`, `[ PRN211 (10) ]`, `[ AI301 (10) ]`...
   - Progress bar, counter, and navigation are localized to the active deck.

4. **Code Simplicity**:
   - Direct, readable C# and XAML without superfluous layers, over-engineered reactive frameworks, or bloated boilerplate.
