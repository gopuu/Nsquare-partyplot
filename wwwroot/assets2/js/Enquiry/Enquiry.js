//<!--MAIN-->

let userId = 0;
// Sample data (replace later with API fetch)
let processes = [];

//FOLLOWUP - START ------------------------------------------------------------------------------------------
function loadFollowUpTypes() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_Types",
        type: "GET",
        success: function (res) {
            console.log("res type:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlFollowUpType");
                ddl.empty();

                ddl.append('<option value="0">-- Select Type --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up types");
        }
    });
}
function loadFollowUpStatus() {
    $.ajax({
        url: '/api/StatusMaster/GetStatusMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Followup" }),
        success: function (res) {
            console.log("res status:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlStatus");
                ddl.empty();

                ddl.append('<option value="0">-- Select Status --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.StatusName}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up status");
        }
    });
}
function loadMoodList() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_MoodList",
        type: "GET",
        success: function (res) {
            console.log("res mood:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlMood");
                ddl.empty();

                ddl.append('<option value="0">-- Select Mood --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up mood list");
        }
    });
}

function loadFollowUpHistoryById(enquiryId) {
    const requestObj = {
        FollowUpId: 0,
        EnquiryId: enquiryId
    };
    console.log("GetEnquiryFollowUp payload:", requestObj);
    $.ajax({
        url: "/api/EnquiryFollowUp/GetEnquiryFollowUp",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (response) {
            console.log("GetEnquiryFollowUp response:", response);
            if (response.status.toLowerCase() === "success") {

                const enquiry = response.data.Enquiry;
                const followups = response.data.FollowUps;

                // -----------------------------
                // Bind enquiry Detail Section
                // -----------------------------
                $("#inqName").text(enquiry.Enquiry_Name || "-");
                $("#inqContact").text(enquiry.Enquiry_Contact || "-");
                let slot = enquiry.Enquiry_Slot || "-";
                $("#inqSlot").text(`${slot}`);

                $("#inqPax").text(enquiry.Enquiry_Pax || "-");

                // event
                let eventName = enquiry.Enquiry_EventName || "-";
                $("#inqEvent").text(`${eventName}`);

                // event date
                let eventDate = (enquiry.Enquiry_EventDate_Format1 || enquiry.Enquiry_EventDate) || "-";
                let start = enquiry.Enquiry_EventStartTime || "-";
                let end = enquiry.Enquiry_EventEndTime || "-";
                $("#inqEventDate").text(`${eventDate} (${start} - ${end})`);

                // halls
                $("#inqHalls").text(enquiry.Enquiry_Halls || "-");

                // email, address, note
                $("#inqEmail").text(enquiry.Enquiry_Email || "-");
                $("#inqAddress").text(enquiry.Enquiry_Address || "-");
                $("#inqNote").text(enquiry.Enquiry_Remarks || "-");

                // Bind Follow-Up History Table
                renderFollowUpHistory(followups);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up history");
        }
    });
}

function renderFollowUpHistory(list) {
    let html = "";

    if (!list || list.length === 0) {
        $("#followupHistoryBody").html(`
                <tr>
                    <td colspan="10" class="text-center text-muted py-2">No follow-ups found</td>
                </tr>
            `);
        return;
    }

    list.forEach((item, index) => {
        let notes = item.DiscussionNotes || "-";
        let shortNotes = notes.length > 50 ? notes.substring(0, 50) + "..." : notes;
        html += `
            <tr>
                <td>${index + 1}</td>
                <td>${item.FollowUpDateTime2 || '-'}</td>
                <td>${item.FollowUpTypeName}</td>
                <td>${item.StatusName}</td>
                <td>${item.MoodName}</td>
                <td>${item.NextFollowUpDateTime2 || '-'}</td>
                <td data-bs-toggle="tooltip" data-bs-placement="top" title="${notes.replace(/" /g, '&quot;')}">
                    ${shortNotes}
                </td>
                <td>${item.AssignedToName}</td>
                <td>${item.Probability}%</td>
                <td>
                    <button class="btn btn-action-xs btn-outline-warning edit-followup" data-id="${item.FollowUpId}">
                        <i class="fa fa-pencil"></i>
                    </button>
                    <button class="btn btn-action-xs btn-outline-danger delete-followup" data-id="${item.FollowUpId}">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    $("#followupHistoryBody").html(html);

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

}

function loadTimeDropdown() {
    let ddl = $("#nextFollowUpTime_modalFollowup");
    ddl.empty();

    for (let hour = 0; hour < 24; hour++) {
        for (let min = 0; min < 60; min += 30) {

            let hour12 = hour % 12;
            if (hour12 === 0) hour12 = 12;

            let ampm = hour < 12 ? "AM" : "PM";
            let hh = String(hour12).padStart(2, "0");
            let mm = String(min).padStart(2, "0");

            let display = `${hh}:${mm} ${ampm}`;
            let value = `${String(hour).padStart(2, "0")}:${mm}`; // 24-hour format
            //console.log("time: display:", display, ", value:", value);
            ddl.append(`<option value = "${value}"> ${display}</option> `);
        }
    }
}

function resetFollowUpForm() {
    // Select first option dynamically (if exists)
    const followUpType = $("#ddlFollowUpType option:first").val();
    const status = $("#ddlStatus option:first").val();
    const mood = $("#ddlMood option:first").val();

    $("#ddlFollowUpType").val(followUpType || "");
    $("#ddlStatus").val(status || "");
    $("#ddlMood").val(mood || "");

    $("#txtDiscussionNotes").val("");
    // Set today's date
    //let today = new Date().toISOString().split("T")[0];
    $("#nextFollowUpDate_modalFollowup").val("");

    // Set first time option
    //let firstTime = $("#nextFollowUpTime_modalFollowup option:first").val();
    $("#nextFollowUpTime_modalFollowup").val("");
    $("#nextFollowUpTime_modalFollowup").prop("selectedIndex", 0).trigger("change");

    //updateNextFollowUpValue();

    $("#txtProbability").val("0");
}

function updateNextFollowUpValue() {
    let date = $("#nextFollowUpDate_modalFollowup").val();
    let time = $("#nextFollowUpTime_modalFollowup").val(); // already 24-hour format

    if (!date || !time) {
        $("#txtNextFollowUpDateTime").val("");
        return;
    }
    $("#txtNextFollowUpDateTime").val(`${date}T${time} `);
}
// Trigger formatting automatically
$("#nextFollowUpDate_modalFollowup, #nextFollowUpTime_modalFollowup").on("change keyup", updateNextFollowUpValue);

$(document).on("click", ".edit-followup", function () {
    let followUpId = $(this).data("id");
    let enquiryId = $("#hfEnquiryDealId").val();

    const requestObj = {
        FollowUpId: Number(followUpId),
        EnquiryId: Number(enquiryId)
    };
    console.log("flwup edit requestObj", requestObj);
    $.ajax({
        url: "/api/EnquiryFollowUp/GetEnquiryFollowUp",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            console.log("edit res:", res);
            if (res.status === "Success" && res.data && res.data.FollowUps.length > 0) {
                let f = res.data.FollowUps;
                let item = f[0];

                // Fill modal fields
                $("#ddlFollowUpType").val(item.FollowUpTypeId);
                $("#ddlStatus").val(item.StatusId);
                $("#ddlMood").val(item.MoodId);
                $("#txtDiscussionNotes").val(item.DiscussionNotes);

                if (item.NextFollowUpDateTime) {
                    $("#nextFollowUpDate_modalFollowup").val(item.NextFollowUpDate);
                    $("#nextFollowUpTime_modalFollowup").val(item.NextFollowUpTime); // HH:mm
                }
                $("#txtNextFollowUpDateTime").val(item.NextFollowUpDate + 'T' + item.NextFollowUpTime);

                $("#txtProbability").val(item.Probability);
                // Store ID for update
                $("#hfFollowUpId").val(followUpId);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to load follow-up for editing");
        }
    });
});

$(document).on("click", ".delete-followup", function () {
    let followUpId = $(this).data("id");
    let enquiryId = $("#hfEnquiryDealId").val();
    let actionBy = sessionStorage.getItem("userId") ?? 0;

    Swal.fire({
        title: "Are you sure?",
        text: "This follow-up will be permanently deleted.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Yes, delete it"
    }).then((result) => {
        if (result.isConfirmed) {

            const deleteObj = {
                FollowUpId: followUpId,
                ActionBy: actionBy
            };

            $.ajax({
                url: "/api/EnquiryFollowUp/DeleteEnquiryFollowUp",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(deleteObj),

                success: function (res) {
                    if (res.status === "Success") {

                        Swal.fire({
                            icon: "success",
                            title: "Deleted!",
                            position: "top-end",
                            toast: true,
                            timer: 1500,
                            showConfirmButton: false
                        });

                        loadFollowUpHistoryById(enquiryId); // Reload table
                    }
                },
                error: function (xhr, error, status) {
                    handleApiError(xhr, error, status, "Failed to delete follow-up");
                }
            });
        }
    });
});

//FOLLOWUP - END ------------------------------------------------------------------------------------------

// Load Enquiry Deals (Drag & Drop view)
function loadEnquiryDeals(processId = 0) {
    const duration = $("#ddlDuration").val();
    const fromAmt = $("#fromAmount_filter").val();
    const toAmt = $("#toAmount_filter").val();

    const inputData = {
        ProcessId: Number(processId),
        Duration: duration,
        FromAmount: fromAmt ? Number(fromAmt) : 0,
        ToAmount: toAmt ? Number(toAmt) : 0,
        FromDate: duration === "customrange" ? $("#fromDate_filter").val() : null,
        ToDate: duration === "customrange" ? $("#toDate_filter").val() : null
    };
    console.log("EnquiryDeal inpuData:", inputData);

    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealList_forDragNDrop",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (response) {
            console.log("EnquiryDeal response:", response);
            if (response.status === "Success") {
                processes = response.data;
                renderBoard();
            } else {
                console.warn("No data received or API returned error:", response.message);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch enquiry deals");
        }
    });
}

// Render board dynamically
function renderBoard() {
    const board = document.getElementById("processBoard");
    const processSelect = document.getElementById("dealProcess");
    board.innerHTML = "";
    processSelect.innerHTML = "";

    processes.forEach(process => {
        let dealsHtml = "";

        if (process.EnquiryDealsJson.length > 0) {
            process.EnquiryDealsJson.forEach(deal => {
                //console.log("deal render:", deal);
                const hideFollowUp = (deal.Is_LeadBooked || deal.Is_LeadLost);
                dealsHtml += `
                    <div class="card shadow-sm mb-2 deal-card cursor-pointer"
                        draggable = "true"
                        data-dealid="${deal.EnquiryDealId}"
                        data-bookingid="${deal.BookingId || 0}"
                        data-deal='${JSON.stringify(deal)}'
                        data-processid="${process.ProcessId}">
                            <div class="card shadow-sm border-0 deal-open">

                                <!-- CARD HEADER -->
                                <div class="card-header bg-rating d-flex align-items-center justify-content-between p-1 py-2">
                                    <h6 class="mb-0 text-dark text-truncate flex-grow-1 me-0">
                                        ${deal.EnquiryDealTitle}
                                    </h6>

                                    <span class="fw-bold text-white small flex-shrink-0"
                                          title="Enquiry Date">
                                        ${deal.EnquiryDate_Format1}
                                    </span>
                                </div>

                                <!-- CARD BODY -->
                                <div class="card-body p-2">
                                    <!-- Row 1 : Deal Name + Event Date -->
                                    <div class="d-flex justify-content-between align-items-center mb-1 small">
                                        <span class="fw-bold text-muted text-truncate me-2">
                                            ${deal.EnquiryDealName}
                                        </span>

                                        <span class="text-muted flex-shrink-0" title="Event Date">
                                            ${deal.EventDate_Format1}
                                        </span>
                                    </div>

                                    <!-- Row 2 : Time + Amount -->
                                    <div class="d-flex justify-content-between align-items-center small">
                                        <span class="fw-bold text-dark flex-shrink-0 fs-9">
                                            ₹${deal.Amount.toLocaleString()}
                                        </span>

                                        <span class="text-muted">
                                            ${deal.EventStartTime} - ${deal.EventEndTime}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <!--Follow-up Row-->
                        ${ hideFollowUp
                        ? ""
                        : `<div class="card-footer py-1 px-2 d-flex justify-content-between align-items-center
                                followup-row text-primary fw-bold open-followup"
                                data-dealid="${deal.EnquiryDealId}">
                                <span>Follow-up</span>
                                <span class="badge bg-secondary">${deal.FollowUpCount}</span>
                            </div>
                                  `
                        }                            
                        </div>
                    `;
            });
        }
        else {
            dealsHtml = `<p class="text-muted fst-italic"> This stage is empty</p> `;
        }

        board.innerHTML += `
                <div class="card process-column"
                    data-processid="${process.ProcessId}"
                    data-isconfirmed="${process.IsConfirmed}"
                    data-islost="${process.IsLost}">
                    <div class="card-header bg-warning p-3">
                        <h6 class="mb-0 text-white">${process.ProcessName}</h6>
                        <small class="text-white">₹${process.TotalAmount.toLocaleString()} • ${process.TotalEnquiryDeals} Deal(s)</small>
                    </div>
                    <div class="card-body deal-dropzone p-2" style="min-height: 500px;">
                        ${dealsHtml}
                    </div>
                </div>
    `;

        processSelect.innerHTML += `<option value = "${process.ProcessId}"> ${process.ProcessName}</option> `;
    });

    enableDragDrop();

    // 🔥 Add click event listener for all deal cards
    $(document).off("click", ".deal-open").on("click", ".deal-open", function () {
        const dealId = $(this).closest(".deal-card").data("dealid");
        const bookingId = $(this).closest(".deal-card").data("bookingid");
        console.log("Clicked dealId:", dealId);

        // 🔒 Validation: Prevent edit if already booked
        if (bookingId > 0) {
            Swal.fire({
                icon: "warning",
                title: "Edit Not Allowed",
                text: "This enquiry has already been booked and cannot be edited."
            });
            return; // stop execution
        }

        // later you can call your API here
        // 🔥 Reset before fetching data
        resetDealForm();

        $.ajax({
            url: `/api/EnquiryDeal/GetEnquiryDealById/${dealId} `,
            type: "GET",
            success: function (response) {
                if (response.status === "Success" && response.data) {
                    const data = response.data;

                    // 🔥 Change modal title for edit mode
                    $("#dealModalLabel").text("Edit Deal (Enquiry)");
                    // Show delete button
                    $("#deleteDealBtn").removeClass("d-none").data("deal-id", dealId);

                    // Populate form fields
                    $("#dealId").val(data.Id);
                    $("#dealProcess").val(data.ProcessId).prop("disabled", true);;
                    $("#dealName").val(data.Name);
                    $("#dealContact").val(data.Contact);
                    $("#dealAddress").val(data.Address || "");
                    $("#dealEmail").val(data.Email || "");

                    // First, set the event type
                    $("#eventTypeDropdown").val(data.EventTypeId).trigger("change");

                    // Wait until subtypes are populated (assuming loadSubTypes is async)
                    setTimeout(() => {
                        $("#eventSubTypeDropdown").val(data.EventSubTypeId);
                    }, 300); // small delay to ensure subtypes are loaded

                    $("#banquetHallDropdown").val(data.BanquetHallId);
                    $("#eventDate").val(data.EventDate_Format2);

                    $("#eventStart").val(data.EventStartTime);
                    $("#eventEnd").val(data.EventEndTime);
                    $("#pax").val(data.Pax);
                    $("#packageDropdown").val(data.PackageId).trigger("change");
                    $("#followUpDate").val(data.FollowUpDate_Format2);

                    $("#estimatedAmount").val(data.EstimatedAmount);
                    $("#leadSourceDropdown").val(data.LeadSourceId);
                    $("#remark").val(data.Remarks || "");

                    // finally, open modal
                    // const modal = new bootstrap.Modal(document.getElementById("dealModal"));
                    // modal.show();
                    $("#dealModal").modal("show"); // reopen only if it was opened from deal
                } else {
                    Swal.fire({
                        icon: "info",
                        title: "Not Found",
                        text: response.message || "No data available for this deal."
                    });
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, error, status, "EnquiryId Fetch Fail");
            }
        });

    });

    $(document).off("click", ".open-followup").on("click", ".open-followup", function (e) {
        e.preventDefault();
        e.stopPropagation();

        const enquiryDealId = $(this).data("dealid");

        openFollowUp(enquiryDealId);
    });

    function validateFollowUp() {
        const followUpDateTime = new Date(); // current datetime
        const nextFollowUpInput = $("#txtNextFollowUpDateTime").val();

        // If no next follow-up entered → consider valid
        if (!nextFollowUpInput) return true;

        const nextFollowUpDate = new Date(nextFollowUpInput);

        // Invalid date
        //if (isNaN(nextFollowUpDate.getTime())) {
        //    Swal.fire("Invalid Date", "Please enter a valid next follow-up date & time.", "error");
        //    return false;
        //}

        // Must be greater than current follow-up time
        if (nextFollowUpDate <= followUpDateTime) {
            Swal.fire(
                "Invalid Next Follow-Up",
                "Next Follow-Up Date/Time must be greater than the current Follow-Up Date/Time.",
                "error"
            );
            return false;
        }
        return true; // Valid
    }

    $("#btnSaveFollowUp").off("click").on("click", function () {
        const enquiryId = $("#hfEnquiryDealId").val();

        if (!enquiryId || enquiryId == "0") {
            Swal.fire("Invalid", "Enquiry ID missing.", "warning");
            return;
        }

        if (!validateFollowUp()) {
            return; // stop if invalid
        }
        const fileUpload_base64 = document.getElementById("attachmentBase64_followUp").value.trim();

        const obj = {
            FollowUpId: $("#hfFollowUpId").val() || 0,  // always insert new record
            EnquiryId: enquiryId,
            FollowUpTypeId: $("#ddlFollowUpType").val(),
            FollowUpDateTime: new Date().toISOString(),   // server accepts nvarchar(30)
            NextFollowUpDateTime: $("#txtNextFollowUpDateTime").val() || null,
            StatusId: $("#ddlStatus").val(),
            MoodId: $("#ddlMood").val(),
            OutcomeId: null,             // you can add later
            Probability: $("#txtProbability").val() || null,
            DiscussionNotes: $("#txtDiscussionNotes").val(),
            InternalRemarks: null,
            CustomerRemarks: null,
            FileUpload: fileUpload_base64,
            AssignedTo: 1,               // dynamic later
            IsClosed: false,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        console.log("Saving Follow-Up:", obj);

        $.ajax({
            url: "/api/EnquiryFollowUp/SaveEnquiryFollowUp",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(obj),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    saveFollowUpAndMoveDeal();
                    resetFollowUpForm();
                    loadFollowUpHistoryById(enquiryId);
                    loadEnquiryDeals();

                    // ✅ Call your moveDeal function with parameters
                    //moveDeal(
                    //    pendingMoveData.dealId,
                    //    pendingMoveData.fromProcessId,
                    //    pendingMoveData.toProcessId,
                    //    nextFollowUp,
                    //    remarks,
                    //    attachmentBase64_enquiryMove
                    //);
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, error, status, "Failed to save follow-up.");
            }
        });

    });
}

//custom tooltip - on-hover -- START --------------------
const tooltip = document.getElementById("dealTooltip");
const board = document.getElementById("processBoard");

board.addEventListener("mouseover", function (e) {
    const card = e.target.closest(".deal-card");
    if (!card) return;

    // 🚫 Do not show tooltip while dragging
    if (card.classList.contains("dragging")) return;

    // Prevent re-trigger when moving inside same card
    if (card.contains(e.relatedTarget)) return;

    const deal = JSON.parse(card.dataset.deal);
    //console.log("deal:", deal);

    tooltip.innerHTML = `
        <div class="title">
            📌 ${deal.EnquiryDealTitle}
            ${deal.BookingId > 0 ? ` - BK#${deal.BookingId}` : ""}
        </div>

        <div class="row-item"><strong>Name:</strong> ${deal.EnquiryDealName || "-"}</div>
        <div class="row-item"><strong>Contact:</strong> ${deal.EnquiryDealContact || "-"}</div>
        <div class="row-item"><strong>Email:</strong> ${deal.EnquiryDealEmail || "-"}</div>
        <div class="row-item"><strong>Address:</strong> ${deal.EnquiryDealAddress || "-"}</div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Event Type:</strong> ${deal.EventTypeName || "-"}</div>
        <div class="row-item"><strong>Event SubType:</strong> ${deal.EventSubTypeName || "-"}</div>
        <div class="row-item"><strong>Hall:</strong> ${deal.HallName || "-"}</div>
        <div class="row-item"><strong>Lead Source:</strong> ${deal.LeadSourceName || "-"}</div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Event Date:</strong> ${deal.EventDate_Format1 || "-"}</div>
        <div class="row-item"><strong>Event Time:</strong> 
            ${deal.EventStartTime || "-"} - ${deal.EventEndTime || "-"}
        </div>

        <div class="row-item"><strong>PAX:</strong> ${deal.Pax || 0}</div>
        <div class="row-item"><strong>Package:</strong> ${deal.PackageName || 0}</div>
        <div class="row-item"><strong>Pakage Amount:</strong> ${deal.EstimatedPackageAmount || 0}</div>
        <div class="row-item"><strong>Estimated Amount:</strong> 
            ₹${deal.Amount ? deal.Amount.toLocaleString() : "0"}
        </div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Enquiry Date:</strong> ${deal.EnquiryDate_Format1 || "-"}</div>
        <div class="row-item d-none"><strong>FollowUps:</strong> ${deal.FollowUpCount || 0}</div>

        <div class="row-item">
            <strong>Status:</strong>
            ${
                deal.Is_LeadBooked
                    ? `<span class="badge bg-success">Booked</span>`
                    : deal.Is_LeadLost
                        ? `<span class="badge bg-danger">Lost</span>`
                        : `<span class="badge bg-warning">Open</span>`
            }
        </div>

        ${deal.Remarks ? `
            <hr style="margin:6px 0; opacity:0.3;">
            <div class="row-item"><strong>Remarks:</strong></div>
            <div style="font-size:12px; opacity:0.9;">
                ${deal.Remarks}
            </div>
        ` : ""}
    `;

    tooltip.style.display = "block";
    const rect = card.getBoundingClientRect();
    const screenWidth = window.innerWidth;
    const tooltipRect = tooltip.getBoundingClientRect();
    const gap = 12;

    let leftPosition;

    if (rect.left < screenWidth / 2) {
        leftPosition = rect.right + gap + window.scrollX;
    }    
    else {
        leftPosition = rect.left - tooltipRect.width - gap + window.scrollX;
    }

    tooltip.style.left = leftPosition + "px";
    tooltip.style.top = rect.top + window.scrollY + "px";
});

board.addEventListener("mouseout", function (e) {
    const card = e.target.closest(".deal-card");
    if (!card) return;

    // Prevent hide when moving inside same card
    if (card.contains(e.relatedTarget)) return;

    tooltip.style.display = "none";
});
//custom tooltip - on-hover -- END --------------------

// Drag & Drop setup
let pendingMoveData = null;
//const bookingModal = new bootstrap.Modal(document.getElementById("bookingModal"));
//bookingModal.show();
//bookingModal.hide();

function enableDragDrop() {
    const cards = document.querySelectorAll(".deal-card");
    const dropzones = document.querySelectorAll(".deal-dropzone");

    cards.forEach(card => {
        card.addEventListener("dragstart", e => {
            const bookingId = parseInt(card.dataset.bookingid) || 0;

            const column = card.closest(".process-column");
            const isConfirmed = column.dataset.isconfirmed === "true";
            //const isLost = column.dataset.islost === "true";

            // 🔒 Block drag if already booked
            if (bookingId > 0 && isConfirmed) {
                e.preventDefault(); // stop drag
                Swal.fire({
                    icon: "warning",
                    title: "Action Not Allowed",
                    text: "This enquiry has already been booked and cannot be moved."
                });
                return;
            }

            // 🛑 Hide tooltip while dragging
            const tooltip = document.getElementById("dealTooltip");
            tooltip.style.display = "none";
            // Optional: disable tooltip temporarily
            card.classList.add("dragging");

            // ✅ Normal drag
            e.dataTransfer.setData("dealId", card.dataset.dealid);
            e.dataTransfer.setData("fromProcessId", card.dataset.processid);
        });
    });

    dropzones.forEach(zone => {
        zone.addEventListener("dragover", e => e.preventDefault());

        zone.addEventListener("drop", e => {
            e.preventDefault();

            //Re-enable Tooltip After Drop
            const tooltip = document.getElementById("dealTooltip");
            tooltip.style.display = "none";
            document.querySelectorAll(".deal-card.dragging").forEach(c => c.classList.remove("dragging"));

            const dealId = parseInt(e.dataTransfer.getData("dealId"));
            const fromProcessId = parseInt(e.dataTransfer.getData("fromProcessId"));

            const column = zone.closest(".process-column");

            const toProcessId = parseInt(column.dataset.processid);
            const isConfirmed = column.dataset.isconfirmed === "true";
            const isLost = column.dataset.islost === "true";

            //const toProcessId = parseInt(
            //    zone.closest(".process-column").dataset.processid
            //);

            if (fromProcessId !== toProcessId) {

                // 🧠 Store pending move ONLY
                pendingMoveData = {
                    EnquiryDealId: dealId,
                    ProcessId: toProcessId
                };

                // 🎯 If dropped in Confirmed column
                if (isConfirmed) {
                    moveOn();
                    console.log("Booked");
                    resetBooking();
                    //Fetch Lead-Deal Data
                    const draggedCard = document.querySelector(`.deal-card[data-dealid="${dealId}"]`);
                    if (!draggedCard) return;
                    var dealData = null;
                    try {
                        dealData = JSON.parse(draggedCard.dataset.deal);
                        console.log("=== CONFIRMED DEAL DATA ===");
                        // Optional: pretty print
                        console.table(dealData);
                    }
                    catch (err) {
                        console.error("Error parsing deal data:", err);
                        return;
                    }
                    
                    //OPEN BOOKING MODAL
                    const dateStr = dealData.EventDate;       // for display
                    const dateStr_Format = dealData.EventDate_Format1;
                    const dateStr_ISO = dealData.EventDate;
                    const shift = dealData.ShiftCode;
                    const shiftname = dealData.ShiftFullName; // fallback if not found
                    const date = dateStr;
                    const dateiso = dateStr_ISO;
                    const hallid = dealData.HallId;

                    $("#BookingId").val(bookingId);
                    // 🟢 Update label content & attributes - using jQuery
                    $("#label_date_shift")
                        .text(`${date}, ${shiftname} Slot`)        // visible text
                        .attr("data-eventdate", dateiso)           // yyyy-mm-dd
                        .attr("data-shiftid", shift);              // FD, MO, etc.
                    $("#banquetDropdown").val(hallid);
                    $("#clientEnquiryDropdown").val(dealData.EnquiryDealId).trigger("change");

                    $("#booking_pax").val(dealData.Pax);

                    setShiftTime(shift);
                    $("#PaymentDetails_DueDate").val('');

                    $("#fromDate").val(dateiso); //.prop("disabled", true);
                    $("#toDate").val(dateiso);

                    $("#title_bookingModal").text("Add Booking Details");
                    $("#bookingModal").modal("show");

                    return;
                }

                // 🎯 If dropped in Lost column (optional handling)
                if (isLost) {
                    console.log("Lost");
                    return;
                }

                // Open follow-up modal
                openFollowUp(dealId);
            }
        });
    });

}

function moveOn(){
    saveFollowUpAndMoveDeal();
    bookingId = 0;
    pendingMoveData = null;
}

$("#followupModal").on("hidden.bs.modal", function () {
    moveOn();
});
var bookingId = 0;
function saveFollowUpAndMoveDeal() {
    console.log("pendingMoveData:", pendingMoveData);
    if (!pendingMoveData) return;
    let actionBy = sessionStorage.getItem("userId") ?? 0;
    const payload = {
        EnquiryDealId: pendingMoveData.EnquiryDealId,
        ProcessId: pendingMoveData.ProcessId,
        FollowUpDate: $("#nextFollowUpDate_modalFollowup").val() || null,
        Remarks: $("#txtDiscussionNotes").val() || null,
        FileUpload: $("#attachmentBase64_followUp").val() || null,
        ActionBy: Number(actionBy)
    };
    console.log("SaveEnquiryDeal_forDragNDrop payload:", payload);
    $.ajax({
        url: "/api/EnquiryDeal/SaveEnquiryDeal_forDragNDrop",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                // Close modal
                $("#followupModal").modal("hide");

                //OPEN BOOKING MODAL IF BOOKED
                //if (pendingMoveData.ProcessId === processes.ProcessId && processes.ProcessName === "Event Booked") {
                //    //here7

                //}
                console.log("res data:", res.data);
                bookingId = res.data.BookingId;
                // Clear pending state
                pendingMoveData = null;

                // Reload board (or move card visually)
                //renderBoard();
                loadEnquiryDeals();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Deal Save Failed!");
        }
    });
}

function openFollowUp(enquiryDealId, moveContext = null) {
    resetFollowUpForm();
    console.log("Opening Follow-Up. EnquiryDealId =", enquiryDealId);
    $("#hfEnquiryDealId").val(enquiryDealId);

    // Optional: store drag-drop move info if exists
    //if (moveContext) {
    //    window.pendingMoveData = moveContext;
    //} else {
    //    window.pendingMoveData = null;
    //}

    loadFollowUpHistoryById(enquiryDealId);

    $("#followupModal").modal("show");
}

// Reset Deal Form
function resetDealForm() {
    $("#dealModalLabel").text("Add Deal (Enquiry)");
    $("#deleteDealBtn").addClass("d-none").data("deal-id", "0"); // hide delete btn

    // Reset all fields
    $("#dealId").val(0);
    $("#dealProcess").val(1).prop("disabled", false);;

    $("#dealName").val("");
    $("#dealContact").val("");
    $("#dealAddress").val("");
    $("#dealEmail").val("");
    $("#eventTypeDropdown").val("0");
    $("#eventSubTypeDropdown").val("0");
    $("#banquetHallDropdown").val("0");
    $("#eventDate").val("");
    $("#eventStart").val("");
    $("#eventEnd").val("");
    $("#pax").val("0");
    $("#leadSourceDropdown").val("0");
    $("#followUpDate").val("");
    $("#estimatedAmount").val("0");
    $("#remark").val("");

    // Hide all error messages
    $("#dealProcess").siblings(".error-msg").hide();
    $("#dealName").siblings(".error-msg").hide();
    $("#dealContact").siblings(".error-msg").hide();
    $("#eventTypeDropdown").siblings(".error-msg").hide();
    $("#eventSubTypeDropdown").siblings(".error-msg").hide();
    $("#banquetHallDropdown").siblings(".error-msg").hide();
    $("#eventDate").siblings(".error-msg").hide();
    $("#pax").siblings(".error-msg").hide();
    $("#leadSourceDropdown").siblings(".error-msg").hide();
    $("#estimatedAmount").siblings(".error-msg").hide();
}

// Validate Deal Form
function validateDealForm() {
    let isValid = true;

    // (0) Process / Stage
    const dealProcessErr = document.getElementById("dealProcess").parentElement.nextElementSibling;
    if ($("#dealProcess").val() === "" || $("#dealProcess").val() === "0") {
        dealProcessErr.classList.remove("d-none");
        isValid = false;
    } else {
        dealProcessErr.classList.add("d-none");
    }

    // (1) Name
    const dealNameErr = document.getElementById("dealName").nextElementSibling;
    if ($("#dealName").val().trim() === "") {
        dealNameErr.classList.remove("d-none");
        isValid = false;
    } else {
        dealNameErr.classList.add("d-none");
    }

    // (2) Contact
    const dealContactErr = document.getElementById("dealContact").nextElementSibling;
    if ($("#dealContact").val().trim() === "") {
        dealContactErr.classList.remove("d-none");
        isValid = false;
    } else {
        dealContactErr.classList.add("d-none");
    }

    // (5) Event Type
    const eventTypeErr = document.getElementById("eventTypeDropdown").parentElement.nextElementSibling;
    if ($("#eventTypeDropdown").val() === "" || $("#eventTypeDropdown").val() === "0") {
        eventTypeErr.classList.remove("d-none");
        isValid = false;
    } else {
        eventTypeErr.classList.add("d-none");
    }

    // (5) Event Sub-Type
    const eventSubTypeErr = document.getElementById("eventSubTypeDropdown").parentElement.nextElementSibling;
    if ($("#eventSubTypeDropdown").val() === "" || $("#eventSubTypeDropdown").val() === "0") {
        eventSubTypeErr.classList.remove("d-none");
        isValid = false;
    } else {
        eventSubTypeErr.classList.add("d-none");
    }

    // (6) Banquet Hall
    const banquetHallErr = document.getElementById("banquetHallDropdown").parentElement.nextElementSibling;
    if ($("#banquetHallDropdown").val() === "" || $("#banquetHallDropdown").val() === "0") {
        banquetHallErr.classList.remove("d-none");
        isValid = false;
    } else {
        banquetHallErr.classList.add("d-none");
    }

    // (7) Event Date
    const eventDateErr = document.getElementById("eventDate").nextElementSibling;
    if ($("#eventDate").val() === "") {
        eventDateErr.classList.remove("d-none");
        isValid = false;
    } else {
        eventDateErr.classList.add("d-none");
    }

    // (8) Start Time
    const eventStartErr = document.getElementById("eventStart").nextElementSibling;
    if ($("#eventStart").val() === "") {
        eventStartErr.classList.remove("d-none");
        isValid = false;
    } else {
        eventStartErr.classList.add("d-none");
    }

    // (8) End Time
    const eventEndErr = document.getElementById("eventEnd").nextElementSibling;
    if ($("#eventEnd").val() === "") {
        eventEndErr.classList.remove("d-none");
        isValid = false;
    } else {
        eventEndErr.classList.add("d-none");
    }

    // (8) Compare Start & End Time
    const startTime = $("#eventStart").val();
    const endTime = $("#eventEnd").val();
    // const eventEndInput = document.getElementById("eventEnd");
    // const eventEndErr = eventEndInput.nextElementSibling;

    // Run comparison only if both fields are filled
    if (startTime && endTime) {
        // Parse times into comparable Date objects
        const start = parseTime(startTime);
        const end = parseTime(endTime);

        if (end <= start) {
            eventEndErr.textContent = "End Time must be after Start Time";
            eventEndErr.classList.remove("d-none");
            isValid = false;
        } else {
            eventEndErr.classList.add("d-none");
        }
    }

    // (9) Pax
    const paxErr = document.getElementById("pax").nextElementSibling;
    if ($("#pax").val().trim() === "") {
        paxErr.classList.remove("d-none");
        isValid = false;
    } else {
        paxErr.classList.add("d-none");
    }

    // (10) Lead Source
    const leadSourceErr = document.getElementById("leadSourceDropdown").parentElement.nextElementSibling;
    if ($("#leadSourceDropdown").val() === "" || $("#leadSourceDropdown").val() === "0") {
        leadSourceErr.classList.remove("d-none");
        isValid = false;
    } else {
        leadSourceErr.classList.add("d-none");
    }

    // (12) Estimated Amount
    const estimatedAmountErr = document.getElementById("estimatedAmount").nextElementSibling;
    if ($("#estimatedAmount").val().trim() === "" || Number($("#estimatedAmount").val()) <= 0) {
        estimatedAmountErr.classList.remove("d-none");
        isValid = false;
    } else {
        estimatedAmountErr.classList.add("d-none");
    }

    return isValid;
}
// Utility: convert "hh:mm AM/PM" → Date object for comparison
function parseTime(timeStr) {
    // e.g. "07:30 PM"
    const [time, modifier] = timeStr.split(" ");
    let [hours, minutes] = time.split(":").map(Number);

    if (modifier === "PM" && hours < 12) hours += 12;
    if (modifier === "AM" && hours === 12) hours = 0;

    const date = new Date();
    date.setHours(hours, minutes, 0, 0);
    return date;
}

function calculateEstimatedAmount() {

    const pax = parseFloat(document.getElementById("pax").value) || 0;
    const packageAmount = parseFloat(document.getElementById("packageAmount").value) || 0;

    const total = pax * packageAmount;

    document.getElementById("estimatedAmount").value =
        total > 0 ? total.toFixed(2) : "";
}

// Initial load
document.addEventListener("DOMContentLoaded", function () {
    loadEnquiryDeals();
    loadEnquiryList();
    loadStateList();

    loadEventTypeList();
    loadEventSubTypeList();
    loadBanquetHallList();
    loadLeadSourceList();

    loadFollowUpTypes();
    loadFollowUpStatus();
    loadMoodList();
    loadTimeDropdown();

    fetchHallDetails();
    loadFoodPackageList();

    // Trigger on input
    document.getElementById("pax").addEventListener("input", calculateEstimatedAmount);
    document.getElementById("packageAmount").addEventListener("input", calculateEstimatedAmount);
    document.getElementById("packageDropdown").addEventListener("change", function () {
        const selectedOption = this.options[this.selectedIndex];
        // Get rate per person from data attribute
        const ratePerPerson = parseFloat(selectedOption.getAttribute("data-rateperperson")) || 0;

        // Set estimated package amount field
        document.getElementById("packageAmount").value = ratePerPerson > 0 ? ratePerPerson : "";

        // Recalculate total
        calculateEstimatedAmount();
    });

    $("#ddlDuration").on("change", function () {
        const duration = $(this).val();

        if (duration === "customrange") {
            $("#customDateRange").removeClass("d-none");
        } else {
            $("#customDateRange").addClass("d-none");
            $("#fromDate_filter").val("");
            $("#toDate_filter").val("");
        }
    });

    $("#btnApplyFilters").on("click", function () {
        loadEnquiryDeals();
        $("#filterDropdown").dropdown("hide");
    });

    $("#btnResetFilters").on("click", function () {
        $("#ddlDuration").val("thismonth");
        $("#fromAmount_filter").val("");
        $("#toAmount_filter").val("");
        $("#fromDate_filter").val("");
        $("#toDate_filter").val("");
        $("#customDateRange").addClass("d-none");

        loadEnquiryDeals();
    });

    //Modal Behaviour -- START ---------------------------------------------------
    //Deal form
    // 1️⃣ Handle + Add Deal button (New Deal)
    document.querySelector('[data-bs-target="#dealModal"]').addEventListener("click", function () {
        resetDealForm(); // always reset before showing modal
    });

    // 2️⃣ Handle modal open (just a safeguard)
    $('#dealModal').on('shown.bs.modal', function () {
        console.log("Modal opened");
        // You can keep additional logic here if needed
    });

    let openedFromDeal = false;

    // When Process Modal closes
    // $("#processModal").on("hidden.bs.modal", function () {
    //     if (openedFromDeal) {
    //         $("#dealModal").modal("show"); // reopen only if it was opened from deal
    //         openedFromDeal = false; // reset flag
    //     }
    // });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openEventTypeModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#eventTypeModal").modal("show");
        resetEventTypeForm();
    });

    // When eventType Modal closes
    $("#eventTypeModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openEventSubTypeModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#eventSubTypeModal").modal("show");
        resetEventSubTypeForm();
    });

    // When eventSubType Modal closes
    $("#eventSubTypeModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openBanquetHallModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#banquetHallModal").modal("show");
        resetBanquetHallForm();
    });

    // When Banquet Hall Modal closes
    $("#banquetHallModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openLeadSourceModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#leadSourceModal").modal("show");
        resetLeadSourceForm();
    });

    // When Lead Source Modal closes
    $("#leadSourceModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    //Modal Behaviour -- START ---------------------------------------------------

    const addonModal = document.getElementById('addonChargeModal');
    const bookingModal = document.getElementById('bookingModal');
    const menuModal = document.getElementById('menuModal');

    //hide/show Add-On MODAL ----- START --------------------------
    addonModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal.classList.remove("show");
        bookingModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    addonModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingModal.classList.add("show");
        bookingModal.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });

    //hide/show Add-On MODAL ----- END --------------------------

    //hide/show MENU MODAL ----- START --------------------------
    menuModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal.classList.remove("show");
        bookingModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    menuModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingModal.classList.add("show");
        bookingModal.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show MENU MODAL ----- END --------------------------

    //Menu Item - START ----- START ---------------------------
    $('#menuModal').on('click', '.btn-success', function () {
        const container = $(this).closest('.border');
        const input = $(this).siblings('input[type="text"]');
        const newItemName = input.val().trim();
        if (newItemName === '') return;

        const itemsContainer = container.find('.items-container');
        const newId = newItemName.replace(/\s+/g, '_') + '_' + Date.now();

        const newItemHtml = `
                <div class="new-item">
                    <input class="form-check-input me-1" type="checkbox" value="${newItemName}" id="${newId}">
                    <label class="form-check-label" for="${newId}">${newItemName}</label>
                    <button type="button" class="btn btn-sm btn-outline-danger delete-item">✖</button>
                </div>
            `;

        itemsContainer.append(newItemHtml);
        input.val(''); // clear input
    });

    // Delete new item
    $('#menuModal').on('click', '.delete-item', function () {
        $(this).closest('.new-item').remove();
    });

    // Select All toggle
    $('#menuModal').on('change', 'input[id^="selectAll"]', function () {
        const isChecked = $(this).is(':checked');
        const container = $(this).closest('.border');
        container.find('.items-container input.form-check-input').prop('checked', isChecked);
    });

    // Update Select All if manual changes
    $('#menuModal').on('change', '.items-container input.form-check-input', function () {
        const container = $(this).closest('.border');
        const all = container.find('.items-container input.form-check-input');
        const selectAllBox = container.find('input[id^="selectAll"]');
        selectAllBox.prop('checked', all.length === all.filter(':checked').length);
    });
    //Menu Item - START ----- END ---------------------------


    // Enquiry/Deal -- START --------------------------------------------------------------------
    // Save Deal Form
    document.getElementById("saveDealBtn").addEventListener("click", function (e) {
        e.preventDefault();
        if (!validateDealForm()) {
            console.log("Deal Save - Validation failed");
            return;
        }
        //////showLoader()()();

        const enquiryData = {
            Id: $("#dealId").val() || 0, // ensure number
            ProcessId: parseInt($("#dealProcess").val()) || 0,
            Name: $("#dealName").val().trim(),
            Contact: $("#dealContact").val().trim(),
            Address: $("#dealAddress").val().trim(),

            Email: $("#dealEmail").val().trim(),
            EventTypeId: parseInt($("#eventTypeDropdown").val()) || 0,
            EventSubTypeId: parseInt($("#eventSubTypeDropdown").val()) || 0,
            BanquetHallId: parseInt($("#banquetHallDropdown").val()) || 0,
            EventDate: $("#eventDate").val(),

            EventDate: $("#eventDate").val(), // if required separately
            EventStartTime: $("#eventStart").val(),
            EventEndTime: $("#eventEnd").val(),
            Pax: parseInt($("#pax").val()) || 0,
            FollowUpDate: $("#followUpDate").val() || null,

            EstimatedAmount: parseFloat($("#estimatedAmount").val()) || 0,
            LeadSourceId: parseInt($("#leadSourceDropdown").val()) || 0,
            Remarks: $("#remark").val().trim(),
            PackageId: parseInt($("#packageDropdown").val()) || 0,
            ActionBy: userId // must be >0
        };
        console.log("Posting EnquiryDeal:", enquiryData);

        $.ajax({
            url: "/api/EnquiryDeal/SaveEnquiryDeal",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(enquiryData),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    loadEnquiryDeals();
                    resetDealForm();
                    bootstrap.Modal.getInstance(document.getElementById("dealModal")).hide();

                    // TODO: refresh deal list if you have one
                    // loadDealList();
                } else {
                    Swal.fire({
                        icon: "info",
                        title: "Failed",
                        text: response.message
                    });
                }
            },
            error: function (xhr) {
                Swal.fire({
                    icon: "error",
                    title: "Error!",
                    text: xhr.responseText || "Something went wrong while saving."
                });
            },
            complete: function () {
                ////hideLoader();
            }
        });
    });

    // Delete Deal
    $(document).on("click", "#deleteDealBtn", function () {
        let dealId = $(this).data("deal-id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this deal?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                //////showLoader()()();

                var inputData = {
                    Id: dealId,
                    ActionBy: userId // use logged-in user id
                };

                $.ajax({
                    url: "/api/EnquiryDeal/DeleteEnquiryDeal",
                    type: "POST", // adjust if API expects DELETE
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            Swal.fire({
                                position: "top-end",
                                icon: "success",
                                title: "Deal Deleted!",
                                showConfirmButton: false,
                                timer: 900
                            });
                            loadEnquiryDeals();
                            resetDealForm();
                            //bootstrap.Modal.getInstance(document.getElementById("dealModal")).hide();
                            $("#dealModal").modal("hide");
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting deal."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    // Enquiry/Deal -- END ----------------------------------------------------------------------

    //EVENT TYPE MASTER -- START --------------------------------------------------------------------
    // Add Event Type
    document.getElementById("eventTypeForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateEventTypeForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();

        let id = $("#eventTypeId").val();
        let name = $("#eventTypeName").val();

        var inputData = {
            Id: id,
            Name: name,
            ActionBy: userId // set actual logged-in user id
        };

        $.ajax({
            url: "/api/EventType/SaveEventType",   // your API for saving event type
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    resetEventTypeForm();
                    loadEventTypeList();
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Error saving event type."
                });
            },
            complete: function () {
                //hideLoader();
            }
        });

    });

    // Edit Event Type
    $(document).on("click", ".edit-eventType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let name = row.find("td:eq(1)").text();

        resetEventTypeForm();
        $("#eventTypeId").val(id);
        $("#eventTypeName").val(name);
    });

    // Delete Event Type
    $(document).on("click", ".delete-eventType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this event type?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: userId // set actual logged-in user id
                }

                $.ajax({
                    url: "/api/EventType/DeleteEventType",
                    type: "POST", // backend expects POST
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            Swal.fire({
                                position: "top-end",
                                icon: "success",
                                title: "Deleted!",
                                showConfirmButton: false,
                                timer: 900
                            });
                            loadEventTypeList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting event type."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    // Event Type dropdown change event
    $(document).on("change", "#eventTypeDropdown, #eventTypeDropdown_SubTypeModal", function () {
        let selectedEventTypeId = $(this).val();
        $("#eventTypeDropdown, #eventTypeDropdown_SubTypeModal").val(selectedEventTypeId);
        if (selectedEventTypeId && selectedEventTypeId !== "0") {
            loadEventSubTypeList(selectedEventTypeId); // Pass selected EventTypeId
        } else {
            // Clear the Event Sub-Type table if no event type selected
            $("#eventSubTypeTable tbody").html(
                `<tr> <td colspan="4" class="text-center text-muted">Please select an Event Type</td></tr> `
            );
        }
    });

    //EVENT TYPE MASTER -- END ----------------------------------------------------------------------

    //EVENT SUB-TYPE MASTER -- START --------------------------------------------------------------------
    // Save / Update Event Sub-Type
    document.getElementById("eventSubTypeForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateEventSubTypeForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();

        let id = $("#eventSubTypeId").val();
        let name = $("#eventSubTypeName").val();
        let eventTypeId = $("#eventTypeDropdown_SubTypeModal").val();

        var inputData = {
            Id: id,
            Name: name,
            EventTypeId: eventTypeId,
            ActionBy: userId // set actual logged-in user id
        };

        $.ajax({
            url: "/api/EventSubType/SaveEventSubType", // your API endpoint
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Saved!");
                    resetEventSubTypeForm();
                    loadEventSubTypeList(eventTypeId);
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Error saving event sub-type");
            }
        });
    });

    // Edit Event Sub-Type
    $(document).on("click", ".edit-eventSubType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let eventtypeid = row.data("eventtypeid");
        let name = row.find("td:eq(1)").text();

        resetEventSubTypeForm();
        $("#eventSubTypeId").val(id);
        $("#eventSubTypeName").val(name);
        $("#eventTypeDropdown_SubTypeModal").val(eventtypeid);
    });

    // Delete Event Sub-Type
    $(document).on("click", ".delete-eventSubType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this event sub-type?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: userId
                };

                $.ajax({
                    url: "/api/EventSubType/DeleteEventSubType", // API endpoint
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            Swal.fire({
                                position: "top-end",
                                icon: "success",
                                title: "Deleted!",
                                showConfirmButton: false,
                                timer: 900
                            });
                            loadEventSubTypeList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting event sub-type."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    //EVENT SUB-TYPE MASTER -- END ----------------------------------------------------------------------

    //EVENT LEAD SOURCE MASTER -- START ----------------------------------------------------------------------
    // ✅ Add / Update Lead Source
    document.getElementById("leadSourceForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateLeadSourceForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();

        let id = $("#leadSourceId").val();
        let name = $("#leadSourceName").val();

        var inputData = {
            Id: id,
            Name: name,
            ActionBy: userId // set actual logged-in user id
        };

        $.ajax({
            url: "/api/LeadSource/SaveLeadSource",   // your API for saving lead source
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    resetLeadSourceForm();
                    loadLeadSourceList();
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Error saving lead source."
                });
            },
            complete: function () {
                //hideLoader();
            }
        });
    });

    // ✅ Edit Lead Source
    $(document).on("click", ".edit-leadSource", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let name = row.find("td:eq(1)").text();

        resetLeadSourceForm();
        $("#leadSourceId").val(id);
        $("#leadSourceName").val(name);
    });

    // ✅ Delete Lead Source
    $(document).on("click", ".delete-leadSource", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this lead source?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: userId // set actual logged-in user id
                }

                $.ajax({
                    url: "/api/LeadSource/DeleteLeadSource",
                    type: "POST", // backend expects POST
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            Swal.fire({
                                position: "top-end",
                                icon: "success",
                                title: "Deleted!",
                                showConfirmButton: false,
                                timer: 900
                            });
                            loadLeadSourceList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting lead source."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });
    //EVENT LEAD SOURCE MASTER -- END ----------------------------------------------------------------------

    flatpickr("#eventDate", {
        dateFormat: "d/m/Y",
        disableMobile: true,
        onChange: function () {
            $("#eventDate").trigger("change");
        }
    });

    $("#eventDate, #eventStart, #eventEnd, #banquetHallDropdown").on("change", function () {

        let eventDate = $("#eventDate").val().trim();
        let eventStart = $("#eventStart").val().trim(); // "02:30 PM"
        let eventEnd = $("#eventEnd").val().trim();   // "05:00 PM"
        let banquetId = parseInt($("#banquetHallDropdown").val(), 10) || 0;

        $("#slotStatusMsg").addClass("d-none");

        if (!eventDate || !eventStart || !eventEnd || banquetId <= 0) {
            return;
        }

        var payload = {
            EventDate: eventDate,
            StartTime: eventStart, // ✅ 12-hr AM/PM
            EndTime: eventEnd,   // ✅ 12-hr AM/PM
            BanquetId: banquetId
        };

        console.log("READY TO CALL API", payload);
        showTempLoader('Checking Availability');
        $.ajax({
            url: "/api/EnquiryDeal/CheckBookingSlotAvailability",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal.close(); // Close loader
                if (res.status === "Success") {
                    let isAvailable = res.data?.IsAvailable === 1;
                    let msgBox = $("#slotStatusMsg");
                    msgBox
                        .removeClass("d-none text-danger text-success bg-success-subtle bg-danger-subtle")
                        .addClass(isAvailable
                            ? "text-success bg-success-subtle"
                            : "text-danger bg-danger-subtle")
                        .text(res.message);
                }
            },
            error: function (xhr, status, error) {
                Swal.close(); // Close loader
                $("#slotStatusMsg")
                    .removeClass("d-none text-success")
                    .addClass("text-danger")
                    .text("Unable to check slot availability");
            }
        });
    });

});

//package dropdown -- START --------------------
let foodPackages = [];
function loadFoodPackageList() {
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodItem_Package/GetCompanyFoodPackage_Dropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            if (response.status === "Success") {
                foodPackages = response.data;
                // populate first dropdown
                populateFoodPackageDropdown($("#packageDropdown, #foodPackage_PackageDropdown"));
            }
            else {
                $("#packageDropdown, #foodPackage_PackageDropdown").html(`<option value="0">No Package Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load food packages');
        }
    });
}

