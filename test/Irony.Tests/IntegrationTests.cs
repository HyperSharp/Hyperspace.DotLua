﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;
using Xunit;

namespace Irony.Tests
{
    public class IntegrationTestGrammar : Grammar
    {
        public IntegrationTestGrammar()
        {
            var comment = new CommentTerminal("comment", "/*", "*/");
            base.NonGrammarTerminals.Add(comment);
            var str = new StringLiteral("str", "'", StringOptions.AllowsLineBreak);
            var stmt = new NonTerminal("stmt");
            stmt.Rule = str | Empty;
            this.Root = stmt;
        }
    }//class

    public class IntegrationTests
    {
        Grammar _grammar;
        LanguageData _language;
        Scanner _scanner;
        ParsingContext _context;
        int _state;

        private void Init(Grammar grammar)
        {
            _grammar = grammar;
            _language = new LanguageData(_grammar);
            var parser = new Parser(_language);
            _scanner = parser.Scanner;
            _context = parser.Context;
            _context.Mode = ParseMode.VsLineScan;
        }

        private void SetSource(string text)
        {
            _scanner.VsSetSource(text, 0);
        }
        private Token Read()
        {
            Token token = _scanner.VsReadToken(ref _state);
            return token;
        }

        [Fact]
        public void TestIntegration_VsScanningComment()
        {
            Init(new IntegrationTestGrammar());
            SetSource(" /*  ");
            Token token = Read();
            Assert.True(token.IsSet(TokenFlags.IsIncomplete), "Expected incomplete token (line 1)");
            token = Read();
            Assert.Null(token);
            SetSource(" comment ");
            token = Read();
            Assert.True(token.IsSet(TokenFlags.IsIncomplete), "Expected incomplete token (line 2)");
            token = Read();
            Assert.Null(token);
            SetSource(" */ /*x*/");
            token = Read();
            Assert.False(token.IsSet(TokenFlags.IsIncomplete), "Expected complete token (line 3)");
            token = Read();
            Assert.False(token.IsSet(TokenFlags.IsIncomplete), "Expected complete token (line 3)");
            token = Read();
            Assert.Null(token);
        }

        [Fact]
        public void TestIntegration_VsScanningString()
        {
            Init(new IntegrationTestGrammar());
            SetSource(" 'abc");
            Token token = Read();
            Assert.True(token.ValueString == "abc", "Expected incomplete token 'abc' (line 1)");
            Assert.True(token.IsSet(TokenFlags.IsIncomplete), "Expected incomplete token (line 1)");
            token = Read();
            Assert.Null(token);
            SetSource(" def ");
            token = Read();
            Assert.True(token.ValueString == " def ", "Expected incomplete token ' def ' (line 2)");
            Assert.True(token.IsSet(TokenFlags.IsIncomplete), "Expected incomplete token (line 2)");
            token = Read();
            Assert.Null(token);
            SetSource("ghi' 'x'");
            token = Read();
            Assert.True(token.ValueString == "ghi", "Expected token 'ghi' (line 3)");
            Assert.False(token.IsSet(TokenFlags.IsIncomplete), "Expected complete token (line 3)");
            token = Read();
            Assert.True(token.ValueString == "x", "Expected token 'x' (line 3)");
            Assert.False(token.IsSet(TokenFlags.IsIncomplete), "Expected complete token (line 3)");
            token = Read();
            Assert.Null(token);
        }

    }//class
}//namespace
