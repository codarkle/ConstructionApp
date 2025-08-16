$.extend(true, $.fn.dataTable.defaults, {
    dom: '<"top table-toolbar d-flex justify-content-between align-items-center"' +
        '<"dt-left-tools d-flex align-items-center" B>' +
        '<"dt-right-tools d-flex align-items-center" f <"column-toggle btn-group ms-2">>' +
        '>rt<"bottom table-pagination"lp><"clear">',
    buttons: [{
        text: '+ Add',
        className: 'btn btn-primary add-btn'
    }],
    processing: true,
    paging: true,
    filter: true,
    serverSide: true, 
    scrollX: true,
    stripeClasses: ['odd', 'even'],
    language: {
        search: "",
        searchPlaceholder:"Search",
        lengthMenu: "_MENU_ rows visible",
        infoEmpty: "No entries available",
        processing: "Loading...",
        paginate: {
            first: "<<",
            last: ">>",
            next: ">",
            previous: "<"
        },
        zeroRecords: "No matching records found",
        emptyTable: "No data available in table"
    },
    drawCallback: function (settings) {
        const api = this.api();
        const rowCount = api.rows({ page: 'current' }).count();
        const desiredRowCount = 10;

        const table = $(api.table().node());
        const columnCount = table.find('thead tr th').length;

        if (rowCount < desiredRowCount) {
            const tbody = table.find('tbody');
            for (let i = rowCount; i < desiredRowCount; i++) {
                const emptyRow = $('<tr class="non-data-row">').append(`<td colspan="${columnCount}">&nbsp;</td>`);
                tbody.append(emptyRow);
             }
        }
    }
});


