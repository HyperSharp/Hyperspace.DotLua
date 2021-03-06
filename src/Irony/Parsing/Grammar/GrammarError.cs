﻿using System;
using System.Collections.Generic;

namespace Irony.Parsing
{
    public enum GrammarErrorLevel
    {
        NoError, //used only for max error level when there are no errors
        Info,
        Warning,
        Conflict, //shift-reduce or reduce-reduce conflict
        Error, //severe grammar error, parser construction cannot continue
        InternalError //internal Irony error
    }

    public class GrammarError
    {
        public readonly GrammarErrorLevel Level;
        public readonly string Message;
        public readonly ParserState State; //can be null!

        public GrammarError(GrammarErrorLevel level, ParserState state, string message)
        {
            Level = level;
            State = state;
            Message = message;
        }

        public override string ToString()
        {
            return Message + " (" + State + ")";
        }
    } //class

    public class GrammarErrorList : List<GrammarError>
    {
        public void Add(GrammarErrorLevel level, ParserState state, string message, params object[] args)
        {
            if (args != null && args.Length > 0)
                message = string.Format(message, args);
            base.Add(new GrammarError(level, state, message));
        }

        public void AddAndThrow(GrammarErrorLevel level, ParserState state, string message, params object[] args)
        {
            Add(level, state, message, args);
            var error = this[Count - 1];
            var exc = new GrammarErrorException(error.Message, error);
            throw exc;
        }

        public GrammarErrorLevel GetMaxLevel()
        {
            var max = GrammarErrorLevel.NoError;
            foreach (var err in this)
                if (max < err.Level)
                    max = err.Level;
            return max;
        }
    }

    //Used to cancel parser construction when fatal error is found
    public class GrammarErrorException : Exception
    {
        public readonly GrammarError Error;

        public GrammarErrorException(string message, GrammarError error) : base(message)
        {
            Error = error;
        }
    } //class
}