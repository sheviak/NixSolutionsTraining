using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    public class OneToOneAttribute : Attribute
    {
        public string Name { get; set; }
        public string FKName { get; set; }

        public OneToOneAttribute([CallerMemberName] string name = null, [CallerMemberName] string fkName = null)
        {
            this.Name = name;
            this.FKName = fkName;
        }
    }
}