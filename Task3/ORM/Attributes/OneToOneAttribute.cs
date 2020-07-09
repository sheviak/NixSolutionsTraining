using System;
using System.Runtime.CompilerServices;

namespace ORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToOne : Attribute
    {
        public string Name { get; set; }
        public string FKName { get; set; }

        public OneToOne([CallerMemberName] string name = null, [CallerMemberName] string fkName = null)
        {
            this.Name = name;
            this.FKName = fkName;
        }
    }
}