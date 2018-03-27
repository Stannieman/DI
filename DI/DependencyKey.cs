using System;

namespace Stannieman.DI
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class DependencyKey : Attribute
    {
        public string Key { get; private set; }

        public DependencyKey(string key)
        {
            Key = key;
        }
    }
}
