﻿using System;
using System.Text;

namespace Irony.Parsing
{
    //A terminal for DSV-formatted files (Delimiter-Separated Values), a generalization of CSV (comma-separated values) format.  
    // See http://en.wikipedia.org/wiki/Delimiter-separated_values
    // For CSV format, there's a recommendation RFC4180 (http://tools.ietf.org/html/rfc4180)
    // It might seem that this terminal is not that useful and it is easy enough to create a custom CSV reader for a particular data format
    // format. However, if you consider all escaping and double-quote enclosing rules, then a custom reader solution would not seem so trivial.
    // So DsvLiteral can simplify this task.  
    public class DsvLiteral : DataLiteralBase
    {
        private char[] _terminators;
        public bool ConsumeTerminator = true; //if true, the source pointer moves after the separator 
        public string Terminator = ",";
        //For last value on the line specify terminator = null; the DsvLiteral will then look for NewLine as terminator
        public DsvLiteral(string name, TypeCode dataType, string terminator) : this(name, dataType)
        {
            Terminator = terminator;
        }

        public DsvLiteral(string name, TypeCode dataType) : base(name, dataType)
        {
        }

        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            if (Terminator == null)
                _terminators = new[] {'\n', '\r'};
            else
                _terminators = new[] {Terminator[0]};
        }

        protected override string ReadBody(ParsingContext context, ISourceStream source)
        {
            string body;
            if (source.PreviewChar == '"')
                body = ReadQuotedBody(context, source);
            else
                body = ReadNotQuotedBody(context, source);
            if (ConsumeTerminator && Terminator != null)
                MoveSourcePositionAfterTerminator(source);
            return body;
        }

        private string ReadQuotedBody(ParsingContext context, ISourceStream source)
        {
            const char dQuoute = '"';
            StringBuilder sb = null;
            var from = source.Location.Position + 1; //skip initial double quote
            while (true)
            {
                var until = source.Text.IndexOf(dQuoute, from);
                if (until < 0)
                    throw new Exception(Resources.ErrDsvNoClosingQuote);
                        // "Could not find a closing quote for quoted value."
                source.PreviewPosition = until; //now points at double-quote
                var piece = source.Text.Substring(from, until - from);
                source.PreviewPosition++; //move after double quote
                if (source.PreviewChar != dQuoute && sb == null)
                    return piece;
                        //quick path - if sb (string builder) was not created yet, we are looking at the very first segment;
                // and if we found a standalone dquote, then we are done - the "piece" is the result. 
                if (sb == null)
                    sb = new StringBuilder(100);
                sb.Append(piece);
                if (source.PreviewChar != dQuoute)
                    return sb.ToString();
                //we have doubled double-quote; add a single double-quoute char to the result and move over both symbols
                sb.Append(dQuoute);
                from = source.PreviewPosition + 1;
            }
        }

        private string ReadNotQuotedBody(ParsingContext context, ISourceStream source)
        {
            var startPos = source.Location.Position;
            var sepPos = source.Text.IndexOfAny(_terminators, startPos);
            if (sepPos < 0)
                sepPos = source.Text.Length;
            source.PreviewPosition = sepPos;
            var valueText = source.Text.Substring(startPos, sepPos - startPos);
            return valueText;
        }

        private void MoveSourcePositionAfterTerminator(ISourceStream source)
        {
            while (!source.EOF())
            {
                while (source.PreviewChar != Terminator[0])
                    source.PreviewPosition++;
                if (source.MatchSymbol(Terminator))
                {
                    source.PreviewPosition += Terminator.Length;
                    return;
                } //if
            } //while
        } //method
    } //class
} //namespace