// ✅ STEP 2: Reusable dropdown populate
function populateFoodPackageDropdown($dropdown) {
    let options = `<option value="0">-- Select Package --</option>`;
    $.each(foodPackages, (i, pkg) => {
        options += `<option value="${pkg.Id}" data-rateperperson="${pkg.RatePerPerson}">${pkg.PackageName}</option>`;
    });
    $dropdown.html(options);
}
//package dropdown -- END --------------------

//banquet hall details - dropdown -- START ----------------------
function fetchHallDetails() {
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }), // Get all halls
        success: function (response) {
            if (response.status === "Success" && response.data) {

                banquetHallsList = response.data;
                let options = `<option value="0">-- Select Hall --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.HallName} (${item.CapacityRangeText})</option>`;
                });
                $("#banquetDropdown").html(options);
            }
            else {
                $("#banquetDropdown").html(`<option value="0">No Hall Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch hall details');
        }
    });
}
//banquet hall details - dropdown -- END ----------------------

// Load event type list from API
function loadEventTypeList() {
    $.ajax({
        url: "/api/EventType/GetEventTypeList",
        type: "GET",
        success: function (response) {
            console.log("eventType response:", response);
            if (response.status === "Success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select Event Type--</option> `; // default option

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-eventType">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-eventType">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
    `;
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });

                $("#eventTypeTable tbody").html(rows);
                $("#eventTypeDropdown, #eventTypeDropdown_SubTypeModal").html(options);
                //loadEventSubTypeList(eventTypeId);
            } else {
                $("#eventTypeTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#eventTypeDropdown").html(`<option value = "0"> No Event Type Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load EventType List");
        }
    });
}

