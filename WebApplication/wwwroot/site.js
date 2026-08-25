document.addEventListener("DOMContentLoaded", function () {
    //DOMContentLoaded körs sist vid laddning

    /*----------------------*/



    // // The opening parenthesis turns the function into an expression
    // (function example() {


    // })
    //     // These final parentheses instantly execute the code
    //     ();

    /*----------------------*/





    /*
    textField.addEventListener("input", function() {
      alert('You just wrote!');
    });

    
 
        textField.addEventListener("input", () => {
       alert('You just wrote!');
     });
 
    
   
         How to Access the Textfield Value Inside
  
        // Using an arrow function to read the textfield value
      textField.addEventListener("input", (e) => {
        console.log("Current text:", e.target.value);
      });
  
    */






    //--------------------------------//
    // Skapa nästa faktura
    //--------------------------------//

    /*När du skapat en faktura och går tillbaka till föregående webbläsarflik, 
    fråga användare om den vill göra en hård omladdning av sidan för att rensa cache-minne*/
    document.addEventListener("visibilitychange", () => {

        if (hdnConfirmCreation.value === '1' && document.visibilityState === 'visible')

            if (confirm("Vill du rensa sidan?")) {



                // 1. Wipe session storage (temporary data for this tab)
                sessionStorage.clear();

                // 2. Wipe local storage (persistent data)
                localStorage.clear();

                // 3. Clear application cookies
                document.cookie.split(";").forEach((cookie) => {
                    const eqPos = cookie.indexOf("=");
                    const name = eqPos > -1 ? cookie.substr(0, eqPos).trim() : cookie.trim();
                    document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/";
                });

                // 4. Force reload via cache-busting URL
                window.location.replace(window.location.origin);
            }

    });







    //---------------------------//
    //Lagra session vid postback
    //---------------------------//

    // 1. Create a single, shared function to store your data
    function saveDataBeforePostback() {



        if (!sessionStorage.getItem("postbackSaved")) {


            //Lägg värden från inputs i value-attributet för varje input så de kan sparas
            articleList.querySelectorAll('.data-row').forEach(row => {

                const article = row.querySelector('input[name^="TbxArticle"]');
                const description = row.querySelector('input[name^="TbxDescription"]');
                const deliveryDate = row.querySelector('input[name^="TbxDeliveryDate"]');
                const quantity = row.querySelector('input[name^="TbxQuantity"]');
                const ppu = row.querySelector('input[name^="TbxPricePerUnit"]');
                const amount = row.querySelector('input[name^="TbxRowAmount"]');
                const chbNotRutPlaceholder = row.querySelector('input[name^="ChbNotRut_Placeholder"]');
                const chbNotRut = row.querySelector('input[name^="ChbNotRut"]');

                if (article) article.setAttribute('value', article.value);
                if (description) description.setAttribute('value', description.value);
                if (deliveryDate) deliveryDate.setAttribute('value', deliveryDate.value);
                if (quantity) quantity.setAttribute('value', quantity.value);
                if (ppu) ppu.setAttribute('value', ppu.value);
                if (amount) amount.setAttribute('value', amount.value);
                chbNotRutPlaceholder?.toggleAttribute('checked', chbNotRutPlaceholder.checked);
                chbNotRut?.toggleAttribute('checked', chbNotRut.checked);


            });


            sessionStorage.setItem("myPostbackData", articleList.innerHTML);
            sessionStorage.setItem("wasPostback", "true");

            // Temporary flag to prevent double-saving if both events fire
            sessionStorage.setItem("postbackSaved", "true");
        }
    }

    // 2. Catch native submit buttons & form.submit() triggers
    var aspNetForm = document.forms[0];
    if (aspNetForm) {
        aspNetForm.addEventListener("submit", saveDataBeforePostback);
    }

    // 3. Catch UseSubmitBehavior="False", LinkButtons, and AutoPostBacks
    var originalDoPostBack = window.__doPostBack;
    if (originalDoPostBack) {
        window.__doPostBack = function (eventTarget, eventArgument) {
            saveDataBeforePostback(); // Run your logic first
            originalDoPostBack(eventTarget, eventArgument); // Let postback continue
        };
    }

    // 4. Read your data back on page load
    if (sessionStorage.getItem("wasPostback") === "true") {

        let qwe = sessionStorage.getItem("myPostbackData");
        articleList.innerHTML = qwe;

        // Clean up everything
        sessionStorage.removeItem("wasPostback");
        sessionStorage.removeItem("myPostbackData");
        sessionStorage.removeItem("postbackSaved");
    }


    /*****************/




















    //När du ändrar i antal eller styckpris så ska ny beräkning göras, därav detta

    let debounceTimer;
    
    // Listen for input events anywhere on the document
    document.addEventListener('input', function (event) {

        const isNumberInput = event.target.type === 'number';

        // Check if the change was triggered by the up/down arrows (inputType is empty for arrow clicks)
        const isArrowClick = isNumberInput && event.inputType === undefined;


        if (isArrowClick) {
            // Fire immediately for arrow clicks
            calculateRow(event.target);
        }

        else {
            // Wait 0.5 seconds (500ms) for typing, resetting the timer on every keystroke
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(calculateRow(event.target), 500);
        }
    });
 



    
    // Select the input element
    const inputField = document.getElementById("TbxGrandInvoiceTotal");

    // Make it read-only
    inputField.readOnly = true;








    filterAndSelect();

    deliveryAddressAutocomplete();

    doArticleFunctions();


    //Fokusera på första skrivbara fältet vid laddning så man kan börja skriva direkt
    //-------------------------------------------------------------------------------
    let focusTextBox;

    //Om välj fakturanummer är ibockat, focus på invoicenumber
    if (chbChangeInvoiceNumber.checked) 

        focusTextBox = tbxInvoiceNumber;
    

    else
    focusTextBox=tbxCustomerNumber;



    // 1. Move focus to the input field
    focusTextBox.focus();


    // 2. Calculate the length of the current text
    const textLength = focusTextBox.value.length;

    // 3. Set both start and end selection to the last character index
    focusTextBox.setSelectionRange(textLength, textLength);

});
//DOMContentLoaded slut

