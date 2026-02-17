let rowIndex = 0;
//var nextPONumber = 0;
//var nextInvoiceNumber = 0;

$(document).ready(function () {
    loadStateDropdown();
    loadSupplierDropdown();
    loadRawMaterialsDropdown();
    loadUnitDropdown();
    loadPaymentModeDropdown();
    loadAccountDropdown();
    fetchData();

    // Add button click
    $(document).on('click', '#btn-add', function (event) {
        // Check Add Permission from Session or Local Storage
        var canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) {
            // Display Access Denied Message
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Add. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault(); // Stop the modal from opening
            $('#addEventModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#addEventModalTitle").text("Add Purchase Order");
            $("#addEventModal").modal("show");
        }
    });

    $("#btnResetForm").on("click", function () {
        resetPurchaseForm();
    });

    // Edit button click
    $(document).on("click", ".btn-edit", function (event) {
        event.preventDefault(); // Stop the modal from opening
        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';

        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin."
            });
            return;
        }

        resetFormFields();

        let table = $('#tableMain table').DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        if (!rowData) return;

        //console.log("EDIT DATA:", rowData);

        // =========================================
        // BASIC HEADER
        // =========================================
        $("#purchaseId").val(rowData.Id);

        $("#purchaseFromSupplier").val(rowData.PurchaseFrom);
        $("#purchaseSupplier").val(rowData.SupplierId);

        $("#purchaseInvoiceDate").val(rowData.InvoiceDate ?? "");
        $("#purchaseInvoiceNo").val(rowData.InvoiceNo);
        $("#purchasePONo").val(rowData.PONo);

        // SET INVOICE URL
        $("#invoiceFileLink").val(rowData.UploadInvoiceFile || "");
        // If URL present → show download button
        if (rowData.UploadInvoiceFile && rowData.UploadInvoiceFile !== "") {
            $("#btnDownloadInvoice").removeClass("d-none");
        }
        else {
            $("#btnDownloadInvoice").addClass("d-none");
        }

        // =========================================
        // DISCOUNT SECTION
        // =========================================
        $("input[name='discountType'][value='" + rowData.DiscountType + "']").prop("checked", true);
        $("#discountValue").val(rowData.TotalDiscountInInvoice);
        $("#discountCalc").text(rowData.DiscountPercentage);
        $("input[name='discountTaxType'][value='" + rowData.DetermineWhenTaxApplicable + "']").prop("checked", true);

        // =========================================
        // OTHER CHARGES
        // =========================================
        $("#deliveryCharge").val(rowData.DeliveryChargeIncludedInInvoice);
        $("#deliveryCGST").val(rowData.DeliveryCharge_CGST);
        $("#deliverySGST").val(rowData.DeliveryCharge_SGST);

        // =========================================
        // TAX SECTION
        // =========================================
        $("#tcsPercent").val(rowData.TCS_Percent);
        $("#tcsCalc").text(rowData.TCS_Percent);
        $("#tdsPercent").val(rowData.TDS_Percent);
        $("#tdsCalc").text(rowData.TDS_Percent);

        // =========================================
        // PAYMENT TYPE
        // =========================================
        if (rowData.PaymentType == 2) {
            $("#payPaid").prop("checked", true);
            $("#payUnpaid").prop("checked", false);
        }
        else {
            $("#payUnpaid").prop("checked", true);
            $("#payPaid").prop("checked", false);
        }

        // Set values BEFORE triggering change
        $("#paymentDate").val(rowData.PaymentDate || "");
        $("#paidAmount").val(rowData.PaidAmount || 0);
        $("#paymentModeId").val(rowData.PaymentModeId || 0);
        $("#paymentRefNo").val(rowData.PaymentRefNo || "");

        // NOW trigger change once everything is set
        $("input[name='paymentType']:checked").trigger("change");


        // =========================================
        // RELOAD MATERIAL ROWS
        // =========================================
        $("#purchaseItemsBody").empty();
        rowIndex = 0;

        if (rowData.JSON_PurchaseItems && rowData.JSON_PurchaseItems.length > 0) {

            rowData.JSON_PurchaseItems.forEach(item => {
                rowIndex++;

                let newRow = buildRowForEdit(item);
                $("#purchaseItemsBody").append(newRow);
            });

        } else {
            addInitialRow();
        }

        updateSrNo();
        calculateSubTotals();
        calculateDiscount();
        calculateOtherCharges();
        calculateTaxes();
        calculateGrandTotal();

        // =========================================
        // OPEN MODAL
        // =========================================
        $("#addEventModalTitle").text("Edit Purchase Order");
        $("#addEventModal").modal("show");
    });

    // SAVE - Handle form submission
    $("#btnSavePurchase").click(function (event) {
        event.preventDefault(); // Stop navigation
        if (!validatePurchaseForm()) return;

        let purchaseId = $("#purchaseId").val();
        let supplierId = $("#purchaseSupplier").val();
        let invoiceDate = $("#purchaseInvoiceDate").val();
        let poNo = $("#purchasePONo").val();
        let invoiceNo = $("#purchaseInvoiceNo").val();

        let discountType = $("input[name='discountType']:checked").val();
        let totalDiscount = parseFloat($("#discountValue").val()) || 0;
        let discountPercent = discountType == "2" ? totalDiscount : 0;
        let determineTaxApply = $("input[name='discountTaxType']:checked").val();

        let subTotalDiscount = parseFloat($("#txtTotalDiscount").text()) || 0;
        let subTotalOtherCharges = parseFloat($("#txtOtherCharges").text()) || 0;

        let deliveryCharge = parseFloat($("#deliveryCharge").val()) || 0;
        let deliveryCGST = parseFloat($("#deliveryCGST").val()) || 0;
        let deliverySGST = parseFloat($("#deliverySGST").val()) || 0;

        let otherTaxAmount = parseFloat($("#txtOtherTaxes").text()) || 0;
        let tcsPercent = parseFloat($("#tcsPercent").val()) || 0;
        let tdsPercent = parseFloat($("#tdsPercent").val()) || 0;

        let grandTotal = parseFloat($("#txtGrandTotal").text()) || 0;

        //let paymentType = $("input[name='paymentType']:checked").attr("id") === "payPaid" ? 2 : 1;
        let paymentType = parseInt($("input[name='paymentType']:checked").val());

        let paymentDate = $("#paymentDate").val() || null;
        let paymentModeId = $("#paymentModeId").val() || null;
        let paidAmount = $("#paidAmount").val() || 0;
        let paymentRefNo = $("#paymentRefNo").val() || null;

        let isUpdateInventory = $("#chkUpdateInventory").is(":checked");

        let itemsList = collectItems();

        // SELECT FILE
        let fileInput = $("#uploadInvoiceFile").get(0);
        let file = fileInput.files.length > 0 ? fileInput.files[0] : null;

        function getBase64WithoutMeta(file, callback) {
            let reader = new FileReader();
            reader.onload = function (e) {
                let base64 = e.target.result.split(',')[1]; // REMOVE metadata
                callback(base64);
            };
            reader.readAsDataURL(file);
        }

        function collectItems() {
            let items = [];

            $("#purchaseItemsBody tr").each(function () {
                let row = $(this);

                items.push({
                    PurchaseId: $("#purchaseId").val(),
                    RawMaterialId: row.find(".rawMaterialSelect").val(),
                    Qty: parseFloat(row.find(".qtyInput").val()) || 0,
                    UnitId: row.find(".unitSelect").val(),
                    Price: parseFloat(row.find(".priceInput").val()) || 0,
                    Amount: parseFloat(row.find(".amountInput").val()) || 0,
                    CGST: parseFloat(row.find(".cgstInput").val()) || 0,
                    SGST: parseFloat(row.find(".sgstInput").val()) || 0,
                    IGST: parseFloat(row.find(".igstInput").val()) || 0,
                    IsSelected: row.find(".row-check").is(":checked")
                });
            });

            return items;
        }

        // FINAL SAVE FUNCTION
        function submitSave(base64File) {

            let payload = {
                Id: purchaseId,
                PurchaseFrom: 1,
                SupplierId: supplierId,
                InvoiceDate: invoiceDate,
                InvoiceNo: invoiceNo,
                PONo: poNo,

                DiscountType: discountType,
                TotalDiscountInInvoice: totalDiscount,
                DiscountPercentage: discountPercent,
                DetermineWhenTaxApplicable: determineTaxApply,

                SubTotal_TotalDiscount_Amount: subTotalDiscount,
                SubTotal_OtherCharges_Amount: subTotalOtherCharges,

                DeliveryChargeIncludedInInvoice: deliveryCharge,
                DeliveryCharge_CGST: deliveryCGST,
                DeliveryCharge_SGST: deliverySGST,

                SubTotal_OtherTaxes_Amount: otherTaxAmount,
                TCS_Percent: tcsPercent,
                TDS_Percent: tdsPercent,

                GrandTotal_Amount: grandTotal,

                PaymentType: paymentType,
                PaymentDate: paymentType === 2 ? $("#paymentDate").val() : null,
                PaidAmount: paymentType === 2 ? paidAmount : null,
                PaymentModeId: paymentType === 2 ? $("#paymentModeId").val() : null,
                PaymentRefNo: paymentType === 2 ? $("#paymentRefNo").val() : null,
                PaymentAccountId: paymentType === 2 ? $("#paymentAccountId").val() : null,
                PaymentRemarks: paymentType === 2 ? $("#paymentRemarks").val() : null,

                ChequeBank: paymentType === 2 ? $("#chequeBank").val() : null,
                ChequeNo: paymentType === 2 ? $("#chequeNo").val() : null,
                ChequeDate: paymentType === 2 ? $("#chequeDate").val() : null,

                UploadInvoiceFile: base64File, // NULL or base64

                IsUpdateInventoryStock: isUpdateInventory,
                JSON_PurchaseItems: itemsList,
                IsActive: true,
                ActionBy: sessionStorage.getItem("userId") || 0
            };

            //console.log("PAYLOAD:", payload);

            $.ajax({
                url: "/api/Purchase/SavePurchase",
                type: "POST",
                data: JSON.stringify(payload),
                contentType: "application/json",
                success: function (response) {

                    if (response.status.toLowerCase() == "success") {
                        Swal.fire({
                            position: "top-end",
                            icon: "success",
                            title: "Purchase Saved",
                            showConfirmButton: false,
                            timer: 900
                        });
                        resetFormFields(); // Reset form fields when modal is shown
                        fetchData();
                        //$("#addEventModal").modal("hide");
                    }
                    else {
                        Swal.fire("Error", response.message, "error");
                    }
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Purchase Save Error");
                }
            });
        }

        // If file exists → convert → save
        if (file) {
            getBase64WithoutMeta(file, function (base64File) {
                submitSave(base64File);
            });
        }
        else {
            submitSave(null);
        }
    });

    // Delete Event handler for remove button click
    $(document).on('click', '.btn-delete', function (event) {
        var canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Delete. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault(); // Stop navigation
            $('#deleteRecordModal').modal('hide');
        }
        else {
            // Get the DataTable instance
            var table = $('#tableMain table').DataTable();

            // Get the row object for the clicked Edit button
            var row = $(this).closest('tr');
            var rowData = table.row(row).data(); // Get the data object for this row

            // Extract values from the row data object
            var id = rowData.Id;

            $('#deleteRecordModal').data('id', id).modal('show');
        }
    });

    // Handle delete confirmation
    $('#delete-record').click(function () {
        var id = $('#deleteRecordModal').data('id');

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $('#btn-loader').show();
        $.ajax({
            url: "/api/Purchase/DeletePurchase",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                //console.log("Delete Data:", response);
                // Parse the response if it's a string
                if (typeof response === 'string') {
                    response = JSON.parse(response);
                }

                //debugger
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Deleted !!",
                        showConfirmButton: false,
                        timer: 900
                    });
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: response.message,
                        icon: "error"
                    });
                }
                fetchData();
                //$('#btn-loader').hide();
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr) {
                //$('#btn-loader').hide();
                //alert('Failed to delete record: ' + xhr.responseText);
                //console.log(xhr.responseText);
                Swal.fire({
                    title: "Delete City Error",
                    text: xhr.responseText,
                    icon: "error"
                });
            },
            complete: function () {
                $('#btn-loader').hide(); // Ensure loader is hidden in any case
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

    //PO No - toggle
    $(document).on("click", "#togglePONo", function () {
        let $poField = $("#purchasePONo");

        if ($poField.prop("disabled")) {
            // Enable field
            $poField.prop("disabled", false);
            $poField.focus();
        } else {
            // Disable field
            $poField.prop("disabled", true);
        }
    });

    // DELETE ROW
    $(document).on("click", ".btnDeleteRow", function () {
        $(this).closest("tr").remove();
        updateSrNo();

        if ($("#purchaseItemsBody tr").length === 0) {
            addInitialRow();
        }
    });


    // RAW MATERIAL SELECT → SHOW STOCK
    $(document).on("change", ".rawMaterialSelect", function () {
        let currentQty = $(this).find("option:selected").data("currentqty") ?? 0;
        $(this).closest("td").find(".stockHint").text("Stock: " + currentQty).removeClass("d-none");
    });

    // ADD NEW ROW BUTTON
    $("#btnAddLine").on("click", function () {
        addNewRow();
    });

    //Payment Mode
    $(document).on("change", "input[name='paymentType']", function () {

        if ($("#payPaid").is(":checked")) {
            $("#paidSection").removeClass("d-none");
            $("#paymentDate").val(new Date().toISOString().split("T")[0]);
        }
        else {
            $("#paidSection").addClass("d-none");

            // reset fields
            $("#paymentDate").val("");
            $("#paidAmount").val("");
            $("#paymentModeId").val("0");
            $("#paymentRefNo").val("");
        }
    });

    $("#paymentModeId").on("change", function () {
        const selectedText = $("#paymentModeId option:selected").text().trim().toLowerCase();

        if (selectedText.includes("cheque")) {
            $(".cheque-fields").removeClass("d-none");
        }
        else {
            $(".cheque-fields").addClass("d-none");
            $("#chequeBank, #chequeNo, #chequeDate").val("");
        }
    });

    // OUTSTANDING PAY MODAL

    //btn - remaining - click
    $(document).on("click", ".btn-remaining", function () {

        const purchaseId = $(this).data("id");
        const poNo = $(this).data("pono");
        const remaining = $(this).text().replace("₹", "").trim();

        console.log("PurchaseId:", purchaseId);
        console.log("PONo:", poNo);

        $("#outPurchaseId").val(purchaseId);
        $("#outRemainingAmount").val(remaining);
        $("#outRemainingText").text(remaining);
        $("#outPONo").text("#" + poNo);

        // Default values
        $("#outPaymentDate").val(new Date().toISOString().split("T")[0]);
        $("#outPaidAmount").val(remaining);

        $("#outPaymentModeId").prop("selectedIndex", 0);
        $("#outAccountId").prop("selectedIndex", 0);
        $("#outReferenceNo").val("");
        $("#outRemarks").val("");
        $(".cheque-out").addClass("d-none");

        $("#outstandingPaymentModal").modal("show");
    });
    //outPaymentModeId - change
    $(document).on("change", "#outPaymentModeId", function () {
        const text = $("#outPaymentModeId option:selected").text().toLowerCase();
        if (text.includes("cheque")) {
            $(".cheque-out").removeClass("d-none");
        } else {
            $(".cheque-out").addClass("d-none");
        }
    });
    //outPaidAmount - input
    $("#outPaidAmount").on("input", function () {
        const remaining = Number($("#outRemainingAmount").val()) || 0;
        if (Number(this.value) > remaining) {
            this.value = remaining;
        }
    });
    //btnSaveOutstanding
    $("#btnSaveOutstanding").click(function () {

        const remaining = Number($("#outRemainingAmount").val()) || 0;

        const payload = {
            PurchaseId: Number($("#outPurchaseId").val()),
            PaymentDate: $("#outPaymentDate").val(),
            PaidAmount: Number($("#outPaidAmount").val()),
            PaymentModeId: Number($("#outPaymentModeId").val()),
            AccountId: Number($("#outAccountId").val()),
            ReferenceNo: $("#outReferenceNo").val(),
            Remarks: $("#outRemarks").val(),
            ChequeBank: $("#outChequeBank").val(),
            ChequeNo: $("#outChequeNo").val(),
            ChequeDate: $("#outChequeDate").val(),
            ActionBy: Number(sessionStorage.getItem("userId") || 0)
        };

        /* ================= BASIC REQUIRED CHECK ================= */
        if (!payload.PaymentDate || !payload.PaymentModeId || !payload.AccountId) {
            Swal.fire("Warning", "Please fill all required fields", "warning");
            return;
        }

        /* ================= AMOUNT VALIDATION ================= */
        if (!payload.PaidAmount || payload.PaidAmount <= 0) {
            Swal.fire("Warning", "Paid amount must be greater than zero", "warning");
            return;
        }

        if (payload.PaidAmount > remaining) {
            Swal.fire(
                "Invalid Amount",
                `Paid amount cannot exceed remaining amount (₹ ${remaining.toLocaleString("en-IN")})`,
                "warning"
            );
            return;
        }

        $.ajax({
            url: "/api/Purchase/SavePurchaseOutstandingPayment",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire("Success", res.message, "success");
                    $("#outstandingPaymentModal").modal("hide");
                    fetchData(); // reload grid
                }
                else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Failed to save payment");
            }
        });
    });

    // OUTSTANDING PAY MODAL

    // 3-dot icon click event
    $("#btnSubTotalMenu").on("click", function () {
        console.log("Subtotal Amount:", $("#txtSubTotal").text());
    });

    // SELECT ALL ROWS
    $(document).on("change", "#selectAllItems", function () {
        let isChecked = $(this).is(":checked");
        $(".row-check").prop("checked", isChecked);
    });

    //FIle Download
    // DOWNLOAD INVOICE FILE
    $("#btnDownloadInvoice").on("click", function () {
        let fileUrl = $("#invoiceFileLink").val();

        if (!fileUrl || fileUrl.trim() === "") {
            Swal.fire("Error", "File not found!", "error");
            return;
        }

        // Create hidden <a> tag and trigger click
        const tempLink = document.createElement("a");
        tempLink.href = fileUrl;
        tempLink.target = "_blank";   // open in new tab (browser safe)
        tempLink.download = "";       // allow download if server permits

        document.body.appendChild(tempLink);
        tempLink.click();
        document.body.removeChild(tempLink);
    });

    //Vendor Widget -------------- START ------------------------    
    let selectedVendorDuration = "thismonth";
    loadVendorPayables(selectedVendorDuration);

    $(".chart-filter-Vendor .payment-option").on("click", function (e) {
        e.preventDefault();

        const newFilter = $(this).data("value");
        if (selectedVendorDuration === newFilter) return;

        selectedVendorDuration = newFilter;

        $(".payment-option").removeClass("active");
        $(this).addClass("active");

        $("#VendorChartFilter").text($(this).text());
        loadVendorPayables(selectedVendorDuration);
    });
    //Vendor Widget -------------- END ------------------------    

});

