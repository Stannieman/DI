namespace Stannieman.DI
{
    public class ContainerConfiguration
    {
        public bool EnablePropertyInjection { get; set; }

        internal ContainerConfiguration Clone()
        {
            return new ContainerConfiguration
            {
                EnablePropertyInjection = EnablePropertyInjection
            };
        }
    }
}
