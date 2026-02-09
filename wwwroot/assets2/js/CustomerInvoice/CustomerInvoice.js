const bookingReportModalEl = document.getElementById("bookingReportModal");
const bookingReportModal = new bootstrap.Modal(bookingReportModalEl);
const invoiceModal = new bootstrap.Modal(document.getElementById('invoiceModal'));
var invoiceNo = 0;
var proformaInvoiceNo = 0;
var gstInvoiceNo = 0;
var invoiceType = null;

$(function () {
    fetchData();
    loadSubEventList();
    loadSlotDropdown();
    getNextInvoiceNumbers();
    loadState();

    //SUMMARY -- START 
    $(document).on('click', '.btn-summary', function () {
        var bookingId = $(this).data('bookingid');
        console.log('Summary BookingId:', bookingId);
        loadBookingCalendar(bookingId);
    });
    //SUMMARY -- END

    //CREATE PROFORMA INVOICE -- START
    $(document).on('click', '.btn-create-proforma', function () {
        resetForm();

        const bookingId = $(this).data('bookingid');
        console.log('Create Proforma BookingId:', bookingId);
        $('#bookingId').val(bookingId);

        invoiceType = "proforma";
        $('#invoiceModalTitle').text('Create Proforma Invoice #' + proformaInvoiceNo);
        invoiceModal.show();

        fetchInvoiceByBookingId(bookingId);
    });
    //CREATE PROFORMA INVOICE -- END

    //CREATE INVOICE -- START
    $(document).on('click', '.btn-create-invoice', function () {
        resetForm();

        const bookingId = $(this).data('bookingid');
        console.log('Create Invoice BookingId:', bookingId);
        $('#bookingId').val(bookingId);

        invoiceType = "invoice";
        $('#invoiceModalTitle').text('Create Invoice #' + invoiceNo);
        invoiceModal.show();

        fetchInvoiceByBookingId(bookingId);
    });
    //CREATE INVOICE -- END

    //EDIT INVOICE -- START
    $(document).on('click', '.btn-edit-invoice', function () {

        resetForm();

        const bookingId = $(this).data('bookingid');
        const invoiceNo = $(this).data('invoiceno');
        $('#bookingId').val(bookingId);

        invoiceType = "invoice";

        $('#invoiceModalTitle').text('Edit Invoice #' + invoiceNo);
        invoiceModal.show();

        fetchInvoiceById({
            BookingId: bookingId,
            InvoiceNo: invoiceNo,
            ProformaInvoiceNo: null
        });
    });
    //EDIT INVOICE -- END

    //EDIT PROFORMA INVOICE -- START
    $(document).on('click', '.btn-edit-proforma', function () {

        resetForm();

        const bookingId = $(this).data('bookingid');
        const proformaNo = $(this).data('proformano');
        $('#bookingId').val(bookingId);

        invoiceType = "proforma";

        $('#invoiceModalTitle').text('Edit Proforma #' + proformaNo);
        invoiceModal.show();

        fetchInvoiceById({
            BookingId: bookingId,
            InvoiceNo: null,
            ProformaInvoiceNo: proformaNo
        });
    });
    //EDIT PROFORMA INVOICE -- END

    //SEND INVOICE -- START
    $(document).on('click', '.btn-send-invoice', function () {
        var bookingId = $(this).data('bookingid');
        console.log('Send Invoice BookingId:', bookingId);
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
            WithHeader: true
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
            GSTInvoiceNo: invoiceNo,   // GST invoice
            WithHeader: true
        };
        generateInvoicePdf(request);
    });
    //PDF INVOICE -- END

    //ITEMS ROW --- START
    $('#btnAddItem').on('click', function () {
        addItemRow();
    });

    // On input change
    $(document).on('input', '.calc', function () {
        recalcRow($(this).closest('tr'));
        recalcAll();
    });

    // On GST switch toggle
    $('#isGstIncluded').on('change', function () {
        $('#invoiceItemsBody tr').each(function () {
            recalcRow($(this));
        });
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

                Total: parseFloat(row.find('.total').val()) || 0
            });
        });

        const isGstIncluded = $('#isGstIncluded').is(':checked');
        const request = {
            Id: parseInt($('#invoiceId').val()) || 0,
            BookingId: parseInt($('#bookingId').val()) || 0,
            InvoiceNo: invoiceType === "invoice" ? invoiceNo : null,
            ProformaInvoiceNo: invoiceType === "proforma" ? proformaInvoiceNo : null,
            GSTInvoiceNo: (invoiceType === "invoice" && isGstIncluded) ? gstInvoiceNo : null,

            CustomerName: $('#customerName').val(),
            InvoiceDate: $('#invoiceDate').val(),
            GSTIN: $('#gstin').val(),
            Address: $('#address').val(),
            EmailId: $('#emailId').val(),
            CustomerState: parseInt($('#customerState').val()) || null,

            SubTotal: parseFloat($('#subTotal').val()) || 0,
            BookingDiscount: parseFloat($('#bookingDiscount').val()) || 0,
            TotalTaxableAmount: parseFloat($('#totalTaxable').val()) || 0,
            TotalDiscountAcrossAllServices: 0,     // if not used
            TotalGST: parseFloat($('#totalGST').val()) || 0,
            TotalVAT: 0,
            GrandTotal: parseFloat($('#grandTotal').val()) || 0,
            RoundOff: parseFloat($('#roundOff').val()) || 0,
            TotalAdvancePaid: parseFloat($('#advancePaid').val()) || 0,
            BalanceAmount: parseFloat($('#balanceAmount').val()) || 0,

            JSON_Items: items,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        console.log("save payload:", request);

        $.ajax({
            url: '/api/CustomerInvoice/SaveCustomerInvoice', // adjust if needed
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function (res) {

                if (res.status === 'Success') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Saved',
                        text: 'Invoice saved successfully',
                        confirmButtonText: 'OK'
                    });
                    fetchData();
                    getNextInvoiceNumbers();
                    //resetForm();
                    $('#btnSaveInvoice').prop('disabled', true);
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
});
function addItemRow() {
    const row = `
        <tr>
            <td class="text-center col-index p-1"></td>

            <td class="col-desc p-1">
                <input type="text" class="form-control form-control-sm desc">
            </td>

            <td class="col-hsn p-1">
                <input type="text" class="form-control form-control-sm hsn">
            </td>

            <td class="col-rate p-1">
                <input type="number" class="form-control form-control-sm text-end rate calc" value="0">
            </td>

            <td class="col-qty p-1">
                <input type="number" class="form-control form-control-sm text-end qty calc" value="1">
            </td>

            <td class="col-subtotal p-1">
                <input type="number" class="form-control form-control-sm text-end subtotal calc" value="0">
            </td>

            <td class="col-discount-rate p-1">
                <input type="number" class="form-control form-control-sm text-end discountrate calc" value="0">
            </td>

            <td class="col-discount-type p-1">
                <input type="number" class="form-control form-control-sm text-end discounttype calc" value="0">
            </td>

            <td class="col-discount-amount p-1">
                <input type="number" class="form-control form-control-sm text-end discountamount calc" value="0">
            </td>

            <td class="col-taxable p-1">
                <input type="number" class="form-control form-control-sm text-end taxable calc" value="0">
            </td>

            <td class="col-gst p-1">
                <input type="number" class="form-control form-control-sm text-end gst calc" value="0">
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end cgst" readonly>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end sgst" readonly>
            </td>

            <td class="col-total p-1">
                <input type="number" class="form-control form-control-sm text-end fw-bold total" readonly>
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
function recalcRow(row) {

    const rate = parseFloat(row.find('.rate').val()) || 0;
    const qty = parseFloat(row.find('.qty').val()) || 0;
    const gstRate = parseFloat(row.find('.gst').val()) || 0;

    const isGstIncluded = $('#isGstIncluded').is(':checked');
    const gstFactor = 1 + (gstRate / 100);

    let taxable = 0;
    let gstAmount = 0;

    if (isGstIncluded) {
        // Rate already includes GST
        const gross = rate * qty;
        taxable = gross / gstFactor;
        gstAmount = gross - taxable;
    } else {
        // GST added on top
        taxable = rate * qty;
        gstAmount = taxable * gstRate / 100;
    }

    const cgst = gstAmount / 2;
    const sgst = gstAmount / 2;
    const total = taxable + gstAmount;

    row.find('.cgst').val(cgst.toFixed(2));
    row.find('.sgst').val(sgst.toFixed(2));
    row.find('.total').val(total.toFixed(2));
}
function recalcAll() {

    let subTotal = 0;
    let totalGST = 0;

    const isGstIncluded = $('#isGstIncluded').is(':checked');

    $('#invoiceItemsBody tr').each(function () {

        const row = $(this);
        const rate = parseFloat(row.find('.rate').val()) || 0;
        const qty = parseFloat(row.find('.qty').val()) || 0;
        const gstRate = parseFloat(row.find('.gst').val()) || 0;

        const gstFactor = 1 + (gstRate / 100);
        let taxable = 0;
        let gstAmount = 0;

        if (isGstIncluded) {
            const gross = rate * qty;
            taxable = gross / gstFactor;
            gstAmount = gross - taxable;
        } else {
            taxable = rate * qty;
            gstAmount = taxable * gstRate / 100;
        }

        subTotal += taxable;
        totalGST += gstAmount;
    });

    const discount = parseFloat($('#bookingDiscount').val()) || 0;
    const grandTotal = subTotal + totalGST - discount;

    $('#subTotal').val(subTotal.toFixed(2));
    $('#totalTaxable').val(subTotal.toFixed(2));
    $('#totalGST').val(totalGST.toFixed(2));
    $('#grandTotal').val(grandTotal.toFixed(2));

    const advance = parseFloat($('#advancePaid').val()) || 0;
    $('#balanceAmount').val((grandTotal - advance).toFixed(2));
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

            /* ---------- MASTER ---------- */
            $('#invoiceId').val(data.Id);
            $('#bookingId').val(data.BookingId);
            $('#customerName').val(data.CustomerName);
            $('#invoiceDate').val(data.InvoiceDate);
            $('#gstin').val(data.GSTIN);
            $('#emailId').val(data.EmailId);
            $('#address').val(data.Address);
            $('#customerState').val(data.CustomerStateId);

            /* ---------- TOTALS (DIRECT SET) ---------- */
            $('#subTotal').val(data.SubTotal);
            $('#bookingDiscount').val(data.BookingDiscount);
            $('#totalTaxable').val(data.TotalTaxableAmount);
            $('#totalGST').val(data.TotalGST);
            $('#grandTotal').val(data.GrandTotal);
            $('#roundOff').val(data.RoundOff);
            $('#advancePaid').val(data.TotalAdvancePaid);
            $('#balanceAmount').val(data.BalanceAmount);

            /* ---------- ITEMS ---------- */
            $('#invoiceItemsBody').empty();

            if (data.Items && data.Items.length > 0) {

                data.Items.forEach(item => {

                    const row = `
                        <tr>
                            <td class="text-center col-index"></td>

                            <td><input class="form-control form-control-sm desc" value="${item.Description ?? ''}"></td>
                            <td><input class="form-control form-control-sm hsn" value="${item.HSN ?? ''}"></td>

                            <td><input class="form-control form-control-sm text-end rate" value="${item.Rate}"></td>
                            <td><input class="form-control form-control-sm text-end qty" value="${item.Qty}"></td>
                            <td><input class="form-control form-control-sm text-end subtotal" value="${item.SubTotal}"></td>

                            <td><input class="form-control form-control-sm text-end discountrate" value="${item.DiscountRate}"></td>
                            <td><input class="form-control form-control-sm text-end discounttype" value="${item.DiscountTypeId}"></td>
                            <td><input class="form-control form-control-sm text-end discountamount" value="${item.DiscountAmount}"></td>

                            <td><input class="form-control form-control-sm text-end taxable" value="${item.TaxableAmount}"></td>
                            <td><input class="form-control form-control-sm text-end gst" value="${item.GSTRate}"></td>

                            <td><input class="form-control form-control-sm text-end cgst" value="${item.CGST}" readonly></td>
                            <td><input class="form-control form-control-sm text-end sgst" value="${item.SGST}" readonly></td>
                            <td><input class="form-control form-control-sm text-end fw-bold total" value="${item.Total}" readonly></td>

                            <td class="text-center">
                                <i class="fa fa-times text-danger btn-delete-row"></i>
                            </td>
                        </tr>
                    `;

                    $('#invoiceItemsBody').append(row);
                });

                updateRowSequence();
            }
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Invoice Failed");
        }
    });
}
//proforma & invoice -- fetch EDIT -- END

//proforma & invoice -- fetch -- START
function fetchInvoiceByBookingId(bookingId) {

    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoiceByBookingId', // adjust if needed
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ BookingId: bookingId }),
        success: function (res) {
            console.log("fetch ByBookingId res:", res);
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

            $('#isGstIncluded').prop('checked', data.IsGSTIncluded === true);

            $('#invoiceItemsBody').empty();

            if (data.ItemsJson && data.ItemsJson.length > 0) {

                data.ItemsJson.forEach(item => {

                    const taxable = parseFloat(item.Taxable) || 0;
                    const gstPercent = taxable > 0 && item.GSTRate ? ((item.GSTRate / taxable) * 100).toFixed(2) : 0;

                    const row = `
                        <tr>
                            <td class="text-center col-index p-1"></td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm desc"
                                       value="${item.Item_Description ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm hsn"
                                       value="${item.HSN ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end rate calc"
                                       value="${item.Rate ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end qty calc"
                                       value="${item.Qty ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end subtotal"
                                       value="${item.SubTotal ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountrate"
                                       value="${item.DiscountRate ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discounttype"
                                       value="${item.DiscounType ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountamount"
                                       value="${item.DiscountAmount ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end taxable"
                                       value="${item.Taxable ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end gst"
                                       value="${gstPercent}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end cgst"
                                       value="${(item.CGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end sgst"
                                       value="${(item.SGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end fw-bold total"
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
                //console.log("Booking List:",data);
                if (actionMode === "singleData") {
                    //resetBooking();
                    data = data[0];
                    console.log("singleData - Booking API Response:", data);
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
                    $("#bookingReport_EventName").text(data.Booking_TypeName + "(" + data.Booking_SubEventName + ")" || "—");
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
                        //$("#btnFoodLabels").prop("disabled", false);
                    }
                    else {
                        $("#bookingReport_FoodAmount").text("0");
                        $("#bookingReport_FoodRate").text("—");
                        //$("#btnFoodLabels").prop("disabled", true);
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
                    // fillPaymentTrailTable(data.PaymentTrailList);

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
                console.warn("API returned error:", response.message);
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

                console.log('Invoice No:', invoiceNo);
                console.log('Proforma Invoice No:', proformaInvoiceNo);
                console.log('GST Invoice No:', gstInvoiceNo);

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

function fetchData() {
    const payload = {
        BookingId: 0,
        NameEmailContact: $('#fltSearch').val() || null,
        SlotId: $('#fltSlot').val() || 0,
        EventFrom: $('#fltEventFrom').val() || null,
        EventTo: $('#fltEventTo').val() || null,
        BookedFrom: $('#fltBookedFrom').val() || null,
        BookedTill: $('#fltBookedTill').val() || null,
        SubEventId: $('#fltEvent').val() || 0
    };
    console.log("payload:", payload);
    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoice_GRID',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("fetch res:", res);
            renderTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "CustomerInvoice Fetch Failed!");
        }
    });

}

