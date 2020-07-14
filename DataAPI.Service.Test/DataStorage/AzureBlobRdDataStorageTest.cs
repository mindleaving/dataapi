using System;
using System.Linq;
using System.Threading.Tasks;
using Commons.Misc;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class AzureBlobRdDataStorageTest
    {
        [Test]
        public async Task CanWriteReadAndDeleteContainer()
        {
            var sut = CreateAzureBlobRdDataStorage(out var metadataStorage);
            var dataType = nameof(DataBlob);
            var id = "BlobStorageTestBlob";
            var dataBlob = new DataBlob(id, new byte[] { 0x42, 0x43, 0x44}, "myFile.bin");
            var payload = DataEncoder.Encode(JsonConvert.SerializeObject(dataBlob));
            var submitter = "jdoe";
            var utcNow = DateTime.UtcNow;
            var container = new GenericDataContainer(id, submitter, utcNow, submitter, utcNow, ApiVersion.Current, payload);

            if (await sut.ExistsAsync(dataType, id))
                await sut.DeleteDataContainerAsync(dataType, id);

            IdReservationResult idReservation = null;
            Assert.That(async () => idReservation = await sut.ReserveIdAsync(dataType, id, submitter), Throws.Nothing);
            Assert.That(idReservation.IsReserved, Is.True);

            Assert.That(() => sut.StoreAsync(dataType, container, true), Throws.Nothing);
            metadataStorage.Setup(x => x.GetFromIdAsync(dataType, id)).Returns(Task.FromResult(container));
            Assert.That(await sut.ExistsAsync(dataType, id), Is.True);

            GenericDataContainer retreivedContainer = null;
            Assert.That(async () => retreivedContainer = await sut.GetFromIdAsync(dataType, id), Throws.Nothing);
            Assert.That(retreivedContainer, Is.Not.Null);

            Assert.That(async () => await sut.DeleteDataContainerAsync(dataType, id), Throws.Nothing);
            metadataStorage.Setup(x => x.GetFromIdAsync(dataType, id)).Returns(Task.FromResult(default(GenericDataContainer)));
            Assert.That(await sut.ExistsAsync(dataType, id), Is.False);
        }

        [Test]
        public async Task CanListCollections()
        {
            var sut = CreateAzureBlobRdDataStorage(out var metadataStorage);
            var imageContainer = CreateImageContainer();
            await sut.StoreAsync(nameof(Image), imageContainer, true); // Ensure there is at least one collection

            var collectionNames = await sut.ListCollectionNamesAsync().ToListAsync();
            Assert.That(collectionNames, Is.Not.Empty);
        }

        private GenericDataContainer CreateImageContainer()
        {
            var id = "BlobStorageTestImage";
            var submitter = "jdoe";
            var image = new Image(id, new byte[] {0x01, 0x02, 0x03}, ".jpg", "myImage.jpg");
            var utcNow = DateTime.UtcNow;
            return new GenericDataContainer(submitter, utcNow, submitter, utcNow, ApiVersion.Current, image);
        }

        private BlobRdDataStorage CreateAzureBlobRdDataStorage(out Mock<IRdDataStorage> metadataStorage)
        {
            metadataStorage = new Mock<IRdDataStorage>();
            metadataStorage
                .Setup(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<GenericDataContainer>(), It.IsAny<bool>()))
                .Returns<string,GenericDataContainer, bool>((dataType, container, overwrite) => Task.FromResult(new StoreResult(container.Id, DataModificationType.Created, false)));
            var cloudAccount = new CloudStorageAccount(
                new StorageCredentials("dataapiblob", Secrets.Get("DataAPI_BlobStorageKey")), true);
            var azureBlobStorage = new AzureBinaryDataStorage(cloudAccount);
            return new BlobRdDataStorage(DataStorageTypes.AzureBlobStorage, azureBlobStorage, metadataStorage.Object, new DummyIdGeneratorManager());
        }
    }
}