//Widgets  -- START ------------------------------------------------------------
// Fetch widget data
function loadWidgetsData() {

    $.ajax({
        url: "/api/Purchase/GetPurchaseList_Widgets",
        method: "GET",
        contentType: "application/json",
        success: function (res) {
            console.log("res widgets:", res);
            if (res.status && res.status.toLowerCase() === "success" && res.data) {
                $("#widget_total_purchase").text(res.data.GrandTotal_Amount);
                $("#widget_outstanding").text(res.data.Outstanding_Amount);
                $("#widget_tax_paid").text(res.data.TaxPaidToSeller);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Purchase Counts Fetch Error");
        }
    });
}

function loadVendorPayables(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardVendorPayables",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            console.log("Vendor Payables API:", res);

            let html = "";

            if (res.status === "Success" && Array.isArray(res.data) && res.data.length > 0) {
                res.data.slice(0, 100).forEach(item => {

                    let badgeClass =
                        item.Status === "Overdue" ? "bg-danger" :
                            item.Status === "Pending" ? "bg-warning text-white" :
                                "bg-secondary";

                    html += `
                        <tr>
                            <td>${item.VendorName}</td>
                            <td class="text-end fw-semibold">
                                ₹ ${formatINR(item.PendingAmount)}
                            </td>
                            <td class="text-center">
                                <span class="badge ${badgeClass}">
                                    ${item.Status}
                                </span>
                            </td>
                        </tr>`;
                });               
            }
            else {
                html = `
                    <tr>
                        <td colspan="3" class="text-muted text-center">
                            No vendor payables
                        </td>
                    </tr>`;
            }

            $("#vendorPayablesBody").html(html);
        },
        error: function () {
            $("#vendorPayablesBody").html(`
                <tr>
                    <td colspan="3" class="text-danger text-center">
                        Error loading data
                    </td>
                </tr>
            `);
        }
    });
}


