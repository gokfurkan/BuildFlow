namespace FurkanGok.BuildFlow.Validation
{
    public interface IValidationRule
    {
        string RuleId { get; }
        string RuleName { get; }
        ValidationSeverity Severity { get; }
        string Description { get; }
        ValidationResult Evaluate(BuildFlowProfile profile);
    }
}
