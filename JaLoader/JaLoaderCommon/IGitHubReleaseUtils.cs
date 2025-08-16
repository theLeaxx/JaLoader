using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JaLoader.Common
{
    public interface IGitHubReleaseUtils
    {
        string GetLatestTagFromAPIURL(string URL, string modName = null);
    }
}