//Widgets  -- END --------------------------------------------------------------

/// TABLE ITEMS GRID ----- START -------------------------------
function formatQty(val) {
    return (parseFloat(val) % 1 === 0)
        ? parseInt(val)                // whole number → 5
        : parseFloat(val).toFixed(2);  // decimal → 1.20
}

// CALL THIS ON RESET OR MODAL OPEN
function addInitialRow() {
    $("#purchaseItemsBody").empty();
    rowIndex = 0;
    addNewRow();
}

// ADD NEW ROW
function addNewRow() {
    rowIndex++;

    //RAW MATERIALS DROPDOWN
    let rawMaterial_Html_dropdown = `<select class="form-select form-select-sm rawMaterialSelect">`;
    rawMaterial_Html_dropdown += `<option value="0" data-currentqty="0">--Select Raw Materail--</option>`;
    rawMaterials.forEach(x => {
        rawMaterial_Html_dropdown += `
                <option value="${x.Id}" data-currentqty="${x.CurrentQty ?? 0}" 
                    data-unitid='${JSON.stringify(x.JSON_PurchaseUnits || []).replace(/"/g, "&quot;")}'>
                    ${x.Name}
                </option>
            `;
    });
    rawMaterial_Html_dropdown += `</select>
            <div class="text-muted small stockHint d-none"></div>`;

    //UNITS DROPDOWN
    let unit_Html_dropdown = `<select class="form-select form-select-sm unitSelect">`;
    unit_Html_dropdown += `<option value="0" data-currentqty="0"></option>`;
    unitMaster.forEach(x => {
        unit_Html_dropdown += `
                <option value="${x.Id}">
                    ${x.UnitName}
                </option>
            `;
    });
    unit_Html_dropdown += `</select>`;

    let newRow = $(`
            <tr data-index="${rowIndex}">

                <!-- Checkbox -->
                <td class="text-center align-middle"><input type="checkbox" class="row-check"></td>

                <!-- SR NO -->
                <td class="text-center align-middle sr-no">${rowIndex}</td>

                <!-- Raw Material -->
                <td>
                    ${rawMaterial_Html_dropdown}
                </td>

                <!-- Qty -->
                <td>
                    <input type="number" class="form-control form-control-sm qtyInput" min="0" step="0.01" value="0">
                </td>

                <!-- Unit -->
                <td>
                    ${unit_Html_dropdown}
                </td>

                <!-- Order Qty Label -->
                <td class="orderQtyLabel fw-bold">0</td>

                <!-- Price -->
                <td>
                    <input type="number" class="form-control form-control-sm priceInput" min="0" step="0.01" value="0">
                </td>

                <!-- Amount -->
                <td>
                    <input type="number" class="form-control form-control-sm amountInput"  min="0" step="0.01" value="0">
                </td>

                <!-- Tax % -->
                <td>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control form-control-sm cgstInput" placeholder="CGST %"  min="0" step="0.01">
                        <input type="number" class="form-control form-control-sm sgstInput" placeholder="SGST %"  min="0" step="0.01">
                        <input type="number" class="form-control form-control-sm igstInput" placeholder="IGST %"  min="0" step="0.01">
                    </div>
                </td>

                <!-- Actions -->
                <td>
                    <button class="btn btn-sm btn-outline-danger btnDeleteRow"><i class="fa fa-trash"></i></button>
                </td>

            </tr>
        `);

    $("#purchaseItemsBody").append(newRow);
    updateSrNo();

    attachRawMaterialEvents(newRow);

    //set first real raw material & unit selected
    // newRow.find(".rawMaterialSelect option:eq(1)").prop("selected", true);
    // newRow.find(".unitSelect option:eq(1)").prop("selected", true);
}

