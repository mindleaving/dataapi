using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using DataAPI.DataStructures.DataIo;
using Newtonsoft.Json;

namespace DataAPI.DataStructures
{
    public class Image : IDataBlob
    {
        [JsonConstructor]
        public Image(
            string id,
            byte[] data,
            string extension,
            string filename = null)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Extension = extension.StartsWith(".") ? extension.ToUpperInvariant() : "." + extension.ToUpperInvariant();
            Filename = Path.GetFileName(filename);
        }

        public static Image FromFile(string filepath, string id = null)
        {
            var data = File.ReadAllBytes(filepath);
            if (id == null)
                id = IdGenerator.Sha1HashFromByteArray(data);
            var extension = Path.GetExtension(filepath);
            var filename = Path.GetFileName(filepath);
            return new Image(id, data, extension, filename);
        }

        /// <summary>
        /// Id of image to be able to reference and access it.
        /// Suggested options:
        /// - A GUID
        /// - USERNAME_DATE_CUSTOMID: E.g. JDOE_2018-07-20_IMG5609
        /// </summary>
        [Required]
        public string Id { get; private set; }

        /// <summary>
        /// Filename (without path) from which this image object was created.
        /// Null, if image object is not created from file
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Image file bytes.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// File extension (with dot). Dot will be added if missing. Example: ".PNG", ".JPG"
        /// </summary>
        [Required]
        [RegularExpression("\\.[a-zA-Z0-9]+")]
        public string Extension { get; private set; }
    }
}