// Reset Event Type Form
function resetEventTypeForm() {
    $("#eventTypeForm")[0].reset();
    $("#eventTypeId").val(0);
    // Reset error message
    $("#eventTypeName").siblings(".error-msg").addClass("d-none");

}

// ✅ Validation Function for Event Type Form
function validateEventTypeForm() {
    let isValid = true;

    let name = $("#eventTypeName").val().trim();

    // Reset error message
    $("#eventTypeName").siblings(".error-msg").addClass("d-none");

    // Validate Event Type Name
    if (name === "" || name === null) {
        $("#eventTypeName").siblings(".error-msg").removeClass("d-none").text("Please enter event type");
        isValid = false;
    }

    return isValid;
}

// Load event sub-type list from API
function loadEventSubTypeList(eventTypeId = 0) {
    var inputData = {
        EventTypeId: eventTypeId // match your C# EventSubType_Request model
    };

    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeList",
        type: "POST", // Must be POST since backend expects POST
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (response) {
            console.log("eventSubType response:", response);
            if (response.status === "Success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select Event Sub-Type--</option> `;
                let options1 = `<option value = "0"> --Select Event--</option> `;

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}" data-eventtypeid="${item.EventTypeId}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-eventSubType">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-eventSubType">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
    `;
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                    options1 += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });

                $("#eventSubTypeTable tbody").html(rows);
                $("#eventSubTypeDropdown").html(options);
                $("#booking_subEventDropdown").html(options1);
            } else {
                $("#eventSubTypeTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#eventSubTypeDropdown").html(`<option value = "0"> No Event Sub-Type Found</option> `);
                $("#booking_subEventDropdown").html(`<option value = "0"> No Event Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching event sub-type list.");
        }
    });
}