const tbxInvoiceNumber = document.getElementById("TbxInvoiceNumber");

const chbChangeInvoiceNumber = document.getElementById('ChbChangeInvoiceNumber');



//Så fort man kallar articleList hämtar den en uppdaterad version av elementet
Object.defineProperty(window, 'articleList', {
    get() {
        const el = document.querySelector('#TblArticleList tbody');
        if (!el) {
            console.warn("Element 'TblArticleList tbody' not found in the DOM.");
            return null;
        }
        return el;
    },
    configurable: true // Allows you to redefine or delete it later if needed
});







function toNumber(parameter) {

    if (typeof parameter === 'string')
        parameter = parameter.replace(',', '.')

    // Convert values to numbers, defaulting to 0
    return parseFloat(parameter) || 0;

}








function toInteger(parameter) {

    /*
     * Converts a floating-point dollar value safely into integer cents.
     * 
     * @CRITICAL This function must use `Number.EPSILON` to correct underlying
     * IEEE 754 binary floating-point drift before rounding. Do not remove.
     * 
     * @example toCents(1.005) => 101 (Not 100)
     * 
     * @param {number|string} amount - The raw decimal currency amount.
     * @returns {number} The safe, whole-integer representation in cents.
     */

    //Att addera Epsilon är till för att det inte ska bli små, små felberäkningar och därmed avrundas fel
    return Math.round((toNumber(parameter) + Number.EPSILON) * 100);
}

function toDecimals(parameter) {

    /*
     * Converts whole integer cents back into a formatted UI display string.
     * 
     * @NOTE This is an Exit Gate function. It should ONLY be used right before
     * rendering to the screen or UI component. Never run math on the result.
     * 
     * @param {number} cents - The pure whole-integer cent value.
     * @returns {string} Explicitly formatted string with exactly 2 decimal places.
     */


    return (parameter / 100).toFixed(2).replace('.', ',');
}




// The function containing the logic you want to execute
function calculateRow(element) {



    // Check if the user is typing in a quantity or price field
    if (element.classList.contains('tbxquantity') || element.classList.contains('tbxpriceperunit')) {

        

        // Find the specific row container for this input
        const row = element.closest('.data-row');


        // Find the specific fields inside THIS row
        const quantityInput = row.querySelector('.tbxquantity');
        const priceInput = row.querySelector('.tbxpriceperunit');
        const amountInput = row.querySelector('.tbx-article-total');




        const quantity = toNumber(quantityInput.value);
        let price = toInteger(priceInput.value);
        let total = Math.round(quantity * price);

        total = toDecimals(total);


        //Om det är ett heltal ska det inte vara några decimaler
        if (total.endsWith(",00")) {
            total = total.slice(0, -3);
        }

        amountInput.value = total;


        calculateTotal();

    }
}

function calculateTotal() {

    

    

    // Select all individual row amount fields
    const articleTotalTextBoxes = document.querySelectorAll('.tbx-article-total');

    let sumOfArticles = 0;
    let rutDeduction = 0;

    articleTotalTextBoxes.forEach(input => {

        // Converts comma decimals to dots and handles empty/NaN values safely
        const convertedInput = toInteger(input.value);

        sumOfArticles += convertedInput;

        if (!chbIsBusinessCustomer.checked) {

            // Finds the parent row of the current input field
            const row = input.closest('.data-row');

            // Checks if the row checkbox exists and is checked
            const isRowBusiness = row ? row.querySelector("[name*='ChbNotRut']").checked : false;


            // Adds to RUT total only if neither global nor row-specific checkbox is checked
            if (!isRowBusiness) {
                rutDeduction += convertedInput;
            }
        }
    });






    let netAmount = 0;
    let valueAddedTax = 0;
    let grossAmount = 0;

    if (chbIsBusinessCustomer.checked) {

        netAmount = sumOfArticles;

        valueAddedTax = Math.round((netAmount * 25) / 100);

        grossAmount = netAmount + valueAddedTax;

    }

    else {

        grossAmount = sumOfArticles;

        //Räkna ut netto
        netAmount = Math.round((grossAmount / 125) * 100);


        //Separera moms
        valueAddedTax = Math.round((netAmount * 25) / 100);


        //Eftersom RUT-avdrag inte får vara mer än 50 % avrundas 0,005 neråt, därav Math.ceil()
        rutDeduction = (rutDeduction / 200) * 100;
        rutDeduction = Math.ceil(rutDeduction - 0.5);


        netAmount = grossAmount - valueAddedTax;

        //Total Due
        grossAmount = grossAmount - rutDeduction;


    }




    netAmount = toDecimals(netAmount);

    document.getElementById('TbxNetAmount').value = netAmount;

    valueAddedTax = toDecimals(valueAddedTax)

    document.getElementById('TbxValueAddedTax').value = valueAddedTax;



    rutDeduction = toDecimals(rutDeduction);

    document.getElementById('TbxRutDeduction').value = rutDeduction;


    grossAmount = toDecimals(grossAmount);

    document.getElementById('TbxGrandInvoiceTotal').value = grossAmount;


}



