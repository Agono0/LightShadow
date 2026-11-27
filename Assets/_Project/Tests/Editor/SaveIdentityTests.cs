using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SaveIdentityTests
    {
        [Test]
        public void OldSave_WithoutBlob_ReadsNull()
        {
            Assert.IsNull(SaveIdentity.Read(new SaveSystem.SaveData()));
        }

        [Test]
        public void RoundTrip_KeepsWhichPickupsWereDone()
        {
            var blob = new SaveIdentity.Blob();
            blob.fuses.Add("Fuse_Physics");
            blob.fragments.Add("the pressed-flower bookmark");
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, SaveIdentity.Key, SaveIdentity.Write(blob));

            SaveIdentity.Blob read = SaveIdentity.Read(data);
            Assert.IsNotNull(read);
            Assert.IsTrue(SaveIdentity.Contains(read.fuses, "Fuse_Physics"));
            Assert.IsFalse(SaveIdentity.Contains(read.fuses, "Fuse_Library"));
            Assert.IsTrue(SaveIdentity.Contains(read.fragments, "the pressed-flower bookmark"));
        }

        [Test]
        public void BrokenBlob_ReadsNull()
        {
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, SaveIdentity.Key, "{not json");
            Assert.IsNull(SaveIdentity.Read(data));
        }
    }
}
