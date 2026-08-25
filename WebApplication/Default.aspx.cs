using MySql.Data.MySqlClient;
using QRCoder;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Texts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Models;
using WebApplication.Utilities;



namespace WebApplication
{
    public partial class Default : Page
    {
        //Boolvärden som används för funktioner, Vid publicering sätts alla på 1
        /************************************/
        public static readonly int isLiveDatabase = 1;
        public static readonly int canLogErrors = 1;
        public static readonly int canEmailErrors = 1;
        public static readonly int canEditDatabase = 1;
        public static readonly int showInvoiceAfterCreation = 1;
        public static readonly int downloadInvoiceAfterCreation = 1;
        /************************************/


        



        //customers är en sammansättning av customerNumbers och customerNames
        private readonly List<string> customers = new List<string>();
        private readonly List<string> customerNumbers = new List<string>();
        private readonly List<string> customerNames = new List<string>();



        //addresses är en sammansättning av streets, postalCodes och cities
        private readonly List<string> addresses = new List<string>();
        private readonly List<string> streets = new List<string>();
        private readonly List<string> postalCodes = new List<string>();
        private readonly List<string> cities = new List<string>();







        //Datumformat som används för hela appen
        private readonly string dateFormat = "yyyy-MM-dd";




       







        // A property to track the success status across postbacks
        private bool IsDataLoadedSuccessfully
        {
            get { return Session["IsDataLoaded"] != null && (bool)Session["IsDataLoaded"]; }
            set { Session["IsDataLoaded"] = value; }
        }



        // Används till länkar som ska läsas av front-enden
        // ResolveUrl: används så det fungerar oavsett om appen ligger i en undermapp på domänen
        public static string AssetsFrontEnd
        {
            get
            {
                string assets = "/wwwroot/";

                // ~ betyder root-mappen av denna applikation
                // ResolveUrl slår upp addressen och gör adressen till rotmappen till något som kan läsas av en webbläsare
                return HttpContext.Current.Handler is Page page ? page.ResolveUrl('~' + assets) : assets;

            }
        }
                

        public static string JQueryFolderPath
        {
            get
            {
                return VirtualPathUtility.Combine(AssetsFrontEnd, "jquery/");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //------------------------------------------
            // Lägger till css- och javascriptfiler programmatiskt då jag vill ha med en query med filtiden för att undvika cachelagring
            //------------------------------------------


            //CSS
            //-------------------
            string cssFilePath = AssetsFrontEnd + "site.css";
            CssLink.Href = cssFilePath + "?" + File.GetLastWriteTime(Server.MapPath(cssFilePath)).ToFileTime();


            //JS
            //-------------------
            string javaScriptFilePath = AssetsFrontEnd + "site.js";
            string fullScriptPath = javaScriptFilePath + "?" + File.GetLastWriteTime(Server.MapPath(javaScriptFilePath)).ToFileTime();
            string fullScriptTag = $"<script src='{fullScriptPath}'></script>";

            //Gör så att JavaScript-filen läggs i slutet av filen
            ScriptManager.RegisterStartupScript(this, GetType(), "externalJs", fullScriptTag, false);





            //Hämta data och lägg längst ner i HTML-koden
            /******************************/

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(GetConnectionString.ConnectionString))
                {

                    mySqlConnection.Open();


                    //Kolla fakturanummer
                    /*************************/

                    List<int> invoiceNumbers = new List<int>();


                    //Nollställning ifall felmeddelande har visats
                    LblInvoiceNumber.Text = "Fakturanummer";

                    LblInvoiceNumber.ForeColor = Color.Empty;
                    LblInvoiceNumber.BackColor = Color.Empty;

                    TbxInvoiceNumber.Enabled = true;
                    TbxInvoiceNumber.TabIndex = 0;




                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "get_invoicenumbers"
                    })
                    {


                        //Hämta alla fakturanummer
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {




                            while (mySqlDataReader.Read())
                            {


                                invoiceNumbers.Add(int.Parse(mySqlDataReader.GetString("fldinvoicenumber")));
                            }
                        }
                    }



                    //Sortera fakturanummer
                    invoiceNumbers.Sort();


                    //Fakturanumren har formen av XX01, XX02 osv, där XX är nuvarande år
                    string thisYear = DateTime.Today.ToString("yy");
                    string sequenceNumber = "01";





