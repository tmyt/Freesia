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
            FilterCompiler<TestClass>.Compile("text ==   'hoge'");
            FilterCompiler<TestClass>.Compile("text ==i  'hoge'");
            FilterCompiler<TestClass>.Compile("text =@   'hoge'");
            FilterCompiler<TestClass>.Compile("text =@i  'hoge'");
            FilterCompiler<TestClass>.Compile("text =~   'hoge'");
            FilterCompiler<TestClass>.Compile("text !=   'hoge'");
            FilterCompiler<TestClass>.Compile("text !=i  'hoge'");
            FilterCompiler<TestClass>.Compile("text !~   'hoge'");
            FilterCompiler<TestClass>.Compile("text !=@  'hoge'");
            FilterCompiler<TestClass>.Compile("text !=@i 'hoge'");

            FilterCompiler<TestClass>.Compile("'hoge' ==   text");
            FilterCompiler<TestClass>.Compile("'hoge' ==i  text");
            FilterCompiler<TestClass>.Compile("'hoge' =@   text");
            FilterCompiler<TestClass>.Compile("'hoge' =@i  text");
            FilterCompiler<TestClass>.Compile("'hoge' =~   text");
            FilterCompiler<TestClass>.Compile("'hoge' !=   text");
            FilterCompiler<TestClass>.Compile("'hoge' !=i  text");
            FilterCompiler<TestClass>.Compile("'hoge' =~   text");
            FilterCompiler<TestClass>.Compile("'hoge' !=@  text");
            FilterCompiler<TestClass>.Compile("'hoge' !=@i text");

            FilterCompiler<TestClass>.Compile("'hoge' ==   'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' ==i  'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' =@   'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' =@i  'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' =~   'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' !=   'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' !=i  'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' !~   'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' !=@  'hoge'");
            FilterCompiler<TestClass>.Compile("'hoge' !=@i 'hoge'");

            FilterCompiler<TestClass>.Compile("favorited == true");
            FilterCompiler<TestClass>.Compile("favorited != true");
            FilterCompiler<TestClass>.Compile("favorited && true");
            FilterCompiler<TestClass>.Compile("favorited || true");
            FilterCompiler<TestClass>.Compile("true == favorited");
            FilterCompiler<TestClass>.Compile("true != favorited");
            FilterCompiler<TestClass>.Compile("true && favorited");
            FilterCompiler<TestClass>.Compile("true || favorited");
            FilterCompiler<TestClass>.Compile("true == true");
            FilterCompiler<TestClass>.Compile("true != true");
            FilterCompiler<TestClass>.Compile("true && true");
            FilterCompiler<TestClass>.Compile("true || true");

            FilterCompiler<TestClass>.Compile("id == 0");
            FilterCompiler<TestClass>.Compile("id != 0");
            FilterCompiler<TestClass>.Compile("id >  0");
            FilterCompiler<TestClass>.Compile("id <  0");
            FilterCompiler<TestClass>.Compile("id >= 0");
            FilterCompiler<TestClass>.Compile("id <= 0");
            FilterCompiler<TestClass>.Compile("0 == id");
            FilterCompiler<TestClass>.Compile("0 != id");
            FilterCompiler<TestClass>.Compile("0 >  id");
            FilterCompiler<TestClass>.Compile("0 <  id");
            FilterCompiler<TestClass>.Compile("0 >= id");
            FilterCompiler<TestClass>.Compile("0 <= id");
            FilterCompiler<TestClass>.Compile("0 == 0");
            FilterCompiler<TestClass>.Compile("0 != 0");
            FilterCompiler<TestClass>.Compile("0 >  0");
            FilterCompiler<TestClass>.Compile("0 <  0");
            FilterCompiler<TestClass>.Compile("0 >= 0");
            FilterCompiler<TestClass>.Compile("0 <= 0");

            // array value
            FilterCompiler<TestClass>.Compile("text ==   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text ==i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text =@   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text =@i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text =~   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text !=   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text !=i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text !~   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text !=@  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("text !=@i {'hoge', 'fuga', 'piyo'}");

            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==   text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==i  text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@   text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@i  text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=   text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=i  text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@  text");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@i text");

            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=i  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !~   {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@  {'hoge', 'fuga', 'piyo'}");
            FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@i {'hoge', 'fuga', 'piyo'}");

            FilterCompiler<TestClass>.Compile("favorited == {true, false}");
            FilterCompiler<TestClass>.Compile("favorited != {true, false}");
            FilterCompiler<TestClass>.Compile("favorited && {true, false}");
            FilterCompiler<TestClass>.Compile("favorited || {true, false}");
            FilterCompiler<TestClass>.Compile("{true, false} == favorited");
            FilterCompiler<TestClass>.Compile("{true, false} != favorited");
            FilterCompiler<TestClass>.Compile("{true, false} && favorited");
            FilterCompiler<TestClass>.Compile("{true, false} || favorited");
            FilterCompiler<TestClass>.Compile("{true, false} == {true, false}");
            FilterCompiler<TestClass>.Compile("{true, false} != {true, false}");
            FilterCompiler<TestClass>.Compile("{true, false} && {true, false}");
            FilterCompiler<TestClass>.Compile("{true, false} || {true, false}");

            FilterCompiler<TestClass>.Compile("id == {0, 1}");
            FilterCompiler<TestClass>.Compile("id != {0, 1}");
            FilterCompiler<TestClass>.Compile("id >  {0, 1}");
            FilterCompiler<TestClass>.Compile("id <  {0, 1}");
            FilterCompiler<TestClass>.Compile("id >= {0, 1}");
            FilterCompiler<TestClass>.Compile("id <= {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} == id");
            FilterCompiler<TestClass>.Compile("{0, 1} != id");
            FilterCompiler<TestClass>.Compile("{0, 1} >  id");
            FilterCompiler<TestClass>.Compile("{0, 1} <  id");
            FilterCompiler<TestClass>.Compile("{0, 1} >= id");
            FilterCompiler<TestClass>.Compile("{0, 1} <= id");
            FilterCompiler<TestClass>.Compile("{0, 1} == {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} != {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} >  {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} <  {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} >= {0, 1}");
            FilterCompiler<TestClass>.Compile("{0, 1} <= {0, 1}");
        }

        [TestMethod]
        public void EvalutionTest()
        {
            var a = new TestClass { favorited = true, id = 1, text = "Hogefuga" };
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text ==   'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text ==i  'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text =@   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text =@i  'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text =~   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !=   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !=i  'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !~   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !=@  'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text !=@i 'hoge'").Invoke(a));

            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' ==   text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' ==i  text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' =@   text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' =@i  text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' =~   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' !=   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' !=i  text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' !~   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' !=@  text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' !=@i text").Invoke(a));

            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' ==   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' ==i  'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' =@   'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' =@i  'hoge'").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("'hoge' =~   'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' !=   'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' !=i  'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' !~   'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' !=@  'hoge'").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("'hoge' !=@i 'hoge'").Invoke(a));

            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited == true").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("favorited != true").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited && true").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited || true").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true == favorited").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("true != favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true && favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true || favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true == true").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("true != true").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true && true").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("true || true").Invoke(a));

            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("id == 0").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id != 0").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id >  0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("id <  0").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id >= 0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("id <= 0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 == id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 != id").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 >  id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 <  id").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 >= id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 <= id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 == 0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 != 0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 >  0").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("0 <  0").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 >= 0").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("0 <= 0").Invoke(a));

            // array value
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text ==   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text ==i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text =@   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text =@i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text =~   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !=   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("text !=i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text !~   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text !=@  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("text !=@i {'hoge', 'fuga', 'piyo'}").Invoke(a));

            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==   text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==i  text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@   text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@i  text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=i  text").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@  text").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@i text").Invoke(a));

            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} ==i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =@i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} =~   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=i  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !~   {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@  {'hoge', 'fuga', 'piyo'}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{'hoge', 'fuga', 'piyo'} !=@i {'hoge', 'fuga', 'piyo'}").Invoke(a));

            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited == {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited != {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited && {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("favorited || {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} == favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} != favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} && favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} || favorited").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} == {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} != {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} && {true, false}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{true, false} || {true, false}").Invoke(a));

            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id == {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id != {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id >  {0, 1}").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("id <  {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id >= {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("id <= {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} == id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} != id").Invoke(a));
            Assert.IsTrue(!FilterCompiler<TestClass>.Compile("{0, 1} >  id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} <  id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} >= id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} <= id").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} == {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} != {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} >  {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} <  {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} >= {0, 1}").Invoke(a));
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("{0, 1} <= {0, 1}").Invoke(a));
        }

        [TestMethod]
        public void AllOps()
        {
            FilterCompiler<TestClass>.Compile(AllOpsScript);
        }

        [TestMethod]
        public void SyntaxHighlight()
        {
            var syntax = FilterCompiler<TestClass>.Parse(AllOpsScript);
            FilterCompiler<TestClass>.SyntaxHighlight(syntax);
            FilterCompiler<TestClass>.ParseForSyntaxHightlight(AllOpsScript);
            var info = FilterCompiler<TestClass>.ParseForSyntaxHightlight("true false user 1 2 3 'a' == !a || b[1] || {true, false, null} text user.func text.length").ToArray();
            info.ToString();
        }

        [TestMethod]
        public void UnaryOperator()
        {
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("!{true}").Invoke(new TestClass()));
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("!{true, true}").Invoke(new TestClass()));
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("!{B}").Invoke(new TestClass { B = true }));
        }

        [TestMethod]
        public void Completion()
        {
            string s;
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
                FilterCompiler<TestClass>.Compile("texta");
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
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("Ints[0].Length == 1")(a));
        }

        [TestMethod]
        public void UserFunctionNamespaceTest()
        {
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("user.func")(new TestClass()));
        }

        [TestMethod]
        public void NullableTest()
        {
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("B")(new TestClass()));
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("Ints.Length == 0")(new TestClass()));
            Assert.IsFalse(FilterCompiler<TestClass>.Compile("TestClass2.S.Length == 0")(new TestClass()));
        }

        [TestMethod]
        public void ComplexSyntaxTest()
        {
            Assert.IsTrue(FilterCompiler<TestClass>.Compile("B || text == null && Ints.Length == 0")(new TestClass()));
        }

        [TestMethod]
        public void StressTest()
        {
            int Iteration = 50, c = 0;
            for (var i = 0; i < Iteration; ++i)
            {
                FilterCompiler<TestClass>.Compile(
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
