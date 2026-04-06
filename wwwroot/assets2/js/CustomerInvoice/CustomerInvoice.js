const bookingReportModalEl = document.getElementById("bookingReportModal");
const bookingReportModal = new bootstrap.Modal(bookingReportModalEl);
const invoiceModal = new bootstrap.Modal(document.getElementById('invoiceModal'));
const sendInvoiceModal = new bootstrap.Modal(document.getElementById('sendInvoiceModal'));
const paymentModal = document.getElementById('paymentModal');

var invoiceNo = 0;
var proformaInvoiceNo = 0;
var gstInvoiceNo = null;
var invoiceType = null;

const REPORT_COLUMNS_CUSTOMER_INVOICE = [
    { key: "InvoiceNo", label: "Invoice No" },
    { key: "EventDate", label: "Event Date" },
    { key: "EventName", label: "Event Name" },
    { key: "HallName", label: "Hall Name" },
    { key: "ClientName", label: "Client Name" },
    { key: "ClientContactNo", label: "Contact No" },
    { key: "TotalAmount", label: "Total Amount" },
    { key: "OutstandingAmount", label: "Outstanding" }
];

let selectedColumns = REPORT_COLUMNS_CUSTOMER_INVOICE.map(c => c.key);
const HEADER_KEY = "gstCustomerInvoicePrintWithHeader";

let selectedPaymentDuration = "thismonth";
let selectedStatusDuration = "thismonth";
let gstList = [];

$(document).ready(function () {
    setCurrentMonthDates();
});

function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fltEventFrom").val(formatDate2(firstDay));
    $("#fltEventTo").val(formatDate2(lastDay));
}

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
    $(".sub_count").text("In " + $(this).text());

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
    sessionStorage.setItem("SelectedFilter_DashboardWidget", selectedMonthDuration);    // Set the datepickers

    $("#fltEventFrom").val(formatDate2(fromDate));
    $("#fltEventTo").val(formatDate2(toDate));

    // 🔥 Load data
    //loadDashboardWidgetCounts();
});

$("#fltEventFrom, #fltEventTo").on("change", function () {

    let fromDate = $("#fltEventFrom").val();
    let toDate = $("#fltEventTo").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        $(".sub").text("In Custom Range");

        $(".sort-option").removeClass("active");

       // loadDashboardWidgetCounts(); // 🔥
    }
});

