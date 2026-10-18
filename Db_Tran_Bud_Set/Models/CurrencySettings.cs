   using System;

   namespace GCAT.Models;

   public static class CurrencySettings
   {
       public static string Symbol { get; private set; } = "\u20B1";

       public static event Action? Changed;

       public static void SetFromLabel(string label)
       {
           if (string.IsNullOrWhiteSpace(label)) return;

           var symbol = label.Trim().Split(' ')[0];
           if (symbol == Symbol) return;

           Symbol = symbol;
           Changed?.Invoke();
       }
   }