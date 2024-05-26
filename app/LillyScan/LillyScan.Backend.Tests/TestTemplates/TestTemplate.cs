using LillyScan.Backend.Tests.IO;
using Xunit.Sdk;

namespace LillyScan.Backend.Tests.TestTemplates
{
    public abstract class TestTemplate<I,O>
    {
        protected readonly IOTestData TestData;

        public TestTemplate(IOTestData testData)
        {
            TestData = testData;
        }

        protected abstract I DecodeInput();
        protected abstract O DecodeOutput();
        protected abstract bool CompareOutputs(O o1, O o2);

        public void Run(Func<I, O> f)
        {
            var i = DecodeInput();
            var o = DecodeOutput();
            Assert.True(CompareOutputs(o, f(i)));
        }        
    }

    public static class TestTemplate
    {
        public static void RunBatchTestFiles<I, O>(Func<string, TestTemplate<I, O>> tt, string name, int start, int end, Func<I, O> f)
        {
            List<int> failedTests = new List<int>();
            List<Exception> exceptions=new List<Exception>();

            for (int i = start; i <= end; i++)
            {
                try
                {
                    tt($"{name}_{i}").Run(f);
                }
                catch (Exception e)
                {
                    failedTests.Add(i);
                    exceptions.Add(e);                    
                }
            }
            if (failedTests.Count > 0)
            {
                var listStr = string.Join(", ", failedTests.Select(i => $"{name}_{i}"));
                //Assert.Fail($"The following tests have failed: {listStr}");
                throw new AggregateException($"The following tests have failed: {listStr}", exceptions.ToArray());
            }

        }
    }

}
