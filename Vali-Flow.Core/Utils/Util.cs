using System.Reflection;

namespace Vali_Flow.Core.Utils;

public static class Util
{
    public static string? GetCurrentMethodName()
    {
        return MethodBase.GetCurrentMethod()?.Name;
    }
}