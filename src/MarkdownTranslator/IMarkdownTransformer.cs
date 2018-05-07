using System;
using System.Collections.Generic;
using System.IO;
using Markdig;

namespace MarkdownTranslator
{
    public interface IMarkdownTransformer
    {
        string TransformMarkdown(string input, MarkdownPipeline pipeline, Func<string, string> func);

        void ConvertMarkdownToPot(string input, MarkdownPipeline pipeline, TextWriter writer);

        void CreatePotFile(List<string> entries, TextWriter writer);
    }
}