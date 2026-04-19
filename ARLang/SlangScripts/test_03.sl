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


FUNCTION VOID Main()
   NUMERIC d;
   d= Quad(1,0-6,9);

   IF ( d == 0 ) THEN
         PRINT "No Roots";
   ELSE
       IF ( d  == 1 ) THEN
         PRINT  "Discriminant is zero";
       ELSE
         PRINT  "Two roots are available";
       ENDIF
   ENDIF
   RETURN;
END