const tbxRutDeduction = document.getElementById('TbxRutDeduction');

const tbxPersonalIdentityNumber = document.getElementById('TbxPersonalIdentityNumber');
let rutCheckboxes = document.querySelectorAll("#TblArticleList input[type='checkbox']");

const lblPersonalIdentityNumber = document.getElementById('LblPersonalIdentityNumber');

function toggleBusinessCustomer() {


    lblPersonalIdentityNumber.classList.toggle('disabled-label-font-color');


    //The static keyword defines utilities belonging to the class itself, 
    //rather than to instances of the class. They are called directly on the class name.
     

    const pricePerUnitTableHeader = document.querySelector('#TblArticleList > thead > tr > th:nth-child(5)');



    let rutCheckboxElements = document.querySelectorAll("#TblArticleList input[type='checkbox']");

    //Värden för RUT-kryssrutor
    let rutCheckboxValues = [];
    


    //Om ändrat till företagskund
    if (chbIsBusinessCustomer.checked) {



        //Spara personnumret som kan användas för om man ändrar tillbaka till privatkund senare
        sessionStorage.setItem("personalIdentityNumber", tbxPersonalIdentityNumber.value);

        //Töm personnumer
        tbxPersonalIdentityNumber.value = "";

        //Disabla personnummer-fältet
        tbxPersonalIdentityNumber.disabled = true;


        

        //Ändra rubrik
        pricePerUnitTableHeader.innerHTML = "À-pris exkl. moms";


        









        //Nollställ array för kryssruts-värden
        rutCheckboxValues = [];

        //Gå igenom kryssruts-elementen
        rutCheckboxElements.forEach(function (checkboxElement) {

            //Lagra värdena i array som kommer lagras i session
            rutCheckboxValues.push(checkboxElement.checked);

            //Kryssa för ruta eftersom det inte är RUT på företag samt inaktivera elementen
            checkboxElement.checked = true;
            checkboxElement.disabled = true;
            checkboxElement.classList.add('is-not-enabled-font-color');

        });

        //Lagra kryssruts-värdena i session ifall man ändrar tillbaka till privatkund senare
        sessionStorage.setItem("checkboxesChecked", rutCheckboxValues);





        tbxRutDeduction.readOnly = true;


    }

    //Om inte företagskund
    else {

        //Sätt tillbaka personnumret som fanns innan man bytte till företagkund
        tbxPersonalIdentityNumber.value = sessionStorage.getItem("personalIdentityNumber");

        //Enabla personnummer-fältet
        tbxPersonalIdentityNumber.disabled = false;



        pricePerUnitTableHeader.innerHTML = "À-pris inkl. moms";






        //Samma för kryssrutor


        if (sessionStorage.getItem("checkboxesChecked") != null) {

            rutCheckboxValues = sessionStorage.getItem("checkboxesChecked").split(',');

            for (var i = 0; i < rutCheckboxElements.length; i++) {

                rutCheckboxElements[i].classList.remove('is-not-enabled-font-color');
                rutCheckboxElements[i].disabled = false;




                if (rutCheckboxValues[i] == 'false')
                    rutCheckboxElements[i].checked = false;

                else
                    rutCheckboxElements[i].checked = true;


            }
        }



        tbxRutDeduction.readOnly = false;

    }


    //"*=" betyder "Välj allt som innehåller följande..."
    pricePerUnitTextBoxes = document.querySelectorAll("[name*='TbxPricePerUnit']");

    //Räkna om enhets-pris
    for (var i = 0; i < pricePerUnitTextBoxes.length; i++) {


        let priceToConvert = pricePerUnitTextBoxes[i].value.replace(",", ".");


        if (chbIsBusinessCustomer.checked) {

            priceToConvert /= 1.25;
        }

        else priceToConvert *= 1.25;



        //Om det finns decimaler, avgränsa till 2
        if (Math.round(priceToConvert).toFixed(2) != priceToConvert.toFixed(2))
            priceToConvert = priceToConvert.toFixed(2);


        //Sätt komma-tecken som decimalavskiljare
        priceToConvert = priceToConvert.toString().replace(".", ",");

        //Placera värden
        pricePerUnitTextBoxes[i].value = priceToConvert;


        calculateRow(pricePerUnitTextBoxes[i]);
    }







    let selectedCustomerBeforeSwitch = lbxCustomers.selectedIndex;
    let selectedAddressBeforeSwitch = lbxAddresses.selectedIndex;

    //Ändra listboxarna
    filterAndSelect();

    //Om ingen kund var markerad när man tryckte på "Företagskund" ska ingen vara markerad efteråt heller,
    //dvs markering i listboxarna tas bort
    if (selectedCustomerBeforeSwitch < 0) {

        lbxCustomers.selectedIndex = -1;

    }

    if (selectedAddressBeforeSwitch < 0) {
        lbxAddresses.selectedIndex = -1;
    }


    deliveryAddressAutocomplete();

}




