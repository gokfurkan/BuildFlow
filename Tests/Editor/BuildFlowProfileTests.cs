using System.Collections.Generic;
using FurkanGok.BuildFlow.Utilities;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FurkanGok.BuildFlow.Tests
{
    public sealed class BuildFlowProfileTests
    {
        [Test]
        public void EnsureProfileId_GeneratesUniqueIds()
        {
            var a = ScriptableObject.CreateInstance<BuildFlowProfile>();
            var b = ScriptableObject.CreateInstance<BuildFlowProfile>();
            a.EnsureProfileId();
            b.EnsureProfileId();

            Assert.IsFalse(string.IsNullOrWhiteSpace(a.ProfileId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(b.ProfileId));
            Assert.AreNotEqual(a.ProfileId, b.ProfileId);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void DuplicateFrom_CreatesNewProfileId()
        {
            var source = ScriptableObject.CreateInstance<BuildFlowProfile>();
            source.EnsureProfileId();
            source.ProfileName = "Source";
            source.ApplicationVersion = "1.2.3";

            var copy = ScriptableObject.CreateInstance<BuildFlowProfile>();
            copy.DuplicateFrom(source);

            Assert.AreNotEqual(source.ProfileId, copy.ProfileId);
            Assert.AreEqual("1.2.3", copy.ApplicationVersion);
            Assert.IsTrue(copy.ProfileName.Contains("Copy"));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(copy);
        }

        [Test]
        public void ArtifactNaming_ResolvesTokens()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.ProfileName = "Demo Game";
            profile.EnsureProfileId();
            profile.ApplicationVersion = "1.0.0";
            profile.BuildNumber = "42";
            profile.TargetPlatform = BuildFlowTargetPlatform.WindowsStandaloneX64;
            profile.BuildConfiguration = BuildFlowBuildConfiguration.Release;

            var name = ArtifactNaming.Resolve("{name}_{version}_{build}_{platform}_{config}", profile);
            Assert.AreEqual("Demo_Game_1.0.0_42_windows-x64_release", name);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PlatformSupport_MapsWindowsToStandaloneWindows64()
        {
            Assert.AreEqual(
                BuildTarget.StandaloneWindows64,
                PlatformSupportUtility.ToUnityBuildTarget(BuildFlowTargetPlatform.WindowsStandaloneX64));
        }
    }
}
