let rowIndex = 0;

const REPORT_COLUMNS_PURCHASE_INVOICE = [
    { key: "SrNo", label: "Sr No" },
    { key: "Supplier", label: "Supplier" },
    { key: "InvoiceDate", label: "Invoice Date" },
    { key: "InvoiceNo", label: "Invoice No" },
    { key: "PONo", label: "PO No" },
    { key: "Subtotal", label: "SubTotal" },
    { key: "Tax", label: "Tax" },
    { key: "Other", label: "Other Charges" },
    { key: "Discount", label: "Discount" },
    { key: "Total", label: "Total" },
    { key: "Paid", label: "Paid" },
    { key: "Outstanding", label: "Outstanding" },
    { key: "CreatedBy", label: "Created By" },
    { key: "Status", label: "Status" }
];

let selectedColumns = REPORT_COLUMNS_PURCHASE_INVOICE.map(c => c.key);
const HEADER_KEY = "gstCustomerInvoicePrintWithHeader";

let selected_widgetPurchaseDuration = "thismonth";
let selected_widgetOutstandingDuration = "thismonth";

$(document).ready(function (event) {
    setCurrentMonthDates();

    //DROPDOWNS
    loadStateDropdown();
    loadSupplierDropdown();
    loadRawMaterialsDropdown();
    loadUnitDropdown();
    loadPaymentModeDropdown();
    loadAccountDropdown();

    $("#purchaseFilter").data("value", "thismonth");
    loadWidgetsData();
    fetchData();

    AppSignalR.subscribePurchase((data) => {
        //console.log("SignalR: Purchase Updated");
        loadWidgetsData();
        fetchData();
    });

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
            $("#addEventModalTitle").text("Add Purchase Invoice");
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
        let purchaseId = rowData.Id;

        var payload = {
            PurchaseId: purchaseId,
            SupplierId: 0,
            InvoiceNo: null
        };
        console.log("edit payload:", payload);

        rowData = null;
        $.ajax({
            url: "/api/Purchase/GetPurchaseList",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                console.log("edit response:", response);
                let data = response?.data ?? [];
                if (!data.length) {
                    Swal.fire("Error", "Data not found", "error");
                    return;
                }
                rowData = data[0]; // single record
                console.log("rowData edit:", rowData);
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
                $("#paymentAccountId").val(rowData.PaymentAccountId || 0);
                $("#paymentRefNo").val(rowData.PaymentRefNo || "");
                $("#paymentRemarks").val(rowData.PaymentRemarks || "");

                // NOW trigger change once everything is set
                $("input[name='paymentType']:checked").trigger("change");

                // =========================================
                // RELOAD MATERIAL ROWS
                // =========================================
                $("#purchaseItemsBody").empty();
                rowIndex = 0;
                let selected_rows = 0;

                if (rowData.JSON_PurchaseItems && rowData.JSON_PurchaseItems.length > 0) {

                    rowData.JSON_PurchaseItems.forEach(item => {
                        rowIndex++;
                        //console.log("row:", rowIndex);
                        if (item.IsSelected) selected_rows++;

                        let newRow = buildRowForEdit(item);
                        $("#purchaseItemsBody").append(newRow);
                    });
                }
                else {
                    addInitialRow();
                }

                //console.log("rowIndex:", rowIndex, ", selected_rows:", selected_rows);
                // ✅ Correct master checkbox logic
                $("#selectAllItems").prop("checked", rowIndex > 0 && rowIndex === selected_rows);

                updateSrNo();
                calculateSubTotals();

                // =========================================
                // OPEN MODAL
                // =========================================
                $("#addEventModalTitle").text("Edit Purchase Invoice");
                $("#addEventModal").modal("show");
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Fetch Purchase for Edit failed");
            }
        });
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
                    PurchaseId: Number($("#purchaseId").val()),
                    RawMaterialId: Number(row.find(".rawMaterialSelect").val()),
                    Qty: parseFloat(row.find(".qtyInput").val()) || 0,
                    UnitId: Number(row.find(".unitSelect").val()),
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
                Id: Number(purchaseId),
                PurchaseFrom: 1,
                SupplierId: Number(supplierId),
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

                GrandTotal_Amount: Number(grandTotal),

                PaymentType: paymentType,
                PaymentDate: paymentType === 2 ? paymentDate : null,
                PaidAmount: paymentType === 2 ? Number(paidAmount) : null,
                PaymentModeId: paymentType === 2 ? Number(paymentModeId) : null,
                PaymentRefNo: paymentType === 2 ? paymentRefNo : null,
                PaymentAccountId: paymentType === 2 ? Number($("#paymentAccountId").val()) : null,
                PaymentRemarks: paymentType === 2 ? $("#paymentRemarks").val() : null,

                ChequeBank: paymentType === 2 ? $("#chequeBank").val() : null,
                ChequeNo: paymentType === 2 ? $("#chequeNo").val() : null,
                ChequeDate: paymentType === 2 ? $("#chequeDate").val() : null,

                UploadInvoiceFile: base64File, // NULL or base64

                IsUpdateInventoryStock: isUpdateInventory,
                JSON_PurchaseItems: itemsList,
                IsActive: true,
                ActionBy: Number(sessionStorage.getItem("userId") || 0)
            };

            console.log("PAYLOAD:", payload);

            $.ajax({
                url: "/api/Purchase/SavePurchase",
                type: "POST",
                data: JSON.stringify(payload),
                contentType: "application/json",
                success: function (response) {
                    if (response.status.toLowerCase() == "success") {
                        handleApiSuccess("Saved!");
                        // ✅ Swal Confirmation
                        Swal.fire({
                            title: 'Download Purchase Receipt?',
                            text: 'Do you want to download the payment receipt now?',
                            icon: 'question',
                            showCancelButton: true,
                            confirmButtonText: 'Yes, Download',
                            cancelButtonText: 'No',
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33'
                        }).then((result) => {

                            if (result.isConfirmed) {
                                purchaseId = response.data.PurchaseId;
                                downloadPurchaseInvoice(purchaseId, invoiceNo);
                            }
                            // ❌ If NO → do nothing (flow continues)

                            // 🔁 Reload payment list if needed
                            resetFormFields(); // Reset form fields when modal is shown
                            loadRawMaterialsDropdown();

                            //fetchData();
                            //loadWidgetsData();
                        });
                        $("#addEventModal").modal("hide");
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

                if (response.status === "Success") {
                    handleApiSuccess("Deleted!");
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: response.message,
                        icon: "error"
                    });
                }

                loadRawMaterialsDropdown();
                //fetchData();
                //loadWidgetsData();
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Purchase failed");
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
        const $this = $(this);
        updateUnitsDropdown(this);

        let currentQty = $this.find("option:selected").data("currentqty") ?? 0;

        let td = $this.closest("td");
        let stockEl = td.find(".stockValue");
        let qtyInput = td.closest("tr").find(".qtyInput");
        // Update stock text + color
        stockEl.text(currentQty)
            .toggleClass("text-danger", currentQty <= 0)
            .toggleClass("text-success", currentQty > 0);
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
        if (!payload.PaymentDate) {
            Swal.fire("Warning", "Payment Date is required", "warning");
            $("#outPaymentDate").focus();
            return false;
        }

        if (payload.PaymentModeId <= 0) {
            Swal.fire("Warning", "Please select Payment Mode", "warning");
            $("#outPaymentModeId").focus();
            return false;
        }

        if (payload.AccountId <= 0) {
            Swal.fire("Warning", "Please select Account", "warning");
            $("#outAccountId").focus();
            return false;
        }

        /* ================= AMOUNT VALIDATION ================= */

        if (!payload.PaidAmount || isNaN(payload.PaidAmount) || payload.PaidAmount <= 0) {
            Swal.fire("Warning", "Paid amount must be greater than zero", "warning");
            $("#outPaidAmount").focus();
            return false;
        }

        if (payload.PaidAmount > remaining) {
            Swal.fire(
                "Invalid Amount",
                `Paid amount cannot exceed remaining amount (₹ ${remaining.toLocaleString("en-IN")})`,
                "warning"
            );
            $("#outPaidAmount").focus();
            return false;
        }
        $.ajax({
            url: "/api/Purchase/SavePurchaseOutstandingPayment",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess(res.message)
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

    $(document).on("click", ".btn-download-invoice", function () {
        const purchaseId = $(this).data("id");
        const invoiceNo = $(this).data("invoice");
        downloadPurchaseInvoice(purchaseId, invoiceNo);
    });

    $(document).on("click", ".btn-download-scannedinvoice", function () {
        const purchaseId = $(this).data("id");
        const invoiceNo = $(this).data("invoice");
        downloadPurchaseScannedInvoice(purchaseId, invoiceNo);
    });

    function initToggle({ containerId, toggleId, labelId, storageKey, showText, hideText, messageId = null }) {
        const container = $(containerId);
        const toggle = $(toggleId);
        const label = $(labelId);
        const message = messageId ? $(messageId) : null; // ✅ FIX

        function updateLabel(isVisible) {
            label.text(isVisible ? hideText : showText);
        }

        const isHidden = localStorage.getItem(storageKey);
        const isVisible = isHidden !== "true";

        container.toggle(isVisible);

        if (message) {
            message.toggle(!isVisible); // ✅ safe
        }

        toggle.prop("checked", isVisible);
        updateLabel(isVisible);

        toggle.off("change").on("change", function () {
            const visible = $(this).is(":checked");

            if (visible) {
                container.stop(true, true).slideDown(200);
                if (message) message.hide(); // ✅ safe
            } else {
                container.stop(true, true).slideUp(200);
                if (message) message.show(); // ✅ safe
            }

            localStorage.setItem(storageKey, visible ? "false" : "true");
            updateLabel(visible);
        });
    }
    //Widgets hide/show -- START --------------
    initToggle({
        containerId: "#purchaseWidgets",
        toggleId: "#toggleWidgets",
        labelId: "#widgetLabel",
        storageKey: "hidepurchaseWidgets",
        showText: "Show Widgets",
        hideText: "Hide Widgets"
    });
    //Widgets hide/show -- END --------------

});

function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    $("#fltInvoiceFrom").val(formatDate2(firstDay));
    $("#fltInvoiceTo").val(formatDate2(lastDay));
}

let selectedMonthDuration2 = "thismonth"; // ✅ default
$(".filter-option2").on("click", function (e) {
    e.preventDefault();
    let newFilter = $(this).data("value");

    // agar same filter, kuch mat karo
    if (selectedMonthDuration2 === newFilter) return;
    selectedMonthDuration2 = newFilter;

    // dropdown active class manage
    $(".filter-option2").removeClass("active");
    $(this).addClass("active");

    $("#purchaseFilter").text("Filter : " + $(this).text());
    //$(".sub_count").text("In " + $(this).text());

    // --------------------------
    // MonthDuration ke hisab se From/To date set karna
    // --------------------------
    let today = new Date();
    let fromDate, toDate;
    $("#customDateRange2").slideUp(150);

    if (selectedMonthDuration2 === "thismonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    }
    else if (selectedMonthDuration2 === "lastmonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        toDate = new Date(today.getFullYear(), today.getMonth(), 0);
    }
    else if (selectedMonthDuration2 === "last7days") { // Last 7 days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 6);
        toDate = today;
    }
    else if (selectedMonthDuration2 === "last30days") { // Last 30 days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 29);
        toDate = today;
    }
    else if (selectedMonthDuration2 === "yesterday") {
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 1);
        toDate = fromDate;
    }
    else if (selectedMonthDuration2 === "last12months") {
        fromDate = new Date(today);
        fromDate.setMonth(today.getMonth() - 12);
        toDate = today;
    }
    else if (selectedMonthDuration2 === "customrange") {
        $("#customDateRange2").slideDown(150);
    }

    $("#fltInvoiceFrom2").val(formatDate2(fromDate));
    $("#fltInvoiceTo2").val(formatDate2(toDate));

    if (selectedMonthDuration2 === "customrange") {
    }
    else {
        loadWidgetsData();
    }

});

let selectedMonthDuration = "thismonth"; // ✅ default
$(".sort-option").on("click", function (e) {
    e.preventDefault();
    let newFilter = $(this).data("value");

    // agar same filter, kuch mat karo
    if (selectedMonthDuration === newFilter) return;
    selectedMonthDuration = newFilter;

    // dropdown active class manage
    $(".sort-option").removeClass("active");
    $(this).addClass("active");

    $("#sortByDropdown").text("Filter : " + $(this).text());
    //$(".sub_count").text("In " + $(this).text());

    // --------------------------
    // MonthDuration ke hisab se From/To date set karna
    // --------------------------
    let today = new Date();
    let fromDate, toDate;

    if (selectedMonthDuration === "thismonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    }
    else if (selectedMonthDuration === "lastmonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        toDate = new Date(today.getFullYear(), today.getMonth(), 0);
    }
    else if (selectedMonthDuration === "last7days") { // Last 7 days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 6);
        toDate = today;
    }
    else if (selectedMonthDuration === "last30days") { // Last 30 days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 29);
        toDate = today;
    }
    else if (selectedMonthDuration === "yesterday") {
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 1);
        toDate = fromDate;
    }
    else if (selectedMonthDuration === "last12months") {
        fromDate = new Date(today);
        fromDate.setMonth(today.getMonth() - 12);
        toDate = today;
    }
    else if (selectedMonthDuration === "customrange") {
        // custom range me datepicker pe user select karega
        // return loadDashboardWidgetCounts();
    }
    // Store selected filter in session
    //sessionStorage.setItem("SelectedFilter_DashboardWidget", selectedMonthDuration);    // Set the datepickers

    $("#fltInvoiceFrom").val(formatDate2(fromDate));
    $("#fltInvoiceTo").val(formatDate2(toDate));
});

$("#fltInvoiceFrom2, #fltInvoiceTo2").on("change", function () {

    let fromDate = $("#fltInvoiceFrom2").val();
    let toDate = $("#fltInvoiceTo2").val();

    if (fromDate && toDate) {
        selectedMonthDuration2 = "customrange";
        //$("#purchaseFilter").text("Filter : Custom Range");
        //$(".sub").text("In Custom Range");
        $(".filter-option2").removeClass("active");
        loadWidgetsData();
    }
});

$("#fltInvoiceFrom, #fltInvoiceTo").on("change", function () {

    let fromDate = $("#fltInvoiceFrom").val();
    let toDate = $("#fltInvoiceTo").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        //$(".sub").text("In Custom Range");

        $(".sort-option").removeClass("active");
        // loadDashboardWidgetCounts(); // 🔥
    }
});