//Ändra autocompletelistor för leveransadress
function deliveryAddressAutocomplete() {

    $(function () {

        $("#TbxDeliveryStreet").autocomplete({
            source: deliveryStreets,
            minLength: 0
        }).focus(function () {
            if (this.value == "") {
                $(this).autocomplete("search");
            }
        });

        $("#TbxDeliveryPostalCode").autocomplete({
            source: deliveryPostalCodes,
            minLength: 0
        }).focus(function () {
            if (this.value == "") {
                $(this).autocomplete("search");
            }
        });

        $("#TbxDeliveryCity").autocomplete({
            source: deliveryCities,
            minLength: 0

        }).focus(function () {
            if (this.value == "") {
                $(this).autocomplete("search");
            }
        });
    });

}






//Läs data som kommer från back-enden i form av arrayer
function validateDataFromDatabase(array) {

    //Om array inte är null och är större än 0, returnera arrayen, annars returnera tom array
    return (array && array.length > 0) ? array : [];

}




//Filter
/****************/




let customers;
let customerNumbers;
let customerNames;
let addresses;
let streets;
let postalCodes;
let cities;


//Kryssruta för företagskund
const chbIsBusinessCustomer = document.getElementById('ChbIsBusinessCustomer');


let deliveryStreets;
let deliveryPostalCodes;
let deliveryCities;


const tbxCustomerNumber = document.getElementById("TbxCustomerNumber");
const tbxCustomerName = document.getElementById('TbxCustomerName');
const tbxStreet = document.getElementById('TbxStreet');
const tbxPostalCode = document.getElementById('TbxPostalCode');
const tbxCity = document.getElementById('TbxCity');





let selectedCustomer = sessionStorage.getItem("selectedCustomerKey");
let selectedAddress = sessionStorage.getItem("selectedAddressKey");



function filterAndSelect() {

    //????
    customerNumbers = validateDataFromDatabase(personCustomerNumbers);
    customerNames = validateDataFromDatabase(personCustomerNames);
    customers = validateDataFromDatabase(personCustomers);
    streets = validateDataFromDatabase(personStreets);
    postalCodes = validateDataFromDatabase(personPostalCodes);
    cities = validateDataFromDatabase(personCities);
    addresses = validateDataFromDatabase(personAddresses);



    if (chbIsBusinessCustomer.checked) {
        customerNumbers = validateDataFromDatabase(businessCustomerNumbers);
        customerNames = validateDataFromDatabase(businessCustomerNames);
        customers = validateDataFromDatabase(businessCustomers);
        streets = validateDataFromDatabase(businessStreets);
        postalCodes = validateDataFromDatabase(businessPostalCodes);
        cities = validateDataFromDatabase(businessCities);
        addresses = validateDataFromDatabase(businessAddresses);
    }




    
    //Ta bort dubletter och tomma element
    /* **************************** */

    //Array.from gör det till en ny array, och Set tar bort dubletter
    deliveryStreets = Array.from(new Set(streets));
    deliveryPostalCodes = Array.from(new Set(postalCodes));
    deliveryCities = Array.from(new Set(cities));




    //JavaScripts inbyggda filter-metod tar bort tomma element
    
    deliveryStreets = deliveryStreets.filter(function (parameter) {
        return parameter;
    });

    deliveryPostalCodes = deliveryPostalCodes.filter(function (parameter) {
        return parameter;
    });

    deliveryCities = deliveryCities.filter(function (parameter) {
        return parameter;
    });
    /* **************************** */



    let filteredCustomers = customers.slice();
    let filteredAddresses = addresses.slice();




    //Returnera booleskt värde huruvida array inkluderar ett visst värde
    function filterItems(arr, query) {
        return !arr.toLowerCase().includes(query.value.toLowerCase());
    }

    //Gå igenom data och använd funktion, om angiven parameter inte hittas, returnera tom sträng
    for (let i = 0; i < filteredCustomers.length; i++) {

        if (filterItems(customerNumbers[i], tbxCustomerNumber))
            filteredCustomers[i] = "";

        if (filterItems(customerNames[i], tbxCustomerName))
            filteredCustomers[i] = "";

        if (filterItems(streets[i], tbxStreet))
            filteredAddresses[i] = "";

        if (filterItems(postalCodes[i], tbxPostalCode))
            filteredAddresses[i] = "";

        if (filterItems(cities[i], tbxCity))
            filteredAddresses[i] = "";


        //Gör samma för kundnummer+namn och hela adresser
        if (customers[i] != filteredCustomers[i] || addresses[i] != filteredAddresses[i]) {
            filteredCustomers[i] = "";
            filteredAddresses[i] = "";
        }


    }


    //Ta bort dubletter
    filteredCustomers = Array.from(new Set(filteredCustomers));
    filteredAddresses = Array.from(new Set(filteredAddresses));


    //Töm kundlista
    for (let i = lbxCustomers.length - 1; i >= 0; i--)
        lbxCustomers.remove(i)


    //Lägg till i kundlista från filtrerade kunder istället
    for (let i = 0; i < filteredCustomers.length; i++)
        if (filteredCustomers[i] != "") {
            var option = document.createElement("option");
            option.text = filteredCustomers[i];
            lbxCustomers.add(option);
        }

    //Töm adresslista
    for (let i = lbxAddresses.length - 1; i >= 0; i--)
        lbxAddresses.remove(i)

    //Lägg till i adresslista från filtrerade adresser istället
    for (let i = 0; i < filteredAddresses.length; i++) {
        if (filteredAddresses[i] != "") {
            var option = document.createElement("option");
            option.text = filteredAddresses[i];
            lbxAddresses.add(option);
        }
    }





    //Om ingen kund är vald sen innan
    if (selectedCustomer != null) {

        
        if (selectedCustomer == -1 || selectedCustomer >= lbxCustomers.selectedIndex)
            selectedCustomer = 0;



        lbxCustomers.selectedIndex = selectedCustomer;

    }

    selectedCustomer = 0;





    if (selectedAddress == null)
        selectedAddress = 0;

    else {
        if (selectedAddress == -1 || selectedAddress >= lbxAddresses.selectedIndex)
            selectedAddress = 0;


        lbxAddresses.selectedIndex = selectedAddress;

        selectedAddress = 0;
    }

    

    toggleClearInvoiceRecipientButton();
    toggleAutofillButtons();
    toggleAddArticleButtonVisibility();

}

