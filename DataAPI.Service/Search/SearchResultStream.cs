using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataAPI.Service.Search
{
    public class SearchResultStream : Stream, IDisposable
    {
        private readonly IAsyncEnumerator<string> searchResults;
        private byte[] currentResultBytes;
        private int currentResultProgress;

        public SearchResultStream(IAsyncEnumerable<string> searchResults)
        {
            this.searchResults = searchResults.GetAsyncEnumerator();
        }

        public override void Flush()
        {
            throw new NotSupportedException("This is a read-only stream");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Task.Run(
                async () =>
                {
                    if (currentResultBytes == null)
                    {
                        if (!await LoadNextResult())
                            return 0;
                    }

                    var bytesRead = 0;
                    while (bytesRead < count)
                    {
                        var remainingBytesInCurrentResult = currentResultBytes.Length - currentResultProgress;
                        if(remainingBytesInCurrentResult == 0)
                        {
                            if(!await LoadNextResult())
                                break;
                            continue;
                        }
                        var remainingBytesToBeRead = count - bytesRead;
                        var bytesToBeCopied = Math.Min(remainingBytesToBeRead, remainingBytesInCurrentResult);
                        Array.Copy(currentResultBytes, currentResultProgress, buffer, bytesRead, bytesToBeCopied);
                        currentResultProgress += bytesToBeCopied;
                        bytesRead += bytesToBeCopied;
                    }
                    return bytesRead;
                }).Result;
        }

        private async Task<bool> LoadNextResult()
        {
            var hasNext = await searchResults.MoveNextAsync();
            if (!hasNext)
                return false;
            currentResultBytes = Encoding.UTF8.GetBytes(searchResults.Current + "\n");
            currentResultProgress = 0;
            return true;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("This is a non-seekable read-only stream");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("This is a non-seekable read-only stream");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This is a read-only stream");
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;
        public override long Length => throw new NotSupportedException("This is a non-seekable read-only stream");
        public override long Position
        {
            get => throw new NotSupportedException("This is a non-seekable read-only stream");
            set => throw new NotSupportedException("This is a non-seekable read-only stream");
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await DisposeDataSource();
        }

        public new void Dispose()
        {
            base.Dispose();
            DisposeDataSource().Wait();
        }

        private async Task DisposeDataSource()
        {
            await searchResults.DisposeAsync();
        }
    }
}