// Reset Event Sub-Type Form
function resetEventSubTypeForm() {
    //$("#eventSubTypeForm")[0].reset();
    $("#eventSubTypeId").val(0);
    $("#eventSubTypeName").val("");

    // Reset error message
    $("#eventSubTypeName").siblings(".error-msg").addClass("d-none");
    $("#eventTypeDropdown_SubTypeModal").siblings(".error-msg").addClass("d-none");
}

// ✅ Event Sub-Type Validation Function
function validateEventSubTypeForm() {
    let isValid = true;

    let eventTypeId = $("#eventTypeDropdown_SubTypeModal").val();
    let name = $("#eventSubTypeName").val().trim();

    // Reset error messages
    $("#eventSubTypeForm .error-msg").addClass("d-none");

    // Validate Event Type
    if (eventTypeId === "0" || !eventTypeId) {
        $("#eventTypeDropdown_SubTypeModal")
            .closest(".flex-grow-1")   // go to parent container
            .find(".error-msg")
            .removeClass("d-none");
        isValid = false;
    }

    // Validate Event Sub-Type Name
    if (!name) {
        $("#eventSubTypeName")
            .closest(".flex-grow-1")   // go to parent container
            .find(".error-msg")
            .removeClass("d-none");
        isValid = false;
    }

    return isValid;
}

// Load banquet hall list from API
function loadBanquetHallList() {
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }), // Get all halls
        success: function (response) {
            console.log("banquetHall response:", response);
            if (response.status === "Success") {
                let options = `<option value = "0"> --Select Banquet Hall--</option> `; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value = "${item.Id}"> ${item.HallName} (${item.CapacityRangeText})</option> `;
                });
                $("#banquetHallDropdown, #banquetDropdown").html(options);
            }
            else {
                $("#banquetHallDropdown, #banquetDropdown").html(`<option value = "0">No Banquet Hall Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Error fetching banquet hall list')
        }
    });
}