// Attach events to RAW MATERIAL dropdown & enable fields
function attachRawMaterialEvents(row) {
    let rawSelect = row.find(".rawMaterialSelect");
    rawSelect.off("change").on("change", function () {
        // Load units for selected raw material
        updateUnitsDropdown(this);
    });
}

function updateUnitsDropdown(rawMaterialSelect) {
    let selectedOption = $(rawMaterialSelect).find("option:selected");

    // Parse JSON_PurchaseUnits
    let unitsJsonRaw = selectedOption.attr("data-unitid");
    // single → double quotes
    unitsJsonRaw = unitsJsonRaw; //.replace(/'/g, '"');
    let unitsJson = [];
    try {
        unitsJson = JSON.parse(unitsJsonRaw);
    } catch (e) {
        console.error("Invalid JSON in data-unitid:", unitsJsonRaw, e);
    }

    //console.log("unitsJson 2:", unitsJson);
    let row = $(rawMaterialSelect).closest("tr");
    let unitDropdown = row.find(".unitSelect");

    // Clear existing options
    unitDropdown.html(`<option value="0">Select</option>`);

    // If JSON is empty → Leave dropdown empty
    if (!unitsJson || unitsJson.length === 0) {
        return;
    }

    //console.log("unitsJson:", unitsJson, ", unitDropdown:", unitDropdown);
    // Populate allowed units only
    let isFirst = true;
    unitsJson.forEach(u => {
        let unitObj = unitMaster.find(x => x.Id === u.UnitId);
        if (unitObj) {
            unitDropdown.append(
                `<option value="${unitObj.Id}" ${isFirst ? "selected" : ""}>
                ${unitObj.UnitName}
             </option>`
            );
            isFirst = false;
        }
    });
}

