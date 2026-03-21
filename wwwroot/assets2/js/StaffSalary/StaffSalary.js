function loadStaffDropdown(selectedId = null) {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            let ddl = $("#ddlStaff");
            ddl.empty().append(`<option value="">-- Select Staff --</option>`);

            response.data.forEach(staff => {
                ddl.append(`<option value="${staff.Id}">${staff.Name}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

function loadStatusDropdown(selectedId = null) {
    $.ajax({
        url: "/api/StatusMaster/GetStatusMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, StatusType: "Salary" }),
        success: function (response) {
            let ddl = $("#ddlStatus");
            ddl.empty().append(`<option value="">-- Select Status --</option>`);

            response.data.forEach(status => {
                ddl.append(`<option value="${status.Id}">${status.StatusName}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

function resetStaffSalaryFields() {
    $("#salaryId").val(0);
    $("#ddlStaff").val("");
    $("#txtSalaryMonth").val("");
    $("#txtBasicSalary").val("");
    $("#txtDeductions").val("");
    $("#ddlStatus").val("");
    $("#txtPaidDate").val(new Date().toISOString().split('T')[0]);
}

$(document).ready(function () {
    resetStaffSalaryFields();
    loadStaffDropdown();
    loadStatusDropdown();
    fetchStaffSalary();

    $("#btn-add").click(function () {
        $("#staffSalaryModalTitle").text("Add Staff Salary");

        $("#txtPaidDate").val(new Date().toISOString().split('T')[0]); // Default today

        $("#staffSalaryModal").modal("show");
    });

    $("#btnSaveStaffSalary").click(function () {

        if ($("#ddlStaff").val() === "") {
            Swal.fire("Validation", "Please select Staff", "warning");
            return;
        }

        if ($("#txtSalaryMonth").val() === "") {
            Swal.fire("Validation", "Please enter Salary Month", "warning");
            return;
        }

        if ($("#txtBasicSalary").val() === "" || parseFloat($("#txtBasicSalary").val()) <= 0) {
            Swal.fire("Validation", "Basic Salary must be greater than 0", "warning");
            return;
        }

        let payload = {
            Id: $("#salaryId").val(),
            StaffId: $("#ddlStaff").val(),
            SalaryMonth: $("#txtSalaryMonth").val(),
            BasicSalary: parseFloat($("#txtBasicSalary").val()),
            Deductions: $("#txtDeductions").val() ? parseFloat($("#txtDeductions").val()) : 0,
            StatusId: $("#ddlStatus").val() || null,
            PaidDate: $("#txtPaidDate").val() || null,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/StaffSalary/SaveStaffSalary",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Staff Salary saved successfully.");
                    resetStaffSalaryFields();
                    $("#staffSalaryModal").modal("hide");

                    fetchStaffSalary(); // reload table
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save StaffSalary  Error")
            }
        });
    });

    $(document).on("click", ".btn-edit-salary", function () {

        let table = $("#tableStaffSalary table").DataTable();
        let rowData = table.row($(this).closest("tr")).data();
        resetStaffSalaryFields();

        $("#salaryId").val(rowData.Id);
        loadStaffDropdown(rowData.StaffId);
        loadStatusDropdown(rowData.StatusId);
        $("#txtSalaryMonth").val(rowData.SalaryMonth);
        $("#txtBasicSalary").val(rowData.BasicSalary);
        $("#txtDeductions").val(rowData.Deductions);
        if (rowData.PaidDate) {
            const date = new Date(rowData.PaidDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#txtPaidDate').val(`${yyyy}-${mm}-${dd}`);
        }
        $("#staffSalaryModalTitle").text("Edit Staff Salary");
        $("#staffSalaryModal").modal("show");
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete", function () {
        // Get DataTable instance
        var table = $("#tableStaffSalary table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();

        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }

        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).modal("show");
    });

    // Confirm delete
    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");

        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        $.ajax({
            url: "/api/StaffSalary/DeleteStaffSalary",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire("Deleted!", response.message, "success");
                    $("#deleteRecordModal").modal("hide");
                    // Reload table
                    fetchStaffSalary();
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Something went wrong!");
            }
        });
    });

});

// Fetch Staff Salary
function fetchStaffSalary(staffId = 0) {
    $('#btn-loader').show(); // optional loader

    const payload = {
        Id: 0 // 0 = all salaries
        // StaffId: staffId
    };

    $.ajax({
        url: "/api/StaffSalary/GetStaffSalary",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            var data = response.data || [];
            renderStaffSalaryTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Something went wrong fetching Staff Salary");
        },
        complete: function () {
            $('#btn-loader').hide();
        }
    });
}

// Render DataTable
function renderStaffSalaryTable(data) {
    if ($.fn.dataTable.isDataTable("#tableStaffSalary table")) {
        $("#tableStaffSalary table").DataTable().destroy();
    }

    // Add Sr No
    data.forEach((x, i) => x.srno = i + 1);

    $("#tableStaffSalary table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No."},
            { data: "StaffName", title: "Staff" },
            {
                data: "SalaryMonth",
                title: "Salary Month",
                render: function (data) {
                    if (!data) return "";
                    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
                    let [year, m] = data.split("-");
                    return `${year}-${months[parseInt(m, 10) - 1]}`;
                }
            },
            { data: "BasicSalary", title: "Basic Salary", className: "text-end" },
            { data: "Deductions", title: "Deductions", className: "text-end" },
            { data: "NetSalary", title: "Net Salary", className: "text-end" },
            { data: "StatusName", title: "Status" },
            {
                data: "PaidDate",
                title: "Paid Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            {
                data: null,
                title: "Actions",
                render: (row) => `
                    <button class="btn btn-outline-warning btn-action-xss btn-edit-salary"><i class="fa fa-edit"></i></button>
                    <button class="btn btn-outline-danger btn-action-xss btn-delete"><i class="fa fa-trash"></i></button>
                `
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });
}