                    //Återställ "Skapa faktura"-knappen om den har blivit disablad tidigare
                    BtnCreateInvoice.Enabled = true;


                    //Om fakturanummer ska genereras automatiskt
                    if (!ChbChangeInvoiceNumber.Checked)
                    {
                        TbxInvoiceNumber.Enabled = false;
                        TbxInvoiceNumber.TabIndex = -1;
                        TbxInvoiceNumber.Text = thisYear + sequenceNumber;
                    }


                    foreach (int invoiceNumber in invoiceNumbers)
                    {
                        //Om numret som står i rutan är upptaget
                        if (TbxInvoiceNumber.Text == invoiceNumber.ToString())
                        {
                            //Om användare fyller i fakturanumret manuellt så händer då detta
                            if (ChbChangeInvoiceNumber.Checked)
                            {
                                LblInvoiceNumber.ForeColor = Color.White;
                                LblInvoiceNumber.BackColor = Color.IndianRed;
                                LblInvoiceNumber.Text = "Fakturanumret är redan använt!";
                                BtnCreateInvoice.Enabled = false;
                            }

                            //Om fakturanummer ska anges automatiskt
                            //Följande gör att fakturanumret räknar från XX01-XX99 och blir det fler under samma år
                            //blir det XX100, XX101 osv.
                            else
                            {
                                //Om fakturanummer redan har uppgått till XX99, ändra till XX100
                                if (TbxInvoiceNumber.Text == thisYear + "99")
                                    TbxInvoiceNumber.Text = thisYear + "100";

                                //Annars addera 1
                                else
                                    TbxInvoiceNumber.Text = (invoiceNumber + 1).ToString();

                            }
                        }
                    }








                    if (!IsDataLoadedSuccessfully || LtrData.Text == "")
                    {

                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "get_personcustomers"
                        })
                        {

                            string personCustomersWithAddresses = "";
                            string businessCustomersWithAddresses = "";

                            for (int i = 0; i < 2; i++)
                            {
                                if (i == 1)
                                {
                                    mySqlCommand.CommandText = "get_businesscustomers";

                                }

                                //Ifall dessa listor har innehåll så töms det så det inte blir dubbel data
                                customerNumbers.Clear();
                                customerNames.Clear();
                                customers.Clear();

                                streets.Clear();
                                postalCodes.Clear();
                                cities.Clear();
                                addresses.Clear();



                                // Execute the command and read the results
                                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                                {

                                

                                    while (mySqlDataReader.Read())
                                    {
                                        customerNumbers.Add(mySqlDataReader.GetString("fldcustomernumber").Trim());
                                        customerNames.Add(mySqlDataReader.GetString("fldname").Trim());

                                        customers.Add((customerNumbers.Last() + " " + customerNames.Last()).Trim());


                                        streets.Add(mySqlDataReader.GetString("fldstreet").Trim());
                                        postalCodes.Add(mySqlDataReader.GetString("fldpostalcode").Trim());
                                        cities.Add(mySqlDataReader.GetString("fldcity").Trim());


                                        string address = "";

                                        if (streets.Last() != "")
                                            address = streets.Last() + ", ";

                                        if (postalCodes.Last() != "")
                                            address += postalCodes.Last() + " ";

                                        if (cities.Last() != "")
                                            address += cities.Last();

                                        address = address.Trim();

                                        if (address.StartsWith(","))
                                            address = address.Remove(0);

                                        if (address.EndsWith(","))
                                            address = address.Remove(address.Length - 1);


                                        addresses.Add(address.Trim());





                                        //$ = Interpolation, gör att du kan lägga in variabler mellan {}
                                        //JsonSerializer = Escapear tecken som " och \ samt gör så ingen kan lägga in exempelvis "</script>" i databasen
                                        if (i == 0)
                                            personCustomersWithAddresses =
                                                $"const personCustomers = {JsonSerializer.Serialize(customers)};" +
                                                $"const personCustomerNumbers = {JsonSerializer.Serialize(customerNumbers)};" +
                                                $"const personCustomerNames = {JsonSerializer.Serialize(customerNames)};" +
                                                $"const personAddresses = {JsonSerializer.Serialize(addresses)};" +
                                                $"const personStreets = {JsonSerializer.Serialize(streets)};" +
                                                $"const personPostalCodes = {JsonSerializer.Serialize(postalCodes)};" +
                                                $"const personCities = {JsonSerializer.Serialize(cities)};";


                                    

                                        else
                                            businessCustomersWithAddresses =
                                                $"const businessCustomers = {JsonSerializer.Serialize(customers)};" +
                                                $"const businessCustomerNumbers = {JsonSerializer.Serialize(customerNumbers)};" +
                                                $"const businessCustomerNames = {JsonSerializer.Serialize(customerNames)};" +
                                                $"const businessAddresses = {JsonSerializer.Serialize(addresses)};" +
                                                $"const businessStreets = {JsonSerializer.Serialize(streets)};" +
                                                $"const businessPostalCodes = {JsonSerializer.Serialize(postalCodes)};" +
                                                $"const businessCities = {JsonSerializer.Serialize(cities)};";




                                        //Kommer läggas längst ner i html-koden
                                        Session["customersandaddresses"] = personCustomersWithAddresses + businessCustomersWithAddresses;


                                    }
                                }
                            }
                        }
                        
                    






