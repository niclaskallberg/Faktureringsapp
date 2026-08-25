<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication.Default" MaintainScrollPositionOnPostback="true" %>



<!DOCTYPE html>

<%-- lang har jag själv lagt till --%>
<html xmlns="http://www.w3.org/1999/xhtml" lang="sv-SE">

<head runat="server">

    <title>Faktureringsprogram</title>


    <%-- jQuery UI till skräddarsydda autocomplete-listor till textrutor --%>
    <%-- PlaceHolder-taggen är till för att kunna köra back-end-kod innanför taggen --%>
    <asp:PlaceHolder runat="server">

        <script src="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery.js") %>"></script>
        <script src="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.min.js") %>"></script>
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.min.css") %>" />
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.structure.min.css") %>" />
        <link rel="stylesheet" href="<%= VirtualPathUtility.Combine(JQueryFolderPath, "jquery-ui.theme.min.css") %>" />

    </asp:PlaceHolder>



    <%-- Href-attributet på detta element läggs till från code-behind --%>
    <link runat="server" id="CssLink" rel="stylesheet" />



    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>

</head>
    

<body>


    <form id="form1" runat="server">
        <a id="LogoLink" href='<%= Page.ResolveUrl("~/") %>'>
            <asp:Image ID="LogoImage" runat="server" ImageUrl="~/wwwroot/img/logo.png" AlternateText="Logotyp" />
        </a>



        <h1 class="screen-reader-only">Faktureringsprogram</h1>


        <%-- CSS-behållare till första raden av element --%>
        <div id="CssContainer">


            <%-- Mottagaruppgifter --%>
            <div runat="server" id="DivInvoiceRecipient">


                <asp:Label ID="LblInvoiceNumber" runat="server" Text="Fakturanummer" AssociatedControlID="TbxInvoiceNumber"></asp:Label>
                <asp:TextBox ID="TbxInvoiceNumber" runat="server" Enabled="False" AutoPostBack="true" ></asp:TextBox>

                <asp:CheckBox ID="ChbChangeInvoiceNumber" runat="server" TabIndex="-1" Text="Välj själv" AutoPostBack="true" />

                <br />


                <asp:Label ID="LblCustomerNumber" runat="server" Text="Kundnummer" AssociatedControlID="TbxCustomerNumber"></asp:Label>
                <asp:TextBox ID="TbxCustomerNumber" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections();" ></asp:TextBox>
                
                <asp:CheckBox ID="ChbIsBusinessCustomer" runat="server" onchange="toggleBusinessCustomer()" Text="Företagskund" TabIndex="-1" />

                <br />

                <asp:Label ID="LblCustomerName" runat="server" Text="Namn" AssociatedControlID="TbxCustomerName"></asp:Label>
                <asp:TextBox ID="TbxCustomerName" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>

                <br />

                <asp:Label ID="LblStreet" runat="server" Text="Gata" AssociatedControlID="TbxStreet"></asp:Label>
                <asp:TextBox ID="TbxStreet" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>
                
                <br />
                
                <asp:Label ID="LblPostalCode" runat="server" Text="Postnummer" AssociatedControlID="TbxPostalCode"></asp:Label>
                <asp:TextBox ID="TbxPostalCode" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect(); storeListboxSelections();" ></asp:TextBox>

                <br />

                <asp:Label ID="LblCity" runat="server" Text="Ort" AssociatedControlID="TbxCity"></asp:Label>
                <asp:TextBox ID="TbxCity" runat="server" oninput="this.classList.remove('auto-filled-background-color'); filterAndSelect();  storeListboxSelections(); capitalizeFirstLetter(this);" ></asp:TextBox>


                <br />

                <asp:Label ID="LblDeliveryAddress" runat="server" Text="Separat leveransadress" AssociatedControlID="ChbDeliveryAddress"></asp:Label>
                <asp:CheckBox ID="ChbDeliveryAddress" runat="server" TabIndex="-1" onclick="toggleDeliveryAddress()"/>

                <br />

                <div id="DivDeliveryAddress" runat="server" class="display-none">

                    <asp:Label ID="LblDeliveryStreet" runat="server" Text="Gata" AssociatedControlID="TbxDeliveryStreet"></asp:Label>
                    <asp:TextBox ID="TbxDeliveryStreet" runat="server" oninput="capitalizeFirstLetter(this)" ></asp:TextBox>
                
                    <br />

                    <asp:Label ID="LblDeliveryPostalCode" runat="server" Text="Postnummer" AssociatedControlID="TbxDeliveryPostalCode"></asp:Label>
                    <asp:TextBox ID="TbxDeliveryPostalCode" runat="server" ></asp:TextBox>

                    <br />

                    <asp:Label ID="LblDeliveryCity" runat="server" Text="Ort" AssociatedControlID="TbxDeliveryCity"></asp:Label>
                    <asp:TextBox ID="TbxDeliveryCity" runat="server" oninput="capitalizeFirstLetter(this)" ></asp:TextBox>

                    <br />
                </div>

                <asp:Label ID="LblPersonalIdentityNumber" runat="server" Text="Personnummer" AssociatedControlID="TbxPersonalIdentityNumber"></asp:Label>
                <asp:TextBox ID="TbxPersonalIdentityNumber" runat="server" ></asp:TextBox>

                
                

                <%-- Rensa textrutor --%>
                 <asp:Button ID="BtnClearInvoiceRecipient" runat="server" OnClientClick="clearInvoiceRecipient()" Text="Rensa textrutor" /> 




            </div>




            <%-- Kundregister --%>
            <div id="DivDatabase" runat="server">

                <h3>Hämta uppgifter</h3>

                <asp:Button ID="BtnShowOldInvoices" runat="server" Text="Se äldre fakturor" OnClientClick="getInvoices()" OnClick="BtnShowOldInvoices_Click" UseSubmitBehavior="False" />


                <div id="DivDatabaseContainer" runat="server">

                    

                    <div id="DivSelectCustomer" runat="server">

                        <h4>Kundnummer och namn</h4>
                        <select size="4" name="LbxCustomers" id="LbxCustomers" onchange="toggleAutofillButtons(); storeListboxSelections();"></select> 
                        <asp:Button ID="BtnAutofillCustomer" runat="server" Text="Använd kund" OnClientClick="autofillCustomer(); filterAndSelect(); storeListboxSelections(); return false;" />
                
                    </div>

                    <div id="DivSelectAddress" runat="server">

                        <h4>Adresser</h4>
                        <select size="4" name="LbxPostalAddresses" id="LbxPostalAddresses" onchange="toggleAutofillButtons(); storeListboxSelections();"></select> 
                        <asp:Button ID="BtnAutofillAddress" runat="server" Text="Använd adress" OnClientClick="autofillAddress(); filterAndSelect(); storeListboxSelections(); return false;" />
                
                    </div>

                    <asp:Button ID="BtnAutofillAll" runat="server" Text="Använd båda" OnClientClick="autofillCustomer(); autofillAddress(); filterAndSelect(); storeListboxSelections(); document.activeElement.blur(); return false;" />







                    <div id="DivOldInvoices" runat="server" visible="False" >

                        <asp:GridView ID="GrvInvoices" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" AllowSorting="True" AutoGenerateColumns="False" OnSorting="GrvInvoices_Sorting" >
                            <AlternatingRowStyle BackColor="White" />
                            <Columns>
                                <asp:TemplateField ShowHeader="False">
                                    <ItemTemplate>
                                        <asp:Button ID="BtnViewInvoice" runat="server" Text="Välj" OnClientClick="openInvoice(this)" OnClick="BtnViewInvoice_Click" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="fldinvoicenumber" HeaderText="Fakturanr" SortExpression="fldinvoicenumber" />
                                <asp:BoundField DataField="fldcustomernumber" HeaderText="Kundnr" SortExpression="fldcustomernumber" />
                                <asp:BoundField DataField="fldname" HeaderText="Namn" SortExpression="fldname" />
                                <asp:BoundField DataField="flddate" HeaderText="Datum" SortExpression="flddate" DataFormatString="{0:yyyy-MM-dd}"  NullDisplayText="Ingen datum" />
                                <asp:BoundField DataField="fldnetamount" HeaderText="Belopp" SortExpression="fldnetamount" />
                            </Columns>
                            <EditRowStyle BackColor="#7C6F57" />
                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                            <RowStyle BackColor="#E3EAEB" />
                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                            <SortedAscendingCellStyle BackColor="#F8FAFA" />
                            <SortedAscendingHeaderStyle BackColor="#246B61" />
                            <SortedDescendingCellStyle BackColor="#D4DFE1" />
                            <SortedDescendingHeaderStyle BackColor="#15524A" />
                        </asp:GridView>








                        <div runat="server" id="DivEditInvoice" Visible="false">








                        
                            

                            <asp:Repeater ID="RptViewInvoice" runat="server">
                                <HeaderTemplate>
                                    <div class="header-block">

                                        <h4>Faktura</h4>
                                        <table class="invoice-header">

                                            <tr><td><strong>Fakturanummer:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldinvoicenumber"] %></td></tr>

                                            <tr>
                                                <td><strong>Datum:</strong></td>

                                                <td>
                                                    <%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["flddate"] == DBNull.Value 
                                                        ? "No Date" 
                                                        : String.Format("{0:yyyy-MM-dd}", (DateTime)((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["flddate"]) %>
                                                </td>


                                            </tr>




                                            <tr>
                                                <td><strong>Förfallodatum:</strong></td>

                                                <td>
                                                    <%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldexpirationdate"] == DBNull.Value 
                                                        ? "No Date" 
                                                        : String.Format("{0:yyyy-MM-dd}", (DateTime)((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldexpirationdate"]) %>
                                                </td>


                                            </tr>



                                            <tr><td><strong>Kundnummer:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldcustomernumber"] %></td></tr>
                                            <tr><td><strong>Namn:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldname"] %></td></tr>
                                            <tr><td><strong>Gata:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldstreet"] %></td></tr>
                                            <tr><td><strong>Postnummer:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldpostalcode"] %></td></tr>
                                            <tr><td><strong>Ort:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldcity"] %></td></tr>
                                            <tr><td><strong>Leveransgata:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverystreet"] %></td></tr>
                                            <tr><td><strong>Leveransort:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverypostalcode"] %></td></tr>
                                            <tr><td><strong>Leveransstad:</strong></td><td><%# ((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["deliverycity"] %></td></tr>

                                            <tr><td><strong>Nettosumma:</strong></td><td><%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldgrossamount"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %></td></tr>
                                            <tr><td><strong>Momssumma:</strong></td><td><%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldvalueaddedtax"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %></td></tr>
                                            <tr><td><strong>RUT-avdrag:</strong></td><td><%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldrutdeduction"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %></td></tr>
                                            <tr><td><strong>Bruttosumma:</strong></td><td><%# Convert.ToDecimal(((System.Data.DataTable)RptViewInvoice.DataSource).Rows[0]["fldnetamount"], System.Globalization.CultureInfo.InvariantCulture).ToString("N2") %></td></tr>


                                        </table>
                                    </div>
                                    <hr />
                                </HeaderTemplate> 

                                <ItemTemplate>
                                    <div class="view-invoice-article-block" >
                                        <h4>Artikel <%# Container.ItemIndex + 1 %></h4>
                                        <table class="my-details-style">
                                            <tr>
                                                <td><strong>Tjänst:</strong></td>
                                                <td><asp:TextBox ID="txtService" runat="server" Text='<%# Eval("fldservicename") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>Beskrivning:</strong></td>
                                                <td><asp:TextBox ID="txtDesc" runat="server" Text='<%# Eval("flddescription") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>Leveransdatum:</strong></td>
                                                <td><asp:TextBox ID="txtDate" runat="server" Text='<%# Eval("flddeliverydate", "{0:yyyy-MM-dd}") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>Antal:</strong></td>
                                                <td><asp:TextBox ID="txtQ" runat="server" Text='<%# Eval("fldquantity") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>À-pris:</strong></td>
                                                <td><asp:TextBox ID="txtPpu" runat="server" Text='<%# Eval("fldpriceperunit") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>Total:</strong></td>
                                                <td><asp:TextBox ID="txtAmount" runat="server" Text='<%# Eval("fldamount") %>' Enabled="False" /></td>
                                            </tr>
                                            <tr>
                                                <td><strong>RUT:</strong></td>
                                                <td><asp:TextBox ID="txtIsRutDeductible" runat="server" Text='<%# Eval("fldisrutdeductible") %>' Enabled="False" /></td>
                                            </tr>


                                        </table>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        
                        




                            <asp:Button ID="BtnUpdateInvoice" runat="server" Text="Spara ändringar" OnClick="BtnUpdateInvoice_Click" Enabled="false"/>
                            <asp:Button ID="BtnDeleteInvoice" runat="server" Text="Radera faktura" OnClick="BtnDeleteInvoice_Click" OnClientClick="deleteInvoice()" />

                        </div>




                    </div>

                    
                </div>
            </div>
        </div>


        

        <%-- Artiklar --%>
        <div runat="server" id="DivArticleList">
            
            <h3>Artiklar</h3>



                <table id="TblArticleList">
                    <thead>

                        <tr>
                            <th>Tjänst/vara</th>
                            <th>Mer information</th>
                            <th>Leveransdatum</th>
                            <th>Antal</th>
                            <th>À-pris inkl. moms</th>
                            <th>Totalt</th>
                        </tr>
                    </thead>


        


                    <tbody>
                            
                        <!-- Row 1 acts as your base template and first row -->
                        <tr class="data-row">
                            <td><input type="text" name="TbxArticle[]" oninput="capitalizeFirstLetter(this)" /></td>
                            <td><input type="text" name="TbxDescription[]" oninput="capitalizeFirstLetter(this)" /></td>
                            <td><input type="date" name="TbxDeliveryDate[]" /></td>
                            <td><input type="number" name="TbxQuantity[]" value="1" step="0.5" min="0.5" class="tbxquantity" /></td>
                            <td><input type="text" name="TbxPricePerUnit[]" value="500" class="tbxpriceperunit" /></td>
                            <td><input type="text" name="TbxRowAmount[]" value="500" class="tbx-article-total" readonly="true" /></td>
                            <td>
                                <input type="checkbox" name="ChbNotRut_Placeholder" onchange="updateCheckboxValue(this)" />
                                <input type="hidden" name="ChbNotRut[]" value="false" />
                                Inget RUT-avdrag
                            </td>
                            <td>
                                <button type="button" class="btn-delete" onclick="removeRow(this)" style="display:none;">Ta bort</button>
                            </td>
                        </tr>





                    </tbody>

                </table>
                <button id="BtnAddArticle" type="button" onclick="addArticle()">Lägg till rad</button>


                <fieldset class="invoice-summary">

                    <!-- Visible to screen readers, invisible to sighted users -->
                    <legend class="screen-reader-only">Invoice Amount Breakdown</legend>


                    <asp:Label ID="LblNetAmount" runat="server" Text="Summa exkl. moms" AssociatedControlID="TbxNetAmount"></asp:Label><asp:TextBox ID="TbxNetAmount" runat="server" value="400,00"></asp:TextBox>
                    <asp:Label ID="LblValueAddedTax" runat="server" Text="Momsbelopp (25 %)" AssociatedControlID="TbxValueAddedTax"></asp:Label><asp:TextBox ID="TbxValueAddedTax" runat="server" value="100,00"></asp:TextBox>
                    <asp:Label ID="LblRutDeduction" runat="server" Text="RUT-avdrag" AssociatedControlID="TbxRutDeduction"></asp:Label><asp:TextBox ID="TbxRutDeduction" runat="server" value="250,00"></asp:TextBox>
                    <asp:Label ID="LblGrandInvoiceTotal" runat="server" Text="Att betala" AssociatedControlID="TbxGrandInvoiceTotal"></asp:Label><asp:TextBox ID="TbxGrandInvoiceTotal" runat="server" value="250,00" ></asp:TextBox>
        
                </fieldset>

            </div>

            
                

        <asp:Label ID="LblError" runat="server" ></asp:Label>

        <%-- Validera först hos klient, sedan skicka till server --%>
        <asp:Button ID="BtnCreateInvoice" runat="server" Text="Skapa faktura" OnClientClick="return validate();" OnClick="BtnCreateInvoice_Click" />


            
        <%-- Bool-värden --%>
        <asp:HiddenField ID="HdnOpenDatabaseButtonPressed" runat="server" />
        <asp:HiddenField ID="HdnInvoiceNumber" runat="server" />
        <asp:HiddenField ID="HdnConfirmDeletion" runat="server" />
        <asp:HiddenField ID="HdnConfirmCreation" runat="server" />
 

        <asp:Literal ID="LtrData" runat="server"></asp:Literal>

    </form>
</body>
</html>