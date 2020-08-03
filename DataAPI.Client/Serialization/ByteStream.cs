using System;
using System.Collections.Generic;
using System.IO;

namespace DataAPI.Client.Serialization
{
    internal class ByteStream : Stream
    {
        private readonly IEnumerator<byte> enumerator;

        public ByteStream(IEnumerable<byte> enumerable)
        {
            enumerator = enumerable.GetEnumerator();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (!enumerator.MoveNext())
                    return i;
                buffer[offset + i] = enumerator.Current;
            }
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
