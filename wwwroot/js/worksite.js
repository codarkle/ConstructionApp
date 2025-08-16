window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        $.ajax({
            url: '/WorkSites/Create',
            type: 'GET',
            success: function (html) {
                $("#createContent").html(html);
                $("#createModal").modal("show");
                $.validator.unobtrusive.parse('#createForm');
                initWorkerAutoComplete();
            }
        });
    }); 

    var table = $('#mainTable').DataTable({
        ajax: {
            url: "/WorkSites/GetWorkSiteList",
            type: "POST",
        },
        columns: [
            { data: 'no', name: 'No', orderable: false, searchable: false },
            { data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { data: 'name', name: 'Name' },
            { data: 'address', name: 'Address' },
            { data: 'cAP', name: 'CAP' },
            { data: 'managerName', name: 'Manager.UserName' },
            { data: 'amount', name: 'Amount' },
            { data: 'safety', name: 'Safety' },
            { data: 'dateStart', name: 'DateStart', },
            { data: 'dateEnd', name: 'DateEnd', },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                            <button class="btn btn-sm btn-info view-site" data-id="${row.id}">
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
            { targets: 8, className: 'min-width-date' },
            { targets: 9, className: 'min-width-date' }
        ],
        initComplete: function () {
            addColumnToggleDropdown(this.api());
        }
    });

    $(document).on('click', '.view-site', function () {
        const id = $(this).data('id');
        localStorage.setItem('selectedWorkSiteId', id);
        window.location.href = `/WorkSites/SiteManage/${id}`;
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
                <td><input type="text" name="AttachFiles[${index}].Author" class="form-control" /></td>
                <td><input type="date" name="AttachFiles[${index}].Deadline" class="form-control" required/></td>
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
    $.validator.addMethod("dategreaterthan", function (value, element, params) {
        var otherVal = $("[name='" + params.other + "']").val();
        if (!value || !otherVal) return true;

        var start = new Date(otherVal);
        var end = new Date(value);
        return end > start;
    });

    $.validator.unobtrusive.adapters.add("dategreaterthan", ["other"], function (options) {
        options.rules["dategreaterthan"] = { other: options.params.other };
        options.messages["dategreaterthan"] = options.message;
    });
    
});

function initWorkerAutoComplete() {
    let selectedWorker = null;
    let addedWorkerIds = new Set();
    $("#workerSearch").autocomplete({
        minLength: 0,
        source: function (request, response) {
            $.ajax({
                url: '/Employees/SearchFreeWorkers',
                data: { term: request.term },
                success: function (data) {
                    const filtered = data.filter(worker => !addedWorkerIds.has(worker.id));
                    if (request.term.length == 0 && filtered.length == 0) {
                        $('#workerSearch').attr('placeholder', 'non ci sono operai disponibili');
                    }
                    else {
                        $('#workerSearch').attr('placeholder', 'Start typing ...');
                    }
                    response(filtered);
                }
            });
        },
        select: function (event, ui) {
            selectedWorker = ui.item;
            $("#addWorkerBtn").prop("disabled", false);
        }
    }).focus(function () {
        $(this).autocomplete("search", "");
    }); 
    $("#addWorkerBtn").click(function () {
        if (!selectedWorker || addedWorkerIds.has(selectedWorker.id)) return;

        addedWorkerIds.add(selectedWorker.id);

        $("#selectedWorkerInputs").append(
            `<input type="hidden" name="SelectedWorkerIds" value="${selectedWorker.id}" id="hidden-worker-${selectedWorker.id}" />`
        );

        $("#workerList").append(
            `<li class="list-group-item d-flex justify-content-between align-items-center" id="worker-${selectedWorker.id}">
                ${selectedWorker.label}
            <button type="button" class="btn btn-sm btn-danger remove-worker" data-id="${selectedWorker.id}"><i class="bi bi-trash"></i></button>
            </li>`
        );

        $("#workerSearch").val('');
        selectedWorker = null;
        $("#addWorkerBtn").prop("disabled", true);
    });
    $(document).on("click", ".remove-worker", function () {
        const id = $(this).data('id');
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to undo this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                addedWorkerIds.delete(id);
                $(`#worker-${id}`).remove();
                $(`#hidden-worker-${id}`).remove();
            }
        });
    });
}