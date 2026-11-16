using System.Threading;
using System.Windows.Controls;
using FluentAssertions;
using SmartStudyAssistant.Wpf.Helpers;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class MarkdownRendererTests
{
    [Fact]
    public void MarkdownRenderer_RendersParagraphWithInlinesAndArrows()
    {
        // Run on STA thread because WPF controls require STA
        var thread = new Thread(() =>
        {
            var panel = new StackPanel();
            var markdown = "Bối cảnh $\\rightarrow$ Phương pháp luận $\\rightarrow$ Kết quả **quan trọng**";

            MarkdownRenderer.RenderMarkdown(panel, markdown);

            panel.Children.Count.Should().BeGreaterThan(0);
            var tb = panel.Children[0] as TextBlock;
            tb.Should().NotBeNull();
            tb!.Inlines.Count.Should().BeGreaterThan(0);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
