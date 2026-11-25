using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace FurkanGok.BuildFlow
{
    /// <summary>
    /// Orchestration profile for BuildFlow builds. Stored in the consuming Unity project, not in the package.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BuildFlowProfile",
        menuName = "BuildFlow/Build Profile",
        order = 0)]
    public sealed class BuildFlowProfile : ScriptableObject
    {
        [SerializeField] private string profileName = "New Build Profile";
        [SerializeField] private string profileId;
        [SerializeField] [TextArea(2, 4)] private string description;
        [SerializeField] private BuildFlowTargetPlatform targetPlatform = BuildFlowTargetPlatform.WindowsStandaloneX64;
        [SerializeField] private BuildFlowBuildConfiguration buildConfiguration = BuildFlowBuildConfiguration.Release;
        [SerializeField] private string applicationVersion = "0.1.0";
        [SerializeField] private string buildNumber = "1";
        [SerializeField] private string outputDirectory = "Builds";
        [SerializeField] private string artifactNamingFormat = BuildFlowConstants.DefaultArtifactFormat;

        [Header("Build Options")]
        [SerializeField] private bool developmentBuild;
        [SerializeField] private bool scriptDebugging;
        [SerializeField] private bool autoconnectProfiler;
        [SerializeField] private BuildFlowCompressionMethod compressionMethod = BuildFlowCompressionMethod.Default;
        [SerializeField] private bool cleanOutputBeforeBuild;
        [SerializeField] private bool allowOverwriteExistingOutput;

        [Header("Scenes")]
        [SerializeField] private bool useEditorBuildSettingsScenes = true;
        [SerializeField] private List<SceneAsset> scenes = new List<SceneAsset>();

        [Header("Unity Native Build Profile (Optional)")]
        [SerializeField] private UnityEditor.Build.Profile.BuildProfile nativeBuildProfile;

        [Header("Windows")]
        [SerializeField] private string windowsExecutableName = "Game";

        [FormerlySerializedAs("androidPackageName")]
        [Header("Android")]
        [SerializeField] private bool overrideAndroidBundleIdentifier;
        [SerializeField] private string androidBundleIdentifier = "com.company.product";

        [Header("iOS")]
        [SerializeField] private bool overrideIosBundleIdentifier;
        [SerializeField] private string iosBundleIdentifier = "com.company.product";

        public string ProfileName
        {
            get => string.IsNullOrWhiteSpace(profileName) ? name : profileName;
            set => profileName = value;
        }

        public string ProfileId => profileId;
        public string Description => description;
        public BuildFlowTargetPlatform TargetPlatform
        {
            get => targetPlatform;
            set => targetPlatform = value;
        }

        public BuildFlowBuildConfiguration BuildConfiguration
        {
            get => buildConfiguration;
            set => buildConfiguration = value;
        }

        public string ApplicationVersion
        {
            get => applicationVersion;
            set => applicationVersion = value;
        }

        public string BuildNumber
        {
            get => buildNumber;
            set => buildNumber = value;
        }

        public string OutputDirectory
        {
            get => outputDirectory;
            set => outputDirectory = value;
        }

        public string ArtifactNamingFormat
        {
            get => artifactNamingFormat;
            set => artifactNamingFormat = value;
        }

        public bool DevelopmentBuild
        {
            get => developmentBuild;
            set => developmentBuild = value;
        }

        public bool ScriptDebugging
        {
            get => scriptDebugging;
            set => scriptDebugging = value;
        }

        public bool AutoconnectProfiler
        {
            get => autoconnectProfiler;
            set => autoconnectProfiler = value;
        }

        public BuildFlowCompressionMethod CompressionMethod
        {
            get => compressionMethod;
            set => compressionMethod = value;
        }

        public bool CleanOutputBeforeBuild
        {
            get => cleanOutputBeforeBuild;
            set => cleanOutputBeforeBuild = value;
        }

        public bool AllowOverwriteExistingOutput
        {
            get => allowOverwriteExistingOutput;
            set => allowOverwriteExistingOutput = value;
        }

        public bool UseEditorBuildSettingsScenes
        {
            get => useEditorBuildSettingsScenes;
            set => useEditorBuildSettingsScenes = value;
        }

        public IReadOnlyList<SceneAsset> Scenes => scenes;

        public UnityEditor.Build.Profile.BuildProfile NativeBuildProfile
        {
            get => nativeBuildProfile;
            set => nativeBuildProfile = value;
        }

        public string WindowsExecutableName
        {
            get => windowsExecutableName;
            set => windowsExecutableName = value;
        }

        public bool OverrideAndroidBundleIdentifier
        {
            get => overrideAndroidBundleIdentifier;
            set => overrideAndroidBundleIdentifier = value;
        }

        public string AndroidBundleIdentifier
        {
            get => androidBundleIdentifier;
            set => androidBundleIdentifier = value;
        }

        public bool OverrideIosBundleIdentifier
        {
            get => overrideIosBundleIdentifier;
            set => overrideIosBundleIdentifier = value;
        }

        public string IosBundleIdentifier
        {
            get => iosBundleIdentifier;
            set => iosBundleIdentifier = value;
        }

        private void OnEnable()
        {
            EnsureProfileId();
            SyncConfigurationFlags();
        }

        private void OnValidate()
        {
            EnsureProfileId();
            SyncConfigurationFlags();
        }

        public void EnsureProfileId()
        {
            if (string.IsNullOrWhiteSpace(profileId))
                profileId = Guid.NewGuid().ToString("N");
        }

        public void RegenerateProfileId()
        {
            profileId = Guid.NewGuid().ToString("N");
        }

        public void SetScenes(IEnumerable<SceneAsset> newScenes)
        {
            scenes = newScenes != null ? new List<SceneAsset>(newScenes) : new List<SceneAsset>();
        }

        public void DuplicateFrom(BuildFlowProfile source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            profileName = source.profileName + " Copy";
            description = source.description;
            targetPlatform = source.targetPlatform;
            buildConfiguration = source.buildConfiguration;
            applicationVersion = source.applicationVersion;
            buildNumber = source.buildNumber;
            outputDirectory = source.outputDirectory;
            artifactNamingFormat = source.artifactNamingFormat;
            developmentBuild = source.developmentBuild;
            scriptDebugging = source.scriptDebugging;
            autoconnectProfiler = source.autoconnectProfiler;
            compressionMethod = source.compressionMethod;
            cleanOutputBeforeBuild = source.cleanOutputBeforeBuild;
            allowOverwriteExistingOutput = source.allowOverwriteExistingOutput;
            useEditorBuildSettingsScenes = source.useEditorBuildSettingsScenes;
            scenes = source.scenes != null ? new List<SceneAsset>(source.scenes) : new List<SceneAsset>();
            nativeBuildProfile = source.nativeBuildProfile;
            windowsExecutableName = source.windowsExecutableName;
            overrideAndroidBundleIdentifier = source.overrideAndroidBundleIdentifier;
            androidBundleIdentifier = source.androidBundleIdentifier;
            overrideIosBundleIdentifier = source.overrideIosBundleIdentifier;
            iosBundleIdentifier = source.iosBundleIdentifier;
            RegenerateProfileId();
        }

        private void SyncConfigurationFlags()
        {
            if (buildConfiguration == BuildFlowBuildConfiguration.Development)
            {
                developmentBuild = true;
            }
            else
            {
                developmentBuild = false;
                scriptDebugging = false;
                autoconnectProfiler = false;
            }
        }
    }
}
