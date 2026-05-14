# Chapter 7 - Function support

---
    EBNF
    module ::= procedure { procedure }

    procedure ::= "FUNCTION" type funcName "(" [ parameterList ] ")"
                  statements
                  "END"
    
    type ::= "NUMERIC"
           | "STRING"
           | "BOOLEAN"
    
    parameterList ::= parameter { "," parameter }
    
    parameter ::= type argName
    
    statements ::= statement { statement }
    
    statement ::= variableDeclStmt
                | printStmt
                | printlnStmt
                | assignmentStmt
                | callStmt
                | ifStmt
                | whileStmt
                | returnStmt
    
    variableDeclStmt ::= type varName ";"
    
    printStmt ::= "PRINT" expr ";"
    
    printlnStmt ::= "PRINTLINE" expr ";"
    
    assignmentStmt ::= variable "=" expr ";"
    
    callStmt ::= callExpr ";"
    
    ifStmt ::= "IF" expr "THEN"
               statements
               [ "ELSE" statements ]
               "ENDIF"
    
    whileStmt ::= "WHILE" expr
                  statements
                  "WEND"
    
    returnStmt ::= "RETURN" expr ";"
    
    expr ::= logicalExpr
    
    logicalExpr ::= relationalExpr
                    { logicOp relationalExpr }
    
    relationalExpr ::= additiveExpr
                       [ relOp additiveExpr ]
    
    additiveExpr ::= multiplicativeExpr
                     { addOp multiplicativeExpr }
    
    multiplicativeExpr ::= unaryExpr
                           { mulOp unaryExpr }
    
    unaryExpr ::= [ "+" | "-" | "!" ]
                  primaryExpr
    
    primaryExpr ::= numericLiteral
                  | stringLiteral
                  | "TRUE"
                  | "FALSE"
                  | variable
                  | "(" expr ")"
                  | callExpr
    
    callExpr ::= funcName "(" [ argumentList ] ")"
    
    argumentList ::= expr { "," expr }
    
    logicOp ::= "&&" | "||"
    
    relOp ::= ">"
            | "<"
            | ">="
            | "<="
            | "<>"
            | "=="
    
    mulOp ::= "*"
            | "/"
    
    addOp ::= "+"
            | "-"
    
    variable ::= identifier
    
    funcName ::= identifier
    
    varName ::= identifier
    
    argName ::= identifier
---