function resetFilter() {
    // 🔹 TEXT INPUTS
    $("#fltInvoiceNo, #fltPONo, #fltSearch").val("");

    // 🔹 DATE INPUTS
    $("#fltInvoiceFrom").val("");
    $("#fltInvoiceTo").val("");

    // 🔹 DROPDOWNS (SELECT)
    $("#fltSupplier").val(0).trigger("change"); // for select2 safe
    $("#fltPaymentStatus").val("0");

    // 🔹 FILTER DROPDOWN RESET
    selectedMonthDuration = "thismonth";
    $("#sortByDropdown").text("Filter : This Month");
    $(".sort-option").removeClass("active");
    $('.sort-option[data-value="thismonth"]').addClass("active");
    setCurrentMonthDates();

    // 🔹 CUSTOM DATE RANGE HIDE (if you add later)
    $("#customDateRange").hide();

    // 🔹 RELOAD DATA
    fetchData();
}
//Widgets  -- START ------------------------------------------------------------
function loadWidgetsData() {
    var payload = {
        EventFrom: $("#fltInvoiceFrom2").val() || "",
        EventTo: $("#fltInvoiceTo2").val() || "",
        FilterDuration: selectedMonthDuration2 //$("#purchaseFilter").data("value") || "thismonth"
    };
    //console.log("widget payload:", payload);
    $.ajax({
        url: "/api/Purchase/GetPurchaseList_Widgets",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("widget res:", res);
            if (res?.status?.toLowerCase() === "success" && res.data) {
                let d = res.data;
                $("#widget_total_purchase").text(`₹ ${d.TotalAmount ?? 0}`);
                $("#widget_outstanding").text(`₹ ${d.TotalOutstandingAmount ?? 0}`);
                $("#widget_total_orders").text(d.TotalOrders ?? 0);

                // 🔴 Highlight outstanding if > 0
                $("#widget_outstanding")
                    .removeClass("text-danger text-dark")
                    .addClass((d.TotalOutstandingAmount ?? 0) > 0 ? "text-danger fw-bold" : "text-dark");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Purchase Widgets Fetch Error");
        }
    });
}
//Widgets  -- END --------------------------------------------------------------

