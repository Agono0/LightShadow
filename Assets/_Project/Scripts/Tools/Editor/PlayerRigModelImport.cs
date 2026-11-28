using UnityEditor;

namespace LightShadow.Tools
{
    // Import policy for the delivered first-person rig OBJs (SM_Player_*,
    // SM_Torch). Seen in the editor 2026-11-28: once the files carry "g"
    // groups, the OBJ importer writes one external .mat per group
    // (BodyMat, CuffMat, Ear_LMat ...) into Models/Hero/Materials. The rig
    // builder replaces every material with the project palette
    // (SceneBuilder.RestyleRigModel), so those files are dead weight; the
    // models import with no materials instead. Static meshes only.
    public class PlayerRigModelImport : AssetPostprocessor
    {
        const string Folder = "Assets/_Project/Models/Hero/";

        public static bool IsRigModel(string path)
        {
            if (!path.StartsWith(Folder) || !path.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase)) return false;
            string file = path.Substring(Folder.Length);
            return file.StartsWith("SM_Player_") || file.StartsWith("SM_Torch");
        }

        void OnPreprocessModel()
        {
            if (!IsRigModel(assetPath)) return;
            var importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
        }
    }
}