function buildRowForEdit(item) {

    rowIndex++;

    let rawMaterialId = item.RawMaterialId;
    let qty = item.Qty;
    let unit = item.UnitId;
    let price = item.Price;
    let amount = item.Amount;
    let cgst = item.CGST;
    let sgst = item.SGST;
    let igst = item.IGST;

    // ----------------------------------------
    // 1️⃣ Populate RAW MATERIAL dropdown same as ADD
    // ----------------------------------------
    //RAW MATERIALS DROPDOWN
    let rawMaterial_Html_dropdown = `<select class="form-select form-select-sm rawMaterialSelect">`;
    rawMaterial_Html_dropdown += `<option value="0" data-currentqty="0">--Select Raw Materail--</option>`;
    rawMaterials.forEach(x => {
        rawMaterial_Html_dropdown += `
                <option value="${x.Id}" data-currentqty="${x.CurrentQty ?? 0}"
                    data-unitid='${JSON.stringify(x.JSON_PurchaseUnits || []).replace(/"/g, "&quot;")}'>
                    ${x.Name}
                </option>
            `;
    });
    rawMaterial_Html_dropdown += `</select>
            <div class="text-muted small stockHint d-none"></div>`;

    //UNITS DROPDOWN
    let unit_Html_dropdown = `<select class="form-select form-select-sm unitSelect">`;
    unit_Html_dropdown += `<option value="0" data-currentqty="0"></option>`;
    unitMaster.forEach(x => {
        unit_Html_dropdown += `
                <option value="${x.Id}">
                    ${x.UnitName}
                </option>
            `;
    });
    unit_Html_dropdown += `</select>`;

    let row = $(`
            <tr data-index="${rowIndex}">
                <td class="text-center align-middle"><input type="checkbox" class="row-check"></td>
                <td class="text-center align-middle sr-no">${rowIndex}</td>

                <!-- Raw Material -->
                <td>
                    ${rawMaterial_Html_dropdown}
                </td>

                <!-- Qty -->
                <td>
                    <input type="number" class="form-control form-control-sm qtyInput"
                           min="0" step="0.01" value="${qty}">
                </td>

                <!-- Unit -->
                <td>
                    ${unit_Html_dropdown}
                </td>

                <!-- ORDER QTY -->
                <td class="orderQtyLabel fw-bold">0</td>

                <!-- PRICE -->
                <td>
                    <input type="number" class="form-control form-control-sm priceInput"
                           min="0" step="0.01" value="${price}">
                </td>

                <!-- AMOUNT -->
                <td>
                    <input type="number" class="form-control form-control-sm amountInput"
                           min="0" step="0.01" value="${amount}">
                </td>

                <!-- TAX -->
                <td>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control form-control-sm cgstInput" placeholder="CGST %" value="${cgst}">
                        <input type="number" class="form-control form-control-sm sgstInput" placeholder="SGST %" value="${sgst}">
                        <input type="number" class="form-control form-control-sm igstInput" placeholder="IGST %" value="${igst}">
                    </div>
                </td>

                <!-- ACTION -->
                <td>
                    <button class="btn btn-sm btn-outline-primary btnEditRow"><i class="fa fa-pen"></i></button>
                    <button class="btn btn-sm btn-outline-danger btnDeleteRow"><i class="fa fa-trash"></i></button>
                </td>

            </tr>
        `);

    attachRawMaterialEvents(row);

    // ----------------------------------------
    // 2️⃣ Set Raw Material + TRIGGER CHANGE (loads units)
    // ----------------------------------------
    let rawSelect = row.find(".rawMaterialSelect");
    rawSelect.val(rawMaterialId).trigger("change");

    // Show current stock for selected material
    let sel = row.find(".rawMaterialSelect option:selected");
    let currentStock = sel.data("currentqty") ?? 0;
    row.find(".stockHint").text("Stock: " + currentStock).removeClass("d-none");

    // ----------------------------------------
    // 3️⃣ After units are loaded → set the correct unit
    // ----------------------------------------
    setTimeout(() => {
        row.find(".unitSelect").val(unit).trigger("change");
    }, 50);

    return row;
}

// UPDATE SR NO AFTER DELETE
function updateSrNo() {
    $("#purchaseItemsBody tr").each(function (i) {
        $(this).find(".sr-no").text(i + 1);
    });
}
/// TABLE ITEMS GRID ----- END -------------------------------

//SHOW GRID TABLE
function renderTable(data) {
    console.log("Purchase Data render:", data);

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {
        $('#tableMain tbody').html(`
                <tr>
                    <td colspan="9" class="text-center fw-bold text-danger">No Data Found !!</td>
                </tr>
            `);

        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { title: "Sr. No." },
                { title: "From" },
                { title: "Invoice Date" },
                { title: "Invoice No" },
                { title: "PO/MRN No" },
                { title: "Total" },
                { title: "Payment" },
                { title: "Created By" },
                { title: "Status" },
                { title: "Action" }
            ]
        });

        return;
    }

    // Add SR No.
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({
        data: data,
        columns: [
            { data: "srno", title: "#" },
            {
                data: null,
                title: "From",
                render: (d) => `
                        <b>${d.SupplierName ?? ""}</b><br>
                        <small class="text-muted">${d.PurchaseFromName ?? ""}</small>
                    `
            },
            { data: "InvoiceDate_Format", title: "Invoice Date" },
            {
                data: "InvoiceNo",
                title: "Invoice No",
                render: function (d) {
                    if (!d) return "-";
                    return `#${d}`;
                }
            },
            {
                data: "PONo",
                title: "PO/MRN No",
                render: function (d) {
                    if (!d) return "-";
                    return `#${d}`;
                }
            },

            {
                data: "GrandTotal_Amount",
                title: "Total",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: null,
                title: "Paid",
                render: (d) => {
                    let paid = d.PaidAmount ?? 0;
                    return `₹ ${paid}`;
                }
            },

            {
                data: null,
                title: "Remaining",
                className: "text-center",
                render: (d) => {
                    let remain = d.Remaining_Amount ?? 0;

                    if (remain > 0) {
                        return `
                            <button class="btn btn-sm btn-action-xss btn-outline-warning btn-remaining"
                                    data-id="${d.Id}"
                                    data-pono="${d.PONo ?? ''}">
                                ₹ ${remain}
                            </button>
                        `;
                    }

                    // zero / no remaining → plain text
                    return `<span class="text-muted">₹ ${remain}</span>`;
                }
            },

            {
                data: null,
                title: "Created By",
                render: (d) => `
                        <b>${d.Created_By_EmpName ?? "-"}</b><br>
                        <small class="text-muted">Created: ${d.Created_On}</small><br>
                        <small class="text-muted">Modified: ${d.Modified_On ?? ""}</small>
                    `
            },

            {
                data: "Status",
                title: "Status",
                render: function (status) {

                    let badgeClass = "bg-secondary";

                    if (!status) status = "Unknown";

                    switch (status.toLowerCase()) {
                        case "pending":
                            badgeClass = "bg-warning text-dark";
                            break;
                        case "approved":
                        case "saved":
                            badgeClass = "bg-success";
                            break;
                        case "rejected":
                            badgeClass = "bg-danger";
                            break;
                        case "paid":
                            badgeClass = "bg-primary";
                            break;
                    }
                    return `<span class="badge ${badgeClass}">${status}</span>`;
                }
            },

            {
                data: null,
                title: "Action",
                render: function (data, type, row) {
                    return `
                            <div class="d-flex gap-1">
                                <button class="btn btn-outline-warning btn-sm btn-action-xss btn-edit" data-id="${row.Id}">
                                    <i class="fa fa-edit"></i>
                                </button>
                                <button class="btn btn-outline-danger btn-sm btn-action-xss btn-delete" data-id="${row.Id}">
                                    <i class="fa fa-trash"></i>
                                </button>
                            </div>
                        `;
                }
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,
        stateSave: false
    });
}

//get data from API
function fetchData() {

    var payload = { Id: 0, SupplierId: 0 };
    $.ajax({
        url: "/api/Purchase/GetPurchaseList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("Purchase Fetch:", response);
            var data = response.data ?? [];
            renderTable(data);

            loadWidgetsData();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Purchase Fetch Error");
        }
    });
}

