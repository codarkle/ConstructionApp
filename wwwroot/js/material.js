window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        $.ajax({
            url: '/Materials/Create',
            type: 'GET',
            success: function (html) {
                $("#createContent").html(html);
                $("#createModal").modal("show");
                $.validator.unobtrusive.parse('#createForm');
            }
        });
    });
    var table = $('#mainTable').DataTable({
        ajax: {
            url: "/Materials/GetMaterialList",
            type: "POST"
        }, 
        columns: [
            { data: 'no', name: 'No', orderable: false, searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { data: 'name', name: 'Name' },
            { data: 'description', name: 'Description' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                        <button class="btn btn-sm btn-warning edit-btn" data-id="${row.id}">
                            <i class="bi bi-pencil"></i>
                        </button>`;

                    if (window.myRole == "Admin") {
                        buttons += `
                        <button class="btn btn-sm btn-danger delete-btn" data-id="${row.id}">
                            <i class="bi bi-trash"></i>
                        </button>`;
                    }
                    return buttons;
                }
            }
        ],
        initComplete: function () {
            addColumnToggleDropdown(this.api());
        }
    });
});