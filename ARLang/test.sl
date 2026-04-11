FUNCTION NUMERIC PrintArgument(NUMERIC pnum)
 PRINTLN "PrintArgument";
 RETURN 143+pnum;
END

FUNCTION NUMERIC Test()
    RETURN 123;
END

FUNCTION VOID Main()
 NUMERIC num;
 num=PrintArgument(1);
 PRINT "num=";
 PRINTLN num;
 RETURN;
END