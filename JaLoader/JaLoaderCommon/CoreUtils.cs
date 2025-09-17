using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace JaLoader.Common
{
    public static class CoreUtils
    {
        internal static readonly Regex BannedCharacters = new Regex(@"[_|]", RegexOptions.Compiled);
    }
}
