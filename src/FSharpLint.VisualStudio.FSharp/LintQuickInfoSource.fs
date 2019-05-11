namespace FSharpLint.VisualStudio.FSharp.Linting

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open Microsoft.VisualStudio.Language.Intellisense
open System.Collections.Generic
open FSharpLint.VisualStudio
open System.Windows.Controls
open System.Windows.Documents
open System.Windows
open System.Threading

type LintQuickInfoSource(buffer: ITextBuffer, viewTagAggregatorFactoryService: IViewTagAggregatorFactoryService) =
    let createInfoText (tooltips: string list) : UIElement =
        let textBlock = TextBlock() 
        tooltips |> List.iteri (fun i tooltip ->
            textBlock.Inlines.Add (Bold (Run "Lint: "))
            textBlock.Inlines.Add (Run tooltip)
            if i < tooltips.Length - 1 then
                textBlock.Inlines.Add (LineBreak()))
             
        textBlock.SetResourceReference(TextBlock.BackgroundProperty, VSColors.ToolTipBrushKey)
        textBlock.SetResourceReference(TextBlock.ForegroundProperty, VSColors.ToolTipTextBrushKey)
        upcast textBlock

    let mutable tagAggregator = None
    let getTagAggregator (textView: Editor.ITextView) =
        tagAggregator
        |> Option.getOrTry (fun _ ->
            let aggregator = viewTagAggregatorFactoryService.CreateTagAggregator<LintTag> textView
            tagAggregator <- Some aggregator
            aggregator)

    interface IAsyncQuickInfoSource with
        member __.GetQuickInfoItemAsync (session: IAsyncQuickInfoSession, cancellationToken: CancellationToken) = 
            if session.TextView.TextBuffer = buffer then
                match session.GetTriggerPoint buffer.CurrentSnapshot |> Option.ofNullable with
                | None -> Tasks.Task.FromResult<QuickInfoItem>(null)
                | Some point ->
                    let span = buffer.CurrentSnapshot.FullSpan
                    let res =
                        let tags = getTagAggregator(session.TextView).GetTags(span)
                        tags
                        |> Seq.map (fun mappedSpan -> 
                            let tooltip = mappedSpan.Tag.ToolTipContent :?> string
                            mappedSpan.Span.GetSpans buffer |> Seq.map (fun span -> span, tooltip))
                        |> Seq.concat
                        |> Seq.filter (fun (span, _) -> point.InSpan span)
                        |> Seq.toList

                    match res with
                    | [] -> Tasks.Task.FromResult<QuickInfoItem>(null)
                    | (span, _) :: _ ->
                        let applicableToSpan = buffer.CurrentSnapshot.CreateTrackingSpan (span.Span, SpanTrackingMode.EdgeExclusive)
                        let item = res |> List.map snd |> createInfoText
                        Tasks.Task.FromResult(QuickInfoItem(applicableToSpan, item))
            else
                Tasks.Task.FromResult<QuickInfoItem>(null)
        
        member __.Dispose() = tagAggregator |> Option.iter (fun x -> x.Dispose())