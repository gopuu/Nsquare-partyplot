let sopDocuments = [];
let sopCurrentPdfUrl = null;
let sopViewerModal = null;

let sopCurrentZoom = 100;
let sopViewMode = "Fit";

$(document).ready(function () {
    const userId = getLoggedInEmployeeId();

    if (!userId) {
        $("#sopLoader").addClass("d-none");
        $("#sopNoData").removeClass("d-none");
        $("#sopNoData h6").text("Unable to identify logged-in employee");
        $("#sopNoData small").text("Please login again.");
        return;
    }

    sopViewerModal = new bootstrap.Modal(document.getElementById("sopViewerModal"));

    loadSOPDocuments();

    $("#sopViewerModal").on("hidden.bs.modal", function () {
        closeSOPDocument();
    });

    $("#sopPdfViewer").on("load", function () {
        const src = $(this).attr("src");

        if (src) {
            $("#sopPdfLoader").addClass("d-none");
        }
    });
});

function getLoggedInEmployeeId() {
    return parseInt(sessionStorage.getItem("userId")) || 0;
}

function loadSOPDocuments() {
    const userId = getLoggedInEmployeeId();

    if (!userId) {
        showSOPEmptyState("Unable to identify logged-in employee.", "Please login again.");
        return;
    }

    $("#sopLoader").removeClass("d-none");
    $("#sopContent").addClass("d-none");
    $("#sopNoData").addClass("d-none");

    const payload = {
        ActionBy: userId
    };

    $.ajax({
        url: "/api/InfinityGrowth_SOP/GetInfinityGrowth_SOP_Documents",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            $("#sopLoader").addClass("d-none");

            if (res.status !== "Success") {
                showSOPEmptyState("Unable to load documents.", res.message || "Please try again.");
                return;
            }

            sopDocuments = res.data || [];

            if (!sopDocuments.length) {
                showSOPEmptyState("No documents available.", "You currently don't have access to any SOP documents.");
                return;
            }

            renderSOPDocuments(sopDocuments);
        },
        error: function (xhr, status, error) {
            $("#sopLoader").addClass("d-none");
            showSOPEmptyState("Unable to load documents.", "Please try again.");
            handleApiError(xhr, status, error, "Failed to Load SOP Documents");
        }
    });
}

function renderSOPDocuments(documents) {
    const $container = $("#sopContent");

    $container.empty();

    const groups = groupSOPDocuments(documents);

    Object.keys(groups).forEach(function (parentName, parentIndex) {

        const categories = groups[parentName];
        const parentId = `sopParent_${parentIndex}`;

        let directDocumentsHtml = "";
        let childCategoriesHtml = "";

        // ============================================
        // 1. Documents directly under Parent Category
        // ============================================
        if (categories[parentName]) {

            const parentDocs = categories[parentName];

            if (parentDocs.length > 0) {
                directDocumentsHtml = `
                    <div class="sop-category-block">
                        <div class="row g-2">
                            ${parentDocs.map(renderSOPDocumentCard).join("")}
                        </div>
                    </div>
                `;
            }
        }

        // ============================================
        // 2. Child Categories + their Documents
        // ============================================
        Object.keys(categories).forEach(function (categoryName) {

            // Already rendered above
            if (categoryName === parentName) {
                return;
            }

            const docs = categories[categoryName];

            childCategoriesHtml += `
                <div class="sop-category-block">

                    <div class="sop-subcategory-title">
                        <i class="fas fa-folder me-2 text-warning"></i>
                        ${escapeHtml(categoryName)}
                    </div>

                    <div class="row g-2">
                        ${docs.map(renderSOPDocumentCard).join("")}
                    </div>

                </div>
            `;
        });

        // ============================================
        // Parent Section
        // ============================================
        $container.append(`
            <div class="sop-section">

                <button type="button"
                        class="sop-section-header"
                        data-bs-toggle="collapse"
                        data-bs-target="#${parentId}"
                        aria-expanded="true">

                    <div class="d-flex align-items-center">

                        <div class="sop-folder-icon">
                            <i class="fas fa-folder-open"></i>
                        </div>

                        <div class="text-start">
                            <div class="sop-section-title">
                                ${escapeHtml(parentName)}
                            </div>

                            <small class="text-muted">
                                ${countSOPDocuments(categories)} document(s)
                            </small>
                        </div>

                    </div>

                    <i class="fas fa-chevron-down sop-chevron"></i>

                </button>

                <div id="${parentId}" class="collapse show">

                    <div class="sop-section-body">

                        <!-- Direct documents of parent -->
                        ${directDocumentsHtml}

                        <!-- Child categories -->
                        ${childCategoriesHtml}

                    </div>

                </div>

            </div>
        `);
    });

    $container.removeClass("d-none");
}

function groupSOPDocuments(documents) {
    const groups = {};

    documents.forEach(function (doc) {

        const parentName =
            doc.ParentCategoryName ||
            doc.CategoryName ||
            "Documents";

        const categoryName =
            doc.CategoryName ||
            "Documents";

        if (!groups[parentName]) {
            groups[parentName] = {};
        }

        if (!groups[parentName][categoryName]) {
            groups[parentName][categoryName] = [];
        }

        groups[parentName][categoryName].push(doc);
    });

    return groups;
}

