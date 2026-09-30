using System.Xml.Linq;

namespace MarkdownPlus.Core;

public static class Extensions
{
    extension(XElement xElement)
    {
        public string DumpString()
        {
            return xElement.ToString(
                #if DEBUG
                #else
                    SaveOptions.DisableFormatting
                #endif
            );
        }
    }
}
