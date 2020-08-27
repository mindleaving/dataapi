using System.IO;

namespace DataAPI.Client.Serialization
{
    public class DownloadStream
    {
        public DownloadStream(Stream stream, string filename)
        {
            Stream = stream;
            Filename = filename;
        }

        public Stream Stream { get; }
        public string Filename { get; }
    }
}