/*Min filter-funktion slut*/










let pricePerUnitTextBoxes;




function doArticleFunctions() {




    // jQuery
    // Inaktivera webbläsarens autocomplete och autofill
    $(document).ready(function () {

        document.querySelectorAll("input[type='text']").forEach(myFunction);

        function myFunction(item) {
            item.setAttribute("autocomplete", "one-time-code");

        }
    });









    //Skriver man "Resa" eller välj det i listan så ska Ej rut-rutan bockas för automatiskt
    document.querySelectorAll("#TblArticleList tbody tr").forEach(function (articleRow) {

        const textBox = articleRow.querySelector("[name*='TbxArticle']");
        const checkBox = articleRow.querySelector("[name*='ChbNotRut']");

        if (!textBox || !checkBox) return;


        textBox.addEventListener("input", () => {

            travelItem(textBox);


        });

        $(textBox).on("autocompleteselect", function (event, ui) {

            travelItem(ui.item);

            
        });


        function travelItem(textBox) {



            //event.target är samma som textBox
            if (textBox.value.toLowerCase() === "resa") {

                // Only flag it as auto-checked if it wasn't already checked by the user
                // Annars blir flaggat som auto-checked även när användaren har bockat för själv
                if (!checkBox.checked) {
                    checkBox.checked = true;
                    checkBox.dataset.autoChecked = "true";
                }
            }

            else {
                // Only uncheck if it was previously checked automatically by the script
                if (checkBox.dataset.autoChecked === "true") {
                    checkBox.checked = false;
                    delete checkBox.dataset.autoChecked; // Clear the flag
                }
            }







        }



    });












    //jQuery
    //Autocomplete för artikelnamn med artiklar från databas
    $('[name*="TbxArticle"]').autocomplete({
        source: services,
        minLength: 0,



        /*Detta block är till för att lägga en scrollbar på enbart menyn istället
        för att webbläsaren ska ta fram en för hela skärmen när autocomplete-listan blir för lång*/
        //---------------------

        open: function (event, ui) {
            var $input = $(this);
            var $menu = $input.autocomplete("widget");
            var resizeTimeout;

            function adjustDropdown() {
                var inputBottom = $input.offset().top + $input.outerHeight() - $(window).scrollTop();
                var availableHeight = $(window).height() - inputBottom - 15;

                $menu.css({
                    "max-height": availableHeight + "px",
                    "overflow-y": "auto",
                    "overflow-x": "hidden"
                });

                $menu.position({
                    my: "left top",
                    at: "left bottom",
                    of: $input,
                    collision: "none"
                });
            }

            // 1. Initial size calculation when opened
            adjustDropdown();

            // 2. Optimized handler for resize and scroll events
            $(window).on("resize.autocompleteScroll scroll.autocompleteScroll", function () {
                // Force an immediate realignment right when the maximize action starts
                adjustDropdown();

                // Clear and reset the timeout to handle the final alignment after animation ends
                clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(function () {
                    adjustDropdown();
                }, 100); // To ensure maximize animation finishes
            });



        },
        close: function (event, ui) {
            // 3. Clean up the event listeners entirely
            $(window).off(".autocompleteScroll");
        }

        // , select: function (event, ui) {
        //     // ui.item contains the label and value of the chosen option
        //     console.log("Selected text: " + ui.item.label);
        //     console.log("Selected value: " + ui.item.value);
        // }

    }).focus(function () {
        if (this.value === "") {
            $(this).autocomplete("search");
        }
    });


















    //Markera all text i "Antal" när den är i fokus
    document.querySelectorAll("[name*='TbxQuantity'").forEach(function (quantityTextBox) {

        

        //Om du tabbar fram till textrutan
        quantityTextBox.onfocus = function () {
            quantityTextBox.select();
        }


        let valueSaved = 0;

        //När du skriver i rutan så sparas det att du har skrivit
        quantityTextBox.oninput = function () {


            valueSaved = 1;

        }



        //Om du klickar i textrutan så markeras texten
        quantityTextBox.onclick = function () {

            quantityTextBox.select();

            //Om du precis har skrivit manuellt blir det avmarkerat istället
            if (valueSaved == 1) {

                window.getSelection().removeAllRanges();
                valueSaved = 0;

            }
        }

    });

    





    pricePerUnitTextBoxes = document.querySelectorAll("[name*='TbxPricePerUnit']");


    //Responsiv
    /**************/
    pricePerUnitTextBoxes.forEach(function (item) {
        item.style.display = "none";
    })

    let pricePerUnitCells = document.querySelectorAll("#TblArticleList td:nth-child(5)");

    pricePerUnitCells.forEach(function (cell) {
        cell.style.maxWidth = pricePerUnitCells[0].clientWidth + 1 + "px";
    })

    pricePerUnitTextBoxes.forEach(function (textBox) {
        textBox.style.display = "inline-block";
        textBox.style.width = "-webkit-fill-available";
    })
    /**************/









}








