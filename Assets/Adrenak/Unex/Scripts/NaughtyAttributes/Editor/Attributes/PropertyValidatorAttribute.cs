using System;

namespace Adrenak.Unex.Editor
{
    public class PropertyValidatorAttribute : BaseAttribute
    {
        public PropertyValidatorAttribute(Type targetAttributeType) : base(targetAttributeType)
        {
        }
    }
}
