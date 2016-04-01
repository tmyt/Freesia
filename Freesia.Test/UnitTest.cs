using System;
using System.Collections.Generic;
using System.Linq;
using Freesia;
using Freesia.Types;
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

        private static bool RunTest(string script, TestClass obj)
        {
            return FilterCompiler<TestClass>.Compile(script)(obj);
        }

        private static bool RunTest(string script)
        {
            return FilterCompiler<TestClass>.Compile(script)(new TestClass());
        }

        private static void AreSequenceEeual<T,U>(Func<T,U> selector, IEnumerable<T> actual, params U[] expected)
        {
            var i = 0;
            foreach (var a in actual)
            {
                if(i >= expected.Length) Assert.Fail("Element count is not match");
                var e = expected[i++];
                Assert.AreEqual(e, selector(a));
            }
            if(i != expected.Length) Assert.Fail("Element count is not match");
        }

        [ClassInitialize]
        public static void Setup(TestContext c)
        {
            FilterCompiler<TestClass>.UserFunctionNamespace = "user";
            FilterCompiler<TestClass>.Functions.Add("func", _ => true);
            RunTest("user.func == false");
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
            Assert.IsTrue(RunTest("text =@i  'hoge'", a));
            Assert.IsTrue(!RunTest("text =~   'hoge'", a));
            Assert.IsTrue(RunTest("text !=   'hoge'", a));
            Assert.IsTrue(RunTest("text !=i  'hoge'", a));
            Assert.IsTrue(RunTest("text !~   'hoge'", a));
            Assert.IsTrue(RunTest("text !=@  'hoge'", a));
            Assert.IsTrue(!RunTest("text !=@i 'hoge'", a));

            Assert.IsTrue(!RunTest("'hoge' ==   text", a));
            Assert.IsTrue(!RunTest("'hoge' ==i  text", a));
            Assert.IsTrue(!RunTest("'hoge' =@   text", a));
            Assert.IsTrue(!RunTest("'hoge' =@i  text", a));
            Assert.IsTrue(!RunTest("'hoge' =~   text", a));
            Assert.IsTrue(RunTest("'hoge' !=   text", a));
            Assert.IsTrue(RunTest("'hoge' !=i  text", a));
            Assert.IsTrue(RunTest("'hoge' !~   text", a));
            Assert.IsTrue(RunTest("'hoge' !=@  text", a));
            Assert.IsTrue(RunTest("'hoge' !=@i text", a));

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
            //var syntax = FilterCompiler<TestClass>.Parse(AllOpsScript);
            //FilterCompiler<TestClass>.SyntaxHighlight(syntax);
            //FilterCompiler<TestClass>.ParseForSyntaxHightlight(AllOpsScript);
            //FilterCompiler<TestClass>.ParseForSyntaxHightlight("true false user 1 2 3 'a' == !a || b[1] || {true, false, null} text user.func text.length").ToArray();
            var infoc = FilterCompiler<TestClass>.ParseForSyntaxHightlight(
                "text == {'a', 'b', 'c'}").ToArray();
            AreSequenceEeual(a=>a.Type, infoc,
                SyntaxType.Identifier,
                SyntaxType.Operator,
                SyntaxType.Operator,
                SyntaxType.String,
                SyntaxType.Operator,
                SyntaxType.String,
                SyntaxType.Operator,
                SyntaxType.String,
                SyntaxType.Operator
                );
            var infob = FilterCompiler<TestClass>.ParseForSyntaxHightlight(
                "ints.contains(x => x.chars)").ToArray();
            AreSequenceEeual(a=>a.Type, infob,
                SyntaxType.Identifier,
                SyntaxType.Operator,
                SyntaxType.Identifier,
                SyntaxType.Operator,
                SyntaxType.Argument,
                SyntaxType.Operator,
                SyntaxType.Argument,
                SyntaxType.Operator,
                SyntaxType.Identifier,
                SyntaxType.Operator
                );
            var infoa = FilterCompiler<TestClass>.ParseForSyntaxHightlight(
                "ints[0].contains(x => x =@i 'aa')").ToArray();
            AreSequenceEeual(a=>a.Type, infoa, 
                SyntaxType.Identifier,
                SyntaxType.Operator,
                SyntaxType.Constant,
                SyntaxType.Operator,
                SyntaxType.Operator,
                SyntaxType.Identifier,
                SyntaxType.Operator,
                SyntaxType.Argument,
                SyntaxType.Operator,
                SyntaxType.Argument,
                SyntaxType.Operator,
                SyntaxType.String,
                SyntaxType.Operator
                );
            var info = FilterCompiler<TestClass>.ParseForSyntaxHightlight(
                "user.func == false && ints.contains(x => x =@i 'aa') && favorited != true || testclass2.s == 'bbb' && id >= 10").ToArray();
            Assert.AreEqual(info[0].Type, SyntaxType.Identifier);  // user
            Assert.AreEqual(info[1].Type, SyntaxType.Operator);    // .
            Assert.AreEqual(info[2].Type, SyntaxType.Identifier);  // func
            Assert.AreEqual(info[3].Type, SyntaxType.Operator);    // ==
            Assert.AreEqual(info[4].Type, SyntaxType.Keyword);     // false
            Assert.AreEqual(info[5].Type, SyntaxType.Operator);    // &&
            Assert.AreEqual(info[6].Type, SyntaxType.Identifier);  // ints
            Assert.AreEqual(info[7].Type, SyntaxType.Operator);    // .
            Assert.AreEqual(info[8].Type, SyntaxType.Identifier);  // contains
            Assert.AreEqual(info[9].Type, SyntaxType.Operator);    // (
            Assert.AreEqual(info[10].Type, SyntaxType.Argument);   // x
            Assert.AreEqual(info[11].Type, SyntaxType.Operator);   // =>
            Assert.AreEqual(info[12].Type, SyntaxType.Argument);   // x
            Assert.AreEqual(info[13].Type, SyntaxType.Operator);   // =@i
            Assert.AreEqual(info[14].Type, SyntaxType.String);     // 'aa'
            Assert.AreEqual(info[15].Type, SyntaxType.Operator);   // )
            Assert.AreEqual(info[16].Type, SyntaxType.Operator);   // &&
            Assert.AreEqual(info[17].Type, SyntaxType.Identifier); // favorited
            Assert.AreEqual(info[18].Type, SyntaxType.Operator);   // !=
            Assert.AreEqual(info[19].Type, SyntaxType.Keyword);    // true
            Assert.AreEqual(info[20].Type, SyntaxType.Operator);   // ||
            Assert.AreEqual(info[21].Type, SyntaxType.Identifier); // testclass2
            Assert.AreEqual(info[22].Type, SyntaxType.Operator);   // .
            Assert.AreEqual(info[23].Type, SyntaxType.Identifier); // s
            Assert.AreEqual(info[24].Type, SyntaxType.Operator);   // ==
            Assert.AreEqual(info[25].Type, SyntaxType.String);     // 'bbb'
            Assert.AreEqual(info[26].Type, SyntaxType.Operator);   // &&
            Assert.AreEqual(info[27].Type, SyntaxType.Identifier); // id
            Assert.AreEqual(info[28].Type, SyntaxType.Operator);   // >=
            Assert.AreEqual(info[29].Type, SyntaxType.Constant);   // 10
        }

        [TestMethod]
        public void UnaryOperator()
        {
            Assert.IsFalse(RunTest("!{true}", new TestClass()));
            Assert.IsFalse(RunTest("!{true, true}", new TestClass()));
            Assert.IsFalse(RunTest("!{B}", new TestClass { B = true }));
        }

        [TestMethod]
        public void CompletionTest()
        {
            string s;
            //FilterCompiler<TestClass>.Completion("'", out s);
            //FilterCompiler<TestClass>.Completion("\"", out s);
            //FilterCompiler<TestClass>.Completion("'a", out s);
            //FilterCompiler<TestClass>.Completion("a", out s);
            //FilterCompiler<TestClass>.Completion("text.l", out s);
            //FilterCompiler<TestClass>.Completion("user.", out s);
            //FilterCompiler<TestClass>.Completion("user.f", out s);
            var completion = FilterCompiler<TestClass>.Completion("ints.contains(x => x == 'a", out s);
            Assert.IsTrue(!completion.Any());
            Assert.AreEqual("", s);
            completion = FilterCompiler<TestClass>.Completion("ints.contains(x => x.c", out s);
            Assert.AreEqual("chars", completion.First());
            Assert.AreEqual("c", s);
            completion = FilterCompiler<TestClass>.Completion("text == 'a' || tex", out s);
            Assert.AreEqual("text", completion.First());
            Assert.AreEqual("tex", s);
            completion = FilterCompiler<TestClass>.Completion("text == 'a' || ints.le", out s);
            Assert.AreEqual("length", completion.First());
            Assert.AreEqual("le", s);
            completion = FilterCompiler<TestClass>.Completion("testclass2.", out s);
            Assert.AreEqual("s", completion.First());
            Assert.AreEqual("", s);
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
            Assert.IsTrue(RunTest("Ints[0] == '1'", a));
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
                new TestClass { B = false, Ints = new string[0] }));
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
            FilterCompiler<TestClass>.Parse("a => a != null");
            try
            {
                FilterCompiler<TestClass>.Compile("a => a != null");
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
            var a = new TestClass { Ints = new[] { "https://www.example.com/" } };
            Assert.IsTrue(RunTest("ints.contains(x => x =@i 'example')", a));
            //FilterCompiler<TestClass>.Compile("entities.urls.contains(x => x =@i 'example')");
            //FilterCompiler<TestClass>.Compile("method(a => a == 1)");
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
