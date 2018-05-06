using System;
using System.Collections.Generic;
using System.Text;
using Markdig;
using MarkdownTranslator.Impl;
using Xunit;

namespace MarkdownTranslator.Tests
{
    public class MarkdownTransformerTests
    {
        readonly IMarkdownTransformer _markdownTransformer;
        readonly MarkdownPipeline _markdownPipeline;
        
        public MarkdownTransformerTests()
        {
            _markdownTransformer = new MarkdownTransformer();
            _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }
        
        [Fact]
        public void Can_transform_paragraph()
        {
            var markdown = new StringBuilder();
            markdown.AppendLine("This *is* a ~~test~~ paragraph.");

            var result = _markdownTransformer.TransformMarkdown(markdown.ToString(),
                _markdownPipeline,
                value => value.Replace("t", "tt").Replace("a", "aa"));
            
            Assert.Equal("This *is* aa ~~ttestt~~ paaraagraaph.", result);
        }

        [Fact]
        public void Can_transform_paragraph_with_new_lines()
        {
            var markdown = new StringBuilder();
            markdown.AppendLine("This *is* a ~~test~~ paragraph.");
            markdown.AppendLine("And a new line.");

            var result = _markdownTransformer.TransformMarkdown(markdown.ToString(),
                _markdownPipeline,
                value => value.Replace("t", "tt").Replace("a", "aa"));
            
            Assert.Equal("This *is* aa ~~ttestt~~ paaraagraaph.\nAnd aa new line.", result);
        }

        [Fact]
        public void Can_translate_cells_in_pipe_table()
        {
            
        }
    }
}