using System;
using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownTranslator.Tests
{
    public class Helpers
    {
        public static string GetMarkdownTree(string markdown, MarkdownPipeline pipeline)
        {
            using (var stringWriter = new StringWriter())
            {
                var renderer = new DebugOutputRenderer(markdown, stringWriter);
                var document = Markdig.Markdown.Parse(markdown, pipeline);
                renderer.Render(document);
                return stringWriter.ToString();
            }
        }
        
        class DebugOutputRenderer : TextRendererBase<DebugOutputRenderer>
        {
            public int Level = 0;
            
            public DebugOutputRenderer(string originalMarkdown, TextWriter writer) : base(writer)
            {
                OriginalMarkdown = originalMarkdown;
                ObjectRenderers.Add(new ContainerBlockRenderer());
                ObjectRenderers.Add(new ContainerInlineRenderer());
                ObjectRenderers.Add(new LeafBlockRenderer());
                ObjectRenderers.Add(new LiteralInlineRenderer());
                ObjectRenderers.Add(new LeafInlineRenderer());
            }
            
            string OriginalMarkdown { get; }

            IDisposable Scope(string name)
            {
                return new ScopeSession(name, this);
            }
            
            class ScopeSession : IDisposable
            {
                private readonly string _name;
                private readonly DebugOutputRenderer _renderer;

                public ScopeSession(string name, DebugOutputRenderer renderer)
                {
                    _name = name;
                    _renderer = renderer;

                    renderer.Write($"<{name}>");
                }
                
                public void Dispose()
                {
                    _renderer.Write($"</{_name}>");
                }
            }

            class ContainerBlockRenderer : MarkdownObjectRenderer<DebugOutputRenderer, ContainerBlock>
            {
                protected override void Write(DebugOutputRenderer renderer, ContainerBlock obj)
                {
                    using (renderer.Scope($"ContainerBlock.{obj.GetType().Name}"))
                    {
                        renderer.WriteChildren(obj);
                    }
                }
            }

            class ContainerInlineRenderer : MarkdownObjectRenderer<DebugOutputRenderer, ContainerInline>
            {
                protected override void Write(DebugOutputRenderer renderer, ContainerInline obj)
                {
                    using (renderer.Scope($"ContainerInline.{obj.GetType().Name}"))
                    {
                        renderer.WriteChildren(obj);
                    }
                }
            }

            class LeafBlockRenderer : MarkdownObjectRenderer<DebugOutputRenderer, LeafBlock>
            {
                protected override void Write(DebugOutputRenderer renderer, LeafBlock obj)
                {
                    using (renderer.Scope($"LeafBlock.{obj.GetType().Name}"))
                    {
                        renderer.WriteLeafInline(obj);
                    }
                }
            }

            class LeafInlineRenderer : MarkdownObjectRenderer<DebugOutputRenderer, LeafInline>
            {
                protected override void Write(DebugOutputRenderer renderer, LeafInline obj)
                {
                    using (renderer.Scope($"LeafInline.{obj.GetType().Name}"))
                    {
                        renderer.WriteLine(obj.GetType().Name);
                    }
                }
            }

            class LiteralInlineRenderer : MarkdownObjectRenderer<DebugOutputRenderer, LiteralInline>
            {
                protected override void Write(DebugOutputRenderer renderer, LiteralInline obj)
                {
                    using (renderer.Scope($"LiteralInline.{obj.GetType().Name}"))
                    {
                        renderer.Write(obj.Content);
                    }
                }
            }
        }
    }
}