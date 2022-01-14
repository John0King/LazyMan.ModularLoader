using ModuleShared;

namespace Module2
{
    public class Class1 : IPlugIn
    {
        public void WriteOutPut()
        {
            Console.WriteLine(this.GetType().FullName);
            Console.WriteLine(typeof(AutoMapper.Mapper).Assembly.FullName);
        }
    }
}
