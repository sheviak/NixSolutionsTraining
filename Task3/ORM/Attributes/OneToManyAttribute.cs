using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    public class OneToManyAttribute : Attribute
    {
        public string Name { get; set; }

        public OneToManyAttribute([CallerMemberName] string name = null)
        {
            this.Name = name;
        }
    }
}