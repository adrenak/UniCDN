using System;

namespace Adrenak.Unex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : DrawerAttribute
    {
    }
}