window.addEventListener('DOMContentLoaded', function () {
    $.ajax({
        url: '/WorkSites/GetMySites',
        type: 'GET',
        success: function (data) {
            if (data != null) {
                data.forEach(site => {
                    $('#workSiteDropDown').append(`<option value="${site.id}">${site.name}</option>`);
                });
                const savedId = localStorage.getItem('selectedWorkSiteId');
                if (savedId) {
                    $('#workSiteDropDown').val(savedId);
                    $('#mainTable').DataTable().ajax.reload();
                }
            }
        }
    });

    const curpath = window.location.pathname.toLowerCase();
    if (curpath === '/vehicles' || curpath === '/maintenances' || curpath === '/materials') {
        $('#workSiteDropDown').hide();
        $('#workSiteLabel').hide();
    } else {
        $('#workSiteDropDown').show();
        $('#workSiteLabel').show();
    }

     $('#workSiteDropDown').on("change", function () {
         const selectedId = $(this).val();
         const currentPath = window.location.pathname;
         $('#mainTable').DataTable().ajax.reload();
         localStorage.setItem('selectedWorkSiteId', selectedId);
         if (currentPath.startsWith("/WorkSites")) {
             if (selectedId == 0) {
                 window.location.href = "/WorkSites";
             } else {
                 window.location.href = `/WorkSites/SiteManage/${selectedId}`;
             }
         }
    }); 

     $(document).on('shown.bs.modal', '#createModal', function () {
         var modal = $(this);
         modal.find('form')[0].reset();
     });

     $(document).on('hide.bs.modal', '#createModal', function () {
         $('#createContent').html('');
     });

     $(document).on('shown.bs.modal', '#editModal', function () {
         var modal = $(this);
         modal.find('form')[0].reset();
     });

     $(document).on('hide.bs.modal', '#editModal', function () {
         $('#editContent').html('');
     });
       
    $(document).on('hide.bs.modal', '#viewModal', function () {
        $('#viewContent').html('');
    });

    //when i click the delete button in main table, shows delete confirm modal
    $(document).on('click', '#mainTable tbody .delete-btn', function () {
        const id = $(this).data('id');
        $('#deleteId').val(id);
        $('#deleteModal').modal('show');
    });

    //confirm to delete in modal
    $('#confirmDeleteBtn').on('click', function () {
        const id = $('#deleteId').val();
        const deleteUrl = `${window.location.pathname}/Delete/${id}`;

        $.ajax({
            url: deleteUrl,
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                $('#deleteModal').modal('hide');
                if (window.location.pathname == '/WorkSites') {
                    localStorage.removeItem('selectedWorkSiteId');
                }
                $('#mainTable').DataTable().ajax.reload();
                location.reload();
            },
            error: function (xhr) {
                alert('Could not delete the item: ' + xhr.responseText);
            }
        });
    });

    //when i click the edit button in main table, shows delete confirm modal
    $(document).on('click', '#mainTable tbody .edit-btn', function () {
        const id = $(this).data('id');
        const editUrl = `${window.location.pathname}/Edit/${id}`;
        $.get(editUrl, function (html) {
            $('#editContent').html(html);
            $('#editModal').modal('show');
            $.validator.unobtrusive.parse('#editForm');
            if (window.location.pathname == '/WorkSites') {
                initWorkerAutoComplete();
            }
        });
    });
     
    //when i create, validation must be done with ajax because of partial view.
     $(document).on('submit', '#createForm', function (e) {
         e.preventDefault();

         const form = $(this);
         const hasFile = form.find('input[type="file"]').length > 0;

         let ajaxOptions = {
             url: form.attr('action'),
             type: 'POST',
             success: function (response) {
                 const hasValidationErrors = $(response).find('.text-danger').filter(function () {
                     return $(this).text().trim().length > 0;
                 }).length > 0;
                 // Check if validation failed and partial view with errors was returned
                 if (hasValidationErrors) {
                     $('#createContent').html(response); // replace modal content
                     $.validator.unobtrusive.parse('#createForm');
                     $('#createForm').find('input, select, textarea').on('input change', function () {
                         const field = $(this);
                         if (field.hasClass('input-validation-error')) {
                             field.removeClass('input-validation-error');
                             field.next('.text-danger').text('');
                         }
                     });
                 } else {
                     //$('#createModal').modal('hide');
                     //$('#mainTable').DataTable().ajax.reload();
                     location.reload();
                 }
             },
             error: function (xhr) {
                 alert('Error: ' + (xhr.responseText || 'An unexpected error occurred.'));
             }
         };

         if (hasFile) {
             // Use FormData to include files
             ajaxOptions.data = new FormData(form[0]);
             ajaxOptions.processData = false;
             ajaxOptions.contentType = false;
         } else {
             // Use simple form serialization for text-only forms
             ajaxOptions.data = form.serialize();
         }

         $.ajax(ajaxOptions);
     });

    $(document).on('submit', '#editForm', function (e) {
         e.preventDefault();

         const form = $(this);
         const hasFile = form.find('input[type="file"]').length > 0;

         let ajaxOptions = {
             url: form.attr('action'),
             type: 'POST',
             success: function (response) {
                 const hasValidationErrors = $(response).find('.text-danger').filter(function () {
                     return $(this).text().trim().length > 0;
                 }).length > 0;
                 // Check if validation failed and partial view with errors was returned
                 if (hasValidationErrors) {
                     $('#editContent').html(response); // replace modal content
                     $.validator.unobtrusive.parse('#editForm');
                     $('#editForm').find('input, select, textarea').on('input change', function () {
                         const field = $(this);
                         if (field.hasClass('input-validation-error')) {
                             field.removeClass('input-validation-error');
                             field.next('.text-danger').text('');
                         }
                     });
                 } else {
                     //$('#editModal').modal('hide');
                     //$('#mainTable').DataTable().ajax.reload();
                     location.reload();
                 }
             },
             error: function (xhr) {
                 alert('Error: ' + (xhr.responseText || 'An unexpected error occurred.'));
             }
         };

         if (hasFile) {
             // Use FormData to include files
             ajaxOptions.data = new FormData(form[0]);  
             ajaxOptions.processData = false;
             ajaxOptions.contentType = false;
         } else {
             // Use simple form serialization for text-only forms
             ajaxOptions.data = form.serialize();
         }

         $.ajax(ajaxOptions);
     });
    $(document).on('click', '.view-file', function () {
        const row = $(this).closest('tr');
        const index = row.data('index');
        var path = $(`input[name="AttachFiles[${index}].StoredFilePath"]`)
        if (path.length > 0) {
            window.open(`/Home/ViewFile?path=${encodeURIComponent(path.val())}`);
        }
        else {
            const input = $(`input[name="AttachFiles[${index}].FormFile"]`)[0];
            if (!input) return;
            const file = input.files[0];
            if (!file) return;
            const fileUrl = URL.createObjectURL(file);
            window.open(fileUrl, '_blank');
        }
    });

    $(document).on('click', '#dropZone', function (e) {
        e.preventDefault();
        $('#pictureInput').click();
    });
    $(document).on('click', '#pictureInput', function (e) {
        e.stopPropagation();
    });
    $(document).on('dragover', '#dropZone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('#dropZone').addClass('bg-light');
    });
    $(document).on('dragleave', '#dropZone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('#dropZone').removeClass('bg-light');
    });
    $(document).on('drop', '#dropZone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('#dropZone').removeClass('bg-light');

        var files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            $('#pictureInput')[0].files = files;
            previewImage(files[0]);
        }
    });
    $(document).on('change', '#pictureInput', function () {
        if (this.files.length > 0) {
            previewImage(this.files[0]);
        }
    });
    function previewImage(file) {
        if (file && file.type.startsWith("image/")) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#picturePreview').attr('src', e.target.result);
            };
            reader.readAsDataURL(file);
        }
    } 
    $(document).on("click", ".remove-row", function () {
        const row = $(this).closest('tr');
        const index = row.data('index');
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
                $(`input[name="AttachFiles[${index}].Dumped"]`).val('true');
                $(`input[name="AttachFiles[${index}].FileName"]`).val("dumpfilename");
                $(`input[name="AttachFiles[${index}].Description"]`).val("dumpdescription");
                $(`input[name="AttachFiles[${index}].Deadline"]`).val("0001-01-01");
                row.hide();
            }
        });
    });
});

function addColumnToggleDropdown(table) {
    const $toggleContainer = $(table.table().container()).find('.column-toggle');

    // Avoid duplication
    if ($toggleContainer.find('.dropdown').length > 0) return;

    const dropdownId = 'colToggle-' + table.table().node().id;

    const $dropdown = $(`
        <div class="dropdown">
            <button class="btn btn-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown" data-bs-auto-close="outside">
                <i class="bi bi-grid-fill"></i>
            </button>
            <ul class="dropdown-menu" id="${dropdownId}"></ul>
        </div>
    `);

    table.columns().every(function (index) {
        if (this.dataSrc() === 'id' || this.dataSrc() === 'no') return;  
        const title = this.header().textContent;
        $dropdown.find('ul').append(`
            <li>
                <label  class="dropdown-item">
                    <input type="checkbox" class="toggle-vis" data-column="${index}" checked />
                    ${title}
                </label >
            </li>
        `);
    });

    $toggleContainer.append($dropdown);

    $dropdown.on('change', '.toggle-vis', function () {
        const column = table.column($(this).data('column'));
        column.visible(this.checked);
    });
}