// Load lead source list from API
function loadLeadSourceList() {
    ////showLoader()();
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            console.log("leadSource response:", response);
            if (response.status && response.status.toLowerCase() === "success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select Lead Source--</option> `; // default option

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-leadSource">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-leadSource">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
    `;
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });

                $("#leadSourceTable tbody").html(rows);
                $("#leadSourceDropdown, #booking_leadReferenceDropdown").html(options);
            }
            else {
                $("#leadSourceTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#leadSourceDropdown, #booking_leadReferenceDropdown").html(`<option value = "0">No Lead Source Found</option> `);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching lead source list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        }
    });
}

// Reset Lead Source Form
function resetLeadSourceForm() {
    $("#leadSourceForm")[0].reset();
    $("#leadSourceId").val(0);
    // Reset error message
    $("#leadSourceName").siblings(".error-msg").addClass("d-none");
}

// ✅ Validation Function for Lead Source Form
function validateLeadSourceForm() {
    let isValid = true;
    let name = $("#leadSourceName").val().trim();

    // Reset error message
    $("#leadSourceName").siblings(".error-msg").addClass("d-none");

    // Validate Lead Source Name
    if (name === "" || name === null) {
        $("#leadSourceName").siblings(".error-msg").removeClass("d-none").text("Please enter lead source");
        isValid = false;
    }

    return isValid;
}

//<!--file upload / drag-before Moving Enquiry -- START -->


// Reusable function for any dropzone + file input + hidden base64 input
function initDropzone(dropzoneId, fileInputId, hiddenBase64Id) {

    const dz = document.getElementById(dropzoneId);
    const fi = document.getElementById(fileInputId);
    const hb = document.getElementById(hiddenBase64Id);

    // console.log("Init Dropzone Debug:");
    // console.log("Dropzone Element:", dz);
    // console.log("File Input Element:", fi);
    // console.log("Hidden Base64 Input:", hb);

    // On click → open file dialog
    dz.addEventListener("click", () => fi.click());

    // Dragging over
    dz.addEventListener("dragover", (e) => {
        e.preventDefault();
        dz.classList.add("bg-primary", "text-white");
    });

    // Drag leave
    dz.addEventListener("dragleave", () => {
        dz.classList.remove("bg-primary", "text-white");
    });

    // File dropped
    dz.addEventListener("drop", (e) => {
        e.preventDefault();
        dz.classList.remove("bg-primary", "text-white");

        const file = e.dataTransfer.files[0];
        handleFile(file, dz, hb, fi);
    });

    // File selected manually
    fi.addEventListener("change", () => {
        const file = fi.files[0];
        handleFile(file, dz, hb, fi);
    });
}

// Core file handling logic
function handleFile(file, dz, hb, fi) {
    if (!file) return;

    // Validate size (max 5 MB)
    if (file.size > 5 * 1024 * 1024) {
        alert("File size exceeds 5 MB limit!");
        fi.value = "";
        hb.value = "";
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const base64 = e.target.result.split(',')[1];
        hb.value = base64;

        // Update UI with filename
        dz.querySelector("p.fw-semibold").innerText = `Selected: ${file.name} `;
    };

    reader.readAsDataURL(file);
}

// Initialize Dropzone for FollowUp Form
//initDropzone("attachmentDropzone", "attachment", "attachmentBase64_enquiryMove");
initDropzone("attachmentDropzone_followUp", "attachment_followUp", "attachmentBase64_followUp");


//<!-- Clock Time Design -- Delas -> START TIME & END TIME -- END -->

/* --------------------- 🕒 START TIME --------------------- */
const input = document.getElementById('eventStart');
const picker = document.getElementById('clockPicker');
const hourDisplay = document.getElementById('hourDisplay');
const minuteDisplay = document.getElementById('minuteDisplay');
const hourHand = document.getElementById('hourHand');
const minuteHand = document.getElementById('minuteHand');
const amBtn = document.getElementById('amBtn');
const pmBtn = document.getElementById('pmBtn');
const setBtn = document.getElementById('setTimeBtn');

let hour = 12;
let minute = 0;
let ampm = 'AM';
let mode = 'hour';

// Toggle picker visibility
input.addEventListener('click', (e) => {
    e.stopPropagation(); // prevent document click handler
    initializePicker(input.value);
    picker.classList.toggle('d-none');
});

// Initialize picker when opening
function initializePicker(value) {
    let time = value.trim();
    if (!time) time = "12:00 AM"; // Default if empty

    const [timePart, modifier] = time.split(" ");
    let [h, m] = timePart.split(":").map(Number);

    ampm = modifier || 'AM';
    hour = h;
    minute = m;

    hourDisplay.textContent = hour.toString().padStart(2, '0');
    minuteDisplay.textContent = minute.toString().padStart(2, '0');
    hourHand.style.transform = `rotate(${hour * 30}deg)`;
    minuteHand.style.transform = `rotate(${minute * 6}deg)`;

    if (ampm === 'AM') {
        amBtn.classList.add('active');
        pmBtn.classList.remove('active');
    } else {
        pmBtn.classList.add('active');
        amBtn.classList.remove('active');
    }

    setMode('hour');
}

// Switch between hour/minute mode
hourDisplay.addEventListener('click', () => setMode('hour'));
minuteDisplay.addEventListener('click', () => setMode('minute'));

function setMode(m) {
    mode = m;

    if (mode === 'hour') {
        hourDisplay.classList.add('active-text');
        minuteDisplay.classList.remove('active-text');
        minuteDisplay.classList.add('inactive-text');
        hourDisplay.classList.remove('inactive-text');
        hourHand.classList.add('active-hand');
        minuteHand.classList.remove('active-hand');
    } else {
        minuteDisplay.classList.add('active-text');
        hourDisplay.classList.remove('active-text');
        hourDisplay.classList.add('inactive-text');
        minuteDisplay.classList.remove('inactive-text');
        minuteHand.classList.add('active-hand');
        hourHand.classList.remove('active-hand');
    }
}

function hidePickerSlowly(picker) {
    picker.classList.add('picker-transition'); // ensure transition enabled

    // Start fade-out
    picker.classList.add('picker-hide');

    // After animation ends, fully hide
    setTimeout(() => {
        picker.classList.add('d-none');
        picker.classList.remove('picker-hide');
    }, 300); // match CSS duration
}

function showPicker(picker) {
    picker.classList.remove('d-none');
    picker.classList.add('picker-transition');

    // Start from hidden state
    picker.classList.add('picker-hide');

    // Fade-in after reflow
    requestAnimationFrame(() => {
        picker.classList.remove('picker-hide');
    });
}

// Handle number click
picker.querySelectorAll('.clock-number').forEach(num => {
    num.addEventListener('click', () => {
        const value = parseInt(num.textContent);
        if (mode === 'hour') {
            hour = value;
            hourDisplay.textContent = value.toString().padStart(2, '0');
            hourHand.style.transform = `rotate(${value * 30}deg)`;
        } else {
            minute = (value % 12) * 5;
            minuteDisplay.textContent = minute.toString().padStart(2, '0');
            minuteHand.style.transform = `rotate(${minute * 6}deg)`;
        }
    });
});

// AM/PM toggle
amBtn.addEventListener('click', () => {
    ampm = 'AM';
    amBtn.classList.add('active');
    pmBtn.classList.remove('active');
});

pmBtn.addEventListener('click', () => {
    ampm = 'PM';
    pmBtn.classList.add('active');
    amBtn.classList.remove('active');
});

// Set time here1
function setTimeStart() {
    input.value = `${hourDisplay.textContent}:${minuteDisplay.textContent} ${ampm} `;
    //picker.classList.add('d-none');
    input.dispatchEvent(new Event("change", { bubbles: true }));
    hidePickerSlowly(picker);
}
setBtn.addEventListener('click', setTimeStart);

// 🔹 Outside click handler
document.addEventListener('click', (e) => {
    if (!picker.contains(e.target) && e.target !== input) {
        if (!picker.classList.contains('d-none')) {
            setTimeStart(); // auto save selected time
        }
    }
});

setMode('hour');


/* --------------------- 🕓 END TIME --------------------- */
const inputEnd = document.getElementById('eventEnd');
const pickerEnd = document.getElementById('clockPickerEnd');
const hourDisplayEnd = document.getElementById('hourDisplayEnd');
const minuteDisplayEnd = document.getElementById('minuteDisplayEnd');
const hourHandEnd = document.getElementById('hourHandEnd');
const minuteHandEnd = document.getElementById('minuteHandEnd');
const amBtnEnd = document.getElementById('amBtnEnd');
const pmBtnEnd = document.getElementById('pmBtnEnd');
const setBtnEnd = document.getElementById('setTimeBtnEnd');

let hourEnd = 12;
let minuteEnd = 0;
let ampmEnd = 'AM';
let modeEnd = 'hour';

inputEnd.addEventListener('click', (e) => {
    e.stopPropagation();
    initializePickerEnd(inputEnd.value);
    pickerEnd.classList.toggle('d-none');
});

function initializePickerEnd(value) {
    let time = value.trim();
    if (!time) time = "12:00 AM";

    const [timePart, modifier] = time.split(" ");
    let [h, m] = timePart.split(":").map(Number);

    ampmEnd = modifier || 'AM';
    hourEnd = h;
    minuteEnd = m;

    hourDisplayEnd.textContent = hourEnd.toString().padStart(2, '0');
    minuteDisplayEnd.textContent = minuteEnd.toString().padStart(2, '0');
    hourHandEnd.style.transform = `rotate(${hourEnd * 30}deg)`;
    minuteHandEnd.style.transform = `rotate(${minuteEnd * 6}deg)`;

    if (ampmEnd === 'AM') {
        amBtnEnd.classList.add('active');
        pmBtnEnd.classList.remove('active');
    } else {
        pmBtnEnd.classList.add('active');
        amBtnEnd.classList.remove('active');
    }

    setModeEnd('hour');
}

hourDisplayEnd.addEventListener('click', () => setModeEnd('hour'));
minuteDisplayEnd.addEventListener('click', () => setModeEnd('minute'));

function setModeEnd(m) {
    modeEnd = m;

    if (modeEnd === 'hour') {
        hourDisplayEnd.classList.add('active-text');
        minuteDisplayEnd.classList.remove('active-text');
        minuteDisplayEnd.classList.add('inactive-text');
        hourDisplayEnd.classList.remove('inactive-text');
        hourHandEnd.classList.add('active-hand');
        minuteHandEnd.classList.remove('active-hand');
    } else {
        minuteDisplayEnd.classList.add('active-text');
        hourDisplayEnd.classList.remove('active-text');
        hourDisplayEnd.classList.add('inactive-text');
        minuteDisplayEnd.classList.remove('inactive-text');
        minuteHandEnd.classList.add('active-hand');
        hourHandEnd.classList.remove('active-hand');
    }
}

pickerEnd.querySelectorAll('.clock-number').forEach(num => {
    num.addEventListener('click', () => {
        const value = parseInt(num.textContent);
        if (modeEnd === 'hour') {
            hourEnd = value;
            hourDisplayEnd.textContent = value.toString().padStart(2, '0');
            hourHandEnd.style.transform = `rotate(${value * 30}deg)`;
        } else {
            minuteEnd = (value % 12) * 5;
            minuteDisplayEnd.textContent = minuteEnd.toString().padStart(2, '0');
            minuteHandEnd.style.transform = `rotate(${minuteEnd * 6}deg)`;
        }
    });
});

amBtnEnd.addEventListener('click', () => {
    ampmEnd = 'AM';
    amBtnEnd.classList.add('active');
    pmBtnEnd.classList.remove('active');
});

pmBtnEnd.addEventListener('click', () => {
    ampmEnd = 'PM';
    pmBtnEnd.classList.add('active');
    amBtnEnd.classList.remove('active');
});

// Set time
function setTimeEnd() {
    inputEnd.value = `${hourDisplayEnd.textContent}:${minuteDisplayEnd.textContent} ${ampmEnd} `;
    inputEnd.dispatchEvent(new Event("change", { bubbles: true }));
    pickerEnd.classList.add('d-none');
}
setBtnEnd.addEventListener('click', setTimeEnd);

// 🔹 Outside click handler
document.addEventListener('click', (e) => {
    if (!pickerEnd.contains(e.target) && e.target !== inputEnd) {
        if (!pickerEnd.classList.contains('d-none')) {
            setTimeEnd();
        }
    }
});

setModeEnd('hour');
//<!-- Clock Time Design -- Delas -> START TIME & END TIME -- END -->

//BOOKING CALENDAR DESIGN - START ----------------------------------------------------------------------
function resetBooking() {
    // Reset all form fields
    $("#bookingForm").trigger("reset");
    // Reset any special fields
    $("#gstSwitch").prop("checked", false);
    $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
    $("#foodPackageContainer .foodPackage_TotalValue").text("0");
    $("#addonItemsContainer").empty();
    $("#headerSummaryContainer").empty();

    //SET STATE 
    const $stateDropdown = $("#clientStateDropdown");
    // Find option with text "Gujarat"
    const gujaratOption = $stateDropdown.find("option").filter(function () {
        return $(this).text().trim().toLowerCase() === "gujarat";
    });
    if (gujaratOption.length > 0) {
        $stateDropdown.val(gujaratOption.val()).trigger("change");
    }
    else {
        $stateDropdown.val("0").trigger("change");
    }

    // ✅ Store FULL package array in button as JSON
    $("#btnFoodLabels").data("foodpackage", "");

    // Reset amounts to defaults
    //resetOtherCharges(false);
    recalculatePaymentSummary();
    $("#Section_PaymentDetails").show();
    //console.log("Booking form reset completed.");

    // Hide dynamic sections manually
    $("#PaymentDetails_TransactionId").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeBank").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeNo").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeDate").closest(".col-md-6").addClass("d-none");
    // Optionally reset dropdown to default
    $("#PaymentDetails_Mode").val("0").trigger("change");

    // Re-select default account manually
    const defaultOption = $("#PaymentDetails_AccountId option").filter(function () {
        return $(this).data("isdefault") === true;
    });
    if (defaultOption.length) {
        $("#PaymentDetails_AccountId").val(defaultOption.val());
    }

}
function recalculatePaymentSummary() {

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gstRate = 5;

    let taxableAmount = 0;

    // 🔹 Hall Rent
    let hallRent = parseFloat($("#booking_hallRent").val()) || 0;
    taxableAmount += hallRent;
    //console.log("hallRent:", hallRent);

    // 🔹 Default Food
    //let defaultFoodTotal = parseFloat($("#foodPackage_TotalValue").text()) || 0;
    //taxableAmount += defaultFoodTotal;
    //console.log("defaultFoodTotal:", defaultFoodTotal);

    // 🔹 Extra Food Rows
    $(".foodPackage_TotalValue").each(function () {
        let value = parseFloat($(this).text()) || 0;
        taxableAmount += value;
        //console.log("defaultFoodTotal rows:", value);
    });

    // 🔹 Other Charges
    let decoration = parseFloat($("#OtherCharges_Decoration_Amount").val()) || 0;
    let room = parseFloat($("#OtherCharges_RoomCharges_Amount").val()) || 0;
    let sound = parseFloat($("#OtherCharges_SoundSystem_Amount").val()) || 0;
    let projector = parseFloat($("#OtherCharges_ProjectorScreen_Amount").val()) || 0;

    taxableAmount += decoration + room + sound + projector;
    //console.log("decoration:", decoration);
    //console.log("room:", room);
    //console.log("sound:", sound);
    //console.log("projector:", projector);

    $("#paymentSummary_TaxableAmount").text(taxableAmount.toFixed(2));
    //console.log("taxableAmount:", taxableAmount);

    // 🔹 GST
    let totalGST = isGSTEnabled ? (taxableAmount * gstRate) / 100 : 0;

    let cgst = isGSTEnabled ? totalGST / 2 : 0;
    let sgst = isGSTEnabled ? totalGST / 2 : 0;
    let igst = 0;

    $("#paymentSummary_CGST").text(cgst.toFixed(2));
    $("#paymentSummary_SGST").text(sgst.toFixed(2));
    $("#paymentSummary_IGST").text(igst.toFixed(2));

    let total = taxableAmount + cgst + sgst + igst;

    $("#paymentSummary_Total").text(total.toFixed(2));
    //console.log("total:", total);

    applyBookingDiscount(total);
}
function applyBookingDiscount(totalAmount) {

    let type = $("#paymentSummary_BookingAmount_Type").val();
    let enteredValue = parseFloat($("#paymentSummary_BookingAmount_Amount").val()) || 0;

    let bookingDiscount = 0;

    if (type == "1") {
        bookingDiscount = enteredValue;
    }
    else if (type == "2") {
        bookingDiscount = (totalAmount * enteredValue) / 100;
    }

    if (bookingDiscount > totalAmount)
        bookingDiscount = totalAmount;

    $("#paymentSummary_BookingAmount").text(bookingDiscount.toFixed(2));

    let payableAmount = totalAmount - bookingDiscount;

    $("#paymentSummary_payableAmount").text(payableAmount.toFixed(2));
}

//SAVE BOOKING ------ START ------------------------------
$("#btnBook").on("click", function () { saveBooking(); });
$("#btnReset").on("click", function () { resetBooking(); });
//SAVE BOOKING ------ END --------------------------------

function saveBooking() {
    //const form = $("#bookingForm");
    const $label = $("#label_date_shift");

    let userId = parseInt(sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0);

    var quillCustomer = new Quill('#OtherCharges_customerEditor', {
        theme: 'snow'
    });

    var quillInternal = new Quill('#OtherCharges_internalEditor', {
        theme: 'snow'
    });

    var customerNote = quillCustomer.root.innerHTML;
    var internalNote = quillInternal.root.innerHTML;

    // 🔹 1. Collect Booking Data
    const bookingData = {
        BookingId: parseInt($("#BookingId").val() || 0),
        EventDate: $label.data("eventdate"),
        ShiftCode: $label.data("shiftid"),
        GSTAccepted: $("#gstSwitch").is(":checked"),

        //Dates
        BanquetId: parseInt($("#banquetDropdown").val() || 0),
        FromDate: $("#fromDate").val(),
        TillDate: $("#toDate").val(),

        //Time
        StartTime: `${$("#startHour").val()}:${$("#startMinute").val()} ${$("#startAMPM").val()}`,
        EndTime: `${$("#endHour").val()}:${$("#endMinute").val()} ${$("#endAMPM").val()}`,

        // Client Info
        Client_EnquiryId: parseInt($("#clientEnquiryDropdown").val() || 0),
        Client_Name: $("#clientName").val(),
        Client_Email: $("#clientEmail").val(),
        Client_Address: $("#clientAddress").val(),
        Client_Contact: $("#clientContact").val(),
        Client_GSTIN: $("#clientGSTIN").val(),
        Client_StateId: parseInt($("#clientStateDropdown").val() || 0),

        // Booking Details
        Booking_SubEventId: parseInt($("#booking_subEventDropdown").val() || 0),
        Booking_PAX: parseInt($("#booking_pax").val() || 0),
        Booking_HallRent: parseInt($("#booking_hallRent").val() || 0),
        Booking_GST: parseFloat($("#booking_gst").val() || 0),
        Booking_LeadReferenceId: parseInt($("#booking_leadReferenceDropdown").val() || 0),
        Booking_MenuType_Id: parseInt($("input[name='menuType']:checked").val() || 0),
        Booking_TypeId: parseInt($("#BookingType").val() || 0),
        Booking_IsAgent: $("#AgentCheck").is(":checked"),

        //Payment
        Payment_ModeId: parseInt($("#PaymentDetails_Mode").val() || 0),
        PaymentDetails_AccountId: parseInt($("#PaymentDetails_AccountId").val() || 0),
        PaymentDetails_TransactionId: $("#PaymentDetails_TransactionId").val(),
        PaymentDetails_ChequeBank: $("#PaymentDetails_ChequeBank").val(),
        PaymentDetails_ChequeNo: $("#PaymentDetails_ChequeNo").val(),
        PaymentDetails_ChequeDate: $("#PaymentDetails_ChequeDate").val(),
        Payment_Amount: parseFloat($("#PaymentDetails_Amount").val()) || 0,
        Payment_PaidTime: $("#PaymentDetails_PaidTime").val(),
        Payment_DueDate: $("#PaymentDetails_DueDate").val(),
        Payment_Note: $("#PaymentDetails_PaymentNote").val(),

        // ✅ Other Charges
        OtherCharges_Decoration_Amount: parseFloat($("#OtherCharges_Decoration_Amount").val() || 0),
        OtherCharges_Decoration_GST: parseFloat($("#OtherCharges_Decoration_GST").val() || 0),
        OtherCharges_RoomCharges_Amount: parseFloat($("#OtherCharges_RoomCharges_Amount").val() || 0),
        OtherCharges_RoomCharges_GST: parseFloat($("#OtherCharges_RoomCharges_GST").val() || 0),
        OtherCharges_SoundSystem_Amount: parseFloat($("#OtherCharges_SoundSystem_Amount").val() || 0),
        OtherCharges_SoundSystem_GST: parseFloat($("#OtherCharges_SoundSystem_GST").val() || 0),
        OtherCharges_ProjectorScreen_Amount: parseFloat($("#OtherCharges_ProjectorScreen_Amount").val() || 0),
        OtherCharges_ProjectorScreen_GST: parseFloat($("#OtherCharges_ProjectorScreen_GST").val() || 0),
        OtherCharges_BookingNoteForCustomer: customerNote,
        OtherCharges_InternalOfficeNote: internalNote,

        // ✅ Payment Summary
        PaymentSummary_TaxableAmount: parseFloat($("#paymentSummary_TaxableAmount").text() || 0),
        PaymentSummary_CGST: parseFloat($("#paymentSummary_CGST").text() || 0),
        PaymentSummary_SGST: parseFloat($("#paymentSummary_SGST").text() || 0),
        PaymentSummary_IGST: parseFloat($("#paymentSummary_IGST").text() || 0),
        PaymentSummary_Total: parseFloat($("#paymentSummary_Total").text() || 0),
        PaymentSummary_BookingDiscount_Type_Id: parseInt($("#paymentSummary_BookingAmount_Type").val() || 0),
        PaymentSummary_BookingDiscount_Type_Amount: parseFloat($("#paymentSummary_BookingAmount_Amount").val() || 0),
        PaymentSummary_BookingDiscount_Total: parseFloat($("#paymentSummary_BookingAmount").text() || 0),
        PaymentSummary_PayableAmount: parseFloat($("#paymentSummary_payableAmount").text() || 0),

        ActionBy: userId
    };

    // 🔹 2. Build Food Packages JSON --here
    const foodPackages = [];

    $(".foodPackageContainer > .food-block, #foodPackageContainer > .food-row").each(function () {
        const $row = $(this);

        // Check if it's a dynamic block (has food-block class) or first row
        const isDynamicBlock = $row.hasClass('food-block');

        // Get common elements
        const $dropdown = $row.find(".foodPackageDropdown, #foodPackage_PackageDropdown");
        const selectedMenuItems = $dropdown.data("selectedItems") || [];

        const $dropdown_AddOns = $row.find(".foodPackage_AddOnItems");
        const selectedAddOnCharges = $dropdown_AddOns.data("selected-AddOnItems") || [];

        // Create base package object
        const foodPackage = {
            Id: parseInt($row.find("package_row_id").val() || 0),
            PackageId: parseInt($dropdown.val()),
            PerPlate: parseFloat($row.find(".perPlate, #foodPackage_RatePerPlate").val() || 0),
            PackageCost: parseFloat($row.find(".packageCost, #foodPackage_PackageCost").val() || 0),
            AddonsCost: parseFloat($row.find(".addonsCost, #foodPackage_Addons").val() || 0),
            GST: parseFloat($row.find(".gstCost, #foodPackage_GST").val() || 0),
            TotalCharges: parseFloat($row.find(".foodPackage_TotalValue, #foodPackage_TotalValue").text() || 0),
            MenuItems: selectedMenuItems,
            AddOns: selectedAddOnCharges
        };

        // Add additional fields for dynamic blocks
        if (isDynamicBlock) {
            //console.log("new row added");
            const $packageNameInput = $row.find('input[placeholder="Addon Food Package Name"]');
            const $paxInput = $row.find('input[placeholder="Pax"]');
            const $dateInput = $row.find('input[type="date"]');
            const $hourInput = $row.find('input[placeholder="HH"]');
            const $minuteInput = $row.find('input[placeholder="MM"]');
            const $ampmSelect = $row.find('select');

            // Construct time string (HH:MM AM/PM)
            const hour = $hourInput.val() || '12';
            const minute = $minuteInput.val() || '00';
            const ampm = $ampmSelect.val() || 'AM';
            const timeString = `${hour}:${minute} ${ampm}`;

            // Add additional fields
            foodPackage.FoodPackageTitleName = $packageNameInput.val() || '';
            foodPackage.Pax = $paxInput.val() || 0;
            foodPackage.FoodPackageDateTime = $dateInput.val() ? `${$dateInput.val()} ${timeString}` : '';
        }
        else {
            //console.log("static row only");
            // For first row, set default values for optional fields
            foodPackage.FoodPackageTitleName = '';
            foodPackage.Pax = 0;
            foodPackage.FoodPackageDateTime = '';
        }

        foodPackages.push(foodPackage);
    });

    // 🔹 3. Attach as JSON string
    bookingData.JSON_FoodPackage = foodPackages;

    //console.log("Final Booking JSON:", bookingData);

    // 🔹 4. AJAX POST
    $.ajax({
        url: "/api/BookingCalendar/SaveBookingCalendar",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(bookingData),
        beforeSend: function () {
            $("#btnSaveBooking").prop("disabled", true).text("Saving...");
        },
        success: function (response) {
            //console.log("save response:", response);
            if (response.status === "Success") {
                handleApiSuccess();
                resetBooking();
                $("#bookingModal").modal("hide");
            }
            else {
                Swal.fire({
                    title: "Booking Save Failed",
                    text: response.message,
                    icon: "error"
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Booking Save Error");
        },
        complete: function () {
            $("#btnSaveBooking").prop("disabled", false).text("Save Booking");
        }
    });
}
function setGSTSwitch(isGSTAccepted) {
    const gstSwitch = document.getElementById("gstSwitch");
    const gstText = document.getElementById("gstSwitchText");

    gstSwitch.checked = !!isGSTAccepted;

    if (gstSwitch.checked) {
        gstText.textContent = "On";
        gstText.classList.remove("text-danger");
        gstText.classList.add("text-success");
    }
    else {
        gstText.textContent = "Off";
        gstText.classList.remove("text-success");
        gstText.classList.add("text-danger");
    }
}
function setShiftTime(shiftAbbr) {
    const times = shiftTimes[shiftAbbr];
    if (!times) return; // invalid shift — do nothing

    // Parse start time
    const [startH, startM, startAMPM] = times.start.match(/(\d+):(\d+)\s*(AM|PM)/).slice(1);
    $("#startHour").val(startH);
    $("#startMinute").val(startM);
    $("#startAMPM").val(startAMPM);

    // Parse end time
    const [endH, endM, endAMPM] = times.end.match(/(\d+):(\d+)\s*(AM|PM)/).slice(1);
    $("#endHour").val(endH);
    $("#endMinute").val(endM);
    $("#endAMPM").val(endAMPM);
}

//Enquiry Client - dropdown -- START ----------------------
function loadEnquiryList() {
    //showLoader();
    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealList",
        type: "GET",
        success: function (response) {
            //console.log("enquiry list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Client --</option>`; // default option
                //options += `<option value="0">+ Add New Client</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `
                               <option
                                   value="${item.Id}"
                                   data-name="${item.Name || ''}"
                                   data-email="${item.Email || ''}"
                                   data-address="${item.Address || ''}"
                                   data-contact="${item.Contact || ''}"
                               >${item.Name} (${item.Contact})</option>`;
                });
                $("#clientEnquiryDropdown").html(options);

                 $("#clientEnquiryDropdown").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#clientName").val(selected.data("name") || "");
                    $("#clientEmail").val(selected.data("email") || "");
                    $("#clientAddress").val(selected.data("address") || "");
                    $("#clientContact").val(selected.data("contact") || "");
                }); 
            } else {
                $("#clientEnquiryDropdown").html(`<option value="0">No Enquiry Client Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load enquiry list');
        }
    });
}
//Enquiry Client - dropdown -- END ------------------------

