using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToMany : Attribute
    {
        public string Name { get; set; }

        public OneToMany([CallerMemberName] string name = null)
        {
            this.Name = name;
        }
    }
}