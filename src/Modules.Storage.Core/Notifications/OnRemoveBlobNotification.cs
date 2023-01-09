using MediatR;

namespace Modules.Storage.Core.Notifications;

public class OnRemoveBlobNotification : INotification
{
    public string AccountId { get; set; }
    public string TargetBlobId { get; set; }
}