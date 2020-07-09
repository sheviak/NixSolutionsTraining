using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PKAttribute : Attribute
    {
        public string Name { get; set; }

        public PKAttribute([CallerMemberName] string name = null)
        {
            this.Name = name;
        }
    }
}