using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FSharpLint.VisualStudioExtension
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(VSPackage.PackageGuidString)]
    public sealed class VSPackage : Package
    {
        public const string PackageGuidString = "651e3218-d95b-4ad0-a33f-5b181499a33a";
    }
}
