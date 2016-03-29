using Freesia.Types;

namespace Freesia.Internal.Types
{
    internal class ASTNode
    {
        public CompilerToken Token { get; set; }
        public ASTNode Left { get; set; }
        public ASTNode Right { get; set; }

        internal string Dump()
        {
            return Left?.Dump() + Token.Value + Right?.Dump();
        }
    }

    internal class ArrayProperty
    {
        public string PropName { get; set; }
        public string ArrayAccessor { get; set; }
        public int Index { get; set; }
    }
}