function renderTable(data) {
    console.log("render data:", data);
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    const hasValidNo = v => v !== null && v !== undefined && Number(v) > 0;
    $('#tableMain table').DataTable({
        data: data,
        responsive: true,
        pageLength: 20,
        searching: true,
        ordering: true,
        lengthChange: false,
        rowCallback: function (row, data) {
            if (Number(data.OutstandingAmount) > 0) {
                $(row).addClass('row-outstanding');
            }
        },
        columns: [
            { data: 'BookingId', title: '#BRN' },

            {
                data: null,
                title: 'Invoice No',
                render: function (row) {

                    const invoiceNo =
                        row.InvoiceNo !== null && row.InvoiceNo !== undefined && row.InvoiceNo > 0
                            ? row.InvoiceNo
                            : null;

                    const proformaNo =
                        row.ProformaInvoiceNo !== null && row.ProformaInvoiceNo !== undefined && row.ProformaInvoiceNo > 0
                            ? row.ProformaInvoiceNo
                            : null;

                    if (invoiceNo && proformaNo) {
                        return `<strong>${invoiceNo} / ${proformaNo}</strong>`;
                    }

                    if (invoiceNo) {
                        return `<strong>${invoiceNo}</strong>`;
                    }

                    if (proformaNo) {
                        return `<strong>${proformaNo}</strong>`;
                    }

                    return '-';
                }
            },

            { data: 'EventDate', title: 'Event Date' },
            { data: 'BookedOn', title: 'Booked On' },
            { data: 'EventName', title: 'Event' },
            { data: 'HallName', title: 'Banquet' },
            { data: 'ClientName', title: 'Client' },
            { data: 'ClientContactNo', title: 'Contact' },

            {
                data: 'TotalAmount',
                title: 'Total Amt.',
                className: 'text-end',
                render: d => `₹ ${Number(d).toLocaleString('en-IN')}`
            },

            {
                data: 'OutstandingAmount',
                title: 'Outstanding Amt.',
                className: 'text-end',
                render: d => `₹ ${Number(d).toLocaleString('en-IN')}`
            },

            {
                data: null,
                title: 'Summary',
                render: row =>
                    `<button class="btn btn-primary btn-action-xs2 btn-summary"
                         data-bookingid="${row.BookingId}">
                    Summary
                </button>`
            },

            {
                data: null,
                title: 'Proforma Invoice',
                render: row =>
                    !hasValidNo(row.ProformaInvoiceNo)
                        ? `<button class="btn btn-secondary btn-action-xs2 btn-create-proforma"
                                   data-bookingid="${row.BookingId}">
                                Create Proforma
                           </button>`
                                    : `<button class="btn btn-outline-warning btn-action-xs2 btn-edit-proforma"
                                   data-bookingid="${row.BookingId}"
                                   data-proformano="${row.ProformaInvoiceNo}">
                                <i class="fa fa-pen me-1"></i>Edit
                           </button>`
            },

            {
                data: null,
                title: 'Send Proforma',
                render: row =>
                    `<span class="text-muted fst-italic">Coming Soon</span>`
            },

            {
                data: null,
                title: 'PDF (Proforma)',
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

            {
                data: null,
                title: 'GST Invoice',
                render: row =>
                    !hasValidNo(row.InvoiceNo)
                        ? `<button class="btn btn-success btn-action-xs2 mb-1 btn-create-invoice"
                                   data-bookingid="${row.BookingId}">Create Invoice
                           </button>`
                        : `<button class="btn btn-outline-warning btn-action-xs2 btn-edit-invoice"
                                   data-bookingid="${row.BookingId}"
                                   data-invoiceno="${row.InvoiceNo}">
                                <i class="fa fa-pen me-1"></i>Edit
                           </button>`           
            },

            {
                data: null,
                title: 'Send Invoice',
                render: row =>
                    `<span class="text-muted fst-italic">Coming Soon</span>`
            },

            {
                data: null,
                title: 'PDF (GST)',
                className: 'text-center',
                render: row =>
                    !hasValidNo(row.InvoiceNo)
                        ? '-'
                        : `<i class="fa fa-file-pdf text-danger fs-8 btn-pdf-invoice"
                               role="button"
                               data-bookingid="${row.BookingId}"
                               data-invoiceno="${row.InvoiceNo}">
                           </i>`
            }            
        ]
    });
}

function resetForm() {
    invoiceType = null;
    $('#invoiceId').val(0);
    $('#bookingId').val(0);
    $('#invoiceNo').val('');
    $('#invoiceDate').val('');
    $('#customerName').val('');
    $('#gstin').val('');
    $('#emailId').val('');
    $('#address').val('');
    $('#subTotal').val(0);
    $('#totalGST').val(0);
    $('#grandTotal').val(0);
    $('#invoiceItemsBody').empty();
    $('#btnSaveInvoice').prop('disabled', false);
}

function resetFilter() {

    $('#fltSearch').val('');
    $('#fltSlot').val('0');
    $('#fltEventFrom').val('');
    $('#fltEventTo').val('');
    $('#fltBookedFrom').val('');
    $('#fltBookedTill').val('');
    $('#fltEvent').val('0');

    console.log('Filters reset');
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
    var payload = { Id: 0 };
    $.ajax({
        url: "/api/BookingCalendar_VendorWorkOrder/GetBookingCalendar_VendorWorkOrder_SlotMaster",
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
            console.log("response state:", response);
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
