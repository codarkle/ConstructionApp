window.addEventListener('DOMContentLoaded', function () {
    $('#alarmSidebar .list-group-item').first().addClass('active');
    selectedItem = $('#alarmSidebar .list-group-item.active').data('topic');
     
    var table = $('#alarmTable').DataTable({
        dom: 't',
        ajax: {
            url: "/Home/GetDeadlineList",
            type: "GET",
            data: function (d) {
                d.typeStr = selectedItem;
            } 
        },
        columns: [
            { data: 'no', name: 'no', orderable: false, searchable: false },
            { data: 'id', name: 'id', visible: false, searchable: false, orderable: false },
            { data: 'name', name: 'name' },
            { data: 'description', name: 'description' },
            { data: 'deadline', name: 'deadline' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                    <div class="d-flex justify-content-center">
                        <button class="btn btn-info view-file"
                            data-id="${row.id}" 
                            data-path="${row.storedFilePath}">
                            <i class="bi bi-eye"></i>
                        </button>
                    </div>`;

                    return buttons;
                }
            }
        ]
    });

    $('#alarmSidebar').on('click', '.list-group-item', function () {
        $('#alarmSidebar .list-group-item').removeClass('active');
        $(this).addClass('active');
        selectedItem = $(this).data('topic');
        table.ajax.reload();
    });

    window.showAlarm = function (type) {
        selectedItem = type;
        const item = document.querySelector(`[data-topic="${type}"]`);
        $('#alarmSidebar .list-group-item').removeClass('active');
        $(item).addClass('active');
        table.ajax.reload();
        $('#alarmModal').modal('show');
    }; 

    $(document).on('click', '.view-file', function () {
        const path = $(this).data('path');
        if (path.length > 0) {
            window.open(`/Home/ViewFile?path=${encodeURIComponent(path)}`);
        }
    });
});
