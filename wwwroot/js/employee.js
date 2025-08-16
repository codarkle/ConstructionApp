window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        $.ajax({
            url: '/Employees/Create',
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
            url: "/Employees/GetEmployeeList",
            type: "POST",
            data: function (d) {
                d.workSiteDropDown = $('#workSiteDropDown').val();
                return d;
            }
        },
        columns: [
            { data: 'no', name: 'No', orderable: false,searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { data: 'userName', name: 'UserName' },
            { data: 'fullName', name: 'FullName' },
            { data: 'address', name: 'Address' },
            { data: 'roleId', name: 'RoleId' },
            { data: 'email', name: 'Email' },
            { data: 'workSite', name: 'WorkSite' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                            <button class="btn btn-sm btn-info view-btn" data-id="${row.id}">
                                <i class="bi bi-eye"></i>
                            </button>`;
                    if (window.myRole == "Admin" || window.MyRole == "Surveyor") {
                        buttons +=
                            `<button class="btn btn-sm btn-warning edit-btn" data-id="${row.id}">
                                <i class="bi bi-pencil"></i>
                            </button>`;
                    }
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

    //when i click the view button in main table, shows delete confirm modal
    $(document).on('click', '#mainTable tbody .view-btn', function () {
        const id = $(this).data('id');
        $.get(`/Employees/Profile/${id}`, function (html) {
            $('#viewContent').html(html);
            $('#viewModal').modal('show');
        });
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
                <td><input type="date" name="AttachFiles[${index}].Deadline" class="form-control" required/></td>
                <td class="align-middle">
                    <div class="form-check d-flex justify-content-center">
                        <input type="checkbox" name="AttachFiles[${index}].Renewed" value="false" class="form-check-input" style="transform: scale(1.25);" />
                    </div>
                    <input type="hidden" name="AttachFiles[${index}].Dumped" value="false"/>
                </td>
                <td class="d-flex">
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