//State - dropdown -- START ----------------------
function loadStateList() {
    //showLoader();
    $.ajax({
        url: "/api/State/GetStateList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0     // or specific Id if needed
        }),
        success: function (response) {
            //console.log("state list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select State --</option>`; // default option
                let gujaratId = 0;
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                    if (item.Name.toLowerCase() === "gujarat") {
                        gujaratId = item.Id;
                    }
                });
                $("#clientStateDropdown").html(options);
                // Set default selection if found
                if (gujaratId > 0) {
                    $("#clientStateDropdown").val(gujaratId);
                }
            } else {
                $("#clientStateDropdown").html(`<option value="0">No State Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load state list');
        }
    });
}
//State - dropdown -- END ------------------------

// food Package Dropdown -------------------------------------------
$(document).on("change", ".foodPackageDropdown", function () {
    const $row = $(this).closest(".food-row");

    const $dropdown = $row.find(".foodPackageDropdown");

    const selectedPackageId = $dropdown.val();

    if (!selectedPackageId || selectedPackageId === "0") {
        Swal.fire({
            icon: "warning",
            title: "Select Package",
            text: "Please select a food package first."
        });
        return;
    }
    const selectedOption = $dropdown.find("option:selected");
    const selectedText = selectedOption.text();
    const ratePerPerson = selectedOption.data("rateperperson") || 0;

    //console.log("selectedPackageId:", selectedPackageId, "selectedText:", selectedText);

    // Set Per Plate inside same row
    $row.find(".perPlate, #foodPackage_RatePerPlate").val(ratePerPerson);

    updatePackageCost($row);  // Pass row if needed

})

// ✅ STEP 3: Attach global delegated change event for ALL dropdowns
$(document).on("click", ".loadMenuBtn", function () {
    const $row = $(this).closest(".food-row");
    const $dropdown = $row.find(".foodPackageDropdown");
    const selectedPackageId = $dropdown.val();

    if (!selectedPackageId || selectedPackageId === "0") {
        Swal.fire({
            icon: "warning",
            title: "Select Package",
            text: "Please select a food package first."
        });
        return;
    }

    const selectedOption = $dropdown.find("option:selected");
    const selectedText = selectedOption.text();
    const ratePerPerson = selectedOption.data("rateperperson") || 0;

    //console.log("selectedPackageId:", selectedPackageId, "selectedText:", selectedText);

    // Set Per Plate inside same row
    //$row.find(".perPlate, #foodPackage_RatePerPlate").val(ratePerPerson);

    //updatePackageCost($row);  // Pass row if needed

    $.ajax({
        url: "/api/BookingCalendar/GetPackage_MenuAndFoodItemsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Package_Id: selectedPackageId }),
        success: function (response) {
            if (response.status === "Success" && response.data.length > 0) {
                renderMenuItems(response.data);
                $("#menuModal .modal-title").text(`Menu Items [${selectedText}]`);
                //$("#bookingModal").modal("hide");
                $("#menuModal").modal("show");
                // Store dropdown reference safely
                $("#menuModal").data("activeDropdown", $dropdown);

                // 🔥 RESTORE SELECTION
                const savedItems = $dropdown.data("selectedItems");
                //console.log("savedItems:", savedItems);
                restoreSavedMenuSelection(savedItems);
            }
            else {
                Swal.fire({
                    icon: "info",
                    title: "Menu Items Not Found",
                    text: "No menu items found for this package."
                });
                $dropdown.val("0");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Package MenuItems list");
        }
    });
});
function restoreSavedMenuSelection(savedItems) {
    if (!savedItems || savedItems.length === 0) return;
    savedItems.forEach(item => {
        const section = $(`#menuModal .menu-section[data-menu-id='${item.FoodMenu_Id}']`);
        if (!section.length) return;

        // 🔹 EXISTING ITEM
        if (item.FoodItem_Id && item.FoodItem_Id !== 0) {
            section.find(`.item-checkbox[value='${item.FoodItem_Id}']`).prop("checked", true);
        }

        // 🔹 NEW CUSTOM ITEM (ID = 0)
        else if (item.FoodItem_Id === 0 && item.FoodItem_Name_NewAdded) {
            const newItemHtml = `
                <div class="form-check me-3 mb-2">
                    <input class="form-check-input item-checkbox new-item"
                           type="checkbox"
                           value="0"
                           data-menu-id="${item.FoodMenu_Id}"
                           data-item-name="${item.FoodItem_Name_NewAdded}"
                           checked>
                    <label class="form-check-label">${item.FoodItem_Name_NewAdded}</label>
                </div>
            `;
            section.find(".item-container").append(newItemHtml);
        }
    });
    updateSelectAllState();
}