function toggleAddArticleButtonVisibility() {

    let numberOfArticles = articleList.querySelectorAll('.data-row').length;



    if (numberOfArticles > 0 && numberOfArticles < 5)
        document.getElementById('BtnAddArticle').style.display = 'inline-block';

    else document.getElementById('BtnAddArticle').style.display = 'none';
}





//Denna funktion sätter stor begynnelsebokstav på ett ord
/***************************************************/

function capitalizeFirstLetter(textInput) {


    if (textInput.value != "")
        textInput.value = textInput.value[0].toUpperCase() + textInput.value.substring(1);

}






//Slå på och av separat leverans-adress
/**********************************************************************/
function toggleDeliveryAddress() {

    const divDeliveryAddress = document.getElementById("DivDeliveryAddress");

    if (document.getElementById("ChbDeliveryAddress").checked) {

        divDeliveryAddress.classList.remove("display-none");

        deliveryAddressAutocomplete();
    }

    else
        divDeliveryAddress.classList.add("display-none");
     
}






//Rensa allt-knapp för mottagarfält
//START
/*****************************/



const invoiceRecipientTextBoxes = document.querySelectorAll("#DivInvoiceRecipient input[type='text']:not(#TbxInvoiceNumber)");


//Tillgängliggör rensa-knappen när det finns något att rensa
for (let i = 0; i < invoiceRecipientTextBoxes.length; i++) {
    invoiceRecipientTextBoxes[i].addEventListener("input", toggleClearInvoiceRecipientButton);
}

document.querySelectorAll("#DivDeliveryAddress input[type='text']").forEach(function (element) {
    element.addEventListener("blur", toggleClearInvoiceRecipientButton);
});






const btnClearInvoiceRecipient = document.getElementById('BtnClearInvoiceRecipient');

function toggleClearInvoiceRecipientButton() {

    btnClearInvoiceRecipient.disabled = true;

    for (var i = 0; i < invoiceRecipientTextBoxes.length; i++) {

        if (invoiceRecipientTextBoxes[i].value != "")
            btnClearInvoiceRecipient.disabled = false;
    }
}



function clearInvoiceRecipient() {


    for (var i = 0; i < invoiceRecipientTextBoxes.length; i++) {
        invoiceRecipientTextBoxes[i].value = "";
        invoiceRecipientTextBoxes[i].classList.remove('auto-filled-background-color');

    }

    filterAndSelect();

    lbxCustomers.selectedIndex = -1;
    lbxAddresses.selectedIndex = -1;
    
    
}
//Rensa allt-knapp för mottagarfält
//SLUT
/*****************************/







//Listrutor



//Lagra valda listrute-alternativ i variabler och sessioner
function storeListboxSelections() {



    selectedCustomer = lbxCustomers.selectedIndex;
    selectedAddress = lbxAddresses.selectedIndex;

    sessionStorage.setItem("selectedCustomerKey", selectedCustomer);
    sessionStorage.setItem("selectedAddressKey", selectedAddress);




}



//Om inget är markerat i respektive lista för kund och adress
//så ska man inte heller kunna trycka på autofyll

const lbxCustomers = document.getElementById("LbxCustomers");
const lbxAddresses = document.getElementById("LbxPostalAddresses");

function toggleAutofillButtons() {



    const btnAutofillCustomer = document.getElementById("BtnAutofillCustomer");
    const btnAutofillAddress = document.getElementById("BtnAutofillAddress");
    const btnAutofillAll = document.getElementById("BtnAutofillAll");

    if (lbxCustomers.selectedIndex == -1)
        btnAutofillCustomer.disabled = true;

    else
        btnAutofillCustomer.disabled = false;



    if (lbxAddresses.selectedIndex == -1)
        btnAutofillAddress.disabled = true;

    else
        btnAutofillAddress.disabled = false;


    //Om båda fyll i-knapparna är tillgängliga, tillgängliggör även snabbknapp
    if (btnAutofillCustomer.disabled || btnAutofillAddress.disabled)
        btnAutofillAll.disabled = true;

    else
        btnAutofillAll.disabled = false;
}








