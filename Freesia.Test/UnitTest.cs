using Freesia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreesiaTest
{
    class TestClass
    {
        public string text { get; set; }
        public ulong id { get; set; }
        public bool favorited { get; set; }
    }

    [TestClass]
    public class UnitTest
    {
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
            FilterCompiler<TestClass>.Compile(
                "text == 'a' && (text ==i 'a' || (text =@ 'a' && text =@i 'a')) || text =~ 'a' || text != 'a' || text !=i 'a' || text !=@ 'a' || text !=@i 'a' ||" +
                "text !~ 'a' && (id == 0 || id != 0 || id < 0 || id > 0 || id <= 0 || id >= 0) && favorited || !favorited && (favorited || favorited && false)");
        }

        [TestMethod]
        public void StressTest()
        {
            int Iteration = 500, c = 0;
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
