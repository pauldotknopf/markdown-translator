# MarkdownTranslator

A library that helps you extract and replace raw content of markdown documents, with awareness of the syntax.

[![MarkdownTranslator](https://img.shields.io/nuget/v/MarkdownTranslator.svg?style=flat-square&label=MarkdownTranslator)](http://www.nuget.org/packages/MarkdownTranslator/)
[![Build Status](https://travis-ci.com/pauldotknopf/markdown-translator.svg?branch=develop)](https://travis-ci.com/pauldotknopf/markdown-translator)

The drive of this is to generate POT files from markdown files. Then, to generate translated markdown documents based off of the results PO file.

This is library-only. There will probably (eventually) be a CI tool to do something like ```markdown-translator input.md output.pot``` and ```markdown-translator translation.po output.md```.
