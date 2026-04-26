using OneOf;
using OneOf.Types;

namespace ARLang;

public class VisitorManager
{
    private readonly string mode;

    public VisitorManager(string mode)
    {
        if (!(mode == "--compile" || mode == "--execute"))
        {
            throw new InvalidOperationException();
        }
        this.mode = mode;
    }

    public OneOf<Error, Success> Execute(string slangScriptPath)
    {
        if (slangScriptPath.EndsWith(".sl"))
        {
            return ExecutePrivate(slangScriptPath);
        }
        else
        {
            foreach (var path in Directory.EnumerateFiles(slangScriptPath))
            {
                OneOf<Error, Success> result = ExecutePrivate(path);
                if (result.IsT0) return result.AsT0;
            }
            return new Success();
        }
    }

    public OneOf<Error, Success> ExecutePrivate(string slangScriptPath)
    {
        string scriptContent = File.ReadAllText(slangScriptPath);
        return new Error(); // TODO
    }

}