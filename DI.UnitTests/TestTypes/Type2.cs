namespace Stannieman.DI.UnitTests.TestTypes
{
    public class Type2 : IType2
    {
        public IType2_2 Type2_2 { get; set; }

        public IType2_1 Type2_1 { get; }

        public Type2(IType2_1 type2_1)
        {
            Type2_1 = type2_1;
        }
    }
}
