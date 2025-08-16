window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        $.ajax({
            url: '/Vehicles/Create',
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
            url: "/Vehicles/GetVehicleList",
            type: "POST",
        }, 
        columns: [
            { data: 'no', name: 'No', orderable: false, searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { data: 'description', name: 'Description' },
            { data: 'plate', name: 'Plate' },
            { data: 'dateInsurance', name: 'DateInsurance'  },
            { data: 'dateRevision',  name: 'DateRevision'  },
            { data: 'dateMaintenance', name: 'DateMaintenance',  },
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

    let fileIndex = 0;

    $(document).on('click', '#btnAddFile', function () {
        fileIndex = $('#filesTable tbody tr').length;
        const input = $('<input type="file" style="display:none;" />');
        input.attr('name', `AttachFiles[${fileIndex}].FormFile`);
        input.attr('id', `AttachFiles_${fileIndex}__FormFile`);

        input.on('change', function () {
            const file = this.files[0];
            if (!file) return;

            const index = fileIndex;

            const row = `
            <tr data-index="${index}">
                <td><input type="text" name="AttachFiles[${index}].FileName" value="${file.name}" class="form-control" required/></td>
                <td><input type="text" name="AttachFiles[${index}].Description" class="form-control" required/></td>
                <td><input type="date" name="AttachFiles[${index}].DateExpiration" class="form-control" required/></td>
                <td><input type="hidden" name="AttachFiles[${index}].Dumped" value="false"/>
                    <button type="button" class="btn btn-info btn-sm view-file"><i class="bi bi-eye"></i></button>
                    <button type="button" class="btn btn-danger btn-sm remove-row"><i class="bi bi-trash"></i></button>
                </td>
            </tr>`;

            $('#filesTable tbody').append(row);
            $('#dynamicFileInputs').append(input);

            fileIndex++;
        });

        input.click();
    });
}); 