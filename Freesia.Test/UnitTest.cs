using System.Linq;
using Freesia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreesiaTest
{
    class TestClass
    {
        public string text { get; set; }
        public ulong id { get; set; }
        public bool favorited { get; set; }
        public bool? B { get; set; }
        public string[] Ints { get; set; }
        public TestClass2 TestClass2 { get; set; }
    }

    class TestClass2
    {
        public string S { get; set; }
    }

    [TestClass]
    public class UnitTest
    {
        private static string AllOpsScript =
            "text == 'a' && (text ==i 'a' || (text =@ 'a' && text =@i 'a')) || text =~ 'a' || text != 'a' || text !=i 'a' || text !=@ 'a' || text !=@i 'a' ||" +
            "text !~ 'a' && (id == 0 || id != 0 || id < 0 || id > 0 || id <= 0 || id >= 0) && favorited || !favorited && (favorited || favorited && false)";

        private bool RunTest(string script, TestClass obj)
        {
            return FilterCompiler<TestClass>.Compile(script)(obj);
        }

        private bool RunTest(string script)
        {
            return FilterCompiler<TestClass>.Compile(script)(new TestClass());
        }

        [ClassInitialize]
        public static void Setup(TestContext c)
        {
            FilterCompiler<TestClass>.UserFunctionNamespace = "user";
            FilterCompiler<TestClass>.Functions.Add("func", _ => true);
        }

        [TestMethod]
        public void CompileTest()
        {
            // single value
            RunTest("text ==   'hoge'");
            RunTest("text ==i  'hoge'");
            RunTest("text =@   'hoge'");
            RunTest("text =@i  'hoge'");
            RunTest("text =~   'hoge'");
            RunTest("text !=   'hoge'");
            RunTest("text !=i  'hoge'");
            RunTest("text !~   'hoge'");
            RunTest("text !=@  'hoge'");
            RunTest("text !=@i 'hoge'");

            RunTest("'hoge' ==   text");
            RunTest("'hoge' ==i  text");
            RunTest("'hoge' =@   text");
            RunTest("'hoge' =@i  text");
            RunTest("'hoge' =~   text");
            RunTest("'hoge' !=   text");
            RunTest("'hoge' !=i  text");
            RunTest("'hoge' =~   text");
            RunTest("'hoge' !=@  text");
            RunTest("'hoge' !=@i text");

            RunTest("'hoge' ==   'hoge'");
            RunTest("'hoge' ==i  'hoge'");
            RunTest("'hoge' =@   'hoge'");
            RunTest("'hoge' =@i  'hoge'");
            RunTest("'hoge' =~   'hoge'");
            RunTest("'hoge' !=   'hoge'");
            RunTest("'hoge' !=i  'hoge'");
            RunTest("'hoge' !~   'hoge'");
            RunTest("'hoge' !=@  'hoge'");
            RunTest("'hoge' !=@i 'hoge'");

            RunTest("favorited == true");
            RunTest("favorited != true");
            RunTest("favorited && true");
            RunTest("favorited || true");
            RunTest("true == favorited");
            RunTest("true != favorited");
            RunTest("true && favorited");
            RunTest("true || favorited");
            RunTest("true == true");
            RunTest("true != true");
            RunTest("true && true");
            RunTest("true || true");

            RunTest("id == 0");
            RunTest("id != 0");
            RunTest("id >  0");
            RunTest("id <  0");
            RunTest("id >= 0");
            RunTest("id <= 0");
            RunTest("0 == id");
            RunTest("0 != id");
            RunTest("0 >  id");
            RunTest("0 <  id");
            RunTest("0 >= id");
            RunTest("0 <= id");
            RunTest("0 == 0");
            RunTest("0 != 0");
            RunTest("0 >  0");
            RunTest("0 <  0");
            RunTest("0 >= 0");
            RunTest("0 <= 0");

            // array value
            RunTest("text ==   {'hoge', 'fuga', 'piyo'}");
            RunTest("text ==i  {'hoge', 'fuga', 'piyo'}");
            RunTest("text =@   {'hoge', 'fuga', 'piyo'}");
            RunTest("text =@i  {'hoge', 'fuga', 'piyo'}");
            RunTest("text =~   {'hoge', 'fuga', 'piyo'}");
            RunTest("text !=   {'hoge', 'fuga', 'piyo'}");
            RunTest("text !=i  {'hoge', 'fuga', 'piyo'}");
            RunTest("text !~   {'hoge', 'fuga', 'piyo'}");
            RunTest("text !=@  {'hoge', 'fuga', 'piyo'}");
            RunTest("text !=@i {'hoge', 'fuga', 'piyo'}");

            RunTest("{'hoge', 'fuga', 'piyo'} ==   text");
            RunTest("{'hoge', 'fuga', 'piyo'} ==i  text");
            RunTest("{'hoge', 'fuga', 'piyo'} =@   text");
            RunTest("{'hoge', 'fuga', 'piyo'} =@i  text");
            RunTest("{'hoge', 'fuga', 'piyo'} =~   text");
            RunTest("{'hoge', 'fuga', 'piyo'} !=   text");
            RunTest("{'hoge', 'fuga', 'piyo'} !=i  text");
            RunTest("{'hoge', 'fuga', 'piyo'} =~   text");
            RunTest("{'hoge', 'fuga', 'piyo'} !=@  text");
            RunTest("{'hoge', 'fuga', 'piyo'} !=@i text");

            RunTest("{'hoge', 'fuga', 'piyo'} ==   {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} ==i  {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} =@   {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} =@i  {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} =~   {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} !=   {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} !=i  {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} !~   {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} !=@  {'hoge', 'fuga', 'piyo'}");
            RunTest("{'hoge', 'fuga', 'piyo'} !=@i {'hoge', 'fuga', 'piyo'}");

            RunTest("favorited == {true, false}");
            RunTest("favorited != {true, false}");
            RunTest("favorited && {true, false}");
            RunTest("favorited || {true, false}");
            RunTest("{true, false} == favorited");
            RunTest("{true, false} != favorited");
            RunTest("{true, false} && favorited");
            RunTest("{true, false} || favorited");
            RunTest("{true, false} == {true, false}");
            RunTest("{true, false} != {true, false}");
            RunTest("{true, false} && {true, false}");
            RunTest("{true, false} || {true, false}");

            RunTest("id == {0, 1}");
            RunTest("id != {0, 1}");
            RunTest("id >  {0, 1}");
            RunTest("id <  {0, 1}");
            RunTest("id >= {0, 1}");
            RunTest("id <= {0, 1}");
            RunTest("{0, 1} == id");
            RunTest("{0, 1} != id");
            RunTest("{0, 1} >  id");
            RunTest("{0, 1} <  id");
            RunTest("{0, 1} >= id");
            RunTest("{0, 1} <= id");
            RunTest("{0, 1} == {0, 1}");
            RunTest("{0, 1} != {0, 1}");
            RunTest("{0, 1} >  {0, 1}");
            RunTest("{0, 1} <  {0, 1}");
            RunTest("{0, 1} >= {0, 1}");
            RunTest("{0, 1} <= {0, 1}");
        }

        [TestMethod]
        public void EvalutionTest()
        {
            var a = new TestClass { favorited = true, id = 1, text = "Hogefuga" };
            Assert.IsTrue(!RunTest("text ==   'hoge'", a));
            Assert.IsTrue(!RunTest("text ==i  'hoge'", a));
            Assert.IsTrue(!RunTest("text =@   'hoge'", a));
            Assert.IsTrue( RunTest("text =@i  'hoge'", a));
            Assert.IsTrue(!RunTest("text =~   'hoge'", a));
            Assert.IsTrue( RunTest("text !=   'hoge'", a));
            Assert.IsTrue( RunTest("text !=i  'hoge'", a));
            Assert.IsTrue( RunTest("text !~   'hoge'", a));
            Assert.IsTrue( RunTest("text !=@  'hoge'", a));
            Assert.IsTrue(!RunTest("text !=@i 'hoge'", a));

            Assert.IsTrue(!RunTest("'hoge' ==   text", a));
            Assert.IsTrue(!RunTest("'hoge' ==i  text", a));
            Assert.IsTrue(!RunTest("'hoge' =@   text", a));
            Assert.IsTrue(!RunTest("'hoge' =@i  text", a));
            Assert.IsTrue(!RunTest("'hoge' =~   text", a));
            Assert.IsTrue( RunTest("'hoge' !=   text", a));
            Assert.IsTrue( RunTest("'hoge' !=i  text", a));
            Assert.IsTrue( RunTest("'hoge' !~   text", a));
            Assert.IsTrue( RunTest("'hoge' !=@  text", a));
            Assert.IsTrue( RunTest("'hoge' !=@i text", a));

            Assert.IsTrue(RunTest("'hoge' ==   'hoge'", a));
            Assert.IsTrue(RunTest("'hoge' ==i  'hoge'", a));
            Assert.IsTrue(RunTest("'hoge' =@   'hoge'", a));
            Assert.IsTrue(RunTest("'hoge' =@i  'hoge'", a));
            Assert.IsTrue(RunTest("'hoge' =~   'hoge'", a));
            Assert.IsTrue(!RunTest("'hoge' !=   'hoge'", a));
            Assert.IsTrue(!RunTest("'hoge' !=i  'hoge'", a));
            Assert.IsTrue(!RunTest("'hoge' !~   'hoge'", a));
            Assert.IsTrue(!RunTest("'hoge' !=@  'hoge'", a));
            Assert.IsTrue(!RunTest("'hoge' !=@i 'hoge'", a));

            Assert.IsTrue(RunTest("favorited == true", a));
            Assert.IsTrue(!RunTest("favorited != true", a));
            Assert.IsTrue(RunTest("favorited && true", a));
            Assert.IsTrue(RunTest("favorited || true", a));
            Assert.IsTrue(RunTest("true == favorited", a));
            Assert.IsTrue(!RunTest("true != favorited", a));
            Assert.IsTrue(RunTest("true && favorited", a));
            Assert.IsTrue(RunTest("true || favorited", a));
            Assert.IsTrue(RunTest("true == true", a));
            Assert.IsTrue(!RunTest("true != true", a));
            Assert.IsTrue(RunTest("true && true", a));
            Assert.IsTrue(RunTest("true || true", a));

            Assert.IsTrue(!RunTest("id == 0", a));
            Assert.IsTrue(RunTest("id != 0", a));
            Assert.IsTrue(RunTest("id >  0", a));
            Assert.IsTrue(!RunTest("id <  0", a));
            Assert.IsTrue(RunTest("id >= 0", a));
            Assert.IsTrue(!RunTest("id <= 0", a));
            Assert.IsTrue(!RunTest("0 == id", a));
            Assert.IsTrue(RunTest("0 != id", a));
            Assert.IsTrue(!RunTest("0 >  id", a));
            Assert.IsTrue(RunTest("0 <  id", a));
            Assert.IsTrue(!RunTest("0 >= id", a));
            Assert.IsTrue(RunTest("0 <= id", a));
            Assert.IsTrue(RunTest("0 == 0", a));
            Assert.IsTrue(!RunTest("0 != 0", a));
            Assert.IsTrue(!RunTest("0 >  0", a));
            Assert.IsTrue(!RunTest("0 <  0", a));
            Assert.IsTrue(RunTest("0 >= 0", a));
            Assert.IsTrue(RunTest("0 <= 0", a));

            // array value
            Assert.IsTrue(!RunTest("text ==   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("text ==i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("text =@   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("text =@i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("text =~   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("text !=   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("text !=i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("text !~   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("text !=@  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("text !=@i {'hoge', 'fuga', 'piyo'}", a));

            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} ==   text", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} ==i  text", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} =@   text", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} =@i  text", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} =~   text", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=   text", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=i  text", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} =~   text", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=@  text", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=@i text", a));

            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} ==   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} ==i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} =@   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} =@i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} =~   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(RunTest("{'hoge', 'fuga', 'piyo'} !=i  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} !~   {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} !=@  {'hoge', 'fuga', 'piyo'}", a));
            Assert.IsTrue(!RunTest("{'hoge', 'fuga', 'piyo'} !=@i {'hoge', 'fuga', 'piyo'}", a));

            Assert.IsTrue(RunTest("favorited == {true, false}", a));
            Assert.IsTrue(RunTest("favorited != {true, false}", a));
            Assert.IsTrue(RunTest("favorited && {true, false}", a));
            Assert.IsTrue(RunTest("favorited || {true, false}", a));
            Assert.IsTrue(RunTest("{true, false} == favorited", a));
            Assert.IsTrue(RunTest("{true, false} != favorited", a));
            Assert.IsTrue(RunTest("{true, false} && favorited", a));
            Assert.IsTrue(RunTest("{true, false} || favorited", a));
            Assert.IsTrue(RunTest("{true, false} == {true, false}", a));
            Assert.IsTrue(RunTest("{true, false} != {true, false}", a));
            Assert.IsTrue(RunTest("{true, false} && {true, false}", a));
            Assert.IsTrue(RunTest("{true, false} || {true, false}", a));

            Assert.IsTrue(RunTest("id == {0, 1}", a));
            Assert.IsTrue(RunTest("id != {0, 1}", a));
            Assert.IsTrue(RunTest("id >  {0, 1}", a));
            Assert.IsTrue(!RunTest("id <  {0, 1}", a));
            Assert.IsTrue(RunTest("id >= {0, 1}", a));
            Assert.IsTrue(RunTest("id <= {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} == id", a));
            Assert.IsTrue(RunTest("{0, 1} != id", a));
            Assert.IsTrue(!RunTest("{0, 1} >  id", a));
            Assert.IsTrue(RunTest("{0, 1} <  id", a));
            Assert.IsTrue(RunTest("{0, 1} >= id", a));
            Assert.IsTrue(RunTest("{0, 1} <= id", a));
            Assert.IsTrue(RunTest("{0, 1} == {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} != {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} >  {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} <  {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} >= {0, 1}", a));
            Assert.IsTrue(RunTest("{0, 1} <= {0, 1}", a));
        }

        [TestMethod]
        public void AllOps()
        {
            RunTest(AllOpsScript);
        }

        [TestMethod]
        public void SyntaxHighlight()
        {
            var syntax = FilterCompiler<TestClass>.Parse(AllOpsScript);
            FilterCompiler<TestClass>.SyntaxHighlight(syntax);
            FilterCompiler<TestClass>.ParseForSyntaxHightlight(AllOpsScript);
            FilterCompiler<TestClass>.ParseForSyntaxHightlight("user.func == true");
            var info = FilterCompiler<TestClass>.ParseForSyntaxHightlight("true false user 1 2 3 'a' == !a || b[1] || {true, false, null} text user.func text.length").ToArray();
            info.ToString();
        }

        [TestMethod]
        public void UnaryOperator()
        {
            Assert.IsFalse(RunTest("!{true}", new TestClass()));
            Assert.IsFalse(RunTest("!{true, true}", new TestClass()));
            Assert.IsFalse(RunTest("!{B}", new TestClass { B = true }));
        }

        [TestMethod]
        public void Completion()
        {
            string s;
            FilterCompiler<TestClass>.Completion("'", out s);
            FilterCompiler<TestClass>.Completion("\"", out s);
            FilterCompiler<TestClass>.Completion("'a", out s);
            FilterCompiler<TestClass>.Completion("a", out s);
            FilterCompiler<TestClass>.Completion("text.l", out s);
            FilterCompiler<TestClass>.Completion("user.", out s);
            FilterCompiler<TestClass>.Completion("user.f", out s);
            var completion = FilterCompiler<TestClass>.Completion("text == 'a' || tex", out s);
            Assert.AreEqual(completion.First(), "text");
            Assert.AreEqual(s, "tex");
            Assert.AreEqual(FilterCompiler<TestClass>.Completion("", out s).Count(),
                typeof(TestClass).GetProperties().Length + 1);
        }

        [TestMethod]
        public void ParserException()
        {
            try
            {
                RunTest("texta");
            }
            catch (ParseException)
            {
                return;
            }
            Assert.Fail("Expected ParseException.");
        }

        [TestMethod]
        public void IndexerTest()
        {
            var a = new TestClass { Ints = new[] { "1" } };
            Assert.IsTrue(RunTest("Ints[0].Length == 1", a));
        }

        [TestMethod]
        public void UserFunctionNamespaceTest()
        {
            Assert.IsTrue(RunTest("user.func"));
        }

        [TestMethod]
        public void NullableTest()
        {
            Assert.IsFalse(RunTest("B"));
            Assert.IsFalse(RunTest("Ints.Length == 0"));
            Assert.IsFalse(RunTest("TestClass2.S.Length == 0"));
        }

        [TestMethod]
        public void ComplexSyntaxTest()
        {
            Assert.IsTrue(RunTest("B || text == null && Ints.Length == 0", 
                new TestClass { B = false, Ints = new string[0]}));
        }

        [TestMethod]
        public void CaseSensitiveTest()
        {
            var a = new TestClass { text = "a", TestClass2 = new TestClass2 { S = "b" } };
            Assert.IsTrue(RunTest("text ==i 'A'", a));
            Assert.IsTrue(RunTest("'A' ==i text", a));
            Assert.IsTrue(RunTest("testclass2.s ==i 'B'", a));
            Assert.IsTrue(RunTest("'B' ==i testclass2.s", a));
        }

        [TestMethod]
        public void LambdaTest()
        {
            FilterCompiler<TestClass>.Parse("a => a == 1");
            try
            {
                FilterCompiler<TestClass>.Compile("a => a == 1");
            }
            catch (ParseException)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void MethodInvokeTest()
        {
            FilterCompiler<TestClass>.Compile("method(a => a == 1)");
        }

        [TestMethod]
        public void StressTest()
        {
            int Iteration = 50, c = 0;
            for (var i = 0; i < Iteration; ++i)
            {
                RunTest(
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} || " +
                    "{'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'} == {'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a'}");
                c += 1;
            }
        }
    }
}
