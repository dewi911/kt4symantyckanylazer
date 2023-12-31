using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Principal;
using System.Text.RegularExpressions;

public abstract class Operand
{
    public abstract string ToTriadString();
}

public class VariableOperand : Operand
{
    public string Name { get; set; }

    public VariableOperand(string name)
    {
        Name = name;
    }

    public override string ToTriadString()
    {
        return $"{Name}";
    }
}

public class ConstantOperand : Operand
{
    public object Value { get; }

    public ConstantOperand(object value)
    {
        Value = value;
    }

    public override string ToTriadString()
    {
        return Value.ToString();
    }
    public bool IsNumeric(out double numericValue)
    {
        if (Value is int intValue)
        {
            numericValue = (double)intValue;
            return true;
        }
        else if (Value is double doubleValue)
        {
            numericValue = doubleValue;
            return true;
        }
        else if (Value is string stringValue && double.TryParse(stringValue, out double parsedValue))
        {
            numericValue = parsedValue;
            return true;
        }
        else
        {
            numericValue = 0; 
            return false;
        }
    }
}

public abstract class Operator
{
    public abstract string ToTriadString(Operand left, Operand right);
}

public class AssignmentOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        return $":= ({left.ToTriadString()} {right.ToTriadString()})";
    }
}

public class AdditionOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        return $"+ ({left.ToTriadString()} {right.ToTriadString()} )";
    }
}

public class SubtractionOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        return $"- ({left.ToTriadString()} {right.ToTriadString()})";
    }
}

public class MultiplicationOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        return $"* ({left.ToTriadString()}  {right.ToTriadString()})";
    }
}


public class DivisionOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        return $"/ ({left.ToTriadString()}  {right.ToTriadString()} )";
    }
}

public class Triad
{
    private static int lineCount = 1;

    public int LineNumber { get; }
    public Operator Operator { get; }
    public Operand LeftOperand { get; set; }
    public Operand RightOperand { get; set; }
    public Operand Result { get; }

    public Triad(Operator op, Operand left, Operand right)
    {
        LineNumber = lineCount++;
        Operator = op;
        LeftOperand = left;
        RightOperand = right;
    }

    public bool IsLineNumberEqual(int number)
    {
        return LineNumber == number;
    }
    public Triad(Operator op, Operand left, Operand right, Operand result) : this(op, left, right)
    {
        Result = result;
    }

    public string ToTriadString()
    {
        if (Result != null)
        {
            return $"{Operator.ToTriadString(LeftOperand, RightOperand)}";
        }
        else
        {
            return $"{Operator.ToTriadString(LeftOperand, RightOperand)}";
        }
    }

    public Operand GetLeftOperand()
    {
        return LeftOperand;
    }
    public bool AreOperandsEqual(Triad other)
    {
        return (LeftOperand != null && other.LeftOperand != null && LeftOperand.ToTriadString() == other.LeftOperand.ToTriadString()) &&
               (RightOperand != null && other.RightOperand != null && RightOperand.ToTriadString() == other.RightOperand.ToTriadString());
    }
    public Operand GetRightOperand()
    {
        return RightOperand;
    }

    public void SetLeftOperand(Operand newLeftOperand)
    {
        LeftOperand = newLeftOperand;
    }

    public void SetRightOperand(Operand newRightOperand)
    {
        RightOperand = newRightOperand;
    }
}

public class Parser
{
    private readonly string input;
    private int index;

    private List<Triad> triads = new List<Triad>();
    private List<Triad> optimizedTriads = new List<Triad>();
    private List<Triad> optimizedTriadstime = new List<Triad>();
    private List<Triad> reducedTriads = new List<Triad>();
    private List<Triad> reducedTriadstime = new List<Triad>();
    public List<Triad> testlist = new List<Triad>();
    public Parser(string input)
    {
        this.input = input;
        this.index = 0;
    }
    
    

    public void ParseAndOptimize()
    {
        Parse();
        OptimizeTriads();
        ReduceTriads();

        Console.WriteLine("\nOriginal Triads:");
        PrintTriads(triads);

        Console.WriteLine("\nOptimized Triads:");
        PrintTriads(optimizedTriadstime);

        Console.WriteLine("\nReduced Triads:");
        PrintTriads(reducedTriadstime);
    }

    