function loadColumnCheckboxes() {
    let html = "";

    REPORT_COLUMNS_PURCHASE_INVOICE.forEach(col => {
        const isChecked = selectedColumns.includes(col.key);
        html += `
        <div class="col-md-4 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">${col.label}</label>
            </div>
        </div>`;
    });

    $("#column-list").html(html);
    updateSelectAllState();
}

function updateSelectAllState() {
    const total = $(".column-checkbox").length;
    const checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}

/* ── With Header toggle ── */
const saved = sessionStorage.getItem(HEADER_KEY);
if (saved !== null)
    document.getElementById("toggleWithHeader").checked = (saved === "true");

document.getElementById("toggleWithHeader").addEventListener("change", function () {
    sessionStorage.setItem(HEADER_KEY, this.checked);
});

/* ── Column selector ── */
$("#btn-select-columns").on("click", function () {
    loadColumnCheckboxes();
    $("#modalColumns").modal("show");
});

$("#chk-select-all").on("change", function () {
    $(".column-checkbox").prop("checked", this.checked);
});


$("#btn-reset-columns").on("click", function () {
    $(".column-checkbox").prop("checked", true);
    $("#chk-select-all").prop("checked", true);
});

$("#btn-save-columns").on("click", function () {
    selectedColumns = [];
    $(".column-checkbox:checked").each(function () {
        selectedColumns.push($(this).val());
    });
    $("#modalColumns").modal("hide");
    //fetchData();
});


