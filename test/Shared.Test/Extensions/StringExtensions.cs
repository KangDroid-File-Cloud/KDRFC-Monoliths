using System.Text;

namespace Shared.Test.Extensions;

public static class StringExtensions
{
    public static Stream CreateStream(this string targetString)
    {
        var byteArray = Encoding.UTF8.GetBytes(targetString);

        return new MemoryStream(byteArray);
    }
}