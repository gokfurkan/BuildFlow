using System.IO;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace FurkanGok.BuildFlow.Profiles
{
    public static class BuildFlowProfileFactory
    {
        public static BuildFlowProfile CreateProfile(string desiredName = null)
        {
            EnsureProfilesFolder();

            var assetName = string.IsNullOrWhiteSpace(desiredName) ? "BuildFlowProfile" : ArtifactNaming.Sanitize(desiredName);
            var path = AssetDatabase.GenerateUniqueAssetPath(
                $"{BuildFlowConstants.DefaultProfileFolder}/{assetName}.asset");

            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.ProfileName = Path.GetFileNameWithoutExtension(path);
            profile.EnsureProfileId();
            profile.ApplicationVersion = PlayerSettings.bundleVersion;
            profile.BuildNumber = "1";
            profile.OutputDirectory = "Builds";
            profile.ArtifactNamingFormat = BuildFlowConstants.DefaultArtifactFormat;
            profile.UseEditorBuildSettingsScenes = true;
            profile.WindowsExecutableName = ArtifactNaming.Sanitize(PlayerSettings.productName);
            profile.AndroidBundleIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            profile.IosBundleIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);

            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(profile);
            Selection.activeObject = profile;
            return profile;
        }

        public static BuildFlowProfile DuplicateProfile(BuildFlowProfile source)
        {
            if (source == null)
                return null;

            EnsureProfilesFolder();
            var path = AssetDatabase.GenerateUniqueAssetPath(
                $"{BuildFlowConstants.DefaultProfileFolder}/{ArtifactNaming.Sanitize(source.ProfileName)}_Copy.asset");

            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.DuplicateFrom(source);
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(profile);
            Selection.activeObject = profile;
            return profile;
        }

        public static bool DeleteProfile(BuildFlowProfile profile, bool askConfirmation = true)
        {
            if (profile == null)
                return false;

            var path = AssetDatabase.GetAssetPath(profile);
            if (string.IsNullOrEmpty(path))
                return false;

            if (askConfirmation)
            {
                var confirmed = EditorUtility.DisplayDialog(
                    "Delete BuildFlow Profile",
                    $"Delete profile '{profile.ProfileName}'?\n\nThis cannot be undone.",
                    "Delete",
                    "Cancel");
                if (!confirmed)
                    return false;
            }

            return AssetDatabase.DeleteAsset(path);
        }

        public static BuildFlowProfile[] FindAllProfiles()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(BuildFlowProfile)}");
            var profiles = new BuildFlowProfile[guids.Length];
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                profiles[i] = AssetDatabase.LoadAssetAtPath<BuildFlowProfile>(path);
            }

            return profiles;
        }

        public static void EnsureProfilesFolder()
        {
            if (AssetDatabase.IsValidFolder(BuildFlowConstants.DefaultProfileFolder))
                return;

            if (!AssetDatabase.IsValidFolder("Assets/BuildFlow"))
                AssetDatabase.CreateFolder("Assets", "BuildFlow");

            if (!AssetDatabase.IsValidFolder(BuildFlowConstants.DefaultProfileFolder))
                AssetDatabase.CreateFolder("Assets/BuildFlow", "Profiles");
        }
    }
}
