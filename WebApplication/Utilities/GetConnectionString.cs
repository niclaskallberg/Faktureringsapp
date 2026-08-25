using System;
using System.Configuration;
using System.Web;
using System.Web.Hosting;

namespace WebApplication.Utilities
{
    //Ordet static på raden nedanför samt innehållet i klassen är tillagt av mig
    public static class GetConnectionString
    {
        //Connection string
        /********************************/

        //Startsida
        //Se äldre fakturor
        //Visa äldre faktura
        //Radera faktura
        //Validera mot databas
        //Lägg in


        // The public property just returns the pre-calculated value
        public static string ConnectionString => _connectionString.Value;

        // Define a private Lazy field
        private static readonly Lazy<string> _connectionString = new Lazy<string>(() =>
        {
            // This logic only runs ONCE, the very first time .Value is accessed
            string name = Default.isLiveDatabase == 1 ? "ConnectionStringProduction" : "ConnectionStringDevelopment";
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString ?? "";
        });
        /********************************/




    }
}