// Reset form fields
function resetFormFields() {

    // ========== BASIC INFO ==========
    $("#purchaseId").val(0);

    // Purchase From = Supplier (default)
    $("#purchaseFromSupplier").prop("checked", true);

    // Supplier dropdown
    $("#purchaseSupplier").val("0");

    // Invoice Date = Today
    let today = new Date().toISOString().split("T")[0];
    $("#purchaseInvoiceDate").val(today);

    // Invoice No + PO No
    fetchNextNumbers();

    // Reset P.O toggle button icon
    $("#togglePONo i").removeClass().addClass("fa fa-pen");

    // ========== ITEMS TABLE ==========
    $("#selectAllItems").prop("checked", false);
    $(".row-check").prop("checked", false);

    $("#purchaseItemsBody").empty();
    rowIndex = 0;

    // Add first row ONLY if raw materials already loaded
    if (rawMaterials.length > 0) {
        addInitialRow();
    }

    // ========== TOTALS & PANELS ==========

    // Totals
    $("#txtSubTotal").text("0.00");
    $("#txtTotalDiscount").text("0.00");
    $("#txtOtherCharges").text("0.00");
    $("#txtOtherTaxes").text("0.00");
    $("#txtGrandTotal").text("0.00");

    // Hide all left panels
    $("#discountDetails").hide();
    $("#otherChargeDetails").hide();
    $("#otherTaxDetails").hide();

    // Reset discount panel
    $("#discountValue").val("0");
    $("#discountCalc").text("00.00");
    $("input[name='discountType'][value='1']").prop("checked", true); // Amount
    $("input[name='discountTaxType'][value='1']").prop("checked", true); // Before Tax

    // Reset other charges
    $("#deliveryCharge").val("");
    $("#deliveryCGST").val("");
    $("#deliverySGST").val("");

    // Reset other taxes
    $("#tcsPercent").val("");
    $("#tcsCalc").text("00.00");
    $("#tdsPercent").val("");
    $("#tdsCalc").text("00.00");

    // Payment Section
    $("#payUnpaid").prop("checked", true);
    $("#paidSection").addClass("d-none");

    $("#paymentDate").val("");
    $("#paidAmount").val("");
    $("#paymentModeId").val("0");
    $("#paymentRefNo").val("");

    // Inventory Update
    $("#chkUpdateInventory").prop("checked", true);

    // Invoice File Reset
    $("#uploadInvoiceFile").val("");
    //$("#invoicePreview").attr("src", "/assets2/img/image_icon.png");
    $("#invoiceFileLink").val("");             // Clear saved URL
    $("#btnDownloadInvoice").addClass("d-none"); // Hide download
    // Scroll modal to top
    $(".modal-body").scrollTop(0);

    //console.log("Purchase form reset completed.");
}

function validatePurchaseForm() {

    // Supplier required
    if ($("#purchaseSupplier").val() == "0") {
        Swal.fire("Validation", "Please select a Supplier", "warning");
        return false;
    }

    // Invoice Date required
    if ($("#purchaseInvoiceDate").val() === "") {
        Swal.fire("Validation", "Invoice Date is required", "warning");
        return false;
    }

    // Ensure items exist
    if ($("#purchaseItemsBody tr").length === 0) {
        Swal.fire("Validation", "Add at least one purchase item", "warning");
        return false;
    }

    // Validate each row
    let isValid = true;

    $("#purchaseItemsBody tr").each(function () {

        let row = $(this);

        let rawMaterial = row.find(".rawMaterialSelect").val();
        let qty = row.find(".qtyInput").val();
        let unit = row.find(".unitSelect").val();
        let price = row.find(".priceInput").val();

        if (rawMaterial == "0") {
            Swal.fire("Validation", "Select Raw Material in all rows", "warning");
            isValid = false;
            return false;
        }

        if (qty === "" || qty <= 0) {
            Swal.fire("Validation", "Qty must be greater than 0", "warning");
            isValid = false;
            return false;
        }

        if (unit == "0") {
            Swal.fire("Validation", "Select Unit in all rows", "warning");
            isValid = false;
            return false;
        }

        if (price === "" || price < 0) {
            Swal.fire("Validation", "Price is missing in some rows", "warning");
            isValid = false;
            return false;
        }
    });

    if (!isValid) return false;

    // If Payment Type = Paid, validate fields
    if ($("#payPaid").is(":checked")) {

        if (!$("#paymentDate").val()) {
            Swal.fire("Validation", "Payment Date is required", "warning");
            return false;
        }

        if ($("#paidAmount").val() === "" || $("#paidAmount").val() <= 0) {
            Swal.fire("Validation", "Paid Amount must be greater than 0", "warning");
            return false;
        }

        if ($("#paymentModeId").val() == "0") {
            Swal.fire("Validation", "Select a Payment Mode", "warning");
            return false;
        }
    }

    return true; // All validations passed
}

