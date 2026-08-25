using System;
using System.Web;
using System.Web.UI;

namespace WebApplication
{
    public partial class WebForm2 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            

            if (Default.downloadInvoiceAfterCreation == 1)
            {
                Response.ContentType = "Application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlDecode(Request.QueryString.ToString()) + ".pdf;");
                Response.WriteFile(Server.MapPath("~/wwwroot/Ny_faktura.pdf"));
            }
        }
    }
}