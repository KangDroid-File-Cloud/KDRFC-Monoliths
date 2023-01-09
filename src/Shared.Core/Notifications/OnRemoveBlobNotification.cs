using MediatR;

namespace Shared.Core.Notifications;

public class OnRemoveBlobNotification : INotification
{
    public string AccountId { get; set; }
    public string TargetBlobId { get; set; }
}