function fetchNextNumbers() {

    // ---- Fetch Purchase Order Number ----
    $.ajax({
        url: "/api/Purchase/GetNextPurchaseOrderNo",
        method: "GET",
        success: function (res) {

            if (res.status === "Success") {
                var nextPONumber = res.data.NextPONo;
                //console.log("Next PO Number:", nextPONumber);
                $("#purchasePONo").val(nextPONumber).prop("disabled", true);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Next PO No. Fetch Error");
        }
    });


    // ---- Fetch Invoice Number ----
    $.ajax({
        url: "/api/Purchase/GetNextInvoiceNo",
        method: "GET",
        success: function (res) {

            if (res.status === "Success") {
                var nextInvoiceNumber = res.data.NextInvoiceNo;
                console.log("Next Invoice Number:", nextInvoiceNumber);
                $("#purchaseInvoiceNo").val(nextInvoiceNumber);

            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Next InvoiceNo Fetch Error");
        }
    });

}

//STATE DROPDOWN
function loadStateDropdown() {
    $.ajax({
        url: "/api/State/GetStateList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {

            $("#stateId").html(`<option value="0">-- Select State --</option>`);

            res.data.forEach(s => {
                $("#stateId").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "State Fetch Error");
        }
    });
}

//SUPPLIER DROPDOWN
function loadSupplierDropdown() {
    var payload = { Id: 0 };
    $.ajax({
        url: "/api/Supplier/GetSupplierList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);
            $("#purchaseSupplier").html(`<option value="0">-- Select Supplier --</option>`);
            response.data.forEach(s => {
                $("#purchaseSupplier").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Supplier Fetch Error");
        }
    });
}

//RAW MATERIALS DROPDOWN
var rawMaterials = [];
function loadRawMaterialsDropdown() {
    $.ajax({
        url: "/api/RawMaterialsMaster/GetRawMaterialList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ItemCategoryId: 0 }),
        success: function (response) {
            //console.log("response rawMaterials:", response);
            rawMaterials = response.data;
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "RawMaterials Fetch Error")
        }
    });
}

//UNIT DROPDOWN
var unitMaster = [];
function loadUnitDropdown() {
    $.ajax({
        url: "/api/UnitMaster/GetUnitMasterList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //console.log("response unitMaster:", response);
            unitMaster = response.data;
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "UnitMaster Fetch Error")
        }
    });
}

//PAYMENT MODE DROPDOWN
function loadPaymentModeDropdown() {
    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //console.log("response PaymentMode:", response);                

            if (response.status !== "Success") {
                console.warn("PaymentMode API Error:", response.message);
                return;
            }
            const $ddlPaymentMode = $("#paymentModeId, #outPaymentModeId");

            if (!response.data || response.data.length === 0) {
                $ddlPaymentMode.prop("disabled", true).html(`<option value="0">No PaymentMode Found</option>`);
            }

            $ddlPaymentMode.html(`<option value="0">-- Select Payment Mode --</option>`);
            response.data.forEach(s => {
                $ddlPaymentMode.append(`<option value="${s.Id}">${s.PaymentModeName}</option>`);
            });
            $ddlPaymentMode.val(response.data[0].Id); // first item by ID
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "PaymentMode Fetch Error")
        }
    });
}

//ACCOUNT DROPDOWN
function loadAccountDropdown() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {

            if (res.status !== "Success") {
                console.warn("Account API Error:", res.message);
                return;
            }

            const $ddlAccount = $("#paymentAccountId, #outAccountId");

            if (!res.data || res.data.length === 0) {
                $ddlAccount.prop("disabled", true).html(`<option value="0">No Accounts Found</option>`);
            }

            // Clear & add default
            $ddlAccount.html(`<option value="0">-- Select Account --</option>`);

            res.data.forEach(a => {
                $ddlAccount.append(`<option value="${a.Id}">${a.AccountName}</option>`);
            });

            // Optional: auto-select default account
            const defaultAcc = res.data.find(x => x.IsDefaultAccount === true);
            if (defaultAcc) {
                $ddlAccount.val(defaultAcc.Id);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Account Fetch Error");
        }
    });
}

//<!--BEHAVIOUR - ADD MODAL - DISCOUNT, TAX OTHER -- START -->
// Hide all except selected
function toggleSection(sectionId) {

    const allSections = ["#discountDetails", "#otherChargeDetails", "#otherTaxDetails"];

    allSections.forEach(id => {
        if (id === sectionId) {
            $(id).slideToggle(200);     // toggle selected
        } else {
            $(id).slideUp(200);         // hide others
        }
    });
}

// Discount Toggle
$("#btnTotalDiscount").on("click", function () {
    toggleSection("#discountDetails");
});

// Other Charges Toggle
$("#btnOtherCharges").on("click", function () {
    toggleSection("#otherChargeDetails");
});

// Other Taxes Toggle
$("#btnOtherTaxes").on("click", function () {
    toggleSection("#otherTaxDetails");
});

function calculateDiscount() {
    let subTotal = parseFloat($("#txtSubTotal").text()) || 0;
    let discValue = parseFloat($("#discountValue").val()) || 0;
    let discType = $("input[name='discountType']:checked").val();

    let finalDisc = 0;

    if (discType == "1") {
        finalDisc = discValue;
    } else {
        finalDisc = (subTotal * discValue / 100);
    }

    $("#discountCalc").text(finalDisc.toFixed(2));
    $("#txtTotalDiscount").text(finalDisc.toFixed(2));
}

$("#discountValue, input[name='discountType']").on("input change", function () {
    calculateDiscount();
});

function calculateOtherCharges() {

    let delivery = parseFloat($("#deliveryCharge").val()) || 0;
    let cgst = parseFloat($("#deliveryCGST").val()) || 0;
    let sgst = parseFloat($("#deliverySGST").val()) || 0;

    // GST amounts
    let cgstAmt = (delivery * cgst) / 100;
    let sgstAmt = (delivery * sgst) / 100;

    // TOTAL OTHER CHARGES
    let totalOther = delivery + cgstAmt + sgstAmt;

    $("#txtOtherCharges").text(totalOther.toFixed(2));
}

$("#deliveryCharge, #deliveryCGST, #deliverySGST").on("input", function () {
    calculateOtherCharges();
});