                        // Hämta tjänster från databas och lägg i autocomplete-lista när sidan laddas första gången
                        /****************************************/
                        
                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "get_servicenames"
                        })
                        {


                            string serviceNames = "const services = [";

                            using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                            {
                                while (mySqlDataReader.Read())
                                    serviceNames += "'" + mySqlDataReader.GetString("fldservicename") + "',";
                            }

                            if (serviceNames.EndsWith(","))
                                serviceNames = serviceNames.Remove(serviceNames.Length - 1);

                            serviceNames += "];";

                            Session["serviceNames"] = serviceNames;

                        }
                    }

                    //Lagra datan från databasen i html-koden så det kan användas av JavaScript
                    LtrData.Text = $"<script>{Session["customersandaddresses"]}{Session["serviceNames"]}</script>";



                    // If we reach this line, mark it as successful
                    IsDataLoadedSuccessfully = true;
                }
            }

            catch (MySqlException ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);


                //Mark as failed so it retries on the next postback
                IsDataLoadedSuccessfully = false;
            }
        }






        //Page_Load kommer före click-metoder




        protected void BtnShowOldInvoices_Click(object sender, EventArgs e)
        {
            DivOldInvoices.Visible = true;
            GrvInvoices.Visible = true;
            DivEditInvoice.Visible = false;

            BindGrid();
        }

        protected void GrvInvoices_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Toggle direction string cleanly using ViewState (safer than Session)
            string dir = (ViewState["Dir"]?.ToString() == "ASC") ? "DESC" : "ASC";
            ViewState["Dir"] = dir;

            // Pass the combined sort expression directly to your database loader
            BindGrid(e.SortExpression + " " + dir);
        }


        private void BindGrid(string sortExpression = "")
        {
            try
            {

                using (MySqlConnection conn = new MySqlConnection(GetConnectionString.ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("get_invoices", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("fldinvoicenumber");
                        dt.Columns.Add("fldcustomernumber");
                        dt.Columns.Add("fldname");
                        dt.Columns.Add("flddate", typeof(DateTime));
                        dt.Columns.Add("fldnetamount", typeof(int));

                        new MySqlDataAdapter(cmd).Fill(dt);

                        // Apply sorting if an expression is provided
                        if (!string.IsNullOrEmpty(sortExpression))
                        {
                            dt.DefaultView.Sort = sortExpression;
                        }

                        GrvInvoices.DataSource = dt;
                        GrvInvoices.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);
            }
        }






        protected void BtnViewInvoice_Click(object sender, EventArgs e)
        {


            GrvInvoices.Visible = false;
            DivEditInvoice.Visible = true;

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(GetConnectionString.ConnectionString))
                {

                    mySqlConnection.Open();


                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandText = "view_invoice",
                        CommandType = CommandType.StoredProcedure
                    })
                    {

                        mySqlCommand.Parameters.AddWithValue("parameter_invoicenumber", HdnInvoiceNumber.Value);
                        mySqlCommand.Parameters["parameter_invoicenumber"].Direction = ParameterDirection.Input;


                        DataTable dataTable = new DataTable();
                        new MySqlDataAdapter(mySqlCommand).Fill(dataTable);
                        RptViewInvoice.DataSource = dataTable;
                        RptViewInvoice.DataBind();


                    }
                }
            }

            catch (MySqlException ex)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.RetrieveFailed, ex);
            }

        }


        protected void BtnUpdateInvoice_Click(object sender, EventArgs e)
        {
            //Kommer läggas till i framtiden
        }




        protected void BtnDeleteInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(GetConnectionString.ConnectionString))
                {
                    mySqlConnection.Open();

                    // Start the C# controlled transaction
                    using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
                    {


                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            Transaction = mySqlTransaction,
                            CommandText = "delete_invoice",
                            CommandType = CommandType.StoredProcedure
                        })
                        {

                            mySqlCommand.Parameters.AddWithValue("invoicenumber", HdnInvoiceNumber.Value);
                            mySqlCommand.Parameters["invoicenumber"].Direction = ParameterDirection.Input;

                            int rowsAffected = mySqlCommand.ExecuteNonQuery();

                            

                            if (rowsAffected != 1)
                                throw new Exception($"Oväntat antal rader påverkade: {rowsAffected}");

                            // If it reaches here, exactly 1 row was deleted
                            mySqlTransaction.Commit();


                            JsHelpers.ShowAlert("Faktura med nummer " + HdnInvoiceNumber.Value + " är raderad!");
                            JsHelpers.DoAny("document.getElementById('" + BtnShowOldInvoices.ID + "').click()");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                // Note: The transaction object will automatically roll back when disposed by its 'using' block if .Commit() was never called.
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.DeleteFailed, ex);
            }

        }




        //AJAX
        //Validera användare mot databas när faktura skapas
        [WebMethod]
        public static string ValidateCustomer(string CustomerNumber, string CustomerName)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(GetConnectionString.ConnectionString))
                {
                    

                    mySqlConnection.Open();



                    using (MySqlCommand mySqlCommand = new MySqlCommand
                    {
                        Connection = mySqlConnection,
                        CommandType = CommandType.Text,
                        CommandText = "CALL check_customer('" + CustomerNumber + "', '" + CustomerName + "');"
                    })
                    {

                        string returnValue = "";





                        //Ska returnera "1" eller "2" eller tom sträng
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                returnValue = mySqlDataReader.GetInt32("@prompt").ToString();
                            }
                        }



                        
                        return returnValue;




                    }
                }
            }
            catch (Exception ex)
            {
                return ErrorHandler.GetErrorMessage(ErrorHandler.ErrorTypes.InvoiceCreationFailed, ex);
            }

        }










        protected void BtnCreateInvoice_Click(object sender, EventArgs e)
        {
            

            Invoice invoice = new Invoice
            {
                InvoiceNumber = TbxInvoiceNumber.Text,
                CustomerNumber = TbxCustomerNumber.Text,
                CustomerName = TbxCustomerName.Text,

                BillingAddress = new Address
                {
                    Street = TbxStreet.Text,
                    PostalCode = TbxPostalCode.Text,
                    City = TbxCity.Text
                },

                DeliveryAddress = new Address()
                {
                    Street = TbxDeliveryStreet.Text,
                    PostalCode = TbxDeliveryPostalCode.Text,
                    City = TbxDeliveryCity.Text

                }

            };



            // Extract finalized layout data for database entry
            List<InvoiceArticleItem> finalArticles = new List<InvoiceArticleItem>();





            string[,] articleTextOnInvoice = new string[5, 6];



            for (int i = 0; i < articleTextOnInvoice.GetLength(0); i++)
            {
                for (int j = 0; j < articleTextOnInvoice.GetLength(1); j++)
                {
                    articleTextOnInvoice[i, j] = "";
                }
            }




            // Retrieve the raw form values as comma-separated strings
            string rawArticles = Request.Form["TbxArticle[]"];

            // If the table is empty or wasn't submitted safely
            if (string.IsNullOrEmpty(rawArticles)) return;

            // Split them into clean, index-aligned arrays
            string[] articles = rawArticles.Split(',');

            string[] descriptions = Request.Form["TbxDescription[]"].Split(',');
            string[] deliveryDates = Request.Form["TbxDeliveryDate[]"].Split(',');
            string[] quantities = Request.Form["TbxQuantity[]"].Split(',');
            string[] prices = Request.Form["TbxPricePerUnit[]"].Split(',');
            string[] amounts = Request.Form["TbxRowAmount[]"].Split(',');
            string[] rutFlags = Request.Form["ChbNotRut[]"].Split(',');


            //MessageBox.Show(articles.Length.ToString());
            // Loop through the items safely using the array length
            for (int i = 0; i < articles.Length; i++)
            {
                string article = articles[i].Trim();
                string description = descriptions[i].Trim();
                string deliveryDate = deliveryDates[i].Trim();
                string quantity = quantities[i].Trim().Replace('.',',');
                string price = prices[i].Trim().Replace('.', ',');
                string amount = amounts[i].Trim().Replace('.', ',');
                bool isNotRut = Convert.ToBoolean(rutFlags[i]);


                finalArticles.Add(new InvoiceArticleItem
                {
                    Article = article,
                    Description = description,
                    DeliveryDate = string.IsNullOrWhiteSpace(deliveryDate) ? (DateTime?)null : DateTime.Parse(deliveryDate),
                    Quantity = decimal.Parse(quantity),
                    PricePerUnit = decimal.Parse(price),
                    Amount = decimal.Parse(amount),
                    IsNotRut = isNotRut


                });


                articleTextOnInvoice[i, 0] = finalArticles[i].Article;
                articleTextOnInvoice[i, 1] = finalArticles[i].Description;



                articleTextOnInvoice[i, 2] = finalArticles[i].DeliveryDate?.ToString(dateFormat) ?? "";

                articleTextOnInvoice[i, 3] = finalArticles[i].Quantity.ToString();
                articleTextOnInvoice[i, 4] = finalArticles[i].PricePerUnit.ToString();
                articleTextOnInvoice[i, 5] = finalArticles[i].Amount.ToString();




            }



            invoice.NetAmount = decimal.Parse(TbxNetAmount.Text);
            invoice.ValueAddedTax = decimal.Parse(TbxValueAddedTax.Text);
            invoice.RutDeduction = decimal.Parse(TbxRutDeduction.Text);
            invoice.GrossAmount = decimal.Parse(TbxGrandInvoiceTotal.Text);








            //Skapa PDF med Spire.PDF NuGet-paket
            //////////////////////////////////////////////
            

            string pdfDirectoryPath = Server.MapPath("~/App_Data/pdf/");

            // Create a PdfDocument object
            PdfDocument pdfDocument = new PdfDocument();

            // Load a PDF file
            pdfDocument.LoadFromFile(pdfDirectoryPath + "Fakturamall.pdf");

            // Get page
            PdfPageBase pdfPageBase = pdfDocument.Pages[0];

            // Create a PdfTextReplacer object based on page
            PdfTextReplacer pdfTextReplacer = new PdfTextReplacer(pdfPageBase);

            //Set properties and variables
            invoice.CreationDate = DateTime.Today;
            invoice.DueDate = DateTime.Today.AddDays(15);
            string oldPaymentDays = "15 dagar";
            string newPaymentDays = oldPaymentDays;


            string oldDeliveryAddressSection = "Samma som betalningsadress";
            string newDeliveryAddressSection = oldDeliveryAddressSection;



            string oldRutInfoFirstRow = "RUT-avdrag (50%) görs på allt utom resa och söks";
            string oldRutInfoSecondRow = "hos Skatteverket på personnummer";
            string newRutInfoFirstRow = oldRutInfoFirstRow;
            string newRutInfoSecondRow = oldRutInfoSecondRow;



            if (ChbDeliveryAddress.Checked)
                newDeliveryAddressSection = "";


            if (ChbIsBusinessCustomer.Checked)
            {
                invoice.DueDate = DateTime.Today.AddDays(30);
                newPaymentDays = "30 dagar";
                newRutInfoFirstRow = "";
                newRutInfoSecondRow = "";
            }



            string[] oldText = {
                "[Datum]",
                "[Kundnummer]",
                "[Fakturanummer]",
                "[Namn]",
                "[Gata]",
                "[Postnummer] [Postort]",
                "[Förfallodatum]",
                "[Belopp]",
                "[Personnummer]",
                "[Summa exkl. moms]",
                "[Momsbelopp]",
                "[RUT-avdrag]",
                oldDeliveryAddressSection,
                oldPaymentDays,
                oldRutInfoFirstRow,
                oldRutInfoSecondRow
            };

            /*Där det står "" är ställen där koordinater måste användas
            istället eftersom det behövs speciella typsnitt till dem*/
            string[] newText =
            {
                invoice.CreationDate.ToString(dateFormat),
                invoice.CustomerNumber,
                invoice.InvoiceNumber,
                "",
                "",
                "",
                "",
                "",
                TbxPersonalIdentityNumber.Text,
                Convert.ToInt32(invoice.NetAmount).ToString() + " kr",
                Convert.ToInt32(invoice.ValueAddedTax).ToString() + " kr",
                Convert.ToInt32(invoice.RutDeduction).ToString() + " kr",
                newDeliveryAddressSection,
                "",
                newRutInfoFirstRow,
                newRutInfoSecondRow
            };



            string expirationDateAsString = invoice.DueDate.ToString(dateFormat);

            //Konvertera till heltal
            string totalAmountAsString = Convert.ToInt32(invoice.GrossAmount).ToString();

            string totalAmountAsStringWithCurrency = totalAmountAsString + " SEK";












            //Hämta koordinater för specifika textstycken
            //-------------------------------------------

            float recipientInfoXCoordinate = 0f;
            float recipientInfoYCoordinate = 0f;


            float deliveryAddressX = 0f;
            float deliveryAddressY = 0f;

            float amountOfPaymentDaysX = 0f;
            float amountOfPaymentDaysY = 0f;



            //Y-positionerna blir något felaktiga så detta kommer läggas till på Y-axeln
            float yOffset = -3f;





            using (PdfTextFinder pdfTextFinder = new PdfTextFinder(pdfPageBase))
            {
                List<PdfTextFragment> pdfTextFragments = pdfTextFinder.Find("[Namn]");

                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    recipientInfoXCoordinate = pdfTextFragment.Positions[0].X;
                    recipientInfoYCoordinate = pdfTextFragment.Positions[0].Y + yOffset;
                }



                pdfTextFragments = pdfTextFinder.Find(oldDeliveryAddressSection);

                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    deliveryAddressX = pdfTextFragment.Positions[0].X;
                    deliveryAddressY = pdfTextFragment.Positions[0].Y + yOffset;
                }





                //Find text with number of payment days
                pdfTextFragments = pdfTextFinder.Find(oldPaymentDays);

                //Loop through the instances
                foreach (PdfTextFragment pdfTextFragment in pdfTextFragments)
                {
                    //Get the position of a specific instance
                    amountOfPaymentDaysX = pdfTextFragment.Positions[0].X;
                    amountOfPaymentDaysY = pdfTextFragment.Positions[0].Y + yOffset;
                }
            }





            //Ersätt alla förekomster av måltext med ny text
            for (int i = 0; i < oldText.Length; i++)
            {
                pdfTextReplacer.ReplaceAllText(oldText[i], newText[i]);
            }


            if (ChbIsBusinessCustomer.Checked)
                pdfTextReplacer.ReplaceText("inkl. moms", "exkl. moms");







            string[] itemsPlaceholderText = {
                "[Artikel]", 
                "[Beskrivning]",
                "[Leveransdatum]",
                "[Antal]", 
                "[A-pris]", 
                "[Art.Belopp]", 
            };




            //Lägg in texten för artiklarna på räkningen
            for (int i = 0; i < articleTextOnInvoice.GetLength(0); i++)
            {
                for (int j = 0; j < articleTextOnInvoice.GetLength(1); j++)
                {
                    pdfTextReplacer.ReplaceText(itemsPlaceholderText[j], articleTextOnInvoice[i, j]);


                }

            }











            //Placera text med koordinater
            /////////////////////////////////////////////////


            PdfTrueTypeFont recipientFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/palab.ttf", 12.5f);
            PdfTrueTypeFont deliveryAddressFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/pala.ttf", 11.8f);
            PdfTrueTypeFont expirationDateFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "palatino_linotype/palab.ttf", 11.8f);
            PdfFont firstTotalAmountFont = new PdfFont(PdfFontFamily.Helvetica, 16f, PdfFontStyle.Bold);
            PdfFont secondTotalAmountFont = new PdfFont(PdfFontFamily.Helvetica, 12.5f, PdfFontStyle.Bold);


            PdfTrueTypeFont numberOfPaymentDaysFont = PdfTrueTypeFont.FromFontFile(pdfDirectoryPath + "segoe_ui/segoeuib.ttf", 9.6f);


            string recipientInfo = invoice.CustomerName + "\n" + invoice.BillingAddress.Street + "\n" + (invoice.BillingAddress.PostalCode + " " + invoice.BillingAddress.City).Trim();

            PdfTextWidget pdfTextWidget1 = new PdfTextWidget(invoice.DeliveryAddress.Street + "\n" + (invoice.DeliveryAddress.PostalCode + " " +
                invoice.DeliveryAddress.City).Trim(), deliveryAddressFont, PdfBrushes.Black);

            PdfTextWidget pdfTextWidget2 = new PdfTextWidget(expirationDateAsString, expirationDateFont, PdfBrushes.Black);
            PdfTextWidget pdfTextWidget3 = new PdfTextWidget(totalAmountAsStringWithCurrency, firstTotalAmountFont, PdfBrushes.Black);
            PdfTextWidget pdfTextWidget4 = new PdfTextWidget(totalAmountAsStringWithCurrency, secondTotalAmountFont, PdfBrushes.Black);




            pdfPageBase.Canvas.DrawString(recipientInfo, recipientFont, PdfBrushes.Black, recipientInfoXCoordinate, recipientInfoYCoordinate);
            pdfTextWidget1.Draw(pdfPageBase, deliveryAddressX, deliveryAddressY);
            pdfTextWidget2.Draw(pdfPageBase, new PointF(442.8f, 270f), new PdfTextLayout());
            pdfTextWidget3.Draw(pdfPageBase, new PointF(445.2f, 306.2f), new PdfTextLayout());
            pdfPageBase.Canvas.DrawString(newPaymentDays, numberOfPaymentDaysFont, PdfBrushes.Black, amountOfPaymentDaysX, amountOfPaymentDaysY);
            pdfTextWidget4.Draw(pdfPageBase, new PointF(437f, 727f), new PdfTextLayout());




            //QR code package
            //--------------------//

            string swishUrl = "https://app.swish.nu/1/p/sw/?sw=46706692286&amt=" + totalAmountAsString + "&msg=Faktura " + invoice.InvoiceNumber;
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(swishUrl, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode bitmapByteQRCode = new BitmapByteQRCode(qRCodeData);
            byte[] qrCodeAsBitmapBytes = bitmapByteQRCode.GetGraphic(20);


            //Place QR code
            using (MemoryStream memoryStream = new MemoryStream(qrCodeAsBitmapBytes))
            {
                PdfImage pdfImage = PdfImage.FromStream(memoryStream);

                //Specify the X and Y coordinates to start drawing the image
                float x = 74f;
                float y = 218.9f;

                //Specify the width and height of the image on the page
                float widthHeight = 75f;

                // Draw QR code at specified location
                pdfPageBase.Canvas.DrawImage(pdfImage, x, y, widthHeight, widthHeight);
            }



            string newFileName = "Faktura " + invoice.InvoiceNumber;


            pdfDocument.DocumentInformation.Title = newFileName;
            pdfDocument.DocumentInformation.Author = "Lillemors Allservice";


            // Save new PDF
            pdfDocument.SaveToFile(Server.MapPath("~/wwwroot/Ny_faktura.pdf"));

            // Dispose resources
            pdfDocument.Dispose();











            //Infoga i databas
            /*****************************/

            MySqlTransaction transaction = null;

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(GetConnectionString.ConnectionString))
                {
                    mySqlConnection.Open();


                    int isBusinessCustomer = 0;

                    if (ChbIsBusinessCustomer.Checked)
                        isBusinessCustomer = 1;

                    NumberFormatInfo numberFormatInfo = new NumberFormatInfo
                    {
                        NumberDecimalSeparator = "."
                    };


                    //Kvitto som ska lagras i databas
                    string receipt =
                        $"Datum: {invoice.CreationDate.ToString(dateFormat)},\\n" +
                        $"Kundnummer: {invoice.CustomerNumber},\\n" +
                        $"Fakturanummer: {invoice.InvoiceNumber},\\n" +
                        $"Företagskund (1=Ja, 0=Nej): {isBusinessCustomer},\\n" +
                        $"Namn: {invoice.CustomerName},\\n" +
                        $"Förfallodatum: {invoice.DueDate.ToString(dateFormat)},\\n" +
                        $"Totalsumma brutto: {invoice.NetAmount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Moms: {invoice.ValueAddedTax.ToString("0.00", numberFormatInfo)},\\n" +
                        $"RUT-avdrag: {invoice.RutDeduction.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Totalsumma netto: {invoice.GrossAmount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Gata: {invoice.BillingAddress.Street},\\n" +
                        $"Postnummer: {invoice.BillingAddress.PostalCode},\\n" +
                        $"Ort: {invoice.BillingAddress.City},\\n" +
                        $"Gata (Leveransadress): {invoice.DeliveryAddress.Street},\\n" +
                        $"Postnummer (Leveransadress): {invoice.DeliveryAddress.PostalCode},\\n" +
                        $"Ort (Leveransadress): {invoice.DeliveryAddress.City}";

                    foreach (var item in finalArticles)
                    {
                        receipt += ",\\n\\n" +
                        $"Artikel: {item.Article},\\n" +
                        $"Beskrivning: {item.Description},\\n" +
                        $"Leveransdatum: {item.DeliveryDate},\\n" +
                        $"Antal: {item.Quantity.ToString(numberFormatInfo)},\\n" +
                        $"À-pris exkl. moms: {item.PricePerUnit.ToString("0.00", numberFormatInfo)},\\n" +
                        $"Belopp inkl. moms: {item.Amount.ToString("0.00", numberFormatInfo)},\\n" +
                        $"RUT-avdrag (1=Ja, 0=Nej): {!item.IsNotRut}";
                    }

                    


                    List<string> insertFields = new List<string> {
                    invoice.CreationDate.ToString(dateFormat),
                    invoice.CustomerNumber,
                    invoice.InvoiceNumber,
                    isBusinessCustomer.ToString(),
                    invoice.CustomerName,
                    invoice.DueDate.ToString(dateFormat),
                    invoice.NetAmount.ToString(numberFormatInfo),
                    invoice.ValueAddedTax.ToString(numberFormatInfo),
                    invoice.RutDeduction.ToString(numberFormatInfo),
                    invoice.GrossAmount.ToString(numberFormatInfo),
                    receipt,
                    invoice.BillingAddress.Street,
                    invoice.BillingAddress.PostalCode,
                    invoice.BillingAddress.City,
                    invoice.DeliveryAddress.Street,
                    invoice.DeliveryAddress.PostalCode,
                    invoice.DeliveryAddress.City,
                    finalArticles.Count.ToString()
                };



                    foreach (var item in finalArticles)
                    {
                        insertFields.AddRange(new string[]
                        {
                        item.Article,
                        item.Description,
                        item.DeliveryDate.ToString(),
                        item.Quantity.ToString(numberFormatInfo),
                        item.PricePerUnit.ToString(numberFormatInfo),
                        item.Amount.ToString(numberFormatInfo),
                        Convert.ToInt32(!item.IsNotRut).ToString()
                        });
                    }



                    while (insertFields.Count < 53)
                    {
                        insertFields.Add("");
                    }



                    for (int i = 0; i < insertFields.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(insertFields[i]) || insertFields[i] == "0001-01-01")
                            insertFields[i] = "NULL";

                        else
                            insertFields[i] = "'" + insertFields[i] + "'";

                    }






                    //Anropa MySql-procedur som lägger in datan och lägg
                    //kommatecken mellan varje element så det kan läggas in
                    string insertData = "CALL insert_data(" + insertFields[0];

                    for (int i = 1; i < insertFields.Count; i++)
                        insertData += "," + insertFields[i];

                    insertData += ");";

                    using (transaction = mySqlConnection.BeginTransaction())
                    {
                        using (MySqlCommand mySqlCommand = new MySqlCommand
                        {
                            Connection = mySqlConnection,
                            CommandText = insertData,
                            CommandType = CommandType.Text
                        })
                        {

                        



                            //Exekvera procedur
                            if (canEditDatabase == 1)
                            {
                                mySqlCommand.ExecuteNonQuery();

                            
                                // Commit only if everything succeeded
                                transaction.Commit();
                            }



                            //Töm sessioner eftersom faktura är skapad
                            Session.Remove("addressesRetrieved");
                            Session.Remove("serviceNames");
                            Session.Remove("invoiceNumbers");
                            IsDataLoadedSuccessfully=false;

                            if (showInvoiceAfterCreation == 1)
                            {
                                Response.Redirect("/WebForm1.aspx?" + newFileName, false);
                                Context.ApplicationInstance.CompleteRequest();
                                //False på andra redirect-parametern tillsammans med CompleteRequest() gör så det inte blir ett ThreadAbortException


                            }
                        }
                    }
                }
            }

            catch (MySqlException ex)
            {
                transaction?.Rollback();
                ErrorHandler.HandleError(ErrorHandler.ErrorTypes.InvoiceCreationFailed, ex);
            }
        }
    }
}