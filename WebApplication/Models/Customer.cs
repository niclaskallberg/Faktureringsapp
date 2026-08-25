using System.Collections.Generic;

namespace WebApplication.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }




        // Change this to a List to allow multiple addresses
        public List<Address> Addresses { get; set; }

        // Constructor to initialize the list
        public Customer()
        {
            Addresses = new List<Address>();
        }


    }
}