using ARLang.SyntaxTree;

namespace ARLang.Visitors;

public interface IVisitorBase
{
    public ARLangExpressionBase Visit(ARLangExpressionBase expression);
}