function loadColumnCheckboxes() {
    let html = "";

    REPORT_COLUMNS_CUSTOMER_INVOICE.forEach(col => {
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
    fetchData();
});


/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewCustomerInvoiceReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {

        // 🔍 Filters
        BookingId: 0,
        NameEmailContact: $("#fltSearch").val().trim() || null,
        SlotId: $("#fltSlot").val() || 0,

        EventFrom: $("#fltEventFrom").val() || null,
        EventTo: $("#fltEventTo").val() || null,

        BookedFrom: $("#fltBookedFrom").val() || null,
        BookedTill: $("#fltBookedTill").val() || null,

        SubEventId: $("#fltEvent").val() || 0,

        IsInvoiceGenerated: $("#fltIsInvoiceGenerated").val() === ""
            ? null
            : $("#fltIsInvoiceGenerated").val() === "true",

        PaymentStatusId: $("#fltPaymentStatus").val() || 0,

        IsExclusiveOfGST: null,
        IsInclusiveOfGST: null,

        FilterDuration: selectedMonthDuration || null,

        // ⚙️ Common
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    // ✅ API URL
    const url = actionName === "downloadExcel"
        ? "/api/CustomerInvoice/DownloadCustomerInvoiceReport_Excel"
        : "/api/CustomerInvoice/DownloadCustomerInvoiceReport_Pdf";

    downloadReport(url, payload, loaderMsg = "Generating Invoice..");
}


$(function () {
    const flag_isGeneratedInvoice = sessionStorage.getItem("IsShowTotalSalesList") === "true";
    if (flag_isGeneratedInvoice) {
        //console.log("Show Total Sales List is enabled");
        // clear after use
        sessionStorage.removeItem("IsShowTotalSalesList");
    }

    fetchData(flag_isGeneratedInvoice);
    loadSubEventList();
    loadSlotDropdown();
    getNextInvoiceNumbers();
    loadState();
    loadGSTMaster();

    loadPaymentCollection(selectedPaymentDuration);
    loadInvoiceStatus(selectedStatusDuration);

    loadAccountDropdown();
    loadPaymentModeDropdown();

    AppSignalR.subscribeSettings((data) => {
        //console.log("Settings Page Update", data);
        window.location.reload();
        fetchData(flag_isGeneratedInvoice);
    });

    //SUMMARY -- START 
    $(document).on('click', '.btn-summary', function () {
        var bookingId = $(this).data('bookingid');
        //console.log('Summary BookingId:', bookingId);
        loadBookingCalendar(bookingId);
    });
    //SUMMARY -- END

    //CREATE PROFORMA INVOICE -- START
    $(document).on('click', '.btn-create-proforma', function () {
        resetForm();

        const bookingId = $(this).data('bookingid');
        //console.log('Create Proforma BookingId:', bookingId);
        $('#bookingId').val(bookingId);
        $('#invoiceNo').val(0);
        $('#proformaInvoiceNo').val(proformaInvoiceNo);

        invoiceType = "proforma";
        $('#invoiceModalTitle').text('Create Proforma Invoice #' + proformaInvoiceNo);
        invoiceModal.show();

        const isInclusiveGST = false;
        fetchInvoiceByBookingId(bookingId, isInclusiveGST, true);
    });
    //CREATE PROFORMA INVOICE -- END

    //CREATE INVOICE - START
    $(document).on('click', '.btn-create-invoice', function () {
        resetForm();

        const bookingId = $(this).data('bookingid');
        const isInclusiveGST = $(this).data('isinclusivegst') === true;      
        fixedSubTotal = isInclusiveGST ? $(this).data('finalamount') : 0;

        //console.log('Create Invoice BookingId:', bookingId);
        $('#bookingId').val(bookingId);
        $('#invoiceNo').val(invoiceNo);
        $('#invoiceNo').data('isinclusivegst', isInclusiveGST);
        $('#proformaInvoiceNo').val(0);

        invoiceType = "invoice";

        const gstLabel = isInclusiveGST ? 'Rates Inclusive GST' : 'Rates Exclusive GST';
        $('#invoiceModalTitle').text(`Create Invoice #${invoiceNo} (${gstLabel})`);

        invoiceModal.show();

        fetchInvoiceByBookingId(bookingId, isInclusiveGST, false);
    });
    //CREATE INVOICE - END

    //EDIT INVOICE -- START
    $(document).on('click', '.btn-edit-invoice', function () {
        ShowHide_DeleteButton();
        resetForm();

        const bookingId = $(this).data('bookingid');
        const edit_invoiceNo = $(this).data('invoiceno');
        fixedSubTotal = $(this).data('finalamount');
        $('#bookingId').val(bookingId);
        $('#invoiceNo').val(edit_invoiceNo);
        $('#gstInvoiceNo').val(0);
        $('#proformaInvoiceNo').val(0);

        invoiceType = "invoice";

        $('#invoiceModalTitle').text('Edit Invoice #' + edit_invoiceNo);
        invoiceModal.show();

        fetchInvoiceById({
            BookingId: bookingId,
            InvoiceNo: edit_invoiceNo,
            GSTInvoiceNo: null,
            ProformaInvoiceNo: null
        });
    });
    //EDIT INVOICE -- END

    //EDIT GST-INVOICE -- START
    $(document).on('click', '.btn-edit-gstinvoice', function () {
        ShowHide_DeleteButton();
        resetForm();

        const bookingId = $(this).data('bookingid');
        const edit_gstinvoiceNo = $(this).data('gstinvoiceno');
        fixedSubTotal = $(this).data('finalamount');
        $('#bookingId').val(bookingId);
        $('#invoiceNo').val(0);
        $('#gstInvoiceNo').val(edit_gstinvoiceNo);
        $('#proformaInvoiceNo').val(0);

        invoiceType = "invoice";

        $('#invoiceModalTitle').text('Edit Invoice #' + edit_gstinvoiceNo);
        invoiceModal.show();

        fetchInvoiceById({
            BookingId: bookingId,
            InvoiceNo: null,
            GSTInvoiceNo: edit_gstinvoiceNo,
            ProformaInvoiceNo: null
        });
    });
    //EDIT GST-INVOICE -- END

    //EDIT PROFORMA INVOICE -- START
    $(document).on('click', '.btn-edit-proforma', function () {
        ShowHide_DeleteButton();
        resetForm();

        const bookingId = $(this).data('bookingid');
        const edit_proformaNo = $(this).data('proformano');

        $('#bookingId').val(bookingId);
        $('#invoiceNo').val(0);
        $('#gstInvoiceNo').val(0);
        $('#proformaInvoiceNo').val(edit_proformaNo);

        invoiceType = "proforma";

        $('#invoiceModalTitle').text('Edit Proforma #' + edit_proformaNo);
        invoiceModal.show();

        fetchInvoiceById({
            BookingId: bookingId,
            InvoiceNo: null,
            GSTInvoiceNo: null,
            ProformaInvoiceNo: edit_proformaNo
        });
    });
    //EDIT PROFORMA INVOICE -- END

    //SEND INVOICE -- START
    $(document).on('click', '.btn-send-invoice', function () {

        const bookingId = $(this).data('bookingid');
        const invoiceNo = $(this).data('invoiceno') || 0;
        const proformaNo = $(this).data('proformano') || 0;

        $("#sendBookingId").val(bookingId);
        $("#sendInvoiceNo").val(invoiceNo);
        $("#sendProformaNo").val(proformaNo);

        // 🔹 Change modal title dynamically
        if (proformaNo) {
            $("#sendInvoiceModal .modal-title").text("Send Proforma Invoice");
        } else {
            $("#sendInvoiceModal .modal-title").text("Send Invoice");
        }

        $("#sendEmail").val("");
        $("#sendContact").val("");
        $("#sendWhatsapp").val("");

        $("#chkEmail, #chkSMS").prop("checked", false);
        $("#chkWhatsapp").prop("checked", true);
        sendInvoiceModal.show();
    });


    $("#sendEmail").on("input", function () {
        $("#chkEmail").prop("checked", $(this).val().trim() !== "");
    });

    $("#sendContact").on("input", function () {
        $("#chkSMS").prop("checked", $(this).val().trim() !== "");
    });

    $("#sendWhatsapp").on("input", function () {
        $("#chkWhatsapp").prop("checked", $(this).val().trim() !== "");
    });

    $("#btnSendInvoice").click(function () {
        const email = $("#chkEmail").is(":checked") ? $("#sendEmail").val().trim() : "";
        const contact = $("#chkSMS").is(":checked") ? $("#sendContact").val().trim() : "";
        const whatsapp = $("#chkWhatsapp").is(":checked") ? $("#sendWhatsapp").val().trim() : "";

        //if (!email && !contact && !whatsapp) {
        if (!whatsapp) {
            Swal.fire({
                icon: "warning",
                title: "Required",
                //text: "Please provide at least one delivery mode."
                text: "WhatsApp number is required!"
            });
            return;
        }

        // Remove spaces & non-numeric characters
        const cleanedNumber = whatsapp.replace(/\D/g, '');
        if (cleanedNumber.length !== 10) {
            Swal.fire("Validation", "Enter valid 10-digit WhatsApp number!", "warning");
            return;
        }

        const payload = {
            BookingId: parseInt($("#sendBookingId").val()),
            InvoiceNo: $("#sendInvoiceNo").val() || "",
            ProformaInvoiceNo: $("#sendProformaNo").val() || "",
            EmailId: email,
            ContactNo: contact,
            WhatsappNo: whatsapp,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };

        const btn = $(this);
        btn.prop("disabled", true).html(`<span class="spinner-border spinner-border-sm"></span> Sending...`);

        $.ajax({
            url: "/api/CustomerInvoice/SendCustomerInvoice",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Invoice Sent");
                    $("#sendInvoiceModal").modal("hide");
                }
                else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: res.message || "Unable to send invoice."
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Send Invoice Failed");
            },
            complete: function () {
                btn.prop("disabled", false).html(`<i class="fa fa-paper-plane me-1"></i>Send`);
            }
        });
    });
    //SEND INVOICE -- END 

    //PDF PROFORMA INVOICE -- START
    $(document).on('click', '.btn-pdf-proforma', function () {

        const bookingId = $(this).data('bookingid');
        const proformaNo = $(this).data('proformano');

        const request = {
            BookingId: bookingId,
            InvoiceNo: null,
            ProformaInvoiceNo: proformaNo,
            WithHeader: true,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        generateInvoicePdf(request);
    });
    //PDF PROFORMA INVOICE -- END

    //PDF INVOICE -- START
    $(document).on('click', '.btn-pdf-invoice', function () {

        const bookingId = $(this).data('bookingid');
        const invoiceNo = $(this).data('invoiceno');

        const request = {
            BookingId: bookingId,
            InvoiceNo: invoiceNo,
            ProformaInvoiceNo: null,
            WithHeader: true,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        generateInvoicePdf(request);
    });
    //PDF INVOICE -- END

    //ITEMS ROW --- START
    $('#btnAddItem').on('click', function () {
        addItemRow();
    });

    // On input change
    $(document).on('input change', '.calc', function () {
        let row = $(this).closest('tr');
        recalcRow(row);
        recalcAll(row); // pass active row
    });

    $(document).on('input change', '#gstin', function () {
        // GSTIN or global change → recalc all rows
        $('#invoiceItemsBody tr').each(function () {
            recalcRow($(this));
            console.log("here2");
        });
        recalcAll();
    });

    // On Advance input change
    $(document).on('input', '#advancePaid, #bookingDiscount', function () {
        recalcAll();
    });

    // On delete row
    $(document).on('click', '.btn-delete-row', function () {
        $(this).closest('tr').remove();
        updateRowSequence();
        recalcAll();
    });
    //ITEMS ROW --- END

    //SAVE
    $('#btnSaveInvoice').on('click', function () {
        if (!validateBeforeSave()) return;

        const items = [];
        $('#invoiceItemsBody tr').each(function (index) {
            const row = $(this);

            items.push({
                SrNo: index+1,
                Description: row.find('.desc').val(),
                HSN: row.find('.hsn').val(),
                Rate: parseFloat(row.find('.rate').val()) || 0,
                Qty: parseFloat(row.find('.qty').val()) || 0,
                SubTotal: parseFloat(row.find('.subtotal').val()) || 0,

                DiscountRate: parseFloat(row.find('.discountrate').val()) || 0,
                DiscountTypeId: parseInt(row.find('.discounttype').val()) || 0,
                DiscountAmount: parseFloat(row.find('.discountamount').val()) || 0,

                TaxableAmount: parseFloat(row.find('.taxable').val()) || 0,
                GSTRate: parseFloat(row.find('.gst').val()) || 0,
                CGST: parseFloat(row.find('.cgst').val()) || 0,
                SGST: parseFloat(row.find('.sgst').val()) || 0,
                IGST: parseFloat(row.find('.igst').val()) || 0,

                Total: parseFloat(row.find('.total').val()) || 0
            });
        });

        //VALIDATION -- START ------------------------------
        let paymentAmount = parseFloat($("#invoicePayment_Amount").val()) || 0;
        let paymentMode = parseInt($("#invoicePayment_Mode").val()) || 0;
        let paymentAccount = parseInt($("#invoicePayment_Account").val()) || 0;

        if (paymentAmount > 0) {

            if (paymentMode === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Payment Mode Required',
                    text: 'Please select a payment mode.',
                    confirmButtonColor: '#d33'
                });
                return false;
            }

            if (paymentAccount === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Account Required',
                    text: 'Please select an account.',
                    confirmButtonColor: '#d33'
                });
                return false;
            }
        }
        //VALIDATION -- END ------------------------------

        //const isGstIncluded = $('#isGstIncluded').is(':checked');
        var totalGst = parseFloat($('#totalGST').val()) || 0;
        const gstInvoiceNoVal = gstInvoiceNo;
        const invoiceNoVal = $('#invoiceNo').val();
        const proformaNoVal = $('#proformaInvoiceNo').val();

        const isExclusiveOfGST = $('#isExclusiveOfGST').val() === 'true' || $('#isExclusiveOfGST').val() === '1';
        const isInclusiveOfGST = $('#isInclusiveOfGST').val() === 'true' || $('#isInclusiveOfGST').val() === '1';

        const request = {
            Id: parseInt($('#invoiceId').val()) || 0,
            BookingId: parseInt($('#bookingId').val()) || 0,

            InvoiceNo: (invoiceType === "invoice" && (totalGst > 0 || gstInvoiceNo == null))
                ? (invoiceNoVal ? Number(invoiceNoVal) : null)
                : null,

            ProformaInvoiceNo: (invoiceType === "proforma")
                ? (proformaNoVal ? Number(proformaNoVal) : null)
                : null,

            GSTInvoiceNo: (invoiceType === "invoice"
                && Math.abs(totalGst) < 0.01
                && gstInvoiceNoVal !== '')
                ? gstInvoiceNoVal
                : null,

            IsExclusiveOfGST : isExclusiveOfGST,
            IsInclusiveOfGST : isInclusiveOfGST,

            CustomerName: $('#customerName').val(),
            InvoiceDate: $('#invoiceDate').val(),
            GSTIN: $('#gstin').val(),
            Address: $('#address').val(),
            EmailId: $('#emailId').val(),
            CustomerState: parseInt($('#customerState').val()) || null,

            Payment_ModeId: parseInt($('#invoicePayment_Mode').val()) || 0,
            Payment_AccountId: parseInt($('#invoicePayment_Account').val()) || 0,
            Payment_TransactionId: $('#invoicePayment_Txn').val()?.trim() || null,
            Payment_ChequeBank: $('#invoicePayment_ChequeBank').val()?.trim() || null,
            Payment_ChequeNo: $('#invoicePayment_ChequeNo').val()?.trim() || null,
            Payment_ChequeDate: $('#invoicePayment_ChequeDate').val() || null,
            Payment_Amount: parseFloat($('#invoicePayment_Amount').val()) || 0,
            Payment_PaidDate: $('#invoicePayment_Date').val() || null,
            Payment_PaidTime: $('#invoicePayment_Time').val() || null,
            Payment_DueDate: $('#invoicePayment_NextDueDate').val() || null,
            Payment_Note: $('#invoicePayment_noteEditor textarea').val()?.trim() || null,

            SubTotal: parseFloat($('#subTotal').val()) || 0,
            BookingDiscount: parseFloat($('#bookingDiscount').val()) || 0,
            TotalTaxableAmount: parseFloat($('#totalTaxable').val()) || 0,
            TotalDiscountAcrossAllServices: 0,     // if not used
            TotalGST: totalGst,
            TotalVAT: 0,
            GrandTotal: parseFloat($('#grandTotal').val()) || 0,
            RoundOff: parseFloat($('#roundOff').val()) || 0,
            TotalAdvancePaid: parseFloat($('#advancePaid').val()) || 0,
            BalanceAmount: parseFloat($('#balanceAmount').val()) || 0,

            JSON_Items: items,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        //console.log("save payload:", request);

        $.ajax({
            url: '/api/CustomerInvoice/SaveCustomerInvoice', // adjust if needed
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function (res) {

                if (res.status === 'Success') {
                    handleApiSuccess('Invoice saved successfully');
                    fetchData();
                    getNextInvoiceNumbers();
                    //resetForm();
                    $('#btnSaveInvoice').prop('disabled', true);
                    invoiceModal.hide();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Failed',
                        text: res.message || 'Failed to save invoice'
                    });
                }
            },
            error: function (xhr, statue, error) {
                handleApiError(xhr, statue, error, "Save Invoice Failed");
            }
        });
    });

    $('#btnAdjustDiff').on('click', function () {
        const currentSubTotal = parseFloat($('#subTotal').val()) || 0;
        const diff = fixedSubTotal - currentSubTotal;

        if (Math.abs(diff) <= 0.01) return;

        adjustLastRow(diff);
        recalcAll();
    });

    function validateBeforeSave() {
        const isInclusiveGST = $('#invoiceNo').data('isinclusivegst') === true;
        if (!isInclusiveGST) return true;

        const currentSubTotal = parseFloat($('#subTotal').val()) || 0;
        const diff = fixedSubTotal - currentSubTotal;

        // ✅ Auto-fix small difference
        if (Math.abs(diff) <= 0.05 && Math.abs(diff) > 0.01) {
            adjustLastRow(diff);
            recalcAll();
        }

        // ❌ Still mismatch → block
        if (Math.abs(diff) > 0.05) {
            $("#btnAdjustDiff").removeClass("d-none");
            $("#diffValue").text(diff.toFixed(2));
            Swal.fire({
                icon: 'error',
                title: 'Invalid Total',
                text: `Difference ₹${diff.toFixed(2)}. Please adjust items.`
            });
            return false;
        }
        return true;
    }

    //FILTER - PAYMENT ---- START ---------------------------
    //collections
    $(document).on("click", ".payment-option", function (e) {
        e.preventDefault();

        const newFilter = $(this).data("value");

        if (selectedPaymentDuration === newFilter) return;

        selectedPaymentDuration = newFilter;

        $(".payment-option").removeClass("active");
        $(this).addClass("active");

        $("#PaymentChartFilter").text($(this).text());

        loadPaymentCollection(selectedPaymentDuration);
    });

    //load Invoice Status
    $(document).on("click", ".invoicestatus-option", function (e) {
        e.preventDefault();
        const newFilter = $(this).data("value");
        if (selectedStatusDuration === newFilter) return;

        selectedStatusDuration = newFilter;

        $(".invoicestatus-option").removeClass("active");
        $(this).addClass("active");

        $("#InvoiceChartFilter").text($(this).text());

        loadInvoiceStatus(selectedStatusDuration);
    });

    //FILTER - PAYMENT ---- END -----------------------------

    //OUTSTANDING PAYMENT -------------------- START ------------------------
    $("#btnSavePayment").click(function () {
        // 🔹 Get values
        let bookingId = Number($("#outstanding_paymentBookingId").val() || 0);
        let accountId = Number($("#outstandingPayment_Account").val());
        let modeId = Number($("#outstandingPayment_Mode").val());
        let paymentDate = $("#outstandingPayment_Date").val();
        let paymentTime = $("#outstandingPayment_Time").val();
        let txnId = $("#outstandingPayment_Txn").val();
        let chequeBank = $("#outstandingPayment_Chequebank").val();
        let chequeNo = $("#outstandingPayment_ChequeNo").val();
        let chequeDate = $("#outstandingPayment_ChequeDate").val();
        let amount = Number($("#outstandingPayment_Amount").val() || 0);
        let dueBalance = Number($("#outstandingPayment_DueBalance").val() || 0);
        let nextDueDate = $("#outstandingPayment_NextDueDate").val();

        // 🔹 Basic Validation
        if (!bookingId) {
            Swal.fire("Validation", "Booking Id missing!", "warning");
            return;
        }

        if (!modeId) {
            Swal.fire("Validation", "Select payment mode!", "warning");
            return;
        }

        if (!accountId) {
            Swal.fire("Validation", "Select account!", "warning");
            return;
        }

        if (!paymentDate) {
            Swal.fire("Validation", "Payment date is required!", "warning");
            return;
        }
        if (!amount || amount <= 0) {
            Swal.fire("Validation", "Valid amount is required!", "warning");
            return;
        }
        var userId = Number(sessionStorage.getItem("userId") ?? "0");
        // 🔹 Payload (matches SaveBooking_Payments procedure)
        const payload = {
            BookingId: bookingId,
            AccountId: accountId,
            ModeId: modeId,
            DateOfPayment: paymentDate,
            PaymentTime: paymentTime || null,
            TxnId: txnId || null,
            ChequeBank: chequeBank || null,
            ChequeNo: chequeNo || null,
            ChequeDate: chequeDate || null,
            Amount: amount,
            DueBalance: dueBalance || 0,
            NextDueDate: nextDueDate || null,
            ActionBy: userId
        };

        ////console.log("Save Payment Payload :", payload);

        $("#btnSavePayment").prop("disabled", true);

        $.ajax({
            url: "/api/Booking_Payments/SaveBookingPayment", // 🔁 API URL
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response?.status === "Success") {
                    handleApiSuccess();

                    // ✅ Swal Confirmation
                    Swal.fire({
                        title: 'Download Receipt?',
                        text: 'Do you want to download the payment receipt now?',
                        icon: 'question',
                        showCancelButton: true,
                        confirmButtonText: 'Yes, Download',
                        cancelButtonText: 'No',
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33'
                    }).then((result) => {
                        var payload = {
                            PaymentId: response.data.PaymentId,
                            BookingId: bookingId,
                            ActionBy: userId
                        }
                        var url = "/api/Booking_Payment/DownloadPaymentReceipt";
                        if (result.isConfirmed) {
                            // ✅ Call download only if YES
                            downloadReport(url, payload, "Generating Receipt...");
                        }
                        // ❌ If NO → do nothing (flow continues)

                        $("#outstanding_paymentForm")[0].reset();
                        //$("#PaymentModal").modal("hide");

                        // 🔁 Reload payment list if needed
                        //loadBookingCalendar();
                        fetchData();
                        loadPaymentHistory(bookingId);
                    });
                } else {
                    Swal.fire("Error", response?.message || "Save failed!", "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Payment Error");
            },
            complete: function () {
                $("#btnSavePayment").prop("disabled", false);
            }
        });
    });

    $('#tableMain').on('click', '.btn-outstanding', function () {
        $("#outstanding_paymentForm")[0].reset();

        const bookingId = $(this).data('bookingid');
        const dueAmount = Number($(this).data('outstandingamount')) || 0;
        if (dueAmount <= 0) {
            $("#dueMessage").removeClass("d-none");
            $("#outstandingPayment_Amount").prop("readonly", true);;
        }
        else {
            $("#dueMessage").addClass("d-none");
            $("#outstandingPayment_Amount").prop("readonly", false);
        }

        //console.log("Outstanding clicked for BookingId:", bookingId);

        $("#outstandingPayment_DueBalance, #outstandingPayment_originalDueAmount").val(dueAmount);
        $("#outstanding_paymentBookingId").val(bookingId);
        setCurrentPaymentDateTime();

        // Call your method
        loadPaymentHistory(bookingId);
        $("#outstandingPayment_Mode").trigger("change");
    });

    //outstandingPayment_Amount - Input
    $(document).on("input", "#outstandingPayment_Amount", function () {
        const originalDue = Number($("#outstandingPayment_originalDueAmount").val()) || 0;
        let payAmount = Number($(this).val()) || 0;

        //if (payAmount > originalDue) {
            //payAmount = originalDue;
            //$(this).val(originalDue);
        //}
        //$(this).val(payAmount);
        const remaining = originalDue - payAmount;
        $("#outstandingPayment_DueBalance").val(remaining.toFixed(2));
    });

    //invoicePayment_Amount - Input
    $(document).on("input", "#invoicePayment_Amount", function () {
        const originalDue = Number($("#invoicePayment_originalDueAmount").val()) || 0;
        let payAmount = Number($(this).val()) || 0;

        // 🔒 Prevent overpayment
        //if (payAmount > originalDue) {
            //payAmount = originalDue;
            //$(this).val(originalDue);
        //}
        //$(this).val(payAmount);
        const remaining = originalDue - payAmount;
        $("#invoicePayment_DueBalance, #balanceAmount").val(remaining.toFixed(2));
    });

    //outstandingPayment_btnwritrnote - click
    $(document).on("click", "#outstandingPayment_btnwritrnote", function () {
        const editor = $(".outstandingPayment_editorsContainer");
        editor.toggleClass("d-none");
        if (editor.hasClass("d-none")) {
            $(this).val("Payment Notes");
        }
        else {
            $(this).val("Hide Notes");
        }

    });
    //invoicePayment_btnWriteNote - click
    $(document).on("click", "#invoicePayment_btnWriteNote", function () {
        const editor = $(".invoicePayment_EditorContainer");
        editor.toggleClass("d-none");
        if (editor.hasClass("d-none")) {
            $(this).val("Payment Notes");
        }
        else {
            $(this).val("Hide Notes");
        }

    });

    //outstandingPayment_Mode - change
    $(document).on("change", "#outstandingPayment_Mode", function () {
        //console.log("outstandingPayment_Mode called");
        let selectedText = $("#outstandingPayment_Mode option:selected").text().toLowerCase();
        //console.log("selectedText:", selectedText);

        // Hide all first
        $(".paymentModal_Fields").addClass("d-none");
        $(".paymentModal_Fields input").val("");
        // CASH
        if (selectedText.includes("cash")) {
            // Nothing to show
        }

        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            // Nothing to show
        }

        // CHEQUE
        else if (selectedText.includes("cheque")) {
            $("#outstandingPayment_Chequebank").closest(".paymentModal_Fields").removeClass("d-none");
            $("#outstandingPayment_ChequeNo").closest(".paymentModal_Fields").removeClass("d-none");
            $("#outstandingPayment_ChequeDate").closest(".paymentModal_Fields").removeClass("d-none");
        }

        // UPI OR CREDIT
        else if (selectedText.includes("upi") || selectedText.includes("credit")) {
            $("#outstandingPayment_Txn").closest(".paymentModal_Fields").removeClass("d-none");
        }
    });

    //invoicePayment_Mode - change
    $(document).on("change", "#invoicePayment_Mode", function () {

        //console.log("invoicePayment_Mode called");

        let selectedText = $("#invoicePayment_Mode option:selected")
            .text()
            .toLowerCase();

        //console.log("selectedText:", selectedText);

        // 🔹 Hide all invoice dynamic fields first
        $(".invoicePayment_Fields").addClass("d-none");
        $(".invoicePayment_Fields input").val("");
        // CASH
        if (selectedText.includes("cash")) {
            // Nothing to show
        }

        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            // Nothing to show
        }

        // CHEQUE
        else if (selectedText.includes("cheque")) {

            $("#invoicePayment_ChequeBank")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");

            $("#invoicePayment_ChequeNo")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");

            $("#invoicePayment_ChequeDate")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");
        }

        // UPI OR CREDIT
        else if (selectedText.includes("upi") || selectedText.includes("credit")) {

            $("#invoicePayment_Txn")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");
        }

    });

    //OUTSTANDING PAYMENT -------------------- END  --------------------------

    function initToggle({containerId, toggleId, labelId, storageKey, showText, hideText, messageId = null}) {
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
        containerId: "#invoiceWidgets",
        toggleId: "#toggleWidgets",
        labelId: "#widgetLabel",
        storageKey: "hideInvoiceWidgets",
        showText: "Show Widgets",
        hideText: "Hide Widgets"
    });
    //Widgets hide/show -- END --------------

    //Filter hide/show -- START --------------
    initToggle({
        containerId: "#invoiceWidgets_filter",
        toggleId: "#toggleWidgets_filter",
        labelId: "#widgetLabel_filter",
        storageKey: "hideInvoiceWidgets_filter",
        showText: "Show Filter",
        hideText: "Hide Filter",
        messageId: "#filterHiddenMessage" // 👈 added
    });
    //filter hide/show -- END --------------

    //tooltip -widget---------- START -----------------------
    const tooltipContent = {
        // ================= PAYMENT COLLECTION =================
        paymentReceivedTitle: `Total of collected Advance + Invoice Amounts, within the selected period`,

        // ================= UPCOMING PAYMENT =================
        upcomingPaymentTitle: `Total of pending Invoice Payments, due within the selected period.`
    };

    Object.keys(tooltipContent).forEach(id => {
        const el = document.getElementById(id);
        if (el) {

            if (el._tooltipInstance) {
                el._tooltipInstance.dispose();
            }

            const tooltip = new bootstrap.Tooltip(el, {
                title: tooltipContent[id],
                html: true,
                placement: 'bottom',
                trigger: 'hover',
                fallbackPlacements: [],
                customClass: 'kpi-wide-tooltip'   // 👈 add this
            });

            el._tooltipInstance = tooltip;
        }
    });
    //tooltip -widget---------- END -----------------------

    //Delete Invoice ---------- START --------------
    $(document).on("click", "#btnDeleteInvoice", function () {
        const invoiceId = $("#invoiceId").val();
        if (!invoiceId || invoiceId == 0) return;
        // Store ID temporarily in delete button
        $("#delete-record").data("id", invoiceId);
        $("#deleteRecordModal").modal("show");
    });

    $(document).on("click", "#delete-record", function () {
        const invoiceId = $(this).data("id");
        if (!invoiceId) return;

        var payload = {
            Id: invoiceId,
            ActionBy: sessionStorage.getItem("userId") // or logged user id
        };

        $.ajax({
            url: "/api/CustomerInvoice/DeleteCustomerInvoice",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    $("#deleteRecordModal").modal("hide");
                    $("#invoiceModal").modal("hide");
                    handleApiSuccess("Invoice Deleted");
                    fetchData();
                }
                else {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Delete Failed',
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Invoice Delete Fail");
            }
        });
    });
    //Delete Invoice ---------- END --------------

    $(document).on("click", "#txtPaidAmount, #barPaid", function () {
        openPaymentModal("PAID");
    });

    $(document).on("click", "#txtPendingAmount, #barPending", function () {
        openPaymentModal("PENDING");
    });

});