function updateSelectAllState() {
    $(".menu-section").each(function () {
        const allItems = $(this).find(".item-checkbox");
        const checkedItems = $(this).find(".item-checkbox:checked");

        const selectAllCheckbox = $(this).find(".select-all");

        if (allItems.length === checkedItems.length && allItems.length > 0) {
            selectAllCheckbox.prop("checked", true);
        } else {
            selectAllCheckbox.prop("checked", false);
        }
    });
}

function updatePackageCost() {

    let pax = parseFloat($("#booking_pax").val()) || 0;
    let rate = parseFloat($("#foodPackage_RatePerPlate").val()) || 0;

    let addOnsCost = parseFloat($("#foodPackage_Addons").val()) || 0;

    let packageCost = pax * rate;

    $("#foodPackage_PackageCost").val(packageCost.toFixed(2));
    packageCost = packageCost + addOnsCost;

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    //let gst = isGSTEnabled ? getGST(hallRent) : 0;

    let gstAmount = isGSTEnabled ? ((packageCost * 5) / 100) : 0;
    $("#foodPackage_GST").val(gstAmount.toFixed(2));

    let totalCharges = packageCost + gstAmount;

    $("#foodPackage_TotalValue").text(totalCharges.toFixed(2));

    recalculatePaymentSummary();  // 🔥 IMPORTANT
}

$(document).on("input", "#foodPackage_RatePerPlate, #booking_pax", function () {
    updatePackageCost();
});

$(document).on("input", ".food-block .pax, .food-block .perPlate, .food-block .addonsCost", function () {
    const row = $(this).closest(".food-block");
    calculateFoodRowTotals(row);
});

$(document).on("input change", "#paymentSummary_BookingAmount_Type, #paymentSummary_BookingAmount_Amount",
    function () {
        recalculatePaymentSummary();
    });

$(document).on("input", "#OtherCharges_Decoration_Amount, #OtherCharges_RoomCharges_Amount, #OtherCharges_SoundSystem_Amount, #OtherCharges_ProjectorScreen_Amount",
    function () {
        recalculateAllOtherCharges();
        recalculatePaymentSummary();
    });

function recalculateAllOtherCharges() {
    calculateOtherCharge("#OtherCharges_Decoration_Amount", "#OtherCharges_Decoration_GST");
    calculateOtherCharge("#OtherCharges_RoomCharges_Amount", "#OtherCharges_RoomCharges_GST");
    calculateOtherCharge("#OtherCharges_SoundSystem_Amount", "#OtherCharges_SoundSystem_GST");
    calculateOtherCharge("#OtherCharges_ProjectorScreen_Amount", "#OtherCharges_ProjectorScreen_GST");
}

function calculateOtherCharge(amountSelector, gstSelector) {

    let amount = parseFloat($(amountSelector).val()) || 0;
    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gstRate = 5;

    let gstAmount = 0;

    if (isGSTEnabled) {
        gstAmount = (amount * gstRate) / 100;
    }

    $(gstSelector).val(gstAmount.toFixed(2));
}

//Calculate PAckage for Extra added package rows
function calculateFoodRowTotals(rowElement) {

    let pax = parseFloat($(rowElement).find(".pax").val()) || 0;
    let perPlate = parseFloat($(rowElement).find(".perPlate").val()) || 0;
    let addOnsCost = parseFloat($(rowElement).find(".addonsCost").val()) || 0;

    let packageCost = pax * perPlate;
    $(rowElement).find(".packageCost").val(packageCost.toFixed(2));

    packageCost = packageCost + addOnsCost;

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gst = isGSTEnabled ? ((packageCost * 5) / 100) : 0;

    $(rowElement).find(".gstCost").val(gst.toFixed(2));

    let total = packageCost + gst;
    //console.log("total:", total, ", packageCost:", packageCost, ", gst:", gst);
    $(rowElement).find(".foodPackage_TotalValue").text(total.toFixed(2));

    recalculatePaymentSummary(); // 🔥 IMPORTANT
}

// When dropdown changes → fill Per Plate and recalc
$(document).on("change", ".foodPackageDropdown", function () {
    const row = $(this).closest(".food-block");
    const rate = $(this).find("option:selected").data("rateperperson") || 0;

    row.find(".perPlate").val(rate);
    calculateFoodRowTotals(row);
});

// When Pax OR Per Plate OR Addons change → recalc
$(document).on("input", ".pax, .perPlate, .addonsCost", function () {
    const row = $(this).closest(".food-block");
    calculateFoodRowTotals(row);
});

// ✅ STEP 4: Build modal dynamically
function renderMenuItems(menuData) {
    let html = "";
    menuData.forEach(menu => {
        const foodItems = menu.FoodItemsList?.length
            ? menu.FoodItemsList.map(item => `
                <div class="form-check me-3 mb-2">
                    <input class="form-check-input item-checkbox" type="checkbox"
                        value="${item.Item_Id}" data-menu-id="${menu.FoodMenu_Id}">
                    <label class="form-check-label">${item.Item_Name}</label>
                </div>`).join("")
            : `<p class="text-muted">No items found.</p>`;

        html += `
            <div class="border rounded p-3 mb-3 bg-light menu-section" data-menu-id="${menu.FoodMenu_Id}">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="mb-0 fw-bold">${menu.FoodMenu_Name}</h6>
                    <div>
                        <input type="checkbox" class="form-check-input select-all" id="selectAll_${menu.FoodMenu_Id}">
                        <label for="selectAll_${menu.FoodMenu_Id}" class="form-check-label">Select All</label>
                    </div>
                </div>
                <div class="d-flex flex-wrap item-container">${foodItems}</div>
                <div class="d-flex mt-2">
                    <input type="text" class="form-control form-control-sm me-2 new-item-input" placeholder="Add New Item">
                    <button type="button" class="btn btn-sm btn-primary btn-addNewItem">Add</button>
                </div>
            </div>`;
    });

    $("#menuModalBody").html(html);
}

//ADD NEW ITEM
$(document).on("click", ".btn-addNewItem", function () {

    let section = $(this).closest(".menu-section");
    let menuId = section.data("menu-id");

    let input = section.find(".new-item-input");
    let itemName = input.val().trim();

    if (!itemName) {
        alert("Please enter item name");
        return;
    }

    let newItemHtml = `
        <div class="form-check me-3 mb-2">
            <input class="form-check-input item-checkbox new-item"
                   type="checkbox"
                   value="0"
                   data-menu-id="${menuId}"
                   data-item-name="${itemName}"
                   checked>
            <label class="form-check-label">${itemName}</label>
        </div>
    `;

    section.find(".item-container").append(newItemHtml);

    input.val("");
});

// ✅ STEP 5: Modal “Select All” and “Save” buttons
$(document).on("change", ".select-all", function () {
    const menuId = $(this).attr("id").split("_")[1];
    const checked = $(this).is(":checked");
    $(`.item-checkbox[data-menu-id='${menuId}']`).prop("checked", checked);
});

//StEP 6: Save MENU
$(document).on("click", ".btn-saveMenu", function () {
    let selectedMenuData = [];

    $(".menu-section").each(function () {
        const menuId = $(this).data("menu-id");
        $(this).find(".item-checkbox:checked").each(function () {
            let isNew = $(this).hasClass("new-item");
            selectedMenuData.push({
                FoodMenu_Id: parseInt(menuId),
                FoodItem_Id: parseInt($(this).val()),
                FoodItem_Name_NewAdded: isNew ? $(this).data("item-name") : null,
                IsChecked: $(this).is(":checked")
            });
        });
    });

    // attach selected items to the dropdown that opened modal
    const $activeDropdown = $("#menuModal").data("activeDropdown");
    if ($activeDropdown) {
        $activeDropdown.data("selectedItems", selectedMenuData);
        $activeDropdown.attr("data-selected-items", JSON.stringify(selectedMenuData));
    }

    //console.log("Saved Menu Selection:", selectedMenuData);
    $("#menuModal").modal("hide");
    //$("#bookingModal").modal("show");
});

//Food Package - dropdown -- END----------------------

//Booking - Food Package --- START --------------------------------------------------
const addFoodBtn = document.getElementById('addFoodBtn');
const foodContainer = document.getElementById('foodPackageContainer');
let addonCounter = 1;

addFoodBtn.addEventListener('click', () => {
    const newRow = document.createElement('div');
    newRow.className = 'food-block border rounded p-3 mt-3';

    newRow.innerHTML = `
            <!-- Row 1 -->
            <div class="row g-2 align-items-end p-2 mb-1">
                <div class="col-md-4">
                    <label class="form-label">Addon Food Package #${addonCounter}</label>
                    <input type="text" class="form-control form-control-sm" placeholder="Addon Food Package Name">
                </div>
                <div class="col-md-1">
                    <label class="form-label">Pax</label>
                    <input type="number" class="form-control form-control-sm pax" placeholder="Pax" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Date</label>
                    <input type="date" class="form-control form-control-sm">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Time</label>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control form-control-sm" placeholder="HH" min="1" max="12" style="width: 70px;">
                        <input type="number" class="form-control form-control-sm" placeholder="MM" min="0" max="59" style="width: 70px;">
                        <select class="form-select form-select-sm" style="width: 90px;">
                            <option value="AM">AM</option>
                            <option value="PM">PM</option>
                        </select>
                    </div>
                </div>
                <div class="col-md-2 d-flex align-items-end justify-content-end">
                    <button type="button" class="btn btn-outline-danger btn-sm remove-btn">&times;</button>
                </div>
            </div>

            <!-- Row 2 -->
            <div class="row g-2 food-row align-items-end mt-1">
                <div class="col-md-3">
                    <label class="form-label">Food Package</label>
                    <div class="d-flex gap-2">
                        <select class="form-select form-select-sm foodPackageDropdown"></select>
                        <button type="button"
                                class="btn btn-outline-primary btn-sm loadMenuBtn">
                            Menu
                        </button>
                    </div>
                    <input type="hidden" class="package_row_id" value="0" />
                </div>
                <div class="col-md-1">
                    <label class="form-label">Per Plate</label>
                    <input type="number" class="form-control form-control-sm perPlate" placeholder="Per Plate" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Package Cost</label>
                    <input type="number" class="form-control form-control-sm packageCost" placeholder="Package Cost" value="0" readonly>
                </div>
                <div class="col-md-1">
                    <label class="form-label">Addons</label>
                    <input type="number" class="form-control form-control-sm addonsCost" placeholder="Addons" value="0" readonly>
                </div>
                <div class="col-md-1">
                    <label class="form-label">GST</label>
                    <input type="number" class="form-control form-control-sm gstCost" placeholder="GST" value="0" readonly>
                </div>
                <div class="col-md-4 d-flex align-items-center justify-content-between">
                    <button type="button" class="btn btn-outline-success btn-sm mt-2 foodPackage_AddOnItems" onclick="event.preventDefault(); openAddonModal(0, this)"><i class="fa fa-check"></i> Addons</button>
                    <span class="fw-bold">Total: <span class="foodPackage_TotalValue">0</span></span>
                </div>
            </div>
        `;

    foodContainer.appendChild(newRow);

    // Populate dropdown in new row
    const dropdown = newRow.querySelector(".foodPackageDropdown");
    populateFoodPackageDropdown($(dropdown));

    // On change of dropdown, set Per Plate rate
    // dropdown.addEventListener('change', function () {
    //     const selected = this.options[this.selectedIndex];
    //     const rate = selected.dataset.rateperperson || 0;
    //     newRow.querySelector(".perPlate").value = rate;
    // });

    // Remove functionality
    newRow.querySelector('.remove-btn').addEventListener('click', () => {
        foodContainer.removeChild(newRow);
    });

    addonCounter++;
});
//Booking - Food Package --- /END --------------------------------------------------

//Food Addons ----- START ----------------------------------------------------------------
let addon_Data = [];
let headerTotals = {};
function loadAddonCharges() {
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodRate_AddonCharge/GetCompanyFoodRateAddonCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            if (res.status === "Success" && res.data && res.data.length > 0) {
                addon_Data = res.data;
                populateAddonModal();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load addon charges');
        }
    });
}

