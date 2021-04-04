﻿using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    public abstract record FluentError
    {
    }

    public record OverrideFluentError : FluentError
    {
        public readonly string Id;
        public EntryKind Kind;

        public OverrideFluentError(string id, EntryKind kind)
        {
            Id = id;
            Kind = kind;
        }

        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {Kind.ToString()}";
        }
    }

    public record ResolverFluentError : FluentError
    {
        public string Description;

        private ResolverFluentError(string desc)
        {
            Description = desc;
        }

        public override string ToString()
        {
            return Description;
        }

        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName)
        {
            return new($"No value: {idName.Span.ToString()}");
        }

        public static ResolverFluentError UnknownVariable(VariableReference outType)
        {
            return new($"Unknown variable: {outType.Id}");
        }

        public static ResolverFluentError TooManyPlaceables()
        {
            return new("Too many placeables");
        }

        public static ResolverFluentError Reference(IInlineExpression self)
        {
            // TODO only allow references here
            if (self.TryConvert(out FunctionReference funcRef))
            {
                return new($"Unknown function: {funcRef.Id}()");
            }

            if (self.TryConvert(out MessageReference msgRef))
            {
                if (msgRef.Attribute == null)
                {
                    return new($"Unknown message: {msgRef.Id}");
                }
                else
                {
                    return new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}");
                }
            }

            if (self.TryConvert(out TermReference termReference))
            {
                if (termReference.Attribute == null)
                {
                    return new($"Uknown term: -{termReference.Id}");
                }
                else
                {
                    return new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}");
                }
            }

            if (self.TryConvert(out VariableReference varRef))
            {
                return new($"Unknown variable: ${varRef.Id}");
            }

            throw new ArgumentException($"Expected reference got ${self.GetType()}");
        }
    }

    public record ParserFluentError : FluentError
    {
        public ParseError Error;

        public override string ToString()
        {
            return Error.Message;
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Function,
    }
}