function updateInvoiceTitle(totalGST) {
    const isInclusiveGST = $('#invoiceNo').data('isinclusivegst') === true;
    //console.log("isInclusiveGST:", isInclusiveGST);
    const gstLabel = isInclusiveGST ? 'Rates Inclusive GST' : 'Rates Exclusive GST';

    if (gstInvoiceNo !== null && gstInvoiceNo !== '' && parseFloat(totalGST) === 0 && invoiceType == "invoice") {
        $('#invoiceModalTitle').text(`Create Invoice #${gstInvoiceNo} (${gstLabel})`);
    }
    else if (invoiceType == "invoice"){
        $('#invoiceModalTitle').text(`Create Invoice #${invoiceNo} (${gstLabel})`);
    }
}

let paymentTable = null;
function openPaymentModal(type) {
    $("#paymentModalTitle").text(type === "PAID" ? "Payment Received Details" : "Payment Pending Details");
    $("#paymentDetailsTable tbody").html("<tr><td colspan='8'>Loading...</td></tr>");
    $("#paymentDetailsModal").modal("show");
    loadPaymentDetails(type);
}
function loadPaymentDetails(type) {
    var payload = {
        Type: type,
        MonthDuration: selectedPaymentDuration,
        FromDate: null,
        ToDate: null
    };
    console.log("payload:", payload);
    $.ajax({
        url: "/api/CustomerInvoice/GetDashboardPaymentCollectionDetails",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res:", res);

            if (res.status !== "Success") {
                Swal.fire({
                    icon: "warning",
                    title: "Notice",
                    text: res.message
                });
                return;
            }

            const data = res.data?.Rows ?? [];
            const totals = res.data?.Totals ?? {};

            /* =========================
               🔹 SAFE TOTALS
            ========================= */
            $("#totalBookings").text(totals.TotalBookings ?? 0);
            $("#totalAmount").text(formatINR2(totals.TotalAmount ?? 0));
            $("#paidAmount").text(formatINR2(totals.PaidAmount ?? 0));
            $("#pendingAmount").text(formatINR2(totals.PendingAmount ?? 0));

            /* =========================
               🔹 DESTROY OLD TABLE
            ========================= */
            if ($.fn.DataTable.isDataTable('#paymentDetailsTable')) {
                $('#paymentDetailsTable').DataTable().clear().destroy();
            }

            /* =========================
               🔹 INIT DATATABLE
            ========================= */
            paymentTable = $("#paymentDetailsTable").DataTable({
                data: data,
                pageLength: 10,
                paging: false,
                scrollCollapse: true,
                ordering: false,
                autoWidth: false,
                language: {
                    emptyTable: "No payment records found"
                },
                columns: [
                    { data: "BookingRefNo", defaultContent: "-" },
                    { data: "CustomerName", defaultContent: "-" },
                    { data: "MobileNo", defaultContent: "-" },
                    { data: "EventDate_Format", defaultContent: "-" },
                    {
                        data: "TotalAmount",
                        className: "text-primary fw-bold",
                        render: formatINR2
                    },
                    {
                        data: "PaidAmount",
                        className: "text-success fw-bold",
                        render: formatINR2
                    },
                    {
                        data: "PendingAmount",
                        className: "text-danger fw-bold",
                        render: formatINR2
                    },
                    {
                        data: "PaymentStatus",
                        render: function (d) {
                            if (d === "Paid")
                                return `<span class="badge bg-success">Paid</span>`;
                            else if (d === "Partial")
                                return `<span class="badge bg-warning text-dark">Partial</span>`;
                            else
                                return `<span class="badge bg-danger">Unpaid</span>`;
                        }
                    }
                ]
            });

            /* =========================
               🔹 OPTIONAL: INLINE EMPTY MESSAGE
            ========================= */
            if (data.length === 0) {
                $("#paymentDetailsTable tbody")
                    .html(`<tr><td colspan="8" class="text-center text-muted">No records available</td></tr>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading payment details.");
        }
    });
}

function ShowHide_DeleteButton() {
    //Delete button HIDE/SHOW
    const isSuperUser = sessionStorage.getItem("isSuperUser") === "true";
    if (isSuperUser) {
        document.getElementById("btnDeleteInvoice").classList.remove("d-none");
    }
}

function loadGSTMaster() {
    $.ajax({
        url: "/api/GSTMaster/GetGSTMaster",
        type: "GET",
        success: function (res) {
            if (res.status === "Success") {
                gstList = res.data;
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load GST master.");
        }
    });
}

function buildGSTDropdown(selectedGST = 0) {
    let options = '';
    gstList.forEach(g => {
        const selected = parseFloat(g.GSTValue) === parseFloat(selectedGST)
            ? 'selected'
            : '';
        options += `<option value="${g.GSTValue}" ${selected}>${g.GSTText}</option>`;
    });
    return options;
}
function setCurrentPaymentDateTime() {

    const now = new Date();

    // Format date → yyyy-mm-dd (required for input[type=date])
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');

    const formattedDate = `${year}-${month}-${day}`;

    // Format time → HH:mm (required for input[type=time])
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    const formattedTime = `${hours}:${minutes}`;

    $("#outstandingPayment_Date").val(formattedDate);
    $("#outstandingPayment_Time").val(formattedTime);
}
function addItemRow() {
    const gstOptions = buildGSTDropdown();
    const row = `
        <tr>
            <td class="text-center col-index p-1"></td>

            <td class="col-desc p-1">
                <input type="text" class="form-control form-control-sm desc p-1">
            </td>

            <td class="col-hsn p-1">
                <input type="text" class="form-control form-control-sm hsn p-1">
            </td>

            <td class="col-rate p-1">
                <input type="number" class="form-control form-control-sm text-end rate calc p-1" value="0">
            </td>

            <td class="col-qty p-1">
                <input type="number" class="form-control form-control-sm text-end qty calc p-1" value="1">
            </td>

            <td class="col-subtotal p-1">
                <input type="number" class="form-control form-control-sm text-end subtotal calc p-1" value="0">
            </td>

            <td class="col-discount-rate p-1">
                <input type="number" class="form-control form-control-sm text-end discountrate calc p-1" value="0">
            </td>

            <td class="col-discount-type p-1">
                <select class="form-select form-select-sm discounttype calc p-1">
                    <option value="1">%</option>
                    <option value="2">Flat</option>
                </select>
            </td>

            <td class="col-discount-amount p-1">
                <input type="number" class="form-control form-control-sm text-end discountamount calc p-1" value="0">
            </td>

            <td class="col-taxable p-1">
                <input type="number" class="form-control form-control-sm text-end taxable calc p-1" value="0">
            </td>

            <td class="col-gst p-1">
                <select class="form-select form-select-sm gst calc p-1">
                    ${gstOptions}
                </select>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end cgst p-1" readonly>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end sgst p-1" readonly>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end igst p-1" readonly>
            </td>

            <td class="col-total p-1">
                <input type="number" class="form-control form-control-sm text-end fw-bold total p-1" readonly>
            </td>

            <td class="text-center col-action p-1">
                <i class="fa fa-times text-danger btn-delete-row" role="button"></i>
            </td>
        </tr>
    `;

    $('#invoiceItemsBody').append(row);
    updateRowSequence();
}
function updateRowSequence() {
    $('#invoiceItemsBody tr').each(function (index) {
        $(this).find('.col-index').text(index + 1);
    });
}

function adjustLastRow(diff) {
    const rows = $('#invoiceItemsBody tr');
    if (rows.length === 0) return;

    //const lastRow = rows.last();
    const lastRow = $('#invoiceItemsBody tr')
        .filter(function () {
            return (parseFloat($(this).find('.qty').val()) || 0) > 0;
        })
        .last();

    let rate = parseFloat(lastRow.find('.rate').val()) || 0;
    let qty = parseFloat(lastRow.find('.qty').val()) || 1;
    let gstRate = parseFloat(lastRow.find('.gst').val()) || 0;

    if (qty === 0) qty = 1;

    const factor = 1 + (gstRate / 100);

    // ✅ Correct adjustment considering GST
    let rateAdjustment = diff / (qty * factor);

    rate = rate + rateAdjustment;

    if (rate < 0) rate = 0;

    lastRow.find('.rate').val(rate.toFixed(2));

    recalcRow(lastRow);
}
function round2(val) {
    return Math.round(val * 100) / 100;
}
function recalcRow(row) {

    const rate = parseFloat(row.find('.rate').val()) || 0;
    const qty = parseFloat(row.find('.qty').val()) || 0;
    const gstRate = parseFloat(row.find('.gst').val()) || 0;

    const discountRate = parseFloat(row.find('.discountrate').val()) || 0;
    const discountType = row.find('.discounttype').val();

    const gstFactor = 1 + (gstRate / 100);

    // Step 1: Subtotal
    let subTotal = round2(rate * qty);
    row.find('.subtotal').val(subTotal.toFixed(2));

    // Step 2: Discount Calculation
    let discountAmount = 0;
    if (discountType === "1") {
        discountAmount = subTotal * discountRate / 100;
    } else {
        discountAmount = discountRate;
    }
    discountAmount = round2(discountAmount);
    row.find('.discountamount').val(discountAmount.toFixed(2));

    // Step 3: Taxable after discount
    let taxable = round2(subTotal - discountAmount);
    if (taxable < 0) taxable = 0;
    row.find('.taxable').val(taxable.toFixed(2));

    // Step 4: GST Calculation
    let gstAmount = round2(taxable * gstRate / 100);

    const gstin = ($('#gstin').val() || '').trim();
    const stateCode = gstin.length >= 2 ? gstin.substring(0, 2) : "";

    let cgst = 0;
    let sgst = 0;
    let igst = 0;

    if (gstin && stateCode !== "24") {
        // Inter-state → IGST
        igst = gstAmount;
    }
    else {
        // Intra-state → CGST + SGST
        cgst = gstAmount / 2;
        sgst = gstAmount / 2;
    }
    let total = round2(taxable + gstAmount);

    row.find('.cgst').val(cgst.toFixed(2));
    row.find('.sgst').val(sgst.toFixed(2));
    row.find('.igst').val(igst.toFixed(2));
    row.find('.total').val(total.toFixed(2));
    recalcAll();
}
function recalcAll() {

    let subTotal = 0;
    let totalTaxable = 0;
    let totalGST = 0;
    let totalDiscount = 0;

    const isInclusiveGST = $('#invoiceNo').data('isinclusivegst') === true;

    let rows = $('#invoiceItemsBody tr');

    // ✅ Step 1: Sum totals
    rows.each(function () {
        const row = $(this);

        let total = parseFloat(row.find('.total').val()) || 0;
        let taxable = parseFloat(row.find('.taxable').val()) || 0;
        let cgst = parseFloat(row.find('.cgst').val()) || 0;
        let sgst = parseFloat(row.find('.sgst').val()) || 0;
        let igst = parseFloat(row.find('.igst').val()) || 0;
        let discountamount = parseFloat(row.find('.discountamount').val()) || 0;

        subTotal += total;
        totalTaxable += taxable;
        totalGST += (cgst + sgst + igst);
        totalDiscount += discountamount;
    });

    // ✅ Step 2: Set fixed subtotal once
    //if (isInclusiveGST && fixedSubTotal === 0) {
    //    fixedSubTotal = subTotal;
    //}

    // ✅ Step 3: Show totals (NO clamp, NO adjustment)
    $('#bookingDiscount').val(totalDiscount.toFixed(2));
    $('#subTotal').val(subTotal.toFixed(2));
    $('#totalTaxable').val(totalTaxable.toFixed(2));
    $('#totalGST').val(totalGST.toFixed(2));

    updateInvoiceTitle(totalGST);

    // ✅ Step 4: Grand total
    const discount = parseFloat($('#bookingDiscount').val()) || 0;
    const grandTotal = totalTaxable + totalGST - discount;
    $('#grandTotal').val(grandTotal.toFixed(2));

    let roundedTotal = Math.round(grandTotal);
    $('#roundOff').val(roundedTotal.toFixed(2));

    const advance = parseFloat($('#advancePaid').val()) || 0;
    $('#balanceAmount, #invoicePayment_DueBalance, #invoicePayment_originalDueAmount')
        .val((roundedTotal - advance).toFixed(2));
}

let fixedSubTotal = 0;
function setFixedSubTotal() {
    fixedSubTotal = parseFloat($('#subTotal').val()) || 0;
}

//proforma & invoice -- fetch EDIT -- START
function fetchInvoiceById(params) {
    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoiceById',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(params),
        success: function (res) {

            if (res.status !== 'Success' || !res.data) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invoice Not Found',
                    text: 'Invoice data not found'
                });
                return;
            }
            const data = res.data;
            console.log("GetCustomerInvoiceById data:", data);

            /* ---------- MASTER ---------- */
            $('#invoiceId').val(data.Id);
            $('#bookingId').val(params.BookingId);
            $('#customerName').val(data.CustomerName);
            $('#invoiceDate').val(data.InvoiceDate);
            $('#gstin').val(data.GSTIN);
            $('#emailId').val(data.EmailId);
            $('#address').val(data.Address);
            $('#customerState').val(data.CustomerStateId);

            $('#isExclusiveOfGST').val(data.IsExclusiveOfGST);
            $('#isInclusiveOfGST').val(data.IsInclusiveOfGST);
            $('#invoiceNo').data('isinclusivegst', data.IsInclusiveOfGST);

            /* ---------- TOTALS (DIRECT SET) ---------- */
            $('#subTotal').val(data.SubTotal);
            $('#bookingDiscount').val(data.BookingDiscount);
            $('#totalTaxable').val(data.TotalTaxableAmount);
            $('#totalGST').val(data.TotalGST);
            // Call after setting GST
            updateInvoiceTitle(data.TotalGST);

            $('#grandTotal').val(data.GrandTotal);
            $('#roundOff').val(data.RoundOff);
            $('#advancePaid').val(data.TotalAdvancePaid).attr('readonly', true);
            $('#balanceAmount').val(data.BalanceAmount);
            /* ---------- ITEMS ---------- */
            $('#invoiceItemsBody').empty();

            if (data.Items && data.Items.length > 0) {

                data.Items.forEach(item => {
                    const gstOptions = buildGSTDropdown(item.GSTRate);
                    const row = `
                        <tr>
                            <td class="text-center col-index"></td>

                            <td><input class="form-control form-control-sm desc p-1" value="${item.Description ?? ''}"></td>
                            <td><input class="form-control form-control-sm hsn p-1" value="${item.HSN ?? ''}"></td>

                            <td><input class="form-control form-control-sm text-end rate calc p-1" value="${item.Rate}"></td>
                            <td><input class="form-control form-control-sm text-end qty calc p-1" value="${item.Qty}"></td>
                            <td><input class="form-control form-control-sm text-end subtotal calc p-1" value="${item.SubTotal}"></td>

                            <td><input class="form-control form-control-sm text-end discountrate calc p-1" value="${item.DiscountRate}"></td>
                            <td>
                                <select class="form-select form-select-sm discounttype p-1 calc">
                                    <option value="1" ${item.DiscountTypeId == 1 ? "selected" : ""}>%</option>
                                    <option value="2" ${item.DiscountTypeId == 2 ? "selected" : ""}>Flat</option>
                                </select>
                            </td>
                            <td><input class="form-control form-control-sm text-end discountamount p-1 calc" value="${item.DiscountAmount}"></td>

                            <td><input class="form-control form-control-sm text-end taxable p-1 calc" value="${item.TaxableAmount}"></td>

                            <td class="col-gst p-1">
                                <select class="form-select form-select-sm gst calc p-1">
                                    ${gstOptions}
                                </select>
                            </td>

                            <td><input class="form-control form-control-sm text-end cgst p-1" value="${item.CGST}" readonly></td>
                            <td><input class="form-control form-control-sm text-end sgst p-1" value="${item.SGST}" readonly></td>
                            <td><input class="form-control form-control-sm text-end igst p-1" value="${item.IGST}" readonly></td>
                            <td><input class="form-control form-control-sm text-end fw-bold total p-1" value="${item.Total}" readonly></td>

                            <td class="text-center">
                                <i class="fa fa-times text-danger btn-delete-row"></i>
                            </td>
                        </tr>
                    `;

                    $('#invoiceItemsBody').append(row);
                });

                updateRowSequence();
            }
            $("#paymentSection").addClass("d-none");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Invoice Failed");
        }
    });
}
//proforma & invoice -- fetch EDIT -- END

//proforma & invoice -- fetch -- START
function fetchInvoiceByBookingId(bookingId, isInclusiveGST = false, isProformaInvoice = false) {
    $("#paymentSection").removeClass("d-none");
    var payload = {
        BookingId: bookingId,
        IsInclusiveGST: isInclusiveGST,
        IsProformaInvoice: isProformaInvoice
    };
    //console.log("payload fetchInvoiceByBookingId:", payload);
    $.ajax({

        url: '/api/CustomerInvoice/GetCustomerInvoiceByBookingId', // adjust if needed
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("fetch ByBookingId res:", res);
            if (res.status !== 'Success' || !res.data) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invoice Not Found',
                    text: 'Invoice data not found',
                    confirmButtonText: 'OK'
                });
                return;
            }
            var data = res.data;
            /* ---------- MASTER FIELDS ---------- */
            $('#customerName').val(data.ClientName);
            $('#invoiceDate').val(data.InvoiceDate);
            $('#gstin').val(data.GSTIN);
            $('#emailId').val(data.Email);
            $('#address').val(data.Address);
            $('#customerState').val(data.ClientStateId);
            $('#isExclusiveOfGST').val(data.IsExclusiveOfGST);
            $('#isInclusiveOfGST').val(data.IsInclusiveOfGST);

            //$('#isGstIncluded').prop('checked', data.IsGSTIncluded === true);

            $('#invoiceItemsBody').empty();

            if (data.ItemsJson && data.ItemsJson.length > 0) {

                data.ItemsJson.forEach(item => {

                    const taxable = parseFloat(item.Taxable) || 0;
                    //const gstPercent = taxable > 0 && item.GSTRate ? ((item.GSTRate / taxable) * 100).toFixed(2) : 0;
                    const gstOptions = buildGSTDropdown(item.GSTRate);

                    const row = `
                        <tr>
                            <td class="text-center col-index p-1"></td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm desc p-1"
                                       value="${item.Item_Description ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm hsn p-1"
                                       value="${item.HSN ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end rate calc p-1"
                                       value="${item.Rate ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end qty calc p-1"
                                       value="${item.Qty ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end subtotal calc p-1"
                                       value="${item.SubTotal ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountrate calc p-1"
                                       value="${item.DiscountRate ?? 0}">
                            </td>

                            <td class="p-1">
                                <select class="form-select form-select-sm discounttype calc p-1">
                                    <option value="1" ${(item.DiscountType ?? 1) == 1 ? "selected" : ""}>%</option>
                                    <option value="2" ${(item.DiscountType ?? 1) == 2 ? "selected" : ""}>Flat</option>
                                </select>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountamount p-1 calc"
                                       value="${item.DiscountAmount ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end taxable p-1 calc"
                                       value="${item.Taxable ?? 0}">
                            </td>

                            <td class="col-gst p-1">
                                <select class="form-select form-select-sm gst calc p-1">
                                    ${gstOptions}
                                </select>
                            </td>


                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end cgst p-1"
                                       value="${(item.CGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end sgst p-1"
                                       value="${(item.SGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end igst p-1"
                                       value="${(item.IGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end fw-bold total p-1"
                                       value="${(item.Total ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="text-center p-1">
                                <i class="fa fa-times text-danger btn-delete-row" role="button"></i>
                            </td>
                        </tr>
                        `;

                    $('#invoiceItemsBody').append(row);
                });

                updateRowSequence();
                recalcAll();
                $('#advancePaid').val(data.PaidAmount).attr('readonly', true);
            } else {
                addItemRow(); // empty invoice
            }
            /* ---------- ITEMS ---------- */
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Invoice by BookingId Failed");
        }
    });
}
//proforma & invoice -- fetch -- END

//FOR SUMMARY -- START
function loadBookingCalendar(bookingId = 0, banquetId = 0, eventDate = null, actionMode = "singleData") {
    var requestData = {
        BookingId: bookingId,
        BanquetId: banquetId,
        EventDate: eventDate
    };

    $.ajax({
        url: '/api/BookingCalendar/GetBookingCalendar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.status === "Success") {
                var data = response.data;
                console.log("Booking List:",data);
                if (actionMode === "singleData") {
                    //resetBooking();
                    data = data[0];
                    //console.log("singleData - Booking API Response:", data);
                    $("#bookingReport_BookingId").val(bookingId);
                    //Client Details
                    $("#bookingReport_clientName").text(data.Client_Name || "—");
                    $("#bookingReport_clientContact").text(data.Client_Contact || "—");
                    $("#bookingReport_clientGSTIN").text(data.Client_GSTIN || "—");
                    $("#bookingReport_clientEmail").text(data.Client_Email || "—");
                    $("#bookingReport_clientBookedBy").text(data.Booked_By_EmpName || "—");
                    $("#bookingReport_internalOfficeNote").html(data.OtherCharges_InternalOfficeNote && data.OtherCharges_InternalOfficeNote.trim() !== ""
                        ? data.OtherCharges_InternalOfficeNote
                        : "— No internal notes available —"
                    );

                    $("#bookingReport_bookingNoteCustomer").html(data.OtherCharges_BookingNoteForCustomer && data.OtherCharges_BookingNoteForCustomer.trim() !== ""
                        ? data.OtherCharges_BookingNoteForCustomer
                        : "— No customer booking notes available —"
                    );
                    $("#VendorWorkOrder_Count").text(data.VendorWorkOrder_Count);

                    //Booking Details
                    $("#bookingReport_BookingRef").text(bookingId || "—");
                    $("#bookingReport_Invoice").text("#" + data.InvoiceNo);
                    $("#bookingReport_EventDate").text(data.FromDate_Full || "—");
                    $("#bookingReport_TillDate").text(data.TillDate_Full || "—");
                    $("#bookingReport_Slot").text(data.ShiftCode || "—");
                    $("#bookingReport_BookedOn").text(data.Booked_On || "—");
                    $("#bookingReport_EventName").text(data.Booking_EventName + "(" + data.Booking_SubEventName + ")" || "—");
                    $("#bookingReport_Banquet").text(data.BanquetPropertyName || "—");
                    $("#bookingReport_Hall").text(data.BanquetName || "—");
                    $("#bookingReport_Pax").text(data.Booking_PAX || "—");

                    vendorWorkOrder_eventDate = data.EventDate;
                    vendorWorkOrder_eventTime = data.StartTime;
                    vendorWorkOrder_slotId = data.SlotId;

                    // Services & Charges
                    $("#bookingReport_HallRent").text(data.Booking_HallRent || "0");
                    if (data.JSON_FoodPackage && data.JSON_FoodPackage.length > 0) {
                        $("#bookingReport_FoodAmount").text(data.JSON_FoodPackage[0].PackageCost || "0");
                        $("#bookingReport_FoodRate").text(data.JSON_FoodPackage[0].PerPlate || "—");
                        //here contnue for food label
                        // ✅ Store FULL package array in button as JSON
                        $("#btnFoodLabels").data("foodpackage", JSON.stringify(data.JSON_FoodPackage));
                        //$("#btnFoodLabels").prop("readonly", false);
                    }
                    else {
                        $("#bookingReport_FoodAmount").text("0");
                        $("#bookingReport_FoodRate").text("—");
                        //$("#btnFoodLabels").prop("readonly", true);
                    }

                    // Payment Trail
                    $("#bookingReport_DealAmount").text(data.PaymentSummary_Total || "0");
                    var taxAmount = (data.PaymentSummary_CGST + data.PaymentSummary_SGST + data.PaymentSummary_IGST);
                    $("#bookingReport_TaxAmount").text(taxAmount || "0");
                    $("#bookingReport_BookingDiscount").text(data.PaymentSummary_BookingDiscount_Total || "0");
                    $("#bookingReport_TotalAmount").text(data.PaymentSummary_PayableAmount || "0");
                    $("#bookingReport_PaidTillDate").text(data.Payment_Amount || "0");
                    $("#bookingReport_Discount").text(data.PaymentSummary_BookingDiscount_Type_Amount || "0");
                    $("#bookingReport_OutstandingAmount").text((data.PaymentSummary_PayableAmount - data.Payment_Amount) || "0");

                    //Payment Details Table
                    fillPaymentTrailTable(data.PaymentTrailList, bookingId);

                    // Food Package and Selected Menu
                    $("#bookingReport_FoodPackageName").text(data?.JSON_FoodPackage?.[0]?.FoodPackageName ?? "—");

                    $("#bookingReport_FoodPackageUpdatedBy").text(data?.Booked_By_EmpName || "—");
                    $("#bookingReport_FoodPackageUpdatedOn").text(data?.Booked_On || "—");

                    // $("#bookingReport_ReachedUs").text(data?.ReachedUs || "—");
                    // $("#bookingReport_Reference").text(data?.Reference || "—");

                    //Open REPORT MODAL
                    bookingReportModal.show();
                }
            }
            else {
                //console.warn("API returned error:", response.message);
                Swal.fire({
                    icon: 'warning',
                    title: 'No Data Found',
                    text: response.message || 'No bookings available.',
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "getBooking fetch fail")

        }
    });
}
// Fill Payment Trail dynamically
function fillPaymentTrailTable(paymentList = [], bookingId) {
    const tbody = $("#bookingReport_PaymentTrailBody");
    tbody.empty(); // Clear old data

    if (!paymentList || paymentList.length === 0) {
        tbody.append(`
                <tr>
                    <td colspan="13" class="text-muted small fst-italic text-center">
                        No payment records found
                    </td>
                </tr>
            `);
        return;
    }

    //console.log("paymentList:", paymentList);

    paymentList.forEach((item, index) => {
        tbody.append(`
                <tr>
                    <td class="text-center">${index + 1}</td>
                    <td class="text-center"><span class="fw-semibold text-dark">#${item.PaymentId || "—"}</span></td>
                    <td class="text-center"><span class="badge bg-success-subtle text-success border">${item.ModeName || "—"}</span></td>
                    <td class="text-center">${item.DateOfPayment2 || "—"}</td>
                    <td class="text-center">${item.TxnId || "—"}</td>
                    <td class="text-center">${item.ChequeBank || "—"}</td>
                    <td class="text-center">${item.ChequeNo || "—"}</td>
                    <td class="text-center">${item.ChequeDate2 || "—"}</td>
                    <td class="text-success fw-semibold text-center">₹${formatINR(item.Amount) || "0"}</td>
                    <td class="text-danger fw-semibold text-center">₹${formatINR(item.DueBalance) || "0"}</td>
                    <td class="text-muted small text-center">
                        ${item.PaymentNotes
                        ? `<span class="text-primary cursor-pointer" title="${item.PaymentNotes}">
                                            View Notes
                                        </span>`
                        : "—"
                    }
                    </td>
                    <td class="text-center">
                        <button class="btn btn-light btn-xs border btn-payment-download" title="View Receipt" data-id="${item.PaymentId}" data-bookingid="${bookingId}">
                            <i class="fa-solid fa-file-invoice text-primary"></i>
                        </button>
                    </td>
                </tr>
            `);
    });
}

//FOR SUMMARY -- END

function calculateInvoiceTotals() {
    let subTotal = 0, totalGST = 0, grandTotal = 0;

    invoiceItems.forEach(x => {
        subTotal += x.SubTotal;
        totalGST += (x.CGST + x.SGST);
        grandTotal += x.Total;
    });

    $('#subTotal').val(subTotal.toFixed(2));
    $('#totalGST').val(totalGST.toFixed(2));
    // Call after setting GST
    updateInvoiceTitle(totalGST);
    $('#grandTotal').val(grandTotal.toFixed(2));
}

function getNextInvoiceNumbers() {

    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoice_NextInvoiceNumbers',
        type: 'GET',
        success: function (res) {
            if (res.status === 'Success' && res.data) {
                invoiceNo = res.data.NextInvoiceNo;
                proformaInvoiceNo = res.data.NextProformaInvoiceNo;
                gstInvoiceNo = res.data.NextGSTInvoiceNo;

                //console.log('Invoice No:', invoiceNo);
                //console.log('Proforma Invoice No:', proformaInvoiceNo);
                //console.log('GST Invoice No:', gstInvoiceNo);

                // OPTIONAL: Assign to inputs
                //$('#invoiceNo').val(invoiceNo);
                //$('#proformaInvoiceNo').val(proformaInvoiceNo);
                //$('#gstInvoiceNo').val(gstInvoiceNo);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Invoice Numbers Failed");
        }
    });
}

function fetchData(isGenerated) {
    //console.log("isGenerated:", isGenerated);
    // 🔥 session-based override
    let isInvoiceGenerated = null;

    // session override
    if (isGenerated === true) {
        isInvoiceGenerated = true; // ✔ boolean
    }
    else {
        const val = $('#fltIsInvoiceGenerated').val();

        if (val === "") {
            isInvoiceGenerated = null;
        } else {
            isInvoiceGenerated = val === "true"; // ✔ boolean
        }
    }
   
    const isExclusiveOfGST = null; // $('#isExclusiveOfGST').val() === 'true' || $('#isExclusiveOfGST').val() === '1';
    const isInclusiveOfGST = null; // $('#isInclusiveOfGST').val() === 'true' || $('#isInclusiveOfGST').val() === '1';

    const payload = {
        BookingId: 0,
        NameEmailContact: $('#fltSearch').val() || null,
        SlotId: $('#fltSlot').val() || 0,
        EventFrom: $('#fltEventFrom').val() || null,
        EventTo: $('#fltEventTo').val() || null,
        BookedFrom: $('#fltBookedFrom').val() || null,
        BookedTill: $('#fltBookedTill').val() || null,
        SubEventId: $('#fltEvent').val() || 0,
        PaymentStatusId: $('#fltPaymentStatus').val() || 0,
        IsInvoiceGenerated: isInvoiceGenerated,
        IsExclusiveOfGST: isExclusiveOfGST,
        IsInclusiveOfGST: isInclusiveOfGST,
        FilterDuration: selectedMonthDuration || null
    };
    console.log("GRID payload:", payload);

    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoice_GRID',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("GRID res:", res);
            if (res.status !== "Success") {
                Swal.fire({
                    icon: "warning",
                    title: "Notice",
                    text: res.message
                });
                return;
            }

            const rows = res.data?.Rows || [];
            const totals = res.data?.Totals || {};

            renderTable(rows, totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "CustomerInvoice Fetch Failed!");
        }
    });
}

function renderTable(data, totals) {
    //console.log("render data:", data);
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);
    //const isSuperUser = sessionStorage.getItem("isSuperUser") === "true";

    const hasValidNo = v => v !== null && v !== undefined && Number(v) > 0;

    $('#tableMain table').DataTable({
        data: data,
        responsive: true,
        pageLength: 20,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false,

        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        rowCallback: function (row, data) {
            if (Number(data.OutstandingAmount) > 0) {
                $(row).addClass('row-outstanding');
            }
        },
        columns: [
            { data: 'srno', visible: false },
            { data: 'BookingId', title: 'B.No', name: 'BookingId' },
            //InvoiceNo / ProformaInvoiceNo / GST No
            {
                data: null,
                title: 'Invoice No',
                name: 'InvoiceNo',
                render: function (row) {

                    const invoiceNo = row.FinalInvoiceNo !== null && row.FinalInvoiceNo !== undefined && row.FinalInvoiceNo !== "0"
                        ? row.FinalInvoiceNo
                            : null;

                    if (invoiceNo) {
                        return `<strong>${invoiceNo}</strong>`;
                    }

                    return '-';
                }
            },

            { data: 'EventDate_Format', title: 'Event Date', name:'EventDate_Format' },
            { data: 'BookedOn', title: 'Booked On', name: 'BookedOn', visible: false },
            { data: 'EventName', title: 'Event (PAX)', name: 'EventName' },
            { data: 'HallName', title: 'Banquet', name: 'HallName' },
            { data: 'ClientName', title: 'Client', name: 'ClientName' },
            { data: 'ClientContactNo', title: 'Contact', name: 'ClientContactNo' },
            //TotalAmount
            {
                data: 'TotalAmount',
                title: 'Total Amt.',
                className: 'text-end',
                name: 'TotalAmount',
                width: "80px",
                render: d => `₹ ${Number(d).toLocaleString('en-IN')}`
            },
            //OutstandingAmount
            {
                data: null,
                title: 'Outstanding Amt.',
                className: 'text-end',
                name: 'OutstandingAmount',
                render: function (d) {

                    let outstanding = d.OutstandingAmount ?? 0;
                    let formatted = Number(outstanding).toLocaleString('en-IN');

                    if (outstanding > 0) {
                        return `
                            <button class="btn btn-sm btn-action-xss btn-outline-danger btn-outstanding"
                                    data-bookingid="${d.BookingId}"
                                    data-outstandingamount="${outstanding}">
                                ₹ ${formatted}
                            </button>
                        `;
                    }

                    return `<span class="text-muted">₹ ${formatted}</span>`;
                }
            },
            //Summary
            {
                data: null,
                title: 'Summary',
                name: 'Summary',
                render: row =>
                    `<button class="btn btn-primary btn-action-xs2 btn-summary"
                         data-bookingid="${row.BookingId}">
                    Summary
                </button>`
            },
            //Proforma Invoice - created/edit
            {
                data: null,
                title: 'Proforma Invoice',
                name: 'ProformaInvoice',
                render: function (row) {

                    if (!hasValidNo(row.ProformaInvoiceNo)) {

                        if (row.IsInvoiceBlocked) {
                            return `
                                <span title="You can create Proforma only on or after the event date.">
                                    <button class="btn btn-secondary btn-action-xs2"
                                            data-bookingid="${row.BookingId}"
                                            disabled>
                                        Create Proforma
                                    </button>
                                </span>
                            `;
                        }

                        return `
                            <button class="btn btn-secondary btn-action-xs2 btn-create-proforma"
                                    data-bookingid="${row.BookingId}">
                                Create Proforma
                            </button>
                        `;
                    }

                    return `
                        <button class="btn btn-outline-warning btn-action-xs2 btn-edit-proforma"
                                data-bookingid="${row.BookingId}"
                                data-proformano="${row.ProformaInvoiceNo}">
                            <i class="fa fa-pen me-1"></i>Edit
                        </button>
                    `;
                }
            },
            //Send Proforma
            {
                data: null,
                title: 'Send Proforma',
                name: 'SendProforma',
                render: row =>
                    hasValidNo(row.ProformaInvoiceNo)
                        ? `<button class="btn btn-outline-primary btn-action-xs2 btn-send-invoice"
                       data-bookingid="${row.BookingId}"
                       data-invoiceno=""
                       data-proformano="${row.ProformaInvoiceNo}">
                    <i class="fa fa-paper-plane me-1"></i>Send
                </button>`
                        : '-'
            },
            //PDF (Proforma)
            {
                data: null,
                title: 'PDF (Proforma)',
                name: 'PDFProforma',
                className: 'text-center',
                render: row =>
                    !hasValidNo(row.ProformaInvoiceNo)
                        ? '-'
                        : `<i class="fa fa-file-pdf text-danger fs-8 btn-pdf-proforma"
                               role="button"
                               data-bookingid="${row.BookingId}"
                               data-proformano="${row.ProformaInvoiceNo}">
                           </i>`                
            },
            //GST Invoice - created/edit    
            {
                data: null,
                title: 'GST Invoice',
                name: 'GSTInvoice',
                render: function (row) {
                    const invoiceNo =
                        row.InvoiceNo !== null && row.InvoiceNo !== undefined && row.InvoiceNo > 0
                            ? row.InvoiceNo
                            : null;

                    const gstInvoiceNo =
                        row.GSTInvoiceNo !== null && row.GSTInvoiceNo !== undefined && row.GSTInvoiceNo !== ''
                            ? row.GSTInvoiceNo
                            : null;

                    if (invoiceNo) {
                        return `
                            <button class="btn btn-outline-warning btn-action-xs2 btn-edit-invoice"
                                    data-finalamount="${row.TotalAmount}"
                                    data-bookingid="${row.BookingId}"
                                    data-invoiceno="${row.InvoiceNo}">
                                <i class="fa fa-pen me-1"></i>Edit
                            </button>
                        `;
                    }

                    if (gstInvoiceNo) {
                        return `
                            <button class="btn btn-outline-warning btn-action-xs2 btn-edit-gstinvoice"
                                    data-finalamount="${row.TotalAmount}"
                                    data-bookingid="${row.BookingId}"
                                    data-gstinvoiceno="${row.GSTInvoiceNo}">
                                <i class="fa fa-pen me-1"></i>Edit
                            </button>
                        `;
                    }

                    if (!hasValidNo(row.InvoiceNo) && !gstInvoiceNo) {

                        if (row.IsInvoiceBlocked) {
                            return `
                                <span title="You can create Invoice only on or after the event date.">
                                    <button class="btn btn-success btn-action-xs2 mb-1"
                                            data-finalamount="${row.TotalAmount}"
                                            data-bookingid="${row.BookingId}" data-isinclusivegst="false"
                                            disabled>
                                        Create Invoice (Rates Exclusive of GST)
                                    </button>
                                    <button class="btn btn-warning btn-action-xs2 mb-1"
                                            data-finalamount="${row.TotalAmount}"
                                            data-bookingid="${row.BookingId}" data-isinclusivegst="true"
                                            disabled>
                                        Create Invoice (Rates Inclusive of GST)
                                    </button>
                                </span>
                            `;
                        }

                        return `
                            <button class="btn btn-success btn-action-xs2 mb-1 btn-create-invoice"
                                    data-finalamount="${row.TotalAmount}"
                                    data-bookingid="${row.BookingId}" data-isinclusivegst="false">
                                Create Invoice (Rates Exclusive of GST)
                            </button>
                            <button class="btn btn-warning btn-action-xs2 mb-1 btn-create-invoice"
                                    data-finalamount="${row.TotalAmount}"
                                    data-bookingid="${row.BookingId}" data-isinclusivegst="true">
                                Create Invoice (Rates Inclusive of GST)
                            </button>
                        `;
                    }
                }
            },
            //Send GST Invoice
            {
                data: null,
                title: 'Send Invoice',
                name: 'SendInvoice',
                render: row =>
                    hasValidNo(row.InvoiceNo)
                        ? `<button class="btn btn-outline-primary btn-action-xs2 btn-send-invoice"
                       data-bookingid="${row.BookingId}"
                       data-invoiceno="${row.InvoiceNo}"
                       data-proformano="">
                    <i class="fa fa-paper-plane me-1"></i>Send
                </button>`
                        : '-'
            },
            //PDF (GST Invoice)
            {
                data: null,
                title: 'PDF (GST)',
                name: 'PDFGST',
                className: 'text-center',
                render: row =>
                    !hasValidNo(row.InvoiceNo)
                        ? '-'
                        : `<i class="fa fa-file-pdf text-danger fs-8 btn-pdf-invoice"
                               role="button"
                               data-bookingid="${row.BookingId}"
                               data-invoiceno="${row.InvoiceNo}">
                           </i>`
            },
        ],

        footerCallback: function (row, data, start, end, display) {
            const api = this.api();

            const format = val => `₹ ${Number(val || 0).toLocaleString('en-IN')}`;

            $(api.column('TotalAmount:name').footer()).html(format(totals.TotalAmount));
            $(api.column('OutstandingAmount:name').footer()).html(format(totals.TotalOutstanding));
        }        
    }); 
}
/*
function isPastDate(dateString) {
    if (!dateString) return false;

    const eventDate = new Date(dateString);
    const today = new Date();

    // Remove time portion
    today.setHours(0, 0, 0, 0);
    eventDate.setHours(0, 0, 0, 0);

    return eventDate < today;
}*/

function resetForm() {
    $("#btnAdjustDiff").addClass("d-none");

    invoiceType = null;
    $('#invoiceId').val(0);
    $('#bookingId').val(0);
    $('#invoiceNo').val('').removeData('isinclusivegst');
    $('#gstInvoiceNo').val('');
    $('#proformaInvoiceNo').val('');
    fixedSubTotal = 0;

    $('#isExclusiveOfGST').val(false);
    $('#isInclusiveOfGST').val(false);

    $('#customerName').val('');
    $('#gstin').val('');
    $('#emailId').val('');
    $('#address').val('');

    $('#invoicePayment_Mode, #invoicePayment_Account').val('0');
    $('#invoicePayment_Txn, #invoicePayment_ChequeBank, #invoicePayment_ChequeNo, #invoicePayment_ChequeDate, #invoicePayment_NextDueDate').val('');
    $('#invoicePayment_Amount, #invoicePayment_originalDueAmount').val(0);
    $('#invoicePayment_DueBalance').val("0.00").prop('readonly', false);
    $('#advancePaid').val("0.00").attr('readonly', true);

    $('#subTotal, #bookingDiscount, #totalTaxable, #totalGST, #grandTotal, #roundOff, #balanceAmount').val('0.00');

    $('#invoiceItemsBody').empty();
    $('#btnSaveInvoice').prop('disabled', false);

    const now = new Date();
    // Format Date → YYYY-MM-DD
    const date = now.toISOString().split("T")[0];
    // Format Time → HH:MM (24hr)
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const time = `${hours}:${minutes}`;
    // Set values
    $('#invoiceDate').val(date);
    $('#invoicePayment_Date').val(date);
    $('#invoicePayment_Time').val(time);
}

function resetFilter() {

    $('#fltSearch').val('');
    $('#fltSlot').val('0');
    $('#fltEventFrom').val('');
    $('#fltEventTo').val('');
    $('#fltBookedFrom').val('');
    $('#fltBookedTill').val('');
    $('#fltEvent').val('0');

    //console.log('Filters reset');
    fetchData();
}

//Event - dropdown -- START ----------------------
function loadSubEventList() {
    //showLoader();
    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeList", // API endpoint
        type: "POST", // POST request
        contentType: "application/json", // Send data as JSON
        data: JSON.stringify({ EventTypeId: 0 }), // Request body
        success: function (response) {
            //console.log("event subtype list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
                $("#fltEvent").html(options);
            } else {
                $("#fltEvent").html(`<option value="0">No Event Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Event list")
        }
    });
}
//Event - dropdown -- END ------------------------

//SLOT DROPDOWN - dropdown -- START ------------------------
function loadSlotDropdown() {
    var payload = { Id: 0, IsActive: true };
    $.ajax({
        url: "/api/SlotMaster/GetSlotList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);
            const $slotDropdowns = $("#fltSlot");
            $slotDropdowns.html(`<option value="0">-- Select --</option>`);
            response.data.forEach(s => {
                $slotDropdowns.append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Slot Fetch Error");
        }
    });
}
//SLOT DROPDOWN -- END ------------------------

//STATE DROPDOWN --- START ------------------------
function loadState() {
    $.ajax({
        url: "/api/State/GetStateList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //console.log("response state:", response);
            const $stateDropdowns = $("#customerState");
            $stateDropdowns.html(`<option value="0">-- Select --</option>`);
            response.data.forEach(s => {
                $stateDropdowns.append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "State Fetch Error");
        }

    });
}    
//STATE DROPDOWN --- END ------------------------

function loadPaymentCollection(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: "/api/CustomerInvoice/GetDashboardPaymentCollectionStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res count PaymentCollection finance:", res);
            if (res.status === "Success" && res.data) {
                //PAYMENT COLLECTION
                const paid = res.data.PaidAmount;
                const pending = res.data.PendingAmount;
                const total = res.data.TotalAmount || 1; // avoid divide by zero

                $("#txtPaidAmount").text(formatINR(paid));
                $("#txtPendingAmount").text(formatINR(pending));

                $("#barPaid").css("width", calcPercent(paid, total) + "%");
                $("#barPending").css("width", calcPercent(pending, total) + "%");
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load payment collections!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment Collection Fetch Error");
        }
    });
}
function loadInvoiceStatus(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("invoice status payload:", payload);
    $.ajax({
        url: "/api/CustomerInvoice/GetDashboardInvoiceStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res count invoice status finance:", res);
            if (res.status === "Success" && res.data) {
                //INVOICE
                const invoiceGeneratedCount = res.data.InvoiceGeneratedCount;
                const invoicePendingCount = res.data.InvoicePendingCount;
                const invoiceTotalCount = res.data.InvoiceTotalCount || 1; // avoid divide by zero

                $("#txtInvoiceGeneratedCount").text(formatINR(invoiceGeneratedCount));
                $("#txtInvoicePending").text(formatINR(invoicePendingCount));

                $("#bar_InvoiceGenerated").css("width", calcPercent(invoiceGeneratedCount, invoiceTotalCount) + "%");
                $("#bar_InvoicePending").css("width", calcPercent(invoicePendingCount, invoiceTotalCount) + "%");
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load invoice status!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Invoice Status Fetch Error");
        }
    });
}

