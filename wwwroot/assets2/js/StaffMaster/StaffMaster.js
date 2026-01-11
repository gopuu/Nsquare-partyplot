function loadDesignationDropdown() {
    $.ajax({
        url: "/api/DesignationMaster/GetDesignationMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            const dropdown = $("#ddlDesignation");
            dropdown.empty().append('<option value="">-- Select --</option>');

            if (res?.status === "Success") {
                res.data.forEach(item => {
                    dropdown.append(`<option value="${item.Id}">${item.DesignationName}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

$(document).ready(function () {

    function getBase64WithName(file, callback) {
        let reader = new FileReader();
        reader.onload = function (e) {
            let base64 = e.target.result.split(',')[1];

            // Combine filename + base64 value
            let finalValue = base64;

            callback(finalValue);
        };
        reader.readAsDataURL(file);
    }

    fetchStaffData();
    loadDesignationDropdown();

    $(document).on("click", "#btn-add", function () {
        resetStaffForm();
        $("#txtJoiningDate").val(new Date().toISOString().split("T")[0]);
        $("#txtBirthDate").val(new Date().toISOString().split("T")[0]);
        $("#txtAnniversaryDate").val(new Date().toISOString().split("T")[0]);
        $("#staffModalTitle").text("Add Staff");
        $("#staffModal").modal("show");
    });

    $(document).on("click", "#btnSaveStaff", function () {
        // --- Validation ---
        const name = $("#txtName").val().trim();
        const phone = $("#txtPhoneNo").val().trim();
        if (!name) {
            Swal.fire("Validation", "Name is required", "warning");
            return;
        }
        if (!phone) {
            Swal.fire("Validation", "Phone number is required", "warning");
            return;
        }

        // SELECT FILE
        let fileInput = $("#txtPhoto").get(0);
        let file = fileInput.files.length > 0 ? fileInput.files[0] : null;
        function getBase64WithoutMeta(file, callback) {
            let reader = new FileReader();
            reader.onload = function (e) {
                let base64 = e.target.result.split(',')[1]; // REMOVE metadata
                callback(base64);
            };
            reader.readAsDataURL(file);
        }
        function saveStaff(base64File) {
            $("#btnSaveStaff").prop("disabled", true);

            const payload = {
                Id: Number($("#staffId").val() || 0),
                Name: $("#txtName").val().trim(),
                Emp_Code: $("#txtEmpCode").val().trim(),
                Address: $("#txtAddress").val().trim(),
                PhoneNo: $("#txtPhoneNo").val().trim(),
                Emergency_PhoneNo: $("#txtEmergencyPhone").val().trim(),
                Email: $("#txtEmail").val().trim(),
                DesignationId: Number($("#ddlDesignation").val() || 0),
                AadharCardNo: $("#txtAadhar").val().trim(),
                JoiningDate: $("#txtJoiningDate").val(),
                BirthDate: $("#txtBirthDate").val(),
                AnniversaryDate: $("#txtAnniversaryDate").val(),
                IsActive: $("#switchIsActive").is(":checked"),
                Salary: Number($("#txtSalary").val() || 0),
                Photo: base64File,
                User_Name: $("#txtUserName").val().trim(),
                Password: $("#txtPassword").val().trim(),
                IsSuperUser: $("#switchIsSuperUser").is(":checked"),
                IsAutoLogin: $("#switchIsAutoLogin").is(":checked"),
                Bank_Name: $("#txtBankName").val().trim(),
                Bank_BranchName: $("#txtBankBranch").val().trim(),
                Bank_IFSC: $("#txtIFSC").val().trim(),
                Bank_AccountNo: $("#txtAccountNo").val().trim(),
                Bank_AccountHolderName: $("#txtBankAccountHolderName").val().trim(),
                Bank_UPI_Id: $("#txtBankUPIId").val().trim(),
                ActionBy: sessionStorage.getItem("userId") ?? 0
            };
            // ---- API Call ----
            $.ajax({
                url: "/api/StaffMaster/SaveStaffMaster",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    if (res?.status === "Success") {
                        Swal.fire({
                            icon: "success",
                            title: "Saved!",
                            timer: 1200,
                            showConfirmButton: false,
                            position: "top-end"
                        });

                        $("#staffModal").modal("hide");
                        fetchStaffData(); // reload list
                    } else {
                        Swal.fire("Error", res?.message || "Save failed", "error");
                    }
                },
                error: function (xhr, status, error) {
                    console.log("Error:", xhr);
                    Swal.fire("API Error", "Something went wrong.", "error");
                },
                complete: function () {
                    $("#btnSaveStaff").prop("disabled", false);
                }
            });
        }
        // If file exists → convert → save
        if (file) {
            getBase64WithoutMeta(file, function (base64File) {
                saveStaff(base64File);
            });
        }
        else {
            saveStaff(null);
        }
    });

    $(document).on("click", ".btn-edit", function () {
        // Permission Check
        const canEdit = (sessionStorage.getItem("CanEdit") || localStorage.getItem("CanEdit")) === "true";
        if (!canEdit) {
            Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
            return;
        }

        // Get row data
        const table = $("#tableStaff table").DataTable();
        const rowData = table.row($(this).closest("tr")).data();
        resetStaffForm(); // Optional call if you have reset logic
        console.log("rowData canEdit :", rowData);

        // Fill Modal Fields
        $("#staffId").val(rowData.Id);
        $("#txtName").val(rowData.Name);
        $("#txtEmpCode").val(rowData.Emp_Code);
        $("#txtAddress").val(rowData.Address);
        $("#txtPhoneNo").val(rowData.PhoneNo);
        $("#txtEmergencyPhone").val(rowData.Emergency_PhoneNo);
        $("#txtEmail").val(rowData.Email);
        $("#ddlDesignation").val(rowData.DesignationId).trigger('change');
        $("#txtAadhar").val(rowData.AadharCardNo);
        // Format date for <input type="date">
        if (rowData.JoiningDate) {
            const date = new Date(rowData.JoiningDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#txtJoiningDate').val(`${yyyy}-${mm}-${dd}`);
        }

        if (rowData.BirthDate) {
            const date = new Date(rowData.BirthDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#txtBirthDate').val(`${yyyy}-${mm}-${dd}`);
        }
        if (rowData.AnniversaryDate) {
            const date = new Date(rowData.AnniversaryDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#txtAnniversaryDate').val(`${yyyy}-${mm}-${dd}`);
        }
        $("#txtSalary").val(rowData.Salary);

        // Photo not prefilled in input type="file" for security reasons
        // SET INVOICE URL
        $("#invoiceFileLink").val(rowData.Photo || "");
        // If URL present → show download button
        if (rowData.Photo && rowData.Photo !== "") {
            $("#btnDownloadInvoice").removeClass("d-none");
        }
        else {
            $("#btnDownloadInvoice").addClass("d-none");
        }

        $("#txtUserName").val(rowData.User_Name);
        $("#txtPassword").val(rowData.Password);
        $("#txtBankName").val(rowData.Bank_Name);
        $("#txtBankBranch").val(rowData.Bank_BranchName);
        $("#txtIFSC").val(rowData.Bank_IFSC);
        $("#txtAccountNo").val(rowData.Bank_AccountNo);
        $("#txtBankAccountHolderName").val(rowData.Bank_AccountHolderName);
        $("#txtBankUPIId").val(rowData.Bank_UPI_Id);

        $("#switchIsSuperUser").prop("checked", rowData.IsSuperUser);
        $("#switchIsAutoLogin").prop("checked", rowData.IsAutoLogin);
        $("#switchIsActive").prop("checked", rowData.IsActive);

        // Change modal title
        $("#staffModalTitle").text("Edit Staff");
        // $("#btnSaveStaff").text("Update");

        // Show modal
        $("#staffModal").modal("show");
    });

    // Delete button click
    $(document).on('click', '.btn-delete', function () {
        const canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }
        let table = $('#tableStaff table').DataTable();
        let rowData = table.row($(this).closest('tr')).data();
        $("#deleteRecordModal")
            .data("id", rowData.Id)
            .modal("show");
    });

    // Confirm Delete
    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");
        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        $('#btn-loader').show();
        $.ajax({
            url: "/api/StaffMaster/DeleteStaffMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (typeof response === "string") {
                    response = JSON.parse(response);
                }
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Staff Deleted!",
                        showConfirmButton: false,
                        timer: 1000
                    });

                } else {
                    Swal.fire("Delete Failed", response.message, "error");
                }
                $("#deleteRecordModal").modal("hide");
                fetchStaffData(); // reload updated list
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Staff Error");
            },
            complete: function () {
                $('#btn-loader').hide();
            }
        });
    });

    $("#btnDownloadInvoice").on("click", function () {
        let fileUrl = $("#invoiceFileLink").val();

        if (!fileUrl || fileUrl.trim() === "") {
            Swal.fire("Error", "File not found!", "error");
            return;
        }

        const link = document.createElement("a");
        link.style.display = "none";
        link.href = fileUrl;
        link.download = fileUrl.split("/").pop(); // file name

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    });

});

function resetStaffForm() {
    $("#staffId").val(0);
    $("#staffModalTitle").text("Add Staff");
    $("#staffModal input, #staffModal textarea").val("");
    $("#switchIsSuperUser, #switchIsAutoLogin,#switchIsActive").prop("checked", false);
}

function fetchStaffData() {
    $.ajax({
        url: '/api/StaffMaster/GetStaffMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }),

        success: function (res) {
            const data = (res && res.data) ? res.data : [];
            renderStaffTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Staff Error");
        }
    });
}

function renderStaffTable(data) {

    if ($.fn.dataTable.isDataTable('#tableStaff table')) {
        $('#tableStaff table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {
        $('#tableStaff table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: '#' },
                { data: 'Name', title: 'Name' },
                { data: 'PhoneNo', title: 'Phone' },
                { data: 'Email', title: 'Email' },
                { data: 'DesignationId', title: 'Designation' },
                {
                    data: null,
                    title: 'Actions',
                    render: () => getActionsHtml()
                }
            ],
            pageLength: 10
        });
        return;
    }

    // Add auto numbering
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableStaff table').DataTable({
        data: data,
        columns: [
            { data: 'srno', title: '#' },
            { data: 'Name', title: 'Name' },
            { data: 'PhoneNo', title: 'Phone' },
            {
                data: "JoiningDate",
                title: "Joining Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            { data: 'Salary', title: 'Salary' },
            { data: 'User_Name', title: 'Username' },
            {
                data: 'Photo',
                title: 'Photo',
                render: function (d) {
                    return d
                        ? `<img src="${d}" class="rounded" width="40" height="40" />`
                        : `<span class="text-muted">No Image</span>`;
                }
            },
            {
                data: 'IsSuperUser',
                title: 'Super User',
                render: d => d ? `<span class="badge bg-success">Yes</span>` : `<span class="badge bg-secondary">No</span>`
            },
            {
                data: 'IsAutoLogin',
                title: 'Auto Login',
                render: d => d ? `<span class="badge bg-info">Yes</span>` : `<span class="badge bg-secondary">No</span>`
            },
            {
                data: 'IsActive',
                title: 'IsActive',
                render: d => d ? `<span class="badge bg-info">Yes</span>` : `<span class="badge bg-secondary">No</span>`
            },
            {
                data: null,
                title: 'Actions',
                render: () => getActionsHtml()
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false
    });
}

function getActionsHtml() {
    return `
    <div class="d-flex gap-2">
            <button class="btn btn-outline-warning btn-sm btn-action-xss  btn-edit"><i class="fa fa-edit"></i></button>
            <button class="btn btn-outline-danger btn-sm btn-action-xss  btn-delete"><i class="fa fa-trash"></i></button>
    </div>
    `;
}
