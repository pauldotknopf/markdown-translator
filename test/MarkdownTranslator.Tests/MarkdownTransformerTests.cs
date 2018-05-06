using System;
using System.Collections.Generic;
using System.Text;
using Markdig;
using MarkdownTranslator.Impl;
using Moq;
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
            
            Assert.Equal("This *is* aa ~~ttestt~~ paaraagraaph.\n", result);
        }
        
        [Fact]
        public void Can_transform_without_trailing_newline()
        {
            var markdown = new StringBuilder();
            markdown.Append("This *is* a ~~test~~ paragraph.");

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
            
            Assert.Equal("This *is* aa ~~ttestt~~ paaraagraaph.\nAnd aa new line.\n", result);
        }

        [Fact]
        public void Can_transform_multiple_paragraphs()
        {
            var moc = new Mock<FuncMoq>(MockBehavior.Strict);
            moc.Setup(x => x.Translate("First paragraph.")).Returns("First paragraph.");
            moc.Setup(x => x.Translate("Next paragraph.")).Returns("Next paragraph.");
            moc.Setup(x => x.Translate("0")).Returns("0");
            moc.Setup(x => x.Translate("1")).Returns("1");

            var markdown = new StringBuilder();
            markdown.AppendLine("First paragraph.");
            markdown.AppendLine("");
            markdown.AppendLine("Next paragraph.");

            var result = _markdownTransformer.TransformMarkdown(markdown.ToString(),
                _markdownPipeline,
                value => moc.Object.Translate(value));
            
            moc.Verify(x => x.Translate("First paragraph."), Times.Exactly(1));
            moc.Verify(x => x.Translate("Next paragraph."), Times.Exactly(1));
            
            Assert.Equal(markdown.ToString(), result);
        }

        [Fact]
        public void Can_translate_cells_in_pipe_table()
        {
            var moc = new Mock<FuncMoq>(MockBehavior.Strict);
            moc.Setup(x => x.Translate("A ")).Returns("A ");
            moc.Setup(x => x.Translate(" B")).Returns(" B");
            moc.Setup(x => x.Translate("0")).Returns("0");
            moc.Setup(x => x.Translate("1")).Returns("1");

            var markdown = new StringBuilder();
            markdown.AppendLine("A | B");
            markdown.AppendLine("-|-");
            markdown.AppendLine("0|1");

            var result = _markdownTransformer.TransformMarkdown(markdown.ToString(),
                _markdownPipeline,
                value => moc.Object.Translate(value));
            
            moc.Verify(x => x.Translate("A "), Times.Exactly(1));
            moc.Verify(x => x.Translate(" B"), Times.Exactly(1));
            moc.Verify(x => x.Translate("0"), Times.Exactly(1));
            moc.Verify(x => x.Translate("1"), Times.Exactly(1));
            
            Assert.Equal(markdown.ToString(), result);
        }

        public class FuncMoq
        {
            public virtual string Translate(string input)
            {
                return input;
            }
        }
    }
}