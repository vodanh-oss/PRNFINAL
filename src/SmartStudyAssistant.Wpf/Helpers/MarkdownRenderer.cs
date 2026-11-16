using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SmartStudyAssistant.Wpf.Helpers;

public static class MarkdownRenderer
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(MarkdownRenderer),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StackPanel panel)
        {
            RenderMarkdown(panel, e.NewValue as string ?? string.Empty);
        }
    }

    public static void RenderMarkdown(StackPanel panel, string rawMarkdown)
    {
        panel.Children.Clear();
        if (string.IsNullOrWhiteSpace(rawMarkdown)) return;

        // 1. Normalize common LaTeX math and arrows to readable Unicode
        var text = NormalizeSymbols(rawMarkdown);

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        var primaryBrush = Application.Current?.TryFindResource("PrimaryBrush") as Brush 
                           ?? new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB));
        var textPrimaryBrush = Application.Current?.TryFindResource("TextPrimaryBrush") as Brush 
                              ?? new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A));
        var textSecondaryBrush = Application.Current?.TryFindResource("TextSecondaryBrush") as Brush 
                                ?? new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69));
        var borderBrush = Application.Current?.TryFindResource("SurfaceBorderBrush") as Brush 
                          ?? new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
        var citationTextBrush = Application.Current?.TryFindResource("CitationBadgeTextBrush") as Brush 
                               ?? primaryBrush;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            // Empty line -> spacing
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                panel.Children.Add(new FrameworkElement { Height = 6 });
                continue;
            }

            // Horizontal rule: --- or ***
            if (trimmed == "---" || trimmed == "***" || trimmed == "___")
            {
                var rule = new Border
                {
                    Height = 1,
                    Background = borderBrush,
                    Margin = new Thickness(0, 8, 0, 8)
                };
                panel.Children.Add(rule);
                continue;
            }

            // Heading 1: # Title
            if (trimmed.StartsWith("# ") && !trimmed.StartsWith("## "))
            {
                var tb = CreateTextBlock(18, FontWeights.Bold, primaryBrush, new Thickness(0, 10, 0, 4));
                AddFormattedInlines(tb, trimmed.Substring(2).Trim(), primaryBrush, citationTextBrush);
                panel.Children.Add(tb);
                continue;
            }

            // Heading 2: ## Title
            if (trimmed.StartsWith("## ") && !trimmed.StartsWith("### "))
            {
                var tb = CreateTextBlock(16, FontWeights.Bold, textPrimaryBrush, new Thickness(0, 8, 0, 4));
                AddFormattedInlines(tb, trimmed.Substring(3).Trim(), textPrimaryBrush, citationTextBrush);
                panel.Children.Add(tb);
                continue;
            }

            // Heading 3: ### Title
            if (trimmed.StartsWith("### ") && !trimmed.StartsWith("#### "))
            {
                var tb = CreateTextBlock(14.5, FontWeights.Bold, primaryBrush, new Thickness(0, 6, 0, 3));
                AddFormattedInlines(tb, trimmed.Substring(4).Trim(), primaryBrush, citationTextBrush);
                panel.Children.Add(tb);
                continue;
            }

            // Heading 4: #### Title
            if (trimmed.StartsWith("#### "))
            {
                var tb = CreateTextBlock(13.5, FontWeights.SemiBold, textPrimaryBrush, new Thickness(0, 5, 0, 2));
                AddFormattedInlines(tb, trimmed.Substring(5).Trim(), textPrimaryBrush, citationTextBrush);
                panel.Children.Add(tb);
                continue;
            }

            // Bullet list: * text or - text
            if ((trimmed.StartsWith("* ") || trimmed.StartsWith("- ") || trimmed.StartsWith("+ ")) && trimmed.Length > 2)
            {
                var itemContent = trimmed.Substring(2).Trim();
                var itemGrid = CreateListRow("•", itemContent, textPrimaryBrush, primaryBrush, citationTextBrush, isBullet: true);
                panel.Children.Add(itemGrid);
                continue;
            }

            // Numbered list: 1. text or 2) text
            var numberMatch = Regex.Match(trimmed, @"^(\d+[\.\)])\s+(.+)$");
            if (numberMatch.Success)
            {
                var prefix = numberMatch.Groups[1].Value;
                var itemContent = numberMatch.Groups[2].Value;
                var itemGrid = CreateListRow(prefix, itemContent, textPrimaryBrush, primaryBrush, citationTextBrush, isBullet: false);
                panel.Children.Add(itemGrid);
                continue;
            }

            // Standard Paragraph
            var p = CreateTextBlock(13.5, FontWeights.Normal, textPrimaryBrush, new Thickness(0, 2, 0, 3));
            AddFormattedInlines(p, trimmed, textPrimaryBrush, citationTextBrush);
            panel.Children.Add(p);
        }
    }

    private static TextBlock CreateTextBlock(double fontSize, FontWeight weight, Brush foreground, Thickness margin)
    {
        return new TextBlock
        {
            FontSize = fontSize,
            FontWeight = weight,
            Foreground = foreground,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = fontSize * 1.45,
            Margin = margin
        };
    }

    private static Grid CreateListRow(string marker, string content, Brush textBrush, Brush primaryBrush, Brush citationBrush, bool isBullet)
    {
        var grid = new Grid { Margin = new Thickness(0, 2, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var markerTb = new TextBlock
        {
            Text = marker,
            FontSize = isBullet ? 15 : 13,
            FontWeight = isBullet ? FontWeights.Bold : FontWeights.SemiBold,
            Foreground = primaryBrush,
            Margin = new Thickness(isBullet ? 4 : 0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(markerTb, 0);
        grid.Children.Add(markerTb);

        var contentTb = new TextBlock
        {
            FontSize = 13.5,
            Foreground = textBrush,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 13.5 * 1.45,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(contentTb, 1);
        AddFormattedInlines(contentTb, content, textBrush, citationBrush);
        grid.Children.Add(contentTb);

        return grid;
    }

    private static void AddFormattedInlines(TextBlock tb, string rawText, Brush defaultBrush, Brush citationBrush)
    {
        if (string.IsNullOrEmpty(rawText)) return;

        // Regex matches:
        // Group 1: **bold**
        // Group 2: *italic*
        // Group 3: `code`
        // Group 4: [Slide/Trang X - ...] citation
        var regex = new Regex(@"(\*\*(?<bold>[^\*]+?)\*\*)|(\*(?<italic>[^\*]+?)\*)|(`(?<code>[^`]+?)`)|(?<citation>\[Slide/Trang\s+[^\]]+?\])");

        int lastIndex = 0;
        foreach (Match match in regex.Matches(rawText))
        {
            // Plain text before match
            if (match.Index > lastIndex)
            {
                var plain = rawText.Substring(lastIndex, match.Index - lastIndex);
                tb.Inlines.Add(new Run(plain) { Foreground = defaultBrush });
            }

            if (match.Groups["bold"].Success)
            {
                var boldText = match.Groups["bold"].Value;
                tb.Inlines.Add(new Bold(new Run(boldText)) { Foreground = defaultBrush });
            }
            else if (match.Groups["italic"].Success)
            {
                var italicText = match.Groups["italic"].Value;
                tb.Inlines.Add(new Italic(new Run(italicText)) { Foreground = defaultBrush });
            }
            else if (match.Groups["code"].Success)
            {
                var codeText = match.Groups["code"].Value;
                var run = new Run(codeText)
                {
                    FontFamily = new FontFamily("Consolas"),
                    Foreground = defaultBrush,
                    Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9))
                };
                tb.Inlines.Add(run);
            }
            else if (match.Groups["citation"].Success)
            {
                var citText = match.Groups["citation"].Value;
                tb.Inlines.Add(new Run(citText)
                {
                    Foreground = citationBrush,
                    FontWeight = FontWeights.SemiBold
                });
            }

            lastIndex = match.Index + match.Length;
        }

        // Remaining plain text after last match
        if (lastIndex < rawText.Length)
        {
            var remaining = rawText.Substring(lastIndex);
            tb.Inlines.Add(new Run(remaining) { Foreground = defaultBrush });
        }
    }

    private static string NormalizeSymbols(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return input
            // Arrows
            .Replace("$\\rightarrow$", "→")
            .Replace("\\rightarrow", "→")
            .Replace("$\\to$", "→")
            .Replace("\\to", "→")
            .Replace("$\\leftarrow$", "←")
            .Replace("\\leftarrow", "←")
            .Replace("$\\Rightarrow$", "⇒")
            .Replace("\\Rightarrow", "⇒")
            .Replace("$\\Leftarrow$", "⇐")
            .Replace("\\Leftarrow", "⇐")
            .Replace("$\\leftrightarrow$", "↔")
            .Replace("\\leftrightarrow", "↔")
            // Comparisons & Math
            .Replace("$\\le$", "≤")
            .Replace("\\le", "≤")
            .Replace("$\\leq$", "≤")
            .Replace("\\leq", "≤")
            .Replace("$\\ge$", "≥")
            .Replace("\\ge", "≥")
            .Replace("$\\geq$", "≥")
            .Replace("\\geq", "≥")
            .Replace("$\\neq$", "≠")
            .Replace("\\neq", "≠")
            .Replace("$\\approx$", "≈")
            .Replace("\\approx", "≈")
            .Replace("$\\times$", "×")
            .Replace("\\times", "×")
            .Replace("$\\div$", "÷")
            .Replace("\\div", "÷")
            .Replace("$\\pm$", "±")
            .Replace("\\pm", "±");
    }
}
