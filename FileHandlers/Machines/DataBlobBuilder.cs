using System.IO;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;

namespace FileHandlers.Machines
{
    public static class DataBlobBuilder
    {
        public static DataBlob FromFile(string filePath)
        {
            return new DataBlob(
                IdGenerator.Sha1HashFromFile(filePath),
                File.ReadAllBytes(filePath),
                Path.GetFileName(filePath));
        }

        public static DataBlob FromByteArray(byte[] array, string filename = null)
        {
            return new DataBlob(
                IdGenerator.Sha1HashFromByteArray(array),
                array,
                filename);
        }
    }
}
