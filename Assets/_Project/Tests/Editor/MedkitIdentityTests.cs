using LightShadow.Core;
using LightShadow.Gameplay;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Audit K1: two medkit props, one carried-kit rule, per-prop persistence.
    public class MedkitIdentityTests
    {
        static readonly string[] Names = { "Medkit_Infirmary", "Medkit_Staff" };

        [Test]
        public void CanPack_RefusesWhileCarrying()
        {
            Assert.IsTrue(MedkitPickup.CanPack(false));
            Assert.IsFalse(MedkitPickup.CanPack(true));
        }

        [Test]
        public void NewSave_HidesOnlyTheKitThatWasTaken()
        {
            var blob = new SaveIdentity.Blob { hasMedkitIdentity = true };
            blob.medkits.Add("Medkit_Staff");
            bool[] pick = SaveIdentity.SelectMedkitRestores(Names, blob, true);
            Assert.IsFalse(pick[0]);
            Assert.IsTrue(pick[1]);
        }

        [Test]
        public void NewSave_NoKitTaken_HidesNone()
        {
            var blob = new SaveIdentity.Blob { hasMedkitIdentity = true };
            bool[] pick = SaveIdentity.SelectMedkitRestores(Names, blob, false);
            Assert.IsFalse(pick[0]);
            Assert.IsFalse(pick[1]);
        }

        [Test]
        public void NewSave_BothTaken_HidesBoth()
        {
            var blob = new SaveIdentity.Blob { hasMedkitIdentity = true };
            blob.medkits.Add("Medkit_Infirmary");
            blob.medkits.Add("Medkit_Staff");
            bool[] pick = SaveIdentity.SelectMedkitRestores(Names, blob, false);
            Assert.IsTrue(pick[0]);
            Assert.IsTrue(pick[1]);
        }

        [Test]
        public void OldSave_CarryingKit_HidesOneNotBoth()
        {
            bool[] pick = SaveIdentity.SelectMedkitRestores(Names, null, true);
            Assert.AreEqual(1, (pick[0] ? 1 : 0) + (pick[1] ? 1 : 0));
        }

        [Test]
        public void OldSave_NotCarrying_HidesNone()
        {
            bool[] pick = SaveIdentity.SelectMedkitRestores(Names, new SaveIdentity.Blob(), false);
            Assert.IsFalse(pick[0]);
            Assert.IsFalse(pick[1]);
        }

        [Test]
        public void Blob_RoundTripsMedkitIdentity()
        {
            var blob = new SaveIdentity.Blob { hasMedkitIdentity = true };
            blob.medkits.Add("Medkit_Infirmary");
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, SaveIdentity.Key, SaveIdentity.Write(blob));
            SaveIdentity.Blob read = SaveIdentity.Read(data);
            Assert.IsTrue(read.hasMedkitIdentity);
            CollectionAssert.AreEqual(new[] { "Medkit_Infirmary" }, read.medkits);
        }
    }
}