/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewPurchaseInvoiceReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    var payload = {
        PONo: $("#fltPONo").val() || "",
        InvoiceNo: $("#fltInvoiceNo").val() || "",
        FilterSearchText: $("#fltSearch").val() || "",
        SupplierId: parseInt($("#fltSupplier").val() || 0),
        EventFrom: $("#fltInvoiceFrom").val() || "",
        EventTo: $("#fltInvoiceTo").val() || "",
        FilterDuration: selectedMonthDuration,
        StatusId: parseInt($("#fltPaymentStatus").val() || 0),
        // ⚙️ Common
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    // ✅ API URL
    const url = actionName === "downloadExcel"
        ? "/api/Purchase/DownloadPurchaseList_GRID_Excel"
        : "/api/Purchase/DownloadPurchaseList_GRID_Pdf";

    downloadReport(url, payload, loaderMsg = "Generating Report..");
}


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
    let rawMaterial_Html_dropdown = `
        <div class="raw-material-wrapper">
            <select class="form-select form-select-sm rawMaterialSelect">
                <option value="0" data-currentqty="0">--Select Raw Material--</option>
                ${rawMaterials.map(x => `
                    <option value="${x.Id}" data-currentqty="${x.CurrentQty ?? 0}">
                        ${x.Name}
                    </option>
                `).join("")}
            </select>

            <div class="stockHint text-muted small">
                Current Stock: <span class="stockValue">0</span>
            </div>
        </div>
    `;

    //UNITS DROPDOWN
    //console.log("unitMaster:", unitMaster);
    const unit_Html_dropdown = `
        <select class="form-select form-select-sm unitSelect">
            <option value="0">-- Select --</option>
            ${unitMaster.map(x => `
                <option value="${x.Id}">
                    ${x.UnitName}
                </option>
            `).join("")}
        </select>
    `;
    //console.log("unit_Html_dropdown:", unit_Html_dropdown);

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
                    <input type="number" class="form-control form-control-sm qtyInput mb-2" min="0" step="0.1" value="0">
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

                <!-- CGST+SGST Tax % -->
                <td>
                    <input type="number" class="form-control form-control-sm cgstInput_sgstInput" placeholder="CGST + SGST %"  min="0" max="100" step="0.01">
                    <input type="hidden" class="form-control form-control-sm cgstInput" placeholder="CGST %"  min="0" step="0.01">
                    <input type="hidden" class="form-control form-control-sm sgstInput" placeholder="SGST %"  min="0" step="0.01">
                    <div class="cgstSgstHint text-muted small"><span class="cgstSgstValue">0</span></div>
                </td>

                <!-- IGST Tax % -->
                <td>
                    <input type="number" class="form-control form-control-sm igstInput" placeholder="IGST %"  min="0" max="100" step="0.01">
                    <div class="igstHint text-muted small"><span class="igstValue">0</span></div>
                </td>

                <!-- Amount -->
                <td>
                    <input type="number" class="form-control form-control-sm amountInput"  min="0" step="0.01" value="0">
                </td>

                <!-- Actions -->
                <td>
                    <button class="btn btn-sm btn-outline-danger btnDeleteRow"><i class="fa fa-trash"></i></button>
                </td>

            </tr>
        `);

    $("#purchaseItemsBody").append(newRow);
    updateSrNo();

    newRow.find(".rawMaterialSelect option").each(function (index) {
        if (index === 0) return;

        let rm = rawMaterials[index - 1];
        $(this).data("units", rm.JSON_PurchaseUnits || []);
    });

    //attachRawMaterialEvents(newRow);
}

// Attach events to RAW MATERIAL dropdown & enable fields
function attachRawMaterialEvents(row) {
    //console.log("here2");
    let rawSelect = row.find(".rawMaterialSelect");
    rawSelect.off("change").on("change", function () {
        // Load units for selected raw material
        updateUnitsDropdown(this);
    });
}

function updateUnitsDropdown2(rawMaterialSelect) {
    let selectedOption = $(rawMaterialSelect).find("option:selected");

    // ✅ Get JSON safely
    let unitsJson = selectedOption.data("units") || [];
    //console.log("unitsJson:", unitsJson);

    let row = $(rawMaterialSelect).closest("tr");
    let unitDropdown = row.find(".unitSelect");

    unitDropdown.html(`<option value="0">--Select--</option>`);

    // ✅ CASE 1: No units → show all
    // ✅ Check if "ALL units" condition
    let isAllUnits = true;
        !unitsJson.length ||
        (unitsJson.length === 1 && unitsJson[0].UnitId === 0);

    // ✅ CASE: Show ALL units
    if (isAllUnits) {
        unitMaster.forEach(u => {
            unitDropdown.append(
                `<option value="${u.Id}">${u.UnitName}</option>`
            );
        });
        return;
    }

    // ✅ CASE 2: Filtered units
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
function updateUnitsDropdown(rawMaterialSelect) {
    let selectedOption = $(rawMaterialSelect).find("option:selected");

    // ✅ Get JSON safely
    let unitsJson = selectedOption.data("units") || [];

    let row = $(rawMaterialSelect).closest("tr");
    let unitDropdown = row.find(".unitSelect");

    // Reset dropdown
    unitDropdown.html(`<option value="0">--Select--</option>`);

    // ✅ Always show ALL units
    unitMaster.forEach(u => {
        unitDropdown.append(
            `<option value="${u.Id}">${u.UnitName}</option>`
        );
    });

    // ✅ Check if "ALL units" condition
    let isAllUnits =
        !unitsJson.length ||
        (unitsJson.length === 1 && unitsJson[0].UnitId === 0);

    // 👉 If ALL units → keep default (0 selected)
    if (isAllUnits) {
        unitDropdown.val("0");
        return;
    }

    // ✅ Otherwise → select first matching unit
    let firstMatch = unitsJson[0]?.UnitId;

    if (firstMatch) {
        unitDropdown.val(firstMatch);
    }
}


function buildRowForEdit(item) {
    console.log("item:", item);
    //rowIndex++;

    let rawMaterialId = item.RawMaterialId;
    let qty = item.Qty;
    let unit = item.UnitId;
    let price = item.Price;
    let amount = item.Amount;

    let cgst = item.CGST;
    let sgst = item.SGST;

    let cgst_amt = item.CGST_Amount;
    let sgst_amt = item.SGST_Amount;

    let igst = item.IGST;
    let igst_amt = item.IGST_Amount;

    let cgst_sgst = cgst + sgst;
    let cgst_sgst_amt = cgst_amt + sgst_amt;

    let isChecked = item.IsSelected ? "checked" : "";

    // ----------------------------------------
    // 1️⃣ Populate RAW MATERIAL dropdown same as ADD
    // ----------------------------------------
    //RAW MATERIALS DROPDOWN
    let rawMaterial_Html_dropdown = `
        <div class="raw-material-wrapper">
            <select class="form-select form-select-sm rawMaterialSelect">
                <option value="0" data-currentqty="0">--Select Raw Material--</option>
                ${rawMaterials.map(x => `
                    <option value="${x.Id}" data-currentqty="${x.CurrentQty ?? 0}">
                        ${x.Name}
                    </option>
                `).join("")}
            </select>

            <div class="stockHint text-muted small">
                Current Stock: <span class="stockValue">0</span>
            </div>
        </div>
    `;

    //UNITS DROPDOWN
    //console.log("unitMaster:", unitMaster);
    const unit_Html_dropdown = `
        <select class="form-select form-select-sm unitSelect">
            <option value="0">-- Select --</option>
            ${unitMaster.map(x => `
                <option value="${x.Id}">
                    ${x.UnitName}
                </option>
            `).join("")}
        </select>
    `;
    //console.log("unit_Html_dropdown:", unit_Html_dropdown);

    let row = $(`
            <tr data-index="${rowIndex}">
                <td class="text-center align-middle"><input type="checkbox" class="row-check" ${isChecked}></td>
                <td class="text-center align-middle sr-no">${rowIndex}</td>

                <!-- Raw Material -->
                <td>
                    ${rawMaterial_Html_dropdown}
                </td>

                <!-- Qty -->
                <td>
                    <input type="number" class="form-control form-control-sm qtyInput" min="0" step="0.1" value="${qty}">
                </td>

                <!-- Unit -->
                <td>
                    ${unit_Html_dropdown}
                </td>

                <!-- ORDER QTY -->
                <td class="orderQtyLabel fw-bold">0</td>

                <!-- PRICE -->
                <td>
                    <input type="number" class="form-control form-control-sm priceInput" min="0" step="0.01" value="${price}">
                </td>

                <!-- CGST + SGST TAX -->
                <td>
                    <input type="number" class="form-control form-control-sm cgstInput_sgstInput" placeholder="CGST + SGST %" value="${cgst_sgst}" min="0" max="100" step="0.01">
                    <input type="hidden" class="form-control form-control-sm cgstInput" placeholder="CGST %" value="${cgst}" min="0" max="100" step="0.01">
                    <input type="hidden" class="form-control form-control-sm sgstInput" placeholder="SGST %" value="${sgst}" min="0" max="100" step="0.01">
                    <div class="cgstSgstHint text-muted small"><span class="cgstSgstValue">₹ ${cgst_sgst_amt}</span></div>
                </td>

                <!-- IGST TAX -->
                <td>
                    <input type="number" class="form-control form-control-sm igstInput" placeholder="IGST %" value="${igst}" min="0" max="100" step="0.01">
                    <div class="igstHint text-muted small"><span class="igstValue">₹ ${igst_amt}</span></div>
                </td>

                <!-- AMOUNT -->
                <td>
                    <input type="number" class="form-control form-control-sm amountInput" min="0" step="0.01" value="${amount}">
                </td>

                <!-- ACTION -->
                <td>
                    <button class="btn btn-sm btn-outline-primary btnEditRow d-none"><i class="fa fa-pen"></i></button>
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

