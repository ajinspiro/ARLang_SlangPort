FUNCTION NUMERIC RecMul( NUMERIC a , NUMERIC b ) 
   IF ( b==0 ) THEN
        RETURN 0;
   ELSE  
        RETURN a+RecMul(a, b-1);
   ENDIF  
END

FUNCTION VOID Main()
    NUMERIC res;
    res = RecMul(4,2);
    PRINTLN res;
    RETURN;
END