function autofillCustomer() {


    for (var i = 0; i < customers.length; i++) {

        if (customers[i] == lbxCustomers.value) {
            tbxCustomerNumber.value = customerNumbers[i];
            tbxCustomerNumber.classList.add('auto-filled-background-color');
            tbxCustomerName.value = customerNames[i];
            tbxCustomerName.classList.add('auto-filled-background-color');
            lbxCustomers.focus();
            lbxCustomers.options[0].focus();
            break; // Behöver inte iterera mer
        }

    }
}



function autofillAddress() {

    for (var i = 0; i < addresses.length; i++) {

        if (addresses[i] == lbxAddresses.value) {

            tbxStreet.value = streets[i];
            if (tbxStreet.value != "")
                tbxStreet.classList.add('auto-filled-background-color');

            tbxPostalCode.value = postalCodes[i];
            if (tbxPostalCode.value != "")
                tbxPostalCode.classList.add('auto-filled-background-color');

            tbxCity.value = cities[i];
            if (tbxCity.value != "")
                tbxCity.classList.add('auto-filled-background-color');


            lbxAddresses.focus(); //Den här raden måste finnas för att nästa rad ska fungera
            lbxAddresses.options[0].focus();

            break;
        }

    }
}






//Se gamla fakturor
//START
/*******************************************************************************************/

const divOldInvoices = document.getElementById('DivOldInvoices');
const HdnOpenDatabaseButtonPressed = document.getElementById('HdnOpenDatabaseButtonPressed');

//Måste kollas varje postback, därför ligger den inte i en metod
if (divOldInvoices != null) {

    if (HdnOpenDatabaseButtonPressed.value == 0) {

        divOldInvoices.classList.add("display-none");

    }

    else if (HdnOpenDatabaseButtonPressed.value == 1) {

        document.getElementById('BtnShowOldInvoices').value = "Gå tillbaka";

        divOldInvoices.classList.remove("display-none");

    }
}


const hdnInvoiceNumber = document.getElementById('HdnInvoiceNumber');

function getInvoices() {

    if (HdnOpenDatabaseButtonPressed.value == 1)
        HdnOpenDatabaseButtonPressed.value = 0;


    else
        HdnOpenDatabaseButtonPressed.value = 1;


    


    hdnInvoiceNumber.value = '';

}



function openInvoice(param) {

    hdnInvoiceNumber.value = param.parentElement.parentElement.querySelector("td:nth-child(2)").textContent;
}




function deleteInvoice() {


    if (confirm('Är du säker på att du vill ta bort faktura med nummer ' + hdnInvoiceNumber.value + '?\nDetta kan inte ångras.')) {
        document.getElementById('HdnConfirmDeletion').value = 1;
    }
}

/*******************************************************************************************/
//Se gamla fakturor
//SLUT






//Artiklar START
/* ********** */


// Synchronizes the checkbox state with our hidden array input
function updateCheckboxValue(checkbox) {
    const hiddenInput = checkbox.nextElementSibling;
    hiddenInput.value = checkbox.checked ? "true" : "false";

    calculateTotal();
}

  

function removeRow(button) {


    // Traverse up to find the <tr> element and remove it
    const row = button.closest('tr');

    


    rutCheckboxValues = sessionStorage.getItem("checkboxesChecked");


    if (rutCheckboxValues !== null) {

        rutCheckboxValues = rutCheckboxValues.split(', ');

        //Tar bort 1 checkrutsvärde som tillhör det element på raden där man tryckte
        rutCheckboxValues.splice(row.sectionRowIndex, 1);


        sessionStorage.setItem("checkboxesChecked", rutCheckboxValues)

    }


    




    row.remove();

    toggleAddArticleButtonVisibility();
    
}


function addArticle() {

    //Om någon mot förmodan skulle lyckas gå förbi knappen som aktiverar
    //denna funktion och det är för mnga rader så returneras användaren
    if (document.querySelectorAll('.data-row').length === 5) {
        alert('Du kan maximalt ha 5 rader');
        return;
    }

    // Find the first row to use as a blueprint
    const firstRow = document.querySelector('.data-row');
    const newRow = firstRow.cloneNode(true);

    // Clear out values in the new row
    const inputs = newRow.querySelectorAll('input:not([type="hidden"])');

    inputs.forEach(input => {

        if (input.type === 'checkbox') {

            input.checked = false;

            if (chbIsBusinessCustomer.checked)
                input.checked = true;


        } else if (input.type === 'number') {
            input.value = "1";
        }


        else if (input.name === 'TbxPricePerUnit[]') {

            if (chbIsBusinessCustomer.checked)
                input.value = "400";
            else {

                input.value = "500";
            }


        }

        else if (input.name === 'TbxRowAmount[]') {

            if (chbIsBusinessCustomer.checked)
                input.value = "400";
            else {

                input.value = "500";
            }


        }


        else {
            input.value = "";
        }

        
    });

    // Reset the hidden checkbox tracker for this new row
    newRow.querySelector('input[type="hidden"]').value = "false";

    //vaf har jag denna checkboxen till?
    if (chbIsBusinessCustomer)
        newRow.querySelector('input[type="hidden"]').value = "true";


    // Show the delete button on the new row
    const deleteBtn = newRow.querySelector('.btn-delete');
    deleteBtn.style.display = 'inline-block';

    // Append to table body
    document.querySelector('#TblArticleList tbody')?.appendChild(newRow);



    doArticleFunctions();
    calculateTotal();

    toggleAddArticleButtonVisibility();

}

