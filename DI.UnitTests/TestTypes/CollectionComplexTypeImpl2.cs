namespace Stannieman.DI.UnitTests.TestTypes
{
    public class CollectionComplexTypeImpl2 : ICollectionType
    {
        public IType4_2 Type4_2 { get; set; }

        public IType4_1 Type4_1 { get; }

        public CollectionComplexTypeImpl2(IType4_1 type4_1)
        {
            Type4_1 = type4_1;
        }
    }
}
