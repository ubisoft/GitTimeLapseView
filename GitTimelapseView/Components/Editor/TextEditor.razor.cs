using BlazorMonaco;
using BlazorMonaco.Editor;
using GitTimelapseView.Actions;
using GitTimelapseView.Data;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GitTimelapseView;

public partial class TextEditor
{
    private CodeEditor? _editor;
    private TextModel? _model;
    private string _previousValue = string.Empty;
    private List<string> _currentDecorations = [];
    private bool _didInit;

    [Inject] private TimelapseService TimelapseService { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private ThemingService ThemingService { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        TimelapseService.CurrentFileRevisionIndexChanged += async (_, args) =>
        {
            if (args.Reason == FileRevisionIndexChangeReason.Explicit)
            {
                await UpdateModelAsync();
            }
        };
        TimelapseService.CurrentCommitChanged += async (_, args) =>
        {
            if (args.Reason == CommitChangeReason.Explicit)
            {
                await UpdateDecorationsAsync();
            }
        };
        return base.OnInitializedAsync();
    }

    private async Task UpdateModelAsync()
    {
        if (_editor == null || !_didInit || TimelapseService.CurrentFileRevision == null)
            return;

        var newValue = string.Join("\n", TimelapseService.CurrentFileRevision.Blocks.Select(x => x.Text));
        if (string.Equals(newValue, _previousValue, StringComparison.Ordinal))
            return;

        if (_model == null)
        {
            try
            {
                var language = "plaintext";
                if (TimelapseService.FilePath is { } filePath)
                {
                    var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                    if (FileExtensions.ExtensionsToLanguage.TryGetValue(extension, out var l))
                    {
                        language = l;
                    }
                }
                _model = await Global.CreateModel(JsRuntime, newValue, language: language);
                await _editor.SetModel(_model);
                if (TimelapseService.InitialLineNumber != null)
                {
                    await _editor.RevealLineInCenter(TimelapseService.InitialLineNumber.Value, ScrollType.Smooth).ConfigureAwait(false);
                    await SelectLineNumberAsync(TimelapseService.InitialLineNumber.Value).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                _model = await _editor.GetModel().ConfigureAwait(false);
            }
        }

        await _model.SetValue(newValue).ConfigureAwait(false);
        _previousValue = newValue;

        await UpdateDecorationsAsync(_model).ConfigureAwait(false);
    }

    private StandaloneEditorConstructionOptions EditorConstructionOptions(Editor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            AutomaticLayout = true,
            GlyphMargin = true,
            ReadOnly = true,
            ScrollBeyondLastLine = false,
            Theme = ThemingService.Theme.MonacoTheme,
        };
    }

    private async Task EditorOnDidInit()
    {
        _didInit = true;
        await UpdateModelAsync();
    }

    private async Task EditorOnDidScroll(ScrollEvent scrollEvent)
    {
        await JsRuntime.InvokeVoidAsync($"setScroll", ".editor-custommargin", scrollEvent.ScrollTop, scrollEvent.ScrollHeight);
    }

    private Task EditorOnMouseDown(EditorMouseEvent mouseEvent)
    {
        if (mouseEvent.Target.Type == MouseTargetType.SCROLLBAR || mouseEvent.Target?.Position == null)
            return Task.CompletedTask;

        return SelectLineNumberAsync(mouseEvent.Target.Position.LineNumber);
    }

    private async Task SelectLineNumberAsync(int lineNumber)
    {
        var block = TimelapseService.CurrentFileRevision?.Blocks.FirstOrDefault(x => lineNumber >= x.StartLine && lineNumber < x.StartLine + x.LineCount);
        if (block != null)
        {
            await new SelectCommitAction(block.InitialCommit).ExecuteAsync();
        }
    }

    private async Task UpdateDecorationsAsync(TextModel? model = null)
    {
        if (_editor == null || !_didInit || TimelapseService.CurrentFileRevision == null)
            return;

        model ??= await _editor.GetModel().ConfigureAwait(false);

        var index = 0;
        var newDecorations = new List<ModelDeltaDecoration>();
        foreach (var block in TimelapseService.CurrentFileRevision.Blocks)
        {
            var isPair = index % 2 == 0;
            var isCommitSelected = string.Equals(block.InitialCommit.Id, TimelapseService.CurrentCommit?.Id, StringComparison.Ordinal) ||
                                   string.Equals(block.FinalCommit.Id, TimelapseService.CurrentCommit?.Id, StringComparison.Ordinal);
            newDecorations.Add(new ModelDeltaDecoration
            {
                Range = new BlazorMonaco.Range(block.StartLine, 0, block.StartLine + block.LineCount - 1, 1),
                Options = new ModelDecorationOptions { IsWholeLine = true, ClassName = isCommitSelected ? "editorHighlighted" : null, GlyphMarginClassName = isPair ? "editorBlockMargin" : "editorBlockMarginAlternate" },
            });
            index++;
        }

        _currentDecorations = await model.DeltaDecorations(_currentDecorations, newDecorations, null).ConfigureAwait(false);
    }
}
