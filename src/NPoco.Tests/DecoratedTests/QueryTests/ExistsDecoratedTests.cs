using System;
using NPoco.Tests.Common;
using NUnit.Framework;

namespace NPoco.Tests.DecoratedTests.QueryTests
{
    [TestFixture]
    public class ExistsDecoratedTests : BaseDBDecoratedTest
    {
        [Test]
        public void ExistsById()
        {
            var exists = Database.Exists<UserDecorated>(InMemoryUsers[1].UserId);
            Assert.IsTrue(exists);
        }

        [Test]
        public void ExistsByFakeID()
        {
            var exists = Database.Exists<UserDecorated>(Int32.MaxValue);
            Assert.IsFalse(exists);
        }

        [Test]
        public void ExistsByNullID()
        {
            var exists = Database.Exists<UserDecorated>(null);
            Assert.IsFalse(exists);
        }

        [Test]
        public void ExistsByCompositeId()
        {
            var composite = Database.SingleById<CompositeObjectDecorated>(new { InMemoryCompositeObjects[1].Key1ID, InMemoryCompositeObjects[1].Key2ID, InMemoryCompositeObjects[1].Key3ID });
            Assert.IsNotNull(composite);

            var exists = Database.Exists<CompositeObjectDecorated>(new { InMemoryCompositeObjects[1].Key1ID, InMemoryCompositeObjects[1].Key2ID, InMemoryCompositeObjects[1].Key3ID });
            Assert.IsTrue(exists);
        }

        [Test]
        public void ExistsByFakeCompositeId()
        {
            var exists = Database.Exists<CompositeObjectDecorated>(new { Key1ID = Int32.MaxValue, Key2ID = Int32.MaxValue, Key3ID = Int32.MaxValue });
            Assert.IsFalse(exists);
        }

        [Test]
        public void ExistsByNullCompositeId()
        {
            var composite = new CompositeObjectDecorated
            {
                Key1ID = 600,
                Key2ID = 602,
                Key3ID = null,
                TextData = "This is some text data.",
                DateEntered = DateTime.Now
            };
            Database.Insert(composite);

            var exists = Database.Exists<CompositeObjectDecorated>(new { Key1ID = 600, Key2ID = 602, Key3ID = DBNull.Value });
            Assert.IsTrue(exists);
        }
    }
}
