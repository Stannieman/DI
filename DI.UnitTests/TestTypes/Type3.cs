namespace Stannieman.DI.UnitTests.TestTypes
{
    public class Type3 : IType3
    {
        public IType3_2 Type3_2 { get; set; }

        public IType3_1 Type3_1 { get; }

        public Type3(IType3_1 type3_1)
        {
            Type3_1 = type3_1;
        }
    }
}
