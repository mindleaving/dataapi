using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.Service.DataStorage;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    public class FileSystemBlobRdDataStorageTest
    {
        private const string TestDataDirectory = @"C:\temp\DataAPI";

        [TearDown]
        [SetUp]
        public void EmptyTestDirectory()
        {
            if(Directory.Exists(TestDataDirectory))
                Directory.Delete(TestDataDirectory, true);
            Directory.CreateDirectory(TestDataDirectory);
        }

        [Test]
        public async Task CanGetId()
        {
            var sut = CreateBlobRdDataStorage(out var metadataStorageMock);

            var dataType = nameof(DataBlob);
            string id = null;
            Assert.That(async () => id = (await sut.GetIdsAsync(dataType, "jdoe", 1)).Single().Id, Throws.Nothing);
            Assert.That(id, Is.Not.Null);
            Assert.That(await sut.ExistsAsync(dataType, id), Is.True);
        }

        [Test]
        public async Task CanStoreGetAndDeleteDataBlob()
        {
            var sut = CreateBlobRdDataStorage(out var metadataStorageMock);

            var id = IdGenerator.FromGuid();
            var obj = new DataBlob(id, new byte[5], "my5bytes.bin");
            await TestStoreGetAndDeleteOfObject(sut, id, obj, metadataStorageMock);
        }

        [Test]
        public async Task CanStoreGetAndDeleteImage()
        {
            var sut = CreateBlobRdDataStorage(out var metadataStorageMock);

            var id = IdGenerator.FromGuid();
            var obj = new Image(id, new byte[5], ".jpg", "my5bytes.jpg");
            await TestStoreGetAndDeleteOfObject(sut, id, obj, metadataStorageMock);
        }

        private static async Task TestStoreGetAndDeleteOfObject<T>(IRdDataStorage sut, string id, T obj, Mock<IRdDataStorage> metadataStorageMock)
        {
            var dataType = obj.GetType().Name;
            var utcNow = DateTime.UtcNow;
            var container = new GenericDataContainer(
                id,
                "jdoe",
                utcNow,
                "jdoe",
                utcNow,
                "1.0.0.0",
                DataEncoder.Encode(JsonConvert.SerializeObject(obj)));
            await sut.StoreAsync(obj.GetType().Name, container, true);
            metadataStorageMock.Verify(x => x.StoreAsync(dataType, It.IsAny<GenericDataContainer>(), true), Times.Once);
            Console.WriteLine("Test 1 successful");

            // Test 2: Get
            metadataStorageMock.Setup(x => x.GetFromIdAsync(dataType, id)).Returns(Task.FromResult(container));
            var getResult = await sut.GetFromIdAsync(dataType, id);
            Assert.That(getResult, Is.Not.Null);
            Assert.That(getResult.Id, Is.EqualTo(container.Id));
            Assert.That(getResult.Submitter, Is.EqualTo(container.Submitter));
            Assert.That(getResult.SubmissionTimeUtc, Is.EqualTo(container.SubmissionTimeUtc));
            Assert.That(getResult.ApiVersion, Is.EqualTo(container.ApiVersion));
            Assert.That(getResult.Data.ToString(), Is.EqualTo(container.Data.ToString()));
            Console.WriteLine("Test 2 successful");

            // Test 3: Delete
            var deleteResult = await sut.DeleteDataContainerAsync(dataType, id);
            Assert.That(deleteResult, Is.True);
            Assert.That(await sut.ExistsAsync(dataType, id), Is.False);
            metadataStorageMock.Verify(x => x.DeleteDataContainerAsync(typeof(T).Name, id), Times.Once);
            Console.WriteLine("Test 3 successful");
        }

        private static IRdDataStorage CreateBlobRdDataStorage(out Mock<IRdDataStorage> metadataStorageMock)
        {
            metadataStorageMock = new Mock<IRdDataStorage>();
            var fileSystemBinaryDataStorage = new FileSystemBinaryDataStorage(TestDataDirectory);
            return new BlobRdDataStorage(DataStorageTypes.FileSystem, fileSystemBinaryDataStorage, metadataStorageMock.Object, new DummyIdGeneratorManager());
        }
    }
}
