using System;
using System.Threading.Tasks;
using DataAPI.Service.IdGeneration;
using Moq;
using NUnit.Framework;

namespace DataAPI.Service.Test.IdGeneration
{
    [TestFixture]
    public class IntegerIdGeneratorTest
    {
        private const string DataType = "Sample";

        [Test]
        public void LockIsObtainedAndReleased()
        {
            var lockObtained = false;
            var lockReleased = false;
            var collection = new Mock<IIdGeneratorStateRepository>();
            collection.Setup(x => x.GetForDataTypeAsync(It.IsAny<string>())).Returns(Task.FromResult(new IdGeneratorState(DataType, 1)));
            collection
                .Setup(x => x.TryGetLockAsync(It.IsAny<IdGeneratorState>()))
                .Callback<IdGeneratorState>(idGeneratorState => lockObtained = true)
                .Returns(Task.FromResult(true));
            collection
                .Setup(x => x.ReleaseLockAsync(It.IsAny<IdGeneratorState>(), It.IsAny<Guid>()))
                .Callback<IdGeneratorState,Guid>((idGeneratorState,guid) => lockReleased = true);
            var sut = new IntegerIdGenerator(collection.Object);
            sut.GetIdsAsync(DataType, 1).Wait();
            Assert.That(lockObtained, "Lock obtained");
            Assert.That(lockReleased, "Lock released");
        }

        [Test]
        public void TimeoutExceptionThrownIfLockCannotBeObtained()
        {
            var collection = new Mock<IIdGeneratorStateRepository>();
            collection
                .Setup(x => x.TryGetLockAsync(It.IsAny<IdGeneratorState>()))
                .Returns(Task.FromResult(false));
            var sut = new IntegerIdGenerator(collection.Object)
            {
                TryPeriod = TimeSpan.FromSeconds(1),
                MaxTryTime = TimeSpan.FromSeconds(3)
            };
            Assert.That(async () => await sut.GetIdsAsync(DataType, 1), Throws.TypeOf<TimeoutException>());
        }

        [Test]
        public void ExpiredLockIsReleased()
        {
            var idGeneratorState = new IdGeneratorState("Sample", 2, true, DateTime.UtcNow - TimeSpan.FromMinutes(3), Guid.NewGuid());
            var collection = CreateIdGeneratorStateRepository(idGeneratorState);
            var sut = new IntegerIdGenerator(collection.Object) { MaxTryTime = TimeSpan.FromSeconds(5) };

            sut.GetIdsAsync(DataType, 1).Wait();

            idGeneratorState = collection.Object.GetForDataTypeAsync(DataType).Result;
            Assert.That(idGeneratorState.IsLocked, Is.False);
            Assert.That(idGeneratorState.LockTime, Is.Null);
            Assert.That(idGeneratorState.LockSessionId, Is.Null);
            Assert.That(idGeneratorState.LastId, Is.EqualTo(3));
        }

        private static Mock<IIdGeneratorStateRepository> CreateIdGeneratorStateRepository(IdGeneratorState initialState)
        {
            var idGeneratorState = initialState;
            var collection = new Mock<IIdGeneratorStateRepository>();
            collection.Setup(x => x.GetForDataTypeAsync(DataType)).Returns(Task.FromResult(CopyState(idGeneratorState)));
            collection
                .Setup(x => x.TryGetLockAsync(It.IsAny<IdGeneratorState>()))
                .Returns<IdGeneratorState>(
                    newState =>
                    {
                        if (idGeneratorState.IsLocked)
                            return Task.FromResult(false);
                        idGeneratorState = CopyState(newState);
                        return Task.FromResult(true);
                    });
            collection
                .Setup(x => x.ReleaseLockAsync(It.IsAny<IdGeneratorState>(), It.IsAny<Guid>()))
                .Callback<IdGeneratorState, Guid>(
                    (newState, guid) =>
                    {
                        if (idGeneratorState.LockSessionId == guid)
                            idGeneratorState = CopyState(newState);
                    });
            return collection;
        }

        private static IdGeneratorState CopyState(IdGeneratorState idGeneratorState)
        {
            return new IdGeneratorState(
                idGeneratorState.DataType,
                idGeneratorState.LastId,
                idGeneratorState.IsLocked,
                idGeneratorState.LockTime,
                idGeneratorState.LockSessionId);
        }
    }
}
