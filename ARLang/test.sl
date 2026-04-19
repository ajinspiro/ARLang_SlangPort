FUNCTION NUMERIC Quad( NUMERIC a , NUMERIC b , NUMERIC c )
   NUMERIC n;
   n = b*b - 4*a*c;
   IF ( n < 0 ) THEN
        RETURN 0;
   ELSE 
     IF ( n == 0 ) THEN
         RETURN 1;
     ELSE
         RETURN 2;
     ENDIF
   ENDIF 
   RETURN 0;
END


FUNCTION VOID CallQuad( NUMERIC a , NUMERIC b , NUMERIC c )
   NUMERIC d;
   d= Quad(a,b,c);
   IF ( d == 0 ) THEN
         PRINTLN "No Roots";
   ELSE
       IF ( d  == 1 ) THEN
         PRINTLN  "Discriminant is zero";
       ELSE
         PRINTLN  "Two roots are available";
       ENDIF
   ENDIF
   RETURN;
END

FUNCTION VOID Main()
    CallQuad(1,5,2);
    CallQuad(1,4,4);
    CallQuad(5,2,1);
    RETURN;
END