//--------FETCH & SHOW GRID DATA -- START --------
function fetchData() {

    var payload = {
        PONo: $("#fltPONo").val() || "",
        InvoiceNo: $("#fltInvoiceNo").val() || "",
        FilterSearchText: $("#fltSearch").val() || "",
        SupplierId: parseInt($("#fltSupplier").val() || 0),
        EventFrom: $("#fltInvoiceFrom").val() || "",
        EventTo: $("#fltInvoiceTo").val() || "",
        FilterDuration: selectedMonthDuration,
        StatusId: parseInt($("#fltPaymentStatus").val() || 0)
    };
    //console.log("payload:", payload);
    $.ajax({
        url: "/api/Purchase/GetPurchaseList_GRID",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("fetch response:", response);
            let list = response?.data?.List ?? [];
            let totals = response?.data?.Totals ?? {};

            renderTable(list);
            bindTotals(totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Purchase List Fetch failed");
        }
    });
}
//SHOW GRID TABLE
function renderTable(data) {
    console.log("render data:", data);

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {
        $('#tableMain table').DataTable({
            data: [],
            columns: getColumns(),
            initComplete: function () {
                $(this.api().table().header())
                    .find('th')
                    .addClass('bg-warning-subtle text-dark');
            }
        });
        return;
    }

    // Add SR No
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({
        data: data,
        columns: getColumns(),
        paging: false,          // 🔥 disable pagination
        ordering: true,
        searching: true,
        lengthChange: false,
        info: false,            // optional: hide "Showing X entries"
        stateSave: false,
        rowCallback: function (row, data) {

            let total = data.TotalAmount ?? 0;
            let paid = data.PaidAmount ?? 0;
            let remain = data.OutstandingAmount ?? 0;
            let isDeleted = data.IsDeleted ?? false;
            $(row).removeClass("row-green row-orange row-red row-deleted");

            if (Math.abs(remain) < 0.01 && paid > 0 && !isDeleted) {
                // ✅ FULL PAID
                $(row).addClass("row-green");
            }
            else if (paid === 0 && !isDeleted) {
                // ❌ NOT PAID
                $(row).addClass("row-red");
            }
            else if (remain > 0 && !isDeleted) {
                // ⚠️ PARTIAL
                $(row).addClass("row-orange");
            }
            else if (isDeleted) {
                // ⚠️ DELETED
                $(row).addClass("row-deleted");
            }
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        }
    });
    function getColumns() {
        return [
            { data: "srno", title: "#" },
            { data: "SupplierName", title: "Supplier" },
            //{ data: "InvoiceDate", title: "Invoice Date" },
            {
                data: 'InvoiceDate', // actual date (ISO format from backend: yyyy-MM-dd)
                title: 'Invoice Date',
                name: 'InvoiceDate',
                render: function (data, type, row) {

                    if (type === 'display') {
                        return row.InvoiceDate_Format; // show formatted
                    }

                    return data; // use raw date for sorting
                }
            },
            {
                data: "InvoiceNo",
                title: "Invoice No",
                className: "text-center",
                render: d => d ? `#${d}` : "-"
            },
            {
                data: "PONo",
                title: "PO No",
                className: "text-center",
                render: d => d ? `#${d}` : "-"
            },

            {
                data: "SubTotalAmount",
                title: "SubTotal",
                className: "text-end",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: "Tax",
                title: "Tax",
                className: "text-end",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: "OtherCharges",
                title: "Other Charges",
                className: "text-end",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: "Discount",
                title: "Discount",
                className: "text-end",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: "TotalAmount",
                title: "Total",
                className: "text-end fw-bold",
                render: x => `₹ ${x ?? 0}`
            },

            {
                data: "PaidAmount",
                title: "Paid",
                className: "text-end text-success",
                render: x => `₹ ${x ?? 0}`
            },
            {
                data: null,
                title: "Outstanding",
                className: "text-end",
                render: (d) => {
                    let remain = d.OutstandingAmount ?? 0;
                    if (remain > 0) {
                        return `
                            <button class="btn btn-sm btn-action-xss btn-outline-danger btn-remaining"
                                    data-id="${d.Id}"
                                    data-pono="${d.PONo ?? ''}">
                                ₹ ${remain}
                            </button>
                        `;
                    }
                    // zero / no remaining → plain text
                    return `<span class="text-mutedd">₹ ${remain}</span>`;
                }
            },

            {
                data: null,
                title: "Created By",
                render: (d) => `
                        <small>${d.CreatedBy ?? "-"}</small><br>
                        <small class="text-mutedd">Created: ${d.CreatedOn}</small><br>
                        ${d.ModifiedOn ? `<small class="text-mutedd">Last Modified: ${d.ModifiedOn ?? ""}</small>` : ""}                        
                    `
            },
            {
                data: "PaymentStatus",
                title: "Status",
                className: "text-center",
                render: (status) => {

                    let cls = "";

                    switch (status) {
                        case "Full Paid":
                            cls = "bg-success text-white";   // Green + White
                            break;

                        case "Partial Paid":
                            cls = "bg-warning text-white";    // Orange + Black
                            break;

                        case "Pending":
                            cls = "bg-danger text-white";    // Red + White
                            break;

                        case "Deleted":
                            cls = "bg-secondary text-white";  // Gray + Black
                            break;

                        default:
                            cls = "bg-light text-dark";
                    }

                    return `<span class="badge ${cls}">${status}</span>`;
                }
            },
            {
                data: null,
                title: "Action",
                className: "text-center",
                render: function (data, type, row) {
                    let scannedInvoiceHtml = '';

                    if (row.IsUploadedScannedInvoice) {
                        scannedInvoiceHtml = `
                            <li>
                                <a class="dropdown-item btn-download-scannedinvoice cursor-pointer" 
                                   data-id="${row.Id}"
                                   data-invoice="${row.InvoiceNo}">
                                    <i class="fa fa-download text-info me-2"></i> Scanned Invoice
                                </a>
                            </li>
                        `;
                    }

                    return `
                        <div class="dropdown">
                            <button class="btn btn-outline-secondary btn-sm dropdown-toggle btn-action-xss"
                                type="button" data-bs-toggle="dropdown">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>

                            <ul class="dropdown-menu">

                                <li>
                                    <a class="dropdown-item btn-download-invoice cursor-pointer" 
                                       data-id="${row.Id}"
                                       data-invoice="${row.InvoiceNo}">
                                        <i class="fa fa-download text-primary me-2"></i> Invoice
                                    </a>
                                </li>

                                ${scannedInvoiceHtml}   <!-- ✅ Conditional -->

                                <li>
                                    <a class="dropdown-item btn-edit cursor-pointer" data-id="${row.Id}">
                                        <i class="fa fa-edit text-warning me-2"></i> Edit
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-delete cursor-pointer" data-id="${row.Id}">
                                        <i class="fa fa-trash text-danger me-2"></i> Delete
                                    </a>
                                </li>

                            </ul>
                        </div>
                    `;
                }
            }
        ];
    }

}
function bindTotals(t) {

    $("#ftSubTotalAmount").html(`₹ ${t.TotalSubTotalAmount ?? 0}`);
    $("#ftTaxAmount").html(`₹ ${t.TotalTax ?? 0}`);
    $("#ftChargesAmount").html(`₹ ${t.TotalOtherCharges ?? 0}`);
    $("#ftDiscountAmount").html(`₹ ${t.TotalDiscount ?? 0}`);
    $("#ftTotalAmount").html(`₹ ${t.TotalAmount ?? 0}`);
    $("#ftPaidAmount").html(`₹ ${t.TotalPaidAmount ?? 0}`);

    const val = t.TotalOutstandingAmount ?? 0;
    $("#ftOutstandingAmount")
        .html(`₹ ${val}`)
        .removeClass("text-danger text-dark")
        .addClass(val > 0 ? "text-danger fw-bold" : "text-dark");
}

