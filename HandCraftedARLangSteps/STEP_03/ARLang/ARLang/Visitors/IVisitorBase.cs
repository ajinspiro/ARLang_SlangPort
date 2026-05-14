using ARLang.SyntaxTree;

namespace ARLang.Visitors;

public interface IVisitorBase
{
    public void Visit(List<ARLangStatementBase> statements);
}