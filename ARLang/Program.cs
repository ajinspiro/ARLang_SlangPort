using ARLang;


if (args.Length != 2)
{
    throw new InvalidOperationException("ARLang: Invalid usage.");
}

VisitorManager manager = new(args[0]);
manager.Execute(args[1]);

// Compiler visitor = new("DynamicTest");