//--------FETCH & SHOW GRID DATA -- END --------


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

        let paymentDate = $("#paymentDate").val();
        let paidAmount = parseFloat($("#paidAmount").val());
        let paymentModeId = parseInt($("#paymentModeId").val()) || 0;
        let paymentAccountId = parseInt($("#paymentAccountId").val()) || 0;

        // Payment Date
        if (!paymentDate) {
            Swal.fire("Validation", "Payment Date is required", "warning");
            $("#paymentDate").focus();
            return false;
        }

        // Paid Amount
        if (!paidAmount || isNaN(paidAmount) || paidAmount <= 0) {
            Swal.fire("Validation", "Paid Amount must be greater than 0", "warning");
            $("#paidAmount").focus();
            return false;
        }

        // Payment Mode
        if (paymentModeId <= 0) {
            Swal.fire("Validation", "Select a Payment Mode", "warning");
            $("#paymentModeId").focus();
            return false;
        }

        // Payment Account
        if (paymentAccountId <= 0) {
            Swal.fire("Validation", "Select a Payment Account", "warning");
            $("#paymentAccountId").focus();
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
            $("#purchaseSupplier, #fltSupplier").html(`<option value="0">-- Select Supplier --</option>`);
            response.data.forEach(s => {
                $("#purchaseSupplier, #fltSupplier").append(
                    `<option value="${s.Id}">${s.CompanyName && s.CompanyName.trim() !== "" ? s.CompanyName : s.Name}</option>`
                );
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
            console.log("response rawMaterials:", response);
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
    //console.log("grandTotal:", grandTotal);
    $("#txtGrandTotal").text(grandTotal.toFixed(2));
}

$("#discountValue, #deliveryCharge, #deliveryCGST, #deliverySGST, #tcsPercent, #tdsPercent").on("input", function () {
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
$(document).on("input change", ".qtyInput, .priceInput, .unitSelect, .cgstInput_sgstInput, .igstInput", function () {

    let row = $(this).closest("tr");

    // --- Get values safely ---
    let qty = parseFloat(row.find(".qtyInput").val()) || 0;
    let price = parseFloat(row.find(".priceInput").val()) || 0;
    let unitText = row.find(".unitSelect option:selected").text().trim();

    // Order Qty = Qty + Unit text (just display purpose)
    let qtyFormatted = formatQty(qty);
    row.find(".orderQtyLabel").text(`${qtyFormatted} ${unitText}`);

    // --- Amount ---
    let amount = qty * price;
    row.find(".amountInput").val(amount.toFixed(2));

    // --- Handle CGST + SGST ---
    if ($(this).hasClass("cgstInput_sgstInput")) {
        let totalTax = parseFloat($(this).val()) || 0;

        if (totalTax < 0) totalTax = 0;
        if (totalTax > 100) totalTax = 100;

        let half = totalTax / 2;

        row.find(".cgstInput").val(half.toFixed(2));
        row.find(".sgstInput").val(half.toFixed(2));

        // --- GST Amount ---
        let gstAmount = amount * totalTax / 100;
        //row.find(".cgstSgstValue").html(`CGST: ${(gstAmount / 2).toFixed(2)} + SGST: ${(gstAmount / 2).toFixed(2)}`);
        row.find(".cgstSgstValue").html(`₹ ${(gstAmount).toFixed(2)}`);

        // clear IGST
        row.find(".igstInput").val("");
        // clear IGST hint
        row.find(".igstValue").html("0");
    }

    // --- Handle IGST ---
    if ($(this).hasClass("igstInput")) {
        let igst = parseFloat($(this).val()) || 0;

        if (igst < 0) igst = 0;
        if (igst > 100) igst = 100;

        // clear CGST/SGST
        row.find(".cgstInput").val(0);
        row.find(".sgstInput").val(0);
        row.find(".cgstInput_sgstInput").val("");

        // --- GST Amount ---
        let igstAmount = amount * igst / 100;
        row.find(".igstValue").html(`₹ ${igstAmount.toFixed(2)}`);
        // clear CGST/SGST hint
        row.find(".cgstSgstValue").html("0");
    }

    // --- Get final tax values ---
    let cgst = parseFloat(row.find(".cgstInput").val()) || 0;
    let sgst = parseFloat(row.find(".sgstInput").val()) || 0;
    let igst = parseFloat(row.find(".igstInput").val()) || 0;

    // --- Calculate tax ---
    let taxPercent = igst > 0 ? igst : (cgst + sgst);
    let taxAmount = amount * taxPercent / 100;

    // --- Final total ---
    let total = amount + taxAmount;

    row.find(".totalInput").val(total.toFixed(2));

    calculateRowTotal(row);
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
    calculateSubTotals();
}
function calculateSubTotals() {
    let total = 0;

    $("#purchaseItemsBody tr").each(function () {
        let amount = parseFloat($(this).find(".amountInput").val()) || 0;
        total += amount;
    });

    $("#txtSubTotal").text(total.toFixed(2));
    //$("#txtGrandTotal").text(total.toFixed(2));
    calculateDiscount();
    calculateOtherCharges();
    calculateTaxes();
    calculateGrandTotal();
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
