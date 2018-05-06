using System;
using Markdig;

namespace MarkdownTranslator
{
    public interface IMarkdownTransformer
    {
        string TransformMarkdown(string input, MarkdownPipeline pipeline, Func<string, string> func);
    }
}