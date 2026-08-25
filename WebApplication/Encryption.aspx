<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="System.Configuration" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {


        //Den här filen är en kryptering för connection-strängarna som en extra säkerhetsåtgärd
        //Gå till startsidan först så du inte blir redirectad dit och gå sedan till dinurl.com/encryption.aspx så körs detta
        //Efter det kan denna fil raderas från servern
        try
        {
            // 1. Open the web.config
            Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);

            // 2. Get the connectionStrings section
            ConfigurationSection section = config.GetSection("connectionStrings");

            // 3. Check if it's already protected
            if (section != null && !section.SectionInformation.IsProtected)
            {
                // 4. Encrypt using the server's Machine Key
                section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                section.SectionInformation.ForceSave = true;


                // 5. Save the changes back to App_Data\settings.config
                config.Save();

                Response.Write("<h2 style='color:green;'>✅ Success!</h2>");
                Response.Write("<p>Your <b>App_Data\\settings.config</b> file is now encrypted.</p>");

                //Stänger av appen så den oktyptrade filen inte sparas i minnet
                System.Web.HttpRuntime.UnloadAppDomain();
            }
            else
            {
                Response.Write("<h2 style='color:blue;'>ℹ️ Already Encrypted</h2>");
                Response.Write("<p>The section was already protected by the server.</p>");

            }
        }
        catch (Exception ex)
        {
            Response.Write("<h2 style='color:red;'>❌ Error</h2>");
            Response.Write("<p>" + ex.Message + "</p>");

        }
    }

</script>