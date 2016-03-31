using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Freesia.Internal.Reflection
{
    internal class Helper
    {
        public static MethodInfo GetEnumerableAnyMethod<TElement>()
        {
            Func<TElement, bool> fakeKeySelector = element => default(bool);
            Expression<Func<IEnumerable<TElement>, bool>> lamda
                = list => list.Any(fakeKeySelector);
            return (lamda.Body as MethodCallExpression).Method;
        }
    }
}
