<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script>
        window.onload = function () {

            window.location.replace('/wwwroot/Ny_faktura.pdf');

        };
    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div>
            <iframe id="DummyFrame" runat="server" hidden="hidden"></iframe>
        </div>
    </form>
</body>
</html>
