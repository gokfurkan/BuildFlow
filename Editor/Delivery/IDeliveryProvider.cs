using FurkanGok.BuildFlow.Build;

namespace FurkanGok.BuildFlow.Delivery
{
    /// <summary>
    /// Extension point for delivering build artifacts (SMTP, Google Drive, S3, remote Mac agent, etc.).
    /// </summary>
    public interface IDeliveryProvider
    {
        string Id { get; }
        string DisplayName { get; }
        bool CanDeliver(BuildFlowBuildResult buildResult);
        DeliveryResult Deliver(BuildFlowBuildResult buildResult);
    }

    public sealed class DeliveryResult
    {
        public bool Succeeded;
        public string Message;
    }
}
