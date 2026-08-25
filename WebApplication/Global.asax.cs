using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.UI.HtmlControls;
using System.Windows;

namespace WebApplication
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {




        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }









        // Nedan metod har jag lagt till själv
        // Ser till så man inte kan börja på en annan aspx-sida än startsidan
        //This method is the earliest point in the lifecycle where you can access Session data, as it has just been fully populated.
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            // 1. Ensure the HTTP context and Session state are initialized for this request
            if (Context == null || Context.Session == null)
            {
                return;
            }


            string path = Request.Url.AbsolutePath.ToLower();

            if (path == "/default.aspx")
                Session["UserHasVisitedHome"] = 1;


            else if (Session["UserHasVisitedHome"] == null)
            {
                Response.Redirect("~/");

            }
        }



        //Lagt till själv
        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (Context.Handler is System.Web.UI.Page page)
            {
                // Hook into the Page's Init event to add the meta tag
                page.Init += (pageSender, pageInitializationEventArguments) =>
                {
                    List<HtmlMeta> htmlMetaList = new List<HtmlMeta>
                    {
                        new HtmlMeta
                        {
                            Name = "robots",
                            Content = "noindex, nofollow"
                        },

                        //Hindrar Google Chrome att uppmana till översättning eftersom det ska alltid vara på svenska
                        new HtmlMeta
                        {
                            Name = "google",
                            Content = "notranslate"

                        }
                    };

                    // 3. Loop through the list and add each tag to the page <head> section
                    if (page.Header != null)
                    {
                        foreach (HtmlMeta htmlMeta in htmlMetaList)
                        {
                            page.Header.Controls.Add(htmlMeta);
                        }
                    }
                };
            }
        }


        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}