function populateAddonModal(savedItems = []) {
    //console.log("savedItems AddOns:", savedItems);
    let container = $("#addonItemsContainer");
    let summaryContainer = $("#headerSummaryContainer");
    container.empty();
    summaryContainer.empty();
    headerTotals = {};

    // Group data by header
    let grouped = {};
    addon_Data.forEach(item => {
        if (!grouped[item.HeaderName]) grouped[item.HeaderName] = [];
        grouped[item.HeaderName].push(item);
    });

    Object.keys(grouped).forEach(header => {
        headerTotals[header] = 0;

        let headerCard = $(`
                <div class="card shadow-sm border-0">
                    <div class="card-header d-flex justify-content-between align-items-center bg-light py-2">
                        <span class="fw-bold">${header}</span>
                        <button class="btn btn-xs btn-outline-secondary toggle-btn">−</button>
                    </div>
                    <div class="card-body p-2" data-header-body="${header}"></div>
                </div>
            `);

        // Column headers
        let columnHeader = $(`
                <div class="d-flex fw-bold text-secondary border-bottom pb-1 mb-2">
                    <div class="flex-grow-1">Food Item</div>
                    <div style="width:90px;" class="text-center">Rate</div>
                    <div style="width:90px;" class="text-center">Pax</div>
                    <div style="width:90px;" class="text-center">Total</div>
                </div>
            `);
        headerCard.find(`[data-header-body="${header}"]`).append(columnHeader);

        // Populate items
        grouped[header].forEach(item => {
            // Find if this item was saved previously
            let saved = savedItems.find(s => s.AddOn_Item_Id === item.Id && s.AddOn_Header_Id === item.HeaderId);
            //console.log("saved AddOns:", saved);
            // If found, prefill values
            let isChecked = saved ? true : false;
            let rate = saved ? saved.AddOn_Rate : item.Amount;
            //let pax = saved ? saved.AddOn_Pax : ($("#booking_pax").val() || 0);

            let rowPax = 0;
            if (currentAddonRow && currentAddonRow.length > 0) {
                let paxInput = currentAddonRow.find(".pax");

                if (paxInput.length > 0) {
                    rowPax = parseFloat(paxInput.val()) || 0;
                }
                else {
                    // fallback for first default row (which doesn't have .pax)
                    rowPax = parseFloat($("#booking_pax").val()) || 0;
                }
            }
            let pax = saved ? saved.AddOn_Pax : rowPax;

            let total = saved ? saved.AddOn_Total : 0;

            let row = $(`
                    <div class="d-flex align-items-center justify-content-between mb-2"
                         data-header="${item.HeaderName}" data-headerid="${item.HeaderId}" data-id="${item.Id}">

                        <div class="d-flex align-items-center flex-grow-1">
                            <input type="checkbox" class="form-check-input me-2 addon-checkbox" ${isChecked ? "checked" : ""}>
                            <span>${item.ItemName}</span>
                        </div>

                        <input type="number" class="form-control form-control-sm rate-input text-center mx-1" step="1"
                               value="${rate}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm pax-input text-center mx-1" step="1"
                               value="${pax}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm total-input text-center mx-1 bg-light"
                               value="${total.toFixed(2)}" readonly style="width:90px;">
                    </div>
                `);

            row.find(".rate-input, .pax-input").on("input", function () {
                //console.log("here1");
                recalcRow_AddOn(row);
            });

            row.find(".addon-checkbox").on("change", function () {
                recalcRow_AddOn(row);
            });

            headerCard.find(`[data-header-body="${header}"]`).append(row);
        });

        container.append(headerCard);

        // Summary row
        summaryContainer.append(`
                <div class="d-flex justify-content-between mb-1 fs-9" data-header-summary="${header}">
                    <span>${header}</span>
                    <span class="header-total">0</span>
                </div>
            `);

        // Toggle button
        headerCard.find(".toggle-btn").on("click", function () {
            let body = headerCard.find(`[data-header-body="${header}"]`);
            let isVisible = body.is(":visible");
            body.toggle();
            $(this).text(isVisible ? "+" : "−");
        });
    });

    $("[data-header]").each(function () {
        recalcRow_AddOn($(this));
    });
}

function recalcRow_AddOn(row) {
    //console.log("here3");
    let checked = row.find(".addon-checkbox").is(":checked");
    let rate = parseFloat(row.find(".rate-input").val()) || 0;
    let pax = parseFloat(row.find(".pax-input").val()) || 0;
    let total = checked ? rate * pax : 0;

    //console.log("---- Addon Row Calculation ----");
    //console.log("Checked:", checked);
    //console.log("Rate:", rate);
    //console.log("Pax:", pax);
    //console.log("Computed Total:", total);
    //console.log("-------------------------------");

    row.find(".total-input").val(total.toFixed(2));
    //console.log("here4");
    recalcSummary_AddOn();
}

function recalcSummary_AddOn() {
    //console.log("recalcSummary called");
    // Reset
    Object.keys(headerTotals).forEach(h => headerTotals[h] = 0);

    //console.log("1 headerTotals:", headerTotals);
    $("[data-header]").each(function () {
        let header = $(this).data("header");
        let total = parseFloat($(this).find(".total-input").val()) || 0;
        headerTotals[header] += total;
    });

    Object.keys(headerTotals).forEach(header => {
        $(`[data-header-summary="${header}"] .header-total`).text(headerTotals[header].toFixed(2));
    });

    //console.log("2 headerTotals:", headerTotals);
    let finalTotal = Object.values(headerTotals).reduce((a, b) => a + b, 0);
    //console.log("finalTotal:", finalTotal);
    $("#finalTotal").text(finalTotal.toFixed(2));

    if (currentAddonButton) {
        const $row = $(currentAddonButton).closest(".food-row, .food-block");
        $row.find("#foodPackage_Addons, .addonsCost").val(finalTotal.toFixed(2));

        // Recalculate that specific row only
        //    if ($row.hasClass("food-block")) {
        //        calculateFoodRowTotals($row);
        //    }
        //    else {
        //        updatePackageCost();
        //    }
    }
    updatePackageCost();
}

// Open modal function
let currentAddonButton = null;
let currentAddonRow = null;

function openAddonModal(booking_foodPackageId = 0, button = null) {
    //console.log("booking_foodPackageId:", booking_foodPackageId);
    currentAddonButton = button; // 🔹 store reference to clicked button
    //currentAddonRow = $(button).closest(".food-row, .food-block");

    currentAddonRow = $(button).closest(".food-block");   // for dynamic rows
    if (currentAddonRow.length === 0) {
        // default first row (no .food-block wrapper)
        currentAddonRow = $(button).closest(".food-row");
    }

    let savedItems = $(button).data("selected-AddOnItems") || [];

    if (addon_Data.length === 0) {
        loadAddonCharges();
    } else {
        populateAddonModal(savedItems);

        // Reset button text if no items saved
        if (savedItems.length === 0) {
            $(button).html('<i class="fa fa-check"></i> Addons');
        }
    }

    $("#addonChargeModal").modal("show");
    //$("#bookingModal").modal("hide");
}

//SAVE AddOns Modal
$(document).on("click", ".btn-saveAddOn", function () {
    let selectedItems = [];
    let grandTotal = 0;

    // Loop through all checked rows
    $("#addonItemsContainer .addon-checkbox:checked").each(function () {
        let row = $(this).closest("[data-header]");
        let header = parseInt(row.data("headerid"));
        let itemId = parseInt(row.data("id"));
        let rate = parseFloat(row.find(".rate-input").val() || 0);
        let pax = parseFloat(row.find(".pax-input").val() || 0);
        let total = parseFloat(row.find(".total-input").val() || 0);

        grandTotal += total;

        selectedItems.push({
            AddOn_Header_Id: header,
            AddOn_Item_Id: itemId,
            AddOn_Rate: rate,
            AddOn_Pax: pax,
            AddOn_Total: total
        });
    });

    // No selection warning
    if (selectedItems.length === 0) {
        Swal.fire({
            icon: "warning",
            title: "No Addons Selected",
            text: "Please select at least one addon item."
        });
        return;
    }

    //console.log("✅ Selected Addon Items:", selectedItems);
    //console.log("💰 Grand Total:", grandTotal.toFixed(2));

    // 🔹 Update the clicked AddOn button’s total span
    if (currentAddonButton) {
        //console.log("here addon btn 1");
        let $btn = $(currentAddonButton);
        let $totalSpan = $btn.closest(".d-flex").find(".foodPackage_TotalValue");

        // Update total text
        $totalSpan.text(grandTotal.toFixed(2));

        // Store selected items for later retrieval
        $btn.data("selected-AddOnItems", selectedItems);
        $btn.data("total", grandTotal);

        // 🔹 Update button label with count
        let itemCount = selectedItems.length;
        $btn.html(`<i class="fa fa-check"></i> Addons (${itemCount})`);

        //Update Amount
        let row = $(currentAddonButton).closest(".food-block");
        // fallback for default row (no .food-block wrapper)
        if (row.length === 0) {
            row = $(currentAddonButton).closest(".food-row");
        }
        //console.log("grandTotal 2:", grandTotal);

        updatePackageCost();
        //calculateFoodRowTotals(row);
        //recalculatePaymentSummary();

        //console.log("grandTotal 3:", grandTotal);
    }
    // Close the modal
    $("#addonChargeModal").modal("hide");
    //$("#bookingModal").modal("show");

});

// Function to update the Addon button with the selected items and total
function updateAddonButton(selectedItems) {
    // Find the button that was clicked (assuming we pass it as a context)
    let addonButton = $(".btn-outline-success").last();  // Adjust as needed to target the correct button
    let total = selectedItems.reduce((sum, item) => sum + item.total, 0);

    // Update the button's total value
    addonButton.next().find(".foodPackage_TotalValue").text(total.toFixed(2));

    // Store the selected items for later use
    addonButton.data("selected-AddOnItems", selectedItems);
}
//Food Addons ----- /END ------------------------------------------------------------------

//< !--Booking Note - text Editor-- - START---->
let editorsInitialized = false;
document.getElementById('OtherCharges_ToggleEditorsBtn').addEventListener('click', function () {
    const container = document.getElementById('OtherCharges_editorsContainer');

    if (container.style.display === 'none') {
        container.style.display = 'block';

        // Initialize editors only once after showing
        if (!editorsInitialized) {
            // Customer editor
            // window.otherCharges_customerEditor = new Quill('#otherCharges_customerEditor', {
            //     theme: 'snow',
            //     modules: { toolbar: toolbarOptions }
            // });

            // // Internal editor
            // window.otherCharges_internalEditor = new Quill('#otherCharges_internalEditor', {
            //     theme: 'snow',
            //     modules: { toolbar: toolbarOptions }
            // });

            editorsInitialized = true;
        }

    }
    else {
        container.style.display = 'none';
    }
});

// 1️⃣ Import parchment
const Parchment = Quill.import('parchment');

// 2️⃣ Define a custom style attributor for font size (instead of class-based)
const SizeStyle = new Parchment.Attributor.Style('size', 'font-size', {
    scope: Parchment.Scope.INLINE,
    whitelist: ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px']
});
Quill.register(SizeStyle, true);

// 3️⃣ Optional: Define custom line height too (if you still want)
const LineHeightStyle = new Parchment.Attributor.Style('lineheight', 'line-height', {
    scope: Parchment.Scope.BLOCK,
    whitelist: ['1.0', '1.2', '1.4', '1.5', '1.6', '2.0', '3.0']
});
Quill.register(LineHeightStyle, true);

// 4️⃣ Toolbar options
var toolbarOptions = [
    ['bold', 'italic', 'underline', 'strike'],
    [{ 'size': ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px'] }],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'lineheight': ['1.0', '1.2', '1.4', '1.5', '1.6', '2.0', '3.0'] }]
];

// 5️⃣ Initialize editors
var otherCharges_customerEditor = new Quill('#OtherCharges_customerEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
});

var otherCharges_internalEditor = new Quill('#OtherCharges_internalEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
});
//< !--Booking Note - text Editor-- - END---->

$("#otherCharges_Reset").on("click", function () {
    // Reset all number inputs inside Other Charges section
    $("#otherCharges").find("input[type='number']").val(0);
    recalculatePaymentSummary();
});


// Using jQuery
$("#gstSwitch").on("change", function () {
    const isChecked = $(this).is(":checked");
    //console.log(isChecked ? "GST Accepted" : "GST Not Accepted");
    updatePackageCost();
    // Recalculate dynamic rows
    $(".food-block").each(function () {
        calculateFoodRowTotals($(this));
    });
    //resetOtherCharges(isChecked);

    updateHallRentGST();

    // Recalculate all Other Charges GST
    recalculateAllOtherCharges();

    recalculatePaymentSummary();

    if (isChecked) {
        $("#gstSwitchText").text("On").removeClass("text-danger").addClass("text-success");  // Green for ON
    } else {
        $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
    }
});

// When Hall Rent value is typed or changed
$("#booking_hallRent").on("input", function () {
    updateHallRentGST();
});

//discount change
$("#paymentSummary_BookingAmount_Type").on("change", function () {
    $("#paymentSummary_BookingAmount_Amount").val(0);
    //updateBookingDiscount();
    recalculatePaymentSummary();
});

$("#paymentSummary_BookingAmount_Amount").on("input", function () {
    //updateBookingDiscount();
    recalculatePaymentSummary();

});

$(document).on("change", "#PaymentDetails_Mode", function () {
    let selectedText = $("#PaymentDetails_Mode option:selected").text().toLowerCase();

    // Hide everything first
    $("#PaymentDetails_TransactionId").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeBank").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeNo").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeDate").closest(".col-md-6").addClass("d-none");

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
        $("#PaymentDetails_ChequeBank").closest(".col-md-6").removeClass("d-none");
        $("#PaymentDetails_ChequeNo").closest(".col-md-6").removeClass("d-none");
        $("#PaymentDetails_ChequeDate").closest(".col-md-6").removeClass("d-none");
    }

    // UPI OR CREDIT
    else if (selectedText.includes("upi") || selectedText.includes("credit")) {
        $("#PaymentDetails_TransactionId").closest(".col-md-6").removeClass("d-none");
    }
});

loadPaymentModeDropdown();
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
            const ddl = $("#PaymentDetails_Mode");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data
                    .filter(x => x.IsActive)
                    .forEach(pm => {
                        ddl.append(`<option value="${pm.Id}">${pm.PaymentModeName}</option>
                        `);
                    });
            } else {
                Swal.fire("Error", res?.message || "Failed to load payment modes!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment Mode Dropdown Error");
        }
    });
}

loadAccountDropdown();
function loadAccountDropdown() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);
    var payload = { Id: 0, ActionBy: actionBy };
    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("GetAccount res:", res);

            const ddl = $("#PaymentDetails_AccountId");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data.forEach(acc => {

                    const accountName = (acc.AccountName || "").trim();
                    const holderName = (acc.AccountHolderName || "").trim();

                    let holderDisplay = "";

                    if (holderName && accountName.toLowerCase() !== holderName.toLowerCase()) {
                        holderDisplay = ` (${holderName})`;
                    }

                    ddl.append(`
                        <option value="${acc.Id}" data-isdefault="${acc.IsDefaultAccount}">
                            ${accountName}${holderDisplay}
                        </option>
                    `);
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
// GST Formula (18%)
function getGST(amount) {
    return amount * 0.18;
}

// Recalculate GST when Hall Rent changes or GST switch toggles
function updateHallRentGST() {

    let hallRent = parseFloat($("#booking_hallRent").val()) || 0;
    let isGSTEnabled = $("#gstSwitch").is(":checked");

    let gst = isGSTEnabled ? getGST(hallRent) : 0;

    $("#booking_gst").val(gst.toFixed(2));

    recalculatePaymentSummary();   // 🔥 IMPORTANT
}


//BOOKING CALENDAR DESIGN - END ----------------------------------------------------------------------
