using System.Collections.Generic;
using FurkanGok.BuildFlow.Validation.Rules;

namespace FurkanGok.BuildFlow.Validation
{
    public sealed class BuildValidator
    {
        private readonly List<IValidationRule> _rules;

        public BuildValidator()
        {
            _rules = new List<IValidationRule>
            {
                new ProfileExistsRule(),
                new ProfileConfigurationRule(),
                new ApplicationVersionRule(),
                new BuildNumberRule(),
                new SceneListRule(),
                new SceneExistsRule(),
                new DisabledScenesInformationRule(),
                new OutputDirectoryRule(),
                new OutputDirectoryWritableRule(),
                new TargetSupportInstalledRule(),
                new OutputOverwriteRule(),
                new NativeBuildProfileMismatchRule(),

                new WindowsSupportRule(),
                new WindowsArchitectureRule(),
                new WindowsOutputPathRule(),
                new WindowsExeNameRule(),

                new AndroidSupportRule(),
                new AndroidSdkNdkRule(),
                new AndroidBundleIdentifierRule(),
                new AndroidVersionCodeRule(),
                new AndroidPackageFormatRule(),
                new AndroidSigningRule(),

                new IosSupportRule(),
                new IosBundleIdentifierRule(),
                new IosExportDirectoryRule(),
                new IosVersionRule(),
                new IosMacLimitationRule()
            };
        }

        public BuildValidator(IEnumerable<IValidationRule> rules)
        {
            _rules = new List<IValidationRule>(rules);
        }

        public ValidationReport Validate(BuildFlowProfile profile)
        {
            var report = new ValidationReport();
            foreach (var rule in _rules)
            {
                report.Add(rule.Evaluate(profile));
            }

            return report;
        }
    }
}
