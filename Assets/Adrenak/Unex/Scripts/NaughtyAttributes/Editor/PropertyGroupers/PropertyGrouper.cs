using System;

namespace Adrenak.Unex.Editor
{
    public abstract class PropertyGrouper
    {
        public abstract void BeginGroup(string label);

        public abstract void EndGroup();
    }
}
