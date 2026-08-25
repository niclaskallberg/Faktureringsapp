using System;
using System.Web.UI;

namespace WebApplication
{
    public partial class WebForm1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            DummyFrame.Src = "~/WebForm2.aspx" + Request.Url.Query;

        }
    }
}