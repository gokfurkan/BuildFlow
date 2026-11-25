using System;
using System.Collections.Generic;
using System.Linq;

namespace FurkanGok.BuildFlow.Validation
{
    [Serializable]
    public sealed class ValidationResult
    {
        public ValidationResult(
            string ruleId,
            string ruleName,
            ValidationSeverity severity,
            string description,
            string detailedMessage,
            bool passed)
        {
            RuleId = ruleId;
            RuleName = ruleName;
            Severity = severity;
            Description = description;
            DetailedMessage = detailedMessage;
            Passed = passed;
        }

        public string RuleId { get; }
        public string RuleName { get; }
        public ValidationSeverity Severity { get; }
        public string Description { get; }
        public string DetailedMessage { get; }
        public bool Passed { get; }

        public bool IsBlocking => !Passed && Severity == ValidationSeverity.Error;
    }

    [Serializable]
    public sealed class ValidationReport
    {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        public IReadOnlyList<ValidationResult> Results => _results;

        public bool HasBlockingErrors => _results.Any(r => r.IsBlocking);

        public int ErrorCount => _results.Count(r => !r.Passed && r.Severity == ValidationSeverity.Error);
        public int WarningCount => _results.Count(r => !r.Passed && r.Severity == ValidationSeverity.Warning);
        public int InformationCount => _results.Count(r => r.Severity == ValidationSeverity.Information);

        public void Add(ValidationResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        public void AddRange(IEnumerable<ValidationResult> results)
        {
            if (results == null)
                return;

            foreach (var result in results)
                Add(result);
        }
    }
}
