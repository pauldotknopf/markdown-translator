using System;
using System.Collections.Generic;
using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using MarkdownTranslator.Utilities;

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
                // Flush any remaining markdown content.
                renderer.Writer.Write(renderer.TakeNext(renderer.OriginalMarkdown.Length - renderer.LastWrittenIndex));
                return writer.ToString();
            }
        }

       
        public void ConvertMarkdownToPot(string input, MarkdownPipeline pipeline, TextWriter writer)
        {
            var translations = new List<string>();

            TransformMarkdown(input, pipeline, value =>
            {
                translations.Add(value);
                return value;
            });

            CreatePotFile(translations, writer);
        }

        public void CreatePotFile(List<string> entries, TextWriter writer)
        {
            writer.WriteLine("#, fuzzy");
            writer.WriteLine("msgid \"\"");
            writer.WriteLine("msgstr \"\"");
            writer.WriteLine("\"Project-Id-Version: PACKAGE VERSION\\n\"");
            writer.WriteLine("\"Report-Msgid-Bugs-To: \\n\"");
            writer.WriteLine("\"POT-Creation-Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mmzzz") + "\\n\"");
            writer.WriteLine("\"PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\\n\"");
            writer.WriteLine("\"Last-Translator: FULL NAME <EMAIL@ADDRESS>\\n\"");
            writer.WriteLine("\"Language-Team: LANGUAGE <LL@li.org>\\n\"");
            writer.WriteLine("\"Language: \\n\"");
            writer.WriteLine("\"MIME-Version: 1.0\\n\"");
            writer.WriteLine("\"Content-Type: text/plain; charset=CHARSET\\n\"");
            writer.WriteLine("\"Content-Transfer-Encoding: 8bit\\n\"");
            writer.WriteLine("\"Plural-Forms: nplurals=INTEGER; plural=EXPRESSION;\\n\"");

            foreach (var entry in entries)
            {
                writer.WriteLine();
                writer.WriteLine();

                WriteString(writer, "msgid", entry);
                WriteString(writer, "msgstr", string.Empty);
            }
        }

        private static void WriteString(TextWriter writer, string type, string value)
        {
            // TODO: support multi-line.
            // Escape things.
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            writer.WriteLine($"{type} \"{value}\"");
        }

        class ReplacementRenderer : MarkdownTransformRenderer
        {
            public ReplacementRenderer(TextWriter writer, string originalMarkdown, Func<string, string> func) : base(writer, originalMarkdown)
            {
                ObjectRenderers.Add(new ContainerInlineRenderer(func));
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
                    var startIndex = obj.Span.Start;
                    
                    // Make sure we flush all previous markdown before rendering this inline entry.
                    renderer.Write(renderer.TakeNext(startIndex - renderer.LastWrittenIndex));
                    
                    var originalMarkdown = renderer.TakeNext(obj.LastChild.Span.End + 1 - startIndex);
                    var newMarkdown = _func(originalMarkdown);
                    
                    renderer.Write(newMarkdown);
                }
            }
        }
    }
}