//DUE PAYMENT -- START -----------------------

function loadAccountDropdown() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {
            const ddl = $("#outstandingPayment_Account, #invoicePayment_Account");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data.forEach(acc => {
                    ddl.append(`<option value="${acc.Id}">${acc.AccountName}</option>`);
                });
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load accounts!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Account Dropdown Error");
        }
    });
}
function loadPaymentModeDropdown() {
    const payload = {
        Id: 0
    };
    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("payment mode res:", res);
            const ddl = $("#outstandingPayment_Mode, #invoicePayment_Mode");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data
                    .filter(x => x.IsActive)
                    .forEach(pm => {
                        ddl.append(`<option value="${pm.Id}">${pm.PaymentModeName}</option>
                        `);
                    });
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load payment modes!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment Mode Dropdown Error");
        }
    });
}

function loadPaymentHistory(bookingId) {
    const payload = {
        PaymentId: 0,
        BookingId: bookingId
    };
    //console.log("loadPaymentHistory payload:", payload);
    $.ajax({
        url: "/api/Booking_Payments/GetBookingPayments",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("Booking_Payments res:", res);
            if (res?.status === "Success") {
                // HIDE download Alll btn
                $(".btn-payment-download-all").addClass("d-none");
                if (res.data.length > 0) {
                    // SHOW download Alll btn
                    $(".btn-payment-download-all").removeClass("d-none");
                }
                $("#paymentModal").modal("show");
                renderPaymentTable( (res.data || []) , bookingId);
            } else {
                Swal.fire("Error", res?.message || "Failed to fetch payments!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Payments Error");
        }
    });
}