/* ********* */
//Artiklar SLUT











//Skapa faktura
/*********************************/


const hdnConfirmCreation = document.getElementById('HdnConfirmCreation');
    const lblError = document.getElementById('LblError');


//Körs bara från ett ställe, och det är när man klickar på "Skapa faktura"
function validate() {


    lblError.textContent = "";
    document.getElementById('HdnConfirmCreation').value = 0;


    // 1. Initialize an empty array to hold errors
    let errors = [];

    // 2. Perform your validations and push strings to the array
    if (tbxInvoiceNumber.value.trim() == "") {
        errors.push("Fakturanummer saknas!");
    }
    if (tbxCustomerNumber.value.trim() == "") {
        errors.push("Kundnummer saknas!");
    }


    if (tbxCustomerName.value.trim() == "") {
        errors.push("Namn saknas!");

    }





    let i = 0;

    document.querySelectorAll("#TblArticleList [name*='TbxArticle']").forEach(function (param) {

        i++;

        if (param.value.trim() == "") {
            errors.push("Artikel " + i + " saknar namn!");
        }
    });






    // 3. Render all errors at once if any exist
    if (errors.length > 0) {

        // .join("\n") merges the array into one string separated by newlines
        lblError.textContent = errors.join("\n");
        return false;
    } else {
        lblError.textContent = ""; // Clear errors if validation passes
    }





    return checkCustomerSync();

    function checkCustomerSync() {
        let result = false; // Default fallback variable

        $.ajax({
            type: 'POST',
            url: 'Default.aspx/ValidateCustomer',
            data: JSON.stringify({
                CustomerNumber: tbxCustomerNumber.value,
                CustomerName: tbxCustomerName.value
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            async: false, // Forces code to wait
            success: function (data) {
                // Assign value to the outer variable
                result = confirmCreation(data.d);
            },
            error: function (xhr, status, error) {
                alert("Något har gått fel!");
                result = false;
            }
        });

        return result; // Final return statement at the bottom
    }

















    //Kolla om kund finns i databas eller ej
    // $.ajax({


    //     type: 'POST',
    //     url: 'Default.aspx/ValidateCustomer',
    //     data: "{ 'CustomerNumber': '" + tbxCustomerNumber.value + "', 'CustomerName': '" + tbxCustomerName.value + "' }",
    //     contentType: "application/json; charset=utf-8",
    //     dataType: 'json',

    //     success: function (data) {

    //         //Skicka parameter till nästa JavaScript-funktion
    //         return confirmCreation(data.d);
    //     },

    //     error: function (xhr, status, error) {
    //         alert("Något har gått fel!")
    //         return false;
    //     },

    //     async: false
    // });
    

    


    //enum, strängarna är för att det ska bli enklare att debugga, annars har man siffror
    /*
    const ErrorTypes = Object.freeze({
    RetrieveFailed: 'RetrieveFailed',
    DeleteFailed: 'DeleteFailed',
    CreateFailed: 'CreateFailed'
    });
    
     
    */

   //Körs från AJAX-metoden, enda stället det körs från
    function confirmCreation(parameter) {

        /* *********************** */
        //Möjliga parametervärden:
        //"": Lyckades
        //"1": Användare behöver svara på en prompt
        //"2": Användare behöver svara på en prompt
        //"Annan sträng": Felmeddelande
        /* *********************** */




        //Om parameter är tomt har allt gått som det ska
        if (parameter == "") {

            document.getElementById('HdnConfirmCreation').value = 1;

            // alert('confirm');
            // 1. Temporarily change the form target to open a new tab
            document.getElementById('form1').target = '_blank';

            // 2. Wait 100 milliseconds, then flip it back to normal
            setTimeout(function () {
                document.getElementById('form1').target = '_self';
            }, 100);

            return true;
        }

        else {



            //Om något har gått fel i back-enden kommer felmeddelandet här
            if (parameter != "2" && parameter != "1") {

                alert(parameter);
            }



            else {
                let promptMessage = "";

                // Om kundnummer stämmer men inte namn
                if (parameter == "2")
                    promptMessage = 'OBS! Kundnumret finns i kundregistret men inte namnet. Om du fortsätter kommer namnet att ersättas.'


                // Om kundnummer inte stämmer
                else if (parameter == "1")
                    promptMessage = 'OBS! Kundnumret finns ej i registret. Om du fortsätter kommer ny kund att skapas.';




                //Promptruta, trycker användare nej ändras värde till 0
                if (promptMessage != "" && confirm(promptMessage)) {

                    document.getElementById('HdnConfirmCreation').value = 1;


                    
                    

                    // 1. Temporarily change the form target to open a new tab
                    document.getElementById('form1').target = '_blank';

                    // 2. Wait 100 milliseconds, then flip it back to normal
                    setTimeout(function () {
                        document.getElementById('form1').target = '_self';
                    }, 100);


                    return true;

                }

                return false;
            }
        } 
    }
}