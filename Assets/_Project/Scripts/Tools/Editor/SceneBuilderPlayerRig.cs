using UnityEditor;
using UnityEngine;
using LightShadow.Player;

namespace LightShadow.Tools
{
    // Delivered player/torch models (SM_Player_Body, SM_Player_Hand_L/R,
    // SM_Torch; OBJ or FBX, any name-matched file under Models/Hero). Each
    // piece is optional: anything missing leaves the primitive hand and torch
    // exactly as built, so the game never depends on the delivery.
    // Written without an editor; the first CI build is the first compile.
    public static partial class SceneBuilder
    {
        const string PlayerBodyModel = "SM_Player_Body";
        const string PlayerHandLModel = "SM_Player_Hand_L";
        const string PlayerHandRModel = "SM_Player_Hand_R";
        const string PlayerTorchModel = "SM_Torch";
        const float TorchSeatForward = 0.02f;

        static bool HeroModelExists(string modelName)
        {
            foreach (string guid in AssetDatabase.FindAssets(modelName, new[] { HeroModelFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = path.Substring(path.LastIndexOf('/') + 1);
                if (file.Equals(modelName + ".fbx", System.StringComparison.OrdinalIgnoreCase)
                    || file.Equals(modelName + ".obj", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        static Transform FindMarker(Transform root, string markerName)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Equals(markerName, System.StringComparison.OrdinalIgnoreCase)) return t;
            }
            return null;
        }

        static void HideMarker(Transform marker)
        {
            if (marker == null) return;
            foreach (Renderer r in marker.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        }

        // Delivered OBJ materials import as plain defaults; use the project's
        // canonical palette so the pieces match the rest of the art.
        static void RestyleRigModel(GameObject model, Material material)
        {
            if (material == null) return;
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
            {
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                r.sharedMaterials = mats;
            }
        }

        // handRoot: the first-person hand; flashlightGo: the light object that
        // should start at the lens; torchPrimitive: the fallback cylinder.
        // Only the right hand is shown in first person (it holds the torch);
        // the left hand and body models are imported for later use.
        static void ApplyPlayerRigModels(GameObject player, GameObject handRoot,
            GameObject flashlightGo, GameObject torchPrimitive, Palette palette)
        {
            Vector3 handGripPoint = new Vector3(0.01f, 0.035f, 0.05f);
            bool handAdded = false;

            if (HeroModelExists(PlayerHandRModel))
            {
                GameObject hand = AddHeroModelChild(handRoot, PlayerHandRModel, 0f, Quaternion.identity,
                    Vector3.zero, "ArtModel_" + PlayerHandRModel);
                if (hand != null)
                {
                    handAdded = true;
                    RestyleRigModel(hand, palette.paper);
                    Transform handGrip = FindMarker(hand.transform, PlayerRigFit.GripName);
                    if (handGrip != null && PlayerRigFit.IsUsable(handGrip.localPosition))
                    {
                        // Grip marker position expressed in the hand root's space.
                        handGripPoint = handRoot.transform.InverseTransformPoint(handGrip.position);
                    }
                    HideMarker(handGrip);
                    foreach (string part in new[] { "Palm", "Thumb", "Forearm" })
                    {
                        Transform box = handRoot.transform.Find(part);
                        if (box != null) box.gameObject.SetActive(false);
                    }
                    Count("player hand model");
                }
            }

            if (HeroModelExists(PlayerTorchModel))
            {
                GameObject torch = AddHeroModelChild(handRoot, PlayerTorchModel, 0f,
                    Quaternion.identity, Vector3.zero, "ArtModel_" + PlayerTorchModel);
                if (torch != null)
                {
                    RestyleRigModel(torch, palette.metal);
                    Transform grip = FindMarker(torch.transform, PlayerRigFit.GripName);
                    Transform muzzle = FindMarker(torch.transform, PlayerRigFit.MuzzleName);
                    Vector3 offset = handGripPoint;
                    if (grip != null && PlayerRigFit.IsUsable(grip.localPosition))
                    {
                        offset = PlayerRigFit.RootOffsetForGrip(handGripPoint, grip.localPosition,
                            torch.transform.localRotation, torch.transform.localScale.x);
                    }
                    // Seat the torch 20 mm forward of the grip alignment: its tail
                    // would otherwise overlap the hand's cuff and forearm
                    // (Ziad's fit note, second-pass models). The muzzle follows.
                    offset += new Vector3(0f, 0f, TorchSeatForward);
                    torch.transform.parent.localPosition = offset;
                    if (muzzle != null && PlayerRigFit.IsUsable(muzzle.localPosition))
                    {
                        flashlightGo.transform.localPosition = PlayerRigFit.MuzzleInHandSpace(
                            offset, muzzle.localPosition, torch.transform.localRotation,
                            torch.transform.localScale.x);
                    }
                    HideMarker(grip);
                    HideMarker(muzzle);
                    if (torchPrimitive != null)
                    {
                        Renderer r = torchPrimitive.GetComponent<Renderer>();
                        if (r != null) r.enabled = false;
                    }
                    // The older flashlight FBX would double up with the new torch.
                    Transform old = handRoot.transform.Find("ArtModel_" + HeroModelFlashlight);
                    if (old != null) old.gameObject.SetActive(false);
                    Count("player torch model");
                }
            }

            if (HeroModelExists(PlayerBodyModel))
            {
                GameObject body = AddHeroModelChild(player, PlayerBodyModel, 0f, Quaternion.identity,
                    Vector3.zero, "ArtModel_" + PlayerBodyModel);
                if (body != null)
                {
                    RestyleRigModel(body, palette.metal);
                    // First person: the body only throws a shadow, never blocks the view.
                    foreach (Renderer r in body.GetComponentsInChildren<Renderer>(true))
                    {
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }
                    Count("player body model");
                }
            }
            if (!handAdded) return;
        }
    }
}
