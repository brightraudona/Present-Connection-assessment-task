using InvoiceSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.Data.Entities
{
    public class Individual
    {
        public Individual(string name)
        {
            var nameParts = name.Split(' ');
            FirstName = nameParts[0];
            if (nameParts.Length == 1)
                LastName = "";
            else if (nameParts.Length == 2)
                LastName = nameParts[1];
            else
                LastName = name.Replace(nameParts[0] + ' ', "");
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
