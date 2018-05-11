using System;
using System.Diagnostics;
using System.IO;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownTranslator.Utilities
{
    public class MarkdownTransformRenderer : TextRendererBase<MarkdownTransformRenderer>
    {
        public MarkdownTransformRenderer(TextWriter writer, string originalMarkdown) : base(writer)
        {
            OriginalMarkdown = originalMarkdown;
            ObjectRenderers.Add(new ContainerBlockRenderer());
            ObjectRenderers.Add(new LeafBlockRenderer());
        }

        // ReSharper disable ArrangeTypeMemberModifiers
        // ReSharper disable InconsistentNaming
        public readonly string OriginalMarkdown;
        public int LastWrittenIndex;
        // ReSharper restore InconsistentNaming
        // ReSharper restore ArrangeTypeMemberModifiers

        public string TakeNext(int length)
        {
            if (length == 0) return null;
            var result = OriginalMarkdown.Substring(LastWrittenIndex, length);
            LastWrittenIndex += length;
            return result;
        }

        class ContainerBlockRenderer : MarkdownObjectRenderer<MarkdownTransformRenderer, ContainerBlock>
        {
            protected override void Write(MarkdownTransformRenderer renderer, ContainerBlock obj)
            {
                renderer.WriteChildren(obj);
            }
        }

        class LeafBlockRenderer : MarkdownObjectRenderer<MarkdownTransformRenderer, LeafBlock>
        {
            protected override void Write(MarkdownTransformRenderer renderer, LeafBlock obj)
            {
                renderer.WriteLeafInline(obj);
            }
        }
    }
}