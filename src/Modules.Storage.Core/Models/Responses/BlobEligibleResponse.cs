namespace Modules.Storage.Core.Models.Responses;

public class BlobEligibleResponse
{
    /// <summary>
    ///     Blob Temp Token(One-Time-Use), EXP in 1 min.
    /// </summary>
    public string Token { get; set; }
}