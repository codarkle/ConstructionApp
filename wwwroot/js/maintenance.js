window.addEventListener('DOMContentLoaded', function () { 
    $(document).on('click', '.add-btn', function () {
        $.ajax({
            url: '/Maintenances/Create',
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
            url: "/Maintenances/GetMaintenanceList",
            type: "POST"
        },
        columns: [
            { data: 'no', name: 'No', orderable: false, searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { data: 'vehicle.description', name: 'Vehicle.Description'},
            { data: 'vehicle.plate', name: 'Vehicle.Plate'},
            { data: 'dateOut', name: 'DateOut' },
            { data: 'kmOut', name:'KmOut' },
            { data: 'dateIn', name: 'DateIn' },
            { data: 'kmIn',  name: 'KmIn' },
            { data: 'driver', name: 'Driver' },
            { data: 'description', name: 'Description' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                        <button class="btn btn-sm btn-info view-btn" data-id="${row.id}">
                            <i class="bi bi-eye"></i>
                        </button>
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
        columnDefs: [
            { targets: 4, className: 'min-width-date' },
            { targets: 6, className: 'min-width-date' }
        ],
        initComplete: function () {
            addColumnToggleDropdown(this.api());
        }
    });

    //when i click the view button in main table, shows delete confirm modal
    $(document).on('click', '#mainTable tbody .view-btn', function () {
        const id = $(this).data('id');
        $.get(`/Vehicles/View/${id}`, function (html) {
            $('#viewContent').html(html);
            $('#viewModal').modal('show');
        });
    });

    $(document).on('change', '#VehicleId', function () {
        var vehicleId = $(this).val();

        if (vehicleId) {
            $.ajax({
                url: '/Maintenances/GetMaxKmIn',
                type: 'GET',
                data: { vehicleId: vehicleId },
                success: function (response) {
                    $('#KmOut').val(response.maxKmIn);
                },
                error: function () {
                    console.error('Error fetching max KmIn.');
                    $('#KmOut').val('0');
                }
            });
        } else {
            $('#KmOut').val('0');
        }
    });
});