using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace sessionhijack.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (sessionhijack.Properties.Resources.resourceMan == null)
          sessionhijack.Properties.Resources.resourceMan = new ResourceManager("sessionhijack.Properties.Resources", typeof (sessionhijack.Properties.Resources).Assembly);
        return sessionhijack.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => sessionhijack.Properties.Resources.resourceCulture;
      set => sessionhijack.Properties.Resources.resourceCulture = value;
    }
  }
}
