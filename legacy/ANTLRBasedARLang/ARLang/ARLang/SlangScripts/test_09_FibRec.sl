FUNCTION NUMERIC FIB( NUMERIC n )
       IF ( n <= 1 ) THEN
            RETURN 1;
       ELSE
            RETURN FIB(n-1) + FIB(n-2);
       ENDIF
END

FUNCTION NUMERIC Main()
NUMERIC d;
d=0;
WHILE ( d <= 10 )
    PRINTLN FIB(d);
    d = d+1;
WEND
RETURN 0;
END
