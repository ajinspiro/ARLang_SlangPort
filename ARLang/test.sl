FUNCTION NUMERIC FACT( NUMERIC d )
    NUMERIC res;
    IF ( d <= 0 ) THEN
          res=1;
    ELSE
          res=d*FACT(d-1);
    ENDIF
    RETURN res;
END

FUNCTION VOID Main( ) 
NUMERIC d;
d=0;
WHILE ( d <= 10 )
    PRINT d;
    PRINT "!=";
    PRINTLN FACT(d);
    d = d+1;
WEND
RETURN;
END