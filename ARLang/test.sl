FUNCTION NUMERIC SomethingElse(NUMERIC num)
 PRINTLN num;
 RETURN 1;
END

FUNCTION VOID Main()
 NUMERIC num;
 num=(1+2*3);
 WHILE (num>=0)  
  PRINT "num="; 
  PRINTLN num;
  num=num-1;
 WEND
 RETURN;
END