
window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        const workSiteId = $('#workSiteDropDown').val();
        if (workSiteId == 0) {
            alert('Prego seleziona il cantiere!');
        }
        else {
            $.ajax({
                url: '/Presences/Create',
                type: 'GET',
                data: { workSiteId: workSiteId },
                success: function (html) {
                    $("#createContent").html(html);
                    $("#createModal").modal("show");
                    $.validator.unobtrusive.parse('#createForm');
                }
            });
        }
    });
    var table = $('#mainTable').DataTable({
        ajax: {
            url: '/Presences/GetPresenceList',
            type: 'POST',
            data: function (d) {
                d.workSiteDropDown = $('#workSiteDropDown').val();
                return d;
            }
        },
        columns: [
            { data: 'no', name: 'No', orderable: false, searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            {
                data: 'date',
                name: 'Date'
            },
            { data: 'workSiteName', name: 'WorkSite.Name' },
            { data: 'employeeName', name: 'Employee.UserName' },
            { data: 'hs', name: 'HS' },
            { data: 'hr', name: 'HR' },
            {
                data: 'cost',
                name: 'Cost',
            },
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
    // Enable/disable employee input fields based on presence checkbox
    $(document).on('change', '.presence-checkbox', function () {
        const row = $(this).closest('tr');
        const isChecked = $(this).is(':checked');
        row.find('input.presence-input').prop('disabled', !isChecked);
    });

    // Optional: Initialize disabled inputs by default
    $('.presence-checkbox').each(function () {
        const row = $(this).closest('tr');
        const isChecked = $(this).is(':checked');
        row.find('input.presence-input').prop('disabled', !isChecked);
    });
}); 