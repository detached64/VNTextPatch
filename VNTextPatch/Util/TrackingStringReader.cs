using System;

namespace VNTextPatch.Util
{
    internal class TrackingStringReader : IDisposable
    {
        private string _str;
        private readonly int _length;

        public TrackingStringReader(string str)
        {
            _str = str;
            _length = str.Length;
        }

        public string ReadLine()
        {
            if (_str == null)
            {
                throw new ObjectDisposedException(nameof(TrackingStringReader));
            }

            int i;
            for (i = Position; i < _length; i++)
            {
                char c = _str[i];
                if (c == '\r' || c == '\n')
                {
                    string line = _str.Substring(Position, i - Position);
                    Position = i + 1;
                    if (c == '\r' && Position < _length && _str[Position] == '\n')
                    {
                        Position++;
                    }

                    return line;
                }
            }

            if (i > Position)
            {
                string line = _str.Substring(Position, i - Position);
                Position = i;
                return line;
            }

            return null;
        }

        public int Position { get; private set; }

        public void Dispose()
        {
            _str = null;
        }
    }
}
