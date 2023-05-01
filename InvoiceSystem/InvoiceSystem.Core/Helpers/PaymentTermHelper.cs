using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace InvoiceSystem.Core.Helpers
{
    public static class PaymentTermHelper
    {
        public static TimeSpan? GetTimeSpan(string input)
        {
            var match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                int days = int.Parse(match.Value);
                return TimeSpan.FromDays(int.Parse(match.Value));
            }
            else
                return null;
        }
    }
}
