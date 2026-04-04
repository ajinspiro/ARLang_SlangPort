grammar ARLang;

module: procedure+ EOF;
procedure:
	'FUNCTION' TYPE IDENTIFIER '(' arglist? ')' statements 'END';
arglist: arg (',' arg)*;
arg: TYPE IDENTIFIER;
statements: statement+;
statement:
	vardeclstatement
	| printstatement
	| printlinestatement
	| assignmentstatement
	| callstatement
	| ifstatement
	| whilestatement
	| returnstatement;
vardeclstatement: TYPE IDENTIFIER ';';
printstatement: 'PRINT' expr ';';
printlinestatement: 'PRINTLN' expr ';';
assignmentstatement: IDENTIFIER '=' expr ';';
callstatement: callexpr ';';
ifstatement:
	'IF' expr 'THEN' statements ('ELSE' statements)? 'ENDIF';
whilestatement: 'WHILE' expr statements 'WEND';
returnstatement: 'RETURN' expr ';';
expr: bexpr;
bexpr: lexpr (LOGICOP lexpr)*;
lexpr: rexpr (RELOP rexpr)?;
rexpr: term (ADDOP term)*;
term: factor (MULOP factor)*;
factor:
	NUMBER			# factor_Number
	| STRING		# factor_String
	| 'TRUE'		# factor_BoolTrue
	| 'FALSE'		# factor_BoolFalse
	| IDENTIFIER	# factor_IDENTIFIER
	| '(' expr ')'	# factor_NestedExpr
	| ADDOP factor	# factor_UnaryFactor
	| '!' factor	# factor_BoolNotOperation
	| callexpr		# factor_CallExpr;
callexpr: IDENTIFIER '(' actuals? ')';
actuals: expr (',' expr)*;
LOGICOP: '&&' | '||';
RELOP: '>' | '<' | '>=' | '<=' | '<>' | '==';
ADDOP: '+' | '-';
MULOP: '*' | '/';
NUMBER: [0-9]+;
STRING: '"' .+? '"';
TYPE: 'NUMERIC' | 'STRING' | 'BOOLEAN';
IDENTIFIER: [a-zA-Z][a-zA-Z0-9_]*;
WS: [\t\r\n ]+ -> skip;