var dataTable;

$(document).ready(function () {
    loadDataTable("GetInquiryList");
});

function loadDataTable(url) {
    $("#tblData").DataTable({
        "ajax": {
            "url": "/inquiry/" + url
        },
        "columns": [
            { "data": "id", width: "10%" },
            { "data": "fullName", width: "15%" },
            { "data": "email", width: "15%" },
            { "data": "phoneNumber", width: "15%" },
            {
                "data": "id",
                "render": function (id) {
                    return `
                        <div class="text-center">
                            <a href="/Inquiry/Details/${id}" class="btn btn-success text-white" style="cusrsor: pointer;">
                                <i class="fas fa-edit"></i>
                            </a>
                        </div>
                    `;
                },
                "width": "5%"
            }
        ]
    });
}