    private void PrintTriads(List<Triad> triadList)
    {
        int count = 1;
        foreach (var triad in triadList)
        {   
            Console.WriteLine($"{count} {triad.ToTriadString()}");
            count++;
        }
    }


 
    private void OptimizeTriads()
    {
        optimizedTriads.Clear();
         
        optimizedTriads.AddRange(triads);
        int seccount = 0;
        
        List<int> numdelete = new List<int>();
        List<int> numdelete1 = new List<int>();
        numdelete.Clear();

        for (int j = 0; j < triads.Count; j++)
            {

                Operand opl = triads[j].LeftOperand;
                Operand opr = triads[j].RightOperand;


                if (opl is ConstantOperand && opr is ConstantOperand)
                {
                    
                    optimizedTriads.Remove(triads[j]);
                           seccount++;
                           numdelete.Add(j);
                        
                }                           
        }

        numdelete.AddRange(numdelete1);
        while (numdelete1.Count > 0)
        {
            for (int m = 0; m < optimizedTriads.Count; m++)
            {
                if (optimizedTriads[m].LeftOperand is TemporaryVariableOperand && optimizedTriads[m].RightOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[m].LeftOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - 1;
                    TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)optimizedTriads[m].RightOperand;

                    int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0] || numericValuerd == numdelete1[0]) optimizedTriads.Remove(triads[m]); numdelete1.RemoveAt(0);
                }
                else if (optimizedTriads[m].LeftOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[m].LeftOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0]) optimizedTriads.Remove(triads[m]); numdelete1.RemoveAt(numdelete1.Count - 1); continue;
                }
                else if (optimizedTriads[m].RightOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[m].RightOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0]) optimizedTriads.Remove(triads[m]); numdelete1.RemoveAt(0);
                }
                
            }

        }
        for (int i = 0; i < optimizedTriads.Count; i++)
        {
            if (optimizedTriads[i].LeftOperand is TemporaryVariableOperand && optimizedTriads[i].RightOperand is TemporaryVariableOperand)
            {
                TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[i].LeftOperand;
                int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - 1;
                TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)optimizedTriads[i].RightOperand;

                int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1));

                if (numericValue <= 0 || numericValuerd <= 0) optimizedTriads.Remove(triads[i]);
            }
        }
        //
        optimizedTriadstime.Clear();
        foreach (var triad in optimizedTriads)
        {
          
            Triad newTriad = new Triad(triad.Operator, triad.LeftOperand, triad.RightOperand, triad.Result);
            optimizedTriadstime.Add(newTriad);
        }
        int coountdelete = 0;
        
       // seccount += 2;
        
        
        for(int l = optimizedTriadstime.Count-1; l >= 0; l--)
        {
            

            if (l >= numdelete[0])
            {
                if (optimizedTriads[l].LeftOperand is TemporaryVariableOperand && optimizedTriads[l].RightOperand is TemporaryVariableOperand)
                {

                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[l].LeftOperand;

                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);

                    optimizedTriadstime[l].LeftOperand = tmpOperandFromInt1;

                    TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)optimizedTriads[l].RightOperand;

                    int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromIntrd = TemporaryVariableOperand.FromInt(numericValuerd);

                    optimizedTriadstime[l].RightOperand = tmpOperandFromIntrd;
                   int lastElement = numdelete[numdelete.Count-1];
                   if (lastElement == l) 
                   { 
                       seccount--;
                       numdelete.RemoveAt(numdelete[numdelete.Count - 1]);
                   }
                    continue;


                }
                else if (optimizedTriads[l].LeftOperand is TemporaryVariableOperand)
                {
               
               
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[l].LeftOperand;
               
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);

                    optimizedTriadstime[l].LeftOperand = tmpOperandFromInt1;

                   int lastElement = numdelete[numdelete.Count-1];
                   if (lastElement == l) 
                   { 
                       seccount--;
                       numdelete.RemoveAt(numdelete[numdelete.Count - 1]);
                   }
                    continue;


                }
                else if (optimizedTriads[l].RightOperand is TemporaryVariableOperand)
                {
                
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriads[l].RightOperand;
                
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);
                
                    optimizedTriadstime[l].RightOperand = tmpOperandFromInt1;

                    int lastElement = numdelete[numdelete.Count - 1];
                    if (lastElement== l)
                    {
                        seccount--;
                        numdelete.RemoveAt(numdelete[numdelete.Count - 1]);
                    }
                    continue;
                }
            }
        }
        for (int i = 0; i < optimizedTriadstime.Count; i++)
        {
            if (optimizedTriadstime[i].LeftOperand is TemporaryVariableOperand && optimizedTriadstime[i].RightOperand is TemporaryVariableOperand)
            {
                TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)optimizedTriadstime[i].LeftOperand;
                int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - 1;
                TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)optimizedTriadstime[i].RightOperand;

                int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1));

                if (numericValue <= 0 || numericValuerd <= 0) optimizedTriadstime.RemoveAt(i);
            }
        }
    }

    private void ReduceTriads()
    {
        


        reducedTriads.AddRange(triads);
        int seccount = 0;
       
        List<int> numdelete = new List<int>();
        List<int> numdelete1 = new List<int>();

        for (int i = 0; i < triads.Count; i++)
        {     
            int count = 0;
            Operand opl = triads[i].LeftOperand;
            Operand opr = triads[i].RightOperand;
            for (int j = 0; j < triads.Count; j++)
            {
                
               Operand opl2 = triads[j].LeftOperand;
               Operand opr2 = triads[j].RightOperand;
                
                 if(opl is VariableOperand && opr is VariableOperand&&opl2 is VariableOperand && opr2 is VariableOperand)
                 {
                     VariableOperand currentLeftVar = (VariableOperand)opl;
                     VariableOperand nextlistLeftVar = (VariableOperand)opl2;
                     VariableOperand currentLeftVar2 = (VariableOperand)opr;
                     VariableOperand nextlistLeftVar2 = (VariableOperand)opr2;
                
                     if (currentLeftVar.Name == nextlistLeftVar.Name&& currentLeftVar2.Name == nextlistLeftVar2.Name)
                     {
                         count++;
                        if (count == 2)
                        { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }
                     }
                 }
                 else if(opl is VariableOperand && opr is TemporaryVariableOperand&& opl2 is VariableOperand && opr2 is TemporaryVariableOperand)
                 {
                     VariableOperand currentLeftVar = (VariableOperand)opl;
                     VariableOperand nextlistLeftVar = (VariableOperand)opl2;
                     TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)opr;
                     TemporaryVariableOperand tmpOperand2 = (TemporaryVariableOperand)opr2;
                     if(currentLeftVar.Name == nextlistLeftVar.Name && tmpOperand == tmpOperand2)
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                 }
                 else if (opl is TemporaryVariableOperand && opr is VariableOperand&& opl2 is TemporaryVariableOperand && opr2 is VariableOperand)
                 {
                     TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)opl;
                     TemporaryVariableOperand tmpOperand2 = (TemporaryVariableOperand)opl2;
                     VariableOperand currentLeftVar = (VariableOperand)opr;
                     VariableOperand nextlistLeftVar = (VariableOperand)opr2;
                     if (tmpOperand == tmpOperand2  && currentLeftVar.Name == nextlistLeftVar.Name)
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }
                    }
                 }
                 else if (opl is VariableOperand && opr is ConstantOperand&& opl2 is VariableOperand && opr2 is ConstantOperand)
                 {
                     VariableOperand currentLeftVar = (VariableOperand)opl;
                     VariableOperand nextlistLeftVar = (VariableOperand)opr2;
                     ConstantOperand currentLeftConst = (ConstantOperand)opr;
                     ConstantOperand nextlistLeftConst = (ConstantOperand)opr2;
                     if (currentLeftVar.Name == nextlistLeftVar.Name && currentLeftConst.Value.Equals(nextlistLeftConst.Value))
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                
                 }
                 else if (opl is ConstantOperand && opr is ConstantOperand && opl2 is ConstantOperand && opr2 is ConstantOperand)
                 {
                     ConstantOperand currentLeftConst = (ConstantOperand)opl;
                     ConstantOperand nextlistLeftConst = (ConstantOperand)opl2;
                     ConstantOperand currentLeftConst2 = (ConstantOperand)opr;
                     ConstantOperand nextlistLeftConst2 = (ConstantOperand)opr2;
                     if (currentLeftConst.Value.Equals(nextlistLeftConst.Value) && currentLeftConst2.Value.Equals(nextlistLeftConst2.Value))
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                
                 }
                 else if (opl is ConstantOperand && opr is VariableOperand && opl2 is ConstantOperand && opr2 is VariableOperand)
                 {
                         ConstantOperand currentLeftConst = (ConstantOperand)opl;
                         ConstantOperand nextlistLeftConst = (ConstantOperand)opl2;
                         VariableOperand currentLeftVar = (VariableOperand)opr2;
                         VariableOperand nextlistLeftVar = (VariableOperand)opr2;
                         if (currentLeftConst.Value.Equals(nextlistLeftConst.Value) && currentLeftVar.Name == nextlistLeftVar.Name)
                         {
                             count++;
                            if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                
                 }
                 else if (opl is ConstantOperand && opr is TemporaryVariableOperand && opl2 is ConstantOperand && opr2 is TemporaryVariableOperand)
                 {
                     ConstantOperand currentLeftConst = (ConstantOperand)opl;
                     ConstantOperand nextlistLeftConst = (ConstantOperand)opl2;
                     TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)opr;
                     TemporaryVariableOperand tmpOperand2 = (TemporaryVariableOperand)opr2;
                     if (currentLeftConst.Value.Equals(nextlistLeftConst.Value) && tmpOperand == tmpOperand2)
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }


                    }
                }
                 else if (opl is TemporaryVariableOperand && opr is TemporaryVariableOperand&& opl2 is TemporaryVariableOperand && opr2 is TemporaryVariableOperand)
                 {
                     TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)opl;
                     TemporaryVariableOperand tmpOperand2 = (TemporaryVariableOperand)opl2;
                     TemporaryVariableOperand tmprOperand = (TemporaryVariableOperand)opr;
                     TemporaryVariableOperand tmprOperand2 = (TemporaryVariableOperand)opr2;
                     if (tmpOperand == tmpOperand2 && tmprOperand == tmprOperand2)
                     {
                         count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                 }
                 else if (opl is TemporaryVariableOperand && opr is ConstantOperand && opl2 is TemporaryVariableOperand && opr2 is ConstantOperand)
                  {   
                         TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)triads[i].LeftOperand;
                         TemporaryVariableOperand tmpOperand2 = (TemporaryVariableOperand)triads[j].LeftOperand; ;
                         ConstantOperand currentLeftConst = (ConstantOperand)triads[i].RightOperand;
                         ConstantOperand nextlistLeftConst = (ConstantOperand)triads[j].RightOperand;
                         if (tmpOperand == tmpOperand2 && currentLeftConst.Value.Equals(nextlistLeftConst.Value))
                         {
                             count++;
                        if (count == 2) { reducedTriads.Remove(triads[j]); reducedTriads.Remove(triads[j + 1]); seccount++; numdelete.Add(j); }

                    }
                  
                  }
               
                 
            }        
        }
        numdelete1.AddRange(numdelete);

        while (numdelete1.Count > 0)
        {
            for (int m = 0; m < reducedTriads.Count; m++)
            {
                if (reducedTriads[m].LeftOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriads[m].LeftOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0]) reducedTriads.Remove(triads[m]); numdelete1.RemoveAt(numdelete1.Count - 1); break;
                }
                else if (reducedTriads[m].LeftOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriads[m].RightOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0]) reducedTriads.Remove(triads[m]); numdelete1.RemoveAt(0);
                }
                else if (reducedTriads[m].LeftOperand is TemporaryVariableOperand && reducedTriads[m].LeftOperand is TemporaryVariableOperand)
                {
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriads[m].LeftOperand;
                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - 1;
                    TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)reducedTriads[m].RightOperand;

                    int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1));
                    if (numericValue == numdelete1[0] || numericValuerd == numdelete1[0]) reducedTriads.Remove(triads[m]); numdelete1.RemoveAt(0);
                }
            }

        }
        //
        reducedTriadstime.Clear();
        foreach (var triad in reducedTriads)
        {
            
            Triad newTriad = new Triad(triad.Operator, triad.LeftOperand, triad.RightOperand, triad.Result);
            reducedTriadstime.Add(newTriad);
        }


        
            int coountdelete = 0;
            int index1 = 0;
        seccount += 1;
        for (int i = reducedTriadstime.Count-1; i >= 0; i--)
        {
            
            if (i >= numdelete[0])
                if (reducedTriadstime[i].LeftOperand is TemporaryVariableOperand && reducedTriadstime[i].RightOperand is TemporaryVariableOperand)
                {
                    
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriadstime[i].LeftOperand;

                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);

                    reducedTriadstime[i].LeftOperand = tmpOperandFromInt1;

                    TemporaryVariableOperand tmpOperandrd = (TemporaryVariableOperand)reducedTriadstime[i].RightOperand;

                    int numericValuerd = int.Parse(tmpOperandrd.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromIntrd = TemporaryVariableOperand.FromInt(numericValuerd);

                    reducedTriadstime[i].RightOperand = tmpOperandFromIntrd;

                    seccount--;

                    
                }
                else if (reducedTriadstime[i].LeftOperand is TemporaryVariableOperand)
                {
                    

                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriadstime[i].LeftOperand;

                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);

                    reducedTriadstime[i].LeftOperand = tmpOperandFromInt1;

                    seccount--;
                    

                }
                else if (reducedTriadstime[i].RightOperand is TemporaryVariableOperand)
                {
                    
                    TemporaryVariableOperand tmpOperand = (TemporaryVariableOperand)reducedTriadstime[i].RightOperand;

                    int numericValue = int.Parse(tmpOperand.ToTriadString().Substring(1)) - seccount;
                    TemporaryVariableOperand tmpOperandFromInt1 = TemporaryVariableOperand.FromInt(numericValue);

                    reducedTriadstime[i].RightOperand = tmpOperandFromInt1;

                    seccount--;
                    
                   
                }



        }
        



    }


   

    

    private bool IsZeroConstantOperand(Operand operand)
    {
        return operand is ConstantOperand && Convert.ToDouble(((ConstantOperand)operand).Value) == 0;
    }
   private void Parse()
    {
        try
        {
            S();
            Console.WriteLine("Парсинг успешен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка парсинга: {ex.Message}");
        }
    }
    private void Match(string token)
    {
        if (index < input.Length && input.Substring(index).StartsWith(token))
        {
            index += token.Length;
        }
        else
        {
            throw new Exception($"Ожидался токен '{token}', но встречен '{input.Substring(index)}'");
        }
    }
    private Operand VariableOrConstant()
    {
        if (char.IsLetter(input[index]))
        {
            string variableName = input[index].ToString();
            index++;

            while (index < input.Length && (char.IsLetterOrDigit(input[index]) || input[index] == '_'))
            {
                variableName += input[index];
                index++;
            }

            return new VariableOperand(variableName);
        }
        else if (char.IsDigit(input[index]) || input[index] == '.')
        {
            string constantValue = input[index].ToString();
            index++;

            while (index < input.Length && (char.IsDigit(input[index]) || input[index] == '.'))
            {
                constantValue += input[index];
                index++;
            }

            return new ConstantOperand(double.Parse(constantValue));  // Assuming all constants are doubles for simplicity
        }
        else if (input[index] == '\'')
        {
            char charValue = CHARCONST();
            return new ConstantOperand(charValue);
        }
        else
        {
            throw new Exception($"Неожиданный символ: {input[index]}");
        }
    }
    private void S()
    {
        F();
        Match(";");
    }
    private void F()
    {
        Operand leftOperand = VariableOrConstant();
        Match(":=");
        Operand rightOperand = T();

        Triad triad = new Triad(new AssignmentOperator(), leftOperand, rightOperand);
        triads.Add(triad);
    }
    private Operand T()
    {
        Operand result = E();
        while (index < input.Length && (input[index] == '+' || input[index] == '-'))
        {
            Operator op;
            if (input[index] == '+')
            {
                Match("+");
                op = new AdditionOperator();
            }
            else if (input[index] == '-')
            {
                Match("-");
                op = new SubtractionOperator();
            }
            else
            {
                throw new Exception($"Неожиданный символ: {input[index]}");
            }
            Operand rightOperand = E();
            result = ApplyOperator(op, result, rightOperand, false);
        }
        return result;
    }

    private Operand E()
    {
        Operand result = A();
        while (index < input.Length && (input[index] == '*' || input[index] == '/'))
        {
            Operator op;
            if (input[index] == '*')
            {
                Match("*");
                op = new MultiplicationOperator();
            }
            else if (input[index] == '/')
            {
                Match("/");
                op = new DivisionOperator();
            }
            else
            {
                throw new Exception($"Неожиданный символ: {input[index]}");
            }
            Operand rightOperand = A();
            result = ApplyOperator(op, result, rightOperand,false);
        }
        return result;
    }

    private Operand A()
    {
        if (input[index] == '(')
        {
            Match("(");
            Operand result = T();
            Match(")");
            return result;
        }
        else if (input[index] == '-')
        {
            Match("-");
            Operand operand = A();
            return new UnaryMinusOperator().Apply(operand);
        }
        else
        {
            return VariableOrConstant();
        }
    }

    private Operand ApplyOperator(Operator op, Operand left, Operand right,bool nujnoli)
    {   
        Operand result = new TemporaryVariableOperand(); // Create a temporary variable for the result
        Triad triad = new Triad(op, left, right, result);
        triads.Add(triad);
        return result;
       
    }
    private char CHARCONST()
    {        
        if (input[index] == '\'')
        {
            index++;
        }
        else
        {
            throw new Exception($"Ожидалась одинарная кавычка");
        }

        if (index < input.Length && char.IsLetterOrDigit(input[index]))
        {
            char charValue = input[index];
            index++;

            if (index < input.Length && input[index] == '\'')
            {
                index++;
                return charValue;
            }
            else
            {
                throw new Exception($"Ожидалась одинарная кавычка");
            }
        }
        else
        {
            throw new Exception($"Ожидалась буква или цифра");
        }
    }

    static void Main()
    {
        string inputbd = "c:=(z+a-1)/2-(l+w+1)+('A'*'T')+'s'*'A'+1/2-(z+a-1)/21-(l+w+1)+('A'*'T');";
        string input = inputbd.Replace(" ", "");
        Parser parser = new Parser(input);
        parser.ParseAndOptimize();
    }
}

public class TemporaryVariableOperand : Operand
{
    private static int count = 1;

    private string name;

    public int NewValue
    {
        get
        {
            return int.Parse(name.Substring(1));
        }
        set
        {
            name = $"^{value}";
        }
    }

    public TemporaryVariableOperand()
    {
        name = $"^{count++}";
    }

    public static TemporaryVariableOperand Create()
    {
        return new TemporaryVariableOperand();
    }

    public static TemporaryVariableOperand FromInt(int value)
    {
        return new TemporaryVariableOperand { NewValue = value };
    }

    
    public override string ToTriadString()
    {
        return name;
    }
}


public class UnaryMinusOperator : Operator
{
    public override string ToTriadString(Operand left, Operand right)
    {
        throw new NotImplementedException();
    }
    

    public Operand Apply(Operand operand)
    {
        return new UnaryMinusResultOperand(operand);
    }
}

public class UnaryMinusResultOperand : Operand
{
    private readonly Operand operand;

    public UnaryMinusResultOperand(Operand operand)
    {
        this.operand = operand;
    }

    public Operand Operand { get; internal set; }

    public override string ToTriadString()
    {
        return $"- {operand.ToTriadString()}";
    }
   
}
