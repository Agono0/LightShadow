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

        // handRoot: the first-person hand; flashlightGo: the light object that
        // should start at the lens; torchPrimitive: the fallback cylinder.
        static void ApplyPlayerRigModels(GameObject player, GameObject handRoot,
            GameObject flashlightGo, GameObject torchPrimitive)
        {
            Vector3 handGripPoint = new Vector3(0.01f, 0.035f, 0.02f);

            if (HeroModelExists(PlayerTorchModel))
            {
                GameObject torch = AddHeroModelChild(handRoot, PlayerTorchModel, 0f,
                    Quaternion.identity, handGripPoint, "ArtModel_" + PlayerTorchModel);
                if (torch != null)
                {
                    Transform grip = FindMarker(torch.transform, PlayerRigFit.GripName);
                    Transform muzzle = FindMarker(torch.transform, PlayerRigFit.MuzzleName);
                    Vector3 offset = handGripPoint;
                    if (grip != null && PlayerRigFit.IsUsable(grip.localPosition))
                    {
                        offset = PlayerRigFit.RootOffsetForGrip(handGripPoint, grip.localPosition,
                            torch.transform.localRotation, torch.transform.localScale.x);
                    }
                    torch.transform.parent.localPosition = offset;
                    if (muzzle != null && PlayerRigFit.IsUsable(muzzle.localPosition))
                    {
                        flashlightGo.transform.localPosition = PlayerRigFit.MuzzleInHandSpace(
                            offset, muzzle.localPosition, torch.transform.localRotation,
                            torch.transform.localScale.x);
                    }
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

            bool handAdded = false;
            foreach (string handModel in new[] { PlayerHandRModel, PlayerHandLModel })
            {
                if (!HeroModelExists(handModel)) continue;
                GameObject hand = AddHeroModelChild(handRoot, handModel, 0f, Quaternion.identity,
                    Vector3.zero, "ArtModel_" + handModel);
                if (hand != null) handAdded = true;
            }
            if (handAdded)
            {
                foreach (string part in new[] { "Palm", "Thumb", "Forearm" })
                {
                    Transform box = handRoot.transform.Find(part);
                    if (box != null) box.gameObject.SetActive(false);
                }
                Count("player hand models");
            }

            if (HeroModelExists(PlayerBodyModel))
            {
                GameObject body = AddHeroModelChild(player, PlayerBodyModel, 0f, Quaternion.identity,
                    Vector3.zero, "ArtModel_" + PlayerBodyModel);
                if (body != null)
                {
                    // First person: the body only throws a shadow, never blocks the view.
                    foreach (Renderer r in body.GetComponentsInChildren<Renderer>(true))
                    {
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }
                    Count("player body model");
                }
            }
        }
    }
}
