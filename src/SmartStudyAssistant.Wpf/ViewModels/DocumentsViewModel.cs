using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;
using SmartStudyAssistant.Core.Services.Parsers;
using SmartStudyAssistant.Core.Services.Rag;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;
    private readonly List<IDocumentParser> _parsers;
    private readonly TextChunker _chunker;

    [ObservableProperty]
    private ObservableCollection<DocumentItem> _documents = new();

    [ObservableProperty]
    private DocumentItem? _selectedDocument;

    [ObservableProperty]
    private ObservableCollection<DocumentChunk> _chunksForSelectedDoc = new();

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _processingProgress;

    [ObservableProperty]
    private string _processingStatus = string.Empty;

    public DocumentsViewModel(IDatabaseService databaseService, AiServiceCoordinator aiCoordinator)
    {
        _databaseService = databaseService;
        _aiCoordinator = aiCoordinator;

        _parsers = new List<IDocumentParser>
        {
            new PdfDocumentParser(),
            new DocxDocumentParser(),
            new TxtDocumentParser()
        };

        _chunker = new TextChunker();
    }

    public async Task InitializeAsync()
    {
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    public async Task LoadDocumentsAsync()
    {
        var list = await _databaseService.GetDocumentsAsync();
        Documents = new ObservableCollection<DocumentItem>(list);
        if (SelectedDocument == null && Documents.Count > 0)
        {
            SelectedDocument = Documents[0];
        }
    }

    partial void OnSelectedDocumentChanged(DocumentItem? value)
    {
        if (value != null)
        {
            _ = LoadChunksForSelectedDocumentAsync(value.Id);
        }
        else
        {
            ChunksForSelectedDoc.Clear();
        }
    }

    private async Task LoadChunksForSelectedDocumentAsync(string documentId)
    {
        var chunks = await _databaseService.GetChunksByDocumentIdAsync(documentId);
        ChunksForSelectedDoc = new ObservableCollection<DocumentChunk>(chunks);
    }

    [RelayCommand]
    private async Task BrowseAndAddFilesAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Chọn slide bài giảng hoặc đề cương ôn tập",
            Filter = "Tài liệu học tập (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt|Tất cả tệp (*.*)|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await ProcessFilesAsync(openFileDialog.FileNames);
        }
    }

    [RelayCommand]
    private async Task LoadSampleDocumentAsync()
    {
        IsProcessing = true;
        ProcessingStatus = "Đang nạp đề cương & slide bài giảng mẫu...";
        try
        {
            await _databaseService.SeedSampleDataAsync(force: true);
            await LoadDocumentsAsync();
            ProcessingStatus = "✓ Đã nạp thành công bài giảng mẫu!";
        }
        catch (Exception ex)
        {
            ProcessingStatus = "✗ Lỗi: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task ProcessFilesAsync(IEnumerable<string> filePaths)
    {
        IsProcessing = true;
        ProcessingProgress = 0;
        var files = filePaths.ToList();
        int total = files.Count;
        int current = 0;

        try
        {
            foreach (var filePath in files)
            {
                current++;
                var ext = Path.GetExtension(filePath);
                var parser = _parsers.FirstOrDefault(p => p.CanParse(ext));
                if (parser == null) continue;

                var fileName = Path.GetFileName(filePath);
                ProcessingStatus = $"Đang đọc: {fileName} ({current}/{total})...";

                var parsed = await parser.ParseAsync(filePath);
                var docItem = new DocumentItem
                {
                    FileName = fileName,
                    FilePath = filePath,
                    FileType = ext.ToLowerInvariant(),
                    TotalPages = parsed.TotalPages,
                    FileSizeBytes = new FileInfo(filePath).Length,
                    IsSelectedForRag = true
                };

                ProcessingStatus = $"Đang phân đoạn (chunking) cho {fileName}...";
                var chunks = _chunker.CreateChunks(docItem.Id, parsed);
                docItem.TotalChunks = chunks.Count;

                // Check if active embedding provider is available (Gemini / OpenAI / Ollama)
                bool hasEmbeddingProvider = _aiCoordinator.HasActiveEmbeddingProvider();
                if (!hasEmbeddingProvider)
                {
                    // Instant vector indexing via internal TF-IDF (takes < 0.1s)
                    ProcessingStatus = $"Lập chỉ mục nhanh (Vector TF-IDF) cho {chunks.Count} đoạn ({fileName})...";
                    ProcessingProgress = 100;
                }
                else
                {
                    // Parallel batch embedding with 4s timeout per chunk
                    ProcessingStatus = $"Đang lập chỉ mục vector embedding cho {chunks.Count} đoạn ({fileName})...";
                    int completedChunks = 0;
                    var parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 6),
                        CancellationToken = CancellationToken.None
                    };

                    await Parallel.ForEachAsync(chunks, parallelOptions, async (chunk, _) =>
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                        try
                        {
                            chunk.Embedding = await _aiCoordinator.GenerateEmbeddingAsync(chunk.TextContent, cts.Token);
                        }
                        catch
                        {
                            chunk.Embedding = null;
                        }

                        var done = Interlocked.Increment(ref completedChunks);
                        ProcessingProgress = (double)done / chunks.Count * 100.0;
                    });
                }

                await _databaseService.SaveDocumentAsync(docItem);
                await _databaseService.SaveChunksAsync(chunks);
            }

            ProcessingStatus = "✓ Hoàn tất trích xuất và lập chỉ mục tài liệu!";
            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = "Lỗi xử lý tài liệu: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ToggleRagSelectionAsync(DocumentItem doc)
    {
        doc.IsSelectedForRag = !doc.IsSelectedForRag;
        await _databaseService.SaveDocumentAsync(doc);
    }

    [RelayCommand]
    private async Task DeleteDocumentAsync(DocumentItem doc)
    {
        await _databaseService.DeleteDocumentAsync(doc.Id);
        Documents.Remove(doc);
        if (SelectedDocument == doc)
        {
            SelectedDocument = Documents.FirstOrDefault();
        }
    }
}
