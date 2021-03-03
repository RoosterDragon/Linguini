﻿using System;

namespace Linguini.IO
{
    public class ZeroCopyReader
    {
        private readonly ReadOnlyMemory<char> _unconsumedData;
        private int _position;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        private ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _position = 0;
        }

        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsNotEof => _position < _unconsumedData.Length;
        public bool IsEof => !IsNotEof;

        public ReadOnlySpan<char> PeekCharSpan(int offset = 0)
        {
            return _unconsumedData.ReadCharSpan(_position + offset);
        }

        public ReadOnlySpan<char> GetCharSpan()
        {
            var chr = PeekCharSpan();
            _position += 1;
            return chr;
        }

        public int SkipBlankBlock()
        {
            var count = 0;
            while (true)
            {
                var start = _position;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _position = start;
                    break;
                }

                count += 1;
            }

            return count;
        }

        public bool SkipEol()
        {
            if ('\n'.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
                return true;
            }

            if ('\r'.EqualsSpans(PeekCharSpan())
                && '\n'.EqualsSpans(PeekCharSpan(1)))
            {
                _position += 2;
                return true;
            }

            return false;
        }

        private int SkipBlankInline()
        {
            var start = _position;
            while (' '.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
            }

            return _position - start;
        }

        public bool ReadByteIf(char c)
        {
            if (c.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
                return true;
            }

            return false;
        }

        public ReadOnlyMemory<char> GetCommentLine()
        {
            var startPosition = _position;

            while (!IsEol())
            {
                _position += 1;
            }

            return _unconsumedData.Slice(startPosition, _position - startPosition);
        }

        private bool IsEol()
        {
            var chr = PeekCharSpan();

            if ('\n'.EqualsSpans(chr)) return true;
            if ('\r'.EqualsSpans(chr)
                && '\n'.EqualsSpans(PeekCharSpan(1)))
            {
                return true;
            }

            return IsEof;
        }

        public ReadOnlyMemory<char> ReadSlice(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start);
        }

        public string ReadSliceToStr(int start, int end)
        {
            return new(_unconsumedData.Slice(start, end - start).ToArray());
        }

        public void SkipToNextEntry()
        {
            ReadOnlySpan<char> chr;
            while (_unconsumedData.TryReadCharSpan(_position, out var span))
            {
                var newline = _position == 0
                              || '\n'.EqualsSpans(PeekCharSpan(-1));

                if (newline && (span.IsAlphaNumeric() || span.IsOneOf('#','-')))
                {
                    break;
                }

                _position += 1;
            }
        }
    }
}