function renderSOPDocumentCard(doc) {
    return `
        <div class="col-xl-4 col-lg-6 col-md-6">
            <button type="button"
                    class="sop-document-card"
                    onclick="viewSOPDocument(${doc.Id})">
                <div class="sop-document-icon">
                    <i class="fas fa-file-pdf"></i>
                </div>

                <div class="sop-document-info">
                    <div class="sop-document-title">
                        ${escapeHtml(doc.Title || doc.FileName || "Document")}
                    </div>

                    <div class="sop-document-meta">
                        <i class="fas fa-eye me-1"></i>
                        View document
                    </div>
                </div>

                <div class="sop-document-action">
                    <i class="fas fa-chevron-right"></i>
                </div>
            </button>
        </div>
    `;
}

function countSOPDocuments(categories) {
    return Object.values(categories).reduce(function (total, documents) {
        return total + documents.length;
    }, 0);
}

function viewSOPDocument(documentId) {
    const userId = getLoggedInEmployeeId();

    if (!userId || !documentId) {
        Swal.fire("Error", "Invalid document request.", "error");
        return;
    }

    const document = sopDocuments.find(x => Number(x.Id) === Number(documentId));

    $("#sopViewerTitle").text(document?.Title || document?.FileName || "Document");
    $("#sopPdfViewer").attr("src", "");
    $("#sopPdfLoader").removeClass("d-none");

    sopCurrentZoom = 100;
    sopViewMode = "Fit";
    $("#sopZoomLabel").text("100%");

    sopViewerModal.show();

    const payload = {
        DocumentId: documentId,
        ActionBy: userId
    };

    fetch("/api/InfinityGrowth_SOP/GetInfinityGrowth_SOP_DocumentsForView", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    })
    .then(async function (response) {
        if (!response.ok) {
            const message = await response.text();
            throw new Error(message || "Unable to open document.");
        }

        return response.blob();
    })
    .then(function (blob) {
        if (blob.type && blob.type !== "application/pdf")
            throw new Error("Invalid document response.");

        if (sopCurrentPdfUrl)
            URL.revokeObjectURL(sopCurrentPdfUrl);

        sopCurrentPdfUrl = URL.createObjectURL(blob);

        fitPageSOP();
    })
    .catch(function (error) {
        $("#sopPdfLoader").addClass("d-none");
        sopViewerModal.hide();

        Swal.fire(
            "Unable to Open Document",
            error.message || "You may not have access to this document.",
            "error"
        );
    });
}

function loadSOPPdf(fragment) {
    if (!sopCurrentPdfUrl) return;

    $("#sopPdfViewer").attr(
        "src",
        `${sopCurrentPdfUrl}#toolbar=0&navpanes=0&scrollbar=1&${fragment}`
    );
}

function zoomInSOP() {
    if (!sopCurrentPdfUrl || sopCurrentZoom >= 300) return;

    sopCurrentZoom += 25;
    sopViewMode = "zoom";

    $("#sopZoomLabel").text(`${sopCurrentZoom}%`);

    loadSOPPdf(`zoom=${sopCurrentZoom}`);
}

function zoomOutSOP() {
    if (!sopCurrentPdfUrl || sopCurrentZoom <= 50) return;

    sopCurrentZoom -= 25;
    sopViewMode = "zoom";

    $("#sopZoomLabel").text(`${sopCurrentZoom}%`);

    loadSOPPdf(`zoom=${sopCurrentZoom}`);
}

function fitWidthSOP() {
    if (!sopCurrentPdfUrl) return;

    sopCurrentZoom = 100;
    sopViewMode = "FitH";

    $("#sopZoomLabel").text("Fit Width");

    loadSOPPdf("view=FitH");
}

function fitPageSOP() {
    if (!sopCurrentPdfUrl) return;

    sopCurrentZoom = 100;
    sopViewMode = "Fit";

    $("#sopZoomLabel").text("Fit Page");

    loadSOPPdf("view=Fit");
}

function closeSOPDocument() {
    $("#sopPdfViewer").attr("src", "");
    $("#sopPdfLoader").removeClass("d-none");

    sopCurrentZoom = 100;
    sopViewMode = "Fit";
    $("#sopZoomLabel").text("100%");

    if (sopCurrentPdfUrl) {
        URL.revokeObjectURL(sopCurrentPdfUrl);
        sopCurrentPdfUrl = null;
    }
}

function showSOPEmptyState(title, message) {
    $("#sopLoader").addClass("d-none");
    $("#sopContent").addClass("d-none");

    $("#sopNoData h6").text(title);
    $("#sopNoData small").text(message);
    $("#sopNoData").removeClass("d-none");
}

function escapeHtml(value) {
    return $("<div>").text(value || "").html();
}

$(document).on("contextmenu", "#sopViewerModal", function (e) {
    e.preventDefault();
});

$(document).on("keydown", function (e) {
    if (!$("#sopViewerModal").hasClass("show")) {
        return;
    }

    if ((e.ctrlKey || e.metaKey) && ["s", "p"].includes(e.key.toLowerCase())) {
        e.preventDefault();
        return false;
    }
});