// -- RENDER TABLE ---
function renderPaymentTable(data, bookingId) {

    // Destroy existing DataTable if exists
    if ($.fn.dataTable && $.fn.dataTable.isDataTable("#tblPaymentHistory table")) {
        $("#tblPaymentHistory table").DataTable().destroy();
    }

    // Add serial numbers
    data.forEach((item, i) => item.srno = i + 1);
    $("#tblPaymentHistory table").DataTable({
        data: data,
        autoWidth: false,
        columns: [
            { data: "srno", title: "#" },
            { data: "TxnId", title: "Txn Id", className: "text-end" },
            { data: "PaymentDateTimeFormat", title: "Date & Time" },
            {
                data: "Amount",
                title: "Amount (₹)",
                className: "text-end",
                render: $.fn.dataTable.render.number(',', '.', 2, '₹ ')
            },
            {
                data: null,
                title: "Receipt",
                className: "text-center",
                render: function (row) {
                    return `<button class="btn btn-sm btn-outline-primary btn-payment-download" data-id="${row.PaymentId}" data-bookingid="${bookingId}">
                                <i class="fas fa-download"></i>
                            </button>`;
                }
            }
        ],
        pageLength: 5,
        ordering: true,
        searching: false
    });
}

// -- Download receipt - button click --
$(document).on("click", ".btn-payment-download", function () {
    let paymentId = $(this).data("id");
    let bookingId = $(this).data("bookingid");  //getBookingId();
    let actionBy = Number(sessionStorage.getItem("userId") ?? "0");

    var payload = {
        PaymentId: paymentId,
        BookingId: bookingId,
        ActionBy: actionBy
    };
    var url = "/api/Booking_Payments/DownloadPaymentReceipt";
    downloadReport(url, payload, "Generating Receipt...");
});

$(document).on("click", ".btn-payment-download-all", function () {
    let paymentId = 0;
    let bookingId = Number($("#outstanding_paymentBookingId").val() || 0);
    let actionBy = Number(sessionStorage.getItem("userId") ?? "0");

    var payload = {
        PaymentId: paymentId,
        BookingId: bookingId,
        ActionBy: actionBy
    };
    var url = "/api/Booking_Payments/DownloadPaymentReceipt";
    downloadReport(url, payload, "Generating Receipt...");
});

//DUE PAYMENT -- END -----------------------

