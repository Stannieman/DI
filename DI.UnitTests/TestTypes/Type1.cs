namespace Stannieman.DI.UnitTests.TestTypes
{
    public class Type1 : IType1
    {
        public IType3 Type3 { get; set; }

        public IType2 Type2 { get; }

        public Type1(IType2 type2)
        {
            Type2 = type2;
        }
    }
}
