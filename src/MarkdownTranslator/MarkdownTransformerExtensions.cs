using System.IO;
using Markdig;

namespace MarkdownTranslator
{
    public static class MarkdownTransformerExtensions
    {
        public static string ConvertMarkdownToPot(this IMarkdownTransformer markdownTransformer, string input, MarkdownPipeline pipeline)
        {
            using (var writer = new StringWriter())
            {
                markdownTransformer.ConvertMarkdownToPot(input, pipeline, writer);
                return writer.ToString();
            }
        }

        public static void ConvertMarkdownToPot(this IMarkdownTransformer markdownTransformer, string input, MarkdownPipeline pipeline, string file)
        {
            using (var fileWriter = File.OpenWrite(file))
            {
                using (var writer = new StreamWriter(fileWriter))
                {
                    markdownTransformer.ConvertMarkdownToPot(input, pipeline, writer);
                }
            }
        }
    }
}