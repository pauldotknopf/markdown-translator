using System;
using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownTranslator.Impl
{
    public class MarkdownTransformer : IMarkdownTransformer
    {
        public string TransformMarkdown(string input, MarkdownPipeline pipeline, Func<string, string> func)
        {
            using (var writer = new StringWriter())
            {
                var renderer = new ReplacementRenderer(writer, input, func);
                var document = Markdown.Parse(input, pipeline);
                renderer.Render(document);
                return writer.ToString();
            }
        }

        class ReplacementRenderer : TextRendererBase<ReplacementRenderer>
        {
            public ReplacementRenderer(TextWriter writer, string originalMarkdown, Func<string, string> func) : base(writer)
            {
                OriginalMarkdown = originalMarkdown;
                ObjectRenderers.Add(new ContainerBlockRenderer());
                ObjectRenderers.Add(new ContainerInlineRenderer(func));
                ObjectRenderers.Add(new LeafBlockRenderer());
                ObjectRenderers.Add(new LiteralInlineRenderer());
                ObjectRenderers.Add(new LeafInlineRenderer());
            }
            
            string OriginalMarkdown { get; }

            class ContainerBlockRenderer : MarkdownObjectRenderer<ReplacementRenderer, ContainerBlock>
            {
                protected override void Write(ReplacementRenderer renderer, ContainerBlock obj)
                {
                    renderer.WriteChildren(obj);
                }
            }

            class ContainerInlineRenderer : MarkdownObjectRenderer<ReplacementRenderer, ContainerInline>
            {
                readonly Func<string, string> _func;

                public ContainerInlineRenderer(Func<string, string> func)
                {
                    _func = func;
                }
                
                protected override void Write(ReplacementRenderer renderer, ContainerInline obj)
                {
                    var start = obj.FirstChild.Span.Start;
                    var length = obj.LastChild.Span.End + 1 - start;
                    var originalMarkdown = renderer.OriginalMarkdown.Substring(start, length);
                    var newMarkdown = _func(originalMarkdown);
                    renderer.Write(newMarkdown);
                }
            }

            class LeafBlockRenderer : MarkdownObjectRenderer<ReplacementRenderer, LeafBlock>
            {
                protected override void Write(ReplacementRenderer renderer, LeafBlock obj)
                {
                    renderer.WriteLeafInline(obj);
                }
            }

            class LeafInlineRenderer : MarkdownObjectRenderer<ReplacementRenderer, LeafInline>
            {
                protected override void Write(ReplacementRenderer renderer, LeafInline obj)
                {
                    renderer.WriteLine(obj.GetType().Name);
                }
            }

            class LiteralInlineRenderer : MarkdownObjectRenderer<ReplacementRenderer, LiteralInline>
            {
                protected override void Write(ReplacementRenderer renderer, LiteralInline obj)
                {
                    renderer.Write(obj.Content);
                }
            }
        }
    }
}