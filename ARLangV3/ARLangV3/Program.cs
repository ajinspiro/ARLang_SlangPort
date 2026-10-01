// using ARLangV3;
// var ast = new Addition(
//     new Constant(1), new Constant(2)
// );
// Console.WriteLine(Evaluate(ast));
// static double Evaluate(Expr expr)
// {
//     return expr.Match(
//         constant => constant.N,
//         additionOperation => Evaluate(additionOperation.E1) + Evaluate(additionOperation.E2),
//         subtractionOperation => Evaluate(subtractionOperation.E1) - Evaluate(subtractionOperation.E2),
//         multiplicationOperation => Evaluate(multiplicationOperation.E1) * Evaluate(multiplicationOperation.E2),
//         divisionOperation => Evaluate(divisionOperation.E1) / Evaluate(divisionOperation.E2)
//     );
// }

using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ARLangV3;
var ast = new Addition(
    new Constant(1), new Constant(2),new Constant(7),new Constant(7)
);
Console.WriteLine(Evaluate(ast));
static double Evaluate(Expr expr)
{
    return expr.Match(
        constant => constant.N,
        additionOperation => { 
            var flags = BindingFlags.Public| BindingFlags.Instance | BindingFlags.DeclaredOnly;
                
             var count =  typeof(Addition).GetProperties(flags).Length;
              var prop = typeof(Addition).GetProperties(flags);
            
             var exp =0d;
            for (int i = 0; i < prop.Length; i++)
              {
               
                var childexpr = feildcount[i];
                
                      exp +=  Evaluate(childexpr!);
                
              
              }
              
              count--;
             
       
        Console.WriteLine("value "+exp);
           
        return exp;
         } ,
        subtractionOperation => Evaluate(subtractionOperation.E1) - Evaluate(subtractionOperation.E2),
        multiplicationOperation => Evaluate(multiplicationOperation.E1) * Evaluate(multiplicationOperation.E2),
        divisionOperation => Evaluate(divisionOperation.E1) / Evaluate(divisionOperation.E2)
    );
}