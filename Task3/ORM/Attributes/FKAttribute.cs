using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FKAttribute : Attribute
    {
        public string Name { get; set; }

        public FKAttribute([CallerMemberName] string name = null)
        {
            this.Name = name;
        }
    }
}