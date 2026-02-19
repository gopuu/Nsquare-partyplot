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
    const fromAmt = $("#fromAmount").val();
    const toAmt = $("#toAmount").val();

    const inputData = {
        ProcessId: Number(processId),
        Duration: duration,
        FromAmount: fromAmt ? Number(fromAmt) : 0,
        ToAmount: toAmt ? Number(toAmt) : 0,
        FromDate: duration === "customrange" ? $("#fromDate").val() : null,
        ToDate: duration === "customrange" ? $("#toDate").val() : null
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
                console.log("deal:", deal);
                const hideFollowUp = (deal.Is_LeadBooked || deal.Is_LeadLost);
                dealsHtml += `
                    <div class="card shadow-sm mb-2 deal-card cursor-pointer"
                        draggable = "true"
                        data-dealid="${deal.EnquiryDealId}"
                        data-processid="${process.ProcessId}">
                            <div class="card-body p-2 deal-open">
                                <h6 class="mb-1">${deal.EnquiryDealTitle}</h6>
                                <div class="d-flex justify-content-between mb-1 small">
                                    <span class="text-muted">${deal.EnquiryDealName}</span>
                                    <span class="text-muted">${deal.EventDate_Format1}</span>
                                </div>
                                <div class="d-flex justify-content-between mb-1 small">
                                    <span class="text-muted d-none">${deal.EventTypeName}</span>
                                    <span class="text-muted">${deal.EventSubTypeName}</span>
                                </div>
                                <div class="d-flex justify-content-between mb-1 small">
                                    <span class="text-muted">${deal.EventStartTime} - ${deal.EventEndTime}</span>
                                </div>
                                <p class="mb-1 fw-bold">₹${deal.Amount.toLocaleString()}</p>
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
                    data-processid="${process.ProcessId}">
                    <div class="card-header bg-light">
                        <h6 class="mb-0">${process.ProcessName}</h6>
                        <small class="text-muted">₹${process.TotalAmount.toLocaleString()} • ${process.TotalEnquiryDeals} Deal(s)</small>
                    </div>
                    <div class="card-body deal-dropzone" style="min-height: 500px;">
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
        console.log("Clicked dealId:", dealId);
        // later you can call your API here
        // 🔥 Reset before fetching data
        resetDealForm();
        ////showLoader()(); // optional - show loader while fetching

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
                    $("#dealProcess").val(data.ProcessId);
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
            },
            complete: function () {
                //hideLoader();
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

// Drag & Drop setup
let pendingMoveData = null;
function enableDragDrop() {
    const cards = document.querySelectorAll(".deal-card");
    const dropzones = document.querySelectorAll(".deal-dropzone");

    cards.forEach(card => {
        card.addEventListener("dragstart", e => {
            e.dataTransfer.setData("dealId", card.dataset.dealid);
            e.dataTransfer.setData("fromProcessId", card.dataset.processid);
        });
    });

    dropzones.forEach(zone => {
        zone.addEventListener("dragover", e => e.preventDefault());

        zone.addEventListener("drop", e => {
            e.preventDefault();

            const dealId = parseInt(e.dataTransfer.getData("dealId"));
            const fromProcessId = parseInt(e.dataTransfer.getData("fromProcessId"));
            const toProcessId = parseInt(
                zone.closest(".process-column").dataset.processid
            );

            if (fromProcessId !== toProcessId) {

                // 🧠 Store pending move ONLY
                pendingMoveData = {
                    EnquiryDealId: dealId,
                    ProcessId: toProcessId
                };

                // Open follow-up modal
                openFollowUp(dealId);
            }
        });
    });

}

$("#followupModal").on("hidden.bs.modal", function () {
    saveFollowUpAndMoveDeal();
    pendingMoveData = null;
});

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
    $("#dealProcess").val(1);
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
    $("#pax").val("");
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

    loadEventTypeList();
    loadBanquetHallList();
    loadLeadSourceList();

    loadFollowUpTypes();
    loadFollowUpStatus();
    loadMoodList();
    loadTimeDropdown();

    fetchHallDetails();

    // Trigger on input
    document.getElementById("pax").addEventListener("input", calculateEstimatedAmount);
    document.getElementById("packageAmount").addEventListener("input", calculateEstimatedAmount);

    $("#ddlDuration").on("change", function () {
        const duration = $(this).val();

        if (duration === "customrange") {
            $("#customDateRange").removeClass("d-none");
        } else {
            $("#customDateRange").addClass("d-none");
            $("#fromDate").val("");
            $("#toDate").val("");
        }
    });

    $("#btnApplyFilters").on("click", function () {
        loadEnquiryDeals();
        $("#filterDropdown").dropdown("hide");
    });

    $("#btnResetFilters").on("click", function () {
        $("#ddlDuration").val("thismonth");
        $("#fromAmount").val("");
        $("#toAmount").val("");
        $("#fromDate").val("");
        $("#toDate").val("");
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
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
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
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Error saving event sub-type."
                });
            },
            complete: function () {
                //hideLoader();
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
    ////showLoader()();
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
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching event type list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
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
    ////showLoader()();

    //let eventTypeId = $("#eventTypeDropdown").val(); // capture selected event type

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
                });

                $("#eventSubTypeTable tbody").html(rows);
                $("#eventSubTypeDropdown").html(options);
            } else {
                $("#eventSubTypeTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#eventSubTypeDropdown").html(`<option value = "0"> No Event Sub-Type Found</option> `);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching event sub-type list."
            });
        },
        complete: function () {
            //hideLoader();
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
    ////showLoader()();
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
                    options += `<option value = "${item.Id}"> ${item.HallName}</option> `;
                });
                $("#banquetHallDropdown").html(options);
            } else {
                $("#banquetHallDropdown").html(`<option value = "0"> No Banquet Hall Found</option> `);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching banquet hall list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
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
                $("#leadSourceDropdown").html(options);
            }
            else {
                $("#leadSourceTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#leadSourceDropdown").html(`<option value = "0"> No Lead Source Found</option> `);
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
/* --------------------- 🕒 NEXT FOLLOW-UP TIME --------------------- */
/*
const followUpInput = document.getElementById('nextFollowUpTime_modalFollowup');
const followUpPicker = document.getElementById('clockPickerFollowUp');
const hourDisplayFollowUp = document.getElementById('hourDisplayFollowUp');
const minuteDisplayFollowUp = document.getElementById('minuteDisplayFollowUp');
const hourHandFollowUp = document.getElementById('hourHandFollowUp');
const minuteHandFollowUp = document.getElementById('minuteHandFollowUp');
const amBtnFollowUp = document.getElementById('amBtnFollowUp');
const pmBtnFollowUp = document.getElementById('pmBtnFollowUp');
const setBtnFollowUp = document.getElementById('setTimeBtnFollowUp');

let hourFollowUp = 12;
let minuteFollowUp = 0;
let ampmFollowUp = 'AM';
let modeFollowUp = 'hour';

// Toggle picker visibility
followUpInput.addEventListener('click', (e) => {
    e.stopPropagation();
    initializeFollowUpPicker(followUpInput.value);
    followUpPicker.classList.toggle('d-none');
});

// Initialize picker when opening
function initializeFollowUpPicker(value) {
    let time = value.trim();
    if (!time) time = "12:00 AM"; // Default if empty

    const [timePart, modifier] = time.split(" ");
    let [h, m] = timePart.split(":").map(Number);

    ampmFollowUp = modifier || 'AM';
    hourFollowUp = h;
    minuteFollowUp = m;

    hourDisplayFollowUp.textContent = hourFollowUp.toString().padStart(2, '0');
    minuteDisplayFollowUp.textContent = minuteFollowUp.toString().padStart(2, '0');
    hourHandFollowUp.style.transform = `rotate(${hourFollowUp * 30}deg)`;
    minuteHandFollowUp.style.transform = `rotate(${minuteFollowUp * 6}deg)`;

    if (ampmFollowUp === 'AM') {
        amBtnFollowUp.classList.add('active');
        pmBtnFollowUp.classList.remove('active');
    } else {
        pmBtnFollowUp.classList.add('active');
        amBtnFollowUp.classList.remove('active');
    }

    setFollowUpMode('hour');
}

// Switch between hour/minute mode
//hourDisplayFollowUp.addEventListener('click', () => setFollowUpMode('hour'));
//minuteDisplayFollowUp.addEventListener('click', () => setFollowUpMode('minute'));

function setFollowUpMode(m) {
    modeFollowUp = m;

    if (modeFollowUp === 'hour') {
        hourDisplayFollowUp.classList.add('active-text');
        minuteDisplayFollowUp.classList.remove('active-text');
        minuteDisplayFollowUp.classList.add('inactive-text');
        hourDisplayFollowUp.classList.remove('inactive-text');
        hourHandFollowUp.classList.add('active-hand');
        minuteHandFollowUp.classList.remove('active-hand');
    } else {
        minuteDisplayFollowUp.classList.add('active-text');
        hourDisplayFollowUp.classList.remove('active-text');
        hourDisplayFollowUp.classList.add('inactive-text');
        minuteDisplayFollowUp.classList.remove('inactive-text');
        minuteHandFollowUp.classList.add('active-hand');
        hourHandFollowUp.classList.remove('active-hand');
    }
}
/*
// Handle number click
followUpPicker.querySelectorAll('.clock-number').forEach(num => {
    num.addEventListener('click', () => {
        const value = parseInt(num.textContent);
        if (modeFollowUp === 'hour') {
            hourFollowUp = value;
            hourDisplayFollowUp.textContent = value.toString().padStart(2, '0');
            hourHandFollowUp.style.transform = `rotate(${value * 30}deg)`;
        } else {
            minuteFollowUp = (value % 12) * 5;
            minuteDisplayFollowUp.textContent = minuteFollowUp.toString().padStart(2, '0');
            minuteHandFollowUp.style.transform = `rotate(${minuteFollowUp * 6}deg)`;
        }
    });
});
// AM/PM toggle
amBtnFollowUp.addEventListener('click', () => {
    ampmFollowUp = 'AM';
    amBtnFollowUp.classList.add('active');
    pmBtnFollowUp.classList.remove('active');
});

pmBtnFollowUp.addEventListener('click', () => {
    ampmFollowUp = 'PM';
    pmBtnFollowUp.classList.add('active');
    amBtnFollowUp.classList.remove('active');
});

// Set time function
function setTimeFollowUp() {
    followUpInput.value = `${hourDisplayFollowUp.textContent}:${minuteDisplayFollowUp.textContent} ${ampmFollowUp} `;
    followUpPicker.classList.add('d-none');
}

// Confirm button click
setBtnFollowUp.addEventListener('click', setTimeFollowUp);

// 🔹 Auto-save on outside click
document.addEventListener('click', (e) => {
    if (!followUpPicker.contains(e.target) && e.target !== followUpInput) {
        if (!followUpPicker.classList.contains('d-none')) {
            setTimeFollowUp();
        }
    }
});

// Default mode
setFollowUpMode('hour');
*/


//<!-- Clock Time Design -- Delas -> START TIME & END TIME -- END -->
