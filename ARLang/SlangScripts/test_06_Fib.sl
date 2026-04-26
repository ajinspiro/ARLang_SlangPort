FUNCTION NUMERIC Main()
 NUMERIC newterm;
 NUMERIC prevterm;
 NUMERIC currterm;
 
 currterm = 1;
 prevterm = 0;

 newterm = currterm + prevterm;

 PRINTLN newterm; 

 WHILE ( newterm <  1000 )
   
   prevterm = currterm;
   currterm = newterm;
   newterm  = currterm + prevterm; 
   PRINTLN newterm;
   
 WEND
 RETURN 0; 
END
