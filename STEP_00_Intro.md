# Introduction

### What is a compiler ?

A compiler in its simplest form is a program that takes a string as input and produces executable program as output.

If we black box the compiler, the input going in to the black box/compiler is a string and the output coming out is an exe file.

If the program in question is an interpreter, the output will not be a file. Instead it will be the execution of the desired effects expressed in the input string.

A very large portion of developer population have no idea what is going on inside that black box. They think such black boxes have big nerdy stuff going inside only Einsteins and Curies can understand. This pragmatic guide lets you see inside the black box and understand the intricacies within so that you can build one when the necessity comes and it comes way more often than you might think.

The black box we are going to build in this book is called ARLang. It will have capabilities to directly execute effects expressed in input string (from now on this input string will be called source code) and produce exe files that can produce those effects upon execution.

ARLang will have 4 stages in its compilation process. You can picturize them as a pipeline, where one stage's output is the next one's input. 
- Lexical analysis of input string
- Parsing / Generation of Abstract Syntax Tree
- Semantic Validation of Abstract Syntax Tree
- Depending upon the user choice the next step will be either:
    - Execution of effects
    - Creation of executable file

The above stages of ARLang can be classified as front end and back end. Lexical analysis, parsing and semantic validation together constitute front end and execution of the program/creation of executable file constitute back end. So on what criteria is this partitioning made ? In real compilers like clang/llvm or java, the front end part is the part which can be reused between all the different machine architectures the compiler needs to support. This is usually an intermediate representation and such IRs are constructed from the ASTs generated from the parser. ARLang is too simple to have an IR at least in this edition of the book. The backend is the machine specific section that takes the IR and either executes it or compiles it into machine code.