function calculateTaxes() {

    let subTotal = parseFloat($("#txtSubTotal").text()) || 0;

    // TCS
    let tcsPercent = parseFloat($("#tcsPercent").val()) || 0;
    let tcs = (subTotal * tcsPercent) / 100;
    $("#tcsCalc").text(tcs.toFixed(2));

    // TDS
    let tdsPercent = parseFloat($("#tdsPercent").val()) || 0;
    let tds = (subTotal * tdsPercent) / 100;
    $("#tdsCalc").text(tds.toFixed(2));

    // Final Other Taxes = TCS - TDS
    let finalOtherTax = tcs - tds;

    $("#txtOtherTaxes").text(finalOtherTax.toFixed(2));

    return finalOtherTax;
}

$("#tcsPercent, #tdsPercent").on("input", function () {
    calculateTaxes();
});

//GRAND TOTAL
function calculateGrandTotal() {

    let subTotal = parseFloat($("#txtSubTotal").text()) || 0;
    let discount = parseFloat($("#txtTotalDiscount").text()) || 0;
    let otherCharges = parseFloat($("#txtOtherCharges").text()) || 0;
    let otherTaxes = parseFloat($("#txtOtherTaxes").text()) || 0;

    let grandTotal = subTotal - discount + otherCharges + otherTaxes;

    $("#txtGrandTotal").text(grandTotal.toFixed(2));
}

$("#discountValue, #deliveryCharge, #deliveryCGST, #deliverySGST, #tcsPercent, #tdsPercent")
    .on("input", function () {
        calculateDiscount();
        calculateOtherCharges();
        calculateTaxes();
        calculateGrandTotal();
    });

//<!--BEHAVIOUR - ADD MODAL - DISCOUNT, TAX OTHER -- END -->

//<!--SUB TOTAL - MODAL -- START -->
$("#btnSubTotalMenu").on("click", function () {
    generateSubTotalBreakdown();
    $("#subTotalBreakdownModal").modal("show");
});

// AUTO CALCULATIONS
$(document).on("input change", ".qtyInput, .priceInput, .unitSelect, .cgstInput, .sgstInput, .igstInput", function () {
    let row = $(this).closest("tr");

    let qty = parseFloat(row.find(".qtyInput").val()) || 0;
    let price = parseFloat(row.find(".priceInput").val()) || 0;
    let unitText = row.find(".unitSelect option:selected").text().trim();

    // Order Qty = Qty + Unit text (just display purpose)
    let qtyFormatted = formatQty(qty);
    row.find(".orderQtyLabel").text(`${qtyFormatted} ${unitText}`);

    // Amount
    row.find(".amountInput").val((qty * price).toFixed(2));

    calculateRowTotal($(this).closest("tr"));
    calculateSubTotals();
});

function calculateRowTotal(row) {

    let qty = parseFloat(row.find(".qtyInput").val()) || 0;
    let price = parseFloat(row.find(".priceInput").val()) || 0;

    let baseAmount = qty * price;

    let cgstPercent = parseFloat(row.find(".cgstInput").val()) || 0;
    let sgstPercent = parseFloat(row.find(".sgstInput").val()) || 0;
    let igstPercent = parseFloat(row.find(".igstInput").val()) || 0;

    let cgstAmt = (baseAmount * cgstPercent) / 100;
    let sgstAmt = (baseAmount * sgstPercent) / 100;
    let igstAmt = (baseAmount * igstPercent) / 100;

    let finalAmount = baseAmount + cgstAmt + sgstAmt + igstAmt;

    row.find(".amountInput").val(finalAmount.toFixed(2));
}
function calculateSubTotals() {
    let total = 0;

    $("#purchaseItemsBody tr").each(function () {
        let amount = parseFloat($(this).find(".amountInput").val()) || 0;
        total += amount;
    });

    $("#txtSubTotal").text(total.toFixed(2));
    $("#txtGrandTotal").text(total.toFixed(2));
}
function generateSubTotalBreakdown() {

    let tbody = $("#subTotalBreakdownTable tbody");
    tbody.html("");

    let sumAmount = 0, sumCGST = 0, sumSGST = 0, sumIGST = 0;

    $("#purchaseItemsBody tr").each(function (i) {
        let row = $(this);

        let name = row.find(".rawMaterialSelect option:selected").text();
        let qty = parseFloat(row.find(".qtyInput").val()) || 0;
        let price = parseFloat(row.find(".priceInput").val()) || 0;

        let baseAmount = qty * price;

        let cgstPercent = parseFloat(row.find(".cgstInput").val()) || 0;
        let sgstPercent = parseFloat(row.find(".sgstInput").val()) || 0;
        let igstPercent = parseFloat(row.find(".igstInput").val()) || 0;

        let cgstAmt = (baseAmount * cgstPercent) / 100;
        let sgstAmt = (baseAmount * sgstPercent) / 100;
        let igstAmt = (baseAmount * igstPercent) / 100;

        let finalAmount = baseAmount + cgstAmt + sgstAmt + igstAmt;

        // Add to table
        tbody.append(`
                <tr>
                    <td>${i + 1}</td>
                    <td>${name}</td>
                    <td>${baseAmount.toFixed(2)}</td>
                    <td>${cgstAmt.toFixed(2)}</td>
                    <td>${sgstAmt.toFixed(2)}</td>
                    <td>${igstAmt.toFixed(2)}</td>
                </tr>
            `);

        // Summation
        sumAmount += baseAmount;
        sumCGST += cgstAmt;
        sumSGST += sgstAmt;
        sumIGST += igstAmt;
    });

    // Fill summary
    $("#sumAmount").text(sumAmount.toFixed(2));
    $("#sumCGST").text(sumCGST.toFixed(2));
    $("#sumSGST").text(sumSGST.toFixed(2));
    $("#sumIGST").text(sumIGST.toFixed(2));

    let grandSub = sumAmount + sumCGST + sumSGST + sumIGST;
    $("#finalSubTotal").text(grandSub.toFixed(2));
}
//<!--SUB TOTAL - MODAL -- END -->

//<!-- MANAGE - child-parent modal close -- START -->
let isChildOpen = false;

/* When child modal opens */
$("#subTotalBreakdownModal").on("show.bs.modal", function () {

    // Hide parent behind
    $("#addEventModal").modal("hide");

    isChildOpen = true;
});

/* When child modal closes */
$("#subTotalBreakdownModal").on("hidden.bs.modal", function () {

    isChildOpen = false;

    // Show parent back
    $("#addEventModal").modal("show");
});

/* ESC LOGIC */
$(document).on("keydown", function (e) {
    if (e.key === "Escape") {

        if (isChildOpen) {
            // Close only child
            $("#subTotalBreakdownModal").modal("hide");
            return;
        }

        // If parent is open & child is NOT
        if ($("#addEventModal").hasClass("show")) {
            $("#addEventModal").modal("hide");
            return;
        }
    }
});

//<!-- MANAGE - child-parent modal close -- END -->
