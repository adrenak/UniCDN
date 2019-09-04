using System;

namespace Adrenak.Unex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowNonSerializedFieldAttribute : DrawerAttribute
    {
    }
}
