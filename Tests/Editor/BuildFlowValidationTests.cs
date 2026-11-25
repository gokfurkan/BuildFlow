using FurkanGok.BuildFlow.Validation;
using FurkanGok.BuildFlow.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace FurkanGok.BuildFlow.Tests
{
    public sealed class BuildFlowValidationTests
    {
        [Test]
        public void ApplicationVersionRule_AcceptsSemanticVersion()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.ApplicationVersion = "1.2.3";
            var result = new ApplicationVersionRule().Evaluate(profile);
            Assert.IsTrue(result.Passed);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ApplicationVersionRule_RejectsInvalidVersion()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.ApplicationVersion = "not-a-version";
            var result = new ApplicationVersionRule().Evaluate(profile);
            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ValidationSeverity.Error, result.Severity);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void BuildNumberRule_RequiresPositiveInteger()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.BuildNumber = "0";
            Assert.IsFalse(new BuildNumberRule().Evaluate(profile).Passed);

            profile.BuildNumber = "7";
            Assert.IsTrue(new BuildNumberRule().Evaluate(profile).Passed);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SceneListRule_FailsWithoutScenes()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.UseEditorBuildSettingsScenes = false;
            profile.SetScenes(null);
            var result = new SceneListRule().Evaluate(profile);
            Assert.IsFalse(result.Passed);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void OutputDirectoryRule_ValidatesPath()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.OutputDirectory = "Builds/Windows";
            Assert.IsTrue(new OutputDirectoryRule().Evaluate(profile).Passed);

            profile.OutputDirectory = "";
            Assert.IsFalse(new OutputDirectoryRule().Evaluate(profile).Passed);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ProfileExistsRule_FailsForNull()
        {
            var result = new ProfileExistsRule().Evaluate(null);
            Assert.IsFalse(result.Passed);
            Assert.IsTrue(result.IsBlocking);
        }

        [Test]
        public void BuildValidator_ReportsBlockingErrorsForEmptyProfile()
        {
            var profile = ScriptableObject.CreateInstance<BuildFlowProfile>();
            profile.EnsureProfileId();
            profile.ProfileName = "Test";
            profile.ApplicationVersion = "bad";
            profile.BuildNumber = "x";
            profile.UseEditorBuildSettingsScenes = false;
            profile.SetScenes(null);
            profile.OutputDirectory = "Builds";

            var report = new BuildValidator().Validate(profile);
            Assert.IsTrue(report.HasBlockingErrors);
            Assert.Greater(report.ErrorCount, 0);
            Object.DestroyImmediate(profile);
        }
    }
}
